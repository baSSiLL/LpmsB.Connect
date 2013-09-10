using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LpmsB.Device
{
    [Flags]
    internal enum DeviceStatus
    {
        CommandModeEnabled = 1 << 0,
        StreamModeEnabled = 1 << 1,
        SleepModeEnabled = 1 << 2,
        GyroscopeCalibrationOn = 1 << 3,
        GyroscopeInitializationFailed = 1 << 5,
        AccelerometerInitializationFailed = 1 << 6,
        MagnetometerInitializationFailed = 1 << 7,
        PressureSensorInitializationFailed = 1 << 8,
        GyroscopeUnresponsive = 1 << 9,
        AccelerometerUnresponsive = 1 << 10,
        MagnetometerUnresponsive = 1 << 11,
        FlashWriteFailed = 1 << 12,
        SetBroadcastFrequencyFailed = 1 << 14,
    }
}
