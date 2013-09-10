using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LpmsB.Device
{
    [Flags]
    internal enum OperationOptions
    {
        DynamicMagnetometerCorrection = 1 << 20,
        GyroscopeThreshold = 1 << 23,
        MagnetometerCompensation = 1 << 24,
        AccelerometerCompensation = 1 << 25,
        GyroscopeAutoCalibration = 1 << 30,
        All = DynamicMagnetometerCorrection | GyroscopeThreshold | MagnetometerCompensation | AccelerometerCompensation | GyroscopeAutoCalibration,
    }
}
