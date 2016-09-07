using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class DampenerDirection : Dampener<Vector>
    {
        public DampenerDirection(Vector InitialValue, double TrackFactor)
            : base(InitialValue, TrackFactor)
        {
            base.SetTarget(InitialValue);
        }
        protected override void Track(double TrackFactor)
        {
            Actual = Actual.Slerp(Target, TrackFactor);
        }
        protected override double differenceAbsoluteValue()
        {
            return Actual.AngleDiffAbs(Target);
        }
    }
}
