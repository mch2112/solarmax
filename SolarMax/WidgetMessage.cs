using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class WidgetMessage
    {
        private IRenderer renderer;

        private int framesToShow;
        private string message;
        
        private int messageCountdown;

        private QPen fillPen = QPen.Black;
        private QPen forePen;
        private QPen borderPen;

        private QSize screenSize;

        private QRectangle rect;
        private QPoint textLocation;
        private QSize messageSize;

        private bool locked = false;

        public WidgetMessage(IRenderer Renderer, int FramesToShow, QPen ForePen, QPen BorderPen)
        {
            this.renderer = Renderer;
            this.framesToShow = FramesToShow;
            this.forePen = ForePen;
            this.borderPen = BorderPen;
        }
        public void Lock()
        {
            locked = true;
        }
        public void Unlock(bool ClearMessage)
        {
            locked = false;
            if (ClearMessage)
            {
                messageCountdown = 0;
            }
        }
        public void SetMessage(string Message)
        {
            if (!locked)
            {
                message = Message;
                messageSize = QSize.Empty;
                messageCountdown = framesToShow;
            }
        }
        public void SetMessage(string Message, bool Lock)
        {
            message = Message;
            messageSize = QSize.Empty;
            messageCountdown = framesToShow;
            locked = Lock;
        }
        public void Render()
        {
            if (locked || --messageCountdown > 0)
            {
                if (messageSize == QSize.Empty)
                {
                    messageSize = renderer.MeasureText(message, renderer.ExtraLargeFont);
                    this.screenSize = QSize.Empty;
                }
                if (renderer.ScreenSize != this.screenSize)
                {
                    rect = new QRectangle((renderer.ScreenSize.Width - messageSize.Width) / 2, renderer.ScreenSize.Height - 100, messageSize.Width, messageSize.Height);
                    this.screenSize = renderer.ScreenSize;
                    textLocation = new QPoint(rect.X + rect.Width / 40, rect.Y + 1);
                }
                renderer.FillRectangle(rect, fillPen, borderPen);
                renderer.DrawString(message, textLocation, forePen, renderer.ExtraLargeFont);
            }
        }
    }
}
