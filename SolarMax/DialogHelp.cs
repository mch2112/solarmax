using System;
using System.Collections.Generic;

namespace SolarMax;

internal class DialogHelp(IRenderer Renderer,
                          QSize ScreenSize,
                          Dialog.CloseCallback CloseCallback,
                          QPen ForePen,
                          QPen BorderPen,
                          QPen BackPen) : Dialog(Renderer, ScreenSize, CloseCallback, ForePen, BorderPen, BackPen)
{
    private List<Tuple<string, QPoint, QPen, QFont>> renderList = null;

    public override bool SendCommand(QCommand Command)
    {
        switch (Command.CommandCode)
        {
            case CommandCode.Enter:
            case CommandCode.Escape:
            case CommandCode.F1:
                closeCallback();
                return true;
            default:
                return false;
        }
    }
    public override string Message => string.Empty;
    public override void Render()
    {
        renderer.FillRectangle(rect, backPen, borderPen);

        foreach (var rl in renderList)
            renderer.DrawString(rl.Item1, rl.Item2, rl.Item3, rl.Item4);
    }
    protected override void SetupLayout()
    {
        QSize size = new(800, 474);
        rect = new QRectangle((renderer.ScreenSize.Width - size.Width) / 2, 20, size.Width, size.Height);
            
        renderList = [];

        float margin = 14;
        float lineSpace = 20;
        float largeLineSpace = lineSpace + 10;
        float firstLine = rect.Y + margin;

        QPoint pt = new(rect.X + margin, firstLine);

        firstLine += 36;

        addToHelpList("Key Commands  ([F1] Show / Hide This Help)", ref pt, 0, renderer.ExtraLargeFont, forePen);

        pt.Y = firstLine;

        addToHelpList("[1] Ecliptic View", ref pt, 0, renderer.LargeFont, forePen);
        addToHelpList("[2] Planet Surface View", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[3] Top Down View", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[4] Following View", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[F2] Show / Hide Status Info", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F3] Search", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F4] Show / Hide Flight Instruments", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F5] Change Label View", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F6] Switch Solid/Vector Planets", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[F7] Show Fewer Stars", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F8] Show More Stars", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[F9] Show/Hide Constellations", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F10] Change Constellation Patterns", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[F11] Show/Hide Constellation Boundaries", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[F12] Highlight Sunlit Surfaces", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Right Click] Mouse Look On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        pt = new QPoint(rect.X + rect.Size.Width * 0.38, firstLine);

        addToHelpList("[Left] View Previous Object", ref pt, 0, renderer.LargeFont, forePen);
        addToHelpList("[Right] View Next Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Up] Fly to Next Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Down] Fly to Previous Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[<] View the Moon", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[>] Fly to Earth", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[R] Swap Position and View", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[\\] Adjust Date/Time", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[=] Switch Local/UTC Time", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[P] Pause", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[T] Go to Today", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Z] Slow Down", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[X] Speed Up", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[C] Reverse Time", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Page Up] Zoom In", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Page Down] Zoom Out", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Home] Zoom In Full", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[End] Zoom Out Full", ref pt, lineSpace, renderer.LargeFont, forePen);

        pt = new QPoint(rect.X + rect.Size.Width * 0.67, firstLine);

        addToHelpList("[5] Show / Hide Ecliptic Grid", ref pt, 0, renderer.LargeFont, forePen);
        addToHelpList("[6] Show / Hide Equatorial Grid", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[7] Show / Hide Local Grid", ref pt, lineSpace, renderer.LargeFont, forePen);
        
        addToHelpList("[W] Pan Up", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[S] Pan Down", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[A] Pan Left", ref pt, lineSpace, renderer.LargeFont, forePen);
        addToHelpList("[D] Pan Right", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Q] Tilt Left", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[E] Tilt Right", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Space] Reset Panning", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        addToHelpList("[Enter] Lock in on Target", ref pt, lineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Backspace] Adjust Lat/Long", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        
        addToHelpList("[~] Change Projection Mode", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Insert] Antialiased Graphics On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Tab] Full-Screen Mode On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        addToHelpList("[Esc] Exit the simulator", ref pt, lineSpace + lineSpace, renderer.LargeItalicFont, forePen);
    }

    private void addToHelpList(string Text, ref QPoint Point, float YOffset, QFont Font, QPen Pen)
    {
        Point.Y += YOffset;
        renderList.Add(new Tuple<string, QPoint, QPen, QFont>(Text, Point, Pen, Font));
    }
}
