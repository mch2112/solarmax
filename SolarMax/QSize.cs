namespace SolarMax;

public struct QSize
{
    public static QSize Empty { get; private set; } = new QSize(0, 0);
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
    public readonly QRectangle GetRectangleCenteredOn(QSize Size)
        => new((this.Width - Size.Width) / 2,
               (this.Height - Size.Height) / 2,
               Size.Width,
               Size.Height);
    public readonly QRectangle GetRectangleDockedBottomRight(QSize Size, float Margin)
        => new(this.Width - Size.Width - Margin,
               this.Height - Size.Height - Margin,
               Size.Width,
               Size.Height);
    public static QSize operator +(QSize S1, QSize S2) => new(S1.Width + S2.Width, S1.Height + S2.Height);
    public static QSize operator -(QSize S1, QSize S2) => new(S1.Width - S2.Width, S1.Height - S2.Height);
    public static QSize operator *(QSize Size, double Factor) => new(Size.Width * Factor, Size.Height * Factor);
    public static QSize operator /(QSize Size, double Factor) => new(Size.Width / Factor, Size.Height / Factor);
    public override readonly bool Equals(object Obj)
    {
        if (Obj is not QSize qs)
            return false;
        return this == qs;
    }
    public readonly bool Equals(QSize S) => this == S;
    public override readonly int GetHashCode()
        => this.Width.GetHashCode() ^ this.Height.GetHashCode();
    public static bool operator ==(QSize A, QSize B)
        => A.Width == B.Width && A.Height == B.Height;

    public static bool operator !=(QSize A, QSize B) => !(A == B);
    public override readonly string ToString()
        => $"{this.Width:0.00} x {this.Height:0.00}";
}
