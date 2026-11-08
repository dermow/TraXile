using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Live source stand-in: demo data plus the core operations settings need.</summary>
    internal sealed class FakeCore : ITrackingSource, ICoreMaintenance
    {
        private readonly DemoTrackingSource _demo = new DemoTrackingSource();
        public Dictionary<ACTIVITY_TYPES, int> Caps { get; } = new Dictionary<ACTIVITY_TYPES, int>();
        public int Cleared { get; private set; }

        public void SetTimeCap(ACTIVITY_TYPES type, int seconds) => Caps[type] = seconds;
        public void StopAndClearHistory() => Cleared++;

        public bool IsDemo => false;
        public string Notice => null;
        public event EventHandler HistoryChanged { add { } remove { } }
        public void Start() { }
        public bool IsLoading => false;
        public double LoadProgress => 1;
        public IReadOnlyList<ActivityInfo> GetHistory() => _demo.GetHistory();
        public LiveState GetLiveState() => _demo.GetLiveState();
        public double GetHideoutSeconds(DateTime from, DateTime to) => 0;
        public int GetTimeCap(ACTIVITY_TYPES type) => Caps.TryGetValue(type, out int c) ? c : 3600;
        public void TogglePause() { }
        public void StopCurrentActivity() { }
        public IReadOnlyList<TagInfo> GetTags() => _demo.GetTags();
        public void AddTag(string activityId, string tagId) { }
        public void RemoveTag(string activityId, string tagId) { }
        public void DeleteActivities(IReadOnlyCollection<string> activityIds) { }
        public void Dispose() { }
    }

    public class SettingsTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly string _clientTxt;
        private readonly FakeCore _core = new FakeCore();
        private readonly List<bool> _appliedThemes = new List<bool>();
        private int _restarts;

        public SettingsTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            Directory.CreateDirectory(Path.Combine(_dir, "logs"));
            _clientTxt = Path.Combine(_dir, "logs", "Client.txt");
            File.WriteAllText(_clientTxt, "");
            _setup.SaveClientTxtPath(_clientTxt);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private SettingsViewModel Create(bool canWrite = true, bool isDark = true) =>
            new SettingsViewModel(_setup, _core, canWrite, isDark, d => _appliedThemes.Add(d), () => _restarts++);

        /// <summary>What the classic app would read: the raw value in config.xml.</summary>
        private string Stored(string key) => XDocument.Load(_setup.ConfigPath).Root.Elements("setting")
            .Where(e => (string)e.Attribute("key") == key).Select(e => (string)e.Attribute("value")).SingleOrDefault();

        [Fact]
        public void Theme_is_applied_and_stored_like_the_classic_app()
        {
            SettingsViewModel vm = Create();

            vm.IsLight = true;

            Assert.Equal(new[] { false }, _appliedThemes);
            Assert.Equal("Light", Stored("theme"));
            Assert.False(SettingsViewModel.ReadIsDark(_setup)); // survives a restart

            vm.IsDark = true;
            Assert.Equal("Dark", Stored("theme"));
        }

        [Fact]
        public void Theme_defaults_to_dark()
        {
            Assert.True(SettingsViewModel.ReadIsDark(_setup));
        }

        [Fact]
        public void Minimize_to_tray_is_stored_as_the_classic_bool_string()
        {
            SettingsViewModel vm = Create();

            vm.MinimizeToTray = true;

            Assert.Equal("True", Stored("MinimizeToTray"));
            Assert.True(Create().MinimizeToTray);
        }

        [Fact]
        public void Read_only_settings_are_not_written()
        {
            SettingsViewModel vm = Create(canWrite: false);

            vm.IsLight = true;
            vm.MinimizeToTray = true;

            Assert.Null(Stored("theme"));
            Assert.Null(Stored("MinimizeToTray"));
            Assert.False(vm.ChangeClientTxt(_clientTxt, out _));
            Assert.Equal(0, _restarts);
        }

        [Fact]
        public void Changing_Client_txt_saves_it_clears_the_history_and_restarts()
        {
            string other = Path.Combine(_dir, "other", "Client.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(other));
            File.WriteAllText(other, "");
            SettingsViewModel vm = Create();

            Assert.True(vm.ChangeClientTxt(other, out string error), error);

            Assert.Equal(other, Stored("poe_logfile_path"));
            Assert.Equal(1, _core.Cleared);
            Assert.Equal(1, _restarts);
        }

        [Fact]
        public void Missing_Client_txt_is_rejected()
        {
            SettingsViewModel vm = Create();

            Assert.False(vm.ChangeClientTxt(Path.Combine(_dir, "nope", "Client.txt"), out string error));

            Assert.NotNull(error);
            Assert.Equal(_clientTxt, Stored("poe_logfile_path"));
            Assert.Equal(0, _restarts);
        }
    }
}
