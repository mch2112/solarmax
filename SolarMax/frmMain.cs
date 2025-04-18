//#define FRAMERATE

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using SolarMax.Controllers;

namespace SolarMax
{
    public partial class frmMain : Form
    {
        private const int FRAMES_PER_SECOND = 20;
        
        private Controller controller;
        private Point lastMouseLocation = Point.Empty;
        private readonly ScreenSaverMode screenSaverMode;
        private readonly IntPtr screensaverPreviewWindowHandle = IntPtr.Zero;
        private CallbackTimer timer;
        private CallbackTimer.TimerDelegate timerCallback;

        private DrawingSurface drawingSurface;

        public frmMain()
        {
            InitializeComponent();

            //HorizonsDownload.GetData(new DateTime(2011, 2, 19, 12, 0, 0));

            //for (int i = 1951; i <= 1969; i++)
            //{
            //    if (i != 1955 && i != 1960 && i != 1965)
            //        HorizonsDownload.GetData(new DateTime(i, 1, 1, 12, 0, 0));
            //}
            this.screenSaverMode = ScreenSaverMode.Application;

            Setup(Properties.Settings.Default.FullScreen ? FormBorderStyle.None : FormBorderStyle.Sizable);
            this.WindowState = Properties.Settings.Default.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;

            this.MouseCaptured = true;
        }
        public frmMain(Rectangle Bounds)
        {
            InitializeComponent();

            this.screenSaverMode = ScreenSaverMode.ScreenSaver;

            this.Bounds = Bounds;
            this.WindowState = FormWindowState.Maximized;

            Setup(FormBorderStyle.None);

            this.MouseCaptured = true;
        }
        public frmMain(IntPtr PreviewWndHandle)
        {
            InitializeComponent();

            this.screenSaverMode = ScreenSaverMode.ScreenSaverPreview;
            screensaverPreviewWindowHandle = PreviewWndHandle;

            // Set the preview window as the parent of this window
            SetParent(this.Handle, screensaverPreviewWindowHandle);

            // Make this a child window so it will close when the parent dialog closes
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            // Place our window inside the parent
            GetClientRect(screensaverPreviewWindowHandle, out Rectangle ParentRect);
            this.Size = ParentRect.Size;
            this.Location = Point.Empty;

            Setup(FormBorderStyle.None);

            this.MouseCaptured = false;
        }
        private void Setup(FormBorderStyle FormBorderStyle)
        {
            drawingSurface = new DrawingSurface(screenSaverMode)
            {
                Dock = DockStyle.Fill
            };
            drawingSurface.Paint += PaintSurface;
            drawingSurface.MouseClick += MouseClick;
            drawingSurface.MouseMove += MouseMove;
            drawingSurface.MouseWheel += MouseWheel;
            
            this.Controls.Add(drawingSurface);
            
            this.Text = Controller.APPLICATION_NAME;
            this.KeyPreview = true;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle;

            bool freshStart = ModifierKeys == (Keys.Control | Keys.Shift);
            if (freshStart)
                Properties.Settings.Default.Reset();

            this.controller = new Controller(screenSaverMode, ShutDown, FRAMES_PER_SECOND, !freshStart);
        }
        private void LoadForm(object sender, EventArgs e)
        {
            if (this.screenSaverMode == ScreenSaverMode.Application)
            {
                var r = Properties.Settings.Default.ScreenBounds;

                if (r.IsEmpty || !r.IsContainedIn(Screen.PrimaryScreen.WorkingArea))
                    this.Bounds = new Rectangle(Screen.PrimaryScreen.WorkingArea.Left + 20,
                                                Screen.PrimaryScreen.WorkingArea.Top + 20,
                                                Screen.PrimaryScreen.WorkingArea.Width - 40,
                                                Screen.PrimaryScreen.WorkingArea.Height - 40);
                else
                    this.Bounds = r;
            }
            timer = new CallbackTimer();
            timerCallback  = new CallbackTimer.TimerDelegate(InvalidateCallback);
            timer.Create(100, 1000 / FRAMES_PER_SECOND, timerCallback);
        }
        protected override bool ProcessCmdKey(ref Message Message, Keys KeyData)
        {
            bool shift = (KeyData & Keys.Shift) == Keys.Shift;
            bool ctrl = (KeyData & Keys.Control) == Keys.Control;
            bool alt = (KeyData & Keys.Alt) == Keys.Alt;

            var keyData = KeyData & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;

            if (controller.SendCommand(new QCommand(keyData, shift, ctrl, alt)) || base.ProcessCmdKey(ref Message, KeyData))
            {
                return true;
            }
            else
            {
                switch (keyData)
                {
                    case Keys.Tab:
                        if (alt)
                        {
                            // Switching to another app; let the cursor go.
                            MouseCaptured = false;
                            return false;
                        }
                        else
                        {
                            if (screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
                                ToggleFullScreen();
                            return true;
                        }
                    default:
                        return false;
                }
            }
        }
        private void ToggleFullScreen()
        {
            if (this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None)
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.MouseCaptured = false;
                var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
                this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                
                controller?.ShowMessage("Window Mode");
            }
            else
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                
                controller?.ShowMessage("Full Screen Mode");
            }
        }
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.MouseCaptured = false;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (controller != null)
            {
                controller.ScreenSize = new QSize(ClientRectangle.Width, ClientRectangle.Height);
                controller.ZoomOut(AdjustmentAmount.All);
            }
        }

        private void InvalidateCallback(IntPtr pWhat, bool success)
        {
            drawingSurface.Invalidate();
        }
#if FRAMERATE
        private Clock frameRateClock = new Clock();
        private Font frameRateCounterFont = new Font("Consolas", 10f);
        private int frameRate = 0;
#endif
        private int frameCount = 0;
        private bool showCredits = true;
        protected void PaintSurface(object sender, PaintEventArgs e)
        {
            if (screenSaverMode == ScreenSaverMode.ScreenSaverPreview && !IsWindowVisible(screensaverPreviewWindowHandle))
            {
                ShutDown();
                return;
            }
            if (controller != null)
            {
                ++frameCount;
                if (showCredits)
                {
                    showCredits &= frameCount < (FRAMES_PER_SECOND * 10);
                    if (!showCredits)
                        controller.ShowCredits = false;
                }
                controller.Render(e.Graphics);

#if FRAMERATE
                if (frameCount % 10 == 0)
                {
                    frameRate = (int)(10.0 / frameRateClock.Seconds);
                    frameRateClock.Reset();
                }
                e.Graphics.DrawString("Frame Rate: " + frameRate.ToString(), frameRateCounterFont, Brushes.White, new PointF(800, 0));
#endif
            }
        }
        protected override CreateParams CreateParams
        {
            // needed to prevent screensaver hook from being trampled
            get
            {
                CreateParams createParams = base.CreateParams;

                if (!DesignMode && screenSaverMode == ScreenSaverMode.ScreenSaverPreview)
                {
                    createParams.Style |= 0x40000000;
                }

                return createParams;
            }
        }

        private bool shuttingDown = false;
        private void ShutDown()
        {
            if (!shuttingDown)
            {
                shuttingDown = true;
                timer.Delete();
                controller.Cancel();
                Properties.Settings.Default.ScreenBounds = this.Bounds;
                Properties.Settings.Default.FullScreen = this.FormBorderStyle == FormBorderStyle.None;
                Properties.Settings.Default.Save();
                Application.Exit();
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            ShutDown();
        }

        private new void MouseMove(object sender, MouseEventArgs e)
        {
            if (this.MouseCaptured)
            {
                Point p = Cursor.Position;
                if (p != lastMouseLocation)
                {
                    bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                    bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                    bool alt= (ModifierKeys & Keys.Alt) == Keys.Alt;

                    if (p.X != lastMouseLocation.X)
                    {
                        double diff = lastMouseLocation.X - p.X;
                        controller.SendCommand(new QCommand(CommandCode.MouseHorizontal, shift, ctrl, alt, diff));
                    }
                    if (p.Y != lastMouseLocation.Y)
                    {
                        double diff = (double)(lastMouseLocation.Y - p.Y);
                        controller.SendCommand(new QCommand(CommandCode.MouseVertical, shift, ctrl, alt, diff)); 
                    }
                    if (true || this.FormBorderStyle == FormBorderStyle.None) // full screen
                        Cursor.Position = lastMouseLocation = new Point((int)(controller.ScreenSize.Width / 2 + this.Location.X), (int)(controller.ScreenSize.Height / 2 + this.Location.Y));
                    else
                        lastMouseLocation = Cursor.Position;
                }
            }
        }
        private new void MouseWheel(object sender, MouseEventArgs e)
        {
            bool ctrl = ((ModifierKeys & Keys.Control) == Keys.Control);
            bool shift = ((ModifierKeys & Keys.Shift) == Keys.Shift);
            bool alt = ((ModifierKeys & Keys.Alt) == Keys.Alt);

            controller.SendCommand(new QCommand(CommandCode.MouseWheel, shift, ctrl, alt, e.Delta)); 
        }
        private bool mouseCaptured = false;
        private bool MouseCaptured
        {
            get { return mouseCaptured; }
            set
            {
                if (mouseCaptured != value)
                {
                    mouseCaptured = value;
                    lastMouseLocation = Cursor.Position;
                    if (mouseCaptured)
                        Cursor.Hide();
                    else
                        Cursor.Show();
                }
            }
        }
        private new void MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !this.MouseCaptured)
            {
                bool ctrl = ((ModifierKeys & Keys.Control) == Keys.Control);
                bool shift = ((ModifierKeys & Keys.Shift) == Keys.Shift);
                bool alt = ((ModifierKeys & Keys.Alt) == Keys.Alt);

                controller.SendCommand(new QCommand(CommandCode.MouseClick, shift, ctrl, alt, new QPoint(e.Location)));
            }
            else if ((this.MouseCaptured || (e.Button == MouseButtons.Right)) && screenSaverMode != ScreenSaverMode.ScreenSaverPreview)
            {
                this.MouseCaptured = !this.MouseCaptured;
                controller?.ShowMessage("Mouse Pan and Zoom " + (this.MouseCaptured ? "On" : "Off"));
            }
        }
        public void FormActivated(object sender, EventArgs e)
        {
            controller?.ResetPanningAdjustments();
        }

        #region Win32 API functions

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        #endregion

    }
}