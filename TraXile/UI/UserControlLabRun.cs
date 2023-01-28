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
    public partial class UserControlLabRun : UserControl
    {
        private TrX_TrackedLabrun _labrun;
        public TrX_TrackedLabrun LabRun
        {
            get { return _labrun; }
            set { _labrun = value; }
        }

        private List<Control> _checkboxes;
        private List<Control> _noteboxes;
        private List<Control> _infoLabels;
        private List<Control> _deleteButtons;
        private Dictionary<string, string> _rememberInputs;

        private bool _isCurrent;
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value; }
        }

        // Enchant positions
        private int lastY = 0;
        private int marginTop = 22;

        // Parent
        private Main _parent;
        

        // Combobox
        public ComboBox EnchantCombo => comboBox1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labrun"></param>
        /// <param name="current"></param>
        public UserControlLabRun(TrX_TrackedLabrun labrun, Main parent, bool current = false)
        {
            InitializeComponent();
            _labrun = labrun;
            _checkboxes = new List<Control>();
            _noteboxes = new List<Control>();
            _infoLabels = new List<Control>();
            _deleteButtons = new List<Control>();
            _parent = parent;
            _isCurrent = current;
            _rememberInputs = new Dictionary<string, string>();
            label1.Text = $"{labrun.Area}, {labrun.Started}";
            label3.Text = labrun.StopWatchValue;
            linkLabel1.LinkColor = Color.LightGreen;
            linkLabel1.VisitedLinkColor = Color.LightGreen;

            comboBox1.Hide();
            button1.Hide();

            if(_isCurrent)
            {
                label1.Font = new Font(label3.Font.FontFamily, 14, FontStyle.Regular);
                label3.Font = new Font(label3.Font.FontFamily, 12, FontStyle.Bold);
            }
            else
            {
                linkLabel1.Hide();
            }

            SetEnchants();
        }

        /// <summary>
        /// Get notes for enchants
        /// </summary>
        /// <returns></returns>
        public List<TrX_EnchantNote> GetEnchantNotes(int enchant_filter = -1)
        {
            List<TrX_EnchantNote> results;
            results = new List<TrX_EnchantNote>();

            if(_isCurrent)
            {
                TrX_EnchantNote note;
                foreach (Control control in _noteboxes)
                {
                    TextBox textBox = (TextBox)control;
                    if(!String.IsNullOrEmpty(textBox.Text))
                    {
                        note = new TrX_EnchantNote(Convert.ToInt32(textBox.Name), textBox.Text, (int)_labrun.TimeStamp);

                        if(enchant_filter == -1 || enchant_filter == Convert.ToInt32(textBox.Name))
                        {
                            results.Add(note);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get list of selected enchants
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelectedEnchants()
        {
            List<int> results;
            results = new List<int>();

            if(_isCurrent)
            {
                foreach(Control c in _checkboxes)
                {
                    CheckBox cb = (CheckBox)c;
                    if(cb.Checked)
                    {
                        results.Add(Convert.ToInt32(cb.Name));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// UPdate stopwatch
        /// </summary>
        public void UpdateInfo()
        {
            label3.Text = LabRun.StopWatchValue;
        }

        /// <summary>
        /// Add enchant to list
        /// </summary>
        /// <param name="enchant"></param>
        public void AddEnchant(TrX_LabEnchant enchant)
        {
            if (!_isCurrent)
            {
                LinkLabel l = new LinkLabel();
                l.Text = "- " + enchant.Text;
                l.Location = new Point(0, lastY + marginTop);
                l.Width = 600;
                l.Name = enchant.ID.ToString();
                l.ForeColor = Color.LightBlue;
                l.LinkColor = Color.LightBlue;
                l.VisitedLinkColor = Color.LightBlue;
                l.LinkClicked += Lbl_LinkClicked;
                //l.Dock = DockStyle.Top;
                panel3.Controls.Add(l);
                if(_labrun.EnchantsTaken.Contains(enchant))
                {
                    l.Text += " <- selected";
                }
                _checkboxes.Add(l);
                lastY = l.Location.Y;
            }
            else
            {
                CheckBox cb = new CheckBox();
                cb.Text = enchant.Text;
                cb.Name = enchant.ID.ToString();
                cb.AutoSize = true;
                cb.Location = new Point(20, lastY + marginTop);
                panel3.Controls.Add(cb);

                LinkLabel lbl = new LinkLabel();
                lbl.Text = $"found: {enchant.EnchantInfo.Found}, selected: {enchant.EnchantInfo.Taken}";
                lbl.AutoSize = true;
                lbl.Name = enchant.ID.ToString();
                lbl.ForeColor = Color.LightBlue;
                lbl.LinkColor = Color.LightBlue;
                lbl.VisitedLinkColor = Color.LightBlue;
                lbl.LinkClicked += Lbl_LinkClicked;
                lbl.Location = new Point(cb.Location.X, cb.Location.Y + 15);
                panel3.Controls.Add(lbl);
                _infoLabels.Add(lbl);

                TextBox tb = new TextBox();
                tb.Location = new Point(0, lastY + marginTop);
                tb.Name = enchant.ID.ToString();

                // Checkif there is remembered input
                if(_rememberInputs.ContainsKey(tb.Name))
                {
                    tb.Text = _rememberInputs[tb.Name];
                }
                else if(enchant.EnchantInfo != null && enchant.EnchantInfo.EnchantNotes.Count > 0)
                {
                    tb.Text = enchant.EnchantInfo.EnchantNotes.Last().Note;
                }
                
                tb.Width = 300;
                panel5.Controls.Add(tb);

                // Delete button
                LinkLabel btt = new LinkLabel();
                btt.Text = "[X]";
                btt.AutoSize = true;
                btt.Location = new Point(tb.Width, tb.Location.Y);
                btt.Name = enchant.ID.ToString();
                btt.LinkColor = Color.Red;
                btt.VisitedLinkColor = Color.Red;
                btt.LinkClicked += Btt_LinkClicked;
                panel5.Controls.Add(btt);

                lastY = tb.Location.Y + lbl.Height;

                _noteboxes.Add(tb);
                _checkboxes.Add(cb);
                _deleteButtons.Add(btt);
            }
        }

        private void Btt_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TrX_LabEnchant enchant;
            enchant = _parent.Logic.LabbieConnector.GetEnchantByID(Convert.ToInt32(((Control)sender).Name));

            if(enchant != null)
            {
                _labrun.Enchants.Remove(enchant);
                SetEnchants();
            }
        }

        private void Lbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _parent.SelectEnchant(Convert.ToInt32(((Control)sender).Name), true);
        }

        /// <summary>
        /// Set all enchants
        /// </summary>
        public void SetEnchants()
        {
            lastY = 0;

            foreach(Control c in _checkboxes)
            {
                panel3.Controls.Remove(c);
            }
            _checkboxes.Clear();

            foreach (Control c in _infoLabels)
            {
                panel3.Controls.Remove(c);
            }

            foreach (Control c in _noteboxes)
            {
                // Save comment input
                TextBox tb = (TextBox)c;
                if (!String.IsNullOrEmpty(tb.Text))
                {
                    if(_rememberInputs.ContainsKey(tb.Name))
                    {
                        _rememberInputs[tb.Name] = tb.Text;
                    }
                    else
                    {
                        _rememberInputs.Add(tb.Name, tb.Text);
                    }
                }
                panel5.Controls.Remove(c);
            }

            foreach (Control c in _deleteButtons)
            {
                panel5.Controls.Remove(c);
            }

            _checkboxes.Clear();

            foreach (TrX_LabEnchant enchant in _labrun.Enchants)
            {
                if(enchant != null)
                {
                    AddEnchant(enchant);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(comboBox1.Visible)
            {
                comboBox1.Hide();
                button1.Hide();
            }
            else
            {
                comboBox1.Show();
                button1.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TrX_LabEnchant enchant;
            enchant = _parent.Logic.LabbieConnector.GetEnchantObjectForText(comboBox1.Text);

            if(enchant == null)
            {
                enchant = new TrX_LabEnchant();
                enchant.Text = _parent.Logic.LabbieConnector.GetValidEnchantName(comboBox1.Text);
                enchant.ID = _parent.Logic.LabbieConnector.AddKnownEnchant(comboBox1.Text);
                _parent.Logic.LabbieConnector.KnownEnchants.Add(enchant);
            }

            enchant.EnchantInfo = _parent.Logic.GetEnchantInfo(enchant.ID);
            _labrun.Enchants.Add(enchant);
            AddEnchant(enchant);
            comboBox1.Text = "";
        }
    }
}
