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
    public partial class FileSelectScreen : Form
    {
        readonly MainW parent;

        public FileSelectScreen(MainW fParent)
        {
            parent = fParent;
            this.Text = APPINFO.NAME + " " + APPINFO.VERSION;
            InitializeComponent();
            label1.Text = "TraXile is tracking your Path of Exile lofgile (Client.txt) to keep track of your activities" + Environment.NewLine +
                "Please select the path to your current Logfile (normally found in <PoE_Install_dir>/Logs/Client.txt)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                this.parent.sPoELogFilePath = ofd.FileName;
                this.parent.AddUpdateAppSettings("PoELogFilePath", ofd.FileName);
                this.Close();
            }
        }
    }
}
