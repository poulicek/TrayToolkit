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


        /// <summary>
        /// Converts the given character to key code
        /// </summary>
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
