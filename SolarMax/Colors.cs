using System;
using System.Collections.Generic;

namespace SolarMax
{
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

                addColor("default_interface_color", 90, 90, 210);
                addColor("ecliptic_compass", 0x16, 0x40, 0x40);
                addColor("ecliptic_grid", 0x09, 0x1D, 0x1D);
                addColor("equatorial_compass", 0x28, 0x48, 0x28);
                addColor("equatorial_grid", 8, 32, 8);
                addColor("local_compass", 70, 45, 20);
                addColor("local_grid", 40, 20, 10);
                addColor("border", 0xFF, 0xFF, 0xFF);
                addColor("instrument_line", 128, 0, 128);
                addColor("instrument_outline", 0, 128, 0);
                addColor("instrument_data", 30, 144, 240);
                addColor("planet_axis", 0x80, 0x80, 0x80);
                addColor("prime_meridian", 255, 0, 0);
                addColor("prime_meridian_dark", 128, 0, 0);
            
                addColor("Sun", 192, 191, 173);
                addColor("Mercury", 110, 110, 110);
                addColor("Venus", 200, 200, 195);
                addColor("Earth", 131, 138, 182);
                addColor("Mars", 220, 195, 144);
                addColor("Jupiter", 210, 210, 155);
                addColor("Saturn", 225, 230, 180);
                addColor("Uranus", 172, 200, 211);
                addColor("Neptune", 160, 183, 215);
                addColor("Moon", 176, 172, 170);
                addColor("Pluto", 232, 226, 192);
                addColor("Titan", 210, 190, 140);

                addColor("constellation_boundary", 32, 32, 32);
                addColor("sunlit_area", 255, 255, 0);

                string[,] s = IO.ReadFile(COLOR_FILENAME, DirectoryLocation.Root);

                for (int i = 0; i <= s.GetUpperBound(0); i++)
                {
                    try
                    {
                        addColor(s[i, 0],
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
            if (colorDictionary.TryGetValue(Key, out QColor c))
                return c;
            else
                return Default;
        }
        public static QColor GetColor(string Key, int MinIntensityIfNotFound, int MaxIntensityIfNotFound)
        {
            if (colorDictionary.TryGetValue(Key, out QColor c))
                return c;
            else
                return GetRandomColor(MinIntensityIfNotFound,
                                      MaxIntensityIfNotFound);
        }
        public static QColor GetColor(string Key)
        {
            if (colorDictionary.TryGetValue(Key, out QColor c))
                return c;
            else
                return new QColor(128, 128, 128);
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
        private static void addColor(string key, int Red, int Green, int Blue)
        {
            if (colorDictionary.ContainsKey(key))
                colorDictionary[key] = new QColor(Red, Green, Blue);
            else
                colorDictionary.Add(key, new QColor(Red, Green, Blue));
        }
    }
}
