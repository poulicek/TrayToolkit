using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrayToolkit.Helpers;
using TrayToolkit.IO.Display.Driver;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.OS.Display
{
    public class DisplayController : IDisposable
    {
        public struct BrightnessInfo
        {
            public short Minimum;
            public short Current;
            public short Maximum;
        }

        public event Action<int> BrightnessChanged;

        private readonly WMI wmiDriver = new WMI();

        // reading the value from the monitor takes ~60 ms (DDC/CI), so it is cached and refreshed on demand
        private volatile int cachedValue = -1;

        /// <summary>
        /// Returns the last known brightness value (0 if unknown, see <see cref="IsValueKnown"/>)
        /// </summary>
        public int CurrentValue { get { return Math.Max(this.cachedValue, 0); } }


        /// <summary>
        /// Returns true if the brightness was successfully read from the display
        /// </summary>
        public bool IsValueKnown { get { return this.cachedValue >= 0; } }


        /// <summary>
        /// Returns the monitor capabilities
        /// </summary>
        public BrightnessInfo Capabilities { get; }



        public DisplayController()
        {
            this.wmiDriver.BrightnessChanged += this.onWmiBrightnessChanged;

            bool isRead;
            this.Capabilities = this.readCapabilities(out isRead);

            if (isRead)
            {
                this.cachedValue = Math.Min(100, (int)this.Capabilities.Current);

                // looking for the built-in panel without delaying the startup
                ThreadingHelper.DoAsync(() => this.wmiDriver.Start());
            }
            else
            {
                // no DDC/CI (e.g. a laptop panel)
                var value = this.wmiDriver.Start();
                if (value >= 0)
                    this.cachedValue = Math.Min(100, value);
            }
        }


        /// <summary>
        /// Turns the sceen off
        /// </summary>
        public static void TurnOffScreen()
        {
            // executing with a time limit as on system the operation runs even when though the screen geos off
            ThreadingHelper.DoAsync(() => User32.SendMessage(0xFFFF, 0x112, 0xF170, 2), 500);
        }

        #region Interface

        /// <summary>
        /// Reads the brightness from the display. Returns false (keeping the last known value) when the read fails.
        /// </summary>
        public bool Refresh()
        {
            bool isRead;
            var value = this.readCapabilities(out isRead).Current;

            if (!isRead)
                value = -1;

            if (value <= 0 && this.wmiDriver.IsAvailable)
            {
                var panelValue = this.wmiDriver.GetBrightness();
                if (panelValue >= 0)
                    value = (short)panelValue;
            }

            if (value < 0)
                return false;

            this.cachedValue = Math.Min(100, (int)value);
            return true;
        }


        /// <summary>
        /// Sets the brightness level
        /// </summary>
        public bool SetBrightness(int brightness)
        {
            try
            {
                brightness = Math.Min(Math.Max(brightness, this.Capabilities.Minimum), this.Capabilities.Maximum);

                using (var monitors = new MonitorsCollector())
                {
                    var all = monitors.All;
                    if (all.Length > 1)
                        Parallel.ForEach(all, m => Dxva2.SetMonitorBrightness(m.hPhysicalMonitor, (short)brightness));
                    else
                        foreach (var m in all)
                            Dxva2.SetMonitorBrightness(m.hPhysicalMonitor, (short)brightness);
                }

                this.wmiDriver.SetBrightness(brightness);
                this.cachedValue = brightness;
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Returns the brightness levels
        /// </summary>
        public int[] GetBrightnessLevels()
        {
            var levels = new List<int>();
            foreach (var l in new int[] { 0, 10, 30, 60, 100 })
                if (l >= this.Capabilities.Minimum && l <= this.Capabilities.Maximum)
                    levels.Add(l);

            return levels.ToArray();
        }


        /// <summary>
        /// Returns the brightness levels
        /// </summary>
        public int[] GetBrightnessLevels(int noOfLevels = 6)
        {
            var levels = new int[noOfLevels];
            var step = (this.Capabilities.Maximum - this.Capabilities.Minimum) / (noOfLevels - 1);

            for (int i = 0; i < noOfLevels; i++)
                levels[i] = this.Capabilities.Minimum + i * step;

            return levels;
        }


        /// <summary>
        /// Turns the screen off
        /// </summary>
        public void TurnOff()
        {
            TurnOffScreen();
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Handles the brightness change of the built-in panel (e.g. by the keyboard keys)
        /// </summary>
        private void onWmiBrightnessChanged(int level)
        {
            this.cachedValue = Math.Min(100, level);
            this.BrightnessChanged?.Invoke(level);
        }


        /// <summary>
        /// Returns the capabilities of the monitor
        /// </summary>
        private BrightnessInfo readCapabilities(out bool isRead)
        {
            isRead = false;
            var info = new BrightnessInfo();
            try
            {
                using (var monitors = new MonitorsCollector())
                    isRead = Dxva2.GetMonitorBrightness(monitors.Primary.hPhysicalMonitor, ref info.Minimum, ref info.Current, ref info.Maximum);

                info.Minimum = 0;
                info.Maximum = 100;
            }
            catch { }
            return info;
        }

        #endregion

        public void Dispose()
        {
            this.wmiDriver.Dispose();
        }
    }
}
