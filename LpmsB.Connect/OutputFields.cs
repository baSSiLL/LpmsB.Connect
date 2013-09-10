using System;

namespace LpmsB
{
    [Flags]
    public enum OutputFields : int
    {
        Pressure = 1 << 9,
        Magnetometer = 1 << 10,
        Accelerometer = 1 << 11,
        Gyroscope = 1 << 12,
        Temperature = 1 << 13,
        HeaveMotion = 1 << 14,
        AngularVelocity = 1 << 16,
        EulerAngles = 1 << 17,
        Quaternion = 1 << 18,
        Altitude = 1 << 19,
        LinearAcceleration = 1 << 21,
        All = Pressure | Magnetometer | Accelerometer | Gyroscope | Temperature | HeaveMotion | AngularVelocity | EulerAngles | Quaternion | Altitude | LinearAcceleration,
    }
}
