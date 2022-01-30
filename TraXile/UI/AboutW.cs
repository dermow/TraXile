using System.Windows.Forms;

namespace TraXile
{
    public partial class AboutW : Form
    {
        public AboutW()
        {
            InitializeComponent();
            label2.Text = TrX_AppInfo.VERSION;
            label1.Text = "Build: " + TrX_AppInfo.BUILDTIME.ToString();
            linkLabel1.Text = "https://github.com/dermow/TraXile/releases/latest";
            linkLabel2.Text = "https://github.com/dermow/TraXile/issues";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel2.Text);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel4.Text);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel3.Text);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel5.Text);
        }
    }
}
