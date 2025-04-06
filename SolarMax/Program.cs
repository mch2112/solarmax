using System;
using System.Windows.Forms;

namespace SolarMax;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (processIsRunning(System.Diagnostics.Process.GetCurrentProcess().ProcessName))
        {
            MessageBox.Show(Controller.APPLICATION_NAME + " is already running.", Controller.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Length > 0)
        {
            string firstArgument = args[0].ToLower().Trim();
            string secondArgument = null;

            if (firstArgument.Length > 2)
            {
                secondArgument = firstArgument[3..].Trim();
                firstArgument = firstArgument[..2];
            }
            else if (args.Length > 1)
                secondArgument = args[1];

            switch (firstArgument)
            {
                case "/c":
                    // Configuration mode
                    //Application.Run(new SettingsForm());
                    break;
                case "/p":
                    // Preview mode
                    if (secondArgument == null)
                    {
                        MessageBox.Show("Preview window handle missing.", Controller.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        Application.Run(new frmMain(new nint(long.Parse(secondArgument))));
                    }
                    break;
                case "/s":
                    // Full-screen mode
                    runAsScreenSaver();
                    Application.Run();
                    break;
                default:
                    // Undefined argument
                    MessageBox.Show("Invalid command line argument \"" + firstArgument + "\" is not valid.", Controller.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }
        }
        else
        {
            runAsApplication();
        }
    }
    private static void runAsScreenSaver()
    {
        Application.Run(new frmMain(Screen.PrimaryScreen.Bounds));
        //foreach (Screen screen in Screen.AllScreens)
        //{
        //    ScreenSaverForm screensaver = new ScreenSaverForm(screen.Bounds);
        //    screensaver.Show();
        //}
    }
    private static void runAsApplication()
    {
        Application.Run(new frmMain());
    }
    private static bool processIsRunning(string process)
    {
        return System.Diagnostics.Process.GetProcessesByName(process).Length >= 2;
    }
}