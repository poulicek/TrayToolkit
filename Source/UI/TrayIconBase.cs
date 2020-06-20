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
using TrayToolkit.Helpers;

namespace TrayToolkit.UI
{
    public abstract class TrayIconBase : Form
    {
        [DllImport("User32.dll")]
        private static extern int SetProcessDPIAware();

        protected NotifyIcon trayIcon;
        private bool redrawRequested;
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

            SystemEvents.PaletteChanged += this.onDisplaySettingsChanged;
            SystemEvents.DisplaySettingsChanged += this.onDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanging += this.onDisplaySettingsChanged;
        }


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


        protected abstract void onTrayIconClick(object sender, MouseEventArgs e);


        async private void onDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (this.redrawRequested)
                return;

            this.redrawRequested = true;
            await Task.Delay(200);
            this.updateLook();
            this.redrawRequested = false;
        }


        /// <summary>
        /// Updates the look
        /// </summary>
        protected virtual void updateLook()
        {
            SetProcessDPIAware();
            var oldIcon = this.trayIcon.Icon;
            this.trayIcon.Icon = this.getIcon();
            oldIcon?.Dispose();
        }


        /// <summary>
        /// Returns true if Windows is in a light mode
        /// </summary>
        protected bool isWindowsInLightMode()
        {
            return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true)?.GetValue("SystemUsesLightTheme") as int? == 1;
        }


        /// <summary>
        /// Sets the icon title
        /// </summary>
        protected void setTitle(string title)
        {
            if (this.InvokeRequired)
                this.BeginInvoke((Action)delegate () { this.Text = this.trayIcon.Text = title; });
            else
                this.Text = this.trayIcon.Text = title;
        }


        /// <summary>
        /// Returns the icon from the resource
        /// </summary>
        private Icon getIcon()
        {
            var resourceName = this.getIconName(this.isWindowsInLightMode());
            if (string.IsNullOrEmpty(resourceName))
                return null;

            using (var bmp = ResourceHelper.GetResourceImage(resourceName))
                return this.getIconFromBitmap(bmp);
        }


        /// <summary>
        /// Returns the resource name of the icon file
        /// </summary>
        protected abstract string getIconName(bool lightMode);


        /// <summary>
        /// Converts the given bitmap to an icon
        /// </summary>
        protected virtual Icon getIconFromBitmap(Bitmap bmp)
        {
            return IconHelper.GetIcon(bmp);
        }


        #region Context Menu

        /// <summary>
        /// Creates the context menu
        /// </summary>
        protected void createContextMenu()
        {
            this.trayIcon.ContextMenu = new ContextMenu(this.getContextMenuItems().ToArray());
        }


        protected virtual List<MenuItem> getContextMenuItems()
        {
            var items = new List<MenuItem>();
            items.Add(new MenuItem("Start with Windows", this.onStartUpClick) { Checked = this.startsWithWindows() });

#if DEBUG
            items.Add(new MenuItem("Save icon...", this.onSaveIconClick));
#endif
            items.Add(new MenuItem("About...", this.onAboutClick));
            items.Add(new MenuItem("-"));
            items.Add(new MenuItem("Exit", this.onMenuExitClick));

            return items;
        }


        /// <summary>
        /// Saves the current icon
        /// </summary>
        private void onSaveIconClick(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog() { InitialDirectory = FileSystemHelper.AppFolder, FileName = "TrayIcon.ico", Filter = "Icon Files | *.ico", AddExtension = true, DefaultExt = ".ico" })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                using (var stream = new FileStream(dlg.FileName, FileMode.Create))
                using (var icon = this.getIcon())
                    icon.Save(stream);
            }
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
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            BalloonTooltip.Show(fvi.ProductName, null, $"{fvi.ProductVersion}{Environment.NewLine}{this.aboutUrl} ");
        }


        protected virtual void onMenuExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            SystemEvents.PaletteChanged -= this.onDisplaySettingsChanged;
            SystemEvents.DisplaySettingsChanged -= this.onDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanging -= this.onDisplaySettingsChanged;

            base.Dispose(disposing);
        }
    }
}
