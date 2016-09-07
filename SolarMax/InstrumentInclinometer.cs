using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class InstrumentInclinometer : Instrument
    {
        private QRectangle rect;
        private QSize newHalfSize;
        private QPoint centerLeft;

        public InstrumentInclinometer(IRenderer Renderer, InstrumentDataSource DataSource, QPen LinePen, QPen BackPen, QPen TextPen)
            : base(Renderer, DataSource, LinePen, BackPen, TextPen)
        {

        }
        public override QSize Size
        {
            set
            {
                base.Size = value;
                this.rect = this.Bounds.GetRectangleCenteredOn(new QSize(this.Size.Width * 0.7, this.Size.Height));

                rect = new QRectangle(rect.X - rect.Width, center.Y - rect.Height * 9 / 20, rect.Width * 2, rect.Height * 9 / 10);
                
                this.newHalfSize = this.rect.Size / 2;
                this.centerLeft = rect.Center;
            }
        }
        public override void Render()
        {
            renderer.DrawArc(rect, -90, 180, BackPen);

            var inc = dataSource.CameraView.Inclination;

            drawArrow(centerLeft, newHalfSize, inc);

            renderer.DrawStringCentered(String.Format("{0:00.0}°", inclinationToUI(inc)), centerLeft, TextPen, renderer.SmallFont);
        }
    }
}
