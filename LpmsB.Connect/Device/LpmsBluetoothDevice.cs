using System;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using LpmsB.Protocol;
using LpmsB.Utils;

namespace LpmsB.Device
{
    internal class LpmsBluetoothDevice
    {
        private Socket socket;
        private volatile SocketException socketConnectException;
        private Transmitter transmitter;
        private DeviceMode mode = DeviceMode.Unknown;

        public LpmsBluetoothDevice(BluetoothAddress address)
        {
            this.address = address;

            NeedUpdateConfiguration = true;
        }

        public BluetoothAddress Address
        {
            get { return address; }
        }
        private readonly BluetoothAddress address;


        #region Connection, mode and status

        public bool IsConnected
        {
            get { return socket != null; }
        }

        public DeviceMode Mode
        {
            get { return mode; }
            set
            {
                Contract.Requires(value != DeviceMode.Unknown);
                Contract.Requires(IsConnected);

                if (value != mode)
                {
                    if (mode == DeviceMode.Stream || mode == DeviceMode.Unknown)
                    {
                        // When in stream mode, device can be switched only to command mode
                        EnterMode(Command.GotoCommandMode);
                        mode = DeviceMode.Command;
                    }
                    
                    if (value != mode)
                    {
                        EnterMode((Command)value);
                        mode = value;
                    }
                }
            }
        }

        public DeviceStatus Status
        {
            get;
            private set;
        }

        private void EnterMode(Command gotoModeCommand)
        {
            Contract.Requires(gotoModeCommand == Command.GotoCommandMode ||
                              gotoModeCommand == Command.GotoStreamMode ||
                              gotoModeCommand == Command.GotoSleepMode);

            SendAcknowledgedCommand(new Packet(1, gotoModeCommand));
        }

        public void Connect(TimeSpan timeout)
        {
            Contract.Requires(!IsConnected);
            Contract.Ensures(IsConnected);

            var endPoint = new BluetoothEndPoint(Address, 1);
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, endPoint.ProtocolType);
            socketConnectException = null;
            var asyncResult = socket.BeginConnect(endPoint, socket_ConnectCompleted, socket);
            if (!asyncResult.AsyncWaitHandle.WaitOne(timeout))
            {
                socket.Close();
                asyncResult.AsyncWaitHandle.WaitOne();
                socket = null;
                throw new SocketException((int)SocketError.TimedOut);
            }
            else if (socketConnectException != null)
            {
                socket = null;
                throw socketConnectException;
            }

            transmitter = new Transmitter(socket);

            socket.Blocking = false;
        }

        private void socket_ConnectCompleted(IAsyncResult asyncResult)
        {
            var socket = (Socket)asyncResult.AsyncState;
            try
            {
                socket.EndConnect(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                // Socket is closed already - nothing to do
            }
            catch (SocketException ex)
            {
                socketConnectException = ex;
            }
        }

        public void Disconnect()
        {
            Contract.Ensures(!IsConnected);
            Contract.Ensures(Mode == DeviceMode.Unknown);

            transmitter = null;

            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Send);
                }
                socket.Close();
                socket = null;
            }

            mode = DeviceMode.Unknown;
            NeedUpdateConfiguration = true;
        }

        public void UpdateStatus()
        {
            Contract.Requires(IsConnected);
            Contract.Requires(Mode == DeviceMode.Command);

            var request = new Packet(1, Command.GetStatus);
            var response = SendGetCommand(request);
            Status = (DeviceStatus)response.ReadUInt32();
        }

        #endregion


        #region Configuration

        private static int[] streamFrequencyTable = new[] { 5, 10, 30, 50, 100, 200, 300, 500 };

        public int StreamFrequency
        {
            get { return streamFrequency; }
            set
            {
                Contract.Requires(value > 0);
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var minDiff = int.MaxValue;
                var closestValue = streamFrequencyTable[0];
                for (int i = 0; i < streamFrequencyTable.Length; i++)
                {
                    var diff = Math.Abs(value - streamFrequencyTable[i]);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        closestValue = streamFrequencyTable[i];
                    }
                }

                var request = new Packet(1, Command.SetStreamFrequency, 4);
                request.WriteUInt32((uint)closestValue);
                SendAcknowledgedCommand(request);

                streamFrequency = closestValue;
                NeedUpdateConfiguration = true;
            }
        }
        private int streamFrequency;

        public OutputFields OutputFields
        {
            get { return outputFields; }
            set
            {
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var filteredValue = value & OutputFields.All;
                var request = new Packet(1, Command.SetTransmitData, 4);
                request.WriteUInt32((uint)filteredValue);
                SendAcknowledgedCommand(request);

                outputFields = filteredValue;
                NeedUpdateConfiguration = true;
            }
        }
        private OutputFields outputFields;

        public OperationOptions OperationOptions
        {
            get { return options; }
            set
            {
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var filteredValue = value & OperationOptions.All;

                var request = new Packet(1, Command.EnableGyroscopeAutoCalibration, 4);
                request.WriteBool((filteredValue & OperationOptions.GyroscopeAutoCalibration) == OperationOptions.GyroscopeAutoCalibration);
                SendAcknowledgedCommand(request);

                request = new Packet(1, Command.EnableGyroscopeThreshold, 4);
                request.WriteBool((filteredValue & OperationOptions.GyroscopeThreshold) == OperationOptions.GyroscopeThreshold);
                SendAcknowledgedCommand(request);

                options = filteredValue;
                NeedUpdateConfiguration = true;
            }
        }
        private OperationOptions options;

        public FilterMode FilterMode
        {
            get { return filterMode; }
            set
            {
                Contract.Requires(Enum.IsDefined(typeof(FilterMode), value));
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var request = new Packet(1, Command.SetFilterMode, 4);
                request.WriteUInt32((uint)value);
                SendAcknowledgedCommand(request);

                filterMode = value;
                NeedUpdateConfiguration = true;
            }
        }
        private FilterMode filterMode;

        public MagnetometerCorrection MagnetometerCorrection
        {
            get { return magnetometerCorrection; }
            set
            {
                Contract.Requires(Enum.IsDefined(typeof(MagnetometerCorrection), value));
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var request = new Packet(1, Command.SetFilterPreset, 4);
                request.WriteUInt32((uint)value);
                SendAcknowledgedCommand(request);

                magnetometerCorrection = value;
                NeedUpdateConfiguration = true;
            }
        }
        private MagnetometerCorrection magnetometerCorrection;

        /// <summary>
        /// Coefficient for the current value in the temporal filter, which prefilters
        /// raw values before passing to the main Kalman filter.
        /// </summary>
        /// <remarks>
        /// Value <c>1</c> effectively disables temporal filter - raw values remain unchanged.
        /// </remarks>
        public float TemporalFilterAlpha
        {
            get { return temporalFilterAlpha; }
            set
            {
                Contract.Requires(0 <= value && value <= 1);
                Contract.Requires(IsConnected);
                Contract.Requires(Mode == DeviceMode.Command);

                var request = new Packet(1, Command.SetLowPassStrength, 4);
                request.WriteSingle(value);
                SendAcknowledgedCommand(request);

                temporalFilterAlpha = value;
                NeedUpdateConfiguration = true;
            }
        }
        private float temporalFilterAlpha;

        public bool NeedUpdateConfiguration
        {
            get;
            private set;
        }

        public void UpdateConfiguration()
        {
            Contract.Requires(IsConnected);
            Contract.Requires(Mode == DeviceMode.Command);

            var response = SendGetCommand(new Packet(1, Command.GetConfiguration));
            var config = response.ReadUInt32();

            streamFrequency = streamFrequencyTable[config & 0x00000007];
            outputFields = (OutputFields)(config & (uint)OutputFields.All);
            options = (OperationOptions)(config & (uint)OperationOptions.All);

            response = SendGetCommand(new Packet(1, Command.GetFilterMode));
            var filterMode = response.ReadUInt32();
            this.filterMode = (FilterMode)filterMode;

            response = SendGetCommand(new Packet(1, Command.GetFilterPreset));
            var magCorrection = response.ReadUInt32();
            this.magnetometerCorrection = (MagnetometerCorrection)magCorrection;

            response = SendGetCommand(new Packet(1, Command.GetLowPassStrength));
            this.temporalFilterAlpha = response.ReadSingle();

            NeedUpdateConfiguration = false;

            ComputeDataPacketLayout();
        }

        #endregion


        #region Data samples

        private int dataPacketSize;
        private int rawDataOffset;
        private int quaternionOffset;

        public int RawDataSize
        {
            get
            {
                Contract.Requires(IsConnected);
                Contract.Requires(!NeedUpdateConfiguration);

                return dataPacketSize - rawDataOffset;
            }
        }

        public void ResetTimeStamp()
        {
            Contract.Requires(IsConnected);
            Contract.Requires(Mode == DeviceMode.Command);

            var request = new Packet(1, Command.ResetTimeStamp);
            SendAcknowledgedCommand(request);
        }

        /// <summary>
        /// Gets the size of the raw data in each data packet according to
        /// current output fields settings.
        /// </summary>
        /// <returns></returns>
        private void ComputeDataPacketLayout()
        {
            Contract.Requires(!NeedUpdateConfiguration);

            var test = new Packet(1, Command.GetSensorData, 1024);
            // timestamp
            test.WriteSingle(0f);
            rawDataOffset = test.DataOffset;

            if (OutputFields.IsSet(OutputFields.Gyroscope))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            if (OutputFields.IsSet(OutputFields.Accelerometer))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            if (OutputFields.IsSet(OutputFields.Magnetometer))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            if (OutputFields.IsSet(OutputFields.AngularVelocity))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            quaternionOffset = -1;
            if (OutputFields.IsSet(OutputFields.Quaternion))
            {
                quaternionOffset = test.DataOffset;
                test.WriteQuaternion(Quaternion.Zero);
            }
            if (OutputFields.IsSet(OutputFields.EulerAngles))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            if (OutputFields.IsSet(OutputFields.LinearAcceleration))
            {
                test.WriteVector3d(Vector3d.Zero);
            }
            if (OutputFields.IsSet(OutputFields.Pressure))
            {
                test.ReadSingle();
            }
            if (OutputFields.IsSet(OutputFields.Altitude))
            {
                test.ReadSingle();
            }
            if (OutputFields.IsSet(OutputFields.Temperature))
            {
                test.ReadSingle();
            }
            if (OutputFields.IsSet(OutputFields.HeaveMotion))
            {
                test.ReadSingle();
            }

            dataPacketSize = test.DataSize;
        }

        public void ReadData(out float timeStamp, out Quaternion orientation, byte[] rawDataBuffer = null)
        {
            Contract.Requires(IsConnected);
            Contract.Requires(Mode == DeviceMode.Stream);
            Contract.Requires(!NeedUpdateConfiguration);
            Contract.Requires(rawDataBuffer == null || rawDataBuffer.Length >= RawDataSize);

            Packet packet;
            do
            {
                packet = transmitter.ReceivePacket(TimeSpan.FromSeconds(0.5));
            }
            while (packet.Command != Command.GetSensorData);

            if (packet.DataSize != dataPacketSize)
                throw new FormatException("Data packet size does not correspond to data fields settings.");

            timeStamp = packet.ReadSingle();

            if (quaternionOffset >= 0)
            {
                packet.DataOffset = quaternionOffset;
                orientation = packet.ReadQuaternion();
            }
            else
            {
                orientation = Quaternion.Identity;
            }

            if (rawDataBuffer != null)
            {
                Buffer.BlockCopy(packet.Data, rawDataOffset, rawDataBuffer, 0, RawDataSize);
            }
        }

        #endregion


        #region Commands

        private void SendAcknowledgedCommand(Packet request)
        {
            if (transmitter == null)
                throw new InvalidOperationException();

            var answerCommand = Command.ReplyNegativeAcknowledge;
            do
            {
                if (answerCommand == Command.ReplyNegativeAcknowledge)
                {
                    transmitter.DiscardPendingPackets();
                    transmitter.SendPacket(request);
                }
                answerCommand = transmitter.ReceivePacket(TimeSpan.FromSeconds(3)).Command;
            }
            while (answerCommand != Command.ReplyAcknowledge);
        }

        private Packet SendGetCommand(Packet request)
        {
            if (transmitter == null)
                throw new InvalidOperationException();

            var answerCommand = Command.ReplyNegativeAcknowledge;
            Packet answer;
            do
            {
                if (answerCommand == Command.ReplyNegativeAcknowledge)
                {
                    transmitter.DiscardPendingPackets();
                    transmitter.SendPacket(request);
                }
                answer = transmitter.ReceivePacket(TimeSpan.FromSeconds(3));
                answerCommand = answer.Command;
            }
            while (answerCommand != request.Command);

            return answer;
        }

        #endregion
    }
}
