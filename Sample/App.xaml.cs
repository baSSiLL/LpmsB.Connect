using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using LpmsB;

namespace Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IList<Sensor> Sensors { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var addresses = LpmsBluetoothDevice.Enumerate(TimeSpan.FromSeconds(15));
            if (addresses.Length == 0)
            {
                MessageBox.Show("No sensors detected.");
                Shutdown();
            }

            Sensors = addresses.Select(a => new Sensor(a)).ToArray();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Sensors != null)
            {
                foreach (var s in Sensors)
                {
                    s.Dispose();
                }
            }

            base.OnExit(e);
        }
    }
}
