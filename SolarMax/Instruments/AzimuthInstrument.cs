using System;
using System.Linq;

namespace SolarMax.Instruments;

internal class AzimuthInstrument(IRenderer Renderer,
                                 DataSourceInstrument DataSource,
                                 QPen LinePen,
                                 QPen BackPen,
                                 QPen TextPen) : Instrument(Renderer, DataSource, LinePen, BackPen, TextPen)
{
    public override void Render()
    {
        renderer.DrawLine(BackPen, center.X, center.Y - innerSize.Height, center.X, center.Y + innerSize.Height);
        renderer.DrawLine(BackPen, center.X - innerSize.Width, center.Y, center.X + innerSize.Width, center.Y);

        foreach (var b in dataSource.Physics.AllOrbiters.Where(o => o.Mass > 3E+26))
        {
            if (!dataSource.Camera.BodyWithCamera.Equals(b) || dataSource.Camera.BodyBeingViewed.Equals(b) || dataSource.Camera.BodyWithCamera.Equals(b) && !dataSource.Camera.PositionLocked)
            {
                var azimuth = (b.PositionSnapshot - dataSource.CameraPosition).Azimuth;
                var x = center.X - Math.Sin(azimuth) * innerSize.Width;
                var y = center.Y - Math.Cos(azimuth) * innerSize.Height;
                renderer.DrawStringCentered(b.Name, new QPoint(x, y), b.CaptionPen, renderer.SmallFont);
            }
        }

        var heading = dataSource.CameraView.Azimuth;

        DrawArrow(center, innerSize, heading + MathEx.HALF_PI);

        renderer.DrawStringCentered($"{HeadingToUI(heading):000.0}°",
                                    new QPoint(center.X - innerSize.Width * 0.8 * Math.Sin(heading), center.Y - innerSize.Height * 0.8 * Math.Cos(heading)),
                                    TextPen,
                                    renderer.SmallFont);
    }
}
