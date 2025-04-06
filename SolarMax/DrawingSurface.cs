using System.Windows.Forms;
using SolarMax.Controllers;

namespace SolarMax;

internal class DrawingSurface : Control
{
    public DrawingSurface(ScreenSaverMode ScreenSaverMode)
    {
        this.DoubleBuffered = true;
        switch (ScreenSaverMode)
        {
            case ScreenSaverMode.ScreenSaverPreview:
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
                break;
            default:
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
                break;
        }
    }
}
