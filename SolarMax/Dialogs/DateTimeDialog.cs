using System;

namespace SolarMax.Dialogs;

internal sealed class DateTimeDialog : Dialog
{
    public delegate void DateTimeChange(DateTime Date);

    private readonly DateTime minDate = new(1948, 1, 1, 0, 0, 0);
    private readonly DateTime maxDate = new(2052, 12, 31, 23, 59, 59);

    private const float NUM_WIDTH = 36;
    private const float YEAR_WIDTH = 60;
    private const float BOX_HEIGHT = 40;

    private readonly QRectangle[] rects = new QRectangle[7];

    private QPoint slash1;
    private QPoint slash2;
    private QPoint colon1;
    private QPoint colon2;

    private QPoint help1Loc;
    private QPoint help2Loc;
    private QPoint help3Loc;
    private QPoint help4Loc;

    private QPoint localLoc;

    private readonly DateTimeChange changeCallback;

    private int cursorIndex = 0;

    private DateTime date;
    private bool utc;

    public DateTimeDialog(IRenderer Renderer, CloseCallback CloseCallback, DateTimeChange ChangeCallback, DateTime DateTime, bool UTC, QSize ScreenSize, QPen ForePen, QPen BorderPen, QPen BackPen)
        : base(Renderer, ScreenSize, CloseCallback, ForePen, BorderPen, BackPen)
    {
        changeCallback = ChangeCallback;
        
        date = DateTime;

        minDate = MathEx.Min(minDate, date.AddYears(-10));
        maxDate = MathEx.Max(maxDate, date.AddYears(10));

        utc = UTC;

        changeCallback(date);
    }
    public override bool SendCommand(QCommand Command)
    {
        switch (Command.CommandCode)
        {
            case CommandCode.LeftArrow:
                cursorIndex = (cursorIndex - 1 + NumBoxes) % NumBoxes;
                return true;
            case CommandCode.RightArrow:
                cursorIndex = (cursorIndex + 1) % NumBoxes;
                return true;
            case CommandCode.UpArrow:
                ChangeValue(true);
                return true;
            case CommandCode.DownArrow:
                ChangeValue(false);
                return true;
            case CommandCode.BackslashPipe:
                UTC = !UTC;
                return false;
            case CommandCode.Enter:
            case CommandCode.Escape:
                closeCallback();
                return true;
            default:
                return false;
        }
    }
    public override string Message => $"Set Date and Time to {Util.GetDateString(DateTime, UTC)}";
    public DateTime DateTime
    {
        get => date;
        set
        {
            date = value;
            changeCallback(date);
        }
    }
    public bool UTC
    {
        get => utc;
        set
        {
            if (utc != value)
            {
                utc = value;
                if (cursorIndex == 6)
                    cursorIndex = 5;
            }
        }
    }
    private int NumBoxes => UTC ? 6 : 7;
    private void ChangeValue(bool Increase)
    {
        switch (cursorIndex)
        {
            case 0:
                if (Increase)
                    date = date.AddYears(1);
                else
                    date = date.AddYears(-1);
                break;
            case 1:
                if (Increase)
                    date = date.AddMonths(1);
                else
                    date = date.AddMonths(-1);
                break;
            case 2:
                if (Increase)
                    date = date.AddDays(1);
                else
                    date = date.AddDays(-1);
                break;
            case 3:
                if (Increase)
                    date = date.AddHours(1);
                else
                    date = date.AddHours(-1);
                break;
            case 4:
                if (Increase)
                    date = date.AddMinutes(1);
                else
                    date = date.AddMinutes(-1);
                break;
            case 5:
                if (Increase)
                    date = date.AddSeconds(1);
                else
                    date = date.AddSeconds(-1);
                break;
            case 6:
                if (date.ToLocalTime().IsAfternoon())
                    date = date.AddHours(-12);
                else
                    date = date.AddHours(12);
                break;
        }
        if (date > maxDate)
            date = maxDate;
        else if (date < minDate)
            date = minDate;

        changeCallback(date);
    }
    protected override void SetupLayout()
    {
        rect = screenSize.GetRectangleDockedBottomRight(new QSize(355, 190), 20);
        
        const float LEFT_MARGIN = 10;

        titleLoc = rect.TopLeft + new QPoint(LEFT_MARGIN, 10);

        float x = rect.Left + LEFT_MARGIN;
        float y = rect.Top + 40;
        
        float margin = 10;

        rects[0] = new QRectangle(x, y, YEAR_WIDTH, BOX_HEIGHT);

        x += YEAR_WIDTH + margin;
        rects[1] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[2] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[3] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[4] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[5] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[6] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        QPoint offset = new(-2, 3);

        slash1 = rects[0].TopRight + offset;
        slash2 = rects[1].TopRight + offset;
        colon1 = rects[3].TopRight + offset;
        colon2 = rects[4].TopRight + offset;

        localLoc = new QPoint(rects[6].Center.X - 1, rects[6].Bottom - 5);

        help1Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 100);
        help2Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 76);
        help3Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 52);
        help4Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 28);
    }
    public override void Render()
    {
        DateTime displayDate = date;

        if (!UTC)
            displayDate = displayDate.ToLocalTime();

        renderer.FillRectangle(rect.Location, rect.Size, backPen, borderPen);

        renderer.DrawString("Date and Time Adjustment", titleLoc, forePen, renderer.LargeFont);

        renderer.DrawStringCentered(displayDate.Year.ToString("0000"), rects[0].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered(displayDate.Month.ToString("00"), rects[1].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered(displayDate.Day.ToString("00"), rects[2].Center, forePen, renderer.ExtraLargeFont);

        
        renderer.DrawStringCentered(displayDate.Minute.ToString("00"), rects[4].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered(displayDate.Second.ToString("00"), rects[5].Center, forePen, renderer.ExtraLargeFont);

        if (UTC)
        {
            renderer.DrawStringCentered(displayDate.Hour.ToString("00"), rects[3].Center, forePen, renderer.ExtraLargeFont);
            renderer.DrawStringCentered("UTC", rects[6].Center, forePen, renderer.ExtraLargeFont);
        }
        else
        {
            renderer.DrawStringCentered(displayDate.ToString("hh"), rects[3].Center, forePen, renderer.ExtraLargeFont);
            renderer.DrawStringCentered(displayDate.IsAfternoon() ? "PM" : "AM", rects[6].Center, forePen, renderer.ExtraLargeFont);
            renderer.DrawStringCentered("Local", localLoc, forePen, renderer.SmallFont);
        }

        renderer.DrawString("/", slash1, forePen, renderer.ExtraLargeFont);
        renderer.DrawString("/", slash2, forePen, renderer.ExtraLargeFont);
        renderer.DrawString(":", colon1, forePen, renderer.ExtraLargeFont);
        renderer.DrawString(":", colon2, forePen, renderer.ExtraLargeFont);

        renderer.DrawRectangle(rects[cursorIndex], borderPen);

        renderer.DrawString("[Left] / [Right] arrows to select", help1Loc, forePen, renderer.LargeFont);
        renderer.DrawString("[Up] / [Down] arrows to change value", help2Loc, forePen, renderer.LargeFont);
        renderer.DrawString("[=] to switch between local and UTC", help3Loc, forePen, renderer.LargeFont);
        renderer.DrawString("Hit [Enter] when done", help4Loc, forePen, renderer.LargeFont);
    }
}
