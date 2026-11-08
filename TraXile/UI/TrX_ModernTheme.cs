using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MaterialSkin;

namespace TraXile.UI
{
    /// <summary>
    /// Central place for the modern look &amp; feel: color palette, chip colors,
    /// chart styling and Windows 11 window chrome (rounded corners, dark title bar).
    /// </summary>
    internal static class TrX_ModernTheme
    {
        // ---------------------------------------------------------------
        // Brand palette
        // ---------------------------------------------------------------

        // Dark theme: neutral graphite header with a warm "exalted gold" accent
        public static readonly Color DarkPrimary = Hex(0x1F2023);
        public static readonly Color DarkPrimaryDark = Hex(0x17181A);
        public static readonly Color DarkPrimaryLight = Hex(0x2B2D31);
        public static readonly Color DarkAccent = Hex(0xF5B942);

        // Light theme: modern indigo header with an amber accent
        public static readonly Color LightPrimary = Hex(0x4F5BD5);
        public static readonly Color LightPrimaryDark = Hex(0x3A44A8);
        public static readonly Color LightPrimaryLight = Hex(0xE0E4FF);
        public static readonly Color LightAccent = Hex(0xF59E0B);

        // Text color used on top of light/pastel fills (chart slices, accent chips)
        public static readonly Color OnLightFill = Hex(0x18181B);

        /// <summary>
        /// Soft, harmonious palette (Tailwind 400 tones) used for categorical data.
        /// Works on both dark and light backgrounds.
        /// </summary>
        public static class Palette
        {
            public static readonly Color Emerald = Hex(0x34D399);
            public static readonly Color Rose = Hex(0xFB7185);
            public static readonly Color Amber = Hex(0xFBBF24);
            public static readonly Color Cyan = Hex(0x22D3EE);
            public static readonly Color Slate = Hex(0x94A3B8);
            public static readonly Color Teal = Hex(0x2DD4BF);
            public static readonly Color Violet = Hex(0xA78BFA);
            public static readonly Color Orange = Hex(0xFB923C);
            public static readonly Color Fuchsia = Hex(0xE879F9);
            public static readonly Color Blue = Hex(0x60A5FA);
            public static readonly Color Indigo = Hex(0x818CF8);
            public static readonly Color Sky = Hex(0x38BDF8);
            public static readonly Color Lime = Hex(0xA3E635);
            public static readonly Color Red = Hex(0xF87171);
            public static readonly Color Purple = Hex(0xC084FC);
            public static readonly Color Pink = Hex(0xF472B6);
            public static readonly Color Stone = Hex(0xD6D3D1);
            public static readonly Color Bronze = Hex(0xC8A27A);
            public static readonly Color Green = Hex(0x4ADE80);
        }

        public static bool IsDark
        {
            get { return MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK; }
        }

        /// <summary>
        /// Build the MaterialSkin color scheme for the given theme.
        /// </summary>
        public static ColorScheme CreateColorScheme(MaterialSkinManager.Themes theme)
        {
            if (theme == MaterialSkinManager.Themes.LIGHT)
            {
                return new ColorScheme(LightPrimary, LightPrimaryDark, LightPrimaryLight, LightAccent, TextShade.WHITE);
            }
            return new ColorScheme(DarkPrimary, DarkPrimaryDark, DarkPrimaryLight, DarkAccent, TextShade.WHITE);
        }

        /// <summary>
        /// Apply theme + matching color scheme to the skin manager.
        /// </summary>
        public static void Apply(MaterialSkinManager msm, MaterialSkinManager.Themes theme)
        {
            msm.Theme = theme;
            msm.ColorScheme = CreateColorScheme(theme);
        }

        // ---------------------------------------------------------------
        // Semantic colors (theme aware)
        // ---------------------------------------------------------------

        public static Color Accent
        {
            get { return IsDark ? DarkAccent : LightAccent; }
        }

        /// <summary>Theme aware primary text color.</summary>
        public static Color Text
        {
            get { return MaterialSkinManager.Instance.TextHighEmphasisNoAlphaColor; }
        }

        /// <summary>Theme aware secondary text color.</summary>
        public static Color TextMuted
        {
            get { return IsDark ? Hex(0x9CA0A8) : Hex(0x6B7280); }
        }

        /// <summary>Link color that is readable in both themes.</summary>
        public static Color Link
        {
            get { return IsDark ? DarkAccent : LightPrimary; }
        }

        /// <summary>Subtle line color for grids and separators.</summary>
        public static Color Grid
        {
            get { return IsDark ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, 0, 0, 0); }
        }

        // Tag "chips"
        public static Color ChipBack { get { return IsDark ? Hex(0x3A3C42) : Hex(0xE4E6EB); } }
        public static Color ChipFore { get { return IsDark ? Hex(0xE6E6E6) : Hex(0x1F2023); } }
        public static Color ChipActiveBack { get { return IsDark ? DarkAccent : LightPrimary; } }
        public static Color ChipActiveFore { get { return IsDark ? OnLightFill : Color.White; } }
        public static Color ChipInactiveBack { get { return IsDark ? Hex(0x2A2B2F) : Hex(0xEEF0F3); } }
        public static Color ChipInactiveFore { get { return IsDark ? Hex(0x7D8088) : Hex(0x9AA0A6); } }

        /// <summary>Hover variant of a fill color (lighter on dark, darker on light).</summary>
        public static Color Hover(Color c)
        {
            return IsDark ? Blend(c, Color.White, 0.12f) : Blend(c, Color.Black, 0.06f);
        }

        public static Color Blend(Color a, Color b, float amount)
        {
            return Color.FromArgb(
                a.A,
                (int)(a.R + (b.R - a.R) * amount),
                (int)(a.G + (b.G - a.G) * amount),
                (int)(a.B + (b.B - a.B) * amount));
        }

        public static void StyleChip(Label lbl)
        {
            lbl.BackColor = ChipBack;
            lbl.ForeColor = ChipFore;
        }

        public static void StyleChipActive(Label lbl)
        {
            lbl.BackColor = ChipActiveBack;
            lbl.ForeColor = ChipActiveFore;
        }

        public static void StyleChipInactive(Label lbl)
        {
            lbl.BackColor = ChipInactiveBack;
            lbl.ForeColor = ChipInactiveFore;
        }

        // Overlays (always dark, they sit on top of the game)
        public static readonly Color OverlayBack = Hex(0x1A1B1E);
        public static readonly Color OverlayFore = Hex(0xF5E6C8);
        public static readonly Color OverlayMoveBack = Hex(0x3A4A7A);
        public static readonly Color OverlayChipInactiveBack = Hex(0x2A2B2F);
        public static readonly Color OverlayChipInactiveFore = Hex(0x7D8088);

        // ---------------------------------------------------------------
        // Charts
        // ---------------------------------------------------------------

        /// <summary>
        /// Give a chart a clean, flat, modern look.
        /// </summary>
        public static void StyleChart(Chart chart, Color background, Font font, bool showLegend)
        {
            chart.BackColor = background;
            chart.BackGradientStyle = GradientStyle.None;
            chart.BorderlineDashStyle = ChartDashStyle.NotSet;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            Font labelFont = new Font(font.FontFamily, 8.25f, FontStyle.Regular);

            foreach (Legend l in chart.Legends)
            {
                l.Enabled = showLegend;
                l.BackColor = background;
                l.ForeColor = Text;
                l.Font = labelFont;
            }

            foreach (Title t in chart.Titles)
            {
                t.ForeColor = Text;
            }

            foreach (ChartArea ca in chart.ChartAreas)
            {
                ca.BackColor = background;
                ca.BackGradientStyle = GradientStyle.None;
                ca.BorderDashStyle = ChartDashStyle.NotSet;
                ca.ShadowColor = Color.Transparent;

                foreach (Axis axis in new[] { ca.AxisX, ca.AxisY })
                {
                    axis.LabelStyle.ForeColor = TextMuted;
                    axis.LabelStyle.Font = labelFont;
                    axis.TitleForeColor = TextMuted;
                    axis.MajorTickMark.Enabled = false;
                    axis.MinorTickMark.Enabled = false;
                    axis.MajorGrid.LineColor = Grid;
                    axis.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
                }

                // Only horizontal guide lines, faint baseline, no left axis line
                ca.AxisX.MajorGrid.Enabled = false;
                ca.AxisX.LineColor = Grid;
                ca.AxisY.MajorGrid.Enabled = true;
                ca.AxisY.MajorGrid.LineWidth = 1;
                ca.AxisY.LineColor = Color.Transparent;
            }
        }

        /// <summary>
        /// Style a single-color data series (bar/column/line charts).
        /// </summary>
        public static void StyleSeries(Series series, Font font)
        {
            series.Color = Accent;
            series.BorderWidth = IsLine(series.ChartType) ? 3 : 0;
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Text;
            series.Font = new Font(font.FontFamily, 8.25f, FontStyle.Regular);
            series.ShadowOffset = 0;

            if (series.ChartType == SeriesChartType.Column || series.ChartType == SeriesChartType.Bar)
            {
                series["PointWidth"] = "0.6";
                series["DrawingStyle"] = "Default";
            }
        }

        /// <summary>
        /// Turn a pie into a modern doughnut with clean slice separators.
        /// </summary>
        public static void StyleDoughnut(Series series, Color background, Font font)
        {
            series.ChartType = SeriesChartType.Doughnut;
            series["DoughnutRadius"] = "55";
            series["PieDrawingStyle"] = "Default";
            series.BorderColor = background;
            series.BorderWidth = 2;
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = OnLightFill;
            series.Font = new Font(font.FontFamily, 8.25f, FontStyle.Bold);
            series.ShadowOffset = 0;
        }

        private static bool IsLine(SeriesChartType t)
        {
            return t == SeriesChartType.Line || t == SeriesChartType.Spline
                || t == SeriesChartType.StepLine || t == SeriesChartType.FastLine;
        }

        // ---------------------------------------------------------------
        // Windows 11 chrome
        // ---------------------------------------------------------------

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWCP_ROUND = 2;
        private const int DWMWCP_ROUNDSMALL = 3;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Rounded corners, subtle border and a dark title bar (where applicable)
        /// on Windows 11. Silently does nothing on older Windows versions.
        /// </summary>
        public static void ApplyWindowChrome(Form form, bool smallCorners = false)
        {
            if (form == null) return;

            if (form.IsHandleCreated)
            {
                ApplyWindowChromeNow(form, smallCorners);
            }
            else
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    form.HandleCreated -= handler;
                    ApplyWindowChromeNow(form, smallCorners);
                };
                form.HandleCreated += handler;
            }
        }

        private static void ApplyWindowChromeNow(Form form, bool smallCorners)
        {
            // Windows 11 = build 22000+
            if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build < 22000)
                return;

            try
            {
                int corner = smallCorners ? DWMWCP_ROUNDSMALL : DWMWCP_ROUND;
                DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));

                int dark = IsDark ? 1 : 0;
                DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

                // COLORREF is 0x00BBGGRR
                Color b = IsDark ? Hex(0x3A3C42) : Hex(0xC9CDD4);
                int colorRef = b.R | (b.G << 8) | (b.B << 16);
                DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref colorRef, sizeof(int));
            }
            catch (Exception)
            {
                // Cosmetic only - never break the app because of window chrome
            }
        }

        private static Color Hex(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        }
    }
}
