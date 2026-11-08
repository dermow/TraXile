using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using SkiaSharp;

namespace TraXile.Next
{
    /// <summary>One categorical color of the activity palette, with its dark and light variant.</summary>
    public sealed class PaletteSlot
    {
        public PaletteSlot(int index, string name, Color dark, Color light)
        {
            Index = index;
            Name = name;
            Dark = dark;
            Light = light;
        }

        /// <summary>Position in the fixed slot order (the order is part of the CVD validation).</summary>
        public int Index { get; }
        public string Name { get; }
        public Color Dark { get; }
        public Color Light { get; }

        public Color For(bool dark) => dark ? Dark : Light;
    }

    /// <summary>
    /// Colors per activity type, the same in every chart, list dot and chip (docs/design.md, "Activity colors").
    /// Derived from the exile-tools.com data colors (rust, green, blue, ochre) plus matching warm tones,
    /// validated for color vision deficiency and contrast in both themes (see ActivityPaletteTests).
    /// Coral is never an activity color: it is reserved for UI accents.
    /// </summary>
    public static class ActivityPalette
    {
        /// <summary>Theme the colors are returned for; set by App.ApplyTheme.</summary>
        public static bool IsDark { get; set; } = true;

        // Fixed slot order: neighbours in this order are guaranteed distinct (also under protanopia /
        // deuteranopia), so charts stack series in slot order. Never reorder without re-validating.
        public static readonly PaletteSlot Forest = new PaletteSlot(0, "Forest", Hex(0x1C7B5A), Hex(0x00603C));
        public static readonly PaletteSlot Mauve = new PaletteSlot(1, "Mauve", Hex(0xE197BC), Hex(0xB765A1));
        public static readonly PaletteSlot Olive = new PaletteSlot(2, "Olive", Hex(0x82862F), Hex(0x6A6700));
        public static readonly PaletteSlot Violet = new PaletteSlot(3, "Violet", Hex(0x916CB9), Hex(0x744399));
        public static readonly PaletteSlot Green = new PaletteSlot(4, "Green", Hex(0x79B76F), Hex(0x44873A));
        public static readonly PaletteSlot Sky = new PaletteSlot(5, "Sky", Hex(0x77C9F3), Hex(0x438DC3));
        public static readonly PaletteSlot Rust = new PaletteSlot(6, "Rust", Hex(0xCE8545), Hex(0xB55B11));
        public static readonly PaletteSlot Blue = new PaletteSlot(7, "Blue", Hex(0x6D92D6), Hex(0x3860AC));
        public static readonly PaletteSlot Ochre = new PaletteSlot(8, "Ochre", Hex(0xD7B355), Hex(0xAC8909));
        public static readonly PaletteSlot Teal = new PaletteSlot(9, "Teal", Hex(0x41B2B2), Hex(0x067971));

        public static readonly IReadOnlyList<PaletteSlot> Slots = new[]
        {
            Forest, Mauve, Olive, Violet, Green, Sky, Rust, Blue, Ochre, Teal
        };

        /// <summary>Neutral for the "Other" bucket and unknown types (reads gray on purpose).</summary>
        public static readonly PaletteSlot Other = new PaletteSlot(-1, "Other", Hex(0x6E6669), Hex(0x9C9295));

        /// <summary>Time spent in the hideout (not an activity, so a neutral as well).</summary>
        public static readonly PaletteSlot Hideout = new PaletteSlot(-2, "Hideout", Hex(0xADA5A1), Hex(0x5E5557));

        /// <summary>
        /// Activity type -> slot, grouped by kind of content so related types share a color:
        /// Map green, Heist rust, Delve ochre, Labyrinth teal, Sanctum violet, Simulacrum mauve,
        /// Expedition logbooks sky, boss fights blue, side areas olive, other league content forest.
        /// </summary>
        private static readonly Dictionary<ACTIVITY_TYPES, PaletteSlot> _map = new Dictionary<ACTIVITY_TYPES, PaletteSlot>
        {
            { ACTIVITY_TYPES.MAP, Green },
            { ACTIVITY_TYPES.HEIST, Rust },
            { ACTIVITY_TYPES.DELVE, Ochre },
            { ACTIVITY_TYPES.LABYRINTH, Teal },
            { ACTIVITY_TYPES.LAB_TRIAL, Teal },
            { ACTIVITY_TYPES.SANCTUM, Violet },
            { ACTIVITY_TYPES.SIMULACRUM, Mauve },
            { ACTIVITY_TYPES.LOGBOOK, Sky },
            { ACTIVITY_TYPES.LOGBOOK_SIDE, Sky },

            // Boss fights and invitations
            { ACTIVITY_TYPES.MAVEN_INVITATION, Blue },
            { ACTIVITY_TYPES.ATZIRI, Blue },
            { ACTIVITY_TYPES.UBER_ATZIRI, Blue },
            { ACTIVITY_TYPES.BREACHSTONE, Blue },

            // Side areas inside another activity
            { ACTIVITY_TYPES.VAAL_SIDEAREA, Olive },
            { ACTIVITY_TYPES.ABYSSAL_DEPTHS, Olive },

            // Other league content
            { ACTIVITY_TYPES.CAMPAIGN, Forest },
            { ACTIVITY_TYPES.TEMPLE, Forest },
            { ACTIVITY_TYPES.TIMELESS_LEGION, Forest },
            { ACTIVITY_TYPES.LAKE_OF_KALANDRA, Forest },
            { ACTIVITY_TYPES.TANES_LABORATORY, Forest },
            { ACTIVITY_TYPES.ANCESTOR_TRIAL, Forest },
            { ACTIVITY_TYPES.INSCRIBED_ULTIMATUM, Forest },
            { ACTIVITY_TYPES.KINGSMARCH, Forest },
            { ACTIVITY_TYPES.RELIQUARY_KEY, Forest },
            { ACTIVITY_TYPES.ANOMALY, Forest },
        };

        private static readonly Dictionary<Color, SolidColorBrush> _brushCache = new Dictionary<Color, SolidColorBrush>();

        /// <summary>Slot of an activity type: boss fights without explicit mapping are blue, the rest is "Other".</summary>
        public static PaletteSlot SlotOf(ACTIVITY_TYPES type)
        {
            if (_map.TryGetValue(type, out PaletteSlot s)) return s;
            return type.ToString().EndsWith("_FIGHT") ? Blue : Other;
        }

        public static Color ColorOf(ACTIVITY_TYPES type) => SlotOf(type).For(IsDark);

        public static Color ColorOf(PaletteSlot slot) => slot.For(IsDark);

        public static Brush Brush(ACTIVITY_TYPES type) => BrushOf(ColorOf(type));

        public static SolidColorBrush BrushOf(PaletteSlot slot) => BrushOf(slot.For(IsDark));

        public static SolidColorBrush BrushOf(Color c)
        {
            lock (_brushCache)
            {
                if (!_brushCache.TryGetValue(c, out SolidColorBrush b))
                {
                    b = new SolidColorBrush(c);
                    b.Freeze();
                    _brushCache[c] = b;
                }
                return b;
            }
        }

        public static SKColor Sk(Color c, byte alpha = 255) => new SKColor(c.R, c.G, c.B, alpha);

        public static SKColor Sk(ACTIVITY_TYPES type) => Sk(ColorOf(type));

        public static SKColor Sk(PaletteSlot slot) => Sk(slot.For(IsDark));

        /// <summary>Activity types that use a slot (for the documentation and tests).</summary>
        public static IEnumerable<ACTIVITY_TYPES> TypesOf(PaletteSlot slot) =>
            System.Enum.GetValues(typeof(ACTIVITY_TYPES)).Cast<ACTIVITY_TYPES>().Where(t => SlotOf(t) == slot);

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
