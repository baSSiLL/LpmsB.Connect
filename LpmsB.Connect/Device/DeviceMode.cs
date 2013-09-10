using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LpmsB.Device
{
    internal enum DeviceMode
    {
        Unknown = -1,
        Command = Protocol.Command.GotoCommandMode,
        Stream = Protocol.Command.GotoStreamMode,
        Sleep = Protocol.Command.GotoSleepMode,
    }
}
