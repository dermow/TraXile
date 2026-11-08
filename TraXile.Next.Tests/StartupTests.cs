using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class StartupTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly string _clientTxt;

        public StartupTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            Directory.CreateDirectory(_setup.AppDataPath);
            string logs = Path.Combine(_dir, "poe", "logs");
            Directory.CreateDirectory(logs);
            _clientTxt = Path.Combine(logs, "Client.txt");
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private void WriteConfig(string path, string clientTxt) =>
            new XDocument(new XElement("root",
                new XElement("setting", new XAttribute("key", "poe_logfile_path"), new XAttribute("value", clientTxt))))
                .Save(path);

        /// <summary>The files the classic app stages in PrepareBackupRestore before it restarts.</summary>
        private void StageClassicRestore()
        {
            File.WriteAllText(_setup.DbPath + ".restore", "restored db");
            WriteConfig(_setup.ConfigPath + ".restore", _clientTxt);
            File.WriteAllText(Path.Combine(_setup.AppDataPath, "labdata.xml.restore"), "<labdata restored=\"1\" />");
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(_clientTxt), "_Client.txt.restore"), "restored log");
        }

        [Fact]
        public void Nothing_happens_without_a_prepared_restore()
        {
            File.WriteAllText(_setup.DbPath, "live db");

            RestoreResult r = BackupRestore.ApplyIfPrepared(_setup);

            Assert.False(r.HasRestored);
            Assert.False(r.HasFailed);
            Assert.Equal("live db", File.ReadAllText(_setup.DbPath));
        }

        [Fact]
        public void Restore_prepared_by_the_classic_app_is_applied()
        {
            File.WriteAllText(_setup.DbPath, "live db");
            WriteConfig(_setup.ConfigPath, _clientTxt);
            File.WriteAllText(_clientTxt, "live log");
            StageClassicRestore();
            Assert.True(BackupRestore.IsPrepared(_setup));

            RestoreResult r = BackupRestore.ApplyIfPrepared(_setup);

            Assert.False(r.HasFailed, r.Error);
            Assert.Equal(4, r.RestoredFiles.Count);
            Assert.Equal("restored db", File.ReadAllText(_setup.DbPath));
            Assert.Contains("restored", File.ReadAllText(Path.Combine(_setup.AppDataPath, "labdata.xml")));
            Assert.Equal("restored log", File.ReadAllText(_clientTxt));
            Assert.False(BackupRestore.IsPrepared(_setup));
            Assert.Empty(Directory.GetFiles(_dir, "*.restore", SearchOption.AllDirectories));
        }

        [Fact]
        public void Client_txt_is_restored_to_the_path_of_the_restored_config()
        {
            // live config points somewhere else; the backup's config decides where Client.txt goes
            WriteConfig(_setup.ConfigPath, Path.Combine(_dir, "old", "Client.txt"));
            StageClassicRestore();

            BackupRestore.ApplyIfPrepared(_setup);

            Assert.Equal(_clientTxt, _setup.ClientTxtPath);
            Assert.Equal("restored log", File.ReadAllText(_clientTxt));
        }

        [Fact]
        public void Restore_works_on_a_profile_without_live_files()
        {
            File.WriteAllText(_setup.DbPath + ".restore", "restored db");

            RestoreResult r = BackupRestore.ApplyIfPrepared(_setup);

            Assert.True(r.HasRestored);
            Assert.Equal("restored db", File.ReadAllText(_setup.DbPath));
        }

        [Fact]
        public void Locked_database_reports_an_error_instead_of_crashing()
        {
            File.WriteAllText(_setup.DbPath, "live db");
            File.WriteAllText(_setup.DbPath + ".restore", "restored db");

            using (new FileStream(_setup.DbPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                RestoreResult r = BackupRestore.ApplyIfPrepared(_setup);
                Assert.True(r.HasFailed);
            }
            Assert.True(BackupRestore.IsPrepared(_setup)); // retried on the next start
        }

        [Fact]
        public void Loading_state_follows_the_source()
        {
            var source = new DemoTrackingSource { SimulateLoading = true };
            using (var vm = new LoadingViewModel(source))
            {
                Assert.True(vm.IsLoading);
                Assert.Equal(0.42, vm.Progress, 2);
                Assert.Equal("42%", vm.PercentText.Replace(" ", ""));

                source.SimulateLoading = false;
                vm.Update();

                Assert.False(vm.IsLoading);
            }
        }
    }
}
