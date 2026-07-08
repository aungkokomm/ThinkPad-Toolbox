using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.ApplicationServices;

namespace LEDControl
{
    public partial class Form1 : Form
    {

        #region constants
        const int EC_DATAPORT = 0x62;
        const int EC_CTRLPORT = 0x66;
        const int EC_STAT_OBF = 0x01;    // Output buffer full
        const int EC_STAT_IBF = 0x02;    // Input buffer full
        const int EC_STAT_CMD = 0x08;
        const byte EC_CTRLPORT_READ = 0x80;
        const byte EC_CTRLPORT_WRITE = 0x81;
        const byte EC_CTRLPORT_QUERY = 0x84;
        const byte TP_ECOFFSET_FAN = 0x2F;  // 1 byte (binary xyzz zzz)
        const byte TP_ECOFFSET_FANSPEED = 0x84; // 16 bit word, lo/hi byte
        const int TP_ECOFFSET_TEMP0 = 0x78;    // 8 temp sensor bytes from here
        const int TP_ECOFFSET_TEMP1 = 0xC0; // 4 temp sensor bytes from here

        const byte TP_LED_OFFSET = 0x0C;

        // Fan control (register HFSP at TP_ECOFFSET_FAN = 0x2F).
        const byte FAN_AUTO = 0x80;    // firmware/BIOS controls the fan (safe default)
        const byte FAN_FULL = 0x40;    // "disengaged": spins at maximum
        #endregion

        #region EC_access_methods
        bool waitportstatus(int bits, bool onoff = false, int timeout = 100)
        {
            ushort port = EC_CTRLPORT;
            int tick = 5;
            //
            // Wait until the control port reaches the desired state, or time out.
            // Returning false on timeout is important: it means the EC is busy (Windows
            // is likely mid-transaction), so the caller aborts rather than colliding.
            //
            for (int time = 0; time < timeout; time += tick)
            {
                byte data = 0;
                try
                {
                    if (activeDriver == 0)
                        data = ols.ReadIoPortByte(port);
                    else if (activeDriver == 1)
                        data = TVicPort.ReadPort(port);
                    else if (activeDriver == 2)
                        data = PawnIoDriver.ReadPort(port);
                }
                catch
                {
                    return false;
                }

                bool flagstate = (((char)data) & bits) != 0,
                    wantedstate = onoff != false;

                if (flagstate == wantedstate)
                    return true;

                System.Threading.Thread.Sleep(tick);
            }
            return false;   // timed out: EC did not become ready
        }

        bool writeport(ushort port, byte data)
        {
            // write byte via WINIO.SYS
            try
            {
                if (activeDriver == 0)
                    ols.WriteIoPortByte(port, data);
                else if (activeDriver == 1)
                    TVicPort.WritePort(port, data);
                else if (activeDriver == 2)
                    PawnIoDriver.WritePort(port, data);
            }
            catch
            {
                return false;
            }
            return true;
        }

        bool readport(ushort port, ref byte pdata)
        {
            byte data = 0;
            try
            {
                if (activeDriver == 0)
                    data = ols.ReadIoPortByte(port);
                else if (activeDriver == 1)
                    data = TVicPort.ReadPort(port);
                else if (activeDriver == 2)
                    data = PawnIoDriver.ReadPort(port);
                pdata = data;
            }
            catch
            {
                return false;
            }
            return true;
        }

        // All EC access goes through a single lock: the embedded-controller port
        // handshake is a multi-step transaction that must not interleave between
        // the UI thread (button clicks) and the background keyboard-backlight worker.
        readonly object ecLock = new object();

        // Windows and other EC tools (e.g. TPFanControl) coordinate access to the embedded
        // controller through this shared named mutex, so they take turns instead of
        // corrupting each other's (and the OS's battery/thermal) transactions.
        static readonly System.Threading.Mutex ecMutex = OpenEcMutex();
        static System.Threading.Mutex OpenEcMutex()
        {
            try { return new System.Threading.Mutex(false, "Global\\Access_EC"); }
            catch { try { return new System.Threading.Mutex(false, "Access_EC"); } catch { return null; } }
        }
        static bool EcAcquire()
        {
            if (ecMutex == null) return false;
            try { return ecMutex.WaitOne(200); }
            catch (System.Threading.AbandonedMutexException) { return true; } // previous owner died; we hold it now
            catch { return false; }
        }
        static void EcRelease(bool held)
        {
            if (held && ecMutex != null) { try { ecMutex.ReleaseMutex(); } catch { } }
        }

        bool ReadByteFromEC(byte offset, ref byte pdata)
        {
            bool held = EcAcquire();
            try { lock (ecLock) { return ReadByteFromECInner(offset, ref pdata); } }
            finally { EcRelease(held); }
        }
        bool ReadByteFromECInner(byte offset, ref byte pdata)
        {
            bool ok;

            // wait for IBF and OBF to clear
            ok = waitportstatus(EC_STAT_IBF | EC_STAT_OBF, false);
            if (ok)
            {

                // tell 'em we want to "READ"
                ok = writeport(EC_CTRLPORT, EC_CTRLPORT_READ);
                if (ok)
                {

                    // wait for IBF to clear (command byte removed from EC's input queue)
                    ok = waitportstatus(EC_STAT_IBF, false);
                    if (ok)
                    {

                        // tell 'em where we want to read from
                        ok = writeport(EC_DATAPORT, offset);
                        if (ok)
                        {

                            // wait for IBF to clear (address byte removed from EC's input queue)
                            // Note: Techically we should waitportstatus(OBF,TRUE) here,(a byte being
                            //       in the EC's output buffer being ready to read).  For some reason
                            //       this never seems to happen
                            ok = waitportstatus(EC_STAT_IBF, false);
                            if (ok)
                            {
                                byte data = 0xFF;

                                // read result (EC byte at offset)
                                ok = readport(EC_DATAPORT, ref data);
                                if (ok)
                                    pdata = data;
                            }
                        }
                    }
                }
            }
            return ok;
        }

        bool WriteByteToEC(byte offset, byte data)
        {
            bool held = EcAcquire();
            try { lock (ecLock) { return WriteByteToECInner(offset, data); } }
            finally { EcRelease(held); }
        }
        bool WriteByteToECInner(byte offset, byte data)
        {
            bool ok;

            // wait for IBF and OBF to clear
            ok = waitportstatus(EC_STAT_IBF | EC_STAT_OBF, false);
            if (ok)
            {

                // tell 'em we want to "WRITE"
                ok = writeport(EC_CTRLPORT, EC_CTRLPORT_WRITE);
                if (ok)
                {

                    // wait for IBF to clear (command byte removed from EC's input queue)
                    ok = waitportstatus(EC_STAT_IBF, false);
                    if (ok)
                    {

                        // tell 'em where we want to write to
                        ok = writeport(EC_DATAPORT, offset);
                        if (ok)
                        {

                            // wait for IBF to clear (address byte removed from EC's input queue)
                            ok = waitportstatus(EC_STAT_IBF, false);
                            if (ok)
                            {
                                // tell 'em what we want to write there
                                ok = writeport(EC_DATAPORT, data);
                                if (ok)
                                {
                                    // wait for IBF to clear (data byte removed from EC's input queue)
                                    ok = waitportstatus(EC_STAT_IBF, false);
                                }
                            }
                        }
                    }
                }
            }
            return ok;
        }
        #endregion

        #region ToggleLEDs
        public enum LEDs
        {
            Power, Microphone, RedDot, Sleep, Fn
        }
        public enum PowerStates
        {
            On, Off, Blink
        }

        // Remember the last state the user set for each named LED, so we can re-apply it after
        // Windows/firmware resets the status LEDs on lock/unlock or sleep/resume.
        readonly object ledDesiredLock = new object();
        readonly Dictionary<LEDs, PowerStates> ledDesired = new Dictionary<LEDs, PowerStates>();

        bool LED(LEDs which, PowerStates what, bool mayOverride = true)
        {
            byte led = 0xFF;
            byte power = 0xFF;
            switch (which)
            {
                case LEDs.Power:
                    led = 0x00;
                    break;
                case LEDs.Microphone:
                    led = 0x0E;
                    break;
                case LEDs.RedDot:
                    led = 0x0A;
                    break;
                case LEDs.Sleep:
                    led = 0x07;
                    break;
                case LEDs.Fn:
                    led = 0x06;
                    break;
            }
            switch (what)
            {
                case PowerStates.On:
                    power = 0x80;
                    break;
                case PowerStates.Off:
                    power = 0x00;
                    break;
                case PowerStates.Blink:
                    power = 0xC0;
                    break;
            }
            lock (ledDesiredLock) { ledDesired[which] = what; }
            byte _out = (byte)(led | power);
            return WriteByteToEC(TP_LED_OFFSET, _out);
        }

        bool LED2(byte led, PowerStates what)
        {
            byte power = 0xFF;
            switch (what)
            {
                case PowerStates.On:
                    power = 0x80;
                    break;
                case PowerStates.Off:
                    power = 0x00;
                    break;
                case PowerStates.Blink:
                    power = 0xC0;
                    break;
            }
            byte _out = (byte)(led | power);
            return WriteByteToEC(TP_LED_OFFSET, _out);
        }

        // Re-apply the LED states the user chose. The ThinkPad firmware resets the status LEDs
        // to its own defaults on lock/unlock and sleep/resume, so we restore the user's choice
        // afterwards. EC writes are guarded (mutex + timeout); if the EC is busy the write just
        // aborts, so this cannot corrupt anything.
        void ReapplyLeds()
        {
            KeyValuePair<LEDs, PowerStates>[] snapshot;
            lock (ledDesiredLock)
            {
                if (ledDesired.Count == 0) return;
                snapshot = ledDesired.ToArray();
            }
            foreach (var kv in snapshot)
            {
                try { LED(kv.Key, kv.Value); } catch { }
            }
        }

        // Fires an immediate re-apply plus one a beat later, because the firmware can set the
        // LEDs just after the unlock/resume event and we want to win that race.
        void ReapplyLedsAfterWake()
        {
            ReapplyLeds();
            System.Threading.Tasks.Task.Run(async () =>
            {
                try { await System.Threading.Tasks.Task.Delay(1200); ReapplyLeds(); } catch { }
            });
        }
        #endregion

        #region Settings
        void ReadSettingsKBD()
        {
            if (rememberKBD.Checked)
            {
                switch (Properties.Settings.Default.KBDLevel)
                {
                    case 0:
                        SetKeyboardLevel(LightLevel.Off);
                        break;
                    case 1:
                        SetKeyboardLevel(LightLevel.Low);
                        break;
                    case 2:
                        SetKeyboardLevel(LightLevel.High);
                        break;
                }
            }

        }
        void SaveSettingsKBD()
        {
            if (rememberKBD.Checked)
            {
                LightLevel p = GetKeyboardLightlevel();
                Console.WriteLine(p);
                switch (p)
                {
                    case LightLevel.Off:
                        Properties.Settings.Default.KBDLevel = 0;
                        break;
                    case LightLevel.Low:
                        Properties.Settings.Default.KBDLevel = 1;
                        break;
                    case LightLevel.High:
                        Properties.Settings.Default.KBDLevel = 2;
                        break;
                    case LightLevel.Unknown:
                        Properties.Settings.Default.KBDLevel = -1;
                        break;
                }
            }
            Properties.Settings.Default.Save();
        }
        void ReadSettings()
        {
            rememberKBD.Checked = Properties.Settings.Default.RememberKBD;
            checkTurnKBLightOff.Checked = Properties.Settings.Default.LightOffWhileFS;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.RememberKBD = rememberKBD.Checked;
            Properties.Settings.Default.LightOffWhileFS = checkTurnKBLightOff.Checked;
            Properties.Settings.Default.Save();
        }
        #endregion

        bool hide_me = false;
        int fanMode = 0;            // 0 = auto (firmware), 1 = full

        // Single-instance guard.
        static System.Threading.Mutex instanceMutex;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int RegisterWindowMessage(string message);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        static readonly int WM_SHOWTPTOOLBOX = RegisterWindowMessage("WM_SHOW_ThinkPadToolbox_5F3A");
        const int HWND_BROADCAST = 0xffff;

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
        public Form1()
        {
            InitializeComponent();
            this.Icon = LEDControl.Properties.Resources.AppIcon;
            SystemEvents.PowerModeChanged += OnPowerChange;
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.WindowState == FormWindowState.Minimized) this.Hide();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            // Only read the EC for temperature / fan RPM while the window is actually on
            // screen. When it lives in the tray, stop polling to keep EC traffic (and any
            // chance of colliding with Windows' battery/thermal management) to a minimum.
            if (statusTimer != null) statusTimer.Enabled = this.Visible;
        }

        Ols ols;
        // The driver actually initialized this run. The Driver *setting* can change
        // before shutdown (e.g. the user picks a different driver in the Choose-driver
        // dialog, which relaunches), so runtime EC dispatch and cleanup must use this,
        // not Properties.Settings.Default.Driver.
        int activeDriver = -1;
        const string error_loading_driver_tvicport = "There was an error loading the TVicPort driver. Please reinstall the application or reboot the machine.\r\n\r\nAlternatively, try running the application using the WinRing0 driver. Hold down [SHIFT] while launching the application in order to select another driver to run the application with.";
        const string error_loading_driver_winring = "The WinRing0 driver could not be loaded.\r\n\r\nOn Windows 11 with Memory Integrity (Core Isolation) turned on, WinRing0 is blocked by Windows. The fix is to install PawnIO - a modern, signed driver that works with Memory Integrity - and switch this app to it:\r\n\r\n1) Install PawnIO from https://pawnio.eu\r\n2) Hold [SHIFT] while launching this app and choose the PawnIO driver.\r\n\r\nAlternatively, reboot the machine, or turn Memory Integrity off, to keep using WinRing0.";
        const string error_loading_driver_pawnio = "There was an error loading the PawnIO driver or its embedded-controller module. Please make sure PawnIO is installed and up to date.\r\n\r\nHold down [SHIFT] while launching the application in order to select another driver to run the application with.";
        const string pawnio_not_installed = "This application is set to use PawnIO, but the PawnIO driver does not appear to be installed on this machine.\r\n\r\nPawnIO is a modern, signed kernel driver that works with Windows Memory Integrity enabled (unlike WinRing0).\r\n\r\nWould you like to open the PawnIO download page now? After installing it, launch this application again.";
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string[] cmd = Environment.GetCommandLineArgs();
            string prev = "";
            foreach (string s in cmd)
            {
                switch (s)
                {
                    case "0":
                        if (prev == "driver")
                        {
                            Properties.Settings.Default.Driver = 0;
                            Properties.Settings.Default.Save();
                        }
                        break;
                    case "1":
                        if (prev == "driver")
                        {
                            Properties.Settings.Default.Driver = 1;
                            Properties.Settings.Default.Save();
                        }
                        break;
                    case "2":
                        if (prev == "driver")
                        {
                            Properties.Settings.Default.Driver = 2;
                            Properties.Settings.Default.Save();
                        }
                        break;
                }
                prev = s;
            }

            // WinRing0 and PawnIO both require administrative privileges. Relaunch
            // elevated automatically - the Windows UAC prompt still appears, but the
            // application no longer shows its own extra warning dialog first.
            if (Properties.Settings.Default.Driver == 0 || Properties.Settings.Default.Driver == 2)
            {
                if (!IsAdministrator())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = Application.ExecutablePath;
                    startInfo.Verb = "runas";
                    string args = "";
                    for (int i = 1; i < cmd.Length; i++)
                    {
                        if (i == cmd.Length - 1) args += cmd[i];
                        else args += cmd[i] + " ";
                    }
                    startInfo.Arguments = args;

                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Environment.Exit(0);
                    }
                }
            }

            // Single instance: only one running copy at a time. Multiple copies would each
            // poll and drive the embedded controller, multiplying the risk of colliding with
            // Windows. If a copy is already running, ask it to show its window and quit this one.
            bool createdNew;
            instanceMutex = new System.Threading.Mutex(true, "ThinkPadToolbox_SingleInstance_5F3A", out createdNew);
            if (!createdNew)
            {
                if (WM_SHOWTPTOOLBOX != 0) PostMessage((IntPtr)HWND_BROADCAST, WM_SHOWTPTOOLBOX, IntPtr.Zero, IntPtr.Zero);
                Environment.Exit(0);
            }

            if (Properties.Settings.Default.Driver == 0)
            {
                ols = new Ols();
                if (ols.GetStatus() != (uint)Ols.Status.NO_ERROR)
                {
                    MessageBox.Show(error_loading_driver_winring, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            else if (Properties.Settings.Default.Driver == 1)
            {
                TVicPort.OpenTVicPort();
                if (TVicPort.IsDriverOpened() == 0)
                {
                    MessageBox.Show(error_loading_driver_tvicport, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            else if (Properties.Settings.Default.Driver == 2)
            {
                if (!PawnIoDriver.Open())
                {
                    if (PawnIoDriver.DeviceMissing)
                    {
                        if (MessageBox.Show(pawnio_not_installed, "ThinkPad Toolbox", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {
                            try { Process.Start(new ProcessStartInfo("https://pawnio.eu/") { UseShellExecute = true }); }
                            catch { }
                        }
                    }
                    else
                    {
                        MessageBox.Show(error_loading_driver_pawnio + "\r\n\r\n" + PawnIoDriver.LastError, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Environment.Exit(0);
                }
            }

            activeDriver = Properties.Settings.Default.Driver;

            foreach (string s in cmd)
            {
                switch (s)
                {
                    case "minimize":
                        this.WindowState = FormWindowState.Minimized;
                        hide_me = true;
                        timer1.Enabled = true;
                        break;
                    case "exit":
                        wrapup();
                        Environment.Exit(0);
                        break;
                    case "on":
                        switch (prev)
                        {
                            case "LEDPower":
                                LED(LEDs.Power, PowerStates.On);
                                break;
                            case "LEDRedDot":
                                LED(LEDs.RedDot, PowerStates.On);
                                break;
                            case "LEDMicrophone":
                                LED(LEDs.Microphone, PowerStates.On);
                                break;
                            case "LEDSleep":
                                LED(LEDs.Sleep, PowerStates.On);
                                break;
                            case "LEDFnLock":
                                LED(LEDs.Fn, PowerStates.On);
                                break;
                            default:
                                LED2(byte.Parse(prev), PowerStates.On);
                                break;
                        }
                        break;
                    case "off":
                        switch (prev)
                        {
                            case "LEDPower":
                                LED(LEDs.Power, PowerStates.Off);
                                break;
                            case "LEDRedDot":
                                LED(LEDs.RedDot, PowerStates.Off);
                                break;
                            case "LEDMicrophone":
                                LED(LEDs.Microphone, PowerStates.Off);
                                break;
                            case "LEDSleep":
                                LED(LEDs.Sleep, PowerStates.Off);
                                break;
                            case "LEDFnLock":
                                LED(LEDs.Fn, PowerStates.Off);
                                break;
                            default:
                                LED2(byte.Parse(prev), PowerStates.Off);
                                break;
                        }
                        break;
                    case "third":
                        switch (prev)
                        {
                            case "LEDPower":
                                LED(LEDs.Power, PowerStates.Blink);
                                break;
                            case "LEDRedDot":
                                LED(LEDs.RedDot, PowerStates.Blink);
                                break;
                            case "LEDMicrophone":
                                LED(LEDs.Microphone, PowerStates.Blink);
                                break;
                            case "LEDSleep":
                                LED(LEDs.Sleep, PowerStates.Blink);
                                break;
                            case "LEDFnLock":
                                LED(LEDs.Fn, PowerStates.Blink);
                                break;
                            default:
                                LED2(byte.Parse(prev), PowerStates.Blink);
                                break;
                        }
                        break;
                }
                prev = s;
            }

            ReadSettings();

            PowerManager.IsMonitorOnChanged += new EventHandler(MonitorOnChanged);

            RegisterForPowerNotifications();

            ReadSettingsKBD();

            if (rememberKBD.Checked) lightTimer.Enabled = true;

            NotifyIcon1.Icon = LEDControl.Properties.Resources.AppIcon;
            NotifyIcon1.Text = "ThinkPad Toolbox";

            StartStatusReadout();
            RefreshFanHighlight();
        }

        #region Toggle_buttons
        const string error_text_buttons = "There was an error setting the LED. Probably the driver is not installed, or some other bad thing happened.";
        private void powerOn_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Power, PowerStates.On)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void powerOff_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Power, PowerStates.Off)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void powerBlink_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Power, PowerStates.Blink)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void dotOn_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.RedDot, PowerStates.On)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void dotOff_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.RedDot, PowerStates.Off)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void dotBlink_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.RedDot, PowerStates.Blink)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void microphoneOn_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Microphone, PowerStates.On)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void microphoneOff_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Microphone, PowerStates.Off)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void microphoneBlink_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Microphone, PowerStates.Blink)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void sleepOn_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Sleep, PowerStates.On)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void sleepOff_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Sleep, PowerStates.Off)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void sleepBlink_Click(object sender, EventArgs e)
        {
            if (!LED(LEDs.Sleep, PowerStates.Blink)) MessageBox.Show(error_text_buttons, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Fan
        const string error_text_fan = "There was an error setting the fan. The driver may not be loaded, or this ThinkPad model may not support fan control through the embedded controller.";

        private void fanAuto_Click(object sender, EventArgs e)
        {
            if (!WriteByteToEC(TP_ECOFFSET_FAN, FAN_AUTO))
            {
                MessageBox.Show(error_text_fan, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            fanMode = 0;
            RefreshFanHighlight();
        }

        private void fanFull_Click(object sender, EventArgs e)
        {
            if (!WriteByteToEC(TP_ECOFFSET_FAN, FAN_FULL))
            {
                MessageBox.Show(error_text_fan, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            fanMode = 1;
            RefreshFanHighlight();
        }
        #endregion

        #region KeyboardBacklight
        private void kbOff_Click(object sender, EventArgs e) { SetKeyboardLevel(LightLevel.Off); RefreshKeyboardHighlight(LightLevel.Off); }
        private void kbLow_Click(object sender, EventArgs e) { SetKeyboardLevel(LightLevel.Low); RefreshKeyboardHighlight(LightLevel.Low); }
        private void kbHigh_Click(object sender, EventArgs e) { SetKeyboardLevel(LightLevel.High); RefreshKeyboardHighlight(LightLevel.High); }
        #endregion

        #region StatusReadout
        int statusBusy = 0;
        string batteryHealthCached = "";

        void StartStatusReadout()
        {
            // Battery health/cycle count barely change; query once in the background and cache.
            System.Threading.Tasks.Task.Run(() => { try { batteryHealthCached = BatteryInfo.HealthLine(); } catch { } });
            statusTimer.Enabled = true;
        }

        // Hottest valid temperature sensor (bytes 0x78..0x7F, degrees C).
        int ReadCpuTemp()
        {
            int max = 0;
            for (byte off = TP_ECOFFSET_TEMP0; off < TP_ECOFFSET_TEMP0 + 8; off++)
            {
                byte t = 0;
                if (ReadByteFromEC(off, ref t))
                {
                    if (t > 0 && t < 128 && t > max) max = t;
                }
            }
            return max;
        }

        // Current fan speed in RPM (16-bit word, lo/hi at 0x84/0x85).
        int ReadFanRpm()
        {
            byte lo = 0, hi = 0;
            ReadByteFromEC(TP_ECOFFSET_FANSPEED, ref lo);
            ReadByteFromEC((byte)(TP_ECOFFSET_FANSPEED + 1), ref hi);
            return lo | (hi << 8);
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            // Read the EC and battery off the UI thread so the readout never blocks it.
            if (System.Threading.Interlocked.CompareExchange(ref statusBusy, 1, 0) != 0) return;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    int temp = ReadCpuTemp();
                    int rpm = ReadFanRpm();
                    string power = BatteryInfo.PowerLine();
                    string health = batteryHealthCached;
                    LightLevel kb = GetKeyboardLightlevel();

                    string sensors = "CPU " + (temp > 0 ? temp + "°C" : "--") + "      Fan " + rpm + " RPM";
                    string battery = power + (string.IsNullOrEmpty(health) ? "" : "   ·   " + health);
                    System.Drawing.Color tempColor = TempColor(temp);

                    DoOnUIThread(delegate ()
                    {
                        lblSensors.Text = sensors;
                        lblSensors.ForeColor = tempColor;
                        lblBattery.Text = battery;
                        RefreshKeyboardHighlight(kb);
                    });
                }
                catch { }
                finally { System.Threading.Interlocked.Exchange(ref statusBusy, 0); }
            });
        }

        static System.Drawing.Color TempColor(int t)
        {
            if (t <= 0) return System.Drawing.SystemColors.ControlText;
            if (t >= 85) return System.Drawing.Color.FromArgb(200, 0, 0);    // hot: red
            if (t >= 70) return System.Drawing.Color.FromArgb(200, 120, 0);  // warm: orange
            return System.Drawing.Color.FromArgb(0, 140, 0);                 // cool: green
        }

        // Active-state highlighting for the Fan and Keyboard rows.
        static readonly System.Drawing.Color ActiveBack = System.Drawing.Color.FromArgb(0, 120, 215);
        void SetActive(System.Windows.Forms.Button b, bool active)
        {
            b.UseVisualStyleBackColor = !active;
            b.BackColor = active ? ActiveBack : System.Drawing.SystemColors.Control;
            b.ForeColor = active ? System.Drawing.Color.White : System.Drawing.SystemColors.ControlText;
        }
        void RefreshFanHighlight()
        {
            SetActive(fanAuto, fanMode == 0);
            SetActive(fanFull, fanMode == 1);
        }
        LightLevel lastKbHighlight = LightLevel.Unknown;
        void RefreshKeyboardHighlight(LightLevel lvl)
        {
            if (lvl == lastKbHighlight) return;
            lastKbHighlight = lvl;
            SetActive(kbOff, lvl == LightLevel.Off);
            SetActive(kbLow, lvl == LightLevel.Low);
            SetActive(kbHigh, lvl == LightLevel.High);
        }
        #endregion

        private void DoOnUIThread(MethodInvoker d)
        {
            if (this.InvokeRequired) { this.Invoke(d); } else { d(); }
        }

        #region NotifyIcon
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height / 2);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height / 2);
        }
        void wrapup()
        {
            SaveSettings();
            SaveSettingsKBD();
            if (fanMode != 0) WriteByteToEC(TP_ECOFFSET_FAN, FAN_AUTO); // restore firmware fan control on exit
            NotifyIcon1.Visible = false;
            if (activeDriver == 0)
            {
                if (ols != null) ols.DeinitializeOls();
            }
            else if (activeDriver == 1)
            {
                TVicPort.CloseTVicPort();
            }
            else if (activeDriver == 2)
            {
                PawnIoDriver.Close();
            }

        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            wrapup();
            Environment.Exit(0);
        }
        #endregion

        bool trayHintShown = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Closing the window keeps the app alive in the notification area.
            // Real exit happens through the tray menu's Quit, or a Windows shutdown.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                if (!trayHintShown)
                {
                    trayHintShown = true;
                    NotifyIcon1.ShowBalloonTip(3000, "ThinkPad Toolbox", "Still running here. Right-click the icon to quit.", ToolTipIcon.Info);
                }
                return;
            }
            wrapup();
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string driver = activeDriver == 2 ? "PawnIO" : activeDriver == 1 ? "TVicPort" : "WinRing0";
            using (var about = new AboutBox(driver, IsAdministrator()))
            {
                about.ShowDialog(this);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try {
                // Creates an XML file from which a scheduled task is created and saved on the system. Based on an example found on StackOverflow, http://stackoverflow.com/questions/5427673/how-to-run-a-program-automatically-as-admin-on-windows-startup
                string username = Environment.UserName;
                string computername = Environment.MachineName;
                string part1 = LEDControl.Properties.Resources.part1;
                string part2 = LEDControl.Properties.Resources.part2;
                string part3 = LEDControl.Properties.Resources.part3;
                string part4 = LEDControl.Properties.Resources.part4;
                string final = part1 + username + part2 + computername + "\\" + username + part3 + Application.ExecutablePath + part4;
                System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.StartupPath + "\\apply.xml");
                sw.Write(final);
                sw.Close();
                Process pr = new Process();
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = "cmd.exe";
                // Same task name as the installer's "start at logon" option, with /f to
                // overwrite, so the two never create parallel startup tasks (which is how
                // multiple copies once launched at login).
                pi.Arguments = "/c schtasks /create /f /tn \"ThinkPad Toolbox\" /xml \"" + Application.StartupPath + "\\apply.xml\"";
                pr.StartInfo = pi;
                pr.Start();
                pr.WaitForExit();
                System.IO.File.Delete(Application.StartupPath + "\\apply.xml");
                DialogResult dr = MessageBox.Show("Registration complete, do you want to open Task Scheduler to check the operation, or further adjust the entry which was created?", "ThinkPad Toolbox", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                // .NET 8's Process.Start defaults to UseShellExecute=false, which cannot launch a .msc; force shell execute.
                if (dr == DialogResult.Yes) Process.Start(new ProcessStartInfo("Taskschd.msc") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to finish this task.\r\n\r\n" + ex.Message, "ThinkPad Toolbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (hide_me)
            {
                DoOnUIThread(delegate ()
                {
                    this.Hide();
                });
                hide_me = false;
                timer1.Enabled = false;
            }
        }

        void OnPowerChange(Object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    if (rememberKBD.Checked)
                    {
                        SetKeyboardLevel(MostCommonLevel());
                        lightTimer.Enabled = true;
                    }
                    ReapplyLedsAfterWake();
                    break;
            }
        }

        // The firmware resets the status LEDs when the session locks/unlocks; restore the
        // user's chosen LED states after every unlock, sign-in, or fast-user-switch reconnect.
        void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionUnlock ||
                e.Reason == SessionSwitchReason.SessionLogon ||
                e.Reason == SessionSwitchReason.ConsoleConnect)
            {
                ReapplyLedsAfterWake();
            }
        }

        private enum LightLevel
        {
            Off, Low, High, Unknown
        }

        private LightLevel GetKeyboardLightlevel()
        {
            byte c = 0x00;
            ReadByteFromEC(0x0d, ref c);
            if (c >= 0 && c < 50) return LightLevel.Off;
            else if (c >= 50 && c < 100) return LightLevel.Low;
            else if (c >= 100 && c < 150) return LightLevel.High;
            else return LightLevel.Unknown;
        }

        private void SetKeyboardLevel(LightLevel lvl)
        {
            byte _out = 0x00;
            switch (lvl)
            {
                case LightLevel.Off:
                    _out = 0x00 | 0x00;
                    WriteByteToEC(0x0d, _out);
                    break;
                case LightLevel.Low:
                    _out = 0x00 | 0x40;
                    WriteByteToEC(0x0d, _out);
                    break;
                case LightLevel.High:
                    _out = 0x00 | 0x80;
                    WriteByteToEC(0x0d, _out);
                    break;
            }
        }

        bool prevStat = true;
        void MonitorOnChanged(object sender, EventArgs e)
        {
            if (rememberKBD.Checked)
            {
                if (prevStat != PowerManager.IsMonitorOn)
                {
                    prevStat = PowerManager.IsMonitorOn;
                }
                if (PowerManager.IsMonitorOn == true)
                {
                    SetKeyboardLevel(MostCommonLevel());
                    lightTimer.Enabled = true;
                }
                else lightTimer.Enabled = false;
                //SaveSettingsKBD();
            }
        }
        List<LightLevel> levels = new List<LightLevel>();
        readonly object levelsLock = new object();
        void RecordLevel(LightLevel l)
        {
            lock (levelsLock) { levels.Add(l); if (levels.Count > 5) levels.RemoveAt(0); }
        }
        LightLevel MostCommonLevel()
        {
            lock (levelsLock)
            {
                if (levels.Count == 0) return LightLevel.Off;
                return (from item in levels
                        group item by item into g
                        orderby g.Count() descending
                        select g.Key).First();
            }
        }
        bool prev_l = false;
        bool prev_v = false;
        LightLevel prev_c;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // Guards against a slow tick's EC work piling up if it outruns the interval.
        private int backlightBusy = 0;
        private void lightTimer_Tick(object sender, EventArgs e)
        {
            // The tick fires on the UI thread; snapshot the setting here, then do all
            // the (potentially slow) EC/COM work on a background thread so the UI
            // never freezes while waiting on the embedded controller.
            if (System.Threading.Interlocked.CompareExchange(ref backlightBusy, 1, 0) != 0) return;
            bool turnOff = checkTurnKBLightOff.Checked;
            bool remember = rememberKBD.Checked;
            System.Threading.Tasks.Task.Run(() =>
            {
                try { BacklightWork(turnOff, remember); }
                catch { }
                finally { System.Threading.Interlocked.Exchange(ref backlightBusy, 0); }
            });
        }
        private void BacklightWork(bool turnOff, bool remember)
        {
            // This feature dims ONLY the keyboard backlight while a full-screen app is in
            // front. It deliberately does not touch the power/mic/Fn LEDs (the original did,
            // which made LEDs flicker when apps like VLC toggled full screen).
            bool full = false;
            if (turnOff)
            {
                string title = GetText(GetForegroundWindow());
                if (IsForegroundFullScreen() && title != "" && title != "Windows Default Lock Screen" && title != "Program Manager")
                {
                    if (!prev_l) { prev_c = GetKeyboardLightlevel(); prev_v = true; }
                    prev_l = true;
                    full = true;
                }
            }
            // Only sample the keyboard level (an EC read) when the "remember level" feature
            // is actually on. When just the full-screen dimmer is enabled, this avoids a
            // continuous once-per-second EC read while nothing needs it.
            if (remember && PowerManager.IsMonitorOn && (!turnOff || !full) && !isLidClosed)
            {
                RecordLevel(GetKeyboardLightlevel());
            }
            if (turnOff)
            {
                if (full)
                {
                    if (prev_v)
                    {
                        SetKeyboardLevel(LightLevel.Off);
                        prev_v = false;
                    }
                }
                else
                {
                    if (prev_l)
                    {
                        prev_l = false;
                        LightLevel lvl = GetKeyboardLightlevel();
                        if (lvl == LightLevel.Off) SetKeyboardLevel(prev_c);
                    }
                }
            }
        }

        private void rememberKBD_CheckedChanged(object sender, EventArgs e)
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static bool IsForegroundFullScreen()
        {
            return IsForegroundFullScreen(null);
        }

        public static bool IsForegroundFullScreen(Screen screen)
        {
            if (screen == null)
            {
                screen = Screen.PrimaryScreen;
            }
            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, GetForegroundWindow()), ref rect);
            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top).Contains(screen.Bounds);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Welcome w = new Welcome();
            w.comboBox1.SelectedIndex = Properties.Settings.Default.Driver;
            if (w.ShowDialog() == DialogResult.OK)
            {
                wrapup();
                Process.Start(Application.ExecutablePath);
                Application.DoEvents();

                Environment.Exit(0);
            }
        }

        private void checkTurnKBLightOff_CheckedChanged(object sender, EventArgs e)
        {
            lightTimer.Enabled = checkTurnKBLightOff.Checked;
        }

        // Example code from:
        // http://stackoverflow.com/questions/3355606/detect-laptop-lid-closure-and-opening

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
            CallingConvention = CallingConvention.StdCall)]

        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
            Int32 Flags);

        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int WM_POWERBROADCAST = 0x0218;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        private bool? _previousLidState = null;

        private void RegisterForPowerNotifications()
        {
            IntPtr handle = this.Handle;
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
                 ref GUID_LIDSWITCH_STATE_CHANGE,
                 DEVICE_NOTIFY_WINDOW_HANDLE);
            Debug.WriteLine(hLIDSWITCHSTATECHANGE.ToString());
        }

        protected override void WndProc(ref Message m)
        {
            // A second copy asked us to come to the front (single-instance handoff).
            if (WM_SHOWTPTOOLBOX != 0 && m.Msg == WM_SHOWTPTOOLBOX)
            {
                DoOnUIThread(delegate ()
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                    this.BringToFront();
                });
                return;
            }

            bool handled = false;
            switch (m.Msg)
            {
                case WM_POWERBROADCAST:
                    OnPowerBroadcast(m.WParam, m.LParam);
                    handled = true;
                    break;
                default:
                    break;
            }
            if (handled)
                DefWndProc(ref m);
            else
                base.WndProc(ref m);
        }

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    bool isLidOpen = ps.Data != 0;

                    if (!isLidOpen == _previousLidState)
                    {
                        LidStatusChanged(isLidOpen);
                    }

                    _previousLidState = isLidOpen;
                }
            }
        }

        bool isLidClosed = false;

        private void LidStatusChanged(bool isLidOpen)
        {
            if (isLidOpen)
            {
                isLidClosed = false;
                //Do some action on lid open event
                //Debug.WriteLine("{0}: Lid opened!", DateTime.Now);
                if (rememberKBD.Checked)
                {
                    SetKeyboardLevel(MostCommonLevel());
                    lightTimer.Enabled = true;
                    //SaveSettingsKBD();
                }
            }
            else
            {
                //Do some action on lid close event
                //Debug.WriteLine("{0}: Lid closed!", DateTime.Now);
                isLidClosed = true;
            }
        }


    }
}
