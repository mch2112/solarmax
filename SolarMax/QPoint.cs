using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SolarMax
{
    public struct QPoint
    {
        public static QPoint Empty { get; private set; }

        public float X { get; set; }
        public float Y { get; set; }

        static QPoint()
        {
            Empty = new QPoint(0f, 0f);
        }
        public QPoint(float X, float Y) : this()
        {
            this.X = X;
            this.Y = Y;
        }
        public QPoint(PointF Point) : this()
        {
            this.X = Point.X;
            this.Y = Point.Y;
        }
        public QPoint(double X, double Y) : this((float)X, (float)Y)
        {
        }
        public bool IsZero { get { return this.X == 0 && this.Y == 0; } }
        public void Overwrite(QPoint P)
        {
#if DEBUG
            if (base.Equals(QPoint.Empty))
                throw new Exception();
#endif
            this.X = P.X;
            this.Y = P.Y;
        }
        public float DistanceTo(QPoint Other)
        {
            var x = this.X - Other.X;
            var y = this.Y - Other.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }
        public void Overwrite(double X, double Y)
        {
            this.X = (float)X;
            this.Y = (float)Y;
        }
        public void Overwrite(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public void Add(float X, float Y)
        {
            this.X += X;
            this.Y += Y;
        }
        public void Add(double X, double Y)
        {
            this.X += (float)X;
            this.Y += (float)Y;
        }
        public void Add(QSize Size)
        {
            this.X += Size.Width;
            this.Y += Size.Height;
        }
        public void Multiply(double Factor)
        {
            this.X *= (float)Factor;
            this.Y *= (float)Factor;
        }
        public void Multiply(float Factor)
        {
            this.X *= Factor;
            this.Y *= Factor;
        }
        public static QPoint operator +(QPoint P1, QPoint P2)
        {
            return new QPoint(P1.X + P2.X, P1.Y + P2.Y);
        }
        public static QPoint operator -(QPoint P1, QPoint P2)
        {
            return new QPoint(P1.X - P2.X, P1.Y - P2.Y);
        }
        public static QPoint operator +(QPoint Point, QSize Size)
        {
            return new QPoint(Point.X + Size.Width, Point.Y + Size.Height);
        }
        public static QPoint operator -(QPoint Point, QSize Size)
        {
            return new QPoint(Point.X - Size.Width, Point.Y - Size.Height);
        }
        public override bool Equals(System.Object Obj)
        {
            if (!(Obj is QPoint))
                return false;

            return base.Equals(Obj);
        }
        public bool Equals(QPoint P)
        {
            return this == P;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
        public static bool operator ==(QPoint A, QPoint B)
        {
            if (System.Object.ReferenceEquals(A, B))
                return true;

            if (((object)A == null) || ((object)B == null))
                return false;

            // Return true if the fields match:
            return A.X == B.X && A.Y == B.Y;
        }

        public static bool operator !=(QPoint A, QPoint B)
        {
            return !(A == B);
        }
        public override string ToString()
        {
            return string.Format("{0:0.00} x {1:0.00}", this.X, this.Y);
        }
    }
}
