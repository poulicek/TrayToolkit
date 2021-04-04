using System.Drawing;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class SystemHelper
    {
        public static Size GetScaledScreenSize()
        {
            using (var g = Graphics.FromHwnd(User32.GetDesktopWindow()))
            {
                var hdc = g.GetHdc();
                return new Size(
                    Gdi32.GetDeviceCaps(hdc, (int)Gdi32.DeviceCap.DESKTOPHORZRES),
                    Gdi32.GetDeviceCaps(hdc, (int)Gdi32.DeviceCap.DESKTOPVERTRES));
            }
        }
    }
}
