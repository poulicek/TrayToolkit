using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class ControlHelper
    {
        /// <summary>
        /// Gets the right bottom corner location
        /// </summary>
        public static Point GetCornerLocation(this Form form)
        {
            var screenArea = Screen.GetWorkingArea(form);
            return new Point(screenArea.Width - 24 - form.Width, screenArea.Height - 16 - form.Height);
        }



        /// <summary>
        /// Shows the unfocused form
        /// </summary>
        public static void ShowUnfocused(this Form form)
        {
            User32.ShowWindow(form.Handle, User32.SW_SHOWNOACTIVATE);
            //var loc = form.Location;
            //User32.SetWindowPos(form.Handle.ToInt32(), 0, loc.X, loc.Y, form.Width, form.Height, User32.SWP_NOACTIVATE);
            form.Invalidate();
        }


        /// <summary>
        /// Returns bounds of a notify icon
        /// </summary>
        public static Rectangle GetNotifyIconBounds(this NotifyIcon notifyIcon)
        {
            try
            {
                var id = notifyIcon.getIdentifier();
                return Shell32.Shell_NotifyIconGetRect(ref id, out User32.RECT bounds) == 0
                    ? Rectangle.FromLTRB(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom)
                    : Rectangle.Empty;
            }
            catch { return Rectangle.Empty; }
        }


        /// <summary>
        /// Returns an identifier of a notify icon
        /// </summary>
        private static Shell32.NOTIFYICONIDENTIFIER getIdentifier(this NotifyIcon notifyIcon)
        {
            var id = new Shell32.NOTIFYICONIDENTIFIER()
            {
                hWnd = notifyIcon.GetPrivateFieldValue<NativeWindow>("window").Handle,
                uID = (uint)notifyIcon.GetPrivateFieldValue<int>("id"),
            };

            id.cbSize = (uint)Marshal.SizeOf(id);
            return id;
        }
    }
}
