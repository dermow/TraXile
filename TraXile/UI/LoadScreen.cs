using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TraXile
{
    public partial class LoadScreen : Form
    {
        public LoadScreen()
        {
            InitializeComponent();
            this.Text = TrX_Static.NAME + " " + TrX_Static.VERSION;
            progressBar1.SetState(2);
        }

        public ProgressBar progressBar
        {
            get { return this.progressBar1; }
        }

        public Label progressLabel
        {
            get { return this.label1; }
        }

        public Label progressLabel2
        {
            get { return this.label3; }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }
}
