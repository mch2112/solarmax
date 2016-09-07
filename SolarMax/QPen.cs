using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WPF
using System.Windows.Media;
#else
using System.Drawing;
#endif

namespace SolarMax
{
    // Winforms specific version of class to encapsulate pens and brushes
    public sealed class QPen
    {
        public static QPen Black { get; private set; }
        public static QPen White { get; private set; }
        public static QPen Yellow { get; private set; }

        public byte Alpha { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
        public int BGRA { get; private set; }

        public System.Drawing.Color Color { get; private set; }

#if WPF
        public Color Pen { get; private set; }
        public SolidColorBrush Brush { get; private set; }
#else
        public Pen Pen { get; private set; }
        public Brush Brush { get; private set; }
#endif
        public QPen(QColor Color) : this(Color.Red, Color.Green, Color.Blue)
        {
         
        }
        public QPen(int Red, int Green, int Blue) : this((byte)Red, (byte)Green, (byte)Blue)
        {

        }
        public QPen(byte Red, byte Green, byte Blue)
        {
            this.Alpha = (byte)255;
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
            var a = Alpha + 1;
            this.BGRA = (Alpha << 24)
                  | ((byte)((Red * a) >> 8) << 16)
                  | ((byte)((Green * a) >> 8) << 8)
                  | ((byte)((Blue * a) >> 8));
#if WPF
            this.Brush = new SolidColorBrush(Color.FromArgb(Alpha, Red, Green, Blue));
            this.Pen = this.Brush.Color;
            
#else
            this.Color = System.Drawing.Color.FromArgb(Red, Green, Blue);
            this.Pen = new Pen(this.Color);
            this.Brush = new SolidBrush(this.Color);
#endif
        }
        static QPen()
        {
            Black = new QPen(0, 0, 0);
            White = new QPen(255, 255, 255);
            Yellow = new QPen(255, 255, 0);
        }
        ~QPen()
        {
#if WPF
#else
            this.Pen.Dispose();
            this.Brush.Dispose();
#endif
        }
        public override string ToString()
        {
            return string.Format("QPen: {0} {1} {2}", this.Red, this.Green, this.Blue);
        }
    }
}
