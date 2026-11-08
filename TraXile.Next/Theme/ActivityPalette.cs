using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using SkiaSharp;

namespace TraXile.Next
{
    /// <summary>
    /// Consistent categorical colors per activity type, used by charts, list dots and chips.
    /// Soft "400" tones that read well on both dark and light Fluent backgrounds.
    /// </summary>
    public static class ActivityPalette
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

        /// <summary>Brand accent ("exalted gold").</summary>
        public static readonly Color Accent = Hex(0xF5B942);

        public static readonly Color Hideout = Blue;

        /// <summary>Neutral for the "Other" bucket, distinct from Slate (Simulacrum/Elder).</summary>
        public static readonly Color Other = Hex(0x6B7280);

        private static readonly Dictionary<ACTIVITY_TYPES, Color> _map = new Dictionary<ACTIVITY_TYPES, Color>
        {
            { ACTIVITY_TYPES.MAP, Emerald },
            { ACTIVITY_TYPES.HEIST, Rose },
            { ACTIVITY_TYPES.DELVE, Amber },
            { ACTIVITY_TYPES.LABYRINTH, Cyan },
            { ACTIVITY_TYPES.LAB_TRIAL, Cyan },
            { ACTIVITY_TYPES.SIMULACRUM, Slate },
            { ACTIVITY_TYPES.TEMPLE, Teal },
            { ACTIVITY_TYPES.MAVEN_INVITATION, Violet },
            { ACTIVITY_TYPES.ATZIRI, Orange },
            { ACTIVITY_TYPES.UBER_ATZIRI, Orange },
            { ACTIVITY_TYPES.ELDER_FIGHT, Slate },
            { ACTIVITY_TYPES.SHAPER_FIGHT, Fuchsia },
            { ACTIVITY_TYPES.MAVEN_FIGHT, Indigo },
            { ACTIVITY_TYPES.SIRUS_FIGHT, Stone },
            { ACTIVITY_TYPES.OTHER, Slate },
            { ACTIVITY_TYPES.CAMPAIGN, Teal },
            { ACTIVITY_TYPES.VAAL_SIDEAREA, Red },
            { ACTIVITY_TYPES.ABYSSAL_DEPTHS, Lime },
            { ACTIVITY_TYPES.LOGBOOK, Sky },
            { ACTIVITY_TYPES.LOGBOOK_SIDE, Sky },
            { ACTIVITY_TYPES.CATARINA_FIGHT, Amber },
            { ACTIVITY_TYPES.BREACHSTONE, Pink },
            { ACTIVITY_TYPES.BLACK_STAR_FIGHT, Rose },
            { ACTIVITY_TYPES.SEARING_EXARCH_FIGHT, Rose },
            { ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT, Indigo },
            { ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT, Indigo },
            { ACTIVITY_TYPES.TIMELESS_LEGION, Violet },
            { ACTIVITY_TYPES.LAKE_OF_KALANDRA, Stone },
            { ACTIVITY_TYPES.SANCTUM, Purple },
            { ACTIVITY_TYPES.TRIALMASTER_FIGHT, Rose },
            { ACTIVITY_TYPES.TANES_LABORATORY, Lime },
            { ACTIVITY_TYPES.ANCESTOR_TRIAL, Teal },
            { ACTIVITY_TYPES.INSCRIBED_ULTIMATUM, Fuchsia },
            { ACTIVITY_TYPES.KINGSMARCH, Bronze },
            { ACTIVITY_TYPES.KING_IN_THE_MISTS_FIGHT, Purple },
            { ACTIVITY_TYPES.BLACK_KNIGHT_FIGHT, Red },
            { ACTIVITY_TYPES.ADMIRAL_VALERIUS_FIGHT, Cyan },
            { ACTIVITY_TYPES.SASAN_FIGHT, Cyan },
            { ACTIVITY_TYPES.OSHABI_FIGHT, Sky },
            { ACTIVITY_TYPES.RELIQUARY_KEY, Amber },
            { ACTIVITY_TYPES.ANOMALY, Orange },
        };

        private static readonly Dictionary<Color, SolidColorBrush> _brushCache = new Dictionary<Color, SolidColorBrush>();

        public static Color ColorOf(ACTIVITY_TYPES type)
        {
            // Incarnations / other boss fights without explicit mapping -> indigo, everything else -> slate
            if (_map.TryGetValue(type, out Color c)) return c;
            return type.ToString().EndsWith("_FIGHT") ? Indigo : Slate;
        }

        public static Brush Brush(ACTIVITY_TYPES type) => BrushOf(ColorOf(type));

        public static SolidColorBrush BrushOf(Color c)
        {
            if (!_brushCache.TryGetValue(c, out SolidColorBrush b))
            {
                b = new SolidColorBrush(c);
                b.Freeze();
                _brushCache[c] = b;
            }
            return b;
        }

        public static SKColor Sk(Color c, byte alpha = 255) => new SKColor(c.R, c.G, c.B, alpha);

        public static SKColor Sk(ACTIVITY_TYPES type) => Sk(ColorOf(type));

        /// <summary>MAVEN_INVITATION -> "Maven Invitation"</summary>
        public static string DisplayName(ACTIVITY_TYPES type)
        {
            string s = type.ToString().Replace('_', ' ').ToLowerInvariant();
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
        }

        private static Color Hex(int rgb) =>
            Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
    }
}
