using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal enum DirectoryLocation { Root, Data }
    internal static class Util
    {
        public const double METERS_PER_AU = 149597870700;
        public const double METERS_PER_LIGHT_YEAR = 9.4607304725808E+15;
        public const double METERS_PER_PARSEC = 30.857E+15;

        public const double ECLIPTIC_EQUINOX_DEGREES = 0;
        public const double ECLIPTIC_OBLIQUITY_DEGREES = 23.4392803055555555556;
        public const double FIRST_PT_OF_ARIES_DEG = 0.337;//.329;

        public const string DOUBLE_STRING_FORMAT = "R";//"0.0#############################e+000";

        public static Vector LocationFromEquatorialCoords(double RightAscensionInHours, double DeclinationInDegrees, double DistanceInParsecs)
        {
            System.Diagnostics.Debug.Assert(DeclinationInDegrees >= -90 && DeclinationInDegrees <= 90);

            return new Vector(Util.METERS_PER_PARSEC * DistanceInParsecs, 0, 0)
                   .GetRotationAboutYAxis(-DeclinationInDegrees.ToRadiansFromDegrees())
                   .GetRotationAboutZAxis(RightAscensionInHours.ToRadiansFromHours())
                   .GetRotationAbout(Vector.UnitZ ^ CelestialBody.Earth.Axis, CelestialBody.Earth.Axis.Tilt);
        }
        public static void EquatorialCoordsFromLocation(Vector Location, out double RightAscension, out double Declination)
        {
            Location = Location.GetRotationAbout(Vector.UnitZ ^ CelestialBody.Earth.Axis, -CelestialBody.Earth.Axis.Tilt);

            RightAscension = Location.Azimuth;
            Declination = Location.Inclination;
        }
        static Util()
        {
            RefractionPressureTempCorrectionFactor = 1.02 * Preferences.AtmosphericPressureMillibars / 1010 * 283 / (273 + Preferences.AtmosphericTemperatureCelsius) / 60;
            RefractionExtraCorrectionFactor = 0.0019279;
        }
        
        private static bool getDateStringWasUTC = false;
        private static DateTime prevDateForDateString = DateTime.MinValue;
        private static string prevGetDateString = string.Empty;
        public static string GetDateString(DateTime DateTimeUTC, bool UTC)
        {
            if (getDateStringWasUTC == UTC && DateTimeUTC == prevDateForDateString)
                return prevGetDateString;
            
            getDateStringWasUTC = UTC;
            prevDateForDateString = DateTimeUTC;

            if (UTC)
                return prevGetDateString = String.Format("{0:yyyy/MM/dd HH:mm:ss} UTC", DateTimeUTC);
            else
                return prevGetDateString = String.Format("{0:yyyy/MM/dd h:mm:ss tt} Local", DateTimeUTC.ToLocalTime());
        }

        private static double RefractionPressureTempCorrectionFactor;
        private static double RefractionExtraCorrectionFactor;
        private static Dictionary<double, double> inclinationsApparentToReal = new Dictionary<double, double>();

        public static double GetInverseRefractionCorrection(double ApparentInclination)
        {
            // Returns a negative value
            return GetRealInclination(ApparentInclination) - ApparentInclination;
        }
        public static double GetRealInclination(double ApparentInclination)
        {
            double inc;

            if (inclinationsApparentToReal.TryGetValue(ApparentInclination, out inc))
                return inc;
            
            inc = ApparentInclination;
            int count = 0;
            const int MAX_ITERATIONS = 50;
            double err;
            while (Math.Abs(err = (inc.Refract() - ApparentInclination)) > MathEx.EPSILON)
            {
                inc -= err;
                if (++count > MAX_ITERATIONS)
                {
#if DEBUG
                    throw new Exception(string.Format("Failed to find inverse refraction for {0:00.0000}", ApparentInclination));
#else
                    return ApparentInclination;
#endif
                }
            }
            inclinationsApparentToReal.Add(ApparentInclination, inc);
            return inc;
        }
        public static double GetRefractionCorrection(double RealInclination)
        {
            var inc = RealInclination.ToDegreesFromRadians();
            var correctionInDegrees = RefractionPressureTempCorrectionFactor / Math.Tan((inc + 10.3 / (inc + 5.11)).ToRadiansFromDegrees()) + RefractionExtraCorrectionFactor;

            return correctionInDegrees.ToRadiansFromDegrees();
        }
        public static double GetApparentInclination(double RealInclination)
        {
            return RealInclination + GetRefractionCorrection(RealInclination);
        }
    }
}
