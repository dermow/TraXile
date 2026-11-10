using System;
using System.IO;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class MaintenanceTests : IDisposable
    {
        private static readonly DateTime Now = new DateTime(2026, 11, 8, 9, 5, 7);

        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly string _clientTxt;
        private readonly FakeCore _core = new FakeCore();
        private int _restarts;

        public MaintenanceTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            Directory.CreateDirectory(Path.Combine(_dir, "logs"));
            _clientTxt = Path.Combine(_dir, "logs", "Client.txt");
            File.WriteAllText(_clientTxt, "a lot of log lines");
            _setup.SaveClientTxtPath(_clientTxt);
            File.WriteAllText(_setup.DbPath, "db");
            File.WriteAllText(Path.Combine(_setup.AppDataPath, "labdata.xml"), "<lab />");
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private SettingsViewModel Create(bool canWrite = true, ITrackingSource source = null) =>
            new SettingsViewModel(_setup, source ?? _core, canWrite, true, d => { }, () => _restarts++);

        [Fact]
        public void Backup_uses_the_classic_folder_layout()
        {
            string dir = Maintenance.CreateBackup(_setup, "Auto_Backup", Now);

            Assert.Equal(Path.Combine(_setup.AppDataPath, "backups", "Auto_Backup", "2026-11-08_090507"), dir);
            foreach (string f in new[] { "Client.txt", "data.db", "config.xml", "labdata.xml" })
            {
                Assert.True(File.Exists(Path.Combine(dir, f)), f);
            }
        }

        [Fact]
        public void Reload_clears_the_history_and_restarts()
        {
            Assert.True(Create().ReloadLogfile(out _));

            Assert.Equal(1, _core.Cleared);
            Assert.Equal(1, _restarts);
            Assert.Equal("a lot of log lines", File.ReadAllText(_clientTxt)); // the log itself stays
        }

        [Fact]
        public void Shrink_keeps_a_dated_copy_and_empties_the_log_but_not_the_history()
        {
            Assert.True(Create().ShrinkLogfile(Now, out string archive, out string error), error);

            Assert.Equal(Path.Combine(_dir, "logs", "Client.2026-11-08-9-5-7.txt"), archive);
            Assert.Equal("a lot of log lines", File.ReadAllText(archive));
            Assert.Equal(0, new FileInfo(_clientTxt).Length);
            Assert.Equal(0, _core.Cleared);
            Assert.Equal(1, _restarts);
        }

        [Fact]
        public void Full_reset_with_backup_saves_everything_first()
        {
            Assert.True(Create().FullReset(backupFirst: true, Now, out string backup, out string error), error);

            Assert.Equal("a lot of log lines", File.ReadAllText(Path.Combine(backup, "Client.txt")));
            Assert.Equal(0, new FileInfo(_clientTxt).Length);
            Assert.Equal(1, _core.Cleared);
            Assert.Equal(1, _restarts);
        }

        [Fact]
        public void Full_reset_without_backup_creates_no_backup()
        {
            Assert.True(Create().FullReset(backupFirst: false, Now, out string backup, out _));

            Assert.Null(backup);
            Assert.False(Directory.Exists(Path.Combine(_setup.AppDataPath, "backups")));
            Assert.Equal(0, new FileInfo(_clientTxt).Length);
        }

        [Fact]
        public void Maintenance_is_not_available_with_demo_data_or_read_only()
        {
            Assert.False(Create(source: new DemoTrackingSource()).CanMaintain);
            SettingsViewModel readOnly = Create(canWrite: false);
            Assert.False(readOnly.CanMaintain);

            Assert.False(readOnly.ReloadLogfile(out _));
            Assert.False(readOnly.FullReset(true, Now, out _, out _));
            Assert.Equal(0, _restarts);
            Assert.Equal("a lot of log lines", File.ReadAllText(_clientTxt));
        }
    }
}
