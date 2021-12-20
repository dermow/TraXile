﻿using System;
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
    public partial class StopWatchOverlay : Form
    {
        private Main _main;
        private ImageList _images;

        public StopWatchOverlay(Main main, ImageList images)
        {
            InitializeComponent();
            _main = main;
            _images = images;
            pictureBox1.Image = images.Images[0];
            pictureBox2.Image = images.Images[0];

            if (_main._currentActivity != null)
            {
                if (_main._currentActivity.ManuallyPaused)
                {
                    linkLabel2.Text = "Resume";
                }
                else
                {
                    linkLabel2.Text = "Pause";
                }
            }
        }

        public void UpdateStopWatch(string curr, string prev, int image_idx = 0, int image_idx_prev = 0)
        {
            label1.Text = curr;
            label2.Text = prev;
            pictureBox1.Image = _images.Images[image_idx];
            pictureBox2.Image = _images.Images[image_idx_prev];

            if (_main._currentActivity != null)
            {
                if (_main._currentActivity.ManuallyPaused)
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
            if(this.FormBorderStyle == FormBorderStyle.None)
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
            if(_main._currentActivity != null)
            {
                if(_main._currentActivity.ManuallyPaused)
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
    }
}