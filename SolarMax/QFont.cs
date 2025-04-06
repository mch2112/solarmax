#if WPF
using System.Windows.Media;
#else
using System.Drawing;
#endif

namespace SolarMax;

internal class QFont
{
#if WPF
    public FontFamily FontFamily { get; private set; }
#else
   private readonly Font font;
#endif
   public float FontSize { get; private set; }
   public bool IsItalic { get; private set; }

    public QFont(string Name, float Size)
    {
        this.FontSize = Size;
        this.IsItalic = false;
#if WPF
        this.FontFamily = new FontFamily(Name);
#else
        this.font = new Font(Name, Size, FontStyle.Regular);
#endif
    }
    public QFont(string Name, float Size, bool Italic)
    {
        this.FontSize = Size;
        this.IsItalic = Italic;
#if WPF
        this.FontFamily = new FontFamily(Name);
        
#else
        this.font = new Font(Name, Size, Italic ? FontStyle.Italic : FontStyle.Regular);
#endif
    }
#if WPF
#else
    public Font Font => this.font;
#endif
}
