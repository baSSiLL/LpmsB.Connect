using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LpmsB.Bluetooth;
using LpmsB;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media;

namespace Sample
{
    public class Sensor : IDisposable, INotifyPropertyChanged
    {
        private readonly LpmsBluetoothDevice device;
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private readonly Thread thread;
        private readonly object sync = new object();
        private bool isDisposed;

        public Sensor(BluetoothAddress address)
        {
            device = new LpmsBluetoothDevice(address);
            device.Connect(TimeSpan.FromSeconds(10));
            
            device.Mode = DeviceMode.Command;
            device.FilterMode = FilterMode.GyroscopeAccelerometerMagnetometer;
            device.OutputFields = OutputFields.Quaternion;
            device.StreamFrequency = 100;
            device.UpdateConfiguration();

            device.ResetTimeStamp();
            device.Mode = DeviceMode.Stream;

            thread = new Thread(ReadData)
            {
                IsBackground = true,
                Name = "Sensor"
            };
            thread.Start();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                stopEvent.Set();
                if (thread.IsAlive)
                {
                    thread.Join();
                }

                device.Disconnect();
                isDisposed = true;
            }
        }

        public Brush Brush
        {
            get
            {
                return IsRotating ? activeBrush : inactiveBrush;
            }
        }
        private static readonly Brush activeBrush = new SolidColorBrush(Colors.LimeGreen);
        private static readonly Brush inactiveBrush = new SolidColorBrush(Colors.Gray);

        private bool IsRotating
        {
            get 
            { 
                lock (sync) return isRotating; 
            }
            set
            {
                var hasChanged = false;
                lock (sync)
                {
                    if (value != isRotating)
                    {
                        isRotating = value;
                        hasChanged = true;
                    }
                }
                
                if (hasChanged && PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Brush"));
                }
            }
        }
        private bool isRotating;

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReadData()
        {
            var prev = Quaternion.Zero;
            while (!stopEvent.WaitOne(0))
            {
                float timeStamp;
                Quaternion q;
                device.ReadData(out timeStamp, out q);
                q.Normalize();
                IsRotating = (q - prev).Length > 0.001;
                prev = q;
            }
        }
    }
}
