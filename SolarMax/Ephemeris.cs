using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class Ephemeris
    {
        public const string CATALOG_FILENAME = "ephemeris_catalog.txt";
        
        public const string CATALOG_START_KEY = "$START_EPHEMERIS_CATALOG";
        public const string CATALOG_END_KEY = "$END_EPHEMERIS_CATALOG";
        public const string EPHEMERIS_START_KEY = "$START_EPHEMERIS";
        public const string EPHEMERIS_END_KEY = "$END_EPHEMERIS";

        public const string DATE_FORMAT_FOR_FILE_NAME = "yyyy-MM-dd_HH-mm-ss_fffffff";
        public const string DATE_FORMAT_1 = "yyyy/MM/dd HH:mm:ss.fffffff";
        public const string DATE_FORMAT_2 = "yyyy/MM/dd HH:mm:ss";

        public const int STARTING_NUM_ORBITERS = 52;
        public const int STARTING_CATALOG_LENGTH = 350;

        private Dictionary<string, EphemerisItem> itemDictionary = null;

        public DateTime Date { get; private set; }
        public DateTime BasedOnDate { get; private set; }
        
        public List<EphemerisItem> Items { get; private set; }
        public Dictionary<string, EphemerisItem> ItemDictionary
        {
            get
            {
                if (itemDictionary == null)
                {
                    itemDictionary = new Dictionary<string, EphemerisItem>();
                    foreach (var i in this.Items)
                        itemDictionary.Add(i.Name, i);
                }
                return itemDictionary;
            }
        }
        public int VersionCount { get; private set; }
        public bool IsLinked { get; private set; }
        public bool IsOriginal { get { return this.VersionCount <= 0; } }
        public bool IsValid { get; private set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }

        public Ephemeris(string[,] Data)
        {
            if (Data.GetUpperBound(0) < 3)
            {
                this.IsValid = false;
                return;
            }

            this.IsValid = true;
            this.Items = new List<EphemerisItem>(STARTING_NUM_ORBITERS);
            this.VersionCount = Data[0, 0].ParseInt(Int32.MaxValue);
           
            int i = 1;
            read(Data, ref i);
        }
        public Ephemeris(string[,] Data, ref int Index)
        {
            this.IsValid = true;
            this.Items = new List<EphemerisItem>(STARTING_NUM_ORBITERS);
            this.VersionCount = 0;

            read(Data, ref Index);
        }
        private void read(string[,] Data, ref int Index)
        {
            this.Date = parseDate(Data[Index, 0]);
            if (this.VersionCount > 0)
            {
                this.BasedOnDate = parseDate(Data[Index, 1]);
            }
            else
            {
                this.BasedOnDate = this.Date;
            }
            this.IsLinked = false;
            
            ++Index;
            for (; Index <= Data.GetUpperBound(0); Index++)
            {
                try
                {
                    string name = Data[Index, 0].Trim();
                    
                    if (name == Ephemeris.EPHEMERIS_END_KEY)
                        break;

                    var pos = new Vector(Data[Index, 1].ParseDouble(0),
                                         Data[Index, 2].ParseDouble(0),
                                         Data[Index, 3].ParseDouble(0));
                    var vel = new Vector(Data[Index, 4].ParseDouble(0),
                                         Data[Index, 5].ParseDouble(0),
                                         Data[Index, 6].ParseDouble(0));

                    Items.Add(new EphemerisItem(name, pos, vel));
                }
                catch
                {
                }
            }
        }
        public Ephemeris(IEnumerable<Orbiter> Orbiters, DateTime Date, DateTime BasedOnDate, int VersionCount)
        {
            this.Date = Date;
            this.BasedOnDate = BasedOnDate;
            this.Items = new List<EphemerisItem>(Ephemeris.STARTING_NUM_ORBITERS);
            this.VersionCount = VersionCount;
            
            foreach (var o in Orbiters)
                Items.Add(new EphemerisItem(o));
            
            this.IsLinked = true;
            this.IsValid = true;
        }
        public static EphemerisCatalog LoadCatalog()
        {
            string[,] s = IO.ReadFile(CATALOG_FILENAME, DirectoryLocation.Data);

            EphemerisCatalog ec = new EphemerisCatalog();

            int i = 0;
            bool inEphemeris = false;
            bool inCatalog = false;
            
            while (i < s.GetUpperBound(0))
            {
                if (inCatalog)
                {
                    if (inEphemeris)
                    {
                        if (s[i, 0] == Ephemeris.EPHEMERIS_END_KEY)
                        {
                            inEphemeris = false;
                        }
                        else
                        {
                            ec.Add(new Ephemeris(s, ref i));
                            inEphemeris = false;
                        }
                    }
                    else if (s[i, 0] == Ephemeris.EPHEMERIS_START_KEY)
                    {
                        inEphemeris = true;
                    }
                    else if (s[i, 0] == Ephemeris.CATALOG_END_KEY)
                    {
                        break;
                    }
                }
                else if (s[i, 0] == Ephemeris.CATALOG_START_KEY)
                {
                    inCatalog = true;
                }
                i++;
            }
            return ec;
        }
        public void Save()
        {
            String s = (this.VersionCount + 1).ToString() + Environment.NewLine;
            
            s += Serialize();

            IO.WriteFile("ephemeris_snapshot_" + this.Date.ToString(Ephemeris.DATE_FORMAT_FOR_FILE_NAME) + ".txt", DirectoryLocation.Data, s);
        }

        public string Serialize()
        {
            string s = this.Date.ToString(DATE_FORMAT_1);

            if (this.VersionCount > 0)
                s += "," + this.BasedOnDate.ToString(DATE_FORMAT_1);

            s += Environment.NewLine;

            var sun = ItemDictionary["Sun"];
            return s + String.Join(Environment.NewLine, Items.Select(b => b.Name + "," + (b.Position - sun.Position).Serialize() + "," + (b.Velocity - sun.Velocity).Serialize())) + Environment.NewLine;
        }
        public bool Link(Orbiter[] Orbiters)
        {
            bool someDifferent = false;
            if (!this.IsLinked)
            {
                var id = this.ItemDictionary;
                EphemerisItem ei;
                foreach (var o in Orbiters)
                {
                    if (id.TryGetValue(o.Name, out ei))
                    {
                        ei.Link(o);
                        if (o.IsDead)
                        {
                            o.Unkill();
                            someDifferent = true;
                        }
                    }
                    else
                    {
                        if (!o.IsDead)
                        {
                            o.Kill();
                            someDifferent = true;
                        }
                    }
                }
                this.IsLinked = true;
            }
            return someDifferent;
        }
        public void Push()
        {
            if (!this.IsLinked)
                throw new Exception("Ephemeris Not Linked");

            foreach (var i in Items)
                if (i.IsLinked)
                    i.Orbiter.SetEphemeris(i.Position, i.Velocity);
        }

        public override string ToString()
        { 
            return string.Format("{0} {1} - {2} Items{3} {4} {5}", this.Date.ToShortDateString(), this.Date.ToShortTimeString(), this.Items.Count, this.IsLinked ? " Linked" : string.Empty, this.MinDate.ToShortDateString(), this.MaxDate.ToShortDateString());
        }

        private static DateTime parseDate(string Date)
        {
            return DateTime.ParseExact(Date.Trim(), new string[] { DATE_FORMAT_1, DATE_FORMAT_2 }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
        }
    }
    internal sealed class EphemerisCatalog
    {
        private List<Ephemeris> list = new List<Ephemeris>(Ephemeris.STARTING_CATALOG_LENGTH);
        private bool isInitialized = false;

        public Ephemeris this[DateTime Date]
        {
            get { return list.FirstOrDefault(e => e.Date == Date); }
        }
        public void Add(Ephemeris e)
        {
            list.Add(e);
            e.MinDate = DateTime.MinValue;
            e.MaxDate = DateTime.MaxValue;
            isInitialized = false;
        }
        public void Remove(Ephemeris e)
        {
            list.Remove(e);
            isInitialized = false;
        }
        public bool HasItems { get { return list.Count > 0; } }
        public bool HasItemWithinSeconds(DateTime Target, double Seconds)
        {
            return GetClosest(Target).Date.SecondsApart(Target) < Seconds;
        }
        public void Init()
        {
            list.Sort((a, b) => (a.Date.CompareTo(b.Date)));

            for (int i = 1; i < list.Count; i++)
            {
                System.Diagnostics.Debug.Assert(list[i - 1].Date != list[i].Date, "Duplicate Ephemeris: " + list[i].Date.ToString());

                list[i - 1].MaxDate = list[i - 1].Date.AddMilliseconds((list[i].Date - list[i - 1].Date).TotalMilliseconds / 2);
                list[i].MinDate = list[i - 1].MaxDate;
            }

            isInitialized = true;

            //this.Save();
        }
        public Ephemeris GetClosest(DateTime DateTime)
        {
            System.Diagnostics.Debug.Assert(isInitialized, "Ephemeris list not initialized");

            int low = 0;
            int high = list.Count - 1;
            int midIndex;

            while (low <= high)
            {
                var midVal = list[midIndex = (low + high) / 2];

                if (midVal.MaxDate < DateTime)
                    low = midIndex + 1;
                else if (midVal.MinDate > DateTime)
                    high = midIndex - 1;
                else
                    return midVal; // key found
            }
            throw new Exception("Could not find closest ephemeris.");
        }
        public void Save()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Ephemeris.CATALOG_START_KEY);

            foreach (var e in list.Where(ee => ee.VersionCount == 0))
            {
                sb.AppendLine(Ephemeris.EPHEMERIS_START_KEY);
                sb.Append(e.Serialize());
                sb.AppendLine(Ephemeris.EPHEMERIS_END_KEY);
            }

            sb.AppendLine(Ephemeris.CATALOG_END_KEY);

            IO.WriteFile(Ephemeris.CATALOG_FILENAME, DirectoryLocation.Data, sb.ToString());
        }
        public override string ToString()
        {
            return string.Join(Environment.NewLine, list.Select(l => l.Date.ToString()));
        }
    }
    internal sealed class EphemerisItem
    {
        public string Name { get; private set; }
        public Orbiter Orbiter { get; private set; }
        public Vector Position { get; private set; }
        public Vector Velocity { get; private set; }

        public EphemerisItem(string Name, Vector Position, Vector Velocity)
        {
            this.Name = Name;
            this.Position = Position;
            this.Velocity = Velocity;
            this.Orbiter = null;
        }
        public EphemerisItem(Orbiter Orbiter)
        {
            this.Name = Orbiter.Name;
            this.Position = Orbiter.Position / 1000.0;
            this.Velocity = Orbiter.Velocity / 1000.0;
            this.Orbiter = Orbiter;
        }
        public void Link(Orbiter Orbiter)
        {
            this.Orbiter = Orbiter;
        }
        public bool IsLinked
        {
            get { return this.Orbiter != null; }
        }
        public override string ToString()
        {
            if (this.Orbiter != null)
                return string.Format("{0} Linked", this.Orbiter.Name);
            else
                return this.Name;
        }
    }
}
