using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace TraXile.UI
{
    public partial class DebugInfo : MaterialForm
    {
        public MaterialMultiLineTextBox DebugInfoTextBox => this.materialMultiLineTextBox1;
        private Main _main;

        public DebugInfo(Main m)
        {
            InitializeComponent();
            _main = m;
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(materialMultiLineTextBox1.Text);
            materialLabel5.Text = "Copied to Clipboard!";
        }
        private void materialButton2_Click(object sender, EventArgs e)
        {
            _main.BuildSupportBundle();
            Process.Start("Explorer.exe", TrX_Static.APPDATA_PATH + @"\Support");
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            Process.Start("Explorer.exe", TrX_Static.APPDATA_PATH + @"\Support");
        }
    }
}
