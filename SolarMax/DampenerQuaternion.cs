using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
	internal class DampenerQuaternion : Dampener<Quaternion>
	{
        public DampenerQuaternion(Quaternion InitialValue, double TrackFactor)
            : base(InitialValue, TrackFactor)
        {
            target = InitialValue;
            base.SetTarget(InitialValue);
        }
        protected override void Track(double TrackFactor)
        {
            this.Actual = Actual.SlerpTo(this.Target, TrackFactor);
        }
        protected override double differenceAbsoluteValue()
        {
            return Math.Abs(Math.Acos(Actual.DotProduct(Target)));
        }
	}
}
