using System;

namespace SolarMax;

internal class InstrumentAttitude(IRenderer Renderer,
                                  InstrumentDataSource DataSource,
                                  QPen LinePen,
                                  QPen BackPen,
                                  QPen TextPen) : Instrument(Renderer, DataSource, LinePen, BackPen, TextPen)
{
    private QSize smallArrowSize;
    private QPoint textCenter;

    public override QSize Size
    {
        set
        {
            base.Size = value;
            this.smallArrowSize = value / 3;
            this.textCenter = center + new QPoint(0, 20);
        }
    }
    public override void Render()
    {
        var top = dataSource.CameraUp.__GetTranslationRelativeToReferenceAsXAxis(dataSource.CameraView);
        var angle = Math.PI - Math.Atan2(top.Z, top.Y);

        QPoint adjCenter = new(center.X - Math.Cos(angle) * 40,
                               center.Y + Math.Sin(angle) * 40);

        DrawArrow(adjCenter, smallArrowSize, angle);

        angle -= MathEx.HALF_PI;

        var xOffset = Math.Cos(angle) * innerSize.Width;
        var yOffset = -Math.Sin(angle) * innerSize.Height;

        renderer.DrawLine(BackPen, new QPoint(center.X - xOffset, center.Y - yOffset), new QPoint(center.X + xOffset, center.Y + yOffset));

        var angleDegrees = angle.NormalizeAngleNegativePiToPi().ToDegreesFromRadians();

        var angleString = (angleDegrees >= 0.0 ? "L" : "R") + String.Format("{0:000.0}°", Math.Abs(angleDegrees));
        renderer.DrawStringCentered(angleString, textCenter, TextPen, renderer.SmallFont);
    }
}
