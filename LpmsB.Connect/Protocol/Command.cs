using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LpmsB.Protocol
{
    internal enum Command : ushort
    {
        ReplyAcknowledge = 0,
        ReplyNegativeAcknowledge = 1,

        UpdateFirmware = 2,
        UpdateIap = 3,
        GetFirmwareVersion = 47,

        GetConfiguration = 4,
        GetStatus = 5,

        GotoCommandMode = 6,
        GotoStreamMode = 7,
        GotoSleepMode = 8,

        GetSensorData = 9,
        SetTransmitData = 10,
        SetStreamFrequency = 11,
        GetRoll = 12,
        GetPitch = 13,
        GetYaw = 14,

        WriteRegisters = 15,
        RestoreFactoryValues = 16,
        
        ResetReference = 17,
        SetOffset = 18,
        
        SelfTest = 19,
        
        SetImuId = 20,
        GetImuId = 21,
        
        StartGyroscopeCalibration = 22,
        EnableGyroscopeAutoCalibration = 23,
        EnableGyroscopeThreshold = 24,
        SetGyroscopeRange = 25,
        GetGyroscopeRange = 26,
        SetGyroscopeAlignmentBias = 48,
        GetGyroscopeAlignmentBias = 49,
        SetGyroscopeAlignmentMatrix = 50,
        GetGyroscopeAlignmentMatrix = 51,
        
        SetAccelerometerBias = 27,
        GetAccelerometerBias = 28,
        SetAccelerometerAlignmentMatrix = 29,
        GetAccelerometerAlignmentMatrix = 30,
        SetAccelerometerRange = 31,
        GetAccelerometerRange = 32,
        
        SetMagnetometerRange = 33,
        GetMagnetometerRange = 34,
        SetHardIronOffset = 35,
        GetHardIronOffset = 36,
        SetSoftIronMatrix = 37,
        GetSoftIronMatrix = 38,
        SetFieldEstimate = 39,
        GetFieldEstimate = 40,

        SetFilterMode = 41,
        GetFilterMode = 42,
        SetFilterPreset = 43,
        GetFilterPreset = 44,
        SetLowPassStrength = 60,
        GetLowPassStrength = 61,

        ResetTimeStamp = 66,
    }
}
