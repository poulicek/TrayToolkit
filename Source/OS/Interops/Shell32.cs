using System;
using System.Runtime.InteropServices;
using static TrayToolkit.OS.Interops.User32;

namespace TrayToolkit.OS.Interops
{
    public static class Shell32
    {
        public struct NOTIFYICONIDENTIFIER
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public Guid guidItem;
        }


        [DllImport("Shell32")]
        public static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);
    }
}
