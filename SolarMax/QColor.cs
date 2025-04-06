using System;

namespace SolarMax;

// Class used to make color definitions generic
public sealed class QColor
{
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }

    public static QColor Black { get; private set; }

    public QColor(int Red, int Green, int Blue)
    {
        this.Red = Red;
        this.Green = Green;
        this.Blue = Blue;
    }
    public QColor(float Red, float Green, float Blue)
    {
        this.Red = (int)Red;
        this.Green = (int)Green;
        this.Blue = (int)Blue;
    }
    static QColor()
    {
        QColor.Black = new QColor(0, 0, 0);
    }
    public QColor Brighten()
    {
        return new QColor(255 - (255 - this.Red) * 2 / 3,
                           255 - (255 - this.Green) * 2 / 3,
                           255 - (255 - this.Blue) * 2 / 3);
    }
    public QColor Brighten(int Amount)
    {
        return new QColor(Math.Min(255, this.Red + Amount),
                          Math.Min(255, this.Green + Amount),
                          Math.Min(255, this.Blue + Amount));
    }
    public QColor Darken()
    {
        return new QColor(this.Red / 2,
                          this.Green / 2,
                          this.Blue / 2);
    }
    public override string ToString() => $"R{this.Red} G{this.Green} B{this.Blue}";
}
