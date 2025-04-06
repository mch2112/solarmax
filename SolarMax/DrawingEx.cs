using System.Drawing;

namespace SolarMax;
internal static class DrawingEx
{
    public static bool IsContainedIn(this Rectangle R, Rectangle Other)
    {
        return Other.X <= R.X &&
               Other.Y <= R.Y &&
               Other.Right >= R.Right &&
               Other.Bottom >= R.Bottom;
    }
}
