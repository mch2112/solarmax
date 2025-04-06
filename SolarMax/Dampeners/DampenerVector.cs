namespace SolarMax.Dampeners;

internal sealed class DampenerVector : Dampener<Vector>
{
    public DampenerVector(Vector InitialValue, double TrackFactor)
        : base(InitialValue, TrackFactor)
    {
        target = new Vector();
        SetTarget(InitialValue);
    }
    public override double AutoLockThreshold
    {
        set => base.AutoLockThreshold = value * value;
    }
    public override double NearLockThreshold
    {
        set => base.NearLockThreshold = value * value;
    }
    protected override void Track(double TrackFactor)
    {
        Actual += (Target - Actual) * TrackFactor;
    }
    protected override double DifferenceAbsoluteValue() 
        => Target.DistanceToSquared(Actual);
}
