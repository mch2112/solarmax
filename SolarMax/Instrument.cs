using System;
using SolarMax.Instruments;

namespace SolarMax;

internal abstract class Instrument(IRenderer Renderer, DataSourceInstrument DataSource, QPen LinePen, QPen BackPen, QPen TextPen) 
    : Widget
{
    public QPen LinePen { get; set; } = LinePen;
    public QPen BackPen { get; set; } = BackPen;
    public QPen TextPen { get; set; } = TextPen;

    private QSize size;
    protected QSize halfSize;
    protected QSize innerSize;
    protected QSize sizeVSmall;
    protected QPoint center;

    private QPoint location;

    protected IRenderer renderer = Renderer;

    protected DataSourceInstrument dataSource = DataSource;

    public virtual QSize Size
    {
        get => size;
        set
        {
            size = value;
            halfSize = value / 2;
            innerSize = value * 2 / 5;
            sizeVSmall = value / 24;
            center = Location + halfSize;
        }
    }
    public virtual QPoint Location
    {
        get => location;
        set
        {
            location = value;
            center = value + halfSize;
        }
    }
    public QRectangle Bounds
    {
        get => new(this.Location, this.Size);
        set
        {
            this.Location = value.Location;
            this.Size = value.Size;
        }
    }

    protected void DrawArrow(QPoint Location, QSize Size, double Angle)
    {
        const double arrowAngle = 0.8;

        var tip  = new QPoint(Location.X + Size.Width * Math.Cos(Angle),
                              Location.Y - Size.Height * Math.Sin(Angle));

        renderer.DrawLine(LinePen, tip, Location);

        var leftFin = new QPoint(tip.X + sizeVSmall.Width * Math.Cos(Angle + Math.PI * arrowAngle),
                                 tip.Y - sizeVSmall.Height * Math.Sin(Angle + Math.PI * arrowAngle));

        renderer.DrawLine(LinePen, tip, leftFin);

        var rightFin = new QPoint(tip.X + sizeVSmall.Width * Math.Cos(Angle - Math.PI * arrowAngle),
                             tip.Y - sizeVSmall.Height * Math.Sin(Angle - Math.PI * arrowAngle));

        renderer.DrawLine(LinePen, tip, rightFin);
    }
}
