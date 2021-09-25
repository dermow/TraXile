using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile
{
    public partial class Settings : Form
    {
#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
        MainW main;
#pragma warning restore IDE0044 // Modifizierer "readonly" hinzufügen

        public Settings(MainW main)
        {
            InitializeComponent();
            this.main = main;

            this.textBox1.Text = main.ReadSetting("PoELogFilePath");
            this.textBox1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("With this action, the statistics will be set to 0 without reloading the log. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                main.ResetStats();
            }
        }

#pragma warning disable IDE1006 // Benennungsstile
        private void button2_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
        {
            DialogResult dr = MessageBox.Show("For this action, the application needs to be restarted. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                main.ReloadLogFile();
            }
        }

#pragma warning disable IDE1006 // Benennungsstile
        private void button3_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
        {
            DialogResult dr = MessageBox.Show("For this action, the application needs to be restarted. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                DialogResult dr2 = ofd.ShowDialog();
                if (dr2 == DialogResult.OK)
                {
                    main.AddUpdateAppSettings("PoELogFilePath", ofd.FileName);
                    main.ReloadLogFile();
                }
            }
        }
    }
}
