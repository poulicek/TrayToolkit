using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TrayToolkit
{
    public abstract class TrayIconBase : Form
    {
        [DllImport("User32.dll")]
        private static extern int SetProcessDPIAware();

        protected NotifyIcon trayIcon;
        private readonly string aboutUrl;

        protected TrayIconBase(string title, string aboutUrl = null)
        {
            SetProcessDPIAware();

            this.Text = title;
            this.aboutUrl = aboutUrl;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Visible = false;
            this.ShowInTaskbar = false;
            this.trayIcon = this.createTrayIcon();
            this.createContextMenu();
            this.updateLook();
        }

        protected abstract void onTrayIconClick(object sender, MouseEventArgs e);


        /// <summary>
        /// Creates a tray icon
        /// </summary>
        private NotifyIcon createTrayIcon()
        {
            var trayIcon = new NotifyIcon()
            {
                Text = this.Text,
                Visible = true
            };

            trayIcon.MouseUp += this.onTrayIconClick;
            return trayIcon;
        }


        /// <summary>
        /// Returns the embedded image instance
        /// </summary>
        protected Bitmap getResourceImage(string imagePath)
        {
            using (var s = this.getResourceStream(imagePath))
                return s == null ? null : new Bitmap(s);
        }


        /// <summary>
        /// Returns the embedded resource stream
        /// </summary>
        protected Stream getResourceStream(string path)
        {
            return Assembly.GetEntryAssembly().GetManifestResourceStream($"{this.GetType().Namespace.Split('.')[0]}.{path}");
        }


        /// <summary>
        /// Returns the icon from the resource
        /// </summary>
        private Icon getIcon()
        {
            var lightMode = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true)?.GetValue("SystemUsesLightTheme") as int? == 1;
            var resourceName = this.getIconName(lightMode);
            if (string.IsNullOrEmpty(resourceName))
                return null;

            using (var bmp = getResourceImage(resourceName))
                return this.getIconFromBitmap(bmp);
        }


        /// <summary>
        /// Converts the given bitmap to an icon
        /// </summary>
        protected virtual Icon getIconFromBitmap(Bitmap bmp)
        {
            return Icon.FromHandle(bmp.GetHicon());
        }


        /// <summary>
        /// Updates the look
        /// </summary>
        protected virtual void updateLook()
        {
            this.trayIcon.Icon = this.getIcon();
        }

        protected abstract string getIconName(bool lightMode);


        protected virtual List<MenuItem> getMenuItems()
        {
            var items = new List<MenuItem>();
            items.Add(new MenuItem("Start with Windows", this.onStartUpClick) { Checked = this.startsWithWindows() });

            if (!string.IsNullOrEmpty(this.aboutUrl))
                items.Add(new MenuItem("About...", this.onAboutClick));
            items.Add(new MenuItem("-"));
            items.Add(new MenuItem("Exit", this.onMenuExitClick));

            return items;
        }


        /// <summary>
        /// Creates the context menu
        /// </summary>
        protected void createContextMenu()
        {
            this.trayIcon.ContextMenu = new ContextMenu(this.getMenuItems().ToArray());
        }


        async private void onDisplaySettingsChanged(object sender, EventArgs e)
        {
            SetProcessDPIAware();

            await Task.Delay(500);
            this.updateLook();

            await Task.Delay(1000);
            this.updateLook();
        }


        /// <summary>
        /// Setting the startup state
        /// </summary>
        protected bool startsWithWindows()
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue(this.Text) as string == Application.ExecutablePath.ToString();
            }
            catch { return false; }
        }


        /// <summary>
        /// Setting the startup state
        /// </summary>
        protected virtual bool setStartup(bool set)
        {
            try
            {
                var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (set)
                    rk.SetValue(this.Text, Application.ExecutablePath.ToString());
                else
                    rk.DeleteValue(this.Text, false);

                return set;
            }
            catch { return !set; }
        }


        protected void onStartUpClick(object sender, EventArgs e)
        {
            var btn = (sender as MenuItem);
            btn.Checked = this.setStartup(!btn.Checked);
        }


        protected virtual void onAboutClick(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(this.aboutUrl));
        }


        protected virtual void onMenuExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
