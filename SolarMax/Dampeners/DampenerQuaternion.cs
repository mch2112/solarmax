using System;

namespace SolarMax.Dampeners;

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
        Actual = Actual.SlerpTo(Target, TrackFactor);
    }
    protected override double DifferenceAbsoluteValue()
    {
        return Math.Abs(Math.Acos(Actual.DotProduct(Target)));
    }
}
