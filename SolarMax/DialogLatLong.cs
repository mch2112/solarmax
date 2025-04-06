namespace SolarMax;

internal sealed class DialogLatLong : Dialog
{
    public delegate void LatLongChange(double Latitude, double Longitude);

    private const float NUM_WIDTH = 48;
    private const float CHAR_WIDTH = 30;
    private const float BOX_HEIGHT = 40;

    private readonly QRectangle[] rects = new QRectangle[8];

    private QPoint latLoc;
    private QPoint lngLoc;
    private QPoint help1Loc;
    private QPoint help2Loc;
    private QPoint help3Loc;

    private readonly LatLongChange changeCallback;

    private int cursorIndex = 0;

    int latDeg, latMin, latSec, lngDeg, lngMin, lngSec;
    bool north, east;

    public DialogLatLong(IRenderer Renderer, CloseCallback CloseCallback, LatLongChange ChangeCallback, double Latitude, double Longitude, QSize ScreenSize, QPen ForePen, QPen BorderPen, QPen BackPen) : base(Renderer, ScreenSize, CloseCallback, ForePen, BorderPen, BackPen)
    {
        this.changeCallback = ChangeCallback;

        Latitude.ToComponents(out latDeg, out latMin, out latSec, out north);
        Longitude.ToComponents(out lngDeg, out lngMin, out lngSec, out east);
    }
    public override bool SendCommand(QCommand Command)
    {
        switch (Command.CommandCode)
        {
            case CommandCode.LeftArrow:
                cursorIndex = (cursorIndex - 1 + 8) % 8;
                return true;
            case CommandCode.RightArrow:
                cursorIndex = (cursorIndex + 1) % 8;
                return true;
            case CommandCode.UpArrow:
                ChangeValue(true);
                return true;
            case CommandCode.DownArrow:
                ChangeValue(false);
                return true;
            case CommandCode.Enter:
            case CommandCode.Escape:
                closeCallback();
                return true;
            default:
                return false;
        }
    }
    public override string Message 
        => $"Latitude: {Latitude.ToLatitudeString()} Longitude: {Longitude.ToLongitudeString()}";
    private void ChangeValue(bool Increase)
    {
        int offset = Increase ? 1 : -1;

        switch (cursorIndex)
        {
            case 0:
                latDeg = (latDeg + offset).Clamp(0, 91);
                if (latDeg >= 90)
                {
                    latDeg = 90;
                    latMin = 0;
                    latSec = 0;
                }
                break;
            case 1:
                if (latDeg >= 90)
                    latDeg = 89;
                latMin = (latMin + offset).NormalizeZeroToSixty();
                break;
            case 2:
                if (latDeg >= 90)
                    latDeg = 89;
                latSec = (latSec + offset).NormalizeZeroToSixty();
                break;
            case 3:
                north = !north;
                break;
            case 4:
                lngDeg = (lngDeg + offset).Clamp(0, 181);
                if (lngDeg >= 180)
                {
                    lngDeg = 180;
                    lngMin = 0;
                    lngSec = 0;
                }
                break;
            case 5:
                if (lngDeg >= 180)
                    lngDeg = 179;
                lngMin = (lngMin + offset).NormalizeZeroToSixty();
                break;
            case 6:
                if (lngDeg >= 180)
                    lngDeg = 179;
                lngSec = (lngSec + offset).NormalizeZeroToSixty();
                break;
            case 7:
                east = !east;
                break;
        }
        changeCallback(Latitude, Longitude);
    }

    public double Latitude => MathEx.FromComponents(latDeg, latMin, latSec, north);
    public double Longitude => MathEx.FromComponents(lngDeg, lngMin, lngSec, east);
    protected override void SetupLayout()
    {
        this.rect = this.screenSize.GetRectangleDockedBottomRight(new QSize(400, 195), 20);

        const float LEFT_MARGIN = 10;

        titleLoc = rect.TopLeft + new QPoint(LEFT_MARGIN, 10);

        float x = rect.Left + LEFT_MARGIN;
        float y = rect.Top + 40;
        float margin = 3;

        rects[0] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[1] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[2] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[3] = new QRectangle(x, y, CHAR_WIDTH, BOX_HEIGHT);

        x += CHAR_WIDTH + margin * 4;
        rects[4] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[5] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[6] = new QRectangle(x, y, NUM_WIDTH, BOX_HEIGHT);

        x += NUM_WIDTH + margin;
        rects[7] = new QRectangle(x, y, CHAR_WIDTH, BOX_HEIGHT);

        latLoc = rects[0].BottomLeft + new QPoint(70, 2);
        lngLoc = rects[4].BottomLeft + new QPoint(70, 2);
        
        help1Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 76);
        help2Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 52);
        help3Loc = new QPoint(rect.Left + LEFT_MARGIN, rect.Bottom - 28);
    }
    public override void Render()
    {
        renderer.FillRectangle(rect.Location, rect.Size, backPen, borderPen);

        renderer.DrawString("Latitude and Longitude Adjustment", titleLoc, forePen, renderer.LargeFont);

        renderer.DrawStringCentered($"{latDeg:00}°", rects[0].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered($"{latMin:00}'", rects[1].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered($"{latSec:00}\"", rects[2].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered(north ? "N" : "S", rects[3].Center, forePen, renderer.ExtraLargeFont);

        renderer.DrawStringCentered($"{lngDeg:00}°", rects[4].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered($"{lngMin:00}'", rects[5].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered($"{lngSec:00}\"", rects[6].Center, forePen, renderer.ExtraLargeFont);
        renderer.DrawStringCentered(east ? "E" : "W", rects[7].Center, forePen, renderer.ExtraLargeFont);

        renderer.DrawString($"({MathEx.FromComponents(latDeg, latMin, latSec, north).ToDegreesFromRadians():00.0000}°)", latLoc, forePen, renderer.SmallFont);
        renderer.DrawString($"({MathEx.FromComponents(lngDeg, lngMin, lngSec, east).ToDegreesFromRadians():000.0000}°)", lngLoc, forePen, renderer.SmallFont);

        renderer.DrawRectangle(rects[cursorIndex], borderPen);

        renderer.DrawString("[Left] / [Right] arrows to select", help1Loc, forePen, renderer.LargeFont);
        renderer.DrawString("[Up] / [Down] arrows to change value", help2Loc, forePen, renderer.LargeFont); 
        renderer.DrawString("Hit [Enter] when done", help3Loc, forePen, renderer.LargeFont);
    }
}