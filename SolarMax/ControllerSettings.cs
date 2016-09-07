using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Controller
    {
        public bool ShowCredits { get; set; }

        private CaptionMode CaptionMode { get; set; }
        private ConstellationMode ConstellationMode { get; set; }
        private bool ShowConstellationBoundaries
        {
            get { return showConstellationBoundaries; }
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
            get { return camera.ViewMode; }
            set
            {
                camera.ZoomTarget = MinimumPixelsPerRadian;
                camera.ViewMode = value;
                projector.ResetPanning();
                switch (value)
                {
                    case SolarMax.ViewMode.Ecliptic:
                        messageWidget.SetMessage("Ecliptic View");
                        break;
                    case SolarMax.ViewMode.Surface:
                        messageWidget.SetMessage("Planet Surface View");
                        break;
                    case SolarMax.ViewMode.TopDown:
                        messageWidget.SetMessage("Top Down View");
                        break;
                    case SolarMax.ViewMode.Follow:
                        messageWidget.SetMessage("Following View");
                        break;
                }
            }
        }

        public double SurfaceLatitude { get { return camera.SurfaceLatitudeTarget; } }
        public double SurfaceLongitude { get { return camera.SurfaceLongitudeTarget; } }
        public QSize ScreenSize
        {
            get { return screenSize; }
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

                    camera.ZoomTarget = validateZoom(camera.ZoomTarget);
                }
            }
        }
        private bool HighQualityRender
        {
            get { return highQualityRender; }
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
            get { return this.showInstruments; }
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
            get { return this.showData; }
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
            get { return this.showEclipticGrid; }
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
            get { return this.showEquatorialGrid; }
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
            get { return this.showLocalGrid; }
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
            get { return renderer.HighlightSunlitAreas; }
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
            get { return renderer.WireFrameBodyRender; }
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
            get { return displayTimeUTC; }
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
                        (currentDialog as DialogDateTime).UTC = value;
                }
            }
        }
        private float MinStarBrightness
        {
            get { return minStarBrightness; }
            set
            {
                minStarBrightness = ((float)((double)value + MathEx.EPSILON).Floor()).Clamp(MIN_STAR_BRIGHTNESS_THRESHOLD, MAX_STAR_BRIGHTNESS_THRESHOLD);
            }
        }
    }
}
