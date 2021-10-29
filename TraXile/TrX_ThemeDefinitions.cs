using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TrX_ThemeLight : TrX_Theme
    {
        /// <summary>
        /// Light Theme
        /// </summary>
        public TrX_ThemeLight()
        {
            MainBackGroundColor = Color.LightGray;
            MainFontColor = Color.Black;
            ListBackColor = Color.White;
            ChartLabelColor = Color.Red;
            ChartSeriesColor = Color.Red;
            ButtonBackColor = Color.DarkGray;
            ButtonForeColor = Color.White;
            ButtonBorderColor = Color.White;
        }
    }

    public class TrX_ThemeDark : TrX_Theme
    {
        /// <summary>
        /// Dark Theme
        /// </summary>
        public TrX_ThemeDark()
        {
            MainBackGroundColor = Color.Black;
            MainFontColor = Color.White;
            ListBackColor = Color.Black;
            ChartLabelColor = Color.White;
            ChartSeriesColor = Color.White;
            ButtonForeColor = Color.Black;
            ButtonBackColor = Color.White;
            ButtonBorderColor = Color.DarkGray;
        }
    }
}
