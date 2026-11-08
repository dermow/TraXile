using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class TimeCapsTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly FakeCore _core = new FakeCore();
        private int _restarts;

        public TimeCapsTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            _setup.WriteSetting("TimeCapMap", "900");
            _setup.WriteSetting("TimeCapMinimum", "15");
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private SettingsViewModel Create(bool canWrite = true) =>
            new SettingsViewModel(_setup, _core, canWrite, true, d => { }, () => _restarts++);

        private string Stored(string key) => XDocument.Load(_setup.ConfigPath).Root.Elements("setting")
            .Where(e => (string)e.Attribute("key") == key).Select(e => (string)e.Attribute("value")).SingleOrDefault();

        private static TimeCapRow Row(SettingsViewModel vm, ACTIVITY_TYPES type) => vm.TimeCaps.Single(r => r.Type == type);

        [Fact]
        public void Keys_match_the_classic_app()
        {
            Assert.Equal("TimeCapMap", SettingsViewModel.TimeCapKey(ACTIVITY_TYPES.MAP));
            Assert.Equal("TimeCapMaven_fight", SettingsViewModel.TimeCapKey(ACTIVITY_TYPES.MAVEN_FIGHT));
        }

        [Fact]
        public void Stored_caps_and_defaults_are_shown()
        {
            SettingsViewModel vm = Create();

            Assert.Equal("15", vm.MinimumText);
            Assert.Equal(900, Row(vm, ACTIVITY_TYPES.MAP).Seconds);
            Assert.Equal(3600, Row(vm, ACTIVITY_TYPES.HEIST).Seconds);
            Assert.Equal(Enum.GetValues(typeof(ACTIVITY_TYPES)).Length, vm.TimeCaps.Count);
            Assert.Equal("15:00", Row(vm, ACTIVITY_TYPES.MAP).DurationText);
        }

        [Fact]
        public void Changed_caps_are_stored_and_reach_the_running_core_without_restart()
        {
            SettingsViewModel vm = Create();
            Row(vm, ACTIVITY_TYPES.HEIST).Text = "1200";

            Assert.True(vm.SaveTimeCaps(out string error), error);

            Assert.Equal("1200", Stored("TimeCapHeist"));
            Assert.Equal(1200, _core.Caps[ACTIVITY_TYPES.HEIST]);
            Assert.Single(_core.Caps); // only what changed
            Assert.False(vm.NeedsRestart);
            Assert.Null(Stored("TimeCapDelve")); // untouched caps are not written
        }

        [Fact]
        public void Changed_minimum_asks_for_a_restart()
        {
            SettingsViewModel vm = Create();
            vm.MinimumText = "30";

            Assert.True(vm.SaveTimeCaps(out _));

            Assert.Equal("30", Stored("TimeCapMinimum"));
            Assert.True(vm.NeedsRestart);
            Assert.Equal(0, _restarts); // the user decides when

            vm.RestartNow();
            Assert.Equal(1, _restarts);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("90000")]
        public void Invalid_caps_are_rejected_and_nothing_is_saved(string text)
        {
            SettingsViewModel vm = Create();
            Row(vm, ACTIVITY_TYPES.HEIST).Text = "1200";
            Row(vm, ACTIVITY_TYPES.MAP).Text = text;

            Assert.False(vm.SaveTimeCaps(out string error));

            Assert.Contains("Map", error);
            Assert.Null(Stored("TimeCapHeist"));
            Assert.Empty(_core.Caps);
        }

        [Fact]
        public void Read_only_caps_are_not_saved()
        {
            SettingsViewModel vm = Create(canWrite: false);
            Row(vm, ACTIVITY_TYPES.MAP).Text = "60";

            Assert.False(vm.SaveTimeCaps(out _));
            Assert.Equal("900", Stored("TimeCapMap"));
        }
    }
}
