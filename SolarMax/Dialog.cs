using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal abstract class Dialog : Widget
    {
        public delegate void CloseCallback();

        protected Dialog.CloseCallback closeCallback;

        protected IRenderer renderer;
        protected QRectangle rect;
        protected QSize screenSize;
        protected QPen forePen;
        protected QPen borderPen;
        protected QPen backPen;

        protected QPoint titleLoc;
        
        protected Dialog(IRenderer Renderer, QSize ScreenSize, CloseCallback CloseCallback, QPen ForePen, QPen BorderPen, QPen BackPen)
        {
            this.renderer = Renderer;
            this.screenSize = ScreenSize;
            this.closeCallback = CloseCallback;
            this.forePen = ForePen;
            this.borderPen = BorderPen;
            this.backPen = BackPen;

            SetupLayout();
        }
        public void Render(QSize ScreenSize)
        {
            if (ScreenSize != this.screenSize)
            {
                this.screenSize = ScreenSize;
                this.SetupLayout();
            }
            this.Render();
        }
        public abstract bool SendCommand(QCommand Key);
        public abstract string Message { get; }

        public virtual void Close()
        {
            closeCallback();
        }

        protected abstract void SetupLayout();
    }
}
