using System;
using System.Globalization;
using System.Text;

namespace SolarMax;

public static class MathEx
{
    public const double TWO_PI = Math.PI * 2.0;
    public const double HALF_PI = Math.PI / 2.0;
    public const double DEG_PER_RAD = 360.0 / Math.PI / 2.0;
    public const double RAD_PER_DEG = Math.PI * 2.0 / 360.0;
    public const double ONE_DEGREE_IN_RADIANS = 1.0 * RAD_PER_DEG;
    public const double TWO_DEGREES_IN_RADIANS = 2.0 * ONE_DEGREE_IN_RADIANS;
    public const double MINUTES_PER_DEGREE = 60.0;
    public const double SECONDS_PER_DEGREE = 3600.0;
    public const double ONE_SECOND_IN_DEGREES = 1.0 / SECONDS_PER_DEGREE;
    public const double HALF_SECOND_IN_DEGREES = 0.5 * ONE_SECOND_IN_DEGREES;
    public const double ONE_SECOND_IN_RADIANS = ONE_DEGREE_IN_RADIANS / SECONDS_PER_DEGREE;
    public const double HALF_SECOND_IN_RADIANS = 0.5 * ONE_SECOND_IN_RADIANS;
    public const double SECONDS_PER_DAY = 86400;
    public const double SECONDS_PER_WEEK = SECONDS_PER_DAY * 7;
    public const double MINUTES_PER_HOUR = 60.0;
    public const double SECONDS_PER_MINUTE = 60.0;
    public const double SECONDS_PER_HOUR = 3600.0;
    public const double EPSILON = 1E-10;

    public static double ToDegreesFromRadians(this double Angle) => Angle * DEG_PER_RAD;
    public static double ToHoursFromRadians(this double Radians) => Radians.NormalizeAngleZeroToTwoPi() * 24.0 / TWO_PI;
    public static double ToRadiansFromSeconds(this double Seconds) => Seconds / (SECONDS_PER_DEGREE / RAD_PER_DEG);
    public static double ToDegreesFromSeconds(this double Seconds) => Seconds / SECONDS_PER_DEGREE;
    public static double ToSecondsFromRadians(this double Angle) => Angle * DEG_PER_RAD * SECONDS_PER_DEGREE;
    public static double ToRadiansFromDegrees(this double Angle) => Angle * RAD_PER_DEG;
    public static double ToRadiansFromHours(this double Hours)
    {
        System.Diagnostics.Debug.Assert(Hours >= -24 && Hours < 24);
        return (Hours * 360 / 24).ToRadiansFromDegrees();
    }
    public static bool IsCloseAbsolute(this double D, double CompareTo) => Math.Abs(D - CompareTo) < EPSILON;
    public static bool IsCloseRelative(this double D, double CompareTo) => Math.Abs(D - CompareTo) < D * EPSILON;
    public static double Floor(this double D) => Math.Floor(D);
    public static int FloorAsInt(this double D) => (int)Math.Floor(D);
    public static bool IsPositive(this double D) => D > 0;
    public static bool IsNegative(this double D) => D < 0;
    public static bool IsNonNegative(this double D) => D >= 0;
    public static bool IsNonPositive(this double D) => D <= 0;
    public static double NormalizeAngleNegativePiToPi(this double Angle)
    {
        Angle = ((Angle % TWO_PI) + TWO_PI) % TWO_PI;

        if (Angle > Math.PI)
            Angle -= TWO_PI;

#if DEBUG
        if (Angle < -Math.PI || Angle > Math.PI)
            throw new Exception();
#endif
        return Angle;
    }
    public static double NormalizeAngleZeroToTwoPi(this double Angle)
    {
        Angle = ((Angle % TWO_PI) + TWO_PI) % TWO_PI;

#if DEBUG
        if (Angle < 0 || Angle > TWO_PI)
            throw new Exception();
#endif
        return Angle;
    }
    public static int NormalizeZeroToSixty(this int Input) => ((Input % 60) + 60) % 60;
    public static double ToNearestSecondInDegrees(this double AngleInRadians)
        => AngleInRadians.IsNonNegative()
               ? (AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians().Floor().ToDegreesFromSeconds()
               : -((-AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians()).Floor().ToDegreesFromSeconds();
    public static double ToNearestSecondInRadians(this double AngleInRadians) 
        => AngleInRadians.IsNonNegative()
            ? (AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians().Floor().ToRadiansFromSeconds()
            : -((-AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians()).Floor().ToRadiansFromSeconds();
    public static int Clamp(this int Input, int MinVal, int MaxVal)
        => Math.Max(MinVal, Math.Min(MaxVal, Input));
    public static float Clamp(this float Input, float MinVal, float MaxVal)
        => Math.Max(MinVal, Math.Min(MaxVal, Input));
    public static double Clamp(this double Input, double MinVal, double MaxVal) => Math.Max(MinVal, Math.Min(MaxVal, Input));
    public static int DaysBetween(this DateTime Date1, DateTime Date2)
        => Math.Abs((Date1 - Date2).TotalDays).Round();
    public static int Round(this double Input)
        => (Input + 0.5).FloorAsInt();
    public static bool IsSameDay(this DateTime Date1, DateTime Date2)
        => Date1.Year == Date2.Year && Date1.Month == Date2.Month && Date1.Day == Date2.Day;
    public static double SecondsApart(this DateTime Date1, DateTime Date2)
        => Math.Abs((Date1 - Date2).TotalSeconds);
    public static DateTime Min(DateTime Date1, DateTime Date2)
        => (Date1 < Date2) ? Date1 : Date2;
    public static DateTime Max(DateTime Date1, DateTime Date2)
        => (Date1 > Date2) ? Date1 : Date2;
    public static bool IsAfternoon(this DateTime Date)
        => Date.Hour > 11;
    public static double Refract(this double ActualInclination)
        => Util.GetApparentInclination(ActualInclination);
    public static double Unrefract(this double ApparentInclination)
        => Util.GetRealInclination(ApparentInclination);

    private static double prevLatAngle = double.MinValue;
    private static string prevLatString = string.Empty;
    public static string ToLatitudeString(this double AngleInRadians)
    {
        if (prevLatAngle == AngleInRadians)
            return prevLatString;

        prevLatAngle = AngleInRadians;

        return prevLatString =
                    AngleInRadians.IsNonNegative()
                        ? AngleInRadians.toDMSString("N")
                        : (-AngleInRadians).toDMSString("S");
    }
    private static double prevLongAngle = double.MinValue;
    private static string prevLongString = string.Empty;
    public static string ToLongitudeString(this double AngleInRadians)
    {
        if (prevLongAngle == AngleInRadians)
            return prevLongString;

        prevLongAngle = AngleInRadians;

        return prevLongString = 
                    AngleInRadians.IsNonNegative()
                        ? AngleInRadians.toDMSString("E")
                        : (-AngleInRadians).toDMSString("W");
    }

    private static double prevAzAngle = double.MinValue;
    private static string prevAzString = string.Empty;
    public static string ToAzimuthString(this double AngleInRadians)
    {
        if (prevAzAngle == AngleInRadians)
            return prevAzString;

        prevAzAngle = AngleInRadians;

        return prevAzString = (TWO_PI - AngleInRadians).NormalizeAngleZeroToTwoPi().ToDMSString();
    }
    private static double prevIncAngle = double.MinValue;
    private static string prevIncString = string.Empty;
    public static string ToInclinationString(this double AngleInRadians)
    {
        if (prevIncAngle == AngleInRadians)
            return prevIncString;

        prevIncAngle = AngleInRadians;

        return prevIncString = AngleInRadians.ToDMSString();
    }
    private static string toDMSString(this double AngleInRadians, string Suffix)
    {
        if (AngleInRadians.IsNegative())
            throw new Exception();

        var AngleInDegrees = AngleInRadians.ToNearestSecondInDegrees() + HALF_SECOND_IN_DEGREES;

        int deg = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * MINUTES_PER_DEGREE % MINUTES_PER_DEGREE;

        int min = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * SECONDS_PER_MINUTE % SECONDS_PER_MINUTE;

        int sec = AngleInDegrees.FloorAsInt();

        return $"{deg:00}° {min:00}' {sec:00}\" {Suffix} ";
    }
    public static string ToDMSString(this double AngleInRadians)
    {
        bool negative = AngleInRadians.IsNegative();

        if (negative)
            AngleInRadians = -AngleInRadians;

        var AngleInDegrees = AngleInRadians.ToNearestSecondInDegrees() + HALF_SECOND_IN_DEGREES;

        int deg = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * MINUTES_PER_DEGREE % MINUTES_PER_DEGREE;

        int min = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * SECONDS_PER_MINUTE % SECONDS_PER_MINUTE;

        int sec = AngleInDegrees.FloorAsInt();

        return negative 
            ? $"-{deg:00}° {min:00}' {sec:00}\"" 
            : $"{deg:00}° {min:00}' {sec:00}\"";
    }
    public static string ToDMSStringMillionthsSecond(this double AngleInRadians)
    {
        bool negative = AngleInRadians.IsNegative();

        if (negative)
            AngleInRadians = -AngleInRadians;

        var AngleInDegrees = AngleInRadians * DEG_PER_RAD + EPSILON;

        int deg = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * MINUTES_PER_DEGREE % MINUTES_PER_DEGREE;

        int min = AngleInDegrees.FloorAsInt();
        AngleInDegrees = AngleInDegrees * SECONDS_PER_MINUTE % SECONDS_PER_MINUTE;

        double sec = AngleInDegrees;

        if (negative)
            return $"-{deg:00}° {min:00}' {sec:00.000000}\"";
        else
            return $"{deg:00}° {min:00}' {sec:00.000000}\"";
    }
    private static double prevHourAngle = double.MinValue;
    private static string prevHourString = string.Empty;

    public static string ToHourAngleString(this double AngleInRadians)
    {
        if (prevHourAngle == AngleInRadians)
            return prevHourString;

        prevHourAngle = AngleInRadians;

        var angle = AngleInRadians.ToHoursFromRadians();

        int hr = angle.FloorAsInt();
        angle = angle * MINUTES_PER_HOUR % MINUTES_PER_HOUR;

        int min = angle.FloorAsInt();
        angle = angle * SECONDS_PER_MINUTE % SECONDS_PER_MINUTE;

        return prevHourString = $"{hr:00}H {min:00}M {angle:00.0}S";
    }
    public static double AngleInRadians(double Degrees, double Minutes, double Seconds)
        => (Degrees + Minutes / MINUTES_PER_DEGREE + Seconds / SECONDS_PER_DEGREE).ToRadiansFromDegrees().NormalizeAngleNegativePiToPi();
    public static void ToComponents(this double AngleInRadians, out int Degrees, out int Minutes, out int Seconds, out bool Positive)
    {
        Positive = AngleInRadians.IsNonNegative();

        double angleInDegrees 
            = Positive
                ? AngleInRadians.ToDegreesFromRadians()
                : -AngleInRadians.ToDegreesFromRadians();

        angleInDegrees += HALF_SECOND_IN_DEGREES;

        Degrees = angleInDegrees.FloorAsInt();
        angleInDegrees = angleInDegrees * MINUTES_PER_DEGREE % MINUTES_PER_DEGREE;

        Minutes = angleInDegrees.FloorAsInt();
        angleInDegrees = angleInDegrees * SECONDS_PER_MINUTE % SECONDS_PER_MINUTE;

        Seconds = angleInDegrees.FloorAsInt();
    }
    public static double FromComponents(int Degrees, int Minutes, int Seconds, bool NorthOrEast)
        => (NorthOrEast ? 1.0 : -1.0) *
           (Degrees * 3600 + Minutes * 60 + Seconds + EPSILON).ToRadiansFromSeconds();
    public static double AngleDifferenceBetweenNegPiToPi(this double Angle1, double Angle2)
        => (Angle1 - Angle2).NormalizeAngleNegativePiToPi();
    public static float ParseFloat(this string Input, float Default)
    {
        if (Input is null)
            return Default;

#if DEBUG
        if (Input.Trim().Length != Input.Length)
            throw new Exception("Padded string in MathEx.ParseFloat: " + Input.Trim());
#endif

        if (Input.Length == 0)
            return Default;
        
        Input = Input.ToLower();
        if (Input.IndexOf('e') > 0)
        {
            string exp = Input[(Input.IndexOf('e') + 1)..];
            if (exp[0] == '+')
                exp = exp[1..];
            return (float)(double.Parse(Input[..Input.IndexOf('e')]) * Math.Pow(10.0, int.Parse(exp)));
        }
        else
        {
            return float.Parse(Input);
        }
    }
    public static double ParseDouble(this string Input, double Default)
    {
        if (Input == null)
            return Default;

#if DEBUG
        if (Input.Trim().Length != Input.Length)
            throw new Exception("Padded string in MathEx.ParseDouble: " + Input.Trim());
#endif
        if (Input.Length == 0)
            return Default;

        Input = Input.ToLower();
        
        if (Input.IndexOf('e') > 0)
        {
            string exp = Input[(Input.IndexOf('e') + 1)..];
            if (exp[0] == '+')
                exp = exp[1..];
            return double.Parse(Input[..Input.IndexOf('e')]) * Math.Pow(10.0, Int32.Parse(exp));
        }
        else
        {
            return double.Parse(Input);
        }
    }
    public static int ParseInt(this string Input, int Default)
    {
        int result;

#if DEBUG
        if (Input.Trim().Length != Input.Length)
            throw new Exception("Padded string in MathEx.ParseInt: " + Input.Trim());
#endif

        if (Input.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
        {
            return int.TryParse(Input[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result)
                ? result
                : Default;
        }
        else
        {
            if (int.TryParse(Input, out result))
                return result;
            else
                return Default;
        }
    }

    private static readonly char[] superscripts = ['\u2070', '\u00b9', '\u00b2', '\u00b3', '\u2074', '\u2075', '\u2076', '\u2077', '\u2078', '\u2079'];

    public static string ToSuperscript(this int Number)
    {
        StringBuilder sb = new(Number.ToString());

        for (int i = 0; i < sb.Length; i++)
            sb[i] = superscripts[sb[i] - 0x30];

        return sb.ToString();
    }
}
