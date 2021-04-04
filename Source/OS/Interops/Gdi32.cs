using System;
using System.Runtime.InteropServices;

namespace TrayToolkit.OS.Interops
{
    public static class Gdi32
    {
        public enum DeviceCap
        {
            DESKTOPVERTRES = 117,
            DESKTOPHORZRES = 118
        }


        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    }
}
