using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace LpmsB.Bluetooth
{
    internal class BluetoothEndPoint : EndPoint
    {
        public BluetoothEndPoint(BluetoothAddress address, int port)
        {
            Address = address;
            Service = Guid.Empty;
            Port = port;
        }

        public BluetoothEndPoint(BluetoothAddress address, Guid service)
        {
            Address = address;
            Service = service;
            Port = 0;
        }

        /// <summary>Creates new end point from information serialized in <see cref="SocketAddress"/>.</summary>
        /// <remarks>See <see cref="WinSockInterop.BluetoothSocketAddress"/> structure.</remarks>
        private BluetoothEndPoint(SocketAddress socketAddress)
        {
            var buffer = Enumerable.Range(0, socketAddress.Size).Select(i => socketAddress[i]).ToArray();
            Address = new BluetoothAddress(buffer, 2);
            Service = new Guid(buffer.Skip(10).Take(16).ToArray());
            Port = BitConverter.ToInt32(buffer, 26);
        }

        public BluetoothAddress Address { get; private set; }

        public Guid Service { get; private set; }

        public int Port { get; private set; }

        public override AddressFamily AddressFamily
        {
            get
            {
                return (AddressFamily)BluetoothInterop.AF_BTH;
            }
        }

        public ProtocolType ProtocolType
        {
            get 
            { 
                return (ProtocolType)BluetoothInterop.BTHPROTO_RFCOMM;
            }
        }

        /// <summary>Serializes to <see cref="SocketAddress"/>.</summary>
        /// <remarks>See <c>SOCKADDR_BTH</c> structure.</remarks>
        public override SocketAddress Serialize()
        {
            Contract.Ensures(Contract.Result<SocketAddress>() != null);

            var result = new SocketAddress(AddressFamily, 2 + 8 + 16 + 4);
            var offset = 2;
            
            var addressBytes = Address.ToEightBytes();
            for (int i = 0; i < addressBytes.Length; i++, offset++)
            {
                result[offset] = addressBytes[i];
            }
            
            var serviceBytes = Service.ToByteArray();
            for (int i = 0; i < serviceBytes.Length; i++, offset++)
            {
                result[offset] = serviceBytes[i];
            }

            var portBytes = BitConverter.GetBytes(Port);
            for (int i = 0; i < portBytes.Length; i++, offset++)
            {
                result[offset] = portBytes[i];
            }

            return result;
        }

        /// <summary>Creates new end point from information serialized in <see cref="SocketAddress"/>.</summary>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            Contract.Ensures(Contract.Result<EndPoint>() is BluetoothEndPoint);

            return new BluetoothEndPoint(socketAddress);
        }
    }
}
