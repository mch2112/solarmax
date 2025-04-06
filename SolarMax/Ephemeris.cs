using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SolarMax;

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
                itemDictionary = [];
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
        this.Date = ParseDate(Data[Index, 0]);
        if (this.VersionCount > 0)
        {
            this.BasedOnDate = ParseDate(Data[Index, 1]);
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

        EphemerisCatalog ec = new();

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
            foreach (var o in Orbiters)
            {
                if (id.TryGetValue(o.Name, out EphemerisItem ei))
                {
                    ei.Link(o);
                    if (o.IsDead)
                    {
                        o.Reify();
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

    public override string ToString() => 
        $"{this.Date.ToShortDateString()} {this.Date.ToShortTimeString()} - {this.Items.Count} Items{(this.IsLinked ? " Linked" : string.Empty)} {this.MinDate.ToShortDateString()} {this.MaxDate.ToShortDateString()}";

    private static DateTime ParseDate(string Date)
        => DateTime.ParseExact(Date.Trim(),
                               [DATE_FORMAT_1, DATE_FORMAT_2],
                               CultureInfo.InvariantCulture,
                               DateTimeStyles.None);
}
