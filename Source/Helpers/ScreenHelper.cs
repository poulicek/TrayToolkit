using System;
using System.Drawing;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class ScreenHelper
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


        public static Bitmap CaptureScreen(Rectangle r)
        {
            var hBitmap = IntPtr.Zero;
            try
            {
                using (var gSrc = Graphics.FromHwnd(User32.GetDesktopWindow()))
                {
                    var hdcSrc = gSrc.GetHdc();
                    hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, r.Width, r.Height);

                    using (var gDst = Graphics.FromHdc(Gdi32.CreateCompatibleDC(hdcSrc)))
                    {
                        var hdcDst = gDst.GetHdc();
                        Gdi32.SelectObject(hdcDst, hBitmap);
                        Gdi32.BitBlt(hdcDst, 0, 0, r.Width, r.Height, hdcSrc, r.X, r.Y, 0x00CC0020);
                    }

                    return Bitmap.FromHbitmap(hBitmap);
                }
            }
            finally
            {
                Gdi32.DeleteObject(hBitmap);
            }
        }
    }
}
