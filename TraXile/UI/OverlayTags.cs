using System;
using System.Drawing;
using System.Windows.Forms;

namespace TraXile.UI
{
    public partial class OverlayTags : BaseOverlay
    {
        private TrX_ActivityTag _tag1;
        public TrX_ActivityTag Tag1
        {
            get { return _tag1; }
            set { _tag1 = value; }
        }

        private TrX_ActivityTag _tag2;
        public TrX_ActivityTag Tag2
        {
            get { return _tag2; }
            set { _tag2 = value; }
        }

        private TrX_ActivityTag _tag3;
        public TrX_ActivityTag Tag3
        {
            get { return _tag3; }
            set { _tag3 = value; }
        }


        private TrX_TrackedActivity _currentActivity;

        public TrX_TrackedActivity CurrentActivity
        {
            get { return _currentActivity; }
            set { _currentActivity = value; }
        }

        public OverlayTags()
        {
            _customSize = new Size(295, 28);
            this.Load += OverlayAdvancedStopwatch_Load;
            this.BackColor = Color.Black;
            this.ForeColor = Color.PeachPuff;
            InitializeComponent();

            label1.AutoSize = false;
            label1.Width = 95;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Location = new Point(0, 0);

            label2.AutoSize = false;
            label2.Width = 95;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.Location = new Point(100, 0);

            label3.AutoSize = false;
            label3.Width = 95;
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label3.Location = new Point(200, 0);

            label1.MouseClick += Label1_MouseClick;
            label1.MouseMove += Label1_MouseMove;
            label1.MouseDown += Label1_MouseDown;
            label1.MouseUp += Label1_MouseUp;

            label2.MouseClick += Label1_MouseClick;
            label2.MouseMove += Label1_MouseMove;
            label2.MouseDown += Label1_MouseDown;
            label2.MouseUp += Label1_MouseUp;

            label3.MouseClick += Label1_MouseClick;
            label3.MouseMove += Label1_MouseMove;
            label3.MouseDown += Label1_MouseDown;
            label3.MouseUp += Label1_MouseUp;
        }

        private void SetLabelInactive(Label lbl)
        {
            lbl.BackColor = Color.Gray;
            lbl.ForeColor = Color.LightGray;
        }

        private void SetLabelToTag(Label lbl, TrX_ActivityTag tag)
        {
            lbl.ForeColor = tag.ForeColor;
            lbl.BackColor = tag.BackColor;
            lbl.Text = tag.DisplayName;
        }

        public void UpdateOverlay()
        {
            if (_currentActivity == null)
            {
                SetLabelInactive(label1);
                SetLabelInactive(label2);
                SetLabelInactive(label3);
            }

            if (_tag1 != null)
            {
                label1.Text = _tag1.DisplayName;

                if (_currentActivity != null && _currentActivity.HasTag(_tag1.ID))
                {
                    SetLabelToTag(label1, _tag1);
                }
                else
                {
                    SetLabelInactive(label1);
                }
            }
            else
            {
                label1.Text = "-";
                SetLabelInactive(label1);
            }

            if (_tag2 != null)
            {
                label2.Text = _tag2.DisplayName;

                if (_currentActivity != null && _currentActivity.HasTag(_tag2.ID))
                {
                    SetLabelToTag(label2, _tag2);
                }
                else
                {
                    SetLabelInactive(label2);
                }
            }
            else
            {
                label2.Text = "-";
                SetLabelInactive(label2);
            }

            if (_tag3 != null)
            {
                label3.Text = _tag3.DisplayName;

                if (_currentActivity != null && _currentActivity.HasTag(_tag3.ID))
                {
                    SetLabelToTag(label3, _tag3);
                }
                else
                {
                    SetLabelInactive(label3);
                }
            }
            else
            {
                label3.Text = "-";
                SetLabelInactive(label3);
            }
        }

        public void SetTextCurrent(string text)
        {

        }

        public void SetTextPrev(string text)
        {

        }

        private void OverlayAdvancedStopwatch_Load(object sender, EventArgs e)
        {
            base.BaseOverlay_Load1(sender, e);
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
    }
}
