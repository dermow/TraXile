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

        public ActivityDetails(TrackedActivity ta, MainW main)
        {
            InitializeComponent();
            mainW = main;
            activity = ta;

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
            RenderTags();
        }

        private void RenderTags()
        {
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

                Label b = new Label();

                if (iCurrCols > (iCols-1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                b.Text = tag.ID;
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.BackColor = tag.BackColor;
                b.ForeColor = tag.ForeColor;
                b.Location = new Point(iX, iY);

                iX += b.Width + 5;
                iCurrCols++;

                panelTags.Controls.Add(b);
            }
        }
    }
}
