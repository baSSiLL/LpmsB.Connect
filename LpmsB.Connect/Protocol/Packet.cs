using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using LpmsB.Utils;

namespace LpmsB.Protocol
{
    internal class Packet
    {
        private readonly ushort sensorId;
        private readonly Command command;
        private byte[] data;
        private BinaryWriter writer;

        public Packet(ushort sensorId, Command command, byte[] data = null)
        {
            Contract.Requires(Enum.IsDefined(typeof(Command), command));
            Contract.Requires(data == null || data.Length <= ushort.MaxValue);

            this.sensorId = sensorId;
            this.command = command;
            this.data = data;
            this.dataSize = data != null ? data.Length : 0;
        }

        public Packet(ushort sensorId, Command command, int initialCapacity)
        {
            Contract.Requires(Enum.IsDefined(typeof(Command), command));
            Contract.Requires(initialCapacity >= 0);

            this.sensorId = sensorId;
            this.command = command;
            this.data = initialCapacity > 0 ? new byte[initialCapacity] : null;
            this.dataSize = 0;
        }

        public ushort SensorId
        {
            get { return sensorId; }
        }

        public Command Command
        {
            get { return command; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        public ushort DataSize
        {
            get { return (ushort)dataSize; }
        }

        public ushort ComputeCheckSum()
        {
            int checksum = SensorId + (ushort)Command + DataSize;
            if (DataSize > 0)
            {
                checksum += Data.Take(DataSize).Sum(b => (int)b);
            }

            return (ushort)checksum;
        }


        #region Data reading and writing

        private int offset = 0;
        private int dataSize;

        public int DataOffset
        {
            get { return offset; }
            set
            {
                Contract.Requires(value <= DataSize);

                offset = value;
            }
        }

        public uint ReadUInt32()
        {
            CheckReadOperation(4);

            var result = BitConverter.ToUInt32(Data, offset);
            offset += 4;
            return result;
        }

        public float ReadSingle()
        {
            CheckReadOperation(4);

            var result = BitConverter.ToSingle(Data, offset);
            offset += 4;
            return result;
        }

        public Vector3d ReadVector3d()
        {
            CheckReadOperation(12);

            var result = new Vector3d();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();

            return result;
        }

        public Quaternion ReadQuaternion()
        {
            CheckReadOperation(16);

            var result = new Quaternion();
            result.W = ReadSingle();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();

            return result;
        }

        private void CheckReadOperation(int size)
        {
            if (offset + size > DataSize)
                throw new InvalidOperationException("Not enough data to read.");
        }

        public void WriteUInt32(uint value)
        {
            CheckWriteOperation(4);

            writer.Write(value);
            writer.Flush();

            offset += 4;
            dataSize = offset;
        }

        public void WriteBool(bool value)
        {
            WriteUInt32((uint)(value ? 1 : 0));
        }

        public void WriteSingle(float value)
        {
            CheckWriteOperation(4);

            writer.Write(value);
            writer.Flush();

            offset += 4;
            dataSize += 4;
        }

        public void WriteVector3d(Vector3d value)
        {
            CheckWriteOperation(12);

            WriteSingle((float)value.X);
            WriteSingle((float)value.Y);
            WriteSingle((float)value.Z);
        }

        public void WriteQuaternion(Quaternion value)
        {
            CheckWriteOperation(16);

            WriteSingle((float)value.W);
            WriteSingle((float)value.X);
            WriteSingle((float)value.Y);
            WriteSingle((float)value.Z);
        }

        private void CheckWriteOperation(int size)
        {
            var needNewWriter = true;

            if (data == null)
            {
                data = new byte[Math.Max(offset + size, 16)];
            }
            else if (offset + size > data.Length)
            {
                var capacity = Math.Max(data.Length * 2, offset + size);
                var oldData = data;
                data = new byte[capacity];
                Buffer.BlockCopy(oldData, 0, data, 0, oldData.Length);
            }
            else if (writer != null)
            {
                needNewWriter = false;
            }

            if (needNewWriter)
            {
                writer = new BinaryWriter(new MemoryStream(data));
                writer.BaseStream.Position = offset;
            }
        }

        #endregion
    }
}
