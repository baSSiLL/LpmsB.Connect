using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;

namespace LpmsB.Bluetooth
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BluetoothAddress : IEquatable<BluetoothAddress>
    {
        public const int MEANINGFUL_SIZE = 6;

        public readonly ulong Value;

        public BluetoothAddress(ulong value)
        {
            Value = value;
        }

        public BluetoothAddress(byte[] value, int startIndex = 0)
        {
            Contract.Requires(value != null);
            Contract.Requires(startIndex >= 0 && startIndex + MEANINGFUL_SIZE <= value.Length);

            var buffer = new byte[sizeof(ulong)];
            Array.Copy(value, startIndex, buffer, 0, MEANINGFUL_SIZE);

            Value = BitConverter.ToUInt64(buffer, 0);
        }

        public byte[] ToSixBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == MEANINGFUL_SIZE);

            return BitConverter.GetBytes(Value).Take(MEANINGFUL_SIZE).ToArray();
        }

        public byte[] ToEightBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == sizeof(ulong));

            return BitConverter.GetBytes(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BluetoothAddress)
                return ((BluetoothAddress)obj) == this;
            return false;
        }

        public bool Equals(BluetoothAddress other)
        {
            return other == this;
        }

        public override string ToString()
        {
            return BitConverter.ToString(ToSixBytes().Reverse().ToArray())
                .Replace('-', ':');
        }

        public static bool operator ==(BluetoothAddress left, BluetoothAddress right)
        {
            return left.Value == right.Value;
        }

        public static bool operator ==(BluetoothAddress left, ulong right)
        {
            return left.Value == right;
        }

        public static bool operator !=(BluetoothAddress left, BluetoothAddress right)
        {
            return left.Value != right.Value;
        }

        public static bool operator !=(BluetoothAddress left, ulong right)
        {
            return left.Value != right;
        }

        public static implicit operator ulong(BluetoothAddress addr)
        {
            return addr.Value;
        }
    }
}
