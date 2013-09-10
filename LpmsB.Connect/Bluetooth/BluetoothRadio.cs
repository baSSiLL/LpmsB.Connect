using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace LpmsB.Bluetooth
{
    internal sealed class BluetoothRadio : IDisposable
    {
        public static BluetoothRadio[] FindAll(int maxCount = int.MaxValue)
        {
            Contract.Requires(maxCount > 0);
            Contract.Ensures(Contract.Result<BluetoothRadio[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<BluetoothRadio[]>(), r => r != null && !r.IsDisposed));

            var findParams = new BluetoothInterop.BluetoothFindRadioParams
            {
                Size = BluetoothInterop.BluetoothFindRadioParams.SIZE
            };

            IntPtr hRadio;
            var hFind = BluetoothInterop.BluetoothFindFirstRadio(ref findParams, out hRadio);

            if (hFind == IntPtr.Zero)
            {
                var hr = Marshal.GetLastWin32Error();
                if (hr != 0 && hr != BluetoothInterop.ERROR_NO_MORE_ITEMS)
                {
                    Trace.TraceWarning("BluetoothFindFirstRadio failed with error code {0}", hr);
                    BluetoothInterop.ThrowBluetoothException(hr);
                }

                return new BluetoothRadio[0];
            }

            try
            {
                var res = new List<BluetoothRadio>();

                while (hRadio != IntPtr.Zero)
                {
                    var radioInfo = new BluetoothInterop.BluetoothRadioInfo
                    {
                        Size = BluetoothInterop.BluetoothRadioInfo.SIZE
                    };

                    var errCode = BluetoothInterop.BluetoothGetRadioInfo(hRadio, ref radioInfo);
                    if (errCode != 0)
                    {
                        Trace.TraceWarning("BluetoothGetRadioInfo failed with error code {0}", errCode);
                        BluetoothInterop.CloseHandle(hRadio);
                    }
                    else
                    {
                        res.Add(new BluetoothRadio(hRadio, radioInfo));

                        if (res.Count == maxCount)
                        {
                            break;
                        }

                    }

                    if (!BluetoothInterop.BluetoothFindNextRadio(hFind, out hRadio))
                    {
                        var hr = Marshal.GetLastWin32Error();
                        if (hr != 0 && hr != BluetoothInterop.ERROR_NO_MORE_ITEMS)
                        {
                            Trace.TraceWarning("BluetoothFindNextRadio failed with error code {0}", hr);
                            BluetoothInterop.ThrowBluetoothException(hr);
                        }

                        break;
                    }
                }

                return res.ToArray();
            }
            finally
            {
                if (!BluetoothInterop.BluetoothFindRadioClose(hFind))
                {
                    Trace.TraceWarning("BluetoothFindRadioClose failed with error code {0}",
                        Marshal.GetLastWin32Error());
                }
            }
        }

        public static BluetoothRadio First
        {
            get
            {
                Contract.Ensures(Contract.Result<BluetoothRadio>() == null
                    || !Contract.Result<BluetoothRadio>().IsDisposed);

                return FindAll(1).FirstOrDefault();
            }
        }

        private BluetoothRadio(IntPtr handle, BluetoothInterop.BluetoothRadioInfo radioInfo)
        {
            Contract.Requires(handle != IntPtr.Zero);

            this.handle = handle;
            Name = radioInfo.Name;
            Address = radioInfo.Address;
            Manufacturer = radioInfo.Manufacturer;
        }

        ~BluetoothRadio()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Contract.Ensures(IsDisposed);

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero)
            {
                BluetoothInterop.CloseHandle(handle);
                handle = IntPtr.Zero;
            }
        }

        public bool IsDisposed
        {
            get { return handle == IntPtr.Zero; }
        }

        public string Name { get; private set; }
        public BluetoothAddress Address { get; private set; }
        public BluetoothManufacturer Manufacturer { get; private set; }
        internal IntPtr Handle { get { return handle; } }

        public override string ToString()
        {
            return string.Format("Bluetooth {0} ({1})", Address, Name);
        }

        public BluetoothDevice[] FindDevices(TimeSpan timeout,
            bool includeAuthenticated = true,
            bool includeConnected = true, bool includeRemembered = true,
            bool includeUnknown = true, bool issueInquiry = true)
        {
            Contract.Requires(timeout >= TimeSpan.Zero);
            Contract.Requires(timeout <= TimeSpan.FromSeconds(48 * BluetoothInterop.TIMEOUT_MULTIPLIER_UNIT_SEC));
            Contract.Requires(!IsDisposed);
            Contract.Ensures(Contract.Result<BluetoothDevice[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<BluetoothDevice[]>(), d => d != null && d.Radio == this));

            var prms = new BluetoothInterop.BluetoothDeviceSearchParams
            {
                Size = BluetoothInterop.BluetoothDeviceSearchParams.SIZE,
                hRadio = handle,
                ReturnAuthenticated = includeAuthenticated,
                ReturnConnected = includeConnected,
                ReturnRemembered = includeRemembered,
                ReturnUnknown = includeUnknown,
                IssueInquiry = issueInquiry,
                TimeoutMultiplier = Convert.ToByte(Math.Round(timeout.TotalSeconds / BluetoothInterop.TIMEOUT_MULTIPLIER_UNIT_SEC)),
            };

            var devInfo = new BluetoothInterop.BluetoothDeviceInfo
            {
                Size = BluetoothInterop.BluetoothDeviceInfo.SIZE
            };

            var res = new List<BluetoothDevice>();

            var hFind = BluetoothInterop.BluetoothFindFirstDevice(ref prms, ref devInfo);
            if (hFind != IntPtr.Zero)
            {
                while (true)
                {
                    res.Add(new BluetoothDevice(this, devInfo));

                    if (!BluetoothInterop.BluetoothFindNextDevice(hFind, ref devInfo))
                    {
                        var hr = Marshal.GetLastWin32Error();
                        if (hr != 0 && hr != BluetoothInterop.ERROR_NO_MORE_ITEMS)
                        {
                            Trace.TraceWarning("BluetoothFindNextDevice failed with error code {0}", hr);
                            BluetoothInterop.ThrowBluetoothException(hr);
                        }

                        break;
                    }
                }

                if (!BluetoothInterop.BluetoothFindDeviceClose(hFind))
                {
                    var hr = Marshal.GetLastWin32Error();
                    Trace.TraceWarning("BluetoothFindDeviceClose failed with error code {0}", hr);
                }
            }
            else
            {
                var hr = Marshal.GetLastWin32Error();
                if (hr != 0 && hr != BluetoothInterop.ERROR_NO_MORE_ITEMS)
                {
                    Trace.TraceWarning("BluetoothFindFirstDevice failed with error code {0}", hr);
                    BluetoothInterop.ThrowBluetoothException(hr);
                }
            }

            return res.ToArray();
        }

        private IntPtr handle;
    }
}
