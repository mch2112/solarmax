using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal abstract class Instrument : Widget
    {
        public QPen LinePen { get; set; }
        public QPen BackPen { get; set; }
        public QPen TextPen { get; set; }
        
        private QSize size;
        protected QSize halfSize;
        protected QSize innerSize;
        protected QSize sizeVSmall;
        protected QPoint center;

        private QPoint location;

        protected IRenderer renderer;

        protected InstrumentDataSource dataSource;

        protected Instrument(IRenderer Renderer, InstrumentDataSource DataSource, QPen LinePen, QPen BackPen, QPen TextPen)
        {
            this.renderer = Renderer;
            this.dataSource = DataSource;
            this.LinePen = LinePen;
            this.BackPen = BackPen;
            this.TextPen = TextPen;
        }
        
        public virtual QSize Size
        {
            get { return size; }
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
            get { return location; }
            set
            {
                location = value;
                center = value + halfSize;
            }
        }
        public QRectangle Bounds
        {
            get { return new QRectangle(this.Location, this.Size); }
            set
            {
                this.Location = value.Location;
                this.Size = value.Size;
            }
        }

        protected void drawArrow(QPoint Location, QSize Size, double Angle)
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
}
