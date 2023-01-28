using System;
using System.Windows.Forms;

namespace TraXile
{
    public partial class FileSelectScreen : Form
    {
        readonly Main _mainWindow;

        public FileSelectScreen(Main fParent)
        {
            _mainWindow = fParent;
            this.Text = TrX_Static.NAME + " " + TrX_Static.VERSION;
            InitializeComponent();
            label1.Text = "TraXile is reading your Path of Exile lofgile (Client.txt) to keep track of your activities" + Environment.NewLine +
                "Please select the path to your current Logfile (normally found in <PoE_Install_Dir>/Logs/Client.txt)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this._mainWindow.AddUpdateAppSettings("poe_logfile_path", ofd.FileName);
                this.Close();
            }
        }
    }
}
