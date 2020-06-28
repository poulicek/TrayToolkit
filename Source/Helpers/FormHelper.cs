using System;
using System.Drawing;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class FormHelper
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
    }
}
