using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class ScreenHelper
    {
        public static Rectangle GetScaledBounds(this Screen screen)
        {
            var hMonitor = screen.GetHMonitor();
            var mi = new User32.MONITORINFOEX() { Size = Marshal.SizeOf(typeof(User32.MONITORINFOEX)) };

            if (!User32.GetMonitorInfo(hMonitor, ref mi))
                return Rectangle.Empty;

            var pt = new Point(mi.Monitor.Left, mi.Monitor.Top);
            var size = getScaledScreenSize(User32.WindowFromPoint(pt));

            return new Rectangle(pt, size);
        }


        public static IntPtr GetHMonitor(this Screen screen)
        {
            var field = typeof(Screen).GetField("hmonitor", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IntPtr)field.GetValue(screen);
        }


        private static Size getScaledScreenSize(IntPtr hwnd)
        {
            using (var g = Graphics.FromHwnd(hwnd))
            {
                var hdc = g.GetHdc();
                return new Size(
                    Gdi32.GetDeviceCaps(hdc, (int)Gdi32.DeviceCap.DESKTOPHORZRES),
                    Gdi32.GetDeviceCaps(hdc, (int)Gdi32.DeviceCap.DESKTOPVERTRES));
            }
        }
    }
}
