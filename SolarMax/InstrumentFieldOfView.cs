using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class InstrumentFieldOfView : Instrument
    {
        private QRectangle arcRect;
        private QPoint drawCenter;
        private QPoint textPoint;

        public InstrumentFieldOfView(IRenderer Renderer, InstrumentDataSource DataSource, QPen LinePen, QPen BackPen, QPen TextPen)
            : base(Renderer, DataSource, LinePen, BackPen, TextPen)
        {
        }
        public override QSize Size
        {
            set
            {
                base.Size = value;
                setupGeometry();
            }
        }

        private void setupGeometry()
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
                setupGeometry();
            }
        }
        public override void Render()
        {
            var field = dataSource.ScreenSize.Width / dataSource.CameraZoom;

            drawArrow(drawCenter, innerSize, field / 2 + MathEx.HALF_PI);
            drawArrow(drawCenter, innerSize, field / -2 + MathEx.HALF_PI);

            renderer.DrawArc(arcRect, -90 - field.ToDegreesFromRadians() / 2, field.ToDegreesFromRadians(), BackPen);

            renderer.DrawStringCentered(String.Format(field > 0.1 ? "{0:000.0}°" : "{0:000.0000}°", field.ToDegreesFromRadians()), textPoint, TextPen, renderer.SmallFont);
        }
    }
}
