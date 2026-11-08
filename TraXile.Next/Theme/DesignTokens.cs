using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace TraXile.Next
{
    /// <summary>One design token: a CSS custom property of exile-tools.com and its value per theme.</summary>
    public sealed class DesignToken
    {
        public DesignToken(string key, string css, Color dark, Color light)
        {
            Key = key;
            Css = css;
            Dark = dark;
            Light = light;
        }

        /// <summary>WPF resource name without suffix: "Bg" -> resources "BgColor" and "BgBrush".</summary>
        public string Key { get; }

        /// <summary>Source CSS custom property, e.g. "--bg".</summary>
        public string Css { get; }

        public Color Dark { get; }
        public Color Light { get; }

        public Color For(bool dark) => dark ? Dark : Light;
    }

    /// <summary>
    /// The exile-tools.com design tokens (snapshot, see docs/design.md) as WPF resources.
    /// Every token becomes a "&lt;Key&gt;Color" and a frozen "&lt;Key&gt;Brush" resource; on top of that the
    /// brushes of the WPF-UI control library are pointed at the tokens, so its controls follow the design
    /// system without retemplating each of them. Swapped as a whole on theme change (see App.ApplyTheme).
    /// </summary>
    public static class DesignTokens
    {
        /// <summary>Date of the website snapshot the values below were taken from.</summary>
        public const string SnapshotDate = "2026-11-08";

        public static readonly IReadOnlyList<DesignToken> All = new[]
        {
            // ---- Surfaces and text ----
            T("Bg", "--bg", 0x131113, 0xF6F1EC),
            T("Surface", "--surface", 0x1C191B, 0xFFFCF9),
            T("SurfaceRaised", "--surface-raised", 0x252124, 0xECE6E0),
            T("SurfaceRow", "--surface-row", 0x1B181A, 0xFCF9F5),
            T("SurfaceRowHover", "--surface-row-hover", 0x242022, 0xFFFFFF),
            T("Sunken", "--footer-bg", 0x0F0D0E, 0xECE5DE),
            T("Text", "--text", 0xF2ECE6, 0x221B1C),
            T("TextMuted", "--text-muted", 0xADA5A1, 0x5E5557),
            T("Border", "--border", 0x3A3437, 0xDAD2CB),
            T("BorderStrong", "--border-strong", 0x514A4D, 0xADA39B),

            // ---- Accent (coral) ----
            T("Accent", "--accent", 0xD95C65, 0xC74450),
            T("AccentHover", "--accent-hover", 0xE7797D, 0x9D3846),
            T("AccentText", "--accent-text", 0xE7797D, 0x9D3846),
            T("AccentSurface", "--accent-surface", 0x3A1C23, 0xFBF0EF),
            T("OnAccent", "--on-accent", 0xFFFFFF, 0xFFFFFF),
            T("BtnPrimaryBg", "--btn-primary-bg", 0xC74450, 0x9D3846),
            T("BtnPrimaryHover", "--btn-primary-hover", 0xB53F4C, 0x7A2D3A),
            T("BtnPrimaryActive", "--btn-primary-active", 0x9D3846, 0x5E2430),
            T("Focus", "--focus", 0xE7797D, 0x9D3846),
            T("Selection", "::selection", 0x9D3846, 0x9D3846),

            // ---- Status ----
            T("StatusSuccess", "--status-success", 0x74AE6C, 0x3E7637),
            T("StatusWarning", "--status-warning", 0xD3A64A, 0x8A6416),
            T("StatusDanger", "--status-danger", 0xE7797D, 0x9D3846),

            // ---- Material and data colors ----
            T("Brass", "--brass", 0xB8935A, 0x8A6A35),
            T("BrassText", "--brass-text", 0xCDAE7C, 0x6E5228),
            T("DataRust", "--gem-str", 0xD07457, 0xA44A30),
            T("DataGreen", "--gem-dex", 0x74AE6C, 0x3E7637),
            T("DataBlue", "--gem-int", 0x6F93D6, 0x3B5EA6),
            T("DataOchre", "--status-warning", 0xD3A64A, 0x8A6416),
            T("DataNeutral", "--gem-none", 0xADA5A1, 0x5E5557),

            // ---- Depth ----
            T("Shadow", "--shadow-color", 0x000000, 0x463024),
            new DesignToken("Scrim", "--scrim", Color.FromArgb(0x8C, 0, 0, 0), Color.FromArgb(0x66, 0x19, 0x1A, 0x1C)),
            // Hover / press veils for controls that sit on any surface (text color at low opacity)
            new DesignToken("HoverVeil", "(derived)", Color.FromArgb(0x14, 0xF2, 0xEC, 0xE6), Color.FromArgb(0x0F, 0x22, 0x1B, 0x1C)),
            new DesignToken("PressVeil", "(derived)", Color.FromArgb(0x0A, 0xF2, 0xEC, 0xE6), Color.FromArgb(0x08, 0x22, 0x1B, 0x1C)),
            new DesignToken("TextDisabled", "(derived)", Color.FromArgb(0x80, 0xAD, 0xA5, 0xA1), Color.FromArgb(0x80, 0x5E, 0x55, 0x57)),
        };

        /// <summary>Theme-independent coral scale (primitive tokens).</summary>
        public static readonly IReadOnlyList<DesignToken> Coral = new[]
        {
            T("CoralHighlight", "--coral-highlight", 0xF2B0B1, 0xF2B0B1),
            T("CoralSoft", "--coral-soft", 0xE7797D, 0xE7797D),
            T("CoralPrimary", "--coral-primary", 0xD95C65, 0xD95C65),
            T("CoralMuted", "--coral-muted", 0xC74450, 0xC74450),
            T("CoralDeep", "--coral-deep", 0x9D3846, 0x9D3846),
        };

        /// <summary>
        /// WPF-UI brush key -> design token. Controls of the library (text box, combo box, menus, dialogs,
        /// navigation, scroll bars, ...) take their colors from these keys.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> WpfUiBrushes = new Dictionary<string, string>
        {
            // Window and text
            ["WindowBackground"] = "Bg",
            ["ApplicationBackgroundBrush"] = "Bg",
            ["SolidBackgroundFillColorBaseBrush"] = "Bg",
            ["SolidBackgroundFillColorSecondaryBrush"] = "Surface",
            ["SolidBackgroundFillColorTertiaryBrush"] = "SurfaceRaised",
            ["SolidBackgroundFillColorQuarternaryBrush"] = "SurfaceRaised",
            ["WindowForeground"] = "Text",
            ["TextFillColorPrimaryBrush"] = "Text",
            ["TextFillColorSecondaryBrush"] = "TextMuted",
            ["TextFillColorTertiaryBrush"] = "TextMuted",
            ["TextFillColorDisabledBrush"] = "TextDisabled",
            ["TextPlaceholderColorBrush"] = "TextMuted",
            ["LabelForeground"] = "Text",

            // Generic control fills and strokes
            ["ControlFillColorDefaultBrush"] = "Surface",
            ["ControlFillColorSecondaryBrush"] = "SurfaceRaised",
            ["ControlFillColorTertiaryBrush"] = "SurfaceRow",
            ["ControlFillColorInputActiveBrush"] = "Surface",
            ["ControlStrongStrokeColorDefaultBrush"] = "BorderStrong",
            ["ControlStrongFillColorDefaultBrush"] = "TextMuted",
            ["ControlElevationBorderBrush"] = "BorderStrong",
            ["CircleElevationBorderBrush"] = "BorderStrong",
            ["ControlStrokeColorDefaultBrush"] = "Border",
            ["ControlStrokeColorSecondaryBrush"] = "BorderStrong",
            ["SubtleFillColorSecondaryBrush"] = "HoverVeil",
            ["SubtleFillColorTertiaryBrush"] = "PressVeil",
            ["DividerStrokeColorDefaultBrush"] = "Border",
            ["SurfaceStrokeColorDefaultBrush"] = "Border",
            ["FocusStrokeColorOuterBrush"] = "Focus",
            ["KeyboardFocusBorderColorBrush"] = "Focus",

            // Cards: surfaces instead of hairlines
            ["CardBackground"] = "SurfaceRow",
            ["CardBackgroundPointerOver"] = "SurfaceRowHover",
            ["CardBackgroundFillColorDefaultBrush"] = "Surface",
            ["CardBackgroundFillColorSecondaryBrush"] = "SurfaceRow",
            ["CardStrokeColorDefaultBrush"] = "Surface",
            ["CardBorderBrush"] = "Surface",
            ["CardForeground"] = "Text",

            // Text box
            ["TextControlBackground"] = "Surface",
            ["TextControlBackgroundPointerOver"] = "Surface",
            ["TextControlBackgroundFocused"] = "Surface",
            ["TextControlForeground"] = "Text",
            ["TextControlPlaceholderForeground"] = "TextMuted",
            ["TextControlButtonForeground"] = "TextMuted",
            ["TextControlElevationBorderBrush"] = "BorderStrong",
            ["TextControlFocusedBorderBrush"] = "Focus",

            // Combo box
            ["ComboBoxBackground"] = "Surface",
            ["ComboBoxBackgroundPointerOver"] = "Surface",
            ["ComboBoxBackgroundFocused"] = "Surface",
            ["ComboBoxForeground"] = "Text",
            ["ComboBoxBorderBrushFocused"] = "Focus",
            ["ComboBoxDropDownBackground"] = "SurfaceRaised",
            ["ComboBoxDropDownBorderBrush"] = "Border",
            ["ComboBoxDropDownGlyphForeground"] = "TextMuted",
            ["ComboBoxItemForeground"] = "Text",
            ["ComboBoxItemForegroundSelected"] = "Text",
            ["ComboBoxItemBackgroundSelected"] = "HoverVeil",
            ["ComboBoxItemPillFillBrush"] = "Accent",

            // Menus, flyouts, tool tips
            ["ContextMenuBackground"] = "SurfaceRaised",
            ["ContextMenuForeground"] = "Text",
            ["ContextMenuBorderBrush"] = "Border",
            ["FlyoutBackground"] = "SurfaceRaised",
            ["FlyoutBorderBrush"] = "Border",
            ["ToolTipBackground"] = "SurfaceRaised",
            ["ToolTipForeground"] = "Text",
            ["ToolTipBorderBrush"] = "Border",
            ["ListViewItemBackgroundPointerOver"] = "HoverVeil",
            ["ListViewItemForeground"] = "Text",
            ["ListViewItemPillFillBrush"] = "Accent",
            ["ListBoxItemForeground"] = "Text",

            // Dialogs
            ["MessageBoxBackground"] = "SurfaceRow",
            ["MessageBoxTopOverlay"] = "Surface",
            ["MessageBoxForeground"] = "Text",
            ["MessageBoxSeparatorBorderBrush"] = "SurfaceRow",
            ["ContentDialogBackground"] = "SurfaceRow",
            ["ContentDialogTopOverlay"] = "Surface",
            ["ContentDialogForeground"] = "Text",
            ["ContentDialogBorderBrush"] = "Border",
            ["ContentDialogSeparatorBorderBrush"] = "SurfaceRow",
            ["ContentDialogSmokeFill"] = "Scrim",
            ["SmokeFillColorDefaultBrush"] = "Scrim",

            // Navigation
            ["NavigationViewContentBackground"] = "Bg",
            ["NavigationViewContentGridBorderBrush"] = "Bg",
            ["NavigationViewItemForeground"] = "TextMuted",
            ["NavigationViewItemForegroundLeftFluent"] = "TextMuted",
            ["NavigationViewItemForegroundPointerOver"] = "Text",
            ["NavigationViewItemForegroundPointerOverLeftFluent"] = "Text",
            ["NavigationViewItemForegroundPressed"] = "Text",
            ["NavigationViewItemBackgroundPointerOver"] = "HoverVeil",
            ["NavigationViewItemBackgroundPressed"] = "PressVeil",
            ["NavigationViewItemBackgroundSelected"] = "SurfaceRow",
            ["NavigationViewItemBackgroundSelectedLeftFluent"] = "SurfaceRow",
            ["NavigationViewSelectionIndicatorForeground"] = "Accent",
            ["LeftNavigationViewSeparatorBrush"] = "Border",

            // Scroll bars
            ["ScrollBarThumbFill"] = "BorderStrong",
            ["ScrollBarTrackFillPointerOver"] = "SurfaceRow",
            ["ScrollBarButtonArrowForeground"] = "TextMuted",

            // Toggles, check boxes, progress
            ["ToggleSwitchFillOn"] = "BtnPrimaryBg",
            ["ToggleSwitchFillOnPointerOver"] = "BtnPrimaryHover",
            ["ToggleSwitchFillOnPressed"] = "BtnPrimaryActive",
            ["ToggleSwitchKnobFillOn"] = "OnAccent",
            ["ToggleSwitchKnobFillOnPointerOver"] = "OnAccent",
            ["ToggleSwitchKnobFillOnPressed"] = "OnAccent",
            ["ToggleSwitchStrokeOff"] = "BorderStrong",
            ["ToggleSwitchStrokeOffPointerOver"] = "TextMuted",
            ["ToggleSwitchKnobFillOff"] = "TextMuted",
            ["ToggleSwitchKnobFillOffPointerOver"] = "Text",
            ["ToggleSwitchContentForeground"] = "Text",
            ["CheckBoxCheckBackgroundFillChecked"] = "BtnPrimaryBg",
            ["CheckBoxCheckBackgroundFillCheckedPointerOver"] = "BtnPrimaryHover",
            ["CheckBoxCheckBackgroundFillCheckedPressed"] = "BtnPrimaryActive",
            ["CheckBoxCheckGlyphForeground"] = "OnAccent",
            ["CheckBoxForeground"] = "Text",
            ["CheckBoxBorderBrush"] = "BorderStrong",
            ["ProgressBarForeground"] = "Accent",
            ["ProgressBarBackground"] = "SurfaceRaised",
            ["ProgressRingForegroundThemeBrush"] = "Accent",

            // Info bars: a quiet panel with a coloured mark
            ["InfoBarInformationalSeverityBackgroundBrush"] = "Surface",
            ["InfoBarInformationalSeverityBorderBrush"] = "Surface",
            ["InfoBarSuccessSeverityBackgroundBrush"] = "Surface",
            ["InfoBarSuccessSeverityBorderBrush"] = "Surface",
            ["InfoBarWarningSeverityBackgroundBrush"] = "Surface",
            ["InfoBarWarningSeverityBorderBrush"] = "Surface",
            ["InfoBarErrorSeverityBackgroundBrush"] = "Surface",
            ["InfoBarErrorSeverityBorderBrush"] = "Surface",
            ["InfoBarTitleForeground"] = "Text",
            ["InfoBarInformationalSeverityIconBackground"] = "Accent",
            ["InfoBarSuccessSeverityIconBackground"] = "StatusSuccess",
            ["InfoBarWarningSeverityIconBackground"] = "StatusWarning",
            ["InfoBarErrorSeverityIconBackground"] = "StatusDanger",
            ["SystemFillColorSuccessBrush"] = "StatusSuccess",
            ["SystemFillColorCautionBrush"] = "StatusWarning",
            ["SystemFillColorCriticalBrush"] = "StatusDanger",
        };

        public static DesignToken Get(string key) => All.Concat(Coral).First(t => t.Key == key);

        /// <summary>Builds the resource dictionary for one theme.</summary>
        public static ResourceDictionary CreateDictionary(bool dark)
        {
            var rd = new ResourceDictionary();
            var brushes = new Dictionary<string, SolidColorBrush>();

            foreach (DesignToken t in All.Concat(Coral))
            {
                Color c = t.For(dark);
                var brush = new SolidColorBrush(c);
                brush.Freeze();
                brushes[t.Key] = brush;
                rd[t.Key + "Color"] = c;
                rd[t.Key + "Brush"] = brush;
            }

            foreach (KeyValuePair<string, string> map in WpfUiBrushes)
            {
                rd[map.Key] = brushes[map.Value];
            }

            // Kept for older views until they are restyled
            rd["PositiveBrush"] = brushes["StatusSuccess"];
            rd["NegativeBrush"] = brushes["StatusDanger"];
            rd["BrandAccentBrush"] = brushes["Accent"];
            rd["BrandAccentColor"] = brushes["Accent"].Color;

            return rd;
        }

        /// <summary>WCAG 2 contrast ratio of two opaque colors (1..21).</summary>
        public static double Contrast(Color a, Color b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (System.Math.Max(la, lb) + 0.05) / (System.Math.Min(la, lb) + 0.05);
        }

        public static double Luminance(Color c)
        {
            double Channel(byte v)
            {
                double s = v / 255.0;
                return s <= 0.03928 ? s / 12.92 : System.Math.Pow((s + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
        }

        private static DesignToken T(string key, string css, int dark, int light) =>
            new DesignToken(key, css, Rgb(dark), Rgb(light));

        internal static Color Rgb(int rgb) =>
            Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
    }
}
