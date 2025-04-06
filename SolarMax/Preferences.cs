using System;

namespace SolarMax;

public enum PlanetLabelType { NameOnly, SymbolOnly, Both }

internal static class Preferences
{
    private const string PREFERENCES_FILE_NAME = "preferences.txt";

    private const float DEFAULT_LABEL_FONT_SIZE = 8;

    private const double DEFAULT_TEMPERATURE_CELSIUS = 10;
    private const double DEFAULT_TEMPERATURE_FAHRENHEIT = DEFAULT_TEMPERATURE_CELSIUS * 9.0 / 5.0 + 32.0;
    private const double DEFAULT_PRESSURE_MILLIBARS = 1013.25;
    private const PlanetLabelType DEFAULT_ORBITER_LABEL_TYPE = PlanetLabelType.NameOnly;

    private static double atmosphericPressureMillibars = DEFAULT_PRESSURE_MILLIBARS;
    private static double atmosphericTemperatureCelsius = DEFAULT_TEMPERATURE_CELSIUS;
    private static float labelFontSize = DEFAULT_LABEL_FONT_SIZE;
    private static PlanetLabelType planetLabelType = DEFAULT_ORBITER_LABEL_TYPE;

    public static double AtmosphericPressureMillibars
    {
        get { return atmosphericPressureMillibars; }
        private set
        {
            atmosphericPressureMillibars = value.Clamp(DEFAULT_PRESSURE_MILLIBARS * 0.8, DEFAULT_PRESSURE_MILLIBARS * 1.2);
        }
    }
    public static double AtmosphericTemperatureCelsius
    {
        get { return atmosphericTemperatureCelsius; }
        private set
        {
            atmosphericTemperatureCelsius = value.Clamp(DEFAULT_TEMPERATURE_CELSIUS - 60, DEFAULT_TEMPERATURE_CELSIUS + 60);
        }
    }
    public static float LabelFontSize
    {
        get => labelFontSize;
        private set => labelFontSize = value.Clamp(3f, 16f);
    }
    public static PlanetLabelType PlanetLabelType
    {
        get => planetLabelType;
        set => planetLabelType = value;
    }
    static Preferences()
    {
        string[,] s = IO.ReadFile(PREFERENCES_FILE_NAME, DirectoryLocation.Root);
        for (int i = 0; i <= s.GetUpperBound(0); i++)
        {
            switch (s[i, 0])
            {
                case "atmospheric_pressure_millibars":
                    Preferences.AtmosphericPressureMillibars = s[i,1].ParseDouble(DEFAULT_PRESSURE_MILLIBARS);
                    break;
                case "atmospheric_temperature_fahrenheit":
                    Preferences.AtmosphericTemperatureCelsius = (s[i, 1].ParseDouble(DEFAULT_TEMPERATURE_FAHRENHEIT) - 32.0) * (5.0 / 9.0);
                    break;
                case "atmospheric_temperature_celsius":
                    Preferences.AtmosphericTemperatureCelsius = s[i, 1].ParseDouble(DEFAULT_TEMPERATURE_CELSIUS);
                    break;
                case "label_font_size":
                    Preferences.LabelFontSize = s[i, 1].ParseFloat(DEFAULT_LABEL_FONT_SIZE);
                    break;
                case "planet_label_type":
                    switch (s[i, 1].ToLower())
                    {
                        case "both":
                            planetLabelType = PlanetLabelType.Both;
                            break;
                        case "symbol":
                        case "symbolonly":
                            planetLabelType = PlanetLabelType.SymbolOnly;
                            break;
                        case "name":
                        case "nameonly":
                            planetLabelType = PlanetLabelType.NameOnly;
                            break;
                    }
                    break;
#if DEBUG
                default:
                    throw new Exception("Invalid Preference Key:" + s[i, 0]);
#endif
            }
        }
    }
}
