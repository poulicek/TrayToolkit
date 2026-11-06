using System;
using System.Management;

namespace TrayToolkit.IO.Display.Driver
{
    /// <summary>
    /// Brightness access for built-in (laptop) panels. Desktops with external monitors have no such panel,
    /// so nothing is subscribed or queried unless <see cref="Start"/> finds one.
    /// </summary>
    internal class WMI : IDisposable
    {
        private ManagementEventWatcher watcher;

        public event Action<int> BrightnessChanged;

        /// <summary>
        /// Returns true when a built-in panel was found by <see cref="Start"/>
        /// </summary>
        public bool IsAvailable { get; private set; }


        /// <summary>
        /// Looks for a built-in panel and starts listening for its brightness changes.
        /// Returns the current panel brightness or -1 if there is none.
        /// </summary>
        public int Start()
        {
            var brightness = this.GetBrightness();
            if (brightness < 0)
                return -1;

            this.IsAvailable = true;

            try
            {
                this.watcher = this.createWatcher();
                this.watcher.Start();
            }
            catch { }

            return brightness;
        }


        /// <summary>
        /// Sets the brightness via WMI
        /// </summary>
        public void SetBrightness(int brightness)
        {
            if (!this.IsAvailable)
                return;

            try
            {
                lock (this)
                {
                    using (var mclass = new ManagementClass("WmiMonitorBrightnessMethods") { Scope = new ManagementScope(@"\\.\root\wmi") })
                    using (var mObjects = mclass.GetInstances())
                        foreach (ManagementObject mObj in mObjects)
                            mObj.InvokeMethod("WmiSetBrightness", new object[] { 0, brightness });
                }
            }
            catch { }
        }


        /// <summary>
        /// Gets the current brightness value
        /// </summary>
        public int GetBrightness()
        {
            try
            {
                lock (this)
                {
                    using (var searcher = new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("SELECT * FROM WmiMonitorBrightness")))
                    using (var mObjects = searcher.Get())
                    {
                        foreach (var mObj in mObjects)
                            foreach (var prop in mObj.Properties)
                                if (prop.Name == "CurrentBrightness")
                                    return (int)(byte)prop.Value;
                    }
                }
            }
            catch { }
            return -1;
        }


        /// <summary>
        /// Creates the event watcher
        /// </summary>
        private ManagementEventWatcher createWatcher()
        {
            // connecting the scope
            var scope = new ManagementScope(@"\\.\root\wmi");
            scope.Connect();

            // creating the watcher
            var watcher = new ManagementEventWatcher(scope, new EventQuery("Select * From WmiMonitorBrightnessEvent"));
            watcher.EventArrived += new EventArrivedEventHandler(this.onWMIEvent);

            return watcher;
        }


        /// <summary>
        /// Handles the WMI event
        /// </summary>
        private void onWMIEvent(object sender, EventArrivedEventArgs e)
        {
            this.BrightnessChanged?.Invoke((int)(byte)e.NewEvent.Properties["Brightness"].Value);
        }


        public void Dispose()
        {
            this.watcher?.Dispose();
        }
    }
}
