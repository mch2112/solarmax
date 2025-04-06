using System;
using System.Drawing;

namespace SolarMax;

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
    public readonly bool IsZero => this.X == 0 && this.Y == 0;
    public void Overwrite(QPoint P)
    {
#if DEBUG
        if (base.Equals(QPoint.Empty))
            throw new Exception();
#endif
        this.X = P.X;
        this.Y = P.Y;
    }
    public readonly float DistanceTo(QPoint Other)
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
        => new(P1.X + P2.X, P1.Y + P2.Y);
    public static QPoint operator -(QPoint P1, QPoint P2) 
        => new(P1.X - P2.X, P1.Y - P2.Y);
    public static QPoint operator +(QPoint Point, QSize Size) 
        => new(Point.X + Size.Width, Point.Y + Size.Height);
    public static QPoint operator -(QPoint Point, QSize Size)
        => new(Point.X - Size.Width, Point.Y - Size.Height);
    public override readonly bool Equals(object Obj)
    {
        if (Obj is not QPoint qp)
            return false;

        return this == qp;
    }
    public readonly bool Equals(QPoint P)
    {
        return this == P;
    }
    public override readonly int GetHashCode()
    {
        return this.X.GetHashCode() ^ this.Y.GetHashCode();
    }
    public static bool operator ==(QPoint A, QPoint B)
        => A.X == B.X && A.Y == B.Y;

    public static bool operator !=(QPoint A, QPoint B)
    {
        return !(A == B);
    }
    public override readonly string ToString() => $"{this.X:0.00} x {this.Y:0.00}";
}
