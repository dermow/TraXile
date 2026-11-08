using System;
using System.IO;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Settings → Backups: create, list, delete and restore, compatible with the classic app.</summary>
    public class BackupTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-test-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;
        private readonly string _clientTxt;
        private readonly FakeCore _core = new FakeCore();
        private int _restarts;
        private bool _gameRunning;

        public BackupTests()
        {
            _setup = new AppSetup(Path.Combine(_dir, "appdata"));
            Directory.CreateDirectory(Path.Combine(_dir, "logs"));
            _clientTxt = Path.Combine(_dir, "logs", "Client.txt");
            File.WriteAllText(_clientTxt, "log v1");
            _setup.SaveClientTxtPath(_clientTxt);
            File.WriteAllText(_setup.DbPath, "db v1");
            File.WriteAllText(Path.Combine(_setup.AppDataPath, "stats.cache"), "cache v1");
            File.WriteAllText(Path.Combine(_setup.AppDataPath, "labdata.xml"), "<lab v1 />");
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        private SettingsViewModel Create(bool canWrite = true)
        {
            var vm = new SettingsViewModel(_setup, _core, canWrite, true, d => { }, () => _restarts++);
            vm.IsGameRunning = () => _gameRunning;
            vm.LoadBackups();
            return vm;
        }

        [Fact]
        public void Creates_a_named_backup_in_the_classic_layout()
        {
            SettingsViewModel vm = Create();
            vm.NewBackupName = "Before league";

            Assert.True(vm.CreateBackup(new DateTime(2026, 11, 8, 21, 5, 9), out BackupRow row, out string error), error);

            string dir = Path.Combine(_setup.AppDataPath, "backups", "Before league", "2026-11-08_210509");
            Assert.Equal(dir, row.Backup.Path);
            Assert.Equal(new[] { "Client.txt", "config.xml", "data.db", "labdata.xml", "stats.cache" }, row.Backup.Files);
            Assert.Equal("db v1", File.ReadAllText(Path.Combine(dir, "data.db")));
            Assert.Same(row, vm.SelectedBackup);
            Assert.Equal("", vm.NewBackupName);
        }

        [Fact]
        public void An_empty_name_becomes_Default()
        {
            SettingsViewModel vm = Create();

            Assert.True(vm.CreateBackup(DateTime.Now, out BackupRow row, out _));

            Assert.Equal("Default", row.Name);
        }

        [Theory]
        [InlineData("my.backup")]
        [InlineData("a/b")]
        [InlineData("..\\evil")]
        public void Names_with_a_path_are_rejected(string name)
        {
            SettingsViewModel vm = Create();
            vm.NewBackupName = name;

            Assert.NotNull(vm.BackupNameError);
            Assert.False(vm.CreateBackup(DateTime.Now, out _, out _));
            Assert.False(Directory.Exists(Path.Combine(_setup.AppDataPath, "backups")));
        }

        [Fact]
        public void Lists_backups_of_the_classic_app_newest_first()
        {
            // classic: backups\<name>\<yyyy-MM-dd_HHmmss>\{Client.txt, data.db, config.xml, ...}
            string old = Path.Combine(_setup.AppDataPath, "backups", "Default", "2025-01-02_030405");
            string recent = Path.Combine(_setup.AppDataPath, "backups", "Auto_Backup", "2026-10-01_120000");
            foreach (string d in new[] { old, recent })
            {
                Directory.CreateDirectory(d);
                File.WriteAllText(Path.Combine(d, "data.db"), "db");
            }

            SettingsViewModel vm = Create();

            Assert.Equal(new[] { "Auto_Backup", "Default" }, vm.BackupList.Select(r => r.Name));
            Assert.Equal(new DateTime(2025, 1, 2, 3, 4, 5), vm.BackupList[1].Backup.Created);
        }

        [Fact]
        public void Deletes_a_backup()
        {
            SettingsViewModel vm = Create();
            vm.CreateBackup(DateTime.Now, out BackupRow row, out _);

            Assert.True(vm.DeleteBackup(row, out string error), error);

            Assert.False(Directory.Exists(row.Backup.Path));
            Assert.Empty(vm.BackupList);
        }

        [Fact]
        public void Never_deletes_outside_the_backup_folder()
        {
            SettingsViewModel vm = Create();
            var outside = new BackupRow { Backup = new BackupInfo { Name = "x", Path = _setup.AppDataPath } };

            Assert.False(vm.DeleteBackup(outside, out _));
            Assert.True(File.Exists(_setup.DbPath));
        }

        [Fact]
        public void Restore_is_staged_restarts_and_is_applied_on_the_next_start()
        {
            SettingsViewModel vm = Create();
            vm.CreateBackup(new DateTime(2026, 1, 1), out BackupRow row, out _);

            // things change after the backup
            File.WriteAllText(_setup.DbPath, "db v2");
            File.WriteAllText(_clientTxt, "log v2");
            File.WriteAllText(Path.Combine(_setup.AppDataPath, "labdata.xml"), "<lab v2 />");

            Assert.True(vm.RestoreBackup(row, out string error), error);
            Assert.Equal(1, _restarts);
            Assert.Equal("db v2", File.ReadAllText(_setup.DbPath)); // nothing replaced while running
            Assert.True(BackupRestore.IsPrepared(_setup));

            RestoreResult result = BackupRestore.ApplyIfPrepared(_setup); // what the next start does

            Assert.False(result.HasFailed, result.Error);
            Assert.Equal("db v1", File.ReadAllText(_setup.DbPath));
            Assert.Equal("log v1", File.ReadAllText(_clientTxt));
            Assert.Equal("<lab v1 />", File.ReadAllText(Path.Combine(_setup.AppDataPath, "labdata.xml")));
        }

        [Fact]
        public void A_backup_of_the_classic_app_is_restored_with_its_staging_names()
        {
            string dir = Path.Combine(_setup.AppDataPath, "backups", "Default", "2025-05-05_101010");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "data.db"), "classic db");
            File.WriteAllText(Path.Combine(dir, "Client.txt"), "classic log");
            File.Copy(_setup.ConfigPath, Path.Combine(dir, "config.xml"));
            SettingsViewModel vm = Create();

            Assert.True(vm.RestoreBackup(vm.BackupList.Single(), out string error), error);

            // the classic DoBackupRestoreIfPrepared looks for exactly these files
            Assert.True(File.Exists(_setup.DbPath + ".restore"));
            Assert.True(File.Exists(_setup.ConfigPath + ".restore"));
            Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(_clientTxt), "_Client.txt.restore")));
        }

        [Fact]
        public void Restore_is_blocked_while_Path_of_Exile_runs()
        {
            SettingsViewModel vm = Create();
            vm.CreateBackup(DateTime.Now, out BackupRow row, out _);
            _gameRunning = true;

            Assert.False(vm.RestoreBackup(row, out string error));

            Assert.Contains("Path of Exile is running", error);
            Assert.False(BackupRestore.IsPrepared(_setup));
            Assert.Equal(0, _restarts);
        }

        [Fact]
        public void A_backup_without_database_cannot_be_restored()
        {
            string dir = Path.Combine(_setup.AppDataPath, "backups", "Broken", "2025-05-05_101010");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "config.xml"), "<settings />");
            SettingsViewModel vm = Create();

            Assert.False(vm.RestoreBackup(vm.BackupList.Single(), out string error));
            Assert.Contains("data.db", error);
        }

        [Fact]
        public void Read_only_cannot_create_delete_or_restore()
        {
            string dir = Path.Combine(_setup.AppDataPath, "backups", "Default", "2025-05-05_101010");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "data.db"), "db");
            SettingsViewModel vm = Create(canWrite: false);
            BackupRow row = vm.BackupList.Single(); // listing works

            Assert.False(vm.CreateBackup(DateTime.Now, out _, out _));
            Assert.False(vm.DeleteBackup(row, out _));
            Assert.False(vm.RestoreBackup(row, out _));
            Assert.True(Directory.Exists(dir));
        }
    }
}
