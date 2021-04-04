using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class InputHelper
    {
        public enum MouseButton { Left = 1, Middle = 2, Right = 3 }

        private static readonly int cbSize = Marshal.SizeOf(typeof(User32.INPUT));

        public static void Down(this Keys key, bool useScanCode = false)
        {
            if (useScanCode)
                User32.SendInput(1, new User32.INPUT[] { User32.INPUT.GetVirtualKeyDown(key, getScanCode(key)) }, cbSize);
            else
                User32.keybd_event((byte)key, 0, User32.KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        public static void Up(this Keys key, bool useScanCode = false)
        {
            if (useScanCode)
                User32.SendInput(1, new User32.INPUT[] { User32.INPUT.GetVirtualKeyUp(key, getScanCode(key)) }, cbSize);
            else
                User32.keybd_event((byte)key, 0, User32.KEYEVENTF_EXTENDEDKEY | User32.KEYEVENTF_KEYUP, 0);
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


        public static void MouseClick(int x, int y, MouseButton btn)
        {
            User32.SetCursorPos(x, y);
            switch (btn)
            {
                case MouseButton.Left:
                    User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                    User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                    break;

                case MouseButton.Middle:
                    User32.mouse_event(User32.MOUSEEVENTF_MIDDLEDOWN, x, y, 0, 0);
                    User32.mouse_event(User32.MOUSEEVENTF_MIDDLEUP, x, y, 0, 0);
                    break;

                case MouseButton.Right:
                    User32.mouse_event(User32.MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
                    User32.mouse_event(User32.MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
                    break;
            }
        }
    }
}
