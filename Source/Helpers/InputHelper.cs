using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class InputHelper
    {
        public static void Down(this Keys key)
        {
            var inputs = new User32.INPUT[] { User32.INPUT.VirtualKeyDown(key, getScanCode(key)) };
            User32.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        }

        public static void Up(this Keys key)
        {
            var inputs = new User32.INPUT[] { User32.INPUT.VirtualKeyUp(key, getScanCode(key)) };
            User32.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        }

        public static Keys GetUnmodifiedKey(this Keys key)
        {
            return key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
        }


        private static ushort getScanCode(Keys key)
        {
            switch (key)
            {
                case Keys.X: return 0x2D;
                case Keys.Up: return 0xC8;
                case Keys.Left: return 0xCB;
                case Keys.Right: return 0xCD;
                case Keys.Down: return 0xD0;
            }

            return 0;
        }
    }
}
