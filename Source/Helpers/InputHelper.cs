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
        public enum MouseClickMode { Down = 1, Up = 2, Click = 3 }

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


        public static void SendUnicodeChar(char input)
        {
            var inputStruct = new User32.INPUT();
            inputStruct.type = User32.INPUTTYPE.Keyboard;
            inputStruct.u.ki.virtualKey = 0;
            inputStruct.u.ki.scanCode = input;
            inputStruct.u.ki.time = 0;
            inputStruct.u.ki.flags = User32.KEYEVENTF.UNICODE;
            inputStruct.u.ki.extraInfo = User32.GetMessageExtraInfo();

            User32.INPUT[] ip = { inputStruct };
            User32.SendInput(1, ip, Marshal.SizeOf(inputStruct));
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


        public static void SetCursorPosition(int x, int y)
        {
            User32.SetCursorPos(x, y);
        }


        public static Point GetCursorPosition()
        {
            return User32.GetCursorPos(out var pt) ? new Point(pt.X, pt.Y) : Point.Empty;
        }


        public static void MouseAction(MouseButton btn, Point pt, MouseClickMode mode)
        {
            if (pt.IsEmpty)
                pt = GetCursorPosition();
            else
                User32.SetCursorPos(pt.X, pt.Y);

            switch (btn)
            {
                case MouseButton.Left:

                    if (mode.HasFlag(MouseClickMode.Down))
                        User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, pt.X, pt.Y, 0, 0);

                    if (mode.HasFlag(MouseClickMode.Up))
                        User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, pt.X, pt.Y, 0, 0);

                    break;

                case MouseButton.Middle:
                    
                    if (mode.HasFlag(MouseClickMode.Down))
                        User32.mouse_event(User32.MOUSEEVENTF_MIDDLEDOWN, pt.X, pt.Y, 0, 0);
                    
                    if (mode.HasFlag(MouseClickMode.Up))
                        User32.mouse_event(User32.MOUSEEVENTF_MIDDLEUP, pt.X, pt.Y, 0, 0);
                    
                    break;

                case MouseButton.Right:
                    
                    if (mode.HasFlag(MouseClickMode.Down))
                        User32.mouse_event(User32.MOUSEEVENTF_RIGHTDOWN, pt.X, pt.Y, 0, 0);
                    
                    if (mode.HasFlag(MouseClickMode.Up))
                        User32.mouse_event(User32.MOUSEEVENTF_RIGHTUP, pt.X, pt.Y, 0, 0);

                    break;
            }
        }


        public static void MouseClick(MouseButton btn)
        {
            MouseAction(btn, Point.Empty, MouseClickMode.Click);
        }


        public static void MouseDown(MouseButton btn)
        {
            MouseAction(btn, Point.Empty, MouseClickMode.Down);
        }


        public static void MouseUp(MouseButton btn)
        {
            MouseAction(btn, Point.Empty, MouseClickMode.Up);
        }


        public static void MouseScroll(int x, int y)
        {
            if (x != 0)
                User32.mouse_event(User32.MOUSEEVENTF_HWHEEL, 0, 0, -x, 0);

            if (y != 0)
                User32.mouse_event(User32.MOUSEEVENTF_WHEEL, 0, 0, y, 0);
        }


        public static Keys CharToKey(char ch)
        {
            var vkey = User32.VkKeyScan(ch);
            var retval = (Keys)(vkey & 0xff);
            var modifiers = vkey >> 8;

            if ((modifiers & 1) != 0) retval |= Keys.Shift;
            if ((modifiers & 2) != 0) retval |= Keys.Control;
            if ((modifiers & 4) != 0) retval |= Keys.Alt;

            return retval;
        }
    }
}
