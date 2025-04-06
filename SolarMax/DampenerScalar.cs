using System;

namespace SolarMax;

internal sealed class DampenerScalar(double InitialValue, double TrackFactor) : Dampener<double>(InitialValue, TrackFactor)
{
    private bool useLogarithmicIncrease = false;

    public override void SetTarget(double Value)
    {
        base.SetTarget(Value);
        useLogarithmicIncrease = Actual > MathEx.EPSILON && Target / Actual > 10;
    }
    protected override void Track(double TrackFactor)
    {
        if (Target > Actual && useLogarithmicIncrease)
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
    protected override double differenceAbsoluteValue() => Math.Abs(Actual - Target);
}
