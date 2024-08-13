using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace TraXile
{
    public partial class FileSelectScreen : Form
    {
        readonly Main _mainWindow;
        private List<string> _autoDiscovered;
        private Trx_PoeLogFileDetector _detector;

        public FileSelectScreen(Main fParent)
        {
            _mainWindow = fParent;
            this.Text = TrX_Static.NAME + " " + TrX_Static.VERSION;
            InitializeComponent();

            _autoDiscovered = new List<string>();
            _detector = new Trx_PoeLogFileDetector();

            try
            {
                _autoDiscovered = _detector.AutoDiscoverPoeLogs();
            }
            catch { }
            

            if(_autoDiscovered.Count == 0)
            {
                button1.Select();
                button2.Visible = false;

                Label lbl = new Label();
                lbl.Text = "No installations of Path of Exile could be found automatically. Try to select the file manually. See README on GitHub for help!";
                lbl.Width = 600;
                lbl.ForeColor = Color.Gold;
                lbl.Location = new Point(10, 20);
                groupBox1.Controls.Add(lbl);

                LinkLabel llbl = new LinkLabel();
                llbl.Text = "https://github.com/dermow/TraXile/blob/master/README.md";
                llbl.Click += Llbl_Click;
                llbl.Width = 600;
                llbl.ForeColor = Color.Gold;
                llbl.LinkColor = Color.Gold;
                llbl.VisitedLinkColor = Color.Gold;
                llbl.Location = new Point(10, 45);
                groupBox1.Controls.Add(llbl);
            }
            else
            {
                button2.Select();

                int x = 10;
                int y = 20;
                bool first = true;
                foreach (string s in _autoDiscovered)
                {
                    RadioButton rb = new RadioButton();
                    rb.Text = s;
                    rb.Location = new Point(x, y);
                    rb.Width = 400;
                    rb.Checked = first;
                    groupBox1.Controls.Add(rb);
                    y += 20;

                    first = false;
                }
            }
        }

        private void Llbl_Click(object sender, EventArgs e)
        {
            Process.Start(((LinkLabel)sender).Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (_mainWindow.Logic.CheckForValidClientLogFile(ofd.FileName))
                {
                    this._mainWindow.AddUpdateAppSettings("poe_logfile_path", ofd.FileName);
                    this.Close();
                }
                else
                {
                    labelDiscoverStatus.Text = "The logfile you selected does not exist or is no valid Client.txt file";
                    labelDiscoverStatus.ForeColor = Color.Red;
                }
                

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            foreach (RadioButton rb in groupBox1.Controls)
            {
                if(rb.Checked)
                {
                    path = rb.Text;
                }
            }


            if (_mainWindow.Logic.CheckForValidClientLogFile(path))
            {
                this._mainWindow.AddUpdateAppSettings("poe_logfile_path", path);
                this.Close();
            }
            else
            {
                labelDiscoverStatus.Text = "The logfile you selected does not exist or is no valid Client.txt file";
                labelDiscoverStatus.ForeColor = Color.Red;
            }
        }
    }
}
