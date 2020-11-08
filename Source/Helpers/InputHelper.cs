using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class InputHelper
    {
        private static readonly int cbSize = Marshal.SizeOf(typeof(User32.INPUT));

        public static void Down(this Keys key, bool useScanCode = false)
        {
            User32.keybd_event((byte)key, 0, User32.KEYEVENTF_EXTENDEDKEY | 0, 0);

            if (useScanCode)
            {
                var scanCode = getScanCode(key);
                if (scanCode > 0)
                    User32.SendInput(1, new User32.INPUT[] { User32.INPUT.GetVirtualKeyDown(key, scanCode) }, cbSize);
            }
        }

        public static void Up(this Keys key, bool useScanCode = false)
        {
            User32.keybd_event((byte)key, 0, User32.KEYEVENTF_KEYUP | 0, 0);

            if (useScanCode)
            {
                var scanCode = getScanCode(key);
                if (scanCode > 0)
                    User32.SendInput(1, new User32.INPUT[] { User32.INPUT.GetVirtualKeyUp(key, scanCode) }, cbSize);
            }
        }


        public static Keys GetUnmodifiedKey(this Keys key)
        {
            return key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
        }


        public static bool IsNumeric(this Keys key)
        {
            return (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }

        public static Keys ToSmallNumeric(this Keys key)
        {
            return key >= Keys.NumPad0 ? key - 48 : key;
        }

        private static ushort getScanCode(Keys key)
        {
            return (ushort)User32.MapVirtualKeyEx((uint)key, User32.MAPVK_VK_TO_VSC, IntPtr.Zero);
        }
    }
}
