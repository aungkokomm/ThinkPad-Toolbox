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

        static int lastPct = -1; // remembered so a flaky reading doesn't blank the percentage

        /// <summary>e.g. "Battery: 85% (charging)  ·  2h 14m left"</summary>
        public static string PowerLine()
        {
            try
            {
                // Charging state + presence from root\wmi BatteryStatus. This is reliable even
                // on ThinkPads where GetSystemPowerStatus intermittently reports BatteryFlag=128
                // / percent=255 (which used to show a false "not detected") while the battery is
                // actually present and charging.
                bool present = false, charging = false, discharging = false, acOnline = false;
                try
                {
                    using (var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT Charging, Discharging, PowerOnline FROM BatteryStatus"))
                    using (var results = searcher.Get())
                        foreach (ManagementObject o in results)
                        {
                            present = true;
                            charging = ToBool(o["Charging"]);
                            discharging = ToBool(o["Discharging"]);
                            acOnline = ToBool(o["PowerOnline"]);
                            break;
                        }
                }
                catch { }

                // Percentage: prefer GetSystemPowerStatus, then WMI capacities, then last known.
                int pct = -1;
                SYSTEM_POWER_STATUS s;
                bool sps = GetSystemPowerStatus(out s);
                if (sps)
                {
                    if (s.BatteryLifePercent <= 100) pct = s.BatteryLifePercent;
                    if (!present && (s.BatteryFlag & 128) == 0 && s.BatteryLifePercent <= 100) present = true;
                    if (!charging && !discharging) charging = (s.BatteryFlag & 8) != 0;
                    if (!acOnline) acOnline = s.ACLineStatus == 1;
                }
                if (pct < 0) pct = WmiChargePercent();
                if (pct >= 0) lastPct = pct; else pct = lastPct;

                if (!present && pct < 0) return "Battery: not detected";

                string state = charging ? "charging" : (discharging ? "on battery" : (acOnline ? "plugged in" : "on battery"));

                if (pct < 0)
                    return "Battery: " + state;   // present, but this machine reports no usable percentage

                string time = "";
                if (!charging && !acOnline && sps && s.BatteryLifeTime > 0)
                {
                    int mins = s.BatteryLifeTime / 60;
                    time = "  ·  " + (mins / 60) + "h " + (mins % 60) + "m left";
                }
                return "Battery: " + pct + "% (" + state + ")" + time;
            }
            catch { return "Battery: unavailable"; }
        }

        static bool ToBool(object v) { try { return v != null && Convert.ToBoolean(v); } catch { return false; } }

        // Fallback charge % from the root\wmi battery classes (the same ones HealthLine uses,
        // which populate reliably on ThinkPads) when GetSystemPowerStatus reports unknown.
        // Returns -1 if unavailable.
        static int WmiChargePercent()
        {
            try
            {
                uint remaining = QueryFirst(@"root\wmi", "SELECT RemainingCapacity FROM BatteryStatus", "RemainingCapacity");
                uint full = QueryFirst(@"root\wmi", "SELECT FullChargedCapacity FROM BatteryFullChargedCapacity", "FullChargedCapacity");
                if (remaining > 0 && full > 0)
                {
                    int pct = (int)Math.Round(100.0 * remaining / full);
                    return pct > 100 ? 100 : pct;
                }
            }
            catch { }
            return -1;
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
