using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrayToolkit.UI
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public AboutDialog(string link) : this()
        {
            this.loadData(link);
        }

        private void loadData(string link)
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.lblProductName.Text += fvi.ProductName;
            this.lblVersion.Text += fvi.ProductVersion.ToString();
        }
    }
}
