using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class Star : CelestialBody
    {
        private const double STAR_RADIUS = 5E+7;
        private const double STAR_MASS = 1E+31;

        public float Magnitude { get; private set; }
        public int HRNum { get; private set; }
        public int HDNum { get; private set; }

        public double RightAscensionInHours { get; private set; }
        public double DeclinationInDegrees { get; private set; }

        private static ResettableShape shapeSmall = Shape.GetSphere(32, 16, 8, STAR_RADIUS).ToResettable();
        private static ResettableShape shapeMedium = Shape.GetSphere(48, 24, 12, STAR_RADIUS).ToResettable();
        private static ResettableShape shapeBig = Shape.GetSphere(64, 32, 16, STAR_RADIUS).ToResettable();

        public string ProperName { get; private set; }
        
        public StarDesignation StarDesignation { get; private set; }
        
        public float ColorIndex { get; private set; }

        //private static Dictionary<int, string> properNames = new Dictionary<int, string>();


        //static Star()
        //{
        //    var s = IO.ReadFile("propernames.txt", DirectoryLocation.Data);

        //    for (int i = 0; i < s.GetUpperBound(0); i++)
        //        properNames.Add(s[i, 0].ParseInt(0), s[i, 1]);
        //}
        public static int pncount = 0;
        public Star(string Name, int HRNum, int HDNum, float Magnitude, double RightAscensionInHours, double DeclinationInDegrees, double DistanceInParsecs, float ColorIndex, string ConstellationAbbreviation, string BayerDesignationAbbreviation, int BayerIndex, int FlamsteedNumber)
        {
            this.ProperName = Name;
            this.HRNum = HRNum;
            this.HDNum = HDNum;
            this.Magnitude = Magnitude;
            this.RightAscensionInHours = RightAscensionInHours;
            this.DeclinationInDegrees = DeclinationInDegrees;

            //if (properNames.ContainsKey(this.HDNum))
            //{
            //    if (this.ProperName != properNames[this.HDNum])
            //    {
            //        System.Diagnostics.Debug.WriteLine(this.ProperName + " -> " + properNames[this.HDNum]);
            //        pncount++;
            //        this.ProperName = properNames[this.HDNum];
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine(this.ProperName + " OK");
            //    }
            //}

#if DEBUG
            if (this.Magnitude > 15f)
                throw new Exception("Star too dim");
            else if (this.Magnitude < -2f)
                throw new Exception("Star too bright");
#endif

            this.StarDesignation = new StarDesignation(ConstellationAbbreviation, BayerDesignationAbbreviation, BayerIndex, FlamsteedNumber);

            this.IsNamed = !string.IsNullOrWhiteSpace(Name);

            string numbers = this.HRNum > 0 ? string.Format("HD{0} HR{1}", this.HDNum, this.HRNum) : string.Format("HD{0}", this.HDNum);

            if (this.StarDesignation.SecondaryDesignation.Length > 0)
                this.Description = string.Format("{0} {1} Mag {2:0.00}", this.StarDesignation.SecondaryDesignation, numbers, this.Magnitude);
            else
                this.Description = string.Format("{0} Mag {1:0.00}", numbers, this.Magnitude);

            if (this.StarDesignation.IsEmpty)
            {
                if (this.IsNamed)
                    this.Name = this.FullName = ProperName;
                else
                    this.Name = this.FullName = string.Format("HD{0}", HDNum);
            }
            else if (this.IsNamed)
            {
                this.Name = this.FullName = string.Format("{0} {1}", this.StarDesignation.ShortDesignation, this.ProperName);
            }
            else
            {
                if (this.StarDesignation.HasBayerOrFlamsteedDesignation)
                    this.Name = this.FullName = this.StarDesignation.ShortDesignation;
                else
                    this.Name = this.FullName = string.Format("HD{0}", this.HDNum);
            }

            if (this.Name.Length == 0)
                throw new Exception();

            this.DisplayName = this.Name;

            this.CaptionZoomThreshold = Math.Max(this.Magnitude + 0.5f, 0f) * 275f;

            this.NumRings = 0;
            this.Axis = Vector.UnitZ;

            this.Position = Util.LocationFromEquatorialCoords(RightAscensionInHours, DeclinationInDegrees, DistanceInParsecs);

            this.ColorIndex = ColorIndex;
            this.Color = Colors.GetColorFromColorIndex(Magnitude, ColorIndex);

            this.Mass = STAR_MASS;
            this.Radius = STAR_RADIUS;
            this.AngularVelocity = 0;
            this.Velocity = new Vector();

            this.RadiusEnhancement = Math.Max(0, (9f - this.Magnitude) / 7);

            this.BodyType = CelestialBodyType.Star;

            this.ShapeSmall = Star.shapeSmall;
            this.ShapeMedium = Star.shapeMedium;
            this.ShapeBig = Star.shapeBig;
            this.HasDynamicShape = false;
            this.HasShape = true;

            this.SortKey = (this.StarDesignation.SortKey + this.Name + this.HDNum.ToString() + this.HRNum.ToString()).ToLower();
            
            this.Snapshot();
        }
        public override IEnumerable<string> SearchNames
        {
            get
            {
                if (IsNamed)
                    yield return this.ProperName;

                if (!StarDesignation.IsEmpty)
                {
                    if (StarDesignation.GreekLetter != GreekLetter.Empty)
                    {
                        yield return string.Format("{0} {1}", StarDesignation.GreekLetter.Abbreviation, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0} {1}", StarDesignation.GreekLetter.Name, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0}{1}", StarDesignation.GreekLetter.Abbreviation, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0}{1}", StarDesignation.GreekLetter.Name, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0} {1}", StarDesignation.GreekLetter.Abbreviation, this.StarDesignation.ConstellationAbbreviation);
                        yield return string.Format("{0} {1}", StarDesignation.GreekLetter.Name, this.StarDesignation.ConstellationAbbreviation);
                        yield return string.Format("{0}{1}", StarDesignation.GreekLetter.Abbreviation, this.StarDesignation.ConstellationAbbreviation);
                        yield return string.Format("{0}{1}", StarDesignation.GreekLetter.Name, this.StarDesignation.ConstellationAbbreviation);
                    }
                    else if (StarDesignation.BayerDesignator.Length > 0)
                    {
                        yield return string.Format("{0} {1}", StarDesignation.BayerDesignator, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0}{1}", StarDesignation.BayerDesignator, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0} {1}", StarDesignation.BayerDesignator, this.StarDesignation.ConstellationAbbreviation);
                        yield return string.Format("{0}{1}", StarDesignation.BayerDesignator, this.StarDesignation.ConstellationAbbreviation);
                    }
                    if (StarDesignation.FlamsteedNumber > 0)
                    {
                        yield return string.Format("{0} {1}", StarDesignation.FlamsteedNumber, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0}{1}", StarDesignation.FlamsteedNumber, this.StarDesignation.ConstellationGenitive);
                        yield return string.Format("{0} {1}", StarDesignation.FlamsteedNumber, this.StarDesignation.ConstellationAbbreviation);
                        yield return string.Format("{0}{1}", StarDesignation.FlamsteedNumber, this.StarDesignation.ConstellationAbbreviation);
                    }
                }
                if (HDNum > 0)
                    yield return string.Format("HD{0}", HDNum.ToString());
                if (HRNum > 0)
                    yield return string.Format("HR{0}", HRNum.ToString());
            }
        }
        public override string ToString()
        {
            return string.Format("Star: {0} ({1}) Mag {2:0.00}", this.Name, this.FullName, this.Magnitude);
        }
        public string Serialize()
        {
            return string.Format("{0},{1},{2},{3:R},{4:R},{5:R},{6:0.00},{7:0.000},{8},{9},{10},{11}",
                                 this.HDNum,
                                 this.HRNum,
                                 this.ProperName,
                                 this.RightAscensionInHours,
                                 this.DeclinationInDegrees,
                                 this.Position.Magnitude / Util.METERS_PER_PARSEC,
                                 this.Magnitude,
                                 this.ColorIndex,
                                 this.StarDesignation.BayerDesignatorOriginal,
                                 this.StarDesignation.BayerIndex,
                                 this.StarDesignation.ConstellationAbbreviation,
                                 this.StarDesignation.FlamsteedNumber);
        }
    }
}
