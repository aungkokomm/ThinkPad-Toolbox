using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEDControl
{
    static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDPIAware();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            // The driver chooser no longer pops up on first run - WinRing0 is the default and
            // works out of the box, and the "Choose driver" button changes it any time. Power
            // users can still force the chooser by holding Shift while launching.
            if (Control.ModifierKeys == Keys.Shift && !Environment.GetCommandLineArgs().Contains("driver"))
            {
                Welcome w = new Welcome();
                if (w.ShowDialog() == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
            }

            Application.Run(new Form1());
        }
    }
}
