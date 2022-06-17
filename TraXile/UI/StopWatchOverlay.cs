using System;
using System.Drawing;
using System.Windows.Forms;

namespace TraXile
{
    public partial class StopWatchOverlay : Form
    {
        private readonly Main _main;
        private ImageList _images;

        public TrX_ActivityTag Tag1 => _tag1;
        private TrX_ActivityTag _tag1;

        public TrX_ActivityTag Tag2 => _tag2;
        private TrX_ActivityTag _tag2;

        public TrX_ActivityTag Tag3 => _tag3;
        private TrX_ActivityTag _tag3;

        public StopWatchOverlay(Main main, ImageList images)
        {
            InitializeComponent();
            _main = main;
            _images = images;
            pictureBox1.Image = images.Images[0];
            pictureBox2.Image = images.Images[0];

            if (_main.Logic.CurrentActivity != null)
            {
                if (_main.Logic.CurrentActivity.ManuallyPaused)
                {
                    linkLabel2.Text = "Resume";
                }
                else
                {
                    linkLabel2.Text = "Pause";
                }
            }
        }

        public void UpdateTagStatus(TrX_ActivityTag tag1, TrX_ActivityTag tag2, TrX_ActivityTag tag3, bool status1, bool status2, bool status3)
        {
            if(tag1 != null)
            { 
                label5.Text = tag1.DisplayName;
                label5.BackColor = status1 ? tag1.BackColor : Color.Gray;
                label5.ForeColor = status1 ? tag1.ForeColor : Color.LightGray;
            }
            else
            {
                label5.Text = "";
                label5.BackColor = this.BackColor;
            }

            if (tag2 != null)
            {
                label6.Text = tag2.DisplayName;
                label6.BackColor = status2 ? tag2.BackColor : Color.Gray;
                label6.ForeColor = status2 ? tag2.ForeColor : Color.LightGray;
            }
            else
            {
                label6.Text = "";
                label6.BackColor = this.BackColor;
            }

            if (tag3 != null)
            {
                label7.Text = tag3.DisplayName;
                label7.BackColor = status3 ? tag3.BackColor : Color.Gray;
                label7.ForeColor = status3 ? tag3.ForeColor : Color.LightGray;
            }
            else
            {
                label7.Text = "";
                label7.BackColor = this.BackColor;
            }
        }

        public void UpdateStopWatch(string curr, string prev, int image_idx = 0, int image_idx_prev = 0)
        {
            label1.Text = curr;
            label2.Text = prev;
            pictureBox1.Image = _images.Images[image_idx];
            pictureBox2.Image = _images.Images[image_idx_prev];

            if (_main.Logic.CurrentActivity != null)
            {
                if (_main.Logic.CurrentActivity.ManuallyPaused)
                {
                    linkLabel2.Text = "Resume";
                }
                else
                {
                    linkLabel2.Text = "Pause";
                }
            }
        }

        public ImageList Images
        {
            get { return _images; }
            set { _images = value; }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.FormBorderStyle == FormBorderStyle.None)
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                _main.AddUpdateAppSettings("overlay.stopwatch.x", this.Location.X.ToString());
                _main.AddUpdateAppSettings("overlay.stopwatch.y", this.Location.Y.ToString());
            }

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_main.Logic.CurrentActivity != null)
            {
                if (_main.Logic.CurrentActivity.ManuallyPaused)
                {
                    linkLabel2.Text = "Pause";
                    _main.ResumeCurrentActivityOrSide();
                }
                else
                {
                    linkLabel2.Text = "Resume";
                    _main.PauseCurrentActivityOrSide();
                }
            }


        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(_main.Logic.CurrentActivity != null)
            {
                _main.Logic.FinishActivity(_main.Logic.CurrentActivity, null, ACTIVITY_TYPES.MAP, new DateTime());
            }
        }
    }
}
