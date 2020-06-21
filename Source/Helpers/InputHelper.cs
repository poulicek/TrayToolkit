using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class InputHelper
    {
        public static void Press(this Keys key)
        {
            User32.keybd_event((byte)key, 0, User32.KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        public static Keys GetUnmodifiedKey(this Keys key)
        {
            return key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
        }
    }
}
