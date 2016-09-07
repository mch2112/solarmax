using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SolarMax
{
    public struct QRectangle
    {
        public QPoint Location { get; set; }
        public QSize Size { get; set; }

        public QRectangle(QPoint Location, QSize Size) : this(Location.X, Location.Y, Size.Width, Size.Height)
        {
        }
        public QRectangle(float X, float Y, float Width, float Height) : this()
        {
            this.Location = new QPoint(X, Y);
            this.Size = new QSize(Width, Height);
        }
        public float X { get { return Location.X; } }
        public float Y { get { return Location.Y; } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }

        public static QRectangle GetRectangleCenteredOn(QPoint Center, QSize Size)
        {
            return new QRectangle(Center.X - Size.Width / 2, Center.Y - Size.Height / 2, Size.Width, Size.Height);
        }
        public static QRectangle operator +(QRectangle Rect, QPoint Point)
        {
            return new QRectangle(Rect.Location + Point, Rect.Size);
        }
        public static QRectangle operator -(QRectangle Rect, QPoint Point)
        {
            return new QRectangle(Rect.Location - Point, Rect.Size);
        }
        public RectangleF ToRectangleF()
        {
            return new RectangleF(Location.X, Location.Y, Size.Width, Size.Height);
        }
        public Rectangle ToRectangle()
        {
            return new Rectangle((int)Location.X, (int)Location.Y, (int)Size.Width, (int)Size.Height);
        }
        public float Left { get { return this.X; } }
        public float Right { get { return this.X + this.Width; } }
        public float Top { get { return this.Y; } }
        public float Bottom { get { return this.Y + this.Height; } }
        public QPoint Center
        {
            get { return new QPoint(this.X + this.Width / 2, this.Y + this.Height / 2); }
        }
        public QPoint TopLeft
        {
            get { return this.Location; }
        }
        public QPoint TopRight
        {
            get { return new QPoint(this.X + this.Width, this.Y); }
        }
        public QPoint BottomLeft
        {
            get { return new QPoint(this.X, this.Y + this.Height); }
        }
        public QPoint BottomRight
        {
            get { return new QPoint(this.X + this.Width, this.Y + this.Height); }
        }
        public QRectangle GetRectangleCenteredOn(QSize Size)
        {
            return new QRectangle(new QPoint(this.X + (this.Size.Width - Size.Width) / 2,
                                             this.Y + (this.Size.Height - Size.Height) / 2),
                                  Size);
        }
        public override bool Equals(System.Object Obj)
        {
            if (!(Obj is QPoint))
                return false;

            return base.Equals(Obj);
        }
        public bool Equals(QRectangle R)
        {
            return this == R;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
        public static bool operator ==(QRectangle A, QRectangle B)
        {
            if (System.Object.ReferenceEquals(A, B))
                return true;

            if (((object)A == null) || ((object)B == null))
                return false;

            // Return true if the fields match:
            return A.X == B.X && A.Y == B.Y && A.Width == B.Width && A.Height == B.Height;
        }

        public static bool operator !=(QRectangle A, QRectangle B)
        {
            return !(A == B);
        }
    }
}
