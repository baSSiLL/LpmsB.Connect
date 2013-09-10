using System;

namespace LpmsB
{
    [Flags]
    public enum OperationOptions
    {
        DynamicMagnetometerCorrection = 1 << 20,
        GyroscopeThreshold = 1 << 23,
        MagnetometerCompensation = 1 << 24,
        AccelerometerCompensation = 1 << 25,
        GyroscopeAutoCalibration = 1 << 30,
        All = DynamicMagnetometerCorrection | GyroscopeThreshold | MagnetometerCompensation | AccelerometerCompensation | GyroscopeAutoCalibration,
    }
}
