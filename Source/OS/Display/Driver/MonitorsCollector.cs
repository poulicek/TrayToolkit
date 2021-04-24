using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.IO.Display.Driver
{
    internal class MonitorsCollector : IEnumerable, IDisposable
    {
        private readonly Dxva2.PHYSICAL_MONITOR primaryMonitor;
        private readonly Dxva2.PHYSICAL_MONITOR[] monitors;


        /// <summary>
        /// Returns the primary monitor
        /// </summary>
        public Dxva2.PHYSICAL_MONITOR Primary { get { return this.primaryMonitor; } }



        public MonitorsCollector()
        {
            this.monitors = this.getMonitors(out this.primaryMonitor);
        }


        /// <summary>
        /// Setting up the monitors
        /// </summary>
        private Dxva2.PHYSICAL_MONITOR[] getMonitors(out Dxva2.PHYSICAL_MONITOR primaryMonitor)
        {
            primaryMonitor = new Dxva2.PHYSICAL_MONITOR();
            var monitors = new List<Dxva2.PHYSICAL_MONITOR>();

            try
            {                
                var field = typeof(System.Windows.Forms.Screen).GetField("hmonitor", BindingFlags.NonPublic | BindingFlags.Instance);

                // enumerating the screens
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    var hMonitor = (IntPtr)field.GetValue(screen);

                    // getting the number of monitors
                    uint noOfMonitors = 0;
                    if (Dxva2.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref noOfMonitors))
                    {
                        // loading the monitor instances
                        var screenMonitors = new Dxva2.PHYSICAL_MONITOR[noOfMonitors];
                        Dxva2.GetPhysicalMonitorsFromHMONITOR(hMonitor, noOfMonitors, screenMonitors);
                        monitors.AddRange(screenMonitors);

                        // setting the primary monitor
                        if (screen.Primary && screenMonitors.Length > 0)
                            primaryMonitor = screenMonitors[0];
                    }
                }
            }
            catch { }
            return monitors.ToArray();
        }


        public IEnumerator GetEnumerator()
        {
            return this.monitors.GetEnumerator();
        }


        public void Dispose()
        {
            Dxva2.DestroyPhysicalMonitors((uint)this.monitors.Length, this.monitors);
        }
    }
}
