using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>
    /// Validates the activity colors (docs/design.md, "Activity colors"). The thresholds follow the dataviz
    /// palette checks; deviations are documented there.
    /// </summary>
    public class ActivityPaletteTests
    {
        public static IEnumerable<object[]> Modes => new[] { new object[] { true }, new object[] { false } };

        private static Color[] Colors(bool dark) => ActivityPalette.Slots.Select(s => s.For(dark)).ToArray();

        private static Color Token(string key, bool dark) => DesignTokens.Get(key).For(dark);

        [Fact]
        public void Slots_are_in_index_order()
        {
            Assert.Equal(Enumerable.Range(0, ActivityPalette.Slots.Count), ActivityPalette.Slots.Select(s => s.Index));
            Assert.InRange(ActivityPalette.Slots.Count, 10, 12);
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Neighbouring_slots_stay_distinct_under_color_vision_deficiency(bool dark)
        {
            Color[] c = Colors(dark);
            for (int i = 0; i + 1 < c.Length; i++)
            {
                double d = ColorMetrics.CvdDeltaE(c[i], c[i + 1]);
                Assert.True(d >= 8, $"slots {i}/{i + 1}: CVD dE {d:N1} < 8 ({(dark ? "dark" : "light")})");
            }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Neighbouring_slots_are_clearly_distinct_for_normal_vision(bool dark)
        {
            Color[] c = Colors(dark);
            for (int i = 0; i + 1 < c.Length; i++)
            {
                double d = ColorMetrics.DeltaE(c[i], c[i + 1]);
                Assert.True(d >= 15, $"slots {i}/{i + 1}: dE {d:N1} < 15");
            }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Any_two_slots_are_distinct_for_normal_vision(bool dark)
        {
            // All pairs can't reach the adjacent floor with 10 colors; activity colors always come with the
            // type name (legend, list), which is the secondary encoding.
            Color[] c = Colors(dark);
            for (int i = 0; i < c.Length; i++)
                for (int j = i + 1; j < c.Length; j++)
                {
                    double d = ColorMetrics.DeltaE(c[i], c[j]);
                    Assert.True(d >= 9, $"slots {i}/{j}: dE {d:N1} < 9");
                }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void No_slot_looks_like_the_coral_accent(bool dark)
        {
            foreach (string accent in new[] { "Accent", "AccentText", "BtnPrimaryBg", "CoralPrimary" })
            {
                foreach (PaletteSlot s in ActivityPalette.Slots)
                {
                    double d = ColorMetrics.DeltaE(s.For(dark), Token(accent, dark));
                    Assert.True(d >= 9, $"{s.Name} vs {accent}: dE {d:N1} < 9");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Slots_have_mark_contrast_on_cards(bool dark)
        {
            foreach (PaletteSlot s in ActivityPalette.Slots)
            {
                double ratio = DesignTokens.Contrast(s.For(dark), Token("Surface", dark));
                Assert.True(ratio >= 3, $"{s.Name} on Surface: {ratio:N2}:1");
            }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Slots_have_color_and_sit_in_the_lightness_band(bool dark)
        {
            // Band follows the brightness of the website's data colors (wider than the generic default)
            (double lo, double hi) = dark ? (0.50, 0.82) : (0.42, 0.66);
            foreach (PaletteSlot s in ActivityPalette.Slots)
            {
                (double l, double c) = ColorMetrics.OkLch(s.For(dark));
                Assert.InRange(l, lo, hi);
                Assert.True(c >= 0.085, $"{s.Name}: chroma {c:N3} reads gray");
            }
        }

        [Theory]
        [MemberData(nameof(Modes))]
        public void Neutrals_read_gray_and_differ_from_each_other(bool dark)
        {
            foreach (PaletteSlot n in new[] { ActivityPalette.Other, ActivityPalette.Hideout })
            {
                Assert.True(ColorMetrics.OkLch(n.For(dark)).C < 0.03, $"{n.Name} is not neutral");
            }
            Assert.True(ColorMetrics.DeltaE(ActivityPalette.Other.For(dark), ActivityPalette.Hideout.For(dark)) >= 15);
        }

        [Fact]
        public void Main_activity_types_have_their_documented_slot()
        {
            Assert.Same(ActivityPalette.Green, ActivityPalette.SlotOf(ACTIVITY_TYPES.MAP));
            Assert.Same(ActivityPalette.Rust, ActivityPalette.SlotOf(ACTIVITY_TYPES.HEIST));
            Assert.Same(ActivityPalette.Ochre, ActivityPalette.SlotOf(ACTIVITY_TYPES.DELVE));
            Assert.Same(ActivityPalette.Teal, ActivityPalette.SlotOf(ACTIVITY_TYPES.LABYRINTH));
            Assert.Same(ActivityPalette.Violet, ActivityPalette.SlotOf(ACTIVITY_TYPES.SANCTUM));
            Assert.Same(ActivityPalette.Blue, ActivityPalette.SlotOf(ACTIVITY_TYPES.MAVEN_FIGHT));
            Assert.Same(ActivityPalette.Other, ActivityPalette.SlotOf(ACTIVITY_TYPES.OTHER));
        }

        [Fact]
        public void Every_boss_fight_is_blue()
        {
            var fights = Enum.GetValues(typeof(ACTIVITY_TYPES)).Cast<ACTIVITY_TYPES>().Where(t => t.ToString().EndsWith("_FIGHT"));
            Assert.All(fights, t => Assert.Same(ActivityPalette.Blue, ActivityPalette.SlotOf(t)));
        }

        [Fact]
        public void Colors_follow_the_theme()
        {
            bool before = ActivityPalette.IsDark;
            try
            {
                ActivityPalette.IsDark = true;
                Assert.Equal(ActivityPalette.Green.Dark, ActivityPalette.ColorOf(ACTIVITY_TYPES.MAP));
                ActivityPalette.IsDark = false;
                Assert.Equal(ActivityPalette.Green.Light, ActivityPalette.ColorOf(ACTIVITY_TYPES.MAP));
            }
            finally
            {
                ActivityPalette.IsDark = before;
            }
        }
    }
}
