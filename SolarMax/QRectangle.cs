using System.Drawing;

namespace SolarMax;

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
    public readonly float X => Location.X;
    public readonly float Y => Location.Y;
    public readonly float Width => Size.Width;
    public readonly float Height => Size.Height;

    public static QRectangle GetRectangleCenteredOn(QPoint Center, QSize Size)
        => new(Center.X - Size.Width / 2, Center.Y - Size.Height / 2, Size.Width, Size.Height);
    public static QRectangle operator +(QRectangle Rect, QPoint Point)
        => new(Rect.Location + Point, Rect.Size);
    public static QRectangle operator -(QRectangle Rect, QPoint Point)
        => new(Rect.Location - Point, Rect.Size);
    public readonly RectangleF ToRectangleF() 
        => new(Location.X, Location.Y, Size.Width, Size.Height);
    public readonly Rectangle ToRectangle()
        => new((int)Location.X, (int)Location.Y, (int)Size.Width, (int)Size.Height);
    public readonly float Left => this.X;
    public readonly float Right => this.X + this.Width;
    public readonly float Top => this.Y;
    public readonly float Bottom => this.Y + this.Height;
    public readonly QPoint Center => new(this.X + this.Width / 2, this.Y + this.Height / 2);
    public readonly QPoint TopLeft => this.Location;
    public readonly QPoint TopRight => new(this.X + this.Width, this.Y);
    public readonly QPoint BottomLeft => new(this.X, this.Y + this.Height);
    public readonly QPoint BottomRight
        => new(this.X + this.Width, this.Y + this.Height);
    public readonly QRectangle GetRectangleCenteredOn(QSize Size)
        => new(new QPoint(this.X + (this.Size.Width - Size.Width) / 2,
                          this.Y + (this.Size.Height - Size.Height) / 2),
               Size);
    public override readonly bool Equals(object Obj)
    {
        return Obj is QRectangle q && this == q;
    }
    public readonly bool Equals(QRectangle R)
    {
        return this == R;
    }
    public override readonly int GetHashCode()
    {
        return this.X.GetHashCode() ^ this.Y.GetHashCode();
    }
    public static bool operator ==(QRectangle A, QRectangle B)
    {
        return A.X == B.X &&
               A.Y == B.Y &&
               A.Width == B.Width &&
               A.Height == B.Height;
    }

    public static bool operator !=(QRectangle A, QRectangle B)
    {
        return !(A == B);
    }
}
