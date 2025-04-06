using System.Drawing;

namespace SolarMax;

// WinForms specific version of class to encapsulate pens and brushes
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

    public Color Color { get; private set; }

    public Pen Pen { get; private set; }
    public Brush Brush { get; private set; }
    public QPen(QColor Color) : this(Color.Red, Color.Green, Color.Blue)
    {
    }
    public QPen(int Red, int Green, int Blue) : this((byte)Red, (byte)Green, (byte)Blue)
    {
    }
    public QPen(byte Red, byte Green, byte Blue)
    {
        this.Alpha = 255;
        this.Red = Red;
        this.Green = Green;
        this.Blue = Blue;
        var a = Alpha + 1;
        this.BGRA = (Alpha << 24)
              | ((byte)((Red * a) >> 8) << 16)
              | ((byte)((Green * a) >> 8) << 8)
              | ((byte)((Blue * a) >> 8));
        this.Color = Color.FromArgb(Red, Green, Blue);
        this.Pen = new Pen(this.Color);
        this.Brush = new SolidBrush(this.Color);
    }
    static QPen()
    {
        Black = new QPen(0, 0, 0);
        White = new QPen(255, 255, 255);
        Yellow = new QPen(255, 255, 0);
    }
    ~QPen()
    {
        this.Pen.Dispose();
        this.Brush.Dispose();
    }
    public override string ToString() => $"QPen: {this.Red} {this.Green} {this.Blue}";
}
