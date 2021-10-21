using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TraXile
{
    public class TxTheme
    {
        private Color
            _mainBackgroundColor,
            _mainFontColor,
            _listBackColor,
            _chartSeriesColor,
            _chartValueLabelForeColor,
            _buttonForeColor;

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
                foreach(ChartArea ca in ((Chart)cnt).ChartAreas)
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
                if(((TextBox)cnt).Multiline)
                {
                    cnt.BackColor = _listBackColor;
                    cnt.ForeColor = _mainFontColor;
                }
            }

            foreach (Control cnt in GetAll(control, typeof(Label)))
            {
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(CheckBox)))
            {
                cnt.ForeColor = _mainFontColor;
            }

            foreach (Control cnt in GetAll(control, typeof(Button)))
            {
                cnt.ForeColor = _buttonForeColor;
            }

            foreach (Control cnt in GetAll(control, typeof(TabControl)))
            {
                cnt.BackColor = _mainBackgroundColor;
                cnt.ForeColor = _buttonForeColor;
            }
        }

        public Color MainBackGroundColor
        {
            get { return this._mainBackgroundColor; }
            set { this._mainBackgroundColor = value; }
        }

        public Color MainFontColor
        {
            get { return this._mainFontColor; }
            set { this._mainFontColor = value; }
        }

        public Color ListBackColor
        {
            get { return this._listBackColor; }
            set { this._listBackColor = value; }
        }

        public Color ChartSeriesColor
        {
            get { return this._chartSeriesColor; }
            set { this._chartSeriesColor = value; }
        }

        public Color ChartLabelColor
        {
            get { return this._chartValueLabelForeColor; }
            set { this._chartValueLabelForeColor = value; }
        }

        public Color ButtonForeColor
        {
            get { return this._buttonForeColor; }
            set { this._buttonForeColor = value; }
        }

    }

   

    
}
