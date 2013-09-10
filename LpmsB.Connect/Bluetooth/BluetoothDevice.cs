using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace LpmsB.Bluetooth
{
    internal sealed class BluetoothDevice
    {
        internal BluetoothDevice(BluetoothRadio radio, BluetoothInterop.BluetoothDeviceInfo devInfo)
        {
            Contract.Requires(radio != null && !radio.IsDisposed);

            this.radio = radio;
            this.devInfo = devInfo;
        }

        private bool CheckHr(int hr, string funcName, bool throwOnError)
        {
            if (hr == 0)
                return true;

            Trace.TraceWarning("Function {0} for device '{1}' returns {2}",
                funcName, this, hr);

            if (throwOnError)
            {
                BluetoothInterop.ThrowBluetoothException(hr);
            }

            return false;
        }

        private bool Refresh(bool throwOnError)
        {
            var handle = Radio.Handle;
            if (handle == IntPtr.Zero)
                return false;

            var hr = BluetoothInterop.BluetoothGetDeviceInfo(handle, ref devInfo);
            return CheckHr(hr, "BluetoothGetDeviceInfo", throwOnError);
        }

        public void Refresh()
        {
            Contract.Requires(!Radio.IsDisposed);
            Refresh(true);
        }

        public bool TryRefresh()
        {
            return Refresh(false);
        }

        public BluetoothRadio Radio
        {
            get
            {
                Contract.Ensures(Contract.Result<BluetoothRadio>() != null);
                return radio;
            }
        }

        public BluetoothAddress Address { get { return devInfo.Address; } }

        public bool IsConnected { get { return devInfo.Connected; } }

        public bool IsAuthenticated { get { return devInfo.Authenticated; } }

        public bool IsRemembered { get { return devInfo.Remembered; } }

        public DateTime LastUsed { get { return (DateTime)devInfo.LastUsed; } }

        public DateTime LastSeen { get { return (DateTime)devInfo.LastSeen; } }

        public string Name
        {
            get { return devInfo.Name; }

            set
            {
                Contract.Requires(!string.IsNullOrEmpty(value));

                if (value != devInfo.Name)
                {
                    var old = devInfo.Name;
                    devInfo.Name = value;
                    var hr = BluetoothInterop.BluetoothUpdateDeviceRecord(ref devInfo);
                    if (hr != 0)
                    {
                        devInfo.Name = old;
                        BluetoothInterop.ThrowBluetoothException(hr);
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name)
                ? Address.ToString()
                : string.Format("{0} ({1})", Address, Name);
        }

        private readonly BluetoothRadio radio;
        private BluetoothInterop.BluetoothDeviceInfo devInfo;
    }
}
