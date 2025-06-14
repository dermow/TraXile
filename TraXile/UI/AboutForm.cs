﻿using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace TraXile.UI
{
    public partial class AboutForm : MaterialForm
    {
        public PictureBox pictureBox => this.pictureBox1;

        public AboutForm(string theme)
        {
            InitializeComponent();

            if(theme == "DARK")
            {
                pictureBox1.Image = Properties.Resources.logo_white;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.logo_bk;
            }

            materialLabel2.Text = $"{TrX_Static.VERSION} (Build: {TrX_Static.BUILDTIME})";
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((LinkLabel)sender).Text);
        }
    }
}
