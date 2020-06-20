using System.Drawing;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class FormHelper
    {
        /// <summary>
        /// Shows the unfocused form
        /// </summary>
        public static void ShowUnfocused(this Form form)
        {
            var loc = form.Location;
            User32.SetWindowPos(form.Handle.ToInt32(), 0, loc.X, loc.Y, form.Width, form.Height, User32.SWP_NOACTIVATE);
            User32.ShowWindow(form.Handle, User32.SW_SHOWNOACTIVATE);
            form.Invalidate();
        }
    }
}
