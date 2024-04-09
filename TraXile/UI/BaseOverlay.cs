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
        bool _flagMouseDown = false;
        bool _isMovable = false;
        string _id;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        Point _initalPoint;

        // Workaround for Bug with BorderStyle.None:
        // https://stackoverflow.com/questions/4163655/form-height-problem-when-formborderstyle-is-none
        internal Size _customSize = new Size(200, 27);
        internal Main _mainWindow;
       
        public Main MainWindow
        {
            get { return _mainWindow; }
            set { _mainWindow = value; }
        }

        internal Color _defaultBackColor;
        internal Color _defaultForeColor;

        public BaseOverlay()
        {
            _defaultBackColor = Color.Black;
            _defaultForeColor = Color.PeachPuff;
            this.ForeColor = _defaultForeColor;
            this.BackColor = _defaultBackColor;
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            this.MouseDown += OverlayStopwatchSimple_MouseDown;
            this.MouseUp += OverlayStopwatchSimple_MouseUp;
            this.MouseMove += OverlayStopwatchSimple_MouseMove;
            this.MouseClick += BaseOverlay_MouseClick;
            this.Load += BaseOverlay_Load1;
            
            InitializeComponent();
            _initalPoint = new Point();
        }

        internal void BaseOverlay_Load1(object sender, EventArgs e)
        {
            this.ClientSize = _customSize;
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
                _flagMouseDown = true;
                _initalPoint = e.Location;
            }
        }

        internal void OverlayStopwatchSimple_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _flagMouseDown = false;
            }
        }

        internal void OverlayStopwatchSimple_MouseMove(object sender, MouseEventArgs e)
        {
            if(_flagMouseDown && _isMovable)
            {
                this.Location = new Point(e.X + this.Left - _initalPoint.X,
                      e.Y + this.Top - _initalPoint.Y);
            }
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isMovable = !_isMovable;
            BackColor = _isMovable ? Color.LightBlue : _defaultBackColor;
            contextMenuStrip1.Items[0].Text = _isMovable ? "Fix Position" : "Move";

            if(!_isMovable)
            {
                _mainWindow.AddUpdateAppSettings($"overlay.{_id}.x", this.Location.X.ToString());
                _mainWindow.AddUpdateAppSettings($"overlay.{_id}.y", this.Location.Y.ToString());
            }
        }

        private void BaseOverlay_Load(object sender, EventArgs e)
        {

        }
    }
}
