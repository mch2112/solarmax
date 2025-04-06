using SolarMax.Dialogs;

namespace SolarMax.Controllers;

internal sealed partial class Controller
{
    public bool ShowCredits { get; set; }

    private CaptionMode CaptionMode { get; set; }
    private ConstellationMode ConstellationMode { get; set; }
    private bool ShowConstellationBoundaries
    {
        get => showConstellationBoundaries;
        set
        {
            if (showConstellationBoundaries != value)
            {
                showConstellationBoundaries = value;
                messageWidget.SetMessage(value ? "Show Constellation Boundaries" : "Hide Constellation Boundaries");
            }
        }
    }
    public ViewMode ViewMode
    {
        get => camera.ViewMode;
        set
        {
            camera.ZoomTarget = MinimumPixelsPerRadian;
            camera.ViewMode = value;
            projector.ResetPanning();
            switch (value)
            {
                case ViewMode.Ecliptic:
                    messageWidget.SetMessage("Ecliptic View");
                    break;
                case ViewMode.Surface:
                    messageWidget.SetMessage("Planet Surface View");
                    break;
                case ViewMode.TopDown:
                    messageWidget.SetMessage("Top Down View");
                    break;
                case ViewMode.Follow:
                    messageWidget.SetMessage("Following View");
                    break;
                default:
                    break;
            }
        }
    }

    public double SurfaceLatitude => camera.SurfaceLatitudeTarget;
    public double SurfaceLongitude => camera.SurfaceLongitudeTarget;
    public QSize ScreenSize
    {
        get => screenSize;
        set
        {
            if (screenSize != value)
            {
                screenSize = value;
                renderer.ScreenSize = value;

                var size = new QSize(value.Width / 4, value.Width / 5);

                if (size.Height > MAX_INSTRUMENT_HEIGHT)
                    size *= (MAX_INSTRUMENT_HEIGHT / size.Height);

                azimuthInstrument.Bounds = new QRectangle(new QPoint(0, renderer.ScreenSize.Height - size.Height - 2), size);
                inclinometerInstrument.Bounds = new QRectangle(azimuthInstrument.Bounds.TopRight, new QSize(size.Width * 0.75, size.Height));
                attitudeInstrument.Bounds = new QRectangle(inclinometerInstrument.Bounds.TopRight, size);
                fieldOfViewInstrument.Bounds = new QRectangle(attitudeInstrument.Bounds.TopRight, size);

                camera.ZoomTarget = ValidateZoom(camera.ZoomTarget);
            }
        }
    }
    private bool HighQualityRender
    {
        get => highQualityRender;
        set
        {
            if (highQualityRender != value)
            {
                highQualityRender = value;
                if (highQualityRender)
                    messageWidget.SetMessage("Antialiased Display");
                else
                    messageWidget.SetMessage("Normal Display");
            }
        }
    }
    private bool ShowInstruments
    {
        get => showInstruments;
        set
        {
            this.showInstruments = !this.showInstruments;
            if (this.showInstruments)
                messageWidget.SetMessage("Show Instruments");
            else
                messageWidget.SetMessage("Hide Instruments");
        }
    }
    private bool ShowData
    {
        get => this.showData;
        set
        {
            this.showData = value;
            switch (this.showData)
            {
                case true:
                    messageWidget.SetMessage("Show Status Data");
                    break;
                case false:
                    messageWidget.SetMessage("Hide Status Data");
                    break;
            }
        }
    }
    private bool ShowEclipticGrid
    {
        get => this.showEclipticGrid;
        set
        {
            this.showEclipticGrid = value;
            switch (this.showEclipticGrid)
            {
                case true:
                    messageWidget.SetMessage("Show Ecliptic Grid");
                    break;
                case false:
                    messageWidget.SetMessage("Hide Ecliptic Grid");
                    break;
            }
        }
    }
    private bool ShowEquatorialGrid
    {
        get => this.showEquatorialGrid;
        set
        {
            this.showEquatorialGrid = value;
            switch (this.showEquatorialGrid)
            {
                case true:
                    messageWidget.SetMessage("Show Equatorial Grid");
                    break;
                case false:
                    messageWidget.SetMessage("Hide Equatorial Grid");
                    break;
            }
        }
    }
    private bool ShowLocalGrid
    {
        get => this.showLocalGrid;
        set
        {
            if (this.ViewMode == SolarMax.ViewMode.Surface)
            {
                this.showLocalGrid = value;
                switch (value)
                {
                    case true:
                        messageWidget.SetMessage("Show Local Grid");
                        break;
                    case false:
                        messageWidget.SetMessage("Hide Local Grid");
                        break;
                }
            }
            else
            {
                messageWidget.SetMessage("Local Grid in Surface Mode Only [2]");
            }
        }
    }
    private bool HighlightSunlitAreas
    {
        get => renderer.HighlightSunlitAreas;
        set
        {
            if (value && !WireFrameBodyRender)
                WireFrameBodyRender = true;

            renderer.HighlightSunlitAreas = value;
            if (value)
                messageWidget.SetMessage("Highlight Sunlit Areas");
            else
                messageWidget.SetMessage("Don't Highlight Sunlit Areas");
        }
    }
    private bool WireFrameBodyRender
    {
        get => renderer.WireFrameBodyRender;
        set
        {
            renderer.WireFrameBodyRender = value;
            if (value)
                messageWidget.SetMessage("Vector Body Render");
            else
                messageWidget.SetMessage("Plain Body Render");
        }
    }
    public bool DisplayTimeUTC
    {
        get => displayTimeUTC;
        set
        {
            if (displayTimeUTC != value)
            {
                displayTimeUTC = value;
                if (value)
                {
                    messageWidget.SetMessage("Time Display in UTC");
                }
                else
                {
                    messageWidget.SetMessage("Time Display in Local Time");
                    
                }
                if (InDateTimeAdjustMode)
                    (currentDialog as DateTimeDialog).UTC = value;
            }
        }
    }
    private float MinStarBrightness
    {
        get => minStarBrightness; 
        set => minStarBrightness = ((float)((double)value + MathEx.EPSILON).Floor())
                                            .Clamp(MIN_STAR_BRIGHTNESS_THRESHOLD, MAX_STAR_BRIGHTNESS_THRESHOLD);
    }
}
