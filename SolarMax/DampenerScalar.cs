using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class DampenerScalar : Dampener<double>
    {
        private bool uselogrithmicIncrease = false;
        public DampenerScalar(double InitialValue, double TrackFactor)
            : base(InitialValue, TrackFactor)
        {
        }
        public override void SetTarget(double Value)
        {
            base.SetTarget(Value);
            uselogrithmicIncrease = Actual > MathEx.EPSILON && Target / Actual > 10;
        }
        protected override void Track(double TrackFactor)
        {
            if (Target > Actual && uselogrithmicIncrease)
            {
                var adj = Math.Exp(Math.Log(Target - Actual) * TrackFactor);
                adj = Math.Max(Actual * TrackFactor / 2.0, adj);
                if (adj > Target - Actual)
                    Actual = Target;
                else
                    Actual += adj;
            }
            else
            {
                Actual += (Target - Actual) * TrackFactor;
            }
        }
        protected override double differenceAbsoluteValue()
        {
            return Math.Abs(Actual - Target);
        }
    }
}
