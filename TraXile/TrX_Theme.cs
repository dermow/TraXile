using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TraXile
{
    public class TrX_Theme
    {
        private Color
            _mainBackgroundColor,
            _mainFontColor,
            _listBackColor,
            _chartSeriesColor,
            _chartValueLabelForeColor,
            _buttonForeColor;
        private Color _buttonBackColor;
        private Color _buttonBorderColor;

        public Color MainBackGroundColor
        {
            get { return _mainBackgroundColor; }
            set { _mainBackgroundColor = value; }
        }

        public Color MainFontColor
        {
            get { return _mainFontColor; }
            set { _mainFontColor = value; }
        }

        public Color ListBackColor
        {
            get { return _listBackColor; }
            set { _listBackColor = value; }
        }

        public Color ChartSeriesColor
        {
            get { return _chartSeriesColor; }
            set { _chartSeriesColor = value; }
        }

        public Color ChartLabelColor
        {
            get { return _chartValueLabelForeColor; }
            set { _chartValueLabelForeColor = value; }
        }

        public Color ButtonForeColor
        {
            get { return _buttonForeColor; }
            set { _buttonForeColor = value; }
        }

        public Color ButtonBorderColor
        {
            get { return _buttonBorderColor; }
            set { _buttonBorderColor = value; }
        }

        public Color ButtonBackColor
        {
            get { return _buttonBackColor; }
            set { _buttonBackColor = value; }
        }

        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        public void Apply(Control control)
        {
            control.BackColor = _mainBackgroundColor;
            control.ForeColor = _mainFontColor;

            foreach (Control cnt in GetAll(control, typeof(Panel)))
            {
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(TableLayoutPanel)))
            {
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(GroupBox)))
            {
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(ListView)))
            {
                cnt.BackColor = _listBackColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(ListViewNF)))
            {
                cnt.BackColor = _listBackColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(Chart)))
            {
                foreach (ChartArea ca in ((Chart)cnt).ChartAreas)
                {
                    ca.BackColor = _mainBackgroundColor;
                }
                foreach (Series se in ((Chart)cnt).Series)
                {
                    if (se.ChartType != SeriesChartType.Pie)
                    {
                        se.Color = _chartSeriesColor;
                        se.LabelForeColor = _chartValueLabelForeColor;
                    }
                }
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(TabPage)))
            {
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(TextBox)))
            {
                if (((TextBox)cnt).Multiline)
                {
                    cnt.BackColor = _listBackColor;
                    cnt.ForeColor = _mainFontColor;
                }
            }

            foreach (Control cnt in GetAll(control, typeof(Label)))
            {
                // Do not overwrite special labels
                if (!cnt.Name.Contains("lbl_"))
                {
                    cnt.ForeColor = _mainFontColor;
                }
            }

            foreach (Control cnt in GetAll(control, typeof(CheckBox)))
            {
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(Button)))
            {
                Button btt = (Button)cnt;
                if (!cnt.Name.Contains("btt_"))
                {
                    btt.ForeColor = _buttonForeColor;
                    btt.BackColor = _buttonBackColor;
                }
                btt.FlatStyle = FlatStyle.Flat;
                btt.FlatAppearance.BorderSize = 1;
                btt.FlatAppearance.BorderColor = _buttonBorderColor;
            }

            foreach (Control cnt in GetAll(control, typeof(TabControl)))
            {
                TabControl tbc = (TabControl)cnt;
                tbc.BackColor = _mainBackgroundColor;
                tbc.ForeColor = _buttonForeColor;
            }

        }



    }




}
