using System;

namespace SolarMax.Dampeners;

internal sealed class DampenerAngle(double InitialValue, double TrackFactor) : Dampener<double>(InitialValue, TrackFactor)
{
    protected override void Track(double TrackFactor)
    {
        Actual = (Actual + Target.AngleDifferenceBetweenNegPiToPi(Actual) * TrackFactor).NormalizeAngleNegativePiToPi();
    }
    public override void SetTarget(double Value)
    {
        base.SetTarget(Value.NormalizeAngleNegativePiToPi());
    }
    protected override double DifferenceAbsoluteValue() 
        => Math.Abs(Actual.AngleDifferenceBetweenNegPiToPi(Target));
}
