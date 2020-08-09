using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class InputHelper
    {
        public static void Down(this Keys key)
        {
            var inputs = new User32.INPUT[] { User32.INPUT.VirtualKeyDown(key) };
            User32.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        }

        public static void Up(this Keys key)
        {
            var inputs = new User32.INPUT[] { User32.INPUT.VirtualKeyUp(key) };
            User32.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(User32.INPUT)));
        }

        public static Keys GetUnmodifiedKey(this Keys key)
        {
            return key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
        }


    }
}
