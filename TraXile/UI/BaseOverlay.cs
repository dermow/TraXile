using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TraXile.UI
{
    public partial class BaseOverlay : Form
    {
        bool flagMouseDown = false;
        bool isMovable = false;
        string id;
        Point initalPoint;

        // Workaround for Bug with BorderStyle.None:
        // https://stackoverflow.com/questions/4163655/form-height-problem-when-formborderstyle-is-none
        internal Size customSize = new Size(200, 27);
        internal Main mainWindow;
        internal Color defaultBackColor;
        internal Color defaultForeColor;

        public BaseOverlay(Main main, string overlay_id)
        {
            mainWindow = main;
            id = overlay_id;
            defaultBackColor = Color.Black;
            defaultForeColor = Color.PeachPuff;
            this.ForeColor = defaultForeColor;
            this.BackColor = defaultBackColor;
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            this.MouseDown += OverlayStopwatchSimple_MouseDown;
            this.MouseUp += OverlayStopwatchSimple_MouseUp;
            this.MouseMove += OverlayStopwatchSimple_MouseMove;
            this.MouseClick += BaseOverlay_MouseClick;
            this.Load += BaseOverlay_Load1;
            
            InitializeComponent();
            initalPoint = new Point();
        }

        internal void BaseOverlay_Load1(object sender, EventArgs e)
        {
            this.ClientSize = customSize;
        }

        internal void BaseOverlay_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, e.Location);
            }
        }

        internal void OverlayStopwatchSimple_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                flagMouseDown = true;
                initalPoint = e.Location;
            }
        }

        internal void OverlayStopwatchSimple_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                flagMouseDown = false;
            }
        }

        internal void OverlayStopwatchSimple_MouseMove(object sender, MouseEventArgs e)
        {
            if(flagMouseDown && isMovable)
            {
                this.Location = new Point(e.X + this.Left - initalPoint.X,
                      e.Y + this.Top - initalPoint.Y);
            }
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isMovable = !isMovable;
            BackColor = isMovable ? Color.LightBlue : defaultBackColor;
            contextMenuStrip1.Items[0].Text = isMovable ? "Fix Position" : "Move";

            if(!isMovable)
            {
                mainWindow.AddUpdateAppSettings($"overlay.{id}.x", this.Location.X.ToString());
                mainWindow.AddUpdateAppSettings($"overlay.{id}.y", this.Location.Y.ToString());
            }
        }

        private void BaseOverlay_Load(object sender, EventArgs e)
        {

        }
    }
}
