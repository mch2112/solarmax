using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SolarMax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double FRAMES_PER_SECOND = 20;

        private Controller controller;
        
        private ScreenSaverMode screenSaverMode;
        private CallbackTimer timer;
        private CallbackTimer.WaitOrTimerDelegate timerCallback;
        
        private WriteableBitmap renderTarget = null; 
        private Image renderTargetContainer = null;

        private Point lastMouseLocation = new Point();

        private Canvas textCanvas;

        public MainWindow()
        {
            InitializeComponent();

            this.screenSaverMode = ScreenSaverMode.Application;

            this.MouseCaptured = true;

            setup();
        }
        private void setup()
        {
            controller = new Controller(this.screenSaverMode, true);

            textCanvas = new Canvas();
            Grid.SetZIndex(textCanvas, 100000);
            grdMain.Children.Add(textCanvas);

            timer = new CallbackTimer();
            timerCallback = new CallbackTimer.WaitOrTimerDelegate(render);
            timer.Create(100, (int)(1000.0 / FRAMES_PER_SECOND), timerCallback);
        }
        private void render(IntPtr lpParameter, bool timerOrWaitFired)
        {
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal, new Action(() => render()));
        }
        private void render()
        {
            if (renderTarget != null)
            {
                renderTarget.Lock();
                renderTarget.Clear();
                controller.Render(renderTarget);
                renderTarget.Unlock();
                textCanvas.Children.Clear();
                foreach (var t in controller.PendingText)
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = t.Item1;
                    if (t.Item5)
                    {
                        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        Canvas.SetLeft(tb, t.Item2.X -tb.DesiredSize.Width / 2);
                    }
                    else
                    {
                        Canvas.SetLeft(tb, t.Item2.X);
                    }
                    Canvas.SetTop(tb, t.Item2.Y);
                    tb.FontFamily = t.Item4.FontFamily;
                    tb.Foreground = t.Item3.Brush;
                    tb.Background = Brushes.Transparent;
                    tb.FontStyle = t.Item5 ? FontStyles.Italic : FontStyles.Normal;
                    textCanvas.Children.Add(tb);
                }
                controller.PendingText.Clear();
            }
        }
        private void setupBitmap(Size Size)
        {
            controller.ScreenSize = new QSize(Size.Width, Size.Height);

            int w = (int)grdMain.ActualWidth;
            int h = (int)grdMain.ActualHeight;

            BitmapEx.SetSize(w, h);
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);

            Image image = new Image();
            image.Source = wb;
            image.SnapsToDevicePixels = true;
            grdMain.Children.Add(image);

            if (renderTargetContainer != null)
                grdMain.Children.Remove(renderTargetContainer);

            renderTargetContainer = image;
            renderTarget = wb;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool alt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

            var keyData = e.Key;

            if (shift && keyData.ToString().Length == 1)
            {
                controller.FindTarget(keyData.ToString(), !alt);
                return;
            }
            if (ctrl && keyData.ToString().Length == 1)
            {
                controller.FindHome(keyData.ToString(), !alt);
                return;
            }

            switch (keyData)
            {
                case Key.Escape:
                    if (controller.InLatLongAdjustMode)
                        controller.InLatLongAdjustMode = false;
                    else if (controller.ShowHelp)
                        controller.ShowHelp = false;
                    else if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
                        shutDown();
                    break;
                case Key.Tab:
                    if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
                        toggleFullScreen();
                    break;;
                case Key.Back:
                    controller.InLatLongAdjustMode = !controller.InLatLongAdjustMode;
                    break;
                case Key.Left:
                    if (controller.InLatLongAdjustMode)
                        controller.SelectLatLong(false);
                    else
                        controller.SetViewTarget(-1);
                    break;
                case Key.Right:
                    if (controller.InLatLongAdjustMode)
                        controller.SelectLatLong(true);
                    else
                        controller.SetViewTarget(1);
                    break;
                case Key.Up:
                    if (controller.InLatLongAdjustMode)
                        controller.AdjustLatLong(true);
                    else
                        controller.SetHome(1);
                    break;
                case Key.Down:
                    if (controller.InLatLongAdjustMode)
                        controller.AdjustLatLong(false);
                    else
                        controller.SetHome(-1);
                    break;
                case Key.Enter:
                    if (controller.InLatLongAdjustMode)
                        controller.InLatLongAdjustMode = false;
                    else
                        controller.Settle();
                    break;

                case Key.D1:
                    controller.ViewMode = ViewMode.Ecliptic;
                    break;
                case Key.D2:
                    controller.ViewMode = ViewMode.Surface;
                    break;
                case Key.D3:
                    controller.ViewMode = ViewMode.TopDown;
                    break;
                case Key.D4:
                    controller.ViewMode = ViewMode.Follow;
                    break;

                case Key.F1:
                    controller.ShowHelp = !controller.ShowHelp;
                    break;
                case Key.F2:
                    controller.ShowEclipticGrid = !controller.ShowEclipticGrid;
                    break;
                case Key.F3:
                    controller.ShowEquatorialGrid = !controller.ShowEquatorialGrid;
                    break;
                case Key.F4:
                    controller.ShowLocalGrid = !controller.ShowLocalGrid;
                    break;
                case Key.F5:
                    controller.SwitchCaptionMode();
                    break;
                case Key.F6:
                    controller.WireFrameBodyRender = !controller.WireFrameBodyRender;
                    break;
                case Key.F7:
                    controller.AdjustMinBrightness(-1f);
                    break;
                case Key.F8:
                    controller.AdjustMinBrightness(1f);
                    break;
                case Key.F9:
                    controller.SwitchConstellationMode();
                    break;
                case Key.F10:
                    controller.SwitchConstellationSet();
                    break;
                case Key.F11:
                    controller.ShowData = !controller.ShowData;
                    break;
                case Key.F12:
                    controller.ShowInstruments = !controller.ShowInstruments;
                    break;
                case Key.PageDown:
                    controller.ZoomOut(false);
                    break;
                case Key.PageUp:
                    controller.ZoomIn(false);
                    break;
                case Key.End:
                    controller.ZoomOut(true);
                    break;
                case Key.Home:
                    controller.ZoomIn(true);
                    break;
                case Key.P:
                    controller.UserPaused = !controller.UserPaused;
                    break;

                case Key.T:
                    controller.GoToToday();
                    break;
                case Key.Z:
                    controller.SlowDown();
                    break;
                case Key.X:
                    controller.SpeedUp();
                    break;
                case Key.C:
                    controller.ReverseTime();
                    break;
                case Key.R:
                    controller.Swap();
                    break;
                case Key.A:
                    controller.PanLeft();
                    break;
                case Key.D:
                    controller.PanRight();
                    break;
                case Key.W:
                    controller.PanUp();
                    break;
                case Key.S:
                    controller.PanDown();
                    break;
                case Key.Q:
                    controller.TiltLeft();
                    break;
                case Key.E:
                    controller.TiltRight();
                    break;
                case Key.Space:
                    controller.ResetAdjustments();
                    break;
                case Key.OemTilde:
                    switch (controller.Projection)
                    {
                        case ProjectionMode.Stereographic:
                            controller.Projection = ProjectionMode.Cylindrical;
                            break;
                        case ProjectionMode.Cylindrical:
                            controller.Projection = ProjectionMode.Orthographic;
                            break;
                        default:
                            controller.Projection = ProjectionMode.Stereographic;
                            break;
                    }
                    break;
                case Key.OemPipe:
                    controller.DisplayTimeUTC = !controller.DisplayTimeUTC;
                    break;
                case Key.Insert:
                    controller.HighQualityRender = !controller.HighQualityRender;
                    break;
                
            }
        }
        private void toggleFullScreen()
        {
        }
        private void shutDown()
        {
            controller.Cancel();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            shutDown();
        }
        private bool mouseCaptured = true;
        private bool MouseCaptured
        {
            get { return mouseCaptured; }
            set
            {
                mouseCaptured = value;
                if (mouseCaptured)
                {
                    lastMouseLocation = Mouse.GetPosition(this);
                    Mouse.OverrideCursor = Cursors.None;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.MouseCaptured)
            {
                Point p = e.GetPosition(this);
                if (p != lastMouseLocation)
                {
                    bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                    bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

                    if (p.X != lastMouseLocation.X)
                    {
                        double diff = (double)(lastMouseLocation.X - p.X) / 300.0;
                        if (shift)
                            controller.Tilt(diff);
                        else if (!ctrl)
                            controller.PanHorizontal(diff);
                    }
                    if (p.Y != lastMouseLocation.Y)
                    {
                        double diff = (double)(lastMouseLocation.Y - p.Y) / 300.0;
                        if (ctrl)
                            controller.Zoom(diff);
                        else if (!shift)
                            controller.PanVertical(diff);
                    }
                    //Mouse. Cursor.Position = lastMouseLocation = new Point((int)(controller.ScreenSize.Width / 2), (int)(controller.ScreenSize.Height / 2));
                    lastMouseLocation = p;
                }
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
            {
                this.MouseCaptured = !this.MouseCaptured;
                if (controller != null)
                    controller.ShowMessage("Mouse Pan and Zoom " + (this.MouseCaptured ? "On" : "Off"));
            }
        }

        private void grdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setupBitmap(e.NewSize);
        }
    }
}
