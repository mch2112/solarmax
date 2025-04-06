namespace SolarMax.Instruments;

internal class FieldOfViewInstrument(IRenderer Renderer,
                                     DataSourceInstrument DataSource,
                                     QPen LinePen,
                                     QPen BackPen,
                                     QPen TextPen) : Instrument(Renderer, DataSource, LinePen, BackPen, TextPen)
{
    private QRectangle arcRect;
    private QPoint drawCenter;
    private QPoint textPoint;

    public override QSize Size
    {
        set
        {
            base.Size = value;
            SetupGeometry();
        }
    }

    private void SetupGeometry()
    {
        drawCenter = new QPoint(center.X, Location.Y + Size.Height * 0.7);
        arcRect = QRectangle.GetRectangleCenteredOn(drawCenter, innerSize);
        textPoint = new QPoint(center.X, Location.Y + Size.Height * 0.8);
    }
    public override QPoint Location
    {
        set
        {
            base.Location = value;
            SetupGeometry();
        }
    }
    public override void Render()
    {
        var field = dataSource.ScreenSize.Width / dataSource.CameraZoom;

        DrawArrow(drawCenter, innerSize, field / 2 + MathEx.HALF_PI);
        DrawArrow(drawCenter, innerSize, field / -2 + MathEx.HALF_PI);

        renderer.DrawArc(arcRect, -90 - field.ToDegreesFromRadians() / 2, field.ToDegreesFromRadians(), BackPen);
        renderer.DrawStringCentered(string.Format(field > 0.1 ? "{0:000.0}°" : "{0:000.0000}°", field.ToDegreesFromRadians()), textPoint, TextPen, renderer.SmallFont);
    }
}
