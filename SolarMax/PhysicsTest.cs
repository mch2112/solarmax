using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax;

internal sealed partial class Physics
{
    private readonly DateTime TEST_DATE = new(2011, 2, 23, 12, 0, 0);
    private const bool TEST = false;
    private Ephemeris testEphemeris = null;
    private string TestReport(Ephemeris Current, Ephemeris Actual)
    {
        if (!TEST)
            throw new Exception();

        StringBuilder sb = new();
        Vector sunPosCurrent = Current.ItemDictionary["Sun"].Position;
        Vector sunPosActual = Actual.ItemDictionary["Sun"].Position;
        Vector earthPosCurrent = Current.ItemDictionary["Earth"].Position;
        Vector earthPosActual = Actual.ItemDictionary["Earth"].Position;

        List<Tuple<Orbiter, Vector, Vector>> errors = [];

        int days = Current.BasedOnDate.DaysBetween(Actual.Date);
        sb.AppendLine(string.Format("Error Summary: {0} days integration {1} sec/step", days, TIME_SLICE_IDEAL));
        sb.AppendLine(string.Format("Start Date: {0} End Date: {1}", Current.BasedOnDate.ToString(DATE_FORMAT_1), Current.Date.ToString(DATE_FORMAT_1)));

        foreach (var e in Current.Items.Where(e => e.Name != "Sun"))
            if (Actual.ItemDictionary.TryGetValue(e.Name, out EphemerisItem value))
                errors.Add(new Tuple<Orbiter, Vector, Vector>(e.Orbiter,
                                                              (e.Position - sunPosCurrent) - (value.Position - sunPosActual),
                                                              value.Position));

        sb.AppendLine($"Average Error: {errors.Average(e => e.Item2.Magnitude):0.000000}km");

        sb.AppendLine(string.Format("Large Objects Average Error (Over 1E+24 kg): {0:0.000000}km ({1:0.0000000}km per day)",
                                    errors.Where(e => e.Item1.Mass >= 1E+27)
                                          .Average(e => e.Item2.Magnitude),
                                    errors.Where(e => e.Item1.Mass >= 1E+27)
                                          .Average(e => e.Item2.Magnitude) / (double)days));

        sb.AppendLine(string.Format("Small Objects Average Error (Under 1E+24 kg): {0:0.000000}km ({1:0.0000000}km per day)",
                                    errors.Where(e => e.Item1.Mass < 1E+27)
                                          .Average(e => e.Item2.Magnitude),
                                    errors.Where(e => e.Item1.Mass < 1E+27)
                                          .Average(e => e.Item2.Magnitude) / (double)days));

        ShowPhaseDistError(Current, Actual, sb, "Sun", "Mercury");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Venus");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Earth");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Mars");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Jupiter");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Saturn");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Uranus");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Neptune");
        ShowPhaseDistError(Current, Actual, sb, "Sun", "Pluto");
        ShowPhaseDistError(Current, Actual, sb, "Earth", "Moon");
        ShowPhaseDistError(Current, Actual, sb, "Mars", "Phobos");
        ShowPhaseDistError(Current, Actual, sb, "Mars", "Deimos");
        ShowPhaseDistError(Current, Actual, sb, "Jupiter", "Io");
        ShowPhaseDistError(Current, Actual, sb, "Jupiter", "Europa");
        ShowPhaseDistError(Current, Actual, sb, "Jupiter", "Ganymede");
        ShowPhaseDistError(Current, Actual, sb, "Jupiter", "Callisto");
        ShowPhaseDistError(Current, Actual, sb, "Saturn", "Titan");
        ShowPhaseDistError(Current, Actual, sb, "Saturn", "Mimas");
        ShowPhaseDistError(Current, Actual, sb, "Uranus", "Titania");
        ShowPhaseDistError(Current, Actual, sb, "Neptune", "Triton");
        ShowPhaseDistError(Current, Actual, sb, "Neptune", "Larissa");
        ShowPhaseDistError(Current, Actual, sb, "Pluto", "Charon");

        sb.AppendLine();
        sb.AppendLine("Error Details:");

        foreach (var e in errors)
            if (e.Item1.Equals(CelestialBody.Earth))
                sb.AppendLine($"{e.Item1.Name}: Error {e.Item2.Magnitude:0.00000000}km");
            else
                sb.AppendLine(string.Format("{0}: Error {1:0.00000000}km Error angle from Earth: {2}",
                                            e.Item1.Name,
                                            e.Item2.Magnitude,
                                            (e.Item1.Position / 1000.0 - earthPosCurrent).AngleDiffAbs(e.Item3 - earthPosActual).ToDMSStringMillionthsSecond()));

        return sb.ToString();
    }

    private static void ShowPhaseDistError(Ephemeris Current, Ephemeris Actual, StringBuilder sb, string Planet, string Moon)
    {
        sb.AppendLine();
        sb.AppendLine($"{Planet} / {Moon}");

        var PlanetCurrent = Current.ItemDictionary[Planet].Position;
        var PlanetActual = Actual.ItemDictionary[Planet].Position;
        var MoonCurrent = Current.ItemDictionary[Moon].Position;
        var MoonActual = Actual.ItemDictionary[Moon].Position;

        sb.AppendLine(string.Format("Phase Current: {0} Actual: {1} Error: {2} ({3:0.0000000}km)",
                      (MoonCurrent - PlanetCurrent).Azimuth.NormalizeAngleZeroToTwoPi().ToDMSStringMillionthsSecond(),
                      (MoonActual - PlanetActual).Azimuth.NormalizeAngleZeroToTwoPi().ToDMSStringMillionthsSecond(),
                      ((MoonCurrent - PlanetCurrent).Azimuth - (MoonActual - PlanetActual).Azimuth).NormalizeAngleNegativePiToPi().ToDMSStringMillionthsSecond(),
                      Math.Abs(Math.Sin((MoonCurrent - PlanetCurrent).Azimuth - (MoonActual - PlanetActual).Azimuth) * (MoonCurrent - PlanetCurrent).Magnitude)));

        sb.AppendLine(string.Format("Distance Current: {0:0.0000}km Actual: {1:0.0000}km Error: {2:0.00000}km",
                      (MoonCurrent - PlanetCurrent).Magnitude,
                      (MoonActual - PlanetActual).Magnitude,
                      (MoonCurrent - PlanetCurrent).Magnitude - (MoonActual - PlanetActual).Magnitude));
    }
}
