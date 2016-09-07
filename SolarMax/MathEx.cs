using System;
using System.Collections.Generic;
using System.Text;

namespace SolarMax
{
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

        public static double ToDegreesFromRadians(this double Angle)
        {
            return Angle * DEG_PER_RAD;
        }
        public static double ToHoursFromRadians(this double Radians)
        {
            return Radians.NormalizeAngleZeroToTwoPi() * 24.0 / MathEx.TWO_PI;
        }
        public static double ToRadiansFromSeconds(this double Seconds)
        {
            return Seconds / (SECONDS_PER_DEGREE / RAD_PER_DEG);
        }
        public static double ToDegreesFromSeconds(this double Seconds)
        {
            return Seconds / SECONDS_PER_DEGREE;
        }
        public static double ToSecondsFromRadians(this double Angle)
        {
            return Angle * DEG_PER_RAD * SECONDS_PER_DEGREE;
        }
        public static double ToRadiansFromDegrees(this double Angle)
        {
            return Angle * RAD_PER_DEG;
        }
        public static double ToRadiansFromHours(this double Hours)
        {
            System.Diagnostics.Debug.Assert(Hours >= -24 && Hours < 24);
            return (Hours * 360 / 24).ToRadiansFromDegrees();
        }
        public static bool IsCloseAbsolute(this double D, double CompareTo)
        {
            return Math.Abs(D - CompareTo) < MathEx.EPSILON;
        }
        public static bool IsCloseRelative(this double D, double CompareTo)
        {
            return Math.Abs(D - CompareTo) < D * MathEx.EPSILON;
        }
        public static double Floor(this double D)
        {
            return Math.Floor(D);
        }
        public static int FloorAsInt(this double D)
        {
            return (int)Math.Floor(D);
        }
        public static bool IsPositive(this double D)
        {
            return D > 0;
        }
        public static bool IsNegative(this double D)
        {
            return D < 0;
        }
        public static bool IsNonNegative(this double D)
        {
            return D >= 0;
        }
        public static bool IsNonPositive(this double D)
        {
            return D <= 0;
        }
        public static double NormalizeAngleNegativePiToPi(this double Angle)
        {
            Angle = ((Angle % MathEx.TWO_PI) + MathEx.TWO_PI) % MathEx.TWO_PI;

            if (Angle > Math.PI)
                Angle -= MathEx.TWO_PI;

#if DEBUG
            if (Angle < -Math.PI || Angle > Math.PI)
                throw new Exception();
#endif
            return Angle;
        }
        public static double NormalizeAngleZeroToTwoPi(this double Angle)
        {
            Angle = ((Angle % MathEx.TWO_PI) + MathEx.TWO_PI) % MathEx.TWO_PI;

#if DEBUG
            if (Angle < 0 || Angle > MathEx.TWO_PI)
                throw new Exception();
#endif

            return Angle;
        }
        public static int NormalizeZeroToSixty(this int Input)
        {
            return ((Input % 60) + 60) % 60;
        }
        public static double ToNearestSecondInDegrees(this double AngleInRadians)
        {
            return AngleInRadians.IsNonNegative() ?
                   (AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians().Floor().ToDegreesFromSeconds() :
                   -((-AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians()).Floor().ToDegreesFromSeconds();
        }
        public static double ToNearestSecondInRadians(this double AngleInRadians)
        {
            return AngleInRadians.IsNonNegative() ?
                   (AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians().Floor().ToRadiansFromSeconds() :
                   -((-AngleInRadians + HALF_SECOND_IN_RADIANS).ToSecondsFromRadians()).Floor().ToRadiansFromSeconds();
        }
        public static int Clamp(this int Input, int MinVal, int MaxVal)
        {
            return Math.Max(MinVal, Math.Min(MaxVal, Input));
        }
        public static float Clamp(this float Input, float MinVal, float MaxVal)
        {
            return Math.Max(MinVal, Math.Min(MaxVal, Input));
        }
        public static double Clamp(this double Input, double MinVal, double MaxVal)
        {
            return Math.Max(MinVal, Math.Min(MaxVal, Input));
        }
        public static int DaysBetween(this DateTime Date1, DateTime Date2)
        {
            return Math.Abs((Date1 - Date2).TotalDays).Round();
        }
        public static int Round(this double Input)
        {
            return (Input + 0.5).FloorAsInt();
        }
        public static bool IsSameDay(this DateTime Date1, DateTime Date2)
        {
            return Date1.Year == Date2.Year && Date1.Month == Date2.Month && Date1.Day == Date2.Day;
        }
        public static double SecondsApart(this DateTime Date1, DateTime Date2)
        {
            return Math.Abs((Date1 - Date2).TotalSeconds);
        }
        public static DateTime Min(DateTime Date1, DateTime Date2)
        {
            return (Date1 < Date2) ? Date1 : Date2;
        }
        public static DateTime Max(DateTime Date1, DateTime Date2)
        {
            return (Date1 > Date2) ? Date1 : Date2;
        }
        public static bool IsAfternoon(this DateTime Date)
        {
            return Date.Hour > 11;
        }
        public static double Refract(this double ActualInclination)
        {
            return Util.GetApparentInclination(ActualInclination);
        }
        public static double Unrefract(this double ApparentInclination)
        {
            return Util.GetRealInclination(ApparentInclination);
        }

        private static double prevLatAngle = double.MinValue;
        private static string prevLatString = string.Empty;
        public static string ToLatitudeString(this double AngleInRadians)
        {
            if (prevLatAngle == AngleInRadians)
                return prevLatString;

            prevLatAngle = AngleInRadians;

            return prevLatString = 
                   (AngleInRadians.IsNonNegative() ?
                   AngleInRadians.toDMSString("N") :
                   (-AngleInRadians).toDMSString("S"));
        }
        private static double prevLongAngle = double.MinValue;
        private static string prevLongString = string.Empty;
        public static string ToLongitudeString(this double AngleInRadians)
        {
            if (prevLongAngle == AngleInRadians)
                return prevLongString;

            prevLongAngle = AngleInRadians;

            return prevLongString = 
                   (AngleInRadians.IsNonNegative() ?
                   AngleInRadians.toDMSString("E") :
                   (-AngleInRadians).toDMSString("W"));
        }

        private static double prevAzAngle = double.MinValue;
        private static string prevAzString = string.Empty;
        public static string ToAzimuthString(this double AngleInRadians)
        {
            if (prevAzAngle == AngleInRadians)
                return prevAzString;

            prevAzAngle = AngleInRadians;

            return prevAzString = ((MathEx.TWO_PI - AngleInRadians).NormalizeAngleZeroToTwoPi().ToDMSString());
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
            AngleInDegrees = AngleInDegrees * MathEx.MINUTES_PER_DEGREE % MathEx.MINUTES_PER_DEGREE;

            int min = AngleInDegrees.FloorAsInt();
            AngleInDegrees = AngleInDegrees * MathEx.SECONDS_PER_MINUTE % MathEx.SECONDS_PER_MINUTE;

            int sec = AngleInDegrees.FloorAsInt();

            return String.Format("{0:00}° {1:00}' {2:00}\" {3} ", deg, min, sec, Suffix);
        }
        public static string ToDMSString(this double AngleInRadians)
        {
            bool negative = AngleInRadians.IsNegative();

            if (negative)
                AngleInRadians = -AngleInRadians;

            var AngleInDegrees = AngleInRadians.ToNearestSecondInDegrees() + HALF_SECOND_IN_DEGREES;

            int deg = AngleInDegrees.FloorAsInt();
            AngleInDegrees = AngleInDegrees * MathEx.MINUTES_PER_DEGREE % MathEx.MINUTES_PER_DEGREE;

            int min = AngleInDegrees.FloorAsInt();
            AngleInDegrees = AngleInDegrees * MathEx.SECONDS_PER_MINUTE % MathEx.SECONDS_PER_MINUTE;

            int sec = AngleInDegrees.FloorAsInt();

            if (negative)
                return String.Format("-{0:00}° {1:00}' {2:00}\"", deg, min, sec);
            else
                return String.Format("{0:00}° {1:00}' {2:00}\"", deg, min, sec);
        }
        public static string ToDMSStringMillionthsSecond(this double AngleInRadians)
        {
            bool negative = AngleInRadians.IsNegative();

            if (negative)
                AngleInRadians = -AngleInRadians;

            var AngleInDegrees = AngleInRadians * MathEx.DEG_PER_RAD + MathEx.EPSILON;

            int deg = AngleInDegrees.FloorAsInt();
            AngleInDegrees = AngleInDegrees * MathEx.MINUTES_PER_DEGREE % MathEx.MINUTES_PER_DEGREE;

            int min = AngleInDegrees.FloorAsInt();
            AngleInDegrees = AngleInDegrees * MathEx.SECONDS_PER_MINUTE % MathEx.SECONDS_PER_MINUTE;

            double sec = AngleInDegrees;

            if (negative)
                return String.Format("-{0:00}° {1:00}' {2:00.000000}\"", deg, min, sec);
            else
                return String.Format("{0:00}° {1:00}' {2:00.000000}\"", deg, min, sec);
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
            angle = angle * MathEx.MINUTES_PER_HOUR % MathEx.MINUTES_PER_HOUR;

            int min = angle.FloorAsInt();
            angle = angle * MathEx.SECONDS_PER_MINUTE % MathEx.SECONDS_PER_MINUTE;

            return prevHourString = String.Format("{0:00}H {1:00}M {2:00.0}S", hr, min, angle);
        }
        public static double AngleInRadians(double Degrees, double Minutes, double Seconds)
        {
            return ((Degrees + Minutes / MathEx.MINUTES_PER_DEGREE + Seconds / MathEx.SECONDS_PER_DEGREE).ToRadiansFromDegrees()).NormalizeAngleNegativePiToPi();
        }
        public static void ToComponents(this double AngleInRadians, out int Degrees, out int Minutes, out int Seconds, out bool Positive)
        {
            Positive = AngleInRadians.IsNonNegative();

            double angleInDegrees;
            if (Positive)
            {
                angleInDegrees = AngleInRadians.ToDegreesFromRadians();
            }
            else
            {
                angleInDegrees = -AngleInRadians.ToDegreesFromRadians();
            }

            angleInDegrees += MathEx.HALF_SECOND_IN_DEGREES;

            Degrees = angleInDegrees.FloorAsInt();
            angleInDegrees = angleInDegrees * MathEx.MINUTES_PER_DEGREE % MathEx.MINUTES_PER_DEGREE;

            Minutes = angleInDegrees.FloorAsInt();
            angleInDegrees = angleInDegrees * MathEx.SECONDS_PER_MINUTE % MathEx.SECONDS_PER_MINUTE;

            Seconds = angleInDegrees.FloorAsInt();
        }
        public static double FromComponents(int Degrees, int Minutes, int Seconds, bool NorthOrEast)
        {
            return (NorthOrEast ? 1.0 : -1.0) *
                   (((double)(Degrees * 3600 + Minutes * 60 + Seconds)) + MathEx.EPSILON).ToRadiansFromSeconds();
        }
        public static double AngleDifferenceBetweenNegPiToPi(this double Angle1, double Angle2)
        {
            return (Angle1 - Angle2).NormalizeAngleNegativePiToPi();
        }
        public static float ParseFloat(this string Input, float Default)
        {
            if (Input == null)
                return Default;

#if DEBUG
            if (Input.Trim().Length != Input.Length)
                throw new Exception("Padded string in MathEx.ParseFloat: " + Input.Trim());
#endif

            Input = Input.ToLower();
            if (Input.Length == 0)
            {
                return Default;
            }
            else if (Input.IndexOf('e') > 0)
            {
                string exp = Input.Substring(Input.IndexOf('e') + 1);
                if (exp[0] == '+')
                    exp = exp.Substring(1);
                string mant = Input.Substring(0, Input.IndexOf('e'));
                return (float)((double.Parse(mant)) * Math.Pow(10.0, Int32.Parse(exp)));
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

            Input = Input.ToLower();
            if (Input.Length == 0)
            {
                return Default;
            }
            else if (Input.IndexOf('e') > 0)
            {
                string exp = Input.Substring(Input.IndexOf('e') + 1);
                if (exp[0] == '+')
                    exp = exp.Substring(1);
                string mant = Input.Substring(0, Input.IndexOf('e'));
                return (double.Parse(mant)) * Math.Pow(10.0, Int32.Parse(exp));
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

            if (Input.ToLower().StartsWith("0x"))
            {
                if (Int32.TryParse(Input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result))
                    return result;
                else
                    return Default;
            }
            else
            {
                if (Int32.TryParse(Input, out result))
                    return result;
                else
                    return Default;
            }
        }

        private static char[] superscripts = new char[] { '\u2070', '\u00b9', '\u00b2', '\u00b3', '\u2074', '\u2075', '\u2076', '\u2077', '\u2078', '\u2079' };

        public static string ToSuperscript(this int Number)
        {
            StringBuilder sb = new StringBuilder(Number.ToString());

            for (int i = 0; i < sb.Length; i++)
                sb[i] = superscripts[sb[i] - 0x30];

            return sb.ToString();
        }
    }
}
