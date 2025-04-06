namespace SolarMax;

internal class InstrumentDataSource(Controller Controller, Physics Physics, Camera Camera, Projector Projector)
{
    public Controller Controller { get; private set; } = Controller;
    public Physics Physics { get; private set; } = Physics;
    public Camera Camera { get; private set; } = Camera;
    public Projector Projector { get; private set; } = Projector;
    public Vector CameraPosition { get; set; }
    public Vector CameraView { get; set; }
    public Vector CameraUp { get; set; }
    public double CameraZoom { get; set; }
    public QSize ScreenSize { get; set; }

    public void Update()
    {
        ScreenSize = Controller.ScreenSize;

        CameraPosition = Camera.Position;
        CameraView = Projector.PanView;
        CameraUp = Projector.PanUp;
        CameraZoom = Camera.Zoom;
    }
}
