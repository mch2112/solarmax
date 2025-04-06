//#define NO_WAIT_FOR_ENTER

using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarMax;

internal class DialogFind : Dialog
{
    public delegate void FoundItemDelegate(CelestialBody Body, bool MoveTo);
    private QRectangle findTextBorder;
    private QPoint findTextPoint;
    private QRectangle hintRectangle;
    private string userText = string.Empty;
    private string foundText = string.Empty;
    private string hintText = string.Empty;
    private const int MAX_LENGTH = 30;
    private readonly Physics physics;
    private readonly FoundItemDelegate findCallback;

    private QPoint help1Loc;
    private QPoint help2Loc;
    private QPoint help3Loc;
    private QPoint help4Loc;

    private static readonly FindEqualityComparer fc = new();

    private int selectIndex;

    public DialogFind(IRenderer Renderer, Physics Physics, QSize ScreenSize, FoundItemDelegate FoundItemDelegate, CloseCallback CloseCallback, QPen ForePen, QPen BorderPen, QPen BackPen)
        : base(Renderer, ScreenSize, CloseCallback, ForePen, BorderPen, BackPen)
    {
        this.physics = Physics;
        physics.PreloadSearch();
        this.findCallback = FoundItemDelegate;
        //this.currentItem = CurrentItem;
        this.selectIndex = 0;
        updateSearch();
    }
    public override bool SendCommand(QCommand Command)
    {
        string txt = Command.Shift ? Command.String.ToUpper() : Command.String.ToLower();

        switch (Command.CommandCode)
        {
            case CommandCode.Enter:
                if (FoundItem != null)
                {
                    if (Command.Shift)
                    {
#if NO_WAIT_FOR_ENTER
                        findCallback(currentItem, false);
#endif
                        findCallback(FoundItem, true);
                    }
                    else
                    {
                        findCallback(FoundItem, false);
                    }
                    closeCallback();
                }
                return true;
            case CommandCode.Escape:
#if NO_WAIT_FOR_ENTER
                findCallback(currentItem, false);
#endif
                closeCallback();
                return true;
            case CommandCode.Backspace:
                if (userText.Length > 0)
                    userText = userText[..^1];
                updateSearch();
                selectIndex = 0;
                return true;
            case CommandCode.Space:
                if (userText.Length > 0)
                    userText += " ";
                selectIndex = 0;
                updateSearch();
                return true;
            case CommandCode.Tab:
                if (Command.Shift)
                    selectIndex--;
                else
                    selectIndex++;
                selectIndex = Math.Max(0, selectIndex);
                updateSearch();
                return true;
            default:
                if (Command.String.Length == 1)
                {
                    selectIndex = 0;
                    userText += txt;
                    updateSearch();
                    return true;
                }
                else
                {
                    return false;
                }
        }
    }
    public CelestialBody FoundItem { get; private set; }
    private void updateSearch()
    {
        if (userText.Length > MAX_LENGTH)
            userText = userText[MAX_LENGTH..];

        if (userText.Length > 0)
        {
            string t = userText.ToLower();
            
            var results = physics.SearchDatabase.Where(b => b.Item1.StartsWith(t)).ToList();
            var result = results.Distinct(fc).Skip(selectIndex).FirstOrDefault();

            if (result == null && selectIndex > 0)
            {
                selectIndex = 0;
                result = results.FirstOrDefault();
            }
            if (result != null)
            {
                userText = result.Item2[..userText.Length];
                foundText = result.Item2;
                hintText = string.Empty;
            
                foreach (var r in results.Distinct(fc).Skip(selectIndex).Take(16))
                    hintText += r.Item2 + " ";

                if (!result.Item3.Equals(FoundItem))
                {
                    FoundItem = result.Item3;
#if NO_WAIT_FOR_ENTER
                    findCallback(FoundItem, false);
#endif
                }
            }
            else
            {
                foundText = string.Empty;
                FoundItem = null;
                hintText = string.Empty;
            }
        }
        else
        {
            foundText = string.Empty;
            FoundItem = null;
            hintText = string.Empty;
        }
    }
    public override void Render()
    {
        renderer.FillRectangle(rect.Location, rect.Size, backPen, borderPen);
        renderer.DrawString("Find a Celestial Body", titleLoc, forePen, renderer.LargeFont);

        renderer.DrawRectangle(findTextBorder, borderPen);

        renderer.DrawString(foundText, findTextPoint, forePen, renderer.LargeFont);
        renderer.DrawString(userText, findTextPoint, borderPen, renderer.LargeFont);

        renderer.DrawString(hintText, hintRectangle, forePen, renderer.LargeFont);
        renderer.DrawString(foundText, hintRectangle, borderPen, renderer.LargeFont);

        var textWidth = renderer.MeasureText(userText + "x", renderer.LargeFont).Width - renderer.MeasureText("x", renderer.LargeFont).Width + 2;
        renderer.DrawLine(borderPen, findTextPoint.X + textWidth, findTextPoint.Y + 2, findTextPoint.X + textWidth, findTextPoint.Y + 20);

        if (FoundItem != null)
        {
            renderer.DrawString("[Enter] to find " + FoundItem.FullName, help1Loc, forePen, renderer.LargeFont);
            renderer.DrawString("[Shift + Enter] to fly to " + FoundItem.FullName, help2Loc, forePen, renderer.LargeFont);
            renderer.DrawString("[Tab] to select next", help3Loc, forePen, renderer.LargeFont);
        }

        renderer.DrawString("[Escape] to Cancel", help4Loc, forePen, renderer.LargeFont);
    }
    public override string Message
    {
        get { return string.Empty; }
    }
    protected override void SetupLayout()
    {
        this.rect = this.screenSize.GetRectangleDockedBottomRight(new QSize(340, 230), 20);

        const float LEFT_MARGIN = 10;

        titleLoc = rect.TopLeft + new QPoint(LEFT_MARGIN, 10);

        findTextBorder = new QRectangle(rect.TopLeft + new QPoint(LEFT_MARGIN, 40), new QSize(rect.Width - LEFT_MARGIN * 2, 30));
        findTextPoint = findTextBorder.Location + new QPoint(4, 2);
        hintRectangle = new QRectangle(rect.Left + LEFT_MARGIN, findTextPoint.Y + 40, findTextBorder.Width, 40);

        help1Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 100);
        help2Loc = new QPoint(help1Loc.X, help1Loc.Y + 24);
        help3Loc = new QPoint(help2Loc.X, help2Loc.Y + 24);
        help4Loc = new QPoint(help3Loc.X, help3Loc.Y + 30);
    }
    private class FindEqualityComparer : IEqualityComparer<Tuple<string, string, CelestialBody>>
    {
        public bool Equals(Tuple<string, string, CelestialBody> b1, Tuple<string, string, CelestialBody> b2)
        {
            return b1.Item3.Equals(b2.Item3);
        }
        public int GetHashCode(Tuple<string, string, CelestialBody> FindItem)
        {
            return FindItem.Item3.GetHashCode();
        }
    }
}
