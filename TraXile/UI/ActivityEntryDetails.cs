using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TraXile.UI
{
    public partial class ActivityEntryDetails : MaterialForm
    {
        private TrX_TrackedActivity _activity;
        private Main _mainWindow;
        private bool _isDeleteMode;
        MaterialSkinManager _msm;

        public ActivityEntryDetails(TrX_TrackedActivity act, Main m, MaterialSkinManager msm)
        {
            InitializeComponent();

            _msm = msm;
           

            materialComboBox1.ForeColor = Color.Black;

            tableLayoutPanel1.BackColor = _msm.BackdropColor;

            _activity = act;
            _mainWindow = m;

            Text = $"{act.Type} Details: {act.Area}";

            materialLabel15.Visible = false;

            foreach (TrX_ActivityTag tag in _mainWindow.Logic.Tags)
            {
                if (!tag.IsDefault)
                {
                    materialComboBox1.Items.Add(tag.DisplayName);
                }
            }

            materialLabel7.Text = act.Type.ToString();
            materialLabel8.Text = act.Area;
            materialLabel9.Text = act.AreaLevel.ToString();
            if(act.Type == ACTIVITY_TYPES.MAP)
                materialLabel9.Text += $" (T{act.MapTier})";

            materialLabel10.Text = act.Started.ToString();
            materialLabel11.Text = act.GetCappedStopwatchValue(_mainWindow.TimeCaps[act.Type]);
            materialLabel13.Text = act.DeathCounter.ToString();



            RenderTags(false);
        }

        private void Lbl_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isDeleteMode)
            {
                TrX_ActivityTag tag = _mainWindow.GetTagByDisplayName(((System.Windows.Forms.Label)sender).Text);
                if (tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot remove auto tags.");
                }
                else
                {
                    _mainWindow.RemoveTagFromActivity(tag.ID, _activity);
                    _activity.RemoveTag(tag.ID);
                    RenderTags(true);
                    _mainWindow.ResetMapHistory();
                    _mainWindow.RequestHistoryUpdate();
                    _mainWindow.RequestDashboardUpdates();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_mainWindow.ValidateTagName(materialComboBox1.Text, true))
            {
                _mainWindow.AddTagAutoCreate(materialComboBox1.Text, _activity);
                RenderTags(true);
                _mainWindow.ResetMapHistory();
                _mainWindow.RequestHistoryUpdate();
                _mainWindow.RequestDashboardUpdates();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!_isDeleteMode)
            {
                _isDeleteMode = true;
                materialLabel15.ForeColor = Color.Red;
                materialLabel15.Show();
            }
            else
            {
                _isDeleteMode = false;
                materialLabel15.Hide();
            }
        }

        private void RenderTags(bool b_init = false)
        {
            if (b_init)
                materialCard2.Controls.Clear();

            int iOffsetX = 17;
            int ioffsetY = 35;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = 3;
            int iCurrCols = 0;

            for (int i = 0; i < _activity.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _mainWindow.GetTagByID(_activity.Tags[i]);

                if (tag == null)
                    continue;

                System.Windows.Forms.Label lbl = new System.Windows.Forms.Label() { Width = 120, Height = 35 };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 40;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                lbl.Text = tag.DisplayName;
                lbl.Name = $"lbl_tag_{tag.ID}";
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.BackColor = _msm.ColorScheme.PrimaryColor;
                lbl.ForeColor = _msm.ColorScheme.TextColor;
                lbl.Location = new Point(iX, iY);
                lbl.MouseClick += Lbl_MouseClick;
                lbl.MinimumSize = new Size(100, 18);

                iX += lbl.Width + 5;
                iCurrCols++;

                materialCard2.Controls.Add(lbl);
            }
        }

    }
}
