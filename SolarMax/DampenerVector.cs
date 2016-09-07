using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class DampenerVector : Dampener<Vector>
    {
        public DampenerVector(Vector InitialValue, double TrackFactor)
            : base(InitialValue, TrackFactor)
        {
            target = new Vector();
            base.SetTarget(InitialValue);
        }
        public override double AutoLockThreshold
        {
            set
            {
                base.AutoLockThreshold = value * value;
            }
        }
        public override double NearLockThreshold
        {
            set
            {
                base.NearLockThreshold = value * value;
            }
        }
        protected override void Track(double TrackFactor)
        {
            this.Actual += (Target - Actual) * TrackFactor;
        }
        protected override double differenceAbsoluteValue()
        {
            return this.Target.DistanceToSquared(this.Actual);
        }
    }
}
