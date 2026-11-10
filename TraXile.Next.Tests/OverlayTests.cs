using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using TraXile.Next.Overlays;
using TraXile.Next.Services;
using Xunit;

namespace TraXile.Next.Tests
{
    public class OverlayTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;

        public OverlayTests()
        {
            _setup = new AppSetup(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private string Stored(string key) => XDocument.Load(_setup.ConfigPath).Root.Elements("setting")
            .Where(e => (string)e.Attribute("key") == key).Select(e => (string)e.Attribute("value")).SingleOrDefault();

        [Fact]
        public void Positions_use_the_classic_keys_in_screen_pixels()
        {
            OverlaySettings.WritePosition(_setup, OverlaySettings.Stopwatch, 1650, -12);

            Assert.Equal("1650", Stored("overlay.stopwatch.x"));
            Assert.Equal("-12", Stored("overlay.stopwatch.y"));
            Assert.Equal((1650, -12), OverlaySettings.ReadPosition(_setup, OverlaySettings.Stopwatch));
        }

        [Fact]
        public void Positions_saved_by_the_classic_app_are_read()
        {
            // classic BaseOverlay: AddUpdateAppSettings($"overlay.{_id}.x", Location.X.ToString())
            _setup.WriteSetting("overlay.tags.x", "300");
            _setup.WriteSetting("overlay.tags.y", "40");

            Assert.Equal((300, 40), OverlaySettings.ReadPosition(_setup, OverlaySettings.Tags));
            Assert.Null(OverlaySettings.ReadPosition(_setup, OverlaySettings.Stopwatch));
        }

        [Fact]
        public void Unreadable_positions_are_ignored()
        {
            _setup.WriteSetting("overlay.stopwatch.x", "abc");
            _setup.WriteSetting("overlay.stopwatch.y", "10");

            Assert.Null(OverlaySettings.ReadPosition(_setup, OverlaySettings.Stopwatch));
        }

        [Theory]
        [InlineData(null, 100)]
        [InlineData("75", 75)]
        [InlineData("0", OverlaySettings.MinOpacity)]
        [InlineData("250", 100)]
        [InlineData("x", 100)]
        public void Opacity_comes_from_the_general_setting(string stored, int expected)
        {
            if (stored != null) _setup.WriteSetting("overlay.general.opacity", stored);

            Assert.Equal(expected, OverlaySettings.ReadOpacity(_setup));
        }

        [Fact]
        public void Pixels_are_converted_to_device_independent_units()
        {
            // 150 % scaling: 1500 px are 1000 DIPs
            var fromDevice = new Matrix(1 / 1.5, 0, 0, 1 / 1.5, 0, 0);

            Point p = OverlayWindow.PixelsToDips(1500, 300, fromDevice);

            Assert.Equal(1000, p.X, 3);
            Assert.Equal(200, p.Y, 3);
        }

        [Fact]
        public void Positions_off_all_screens_are_rejected()
        {
            var desktop = new Rect(-1920, 0, 3840, 1080); // second monitor on the left

            Assert.True(OverlayWindow.IsOnScreen(new Point(-1800, 100), desktop));
            Assert.False(OverlayWindow.IsOnScreen(new Point(2500, 100), desktop)); // monitor removed
            Assert.False(OverlayWindow.IsOnScreen(new Point(100, 1075), desktop));
        }

        [Fact]
        public void Tag_slots_default_to_the_classic_values()
        {
            Assert.Equal(new[] { "blight", "expedition", null }, OverlaySettings.ReadTagSlots(_setup));

            _setup.WriteSetting("overlay.tags.tag3", "Juiced");
            _setup.WriteSetting("overlay.tags.tag1", "");
            _setup.WriteSetting("overlay.tags.tag2", "None"); // classic picker for an empty slot
            Assert.Equal(new[] { null, null, "Juiced" }, OverlaySettings.ReadTagSlots(_setup));
        }

        [Fact]
        public void Tag_slots_resolve_display_names_like_the_classic_app_then_ids()
        {
            var tags = new[]
            {
                new TagInfo { Id = "t16.5", Name = "T16.5" },
                new TagInfo { Id = "juiced", Name = "Very juicy" },
            };

            Assert.Equal("t16.5", OverlaySettings.ResolveTag("T16.5", tags).Id);
            Assert.Equal("juiced", OverlaySettings.ResolveTag("Very juicy", tags).Id);
            Assert.Equal("juiced", OverlaySettings.ResolveTag("juiced", tags).Id); // ID as fallback
            Assert.Null(OverlaySettings.ResolveTag("deleted-tag", tags));
            Assert.Null(OverlaySettings.ResolveTag(null, tags));
        }

        [Fact]
        public void Tag_slots_are_active_while_the_current_activity_has_the_tag()
        {
            var blight = new TagInfo { Id = "blight", Name = "Blight" };
            var juiced = new TagInfo { Id = "juiced", Name = "Juiced" };
            var live = new LiveState { IsTracking = true, ActiveTags = new[] { juiced } };

            TagSlotState[] slots = TagsOverlay.SlotsFor(live, new[] { blight, null, juiced });

            Assert.False(slots[0].IsActive);
            Assert.Equal("—", slots[1].Text);
            Assert.True(slots[2].IsActive);
            Assert.All(TagsOverlay.SlotsFor(new LiveState(), new[] { blight, null, juiced }), s => Assert.False(s.IsActive));
        }

        [Fact]
        public void Previous_activity_is_the_last_one_of_the_same_type()
        {
            var source = new DemoTrackingSource();
            LiveState live = source.GetLiveState();

            Assert.NotNull(live.PreviousActivity);
            Assert.Equal(live.Activity.Type, live.PreviousActivity.Type);
            Assert.Same(source.GetHistory().First(a => a.Type == live.Activity.Type).Id, live.PreviousActivity.Id);
        }

        [Fact]
        public void Stopwatch_shows_the_elapsed_time_or_zero_while_idle()
        {
            var live = new LiveState { IsTracking = true, Elapsed = TimeSpan.FromSeconds(3725) };

            Assert.Equal("1:02:05", StopwatchOverlay.TextFor(live));
            Assert.Equal("00:00", StopwatchOverlay.TextFor(new LiveState()));
        }
    }
}
