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
    public partial class ActivityDetails : Form
    {
        private TrackedActivity activity;
        private MainW mainW;
        private bool bDeleteMode;

        public ActivityDetails(TrackedActivity ta, MainW main)
        {
            InitializeComponent();
            mainW = main;
            activity = ta;
            bDeleteMode = false;

            labelTime.Text = ta.Started.ToString();
            labelType.Text = ta.Type.ToString();
            if(ta.IsZana)
            {
                labelType.Text += " (Zana)";
            }
            labelArea.Text = ta.Area;
            labelStopWatch.Text = ta.StopWatchValue;
            labelDeaths.Text = ta.DeathCounter.ToString();
            Text = ta.Type + " Details: " + ta.Area;
            label9.Text = ta.Type.ToString();

            if(activity.AreaLevel > 0)
            {
                if(activity.Type == ACTIVITY_TYPES.MAP)
                {
                    label10.Text = "T" + activity.MapTier.ToString();
                }
                else
                {
                    label10.Text = "Lvl. " + activity.AreaLevel.ToString();
                }
            }
            else
            {
                label10.Text = "unknown";
            }

            foreach(ActivityTag tag in main.Tags)
            {
                if(!tag.IsDefault)
                {
                    comboBox1.Items.Add(tag.DisplayName);
                }
            }

            RenderTags();
        }

        private void RenderTags(bool b_init = false)
        {
            if (b_init)
                panelTags.Controls.Clear();

            int iOffsetX = 10;
            int ioffsetY = 10;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = 3;
            int iCurrCols = 0;

            for(int i = 0; i < activity.Tags.Count; i++)
            {
                ActivityTag tag = mainW.GetTagByID(activity.Tags[i]);

                if (tag == null)
                    continue;

                Label lbl = new Label();

                if (iCurrCols > (iCols-1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                lbl.Text = tag.DisplayName;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.BackColor = tag.BackColor;
                lbl.ForeColor = tag.ForeColor;
                lbl.Location = new Point(iX, iY);
                lbl.MouseClick += Lbl_MouseClick;

                iX += lbl.Width + 5;
                iCurrCols++;

                panelTags.Controls.Add(lbl);
            }
        }

        private void Lbl_MouseClick(object sender, MouseEventArgs e)
        {
            if(bDeleteMode)
            {
                ActivityTag tag = mainW.GetTagByDisplayName(((Label)sender).Text);
                if(tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot remove auto tags.");
                }
                else
                {
                    mainW.RemoveTagFromActivity(tag.ID, activity);
                    activity.RemoveTag(tag.ID);
                    RenderTags(true);
                    mainW.ResetMapHistory();
                }
               
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (mainW.ValidateTagName(comboBox1.Text, true))
            {
                mainW.AddTagAutoCreate(comboBox1.Text, activity);
                RenderTags(true);
                mainW.ResetMapHistory();
            }
        }

        private void ActivityDetails_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(!bDeleteMode)
            {
                bDeleteMode = true;
                label8.ForeColor = Color.Red;
            }
            else
            {
                bDeleteMode = false;
                label8.ForeColor = Color.Black;
            }
        }
    }
}
