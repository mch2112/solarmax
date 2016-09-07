using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    public struct QSize
    {
        public static QSize Empty { get; private set; }
        public float Width { get; set; }
        public float Height { get; set; }

#if WPF
        public QSize(System.Windows.Size Size) : this()
        {
            this.Width = (float)Size.Width;
            this.Height = (float)Size.Height;
        }
#endif
        public QSize(float Width, float Height) : this()
        {
            this.Width = Width;
            this.Height = Height;
        }
        public QSize(double Width, double Height) : this()
        {
            this.Width = (float)Width;
            this.Height = (float)Height;
        }
        static QSize()
        {
            Empty = new QSize(0, 0);
        }
        public QRectangle GetRectangleCenteredOn(QSize Size)
        {
            return new QRectangle((this.Width - Size.Width) / 2, (this.Height - Size.Height) / 2, Size.Width, Size.Height);
        }
        public QRectangle GetRectangleDockedBottomRight(QSize Size, float Margin)
        {
            return new QRectangle(this.Width - Size.Width - Margin, this.Height - Size.Height - Margin, Size.Width, Size.Height);
        }
        public static QSize operator +(QSize S1, QSize S2)
        {
            return new QSize(S1.Width + S2.Width, S1.Height + S2.Height);
        }
        public static QSize operator -(QSize S1, QSize S2)
        {
            return new QSize(S1.Width - S2.Width, S1.Height - S2.Height);
        }
        public static QSize operator *(QSize Size, double Factor)
        {
            return new QSize(Size.Width * Factor, Size.Height * Factor);
        }
        public static QSize operator /(QSize Size, double Factor)
        {
            return new QSize(Size.Width / Factor, Size.Height / Factor);
        }
        public override bool Equals(System.Object Obj)
        {
            if (!(Obj is QSize))
                return false;

            return base.Equals(Obj);
        }
        public bool Equals(QSize S)
        {
            return this == S;
        }
        public override int GetHashCode()
        {
            return this.Width.GetHashCode() ^ this.Height.GetHashCode();
        }
        public static bool operator ==(QSize A, QSize B)
        {
            if (System.Object.ReferenceEquals(A, B))
                return true;

            if (((object)A == null) || ((object)B == null))
                return false;

            // Return true if the fields match:
            return A.Width == B.Width && A.Height == B.Height;
        }

        public static bool operator !=(QSize A, QSize B)
        {
            return !(A == B);
        }
        public override string ToString()
        {
            return string.Format("{0:0.00} x {1:0.00}", this.Width, this.Height);
        }
    }
}
