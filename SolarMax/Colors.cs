using System;
using System.Collections.Generic;

namespace SolarMax;

internal static class Colors
{
    public const string COLOR_FILENAME = "colors.txt";

    private const int MAX_COLOR_INDEX_BRIGHTNESS = 255;
    private const int MIN_COLOR_INDEX_BRIGHTNESS = 10;

    private static readonly Dictionary<string, QColor> colorDictionary;
    private static readonly Random r = new();

    static Colors()
    {
        try
        {
            colorDictionary = [];

            AddColor("default_interface_color", 90, 90, 210);
            AddColor("ecliptic_compass", 0x16, 0x40, 0x40);
            AddColor("ecliptic_grid", 0x09, 0x1D, 0x1D);
            AddColor("equatorial_compass", 0x28, 0x48, 0x28);
            AddColor("equatorial_grid", 8, 32, 8);
            AddColor("local_compass", 70, 45, 20);
            AddColor("local_grid", 40, 20, 10);
            AddColor("border", 0xFF, 0xFF, 0xFF);
            AddColor("instrument_line", 128, 0, 128);
            AddColor("instrument_outline", 0, 128, 0);
            AddColor("instrument_data", 30, 144, 240);
            AddColor("planet_axis", 0x80, 0x80, 0x80);
            AddColor("prime_meridian", 255, 0, 0);
            AddColor("prime_meridian_dark", 128, 0, 0);
        
            AddColor("Sun", 192, 191, 173);
            AddColor("Mercury", 110, 110, 110);
            AddColor("Venus", 200, 200, 195);
            AddColor("Earth", 131, 138, 182);
            AddColor("Mars", 220, 195, 144);
            AddColor("Jupiter", 210, 210, 155);
            AddColor("Saturn", 225, 230, 180);
            AddColor("Uranus", 172, 200, 211);
            AddColor("Neptune", 160, 183, 215);
            AddColor("Moon", 176, 172, 170);
            AddColor("Pluto", 232, 226, 192);
            AddColor("Titan", 210, 190, 140);

            AddColor("constellation_boundary", 32, 32, 32);
            AddColor("sunlit_area", 255, 255, 0);

            string[,] s = IO.ReadFile(COLOR_FILENAME, DirectoryLocation.Root);

            for (int i = 0; i <= s.GetUpperBound(0); i++)
            {
                try
                {
                    AddColor(s[i, 0],
                             s[i, 1].ParseInt(128),
                             s[i, 2].ParseInt(128),
                             s[i, 3].ParseInt(128));
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }
    public static QColor GetColor(string Key, QColor Default)
    {
        return colorDictionary.TryGetValue(Key, out QColor c) ? c : Default;
    }
    public static QColor GetColor(string Key, int MinIntensityIfNotFound, int MaxIntensityIfNotFound)
    {
        return colorDictionary.TryGetValue(Key, out QColor c)
            ? c
            : GetRandomColor(MinIntensityIfNotFound,
                                  MaxIntensityIfNotFound);
    }
    public static QColor GetColor(string Key)
    {
        return colorDictionary.TryGetValue(Key, out QColor c) ? c : new QColor(128, 128, 128);
    }
    public static QColor GetRandomColor(int MinIntensity, int MaxIntensity)
    {
        return new QColor(r.Next(MaxIntensity + 1 - MinIntensity) + MinIntensity,
                          r.Next(MaxIntensity + 1 - MinIntensity) + MinIntensity,
                          r.Next(MaxIntensity + 1 - MinIntensity) + MinIntensity);
    }
    public static QColor ExtraDarken(QColor Input)
    {
        return new QColor(Input.Red   / 3,
                          Input.Green / 3,
                          Input.Blue  / 3);
    }
    public static QColor GetColorFromColorIndex(float Magnitude, float ColorIndex)
    {
        var adjustedColorIndex = ColorIndex / 10;

        var brightness = 255 - (Magnitude + 1) * 32;
        return new QColor((brightness * (1 + adjustedColorIndex)).Clamp(MIN_COLOR_INDEX_BRIGHTNESS, MAX_COLOR_INDEX_BRIGHTNESS),
                          brightness.Clamp(MIN_COLOR_INDEX_BRIGHTNESS, MAX_COLOR_INDEX_BRIGHTNESS),
                          (brightness * (1 - adjustedColorIndex)).Clamp(MIN_COLOR_INDEX_BRIGHTNESS, MAX_COLOR_INDEX_BRIGHTNESS));
    }
    private static void AddColor(string key, int Red, int Green, int Blue)
    {
        if (colorDictionary.ContainsKey(key))
            colorDictionary[key] = new QColor(Red, Green, Blue);
        else
            colorDictionary.Add(key, new QColor(Red, Green, Blue));
    }
}
