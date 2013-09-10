namespace LpmsB
{
    public enum DeviceMode
    {
        Unknown = -1,
        Command = Protocol.Command.GotoCommandMode,
        Stream = Protocol.Command.GotoStreamMode,
        Sleep = Protocol.Command.GotoSleepMode,
    }
}
