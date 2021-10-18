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
        readonly MainW _mainWindow;

        public FileSelectScreen(MainW fParent)
        {
            _mainWindow = fParent;
            this.Text = APPINFO.NAME + " " + APPINFO.VERSION;
            InitializeComponent();
            label1.Text = "TraXile is reading your Path of Exile lofgile (Client.txt) to keep track of your activities" + Environment.NewLine +
                "Please select the path to your current Logfile (normally found in <PoE_Install_Dir>/Logs/Client.txt)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                this._mainWindow.SettingPoeLogFilePath = ofd.FileName;
                this._mainWindow.AddUpdateAppSettings("PoELogFilePath", ofd.FileName);
                this.Close();
            }
        }
    }
}
