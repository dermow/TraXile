using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Settings → Overlays.</summary>
    public class OverlaySettingsTests : IDisposable
    {
        private sealed class FakeOverlays : IOverlayHost
        {
            public int Reloaded { get; private set; }
            public int Resets { get; private set; }
            public void ReloadSettings() => Reloaded++;
            public void ResetPositions() => Resets++;
        }

        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly FakeCore _core = new FakeCore();
        private readonly FakeOverlays _overlays = new FakeOverlays();

        public OverlaySettingsTests()
        {
            _setup = new AppSetup(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private SettingsViewModel Create(bool canWrite = true) =>
            new SettingsViewModel(_setup, _core, canWrite, true, d => { }, () => { }, _overlays);

        private XElement[] Settings() => File.Exists(_setup.ConfigPath)
            ? XDocument.Load(_setup.ConfigPath).Root.Elements("setting").ToArray()
            : Array.Empty<XElement>();

        private string Stored(string key) => Settings()
            .Where(e => (string)e.Attribute("key") == key).Select(e => (string)e.Attribute("value")).SingleOrDefault();

        [Fact]
        public void Defaults_match_the_classic_app()
        {
            SettingsViewModel vm = Create();

            Assert.Equal(100, vm.OverlayOpacity);
            Assert.False(vm.StopwatchShowDefault);
            Assert.False(vm.TagsShowDefault);
            // classic defaults "blight" / "expedition" are IDs; the picker shows the tag names
            Assert.Equal("Blight", vm.TagSlot1);
            Assert.Equal("Expedition", vm.TagSlot2);
            Assert.Equal("None", vm.TagSlot3);
            Assert.Equal("None", vm.TagSlotOptions.First());
            Assert.Contains("Juiced", vm.TagSlotOptions);
        }

        [Fact]
        public void Opacity_is_stored_and_applied_to_the_running_overlays()
        {
            SettingsViewModel vm = Create();

            vm.OverlayOpacity = 80;

            Assert.Equal("80", Stored("overlay.general.opacity"));
            Assert.Equal(1, _overlays.Reloaded);

            vm.OverlayOpacity = 0; // never invisible
            Assert.Equal(OverlaySettings.MinOpacity, vm.OverlayOpacity);
        }

        [Fact]
        public void Show_by_default_uses_the_keys_that_are_read()
        {
            SettingsViewModel vm = Create();

            vm.StopwatchShowDefault = true;
            vm.TagsShowDefault = true;

            Assert.Equal("True", Stored("overlay.stopwatch.showDefault"));
            Assert.Equal("True", Stored("overlay.tags.showDefault"));
            Assert.True(OverlaySettings.ReadShowDefault(_setup, OverlaySettings.Stopwatch));
            Assert.Null(Stored("overlay.stopwatch.default")); // classic bug key
        }

        [Fact]
        public void Tag_slots_are_stored_as_display_names_under_overlay_tags()
        {
            SettingsViewModel vm = Create();

            vm.TagSlot1 = "Juiced";
            vm.TagSlot3 = "Harvest";
            vm.TagSlot2 = "None";

            Assert.Equal("Juiced", Stored("overlay.tags.tag1"));
            Assert.Equal("None", Stored("overlay.tags.tag2"));
            Assert.Equal("Harvest", Stored("overlay.tags.tag3"));
            Assert.Equal(new[] { "Juiced", null, "Harvest" }, OverlaySettings.ReadTagSlots(_setup));
            Assert.Equal(3, _overlays.Reloaded);

            // classic bug wrote overlay.stopwatch.tag1..3
            Assert.DoesNotContain(Settings(), e => ((string)e.Attribute("key")).StartsWith("overlay.stopwatch.tag"));
        }

        [Fact]
        public void A_stored_tag_that_does_not_exist_stays_selected()
        {
            _setup.WriteSetting("overlay.tags.tag1", "mirage");

            SettingsViewModel vm = Create();

            Assert.Equal("mirage", vm.TagSlot1);
            Assert.Contains("mirage", vm.TagSlotOptions);
        }

        [Fact]
        public void The_combo_box_resetting_its_selection_writes_nothing()
        {
            SettingsViewModel vm = Create();

            vm.TagSlot1 = null;

            Assert.Equal("Blight", vm.TagSlot1);
            Assert.Null(Stored("overlay.tags.tag1"));
        }

        [Fact]
        public void Reset_positions_moves_the_overlays()
        {
            Create().ResetOverlayPositionsCommand.Execute(null);

            Assert.Equal(1, _overlays.Resets);
        }

        [Fact]
        public void Read_only_writes_nothing()
        {
            SettingsViewModel vm = Create(canWrite: false);

            vm.OverlayOpacity = 50;
            vm.StopwatchShowDefault = true;
            vm.TagSlot1 = "Juiced";

            Assert.DoesNotContain(Settings(), e => ((string)e.Attribute("key")).StartsWith("overlay."));
            Assert.False(vm.ResetOverlayPositionsCommand.CanExecute(null));
        }
    }
}
