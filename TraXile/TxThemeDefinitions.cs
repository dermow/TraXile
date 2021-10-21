using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TxThemeLight : TxTheme
    {
        /// <summary>
        /// Light Theme
        /// </summary>
        public TxThemeLight()
        {
            this.MainBackGroundColor = Color.LightGray;
            this.MainFontColor = Color.Black;
            this.ListBackColor = Color.White;
            this.ChartLabelColor = Color.Red;
            this.ChartSeriesColor = Color.Red;
            this.ButtonForeColor = Color.Black;
        }
    }

    public class TxThemeDark : TxTheme
    {
        /// <summary>
        /// Light Theme
        /// </summary>
        public TxThemeDark()
        {
            this.MainBackGroundColor = Color.Black;
            this.MainFontColor = Color.White;
            this.ListBackColor = Color.Black;
            this.ChartLabelColor = Color.White;
            this.ChartSeriesColor = Color.White;
            this.ButtonForeColor = Color.Black;
        }
    }
}
