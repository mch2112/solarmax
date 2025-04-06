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
    private string testReport(Ephemeris Current, Ephemeris Actual)
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

        showPhaseDistError(Current, Actual, sb, "Sun", "Mercury");
        showPhaseDistError(Current, Actual, sb, "Sun", "Venus");
        showPhaseDistError(Current, Actual, sb, "Sun", "Earth");
        showPhaseDistError(Current, Actual, sb, "Sun", "Mars");
        showPhaseDistError(Current, Actual, sb, "Sun", "Jupiter");
        showPhaseDistError(Current, Actual, sb, "Sun", "Saturn");
        showPhaseDistError(Current, Actual, sb, "Sun", "Uranus");
        showPhaseDistError(Current, Actual, sb, "Sun", "Neptune");
        showPhaseDistError(Current, Actual, sb, "Sun", "Pluto");
        showPhaseDistError(Current, Actual, sb, "Earth", "Moon");
        showPhaseDistError(Current, Actual, sb, "Mars", "Phobos");
        showPhaseDistError(Current, Actual, sb, "Mars", "Deimos");
        showPhaseDistError(Current, Actual, sb, "Jupiter", "Io");
        showPhaseDistError(Current, Actual, sb, "Jupiter", "Europa");
        showPhaseDistError(Current, Actual, sb, "Jupiter", "Ganymede");
        showPhaseDistError(Current, Actual, sb, "Jupiter", "Callisto");
        showPhaseDistError(Current, Actual, sb, "Saturn", "Titan");
        showPhaseDistError(Current, Actual, sb, "Saturn", "Mimas");
        showPhaseDistError(Current, Actual, sb, "Uranus", "Titania");
        showPhaseDistError(Current, Actual, sb, "Neptune", "Triton");
        showPhaseDistError(Current, Actual, sb, "Neptune", "Larissa");
        showPhaseDistError(Current, Actual, sb, "Pluto", "Charon");

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

    private static void showPhaseDistError(Ephemeris Current, Ephemeris Actual, StringBuilder sb, string Planet, string Moon)
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
