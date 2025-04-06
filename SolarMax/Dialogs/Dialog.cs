namespace SolarMax.Dialogs;

internal abstract class Dialog : Widget
{
    public delegate void CloseCallback();

    protected CloseCallback closeCallback;

    protected IRenderer renderer;
    protected QRectangle rect;
    protected QSize screenSize;
    protected QPen forePen;
    protected QPen borderPen;
    protected QPen backPen;

    protected QPoint titleLoc;
    
    protected Dialog(IRenderer Renderer, QSize ScreenSize, CloseCallback CloseCallback, QPen ForePen, QPen BorderPen, QPen BackPen)
    {
        renderer = Renderer;
        screenSize = ScreenSize;
        closeCallback = CloseCallback;
        forePen = ForePen;
        borderPen = BorderPen;
        backPen = BackPen;

        SetupLayout();
    }
    public void Render(QSize ScreenSize)
    {
        if (ScreenSize != screenSize)
        {
            screenSize = ScreenSize;
            SetupLayout();
        }
        Render();
    }
    public abstract bool SendCommand(QCommand Key);
    public abstract string Message { get; }

    public virtual void Close()
    {
        closeCallback();
    }

    protected abstract void SetupLayout();
}
