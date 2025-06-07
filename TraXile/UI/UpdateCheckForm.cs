using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using MaterialSkin;
using MaterialSkin.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TraXile.UI
{
    public partial class UpdateCheckForm : MaterialForm
    {
        private bool _updateAvailable = false;
        private string _newVersion = "";
        private string _currVersion = "";
        private List<string> _changes = new List<string>();

        public UpdateCheckForm(bool updateAvailable, string currVersion, string newVersion, List<string> changes)
        {
            InitializeComponent();

            _updateAvailable = updateAvailable;
            _currVersion = currVersion;
            _newVersion = newVersion;
            _changes = changes;

            materialLabel6.Text = $"What´s new in {_newVersion}?";

        }


        public void SetState()
        {
            materialLabel3.Text = _currVersion;
            materialLabel4.Text = _newVersion;

            foreach (string s in _changes)
            {
                materialMultiLineTextBox21.Text += $"- {s}{Environment.NewLine}";
            }

            if (_updateAvailable)
            {
                materialLabel1.Text = "Update available!";
                materialButton1.Enabled = true;
            }
            else
            {
                materialLabel1.Text = "You are already up to date!";
                materialButton1.Enabled = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void UpdateCheckForm_Load(object sender, EventArgs e)
        {
            materialMultiLineTextBox21.SelectionStart = materialMultiLineTextBox21.Text.Length;
            materialMultiLineTextBox21.SelectionLength = 0;
            materialMultiLineTextBox21.TabStop = false;
            materialMultiLineTextBox21.Cursor = Cursors.Default;
        }

        private void materialMultiLineTextBox21_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialMultiLineTextBox21_Enter(object sender, EventArgs e)
        {

        }

        private void materialMultiLineTextBox21_MouseDown(object sender, MouseEventArgs e)
        {

        }
    }
}
