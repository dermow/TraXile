using System.Linq;
using System.Windows.Media;
using Xunit;

namespace TraXile.Next.Tests
{
    public class DesignTokensTests
    {
        [Fact]
        public void Token_keys_are_unique()
        {
            var keys = DesignTokens.All.Concat(DesignTokens.Coral).Select(t => t.Key).ToList();
            Assert.Equal(keys.Count, keys.Distinct().Count());
        }

        [Fact]
        public void Every_WpfUi_brush_points_to_an_existing_token()
        {
            var keys = DesignTokens.All.Select(t => t.Key).ToHashSet();
            Assert.All(DesignTokens.WpfUiBrushes, m => Assert.Contains(m.Value, keys));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Dictionary_contains_color_and_brush_for_every_token(bool dark)
        {
            var rd = DesignTokens.CreateDictionary(dark);

            foreach (DesignToken t in DesignTokens.All)
            {
                Assert.Equal(t.For(dark), (Color)rd[t.Key + "Color"]);
                var brush = Assert.IsType<SolidColorBrush>(rd[t.Key + "Brush"]);
                Assert.True(brush.IsFrozen);
                Assert.Equal(t.For(dark), brush.Color);
            }
        }

        // The website documents these contrast ratios; the app must not fall below WCAG AA (4.5:1) for text.
        [Theory]
        [InlineData("Text", "Bg", true)]
        [InlineData("Text", "Bg", false)]
        [InlineData("TextMuted", "Bg", true)]
        [InlineData("TextMuted", "Bg", false)]
        [InlineData("TextMuted", "Surface", true)]
        [InlineData("TextMuted", "Surface", false)]
        [InlineData("AccentText", "Bg", true)]
        [InlineData("AccentText", "Bg", false)]
        [InlineData("OnAccent", "BtnPrimaryBg", true)]
        [InlineData("OnAccent", "BtnPrimaryBg", false)]
        [InlineData("Text", "AccentSurface", true)]
        [InlineData("Text", "AccentSurface", false)]
        public void Text_tokens_have_enough_contrast(string fore, string back, bool dark)
        {
            double ratio = DesignTokens.Contrast(DesignTokens.Get(fore).For(dark), DesignTokens.Get(back).For(dark));
            Assert.True(ratio >= 4.5, $"{fore} on {back} ({(dark ? "dark" : "light")}): {ratio:N2}:1");
        }

        [Fact]
        public void Snapshot_values_match_the_website()
        {
            // spot checks against foundation.css (2026-11-08)
            Assert.Equal(Color.FromRgb(0x13, 0x11, 0x13), DesignTokens.Get("Bg").Dark);
            Assert.Equal(Color.FromRgb(0xF6, 0xF1, 0xEC), DesignTokens.Get("Bg").Light);
            Assert.Equal(Color.FromRgb(0xD9, 0x5C, 0x65), DesignTokens.Get("CoralPrimary").Dark);
        }
    }
}
