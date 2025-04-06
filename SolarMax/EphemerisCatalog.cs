using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax;

internal sealed class EphemerisCatalog
{
    private readonly List<Ephemeris> list = new(Ephemeris.STARTING_CATALOG_LENGTH);
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
        StringBuilder sb = new();
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
    public override string ToString() => string.Join(Environment.NewLine, list.Select(l => l.Date.ToString()));
}
