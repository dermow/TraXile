using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile.UI
{
    public partial class UpdateDialog : Form
    {
        private bool _updateAvailable = false;
        private string _newVersion = "";
        private string _currVersion = "";
        private List<string> _changes = new List<string>();

        public UpdateDialog(bool updateAvailable, string currVersion, string newVersion, List<string> changes)
        {
            InitializeComponent();
            _updateAvailable = updateAvailable;
            _currVersion = currVersion;
            _newVersion = newVersion;
            _changes = changes;
        }

        public void SetState()
        {
            label2.Text = _currVersion;
            label3.Text = _newVersion;

            if (_updateAvailable)
            {
                label1.Text = "Update available!";
                panel1.BackColor = Color.Gray;
                panel2.BackColor = Color.Green;
                button1.Enabled = true;

                foreach(string s in _changes)
                {
                    textBox1.AppendText($"- {s}{Environment.NewLine}");
                }
            }
            else
            {
                label1.Text = "You are already up to date!";
                panel1.BackColor = Color.Green;
                panel2.BackColor = Color.Green;
                button1.Enabled = false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
