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
    public partial class OverlaySimpleStopwatch : BaseOverlay
    {
        public OverlaySimpleStopwatch()
        {
            _customSize = new Size(115, 28);
            this.Load += OverlaySimpleStopwatch_Load;
            this.BackColor = Color.Black;
            this.ForeColor = Color.PeachPuff;
            InitializeComponent();

            // Interact with click on filling lable, same as click on base form
            label1.MouseDown += Label1_MouseDown;
            label1.MouseUp += Label1_MouseUp;
            label1.MouseMove += Label1_MouseMove;
            label1.MouseClick += Label1_MouseClick;
        }

        private void Label1_MouseClick(object sender, MouseEventArgs e)
        {
            base.BaseOverlay_MouseClick(sender, e);
        }

        private void Label1_MouseMove(object sender, MouseEventArgs e)
        {
            base.OverlayStopwatchSimple_MouseMove(sender, e);
        }

        private void Label1_MouseUp(object sender, MouseEventArgs e)
        {
            base.OverlayStopwatchSimple_MouseUp(sender, e);
        }

        private void Label1_MouseDown(object sender, MouseEventArgs e)
        {
            base.OverlayStopwatchSimple_MouseDown(sender, e);
        }

        private void OverlaySimpleStopwatch_Load(object sender, EventArgs e)
        {
            base.BaseOverlay_Load1(sender, e);
            label1.AutoSize = false;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Dock = DockStyle.Fill;
        }

        public void SetText(string text)
        {
            label1.Text = text;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


    }
}
