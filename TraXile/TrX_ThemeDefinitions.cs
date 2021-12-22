using System.Drawing;

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
            MainBackGroundColor = Color.FromArgb(15, 15, 15);
            MainFontColor = Color.White;
            ListBackColor = Color.FromArgb(15, 15, 15);
            ChartLabelColor = Color.White;
            ChartSeriesColor = Color.White;
            ButtonForeColor = Color.FromArgb(15, 15, 15);
            ButtonBackColor = Color.White;
            ButtonBorderColor = Color.DarkGray;
        }
    }
}
