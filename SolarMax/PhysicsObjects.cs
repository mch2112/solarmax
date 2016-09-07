using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Physics
    {
        private const int DEFAULT_STARTING_NUM_STARS = 9200;
        private const int DEFAULT_STARTING_NUM_CONSTELLATIONS = 100;
        private const int DEFAULT_STARTING_NUM_ALL_BODIES = 9400;

        public LinkedList<CelestialBody> AllBodies { get; private set; }
        public Orbiter[] AllOrbiters { get; private set; }
        public List<Constellation> Constellations { get; private set; }
        public List<Star> Stars { get; private set; }
        public Dictionary<string, CelestialBody> BodyDictionary { get; private set; }
        public Dictionary<int, Star> StarDictionary { get; private set; }
        public List<Tuple<string, string, CelestialBody>> SearchDatabase { get; private set; }

        public void PreloadSearch()
        {
            if (SearchDatabase == null)
            {
                SearchDatabase = new List<Tuple<string, string, CelestialBody>>(DEFAULT_STARTING_NUM_ALL_BODIES);
                foreach (var cb in AllBodies)
                    foreach (var s in cb.SearchNames)
                        SearchDatabase.Add(new Tuple<string, string, CelestialBody>(s.ToLower(), s, cb));

                SearchDatabase.Sort((a, b) => (a.Item1.CompareTo(b.Item1)));
            }
        }

        private void createOrbiters()
        {
            string[,] constant_data = IO.ReadFile(CONSTANT_DATA_FILENAME, DirectoryLocation.Data);

            List<Orbiter> oo = new List<Orbiter>(Ephemeris.STARTING_NUM_ORBITERS);

            for (int i = 0; i <= constant_data.GetUpperBound(0); i++)
            {
                oo.Add(new Orbiter(Name: constant_data[i, 0],
                                   MGInM3PerS2: constant_data[i, 1].ParseDouble(0) * 1E+9,
                                   MassInGrams: constant_data[i, 2].ParseDouble(0) * 1E+3,
                                   RadiusInMeters: constant_data[i, 3].ParseDouble(0) * 1000.0,
                                   AngularVelocity: MathEx.TWO_PI / constant_data[i, 4].ParseDouble(86400.0),
                                   NPRightAscension: constant_data[i, 5].ParseDouble(0).ToRadiansFromDegrees(),
                                   NPDeclination: constant_data[i, 6].ParseDouble(0).ToRadiansFromDegrees(),
                                   StartingAngle: constant_data[i, 7].ParseDouble(0).ToRadiansFromDegrees(),
                                   Oblateness: constant_data[i, 8].ParseDouble(0)
                                   ));
            }
            this.AllOrbiters = oo.ToArray();
            foreach (var o in this.AllOrbiters)
            {
                this.registerCelestialBody(o);
            }
        }
        private void loadEphemeres(bool AllowLoadSnapshot)
        {
            this.availableEphemeres = Ephemeris.LoadCatalog();

            foreach (var fi in IO.EnumerateFiles(DirectoryLocation.Data, "ephemeris*.txt"))
            {
                if (fi.Name != Ephemeris.CATALOG_FILENAME)
                {
                    Ephemeris e = new Ephemeris(IO.ReadFile(fi.FullName, DirectoryLocation.Data));
                    if (AllowLoadSnapshot || e.VersionCount <= 0)
                        if (e.VersionCount < MAX_SNAPSHOT_GENERATIONS)
                            this.availableEphemeres.Add(e);
                }
            }
            if (!this.availableEphemeres.HasItems)
                throw new Exception("No Ephemeris Found");

            if (TEST)
            {
                testEphemeris = availableEphemeres[TEST_DATE];
                availableEphemeres.Remove(testEphemeris);
            }
            availableEphemeres.Init();
        }
        private void invokeEphemeris(bool SetTimeMode, bool Wait, bool EstablishGravitationalInfluences)
        {
            if (Wait)
                wait();

            if (this.ephemeris.Link(this.AllOrbiters))
                EstablishGravitationalInfluences = true;

            this.ephemeris.Push();
            this.Date = this.StartDate = this.ephemeris.Date;

            foreach (var cb in this.AllOrbiters)
                cb.RotateTo((this.ephemeris.Date - EPOCH_START).TotalSeconds);

            this.totalElapsedTime = 0;

            if (EstablishGravitationalInfluences)
                establishGravitationalInfluences();

            this.integrator.Init(this);

            if (Wait)
                unwait();

            if (SetTimeMode)
                if (TEST) // test to a certain date
                {
                    this.TimeMode = TimeMode.TargetToPaused;
                }
                else
                {
                    this.TimeMode = TimeMode.TargetToNormal;
                }
        }

        private void unwait()
        {
            this.waitState = WaitState.None;
        }

        private void wait()
        {
            this.waitState = WaitState.Requested;
            while (this.waitState != WaitState.Confirmed && !this.Paused && !this.cancel)
                System.Threading.Thread.Sleep(0);
        }
        private void setupCaptioning()
        {
            // Don't AlwaysCaption for objects close to larger ones (moons, e.g.)
            for (int i = 1; i < AllOrbiters.Length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (AllOrbiters[i].Position.DistanceToSquared(AllOrbiters[j].Position) < 1E+20)
                    {
                        if (AllOrbiters[i].Mass > AllOrbiters[j].Mass)
                            AllOrbiters[j].CaptionZoomThreshold = double.MaxValue;
                        else
                            AllOrbiters[i].CaptionZoomThreshold = double.MaxValue;
                    }
                }
            }
        }
        private void loadStars()
        {
            string[,] ephermeris = IO.ReadFile("stars.txt", DirectoryLocation.Data);
            this.Stars = new List<Star>(DEFAULT_STARTING_NUM_STARS);
            for (int i = 0; i <= ephermeris.GetUpperBound(0); i++)
            {
                var s = new Star(Name: ephermeris[i, 2],
                                 HRNum: ephermeris[i, 1].ParseInt(0),
                                 HDNum: ephermeris[i, 0].ParseInt(0),
                                 Magnitude: ephermeris[i, 6].ParseFloat(7f),
                                 RightAscensionInHours: ephermeris[i, 3].ParseDouble(0),
                                 DeclinationInDegrees: ephermeris[i, 4].ParseDouble(0),
                                 DistanceInParsecs: ephermeris[i, 5].ParseDouble(0),
                                 ColorIndex: ephermeris[i, 7].ParseFloat(0),
                                 BayerDesignationAbbreviation: ephermeris[i, 8],
                                 BayerIndex: ephermeris[i, 9].ParseInt(0),
                                 ConstellationAbbreviation: ephermeris[i, 10],
                                 FlamsteedNumber: ephermeris[i, 11].ParseInt(0));

                this.Stars.Add(s);
            }

            this.Stars.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
            
            foreach (var s in this.Stars)
                this.registerCelestialBody(s);

            this.Stars.Sort((a, b) => a.Magnitude.CompareTo(b.Magnitude));
        }
        
        private void registerCelestialBody(Star Star)
        {
            if (!this.StarDictionary.ContainsKey(Star.HRNum))
                this.StarDictionary.Add(Star.HRNum, Star);

            registerCelestialBody(Star as CelestialBody);
        }
        private void registerCelestialBody(Constellation Constellation)
        {
            Constellation.FixConstellationLocation();

            registerCelestialBody(Constellation as CelestialBody);
        }
        private void registerCelestialBody(CelestialBody CB)
        {
            AllBodies.AddLast(CB);
            if (!BodyDictionary.ContainsKey(CB.Name))
                BodyDictionary.Add(CB.Name, CB);
        }
        private void setupConstellations()
        {
            string[,] Data = IO.ReadFile(Constellation.CONSTELLATION_DEF_FILENAME, DirectoryLocation.Data);

            this.Constellations = new List<Constellation>(DEFAULT_STARTING_NUM_CONSTELLATIONS);

            for (int i = 0; i <= Data.GetUpperBound(0); i++)
            {
                int index = Data[i, 0].ParseInt(-1);
                string name = Data[i, 1];

                Constellation c = this.Constellations.FirstOrDefault(cc => cc.Name == name);

                bool isNew = c == null;

                if (isNew)
                {
                    c = new Constellation(name, this);
                    this.Constellations.Add(c);
                }
                for (int j = 1; j <= Data.GetUpperBound(1); j++)
                {
                    if (Data[i, j] == null)
                        break;
                    c.WithLines(index, Data[i, j].Trim().Split('-').Select(n => n.ParseInt(0)).ToList());
                }
            }
            this.Constellations.Sort((a, b) => a.Name.CompareTo(b.Name));
            
            foreach (var c in this.Constellations)
                this.registerCelestialBody(c);
        }
    }
}
