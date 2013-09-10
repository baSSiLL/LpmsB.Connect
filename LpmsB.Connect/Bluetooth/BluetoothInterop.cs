using System;
using System.Runtime.InteropServices;
using System.IO;

namespace LpmsB.Bluetooth
{
    internal static class BluetoothInterop
    {
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const double TIMEOUT_MULTIPLIER_UNIT_SEC = 1.28;
        public const int AF_BTH = 32;
        public const int BTHPROTO_RFCOMM = 3;

        [StructLayout(LayoutKind.Sequential)]
        internal struct BluetoothFindRadioParams
        {
            public static readonly int SIZE = Marshal.SizeOf(typeof(BluetoothFindRadioParams));

            public int Size;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct BluetoothRadioInfo
        {
            public static readonly int SIZE = Marshal.SizeOf(typeof(BluetoothRadioInfo));

            public int Size;
            public BluetoothAddress Address;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
            public string Name;
            public uint ClassOfDevice;
            public ushort Subversion;
            [MarshalAs(UnmanagedType.U2)]
            public BluetoothManufacturer Manufacturer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BluetoothDeviceInfo
        {
            public static readonly int SIZE = Marshal.SizeOf(typeof(BluetoothDeviceInfo));

            public int Size;
            public BluetoothAddress Address;
            public uint ClassOfDevice;
            public bool Connected;
            public bool Remembered;
            public bool Authenticated;
            public SystemTime LastSeen;
            public SystemTime LastUsed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
            public string Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BluetoothDeviceSearchParams
        {
            public static readonly int SIZE = Marshal.SizeOf(typeof(BluetoothDeviceSearchParams));

            public int Size;
            public bool ReturnAuthenticated;
            public bool ReturnRemembered;
            public bool ReturnUnknown;
            public bool ReturnConnected;
            public bool IssueInquiry;
            public byte TimeoutMultiplier;
            public IntPtr hRadio;
        }

        public static void ThrowBluetoothException()
        {
            var errorCode = Marshal.GetLastWin32Error();
            ThrowBluetoothException(errorCode);
        }

        public static void ThrowBluetoothException(int errorCode)
        {
            if (errorCode != 0)
            {
                var exc = Marshal.GetExceptionForHR(errorCode)
                    ?? new IOException("Bluetooth error " + errorCode);
                throw exc;
            }
            else
            {
                throw new IOException("Unknown Bluetooth error");
            }
        }

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern IntPtr BluetoothFindFirstRadio(ref BluetoothFindRadioParams findParams, out IntPtr hRadio);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextRadio(IntPtr hFind, out IntPtr hRadio);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern int BluetoothGetRadioInfo(IntPtr hRadio, ref BluetoothRadioInfo radioInfo);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern IntPtr BluetoothFindFirstDevice(ref BluetoothDeviceSearchParams searchParams, ref BluetoothDeviceInfo deviceInfo);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BluetoothDeviceInfo deviceInfo);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindDeviceClose(IntPtr hFind);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern int BluetoothGetDeviceInfo(IntPtr hRadio, ref BluetoothDeviceInfo deviceInfo);

        [DllImport("Irprops.cpl", SetLastError = true)]
        public static extern int BluetoothUpdateDeviceRecord(ref BluetoothDeviceInfo deviceInfo);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);
    }
}
