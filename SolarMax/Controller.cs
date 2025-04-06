using SolarMax.Integrators;
using System;
using System.Collections.Generic;

namespace SolarMax;

internal enum ScreenSaverMode { Application, ScreenSaver, ScreenSaverPreview }
internal enum CaptionMode { None, DynamicOnly, Auto }
internal enum ConstellationMode { None, Lines, LinesAndCaptions }

internal sealed partial class Controller
{
    public delegate void ShutdownRequestDelegate();

    public const string APPLICATION_NAME = "SolarMax";
    public const string APPLICATION_VERSION = "1.0.5";
    private const double MAX_INSTRUMENT_HEIGHT = 175;
    
    private const double PAN_ADJUST = 1.0 / 200;

    private const float MAX_STAR_BRIGHTNESS_THRESHOLD = 7f;
    private const float MIN_STAR_BRIGHTNESS_THRESHOLD = -1f;

    private readonly Projector projector;
    private QSize screenSize = QSize.Empty;
    
    private readonly Physics physics;

    private Shape equatorialGrid { get; set; }
    private Shape eclipticGrid { get; set; }
    private ResettableShape localGrid { get; set; }
    private ResettableShape localGridWithRefraction { get; set; }

    private bool showInstruments;
    private bool showData;
    private bool showEclipticGrid;
    private bool showEquatorialGrid;
    private bool showLocalGrid;
    private bool highQualityRender;
    private bool showConstellationBoundaries;

    private bool displayTimeUTC;

    private float minStarBrightness = float.MinValue;

    private readonly ScreenSaverMode screenSaverMode;
    private QSize creditsSize;
    private readonly string credits = $"{APPLICATION_NAME} {APPLICATION_VERSION} (c) 2013-{DateTime.Now.Year} Matthew Hamilton";

    private InstrumentDataSource instrumentDataSource;
    private InstrumentAzimuth azimuthInstrument;
    private InstrumentInclinometer inclinometerInstrument;
    private InstrumentAttitude attitudeInstrument;
    private InstrumentFieldOfView fieldOfViewInstrument;

    private WidgetData dataWidget;
    private WidgetMessage messageWidget;

    private Dialog currentDialog = null;
    private readonly ShutdownRequestDelegate shutdownCallback;

    private bool physicsStartupDone = false;
#if WPF
    private System.Windows.Media.Imaging.WriteableBitmap drawingTarget = null;
#else
    private System.Drawing.Graphics drawingTarget = null;
#endif

    public Controller(ScreenSaverMode ScreenSaverMode, ShutdownRequestDelegate ShutdownCallback, int FrameRate, bool AllowLoadSnapshot)
    {
        this.shutdownCallback = ShutdownCallback;
        this.screenSaverMode = ScreenSaverMode;
        this.projector = new Projector(Properties.Settings.Default.ProjectionMode);

        this.renderer = new Renderer(screenSaverMode, projector);

        this.defaultPen = new QPen(Colors.GetColor("default_interface_color"));
        this.borderPen = new QPen(Colors.GetColor("border"));

        this.constellationBoundaryPen = new QPen(Colors.GetColor("constellation_boundary"));

        this.eclipticCompassPen = new QPen(Colors.GetColor("ecliptic_compass"));
        this.eclipticGridPen = new QPen(Colors.GetColor("ecliptic_grid"));
        
        this.equatorialCompassPen = new QPen(Colors.GetColor("equatorial_compass"));
        this.equatorialGridPen = new QPen(Colors.GetColor("equatorial_grid"));

        this.localCompassPen = new QPen(Colors.GetColor("local_compass"));
        this.localGridPen = new QPen(Colors.GetColor("local_grid"));

        this.instrumentLinePen = new QPen(Colors.GetColor("instrument_line"));
        this.instrumentOutlinePen = new QPen(Colors.GetColor("instrument_outline"));
        this.instrumentDataPen = new QPen(Colors.GetColor("instrument_data"));

        physics = new Physics(new VelocityVerletIntegrator(true), AllowLoadSnapshot, physicsStartupDoneCallback);

        setupGrids();

        this.camera = new Camera(Properties.Settings.Default.ViewMode, CelestialBody.Earth, CelestialBody.Sun, Properties.Settings.Default.SurfaceLatitude, Properties.Settings.Default.SurfaceLongitude);
        
        setupInstrumentsAndWidgets(FrameRate);
        
        switch (ScreenSaverMode)
        {
            case ScreenSaverMode.Application:
                physics.SleepBetweenCycles = 0;
                break;
            case ScreenSaverMode.ScreenSaver:
                physics.SleepBetweenCycles = 10;
                break;
            case ScreenSaverMode.ScreenSaverPreview:
                physics.SleepBetweenCycles = 100;
                break;
        }

        physics.Go(System.Threading.ThreadPriority.AboveNormal);

        this.ShowData = Properties.Settings.Default.ShowData;
        this.ShowEquatorialGrid = Properties.Settings.Default.ShowEquatorialGrid;
        this.ShowEclipticGrid = Properties.Settings.Default.ShowEclipticGrid;
        this.showLocalGrid = Properties.Settings.Default.ShowLocalGrid;
        this.InHelpMode = Properties.Settings.Default.ShowHelp;
        this.showInstruments = Properties.Settings.Default.ShowInstruments;
        this.CaptionMode = screenSaverMode == SolarMax.ScreenSaverMode.ScreenSaverPreview ? CaptionMode.DynamicOnly : Properties.Settings.Default.CaptionMode;
        this.ConstellationMode = Properties.Settings.Default.ConstellationMode;
        Constellation.UseAltShapes = Properties.Settings.Default.UseAltConstellationShapes;
        this.ShowConstellationBoundaries = Properties.Settings.Default.ShowConstellationBoundaries;
        this.MinStarBrightness = Properties.Settings.Default.MinStarBrightness;
        this.DisplayTimeUTC = Properties.Settings.Default.DisplayTimeUTC;
        
        renderer.WireFrameBodyRender = Properties.Settings.Default.WireFrameBodyRender;
        renderer.HighlightSunlitAreas = Properties.Settings.Default.HighlightSunlitAreas;
        
        this.HighQualityRender = Properties.Settings.Default.HighQualityRender;
        
        camera.SetTarget(getDefaultViewTarget());
        this.ShowCredits = true;
        creditsSize = QSize.Empty;
    }
    
    private void setupInstrumentsAndWidgets(int FrameRate)
    {
        instrumentDataSource = new InstrumentDataSource(this, physics, camera, projector);
        azimuthInstrument = new InstrumentAzimuth(renderer, instrumentDataSource, instrumentLinePen, instrumentOutlinePen, instrumentDataPen);
        inclinometerInstrument = new InstrumentInclinometer(renderer, instrumentDataSource, instrumentLinePen, instrumentOutlinePen, instrumentDataPen);
        attitudeInstrument = new InstrumentAttitude(renderer, instrumentDataSource, instrumentLinePen, instrumentOutlinePen, instrumentDataPen);
        fieldOfViewInstrument = new InstrumentFieldOfView(renderer, instrumentDataSource, instrumentLinePen, instrumentOutlinePen, instrumentDataPen);

        messageWidget = new WidgetMessage(renderer, FrameRate * 2, defaultPen, borderPen);
        messageWidget.SetMessage("Computing Starting Locations...", true);
        dataWidget = new WidgetData(this, renderer, physics, camera, defaultPen);
    }

    private CelestialBody getDefaultViewTarget()
    {
        if (physics.BodyDictionary.TryGetValue(Properties.Settings.Default.DefaultViewTarget, out CelestialBody cb))
            if (isValidViewTarget(cb))
                return cb;

        return CelestialBody.Moon;
    }
    
    ~Controller()
    {
        if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
        {
            Properties.Settings.Default.ShowData = this.ShowData;
            
            Properties.Settings.Default.ProjectionMode = projector.ProjectionMode;
            Properties.Settings.Default.ShowEclipticGrid = this.ShowEclipticGrid;
            Properties.Settings.Default.ShowEquatorialGrid = this.ShowEquatorialGrid;
            Properties.Settings.Default.ShowLocalGrid = this.ShowLocalGrid;
            Properties.Settings.Default.ViewMode = this.ViewMode;
            Properties.Settings.Default.ShowInstruments = this.showInstruments;
            Properties.Settings.Default.ShowHelp = this.InHelpMode;
            Properties.Settings.Default.CaptionMode = this.CaptionMode;
            Properties.Settings.Default.MinStarBrightness = this.MinStarBrightness;
            Properties.Settings.Default.SurfaceLatitude = camera.SurfaceLatitudeTarget;
            Properties.Settings.Default.SurfaceLongitude = camera.SurfaceLongitudeTarget;
            Properties.Settings.Default.DisplayTimeUTC = this.DisplayTimeUTC;
            Properties.Settings.Default.WireFrameBodyRender = renderer.WireFrameBodyRender;
            Properties.Settings.Default.HighlightSunlitAreas = renderer.HighlightSunlitAreas;
            Properties.Settings.Default.ConstellationMode = this.ConstellationMode; 
            Properties.Settings.Default.UseAltConstellationShapes = Constellation.UseAltShapes;
            Properties.Settings.Default.ShowConstellationBoundaries = ShowConstellationBoundaries;
            Properties.Settings.Default.HighQualityRender = this.HighQualityRender;
            Properties.Settings.Default.DefaultViewTarget = camera.BodyBeingViewed.Name;
            Properties.Settings.Default.Save();
        }
    }

    private void physicsStartupDoneCallback()
    {
        physicsStartupDone = true;
        messageWidget.Unlock(true);
    }
    public bool SendCommand(QCommand Command)
    {
        if (Command.CommandCode == CommandCode.MouseClick)
        {
            CelestialBody cb = FindClosestBody(Command.Point, renderer.RenderLocations, renderer.NumRenderLocations);
            if (cb != null)
            {
                ResetPanningAdjustments();
                if (Command.Shift)
                    setHome(cb);
                else
                    setViewTarget(cb);

                return true;
            }
            else
            {
                return false;
            }
        }
        if (currentDialog != null)
            if (currentDialog.SendCommand(Command))
                return true;

        switch (Command.CommandCode)
        {
            case CommandCode.MouseHorizontal:
                if (Command.Shift)
                    tilt(Command.Data);
                else if (!Command.Control)
                    panHorizontal(Command.Data);
                break;
            case CommandCode.MouseVertical:
                if (Command.Control)
                    zoom(Command.Data);
                else if (!Command.Shift)
                    panVertical(Command.Data);
                break;
            case CommandCode.MouseWheel:
                zoom(Command.Data);
                break;
            case CommandCode.Escape:
                if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
                    shutdownCallback();
                break;
            case CommandCode.Enter:
                this.settle();
                break;
            case CommandCode.UpArrow:
                this.setHome(Forward: true);
                break;
            case CommandCode.DownArrow:
                this.setHome(Forward: false);
                break;
            case CommandCode.LeftArrow:
                this.setViewTarget(Forward: false);
                break;
            case CommandCode.RightArrow:
                this.setViewTarget(Forward: true);
                break;
            case CommandCode.Five:
                this.ShowEclipticGrid = !this.ShowEclipticGrid;
                break;
            case CommandCode.Six:
                this.ShowEquatorialGrid = !this.ShowEquatorialGrid;
                break;
            case CommandCode.Seven:
                this.ShowLocalGrid = !this.ShowLocalGrid;
                break;
            case CommandCode.F1:
                this.InHelpMode = !this.InHelpMode;
                break;
            case CommandCode.F2:
                this.ShowData = !this.ShowData;
                break;
            case CommandCode.F3:
                this.InFindMode = !this.InFindMode;
                break;
            case CommandCode.F4:
                
                if (Command.Alt)
                    return false;

                this.ShowInstruments = !this.ShowInstruments;
                break;
            case CommandCode.F5:
                this.switchCaptionMode();
                break;
            case CommandCode.F6:
                this.WireFrameBodyRender = !this.WireFrameBodyRender;
                break;
            case CommandCode.F7:
                this.adjustMinBrightness(-1f);
                break;
            case CommandCode.F8:
                this.adjustMinBrightness(1f);
                break;
            case CommandCode.F9:
                this.switchConstellationMode();
                break;
            case CommandCode.F10:
                this.switchConstellationSet();
                break;
            case CommandCode.F11:
                this.ShowConstellationBoundaries = !this.ShowConstellationBoundaries;
                break;
            case CommandCode.PageDown:
                this.ZoomOut(getAdjustmentCode(Command));
                break;
            case CommandCode.PageUp:
                this.zoomIn(getAdjustmentCode(Command));
                break;
            case CommandCode.End:
                this.ZoomOut(AdjustmentAmount.All);
                break;
            case CommandCode.Home:
                this.zoomIn(AdjustmentAmount.All);
                break;
            case CommandCode.P:
            case CommandCode.Pause:
                if (!this.InDateTimeAdjustMode)
                    this.UserPaused = !this.UserPaused;
                break;
            case CommandCode.Backspace:
                this.InLatLongAdjustMode = !this.InLatLongAdjustMode;
                break;
            case CommandCode.BackslashPipe:
                if (physicsStartupDone)
                    this.InDateTimeAdjustMode = !this.InDateTimeAdjustMode;
                break;
            case CommandCode.PlusEquals:
                this.DisplayTimeUTC = !this.DisplayTimeUTC;
                break;
            case CommandCode.One:
                this.ViewMode = ViewMode.Ecliptic;
                break;
            case CommandCode.Two:
                this.ViewMode = ViewMode.Surface;
                break;
            case CommandCode.Three:
                this.ViewMode = ViewMode.TopDown;
                break;
            case CommandCode.Four:
                this.ViewMode = ViewMode.Follow;
                break;
            case CommandCode.T:
                goToToday();
                break;
            case CommandCode.Z:
                slowDown(getAdjustmentCode(Command));
                break;
            case CommandCode.X:
                speedUp(getAdjustmentCode(Command));
                break;
            case CommandCode.R:
                swapViewAndHome();
                break;
            case CommandCode.C:
                reverseTime();
                break;
            case CommandCode.A:
                panLeft(getAdjustmentCode(Command));
                break;
            case CommandCode.D:
                panRight(getAdjustmentCode(Command));
                break;
            case CommandCode.W:
                panUp(getAdjustmentCode(Command));
                break;
            case CommandCode.S:
                panDown(getAdjustmentCode(Command));
                break;
            case CommandCode.Q:
                tiltLeft(getAdjustmentCode(Command));
                break;
            case CommandCode.E:
                tiltRight(getAdjustmentCode(Command));
                break;
            case CommandCode.Space:
                ResetPanningAdjustments();
                break;
            case CommandCode.TildeBackTick:
                switchProjectionMode();
                break;
            case CommandCode.Insert:
                HighQualityRender = !HighQualityRender;
                break;
            case CommandCode.CommaLeftAngle:
                findTarget();
                break;
            case CommandCode.PeriodRightAngle:
                findHome();
                break;
            case CommandCode.F12:
                this.HighlightSunlitAreas = !this.HighlightSunlitAreas;
                break;
            default:
                return false;
        }
        return true;
    }

    private void switchProjectionMode()
    {
        ProjectionMode = this.ProjectionMode switch
        {
            ProjectionMode.Stereographic => ProjectionMode.Cylindrical,
            ProjectionMode.Cylindrical => ProjectionMode.Orthographic,
            _ => ProjectionMode.Stereographic,
        };
    }

    private static CelestialBody FindClosestBody(QPoint Point, List<CelestialBody> RenderLocations, int NumLocations)
    {
        CelestialBody closest = null;
        float minDist = 300f;
        for (int i = 0; i < NumLocations; i++)
        {
            if (RenderLocations[i].RenderPoint.DistanceTo(Point) < minDist)
            {
                closest = RenderLocations[i];
                minDist = RenderLocations[i].RenderPoint.DistanceTo(Point);
            }
        }
        return closest;
    }

    private void settle()
    {
        camera.Track(false);
        projector.Settle();
        messageWidget.SetMessage("View Settled");
    }
    
    public void Cancel()
    {
        physics.Cancel();
    }

    
    public ProjectionMode ProjectionMode
    {
        get { return projector.ProjectionMode; }
        set
        {
            projector.ProjectionMode = value;
            switch (value)
            {
                case ProjectionMode.Cylindrical:
                    messageWidget.SetMessage("Cylindrical Projection");
                    break;
                case ProjectionMode.Orthographic:
                    messageWidget.SetMessage("Orthographic Projection");
                    break;
                case ProjectionMode.Stereographic:
                    messageWidget.SetMessage("Stereographic Projection");
                    break;
            }
        }
    }
    public void ShowMessage(string Message)
    {
        messageWidget.SetMessage(Message);
    }
    
    private bool CanRender(CelestialBody CB)
    {
        return (this.ViewMode == ViewMode.TopDown || CB != camera.BodyWithCamera) &&
               (CB is not Star cb || (CanViewStars && cb.Magnitude < MinStarBrightness)) &&
               (ConstellationMode != ConstellationMode.None || CB is not Constellation);
    }
    
    private void switchConstellationMode()
    {
        switch (this.ConstellationMode)
        {
            case ConstellationMode.LinesAndCaptions:
                ConstellationMode = ConstellationMode.None;
                messageWidget.SetMessage("Hide Constellations");
                break;
            case ConstellationMode.Lines:
                messageWidget.SetMessage("Show Constellation Patterns and Names");
                ConstellationMode = SolarMax.ConstellationMode.LinesAndCaptions;
                break;
            case ConstellationMode.None:
                ConstellationMode = SolarMax.ConstellationMode.Lines;
                messageWidget.SetMessage("Show Constellation Patterns");
                break;
        }
    }
    private void switchCaptionMode()
    {
        switch (this.CaptionMode)
        {
            case CaptionMode.None:
                this.CaptionMode = SolarMax.CaptionMode.DynamicOnly;
                messageWidget.SetMessage("Show Captions for Orbiting Objects Only");
                break;
            case CaptionMode.DynamicOnly:
                this.CaptionMode = SolarMax.CaptionMode.Auto;
                messageWidget.SetMessage("Auto Caption Mode");
                break;
            default:
                this.CaptionMode = CaptionMode.None;
                messageWidget.SetMessage("Hide Captions");
                break;
        }
    }
    private void switchConstellationSet()
    {
        if (this.ConstellationMode == ConstellationMode.None)
            switchConstellationMode();
        else
            Constellation.UseAltShapes = !Constellation.UseAltShapes;
        
        if (Constellation.UseAltShapes)
            messageWidget.SetMessage("Use Alternate Constellation Patterns");
        else
            messageWidget.SetMessage("Use Traditional Constellation Patterns");
    }

    private void adjustMinBrightness(float Adjustment)
    {
        float oldVal = MinStarBrightness;

        MinStarBrightness = (MinStarBrightness + Adjustment).Clamp(MIN_STAR_BRIGHTNESS_THRESHOLD, MAX_STAR_BRIGHTNESS_THRESHOLD);

        if (!CanViewStars)
            messageWidget.SetMessage("Show No Stars");
        else if (MinStarBrightness >= MAX_STAR_BRIGHTNESS_THRESHOLD)
            messageWidget.SetMessage("Show All Stars");
        else if (MinStarBrightness > oldVal)
            messageWidget.SetMessage(String.Format("Show More Stars (Mag {0:0.0}+)", MinStarBrightness));
        else if (MinStarBrightness < oldVal)
            messageWidget.SetMessage(String.Format("Show Fewer Stars (Mag {0:0.0}+)", MinStarBrightness));
    }

    private void setupGrids()
    {
        this.eclipticGrid = Shape.GetEclipticGrid().Normalized;
        this.equatorialGrid = Shape.GetEquatorialGrid().Normalized;
        this.localGrid = Shape.GetLocalGrid(false).ToResettable();
        this.localGridWithRefraction = Shape.GetLocalGrid(true).ToResettable();
    }

    private LinkedListNode<CelestialBody> lastNode = null;
    
}
