using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TraXile
{
    public partial class ActivityDetails : Form
    {
        private readonly TrX_TrackedActivity _trackedActivity;
        private readonly Main _mainWindow;
        private bool _isDeleteMode;

        public ActivityDetails(TrX_TrackedActivity ta, Main main)
        {
            InitializeComponent();
            _mainWindow = main;
            _trackedActivity = ta;
            _isDeleteMode = false;
            this.Icon = Icon.FromHandle(((Bitmap)main.imageList1.Images[main.GetImageIndex(ta)]).GetHicon());
            pictureBox1.Image = main.imageList2.Images[main.GetImageIndex(ta)];

            labelTime.Text = ta.Started.ToString();
            labelType.Text = ta.Type.ToString();
            label12.Text = "ID: " + ta.UniqueID;
            if (ta.IsZana)
            {
                labelType.Text += " (Zana)";
            }
            labelArea.Text = ta.Area;
            labelStopWatch.Text = ta.GetCappedStopwatchValue(main.TimeCaps[ta.Type]);
            labelDeaths.Text = ta.DeathCounter.ToString();
            Text = ta.Type + " Details: " + ta.Area;
            label9.Text = ta.Type == ACTIVITY_TYPES.BREACHSTONE ? main.GetBreachStoneName(ta.Area, ta.AreaLevel) : ta.Type.ToString();
            label13.Text = ta.Area.ToString();

            if (!main.IS_IN_DEBUG_MODE)
            {
                button3.Hide();
            }

            if (_trackedActivity.AreaLevel > 0)
            {
                if (_trackedActivity.Type == ACTIVITY_TYPES.MAP)
                {
                    label10.Text = "T" + _trackedActivity.MapTier.ToString();
                }
                else
                {
                    label10.Text = "Lvl. " + _trackedActivity.AreaLevel.ToString();
                }
            }
            else
            {
                label10.Text = "unknown";
            }

            foreach (TrX_ActivityTag tag in main.Logic.Tags)
            {
                if (!tag.IsDefault)
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

            for (int i = 0; i < _trackedActivity.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _mainWindow.GetTagByID(_trackedActivity.Tags[i]);

                if (tag == null)
                    continue;

                Label lbl = new Label();

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                lbl.Text = tag.DisplayName;
                lbl.Name = "lbl_tag_" + tag.ID;
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
            if (_isDeleteMode)
            {
                TrX_ActivityTag tag = _mainWindow.GetTagByDisplayName(((Label)sender).Text);
                if (tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot remove auto tags.");
                }
                else
                {
                    _mainWindow.RemoveTagFromActivity(tag.ID, _trackedActivity);
                    _trackedActivity.RemoveTag(tag.ID);
                    RenderTags(true);
                    _mainWindow.ResetMapHistory();
                    _mainWindow.RequestHistoryUpdate();
                    _mainWindow.RequestDashboardUpdates();
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_mainWindow.ValidateTagName(comboBox1.Text, true))
            {
                _mainWindow.AddTagAutoCreate(comboBox1.Text, _trackedActivity);
                RenderTags(true);
                _mainWindow.ResetMapHistory();
                _mainWindow.RequestHistoryUpdate();
                _mainWindow.RequestDashboardUpdates();
            }
        }

        private void ActivityDetails_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!_isDeleteMode)
            {
                _isDeleteMode = true;
                label8.ForeColor = Color.Red;
            }
            else
            {
                _isDeleteMode = false;
                label8.ForeColor = Color.Black;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_trackedActivity.DebugStartEventLine);
            sb.AppendLine(_trackedActivity.DebugEndEventLine);
            sb.AppendLine(_trackedActivity.LastEnded.ToString());
            sb.Append(_trackedActivity.InstanceEndpoint);

            MessageBox.Show(sb.ToString());
        }
    }
}
