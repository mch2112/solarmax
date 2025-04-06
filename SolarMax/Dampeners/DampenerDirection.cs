namespace SolarMax.Dampeners;

internal sealed class DampenerDirection : Dampener<Vector>
{
    public DampenerDirection(Vector InitialValue, double TrackFactor)
        : base(InitialValue, TrackFactor)
    {
        SetTarget(InitialValue); /* needed? */
    }
    protected override void Track(double TrackFactor)
    {
        Actual = Actual.Slerp(Target, TrackFactor);
    }
    protected override double DifferenceAbsoluteValue()
        => Actual.AngleDiffAbs(Target);
}
