using System;
using System.Collections.Generic;

namespace SolarMax.Dialogs;

internal class HelpDialog(IRenderer Renderer,
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

        AddToHelpList("Key Commands  ([F1] Show / Hide This Help)", ref pt, 0, renderer.ExtraLargeFont, forePen);

        pt.Y = firstLine;

        AddToHelpList("[1] Ecliptic View", ref pt, 0, renderer.LargeFont, forePen);
        AddToHelpList("[2] Planet Surface View", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[3] Top Down View", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[4] Following View", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[F2] Show / Hide Status Info", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F3] Search", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F4] Show / Hide Flight Instruments", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F5] Change Label View", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F6] Switch Solid/Vector Planets", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[F7] Show Fewer Stars", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F8] Show More Stars", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[F9] Show/Hide Constellations", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F10] Change Constellation Patterns", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[F11] Show/Hide Constellation Boundaries", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[F12] Highlight Sunlit Surfaces", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Right Click] Mouse Look On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        pt = new QPoint(rect.X + rect.Size.Width * 0.38, firstLine);

        AddToHelpList("[Left] View Previous Object", ref pt, 0, renderer.LargeFont, forePen);
        AddToHelpList("[Right] View Next Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Up] Fly to Next Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Down] Fly to Previous Object", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[<] View the Moon", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[>] Fly to Earth", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[R] Swap Position and View", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[\\] Adjust Date/Time", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[=] Switch Local/UTC Time", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[P] Pause", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[T] Go to Today", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Z] Slow Down", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[X] Speed Up", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[C] Reverse Time", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Page Up] Zoom In", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Page Down] Zoom Out", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Home] Zoom In Full", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[End] Zoom Out Full", ref pt, lineSpace, renderer.LargeFont, forePen);

        pt = new QPoint(rect.X + rect.Size.Width * 0.67, firstLine);

        AddToHelpList("[5] Show / Hide Ecliptic Grid", ref pt, 0, renderer.LargeFont, forePen);
        AddToHelpList("[6] Show / Hide Equatorial Grid", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[7] Show / Hide Local Grid", ref pt, lineSpace, renderer.LargeFont, forePen);
        
        AddToHelpList("[W] Pan Up", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[S] Pan Down", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[A] Pan Left", ref pt, lineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[D] Pan Right", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Q] Tilt Left", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[E] Tilt Right", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Space] Reset Panning", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        AddToHelpList("[Enter] Lock in on Target", ref pt, lineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Backspace] Adjust Lat/Long", ref pt, largeLineSpace, renderer.LargeFont, forePen);
        
        AddToHelpList("[~] Change Projection Mode", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Insert] Antialiased Graphics On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Tab] Full-Screen Mode On / Off", ref pt, largeLineSpace, renderer.LargeFont, forePen);

        AddToHelpList("[Esc] Exit the simulator", ref pt, lineSpace + lineSpace, renderer.LargeItalicFont, forePen);
    }

    private void AddToHelpList(string Text, ref QPoint Point, float YOffset, QFont Font, QPen Pen)
    {
        Point.Y += YOffset;
        renderList.Add(new Tuple<string, QPoint, QPen, QFont>(Text, Point, Pen, Font));
    }
}
