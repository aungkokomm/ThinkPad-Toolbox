using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace LEDControl
{
    /// <summary>
    /// Embedded-controller access through PawnIO (https://pawnio.eu), the modern
    /// attestation-signed kernel driver that loads with Memory Integrity / HVCI
    /// enabled, unlike the old WinRing0 driver (which the Windows vulnerable-driver
    /// blocklist now refuses to load).
    ///
    /// This talks to the PawnIO device directly (CreateFile + DeviceIoControl), the
    /// same way LibreHardwareMonitor does, so it needs no PawnIOLib.dll on the search
    /// path. It loads the embedded LpcACPIEC module and uses ioctl_pio_read /
    /// ioctl_pio_write, which the module restricts to the ACPI embedded-controller
    /// ports 0x62 and 0x66 - exactly the two ports this application uses.
    /// </summary>
    static class PawnIoDriver
    {
        // Control codes for the PawnIO device (match LibreHardwareMonitor).
        const uint DEVICE_TYPE = 41394u << 16;
        const int FN_NAME_LENGTH = 32;
        const uint IOCTL_PIO_EXECUTE_FN = 0x841 << 2;
        const uint IOCTL_PIO_LOAD_BINARY = 0x821 << 2;
        const uint CTL_LOAD_BINARY = DEVICE_TYPE | IOCTL_PIO_LOAD_BINARY;
        const uint CTL_EXECUTE = DEVICE_TYPE | IOCTL_PIO_EXECUTE_FN;

        const uint FILE_SHARE_READ = 0x1;
        const uint FILE_SHARE_WRITE = 0x2;
        const uint OPEN_EXISTING = 3;
        const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        const int ERROR_FILE_NOT_FOUND = 2;
        const int ERROR_PATH_NOT_FOUND = 3;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode,
            byte[] lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        static SafeFileHandle _handle;

        public static bool IsOpen { get { return _handle != null && !_handle.IsInvalid && !_handle.IsClosed; } }

        /// <summary>Human-readable reason the last Open() failed, for the error dialog.</summary>
        public static string LastError { get; private set; } = "";

        /// <summary>True when Open() failed because the PawnIO device does not exist (driver not installed).</summary>
        public static bool DeviceMissing { get; private set; }

        public static bool Open()
        {
            if (IsOpen) return true;
            DeviceMissing = false;

            _handle = CreateFileW(@"\\?\GLOBALROOT\Device\PawnIO",
                (uint)FileAccess.ReadWrite, FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

            if (_handle == null || _handle.IsInvalid)
            {
                int err = Marshal.GetLastWin32Error();
                DeviceMissing = (err == ERROR_FILE_NOT_FOUND || err == ERROR_PATH_NOT_FOUND);
                LastError = "Could not open the PawnIO device (Win32 error " + err + ").";
                _handle = null;
                return false;
            }

            byte[] module = GetModuleBlob();
            if (module == null)
            {
                LastError = "The embedded LpcACPIEC.bin module was not found in the application.";
                Close();
                return false;
            }

            uint returned;
            if (!DeviceIoControl(_handle, CTL_LOAD_BINARY, module, (uint)module.Length, null, 0, out returned, IntPtr.Zero))
            {
                LastError = "Loading the EC module into PawnIO failed (Win32 error " + Marshal.GetLastWin32Error() +
                            "). The installed PawnIO version may be incompatible with the bundled module.";
                Close();
                return false;
            }

            LastError = "";
            return true;
        }

        static byte[] GetModuleBlob()
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("LEDControl.Resources.LpcACPIEC.bin"))
            {
                if (s == null) return null;
                using (var ms = new MemoryStream()) { s.CopyTo(ms); return ms.ToArray(); }
            }
        }

        public static void Close()
        {
            if (_handle != null)
            {
                try { if (!_handle.IsInvalid && !_handle.IsClosed) _handle.Close(); } catch { }
                _handle = null;
            }
        }

        // Buffer layout: 32-byte ASCII function name followed by the input int64 values.
        static long[] Execute(string name, long[] input, int outLength)
        {
            int outBytes = outLength * sizeof(long);
            byte[] output = new byte[outBytes > 0 ? outBytes : sizeof(long)]; // avoid a zero-length array
            byte[] totalInput = new byte[(input.Length * sizeof(long)) + FN_NAME_LENGTH];
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            Buffer.BlockCopy(nameBytes, 0, totalInput, 0, Math.Min(FN_NAME_LENGTH - 1, nameBytes.Length));
            Buffer.BlockCopy(input, 0, totalInput, FN_NAME_LENGTH, input.Length * sizeof(long));

            uint read;
            if (DeviceIoControl(_handle, CTL_EXECUTE, totalInput, (uint)totalInput.Length, output, (uint)outBytes, out read, IntPtr.Zero))
            {
                long[] outp = new long[read / sizeof(long)];
                Buffer.BlockCopy(output, 0, outp, 0, (int)read);
                return outp;
            }
            return new long[outLength];
        }

        public static byte ReadPort(ushort port)
        {
            long[] outp = Execute("ioctl_pio_read", new long[] { port }, 1);
            return outp.Length > 0 ? (byte)outp[0] : (byte)0xFF;
        }

        public static void WritePort(ushort port, byte value)
        {
            Execute("ioctl_pio_write", new long[] { port, value }, 0);
        }
    }
}
