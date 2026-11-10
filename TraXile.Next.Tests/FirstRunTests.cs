using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class FirstRunTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly string _clientTxt;

        public FirstRunTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            string logs = Path.Combine(_dir, "Path of Exile", "logs");
            Directory.CreateDirectory(logs);
            _clientTxt = Path.Combine(logs, "Client.txt");
            File.WriteAllText(_clientTxt, "2026/11/08 10:00:00 123 abc [INFO Client 1] Connecting to instance server\n");
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        /// <summary>A config.xml as written by the classic 2.x app.</summary>
        private void WriteClassicConfig(params (string key, string value)[] settings)
        {
            Directory.CreateDirectory(_setup.AppDataPath);
            new XDocument(new XElement("root",
                settings.Select(s => new XElement("setting", new XAttribute("key", s.key), new XAttribute("value", s.value)))))
                .Save(_setup.ConfigPath);
        }

        [Fact]
        public void Fresh_profile_needs_first_run()
        {
            Assert.False(File.Exists(_setup.ConfigPath));
            Assert.Null(_setup.ClientTxtPath);
            Assert.False(_setup.HasValidClientTxt);
        }

        [Fact]
        public void Existing_classic_config_is_used_unchanged()
        {
            WriteClassicConfig(("poe_logfile_path", _clientTxt), ("theme", "Dark"));
            DateTime before = File.GetLastWriteTimeUtc(_setup.ConfigPath);

            Assert.True(_setup.HasValidClientTxt);
            Assert.Equal(_clientTxt, _setup.ClientTxtPath);
            Assert.Equal(before, File.GetLastWriteTimeUtc(_setup.ConfigPath));
        }

        [Fact]
        public void Configured_but_missing_Client_txt_needs_first_run()
        {
            WriteClassicConfig(("poe_logfile_path", Path.Combine(_dir, "gone", "Client.txt")));
            Assert.False(_setup.HasValidClientTxt);
        }

        [Fact]
        public void Saving_writes_the_classic_key_and_keeps_other_settings()
        {
            WriteClassicConfig(("theme", "Light"), ("TimeCapMap", "1800"));

            _setup.SaveClientTxtPath(_clientTxt);

            XDocument doc = XDocument.Load(_setup.ConfigPath);
            string Value(string key) => doc.Root.Elements("setting").Single(e => (string)e.Attribute("key") == key).Attribute("value").Value;
            Assert.Equal(_clientTxt, Value("poe_logfile_path"));
            Assert.Equal("Light", Value("theme"));
            Assert.Equal("1800", Value("TimeCapMap"));
            Assert.True(new AppSetup(_setup.AppDataPath).HasValidClientTxt);
        }

        [Fact]
        public void Saving_creates_the_data_folder_on_a_fresh_profile()
        {
            _setup.SaveClientTxtPath(_clientTxt);
            Assert.True(File.Exists(_setup.ConfigPath));
        }

        [Fact]
        public void First_detected_installation_is_preselected()
        {
            var vm = new FirstRunViewModel(_setup, new[] { _clientTxt, @"D:\other\Client.txt" });

            Assert.Same(vm.Candidates[0], vm.Selected);
            Assert.Equal("Standalone", vm.Candidates[0].Source);
            Assert.True(vm.CanStart);
        }

        [Fact]
        public void Without_detection_nothing_can_start_until_a_file_is_picked()
        {
            var vm = new FirstRunViewModel(_setup, Array.Empty<string>());
            Assert.False(vm.HasCandidates);
            Assert.False(vm.CanStart);

            Assert.True(vm.AddManual(_clientTxt));

            Assert.True(vm.CanStart);
            Assert.Equal("Selected manually", vm.Selected.Source);
        }

        [Fact]
        public void Manual_pick_rejects_other_files()
        {
            string other = Path.Combine(_dir, "notes.txt");
            File.WriteAllText(other, "x");
            var vm = new FirstRunViewModel(_setup, Array.Empty<string>());

            Assert.False(vm.AddManual(other));
            Assert.True(vm.HasError);
            Assert.False(vm.AddManual(Path.Combine(_dir, "missing", "Client.txt")));
            Assert.False(vm.CanStart);
        }

        [Fact]
        public void Manual_pick_of_a_detected_file_selects_it_instead_of_adding_it_twice()
        {
            var vm = new FirstRunViewModel(_setup, new[] { @"D:\other\Client.txt", _clientTxt });

            vm.AddManual(_clientTxt);

            Assert.Equal(2, vm.Candidates.Count);
            Assert.Equal(_clientTxt, vm.Selected.Path);
        }

        [Fact]
        public void Confirm_saves_the_selection()
        {
            var vm = new FirstRunViewModel(_setup, new[] { _clientTxt });

            Assert.True(vm.Confirm());

            Assert.Equal(_clientTxt, _setup.ClientTxtPath);
        }
    }
}
