using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace LpmsB.Protocol
{
    internal class Transmitter
    {
        private const int PACKET_NON_DATA_SIZE = 11;
        private const byte PACKET_START = 0x3A;
        private const byte PACKET_END1 = 0x0D;
        private const byte PACKET_END2 = 0x0A;

        private readonly Socket socket;

        private readonly object syncSend = new object();
        private byte[] sendBuffer = new byte[1024];
        private BinaryWriter sendWriter;

        private readonly object syncReceive = new object();
        private byte[] receiveBuffer = new byte[1024];
        private int receivePosition = 0;
        private int readPosition = 0;
        private Stopwatch receiveStopwatch = new Stopwatch();
        private TimeSpan receiveTimeout;

        public Transmitter(Socket socket)
        {
            Contract.Requires(socket != null);

            this.socket = socket;
            sendWriter = new BinaryWriter(new MemoryStream(sendBuffer));
        }

        // TODO: Reduce the amount of packet instance created during send/receive operations
        //       Maybe, limited number of packets should be owned by transmitter and reused

        public void SendPacket(Packet packet)
        {
            Contract.Requires(packet != null);

            lock (syncSend)
            {
                var packetSize = packet.DataSize + PACKET_NON_DATA_SIZE;
                if (EnsureBufferSize(ref sendBuffer, packetSize, false))
                {
                    sendWriter = new BinaryWriter(new MemoryStream(sendBuffer));
                }

                // TODO: Consider using internal packet storage as send buffer
                sendWriter.BaseStream.SetLength(0);
                sendWriter.Write(PACKET_START);
                sendWriter.Write(packet.SensorId);
                sendWriter.Write((ushort)packet.Command);
                sendWriter.Write(packet.DataSize);
                if (packet.DataSize > 0)
                {
                    sendWriter.Write(packet.Data, 0, packet.DataSize);
                }
                var checksum = packet.ComputeCheckSum();
                sendWriter.Write(checksum);
                sendWriter.Write(PACKET_END1);
                sendWriter.Write(PACKET_END2);

                socket.Send(sendBuffer, packetSize, SocketFlags.None);
            }
        }

        public Packet ReceivePacket(TimeSpan timeout)
        {
            Contract.Requires(timeout >= TimeSpan.Zero);
            Contract.Ensures(Contract.Result<Packet>() != null);

            return ReadPacket(timeout, false);
        }

        /// <summary>
        /// Throw away the packets that are currently in receive buffer.
        /// </summary>
        public void DiscardPendingPackets()
        {
            lock (syncReceive)
            {
                bool wasRead = true;
                while (wasRead)
                {
                    try
                    {
                        ReadPacket(TimeSpan.Zero, true);
                    }
                    catch (TimeoutException)
                    {
                        wasRead = false;
                    }
                }
            }
        }

        private Packet ReadPacket(TimeSpan timeout, bool discard)
        {
            // TODO: Think about replacement of TimeoutException with null result
            lock (syncReceive)
            {
                receiveTimeout = timeout;
                receiveStopwatch.Restart();

                while (ReadNextByte() != PACKET_START) { }
                var packetStart = readPosition - 1;

                ushort sensorId;
                Command command;
                ushort dataSize;
                byte[] data;
                ushort checksum;
                try
                {
                    ReadPacketHeader(out sensorId, out command, out dataSize);
                    EnsureBufferSize(ref receiveBuffer, dataSize + PACKET_NON_DATA_SIZE, true);
                    data = ReadPacketData(dataSize, discard);
                    ReadPacketFooter(out checksum);
                }
                catch (TimeoutException)
                {
                    // Return to the beginning of packet, so it can be read by a later call
                    readPosition = packetStart;
                    throw;
                }

                if (!discard)
                {
                    var packet = new Packet(sensorId, command, data);
                    var computedChecksum = packet.ComputeCheckSum();
                    if (computedChecksum != checksum)
                    {
                        throw new FormatException("Packet is corrupt (sum check failed).");
                    }
                    return packet;
                }
                else
                {
                    return null;
                }
            }
        }

        private void ReadPacketHeader(out ushort sensorId, out Command command, out ushort dataSize)
        {
            sensorId = (ushort)(ReadNextByte() + (ReadNextByte() << 8));
            command = (Command)(ReadNextByte() + (ReadNextByte() << 8));
            dataSize = (ushort)(ReadNextByte() + (ReadNextByte() << 8));
        }

        private byte[] ReadPacketData(ushort dataSize, bool discard)
        {
            byte[] data = null;
            
            if (!discard)
            {
                if (dataSize > 0)
                {
                    data = new byte[dataSize];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = ReadNextByte();
                    }
                }
            }
            else
            {
                while (dataSize-- > 0)
                {
                    ReadNextByte();
                }
            }

            return data;
        }

        private void ReadPacketFooter(out ushort checksum)
        {
            checksum = (ushort)(ReadNextByte() + (ReadNextByte() << 8));

            if (ReadNextByte() != PACKET_END1 || ReadNextByte() != PACKET_END2)
            {
                throw new FormatException("Invalid packet ending.");
            }
        }

        private byte ReadNextByte()
        {
            if (readPosition == receivePosition)
            {
                while (!ReceiveNextChunk())
                {
                    var elapsed = receiveStopwatch.Elapsed;
                    if (elapsed > receiveTimeout)
                    {
                        throw new TimeoutException();
                    }
                    Thread.Sleep(1);
                }
            }

            var result = receiveBuffer[readPosition++];
            if (readPosition == receiveBuffer.Length)
            {
                readPosition = 0;
                receivePosition -= receiveBuffer.Length;
            }

            return result;
        }

        private bool ReceiveNextChunk()
        {
            Contract.Assert(readPosition == receivePosition);

            var available = socket.Available;
            if (available > 0)
            {
                var offset = receivePosition % receiveBuffer.Length;
                var sizeToReceive = Math.Min(available, receiveBuffer.Length - offset);
                var sizeReceived = socket.Receive(receiveBuffer, offset, sizeToReceive, SocketFlags.None);
                receivePosition += sizeReceived;

                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool EnsureBufferSize(ref byte[] buffer, int size, bool keepData)
        {
            if (buffer.Length < size)
            {
                var newLength = buffer.Length * 2;
                if (newLength < size)
                    newLength = size;

                var newBuffer = new byte[newLength];
                if (keepData)
                {
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                }
                buffer = newBuffer;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
