using System;
using System.Management;
using System.Runtime.InteropServices;

namespace LEDControl
{
    /// <summary>
    /// Battery information for the status readout.
    ///
    /// Live charge / charging state comes from GetSystemPowerStatus (no dependency,
    /// always available). Battery health (full-charge vs design capacity) and cycle
    /// count come from the root\wmi battery classes, which some laptops/batteries do
    /// not populate - everything degrades gracefully to whatever is available.
    /// </summary>
    static class BatteryInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS status);

        /// <summary>e.g. "Battery: 85% (charging)  ·  2h 14m left"</summary>
        public static string PowerLine()
        {
            try
            {
                SYSTEM_POWER_STATUS s;
                if (!GetSystemPowerStatus(out s)) return "Battery: unavailable";
                if (s.BatteryFlag == 128 || s.BatteryLifePercent == 255) return "Battery: not detected";

                bool charging = (s.BatteryFlag & 8) != 0;
                string state = charging ? "charging" : (s.ACLineStatus == 1 ? "plugged in" : "on battery");

                string time = "";
                if (!charging && s.ACLineStatus != 1 && s.BatteryLifeTime > 0)
                {
                    int mins = s.BatteryLifeTime / 60;
                    time = "  ·  " + (mins / 60) + "h " + (mins % 60) + "m left";
                }
                return "Battery: " + s.BatteryLifePercent + "% (" + state + ")" + time;
            }
            catch { return "Battery: unavailable"; }
        }

        /// <summary>e.g. "Health 92%   ·   142 cycles" (empty if unsupported).</summary>
        public static string HealthLine()
        {
            uint designed = 0, fullCharge = 0, cycles = 0;
            try { designed = QueryFirst(@"root\wmi", "SELECT DesignedCapacity FROM BatteryStaticData", "DesignedCapacity"); } catch { }
            try { fullCharge = QueryFirst(@"root\wmi", "SELECT FullChargedCapacity FROM BatteryFullChargedCapacity", "FullChargedCapacity"); } catch { }
            try { cycles = QueryFirst(@"root\wmi", "SELECT CycleCount FROM BatteryCycleCount", "CycleCount"); } catch { }

            string result = "";
            if (designed > 0 && fullCharge > 0)
            {
                int health = (int)Math.Round(100.0 * fullCharge / designed);
                if (health > 0 && health <= 100) result = "Health " + health + "%";
            }
            if (cycles > 0)
                result += (result.Length > 0 ? "   ·   " : "") + cycles + " cycles";
            return result;
        }

        static uint QueryFirst(string scope, string query, string property)
        {
            using (var searcher = new ManagementObjectSearcher(scope, query))
            using (var results = searcher.Get())
            {
                foreach (ManagementObject o in results)
                {
                    object v = o[property];
                    if (v != null) return Convert.ToUInt32(v);
                }
            }
            return 0;
        }
    }
}
