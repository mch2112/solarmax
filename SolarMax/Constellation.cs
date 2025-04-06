using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class Constellation : CelestialBody
    {
        public const string CONSTELLATION_DEF_FILENAME = "constellations.txt";
        public const string CONSTELLATION_BOUNDARY_FILENAME = "constellation_boundaries.txt";
        public const double STANDARD_CONSTELLATION_DISTANCE_PARSECS = 50;

        private const int MIN_INTENSITY_FOR_RANDOM_COLOR = 15;
        private const int MAX_INTENSITY_FOR_RANDOM_COLOR = 60;

        public string GenitiveName { get; private set; }
        public string Abbreviation { get; private set; }
        public static bool UseAltShapes { get; set; }
        public static Shape ConstellationBoundaries { get; private set; }
        public static Dictionary<string, string> GenitiveNames { get; private set; }
        public static Dictionary<string, string> AllNames { get; private set; }

        private Shape normalShape;
        private Shape altShape;
        public Shape Shape => UseAltShapes ? altShape : normalShape;

        private static string[,] names;

        public List<Star> Stars { get; private set; }
        private List<Tuple<Star, Star>> StarPairs { get; set; }
        private readonly Physics physics;

        public Constellation(string Name, Physics Physics)
        {
            this.Name = Name;
            this.FullName = "Constellation " + Name;
            this.DisplayName = this.Name;
            this.physics = Physics;
            this.Stars = [];
            this.StarPairs = [];
            this.Color = Colors.GetColor(this.Name, Colors.GetColor("constellation_default", MIN_INTENSITY_FOR_RANDOM_COLOR, MAX_INTENSITY_FOR_RANDOM_COLOR));
            this.normalShape = new Shape();
            this.altShape = new Shape();
            this.Velocity = new Vector();
            this.BodyType = CelestialBodyType.Constellation;
            this.HasDynamicShape = false;
            this.HasShape = false;
            this.RadiusEnhancement = 0;
            for (int i = 0; i <= names.GetUpperBound(0); i++)
            {
                if (names[i, 0].Equals(this.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.GenitiveName = names[i, 1];
                    this.Abbreviation = names[i, 2];
                }
            }

            this.SortKey = this.Name;
#if DEBUG
            if (string.IsNullOrWhiteSpace(this.GenitiveName) || string.IsNullOrWhiteSpace(this.Abbreviation))
                throw new Exception("Constellation without genitive name or abbreviation: " + this.Name);
#endif
        }
        static Constellation()
        {
            UseAltShapes = false;
            setupNames();
            ConstellationBoundaries = getConstellationBoundaries();
        }

        private static void setupNames()
        {
            names = new string[,]
            {   { "Andromeda"          , "Andromedae"         , "And" },
				{ "Antlia"             , "Antliae"            , "Ant" },
				{ "Apus"               , "Apodis"             , "Aps" },
				{ "Aquarius"           , "Aquarii"            , "Aqr" },
				{ "Aquila"             , "Aquilae"            , "Aql" },
				{ "Ara"                , "Arae"               , "Ara" },
				{ "Aries"              , "Arietis"            , "Ari" },
				{ "Auriga"             , "Aurigae"            , "Aur" },
				{ "Bootes"             , "Bootis"             , "Boo" },
				{ "Caelum"             , "Caeli"              , "Cae" },
				{ "Camelopardalis"     , "Camelopardalis"     , "Cam" },
				{ "Cancer"             , "Cancri"             , "Cnc" },
				{ "Canes Venatici"     , "Canum Venaticorum"  , "CVn" },
				{ "Canis Major"        , "Canis Majoris"      , "CMa" },
				{ "Canis Minor"        , "Canis Minoris"      , "CMi" },
				{ "Capricornus"        , "Capricorni"         , "Cap" },
				{ "Carina"             , "Carinae"            , "Car" },
				{ "Cassiopeia"         , "Cassiopeiae"        , "Cas" },
				{ "Centaurus"          , "Centauri"           , "Cen" },
				{ "Cepheus"            , "Cephei"             , "Cep" },
				{ "Cetus"              , "Ceti"               , "Cet" },
				{ "Chamaeleon"         , "Chamaeleontis"      , "Cha" },
				{ "Circinus"           , "Circini"            , "Cir" },
				{ "Columba"            , "Columbae"           , "Col" },
				{ "Coma Berenices"     , "Comae Berenices"    , "Com" },
				{ "Corona Australis"   , "Coronae Australis"  , "CrA" },
				{ "Corona Borealis"    , "Coronae Borealis"   , "CrB" },
				{ "Corvus"             , "Corvi"              , "Crv" },
				{ "Crater"             , "Crateris"           , "Crt" },
				{ "Crux"               , "Crucis"             , "Cru" },
				{ "Cygnus"             , "Cygni"              , "Cyg" },
				{ "Delphinus"          , "Delphini"           , "Del" },
				{ "Dorado"             , "Doradus"            , "Dor" },
				{ "Draco"              , "Draconis"           , "Dra" },
				{ "Equuleus"           , "Equulei"            , "Equ" },
				{ "Eridanus"           , "Eridani"            , "Eri" },
				{ "Fornax"             , "Fornacis"           , "For" },
				{ "Gemini"             , "Geminorum"          , "Gem" },
				{ "Grus"               , "Gruis"              , "Gru" },
				{ "Hercules"           , "Herculis"           , "Her" },
				{ "Horologium"         , "Horologii"          , "Hor" },
				{ "Hydra"              , "Hydrae"             , "Hya" },
				{ "Hydrus"             , "Hydri"              , "Hyi" },
				{ "Indus"              , "Indi"               , "Ind" },
				{ "Lacerta"            , "Lacertae"           , "Lac" },
				{ "Leo Minor"          , "Leonis Minoris"     , "LMi" },
				{ "Leo"                , "Leonis"             , "Leo" },
				{ "Lepus"              , "Leporis"            , "Lep" },
				{ "Libra"              , "Librae"             , "Lib" },
				{ "Lupus"              , "Lupi"               , "Lup" },
				{ "Lynx"               , "Lyncis"             , "Lyn" },
				{ "Lyra"               , "Lyrae"              , "Lyr" },
				{ "Mensa"              , "Mensae"             , "Men" },
				{ "Microscopium"       , "Microscopii"        , "Mic" },
				{ "Monoceros"          , "Monocerotis"        , "Mon" },
				{ "Musca"              , "Muscae"             , "Mus" },
				{ "Norma"              , "Normae"             , "Nor" },
				{ "Octans"             , "Octantis"           , "Oct" },
				{ "Ophiuchus"          , "Ophiuchi"           , "Oph" },
				{ "Orion"              , "Orionis"            , "Ori" },
				{ "Pavo"               , "Pavonis"            , "Pav" },
				{ "Pegasus"            , "Pegasi"             , "Peg" },
				{ "Perseus"            , "Persei"             , "Per" },
				{ "Phoenix"            , "Phoenicis"          , "Phe" },
				{ "Pictor"             , "Pictoris"           , "Pic" },
				{ "Pisces"             , "Piscium"            , "Psc" },
				{ "Piscis Austrinus"   , "Piscis Austrini"    , "PsA" },
				{ "Puppis"             , "Puppis"             , "Pup" },
				{ "Pyxis"              , "Pyxidis"            , "Pyx" },
				{ "Reticulum"          , "Reticuli"           , "Ret" },
				{ "Sagitta"            , "Sagittae"           , "Sge" },
				{ "Sagittarius"        , "Sagittarii"         , "Sgr" },
				{ "Scorpius"           , "Scorpii"            , "Sco" },
				{ "Sculptor"           , "Sculptoris"         , "Scl" },
				{ "Scutum"             , "Scuti"              , "Sct" },
				{ "Serpens"            , "Serpentis"          , "Ser" },
				{ "Serpens Caput"      , "Serpentis"          , "Ser" },
				{ "Serpens Cauda"      , "Serpentis"          , "Ser" },
				{ "Sextans"            , "Sextantis"          , "Sex" },
				{ "Taurus"             , "Tauri"              , "Tau" },
				{ "Telescopium"        , "Telescopii"         , "Tel" },
				{ "Triangulum Australe", "Trianguli Australis", "TrA" },
				{ "Triangulum"         , "Trianguli"          , "Tri" },
				{ "Tucana"             , "Tucanae"            , "Tuc" },
				{ "Ursa Major"         , "Ursae Majoris"      , "UMa" },
				{ "Ursa Minor"         , "Ursae Minoris"      , "UMi" },
				{ "Vela"               , "Velorum"            , "Vel" },
				{ "Virgo"              , "Virginis"           , "Vir" },
				{ "Volans"             , "Volantis"           , "Vol" },
				{ "Vulpecula"          , "Vulpeculae"         , "Vul" }
            };
            GenitiveNames = [];
            AllNames = [];
            for (int i = 0; i <= names.GetUpperBound(0); i++)
            {
                if (!GenitiveNames.ContainsKey(names[i, 2]))
                    GenitiveNames.Add(names[i, 2], names[i, 1]);
                if (!AllNames.ContainsKey(names[i, 2])) 
                    AllNames.Add(names[i, 2], names[i, 0]);
            }
        }
        public override IEnumerable<string> SearchNames
        {
            get
            {
                yield return this.Name;
                yield return this.Abbreviation;
            }
        }
        protected override QColor Color
        {
            set
            {
                this.Pen = new QPen(value);
                this.CaptionPen = new QPen(value.Brighten(20));
                this.FrontPen = this.Pen;
                this.BackPen = this.Pen;
            }
        }
        public string Serialize()
        {
            List<List<Star>> lls = [];

            string s = this.Name;
            
            foreach (var sp in this.StarPairs)
                lls.Add([sp.Item1, sp.Item2]);

            bool done;

            do
            {
                done = true;
                for (int i = 0; i < lls.Count - 1; i++)
                {
                    if (lls[i].Last().HRNum == lls[i + 1].First().HRNum)
                    {
                        lls[i + 1].RemoveAt(0);
                        lls[i].AddRange(lls[i + 1]);
                        lls.RemoveAt(i + 1);
                        done = false;
                    }
                }
            }
            while (!done);

            return this.Name + "," + string.Join(",", lls.Select(hh => string.Join("-", hh.Select(h => h.HRNum))));
        }
        public Constellation WithLine(int Index, params int[] HRNums)
        {
            return this.WithLines(Index, [.. HRNums]);
        }
        public Constellation WithLines(int Index, List<int> HRNums)
        {
            for (int i = 0; i < HRNums.Count - 1; i++)
            {
                Star s1 = physics.StarDictionary[HRNums[i]];
                Star s2 = physics.StarDictionary[HRNums[i + 1]];

                if (!Stars.Contains(s1))
                    Stars.Add(s1);
                if (!Stars.Contains(s2))
                    Stars.Add(s2);

                switch (Index)
                {
                    case 0:
                        normalShape.AddLine(new Line(s1.Position, s2.Position));
                        break;
                    case 1:
                        altShape.AddLine(new Line(s1.Position, s2.Position));
                        break;
                }
                StarPairs.Add(new Tuple<Star, Star>(s1, s2));
            }
            return this;
        }
        public void FixConstellationLocation()
        {
            if (Stars.Count < 0)
            {
                this.Position = Vector.Zero;
                return;
            }

            // POSITION IS BETWEEN BRIGHTEST TWO STARS

            Star brightest = null;
            Star secondBrightest = null;
            foreach (var s in Stars)
            {
                if (brightest == null || s.Magnitude < brightest.Magnitude)
                {
                    secondBrightest = brightest;
                    brightest = s;
                }
                else if (secondBrightest == null || s.Magnitude < secondBrightest.Magnitude)
                {
                    secondBrightest = s;
                }
            }

            if (brightest == null)
                this.Position = Vector.UnitZ * Util.METERS_PER_PARSEC;
            else if (secondBrightest == null)
                this.Position = brightest.Position.Unit * (STANDARD_CONSTELLATION_DISTANCE_PARSECS * Util.METERS_PER_PARSEC);
            else
                this.Position = (brightest.Position.Unit + secondBrightest.Position.Unit) * (0.5 * STANDARD_CONSTELLATION_DISTANCE_PARSECS * Util.METERS_PER_PARSEC);

            this.Snapshot();

            this.normalShape = this.normalShape.Normalized;
            this.altShape = this.altShape.Normalized;


            // WEIGHTED AVERAGE METHOD

            /*
            Vector sum = Vector.Zero;

            foreach (var s in Stars)
                sum += s.Location.Unit;

            this.Location = sum.Unit * 1E+20;
            */ 
            
            // AVERAGE OF ANGLES METHOD

            /*
            
            double minRA = 24;
            double maxRA = -24;
            double minDecl = 90;
            double maxDecl = -90;

            foreach (var s in Stars)
            {
                minRA = Math.Min(minRA, s.RightAscensionInHours);
                maxRA = Math.Max(maxRA, s.RightAscensionInHours);
                minDecl = Math.Min(minDecl, s.DeclinationInDegrees);
                maxDecl = Math.Max(maxDecl, s.DeclinationInDegrees);
            }

            if ((maxRA - minRA) > 12)
            {
                if (maxRA > 12)
                    maxRA -= 24;
                else if (maxRA < -12)
                    maxRA += 24;
                if (minRA > 12)
                    minRA -= 24;
                else if (minRA < -12)
                    minRA += 24;

            }

            this.Location = Util.LocationFromEquatorialCoords(Util.Average(minRA, maxRA), Util.Average(minDecl, maxDecl), 10);
            */
        }
        
        private static Shape getConstellationBoundaries()
        {
            string[,] s = IO.ReadFile(CONSTELLATION_BOUNDARY_FILENAME, DirectoryLocation.Data);

            var cb = new Shape();
            List<LineBase> lines = [];
            for (int i = 0; i <= s.GetUpperBound(0); i++)
            {
                if (s[i, 0].StartsWith('<'))
                    continue;

                lines.Add(new Line(Util.LocationFromEquatorialCoords(s[i, 0].ParseDouble(0),
                                                                     s[i, 1].ParseDouble(0),
                                                                     STANDARD_CONSTELLATION_DISTANCE_PARSECS),
                                   Util.LocationFromEquatorialCoords(s[i, 2].ParseDouble(0),
                                                                     s[i, 3].ParseDouble(0),
                                                                     STANDARD_CONSTELLATION_DISTANCE_PARSECS)));
            }
            cb.AddLines(lines);
            return cb.Normalized;
        }
        public static string SerializeConstellationList(List<Constellation> Input)
            => string.Concat(Input.Select(c => c.Serialize() + Environment.NewLine));
    }
}
