namespace SolarMax;

internal abstract class Widget
{
    protected static float HeadingToUI(double Input)
        => (float)((360.0 - Input.ToDegreesFromRadians()) % 360.0);
    protected static float InclinationToUI(double Input)
        => (float)(Input.ToDegreesFromRadians() % 360.0);
    protected static string RangeForUI(double Input)
    {
        return (Input > 0.5 * Util.METERS_PER_LIGHT_YEAR) ? $"{Input / Util.METERS_PER_LIGHT_YEAR:0.0} LY" :
               (Input > 1.5 * Util.METERS_PER_AU) ? $"{Input / Util.METERS_PER_AU:0.0} AU" :
               $"{Input / 1000:0,000} km";
    }
    public abstract void Render();
}
