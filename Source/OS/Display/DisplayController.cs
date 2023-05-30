using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns the current brightness value
        /// </summary>
        public int CurrentValue { get { return this.getCurrentValue(); } }


        /// <summary>
        /// Returns the monitor capabilities
        /// </summary>
        public BrightnessInfo Capabilities { get; }



        public DisplayController()
        {
            this.wmiDriver.BrightnessChanged += (level) => this.BrightnessChanged?.Invoke(level);
            this.Capabilities = this.getCapabilities();
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
        /// Sets the brightness level
        /// </summary>
        public bool SetBrightness(int brightness)
        {
            try
            {
                brightness = Math.Min(Math.Max(brightness, this.Capabilities.Minimum), this.Capabilities.Maximum);

                using (var monitors = new MonitorsCollector())
                    foreach (Dxva2.PHYSICAL_MONITOR m in monitors)
                        Dxva2.SetMonitorBrightness(m.hPhysicalMonitor, (short)brightness);

                this.wmiDriver.SetBrightness(brightness);
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
        /// Returns the initial value
        /// </summary>
        private int getCurrentValue()
        {
            var value = (int)this.getCapabilities().Current;
            if (value <= 0)
                value = this.wmiDriver.GetBrightness();

            return Math.Min(100, Math.Max(value, 0));
        }


        /// <summary>
        /// Returns the capabilities of the monitor
        /// </summary>
        private BrightnessInfo getCapabilities()
        {
            var info = new BrightnessInfo();
            try
            {
                using (var monitors = new MonitorsCollector())
                    Dxva2.GetMonitorBrightness(monitors.Primary.hPhysicalMonitor, ref info.Minimum, ref info.Current, ref info.Maximum);

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
