using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class DampenerAngle : Dampener<double>
    {
        public DampenerAngle(double InitialValue, double TrackFactor) : base(InitialValue, TrackFactor)
        {
        }
        protected override void Track(double TrackFactor)
        {
            Actual = (Actual + Target.AngleDifferenceBetweenNegPiToPi(Actual) * TrackFactor).NormalizeAngleNegativePiToPi();
        }
        public override void SetTarget(double Value)
        {
            base.SetTarget(Value.NormalizeAngleNegativePiToPi());
        }
        protected override double differenceAbsoluteValue()
        {
            return Math.Abs(Actual.AngleDifferenceBetweenNegPiToPi(Target));
        }
    }
}
