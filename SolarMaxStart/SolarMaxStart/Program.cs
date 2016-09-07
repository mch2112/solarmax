using System;
using System.Windows.Forms;

namespace SolarMaxStart
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!isFrameworkOK())
            {
                if (MessageBox.Show("This program requires Microsoft .Net Framework 4 (Client Profile). Click OK to go download this file from microsoft.com",
                                    ".Net Framework 4",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start(@"http://www.microsoft.com/downloads/en/details.aspx?displaylang=en&FamilyID=5765d7a8-7722-4888-a970-ac39b33fd8ab");
                }
                return;
            }
            else
            {
                System.Diagnostics.Process.Start(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "SolarMax.exe"));
            }
        }

        private static bool isFrameworkOK()
        {
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client");
            return regKey != null && regKey.GetValue("Install", "0").ToString() == "1";
        }
    }
}
