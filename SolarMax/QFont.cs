using System.Drawing;

namespace SolarMax;

internal class QFont
{
   private readonly Font font;
   public float FontSize { get; private set; }
   public bool IsItalic { get; private set; }

    public QFont(string Name, float Size)
    {
        this.FontSize = Size;
        this.IsItalic = false;
        this.font = new Font(Name, Size, FontStyle.Regular);
    }
    public QFont(string Name, float Size, bool Italic)
    {
        this.FontSize = Size;
        this.IsItalic = Italic;
        this.font = new Font(Name, Size, Italic ? FontStyle.Italic : FontStyle.Regular);
    }
    public Font Font => this.font;
}
