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
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.Text = APPINFO.NAME + " " + APPINFO.VERSION;
            label2.Text = "Version: " + APPINFO.VERSION;
            linkLabel2.Text = APPINFO.ISSUE_URL;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(APPINFO.ISSUE_URL);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
