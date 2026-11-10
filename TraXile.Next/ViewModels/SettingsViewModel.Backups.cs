using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>A backup in the list of the settings page.</summary>
    public sealed class BackupRow
    {
        public BackupInfo Backup { get; set; }
        public string Name => Backup.Name;
        public string CreatedText => Backup.Created.ToString("yyyy-MM-dd HH:mm");
        public string Details => $"{FormatSize(Backup.SizeBytes)} · {string.Join(", ", Backup.Files)}";
        public bool CanRestore => Backup.CanRestore;

        public static string FormatSize(long bytes) =>
            bytes >= 1024L * 1024 * 1024 ? $"{bytes / (1024.0 * 1024 * 1024):0.0} GB" :
            bytes >= 1024L * 1024 ? $"{bytes / (1024.0 * 1024):0.0} MB" :
            $"{Math.Max(1, bytes / 1024)} KB";
    }

    /// <summary>
    /// Backup &amp; restore (classic: Settings → Backups). Same folder layout as the classic app; a restore is
    /// staged and applied when TraXile restarts, and is blocked while Path of Exile runs.
    /// </summary>
    public sealed partial class SettingsViewModel
    {
        /// <summary>Checks whether Path of Exile runs (replaced in tests).</summary>
        public Func<bool> IsGameRunning { get; set; } = Backups.IsGameRunning;

        public ObservableCollection<BackupRow> BackupList { get; } = new ObservableCollection<BackupRow>();

        public bool HasBackups => BackupList.Count > 0;

        public string BackupFolder => Backups.Folder(_setup);

        private string _newBackupName = "";
        public string NewBackupName
        {
            get => _newBackupName;
            set
            {
                if (!SetProperty(ref _newBackupName, value ?? "")) return;
                OnPropertyChanged(nameof(BackupNameError));
                OnPropertyChanged(nameof(HasBackupNameError));
            }
        }

        /// <summary>Shown under the name box while the name is not usable.</summary>
        public string BackupNameError => Backups.ValidateName(NewBackupName);

        public bool HasBackupNameError => BackupNameError != null;

        private BackupRow _selectedBackup;
        public BackupRow SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                if (!SetProperty(ref _selectedBackup, value)) return;
                OnPropertyChanged(nameof(HasSelectedBackup));
                OnPropertyChanged(nameof(CanChangeSelectedBackup));
            }
        }

        public bool HasSelectedBackup => SelectedBackup != null;

        /// <summary>Restore and delete are possible: a backup is selected and settings may be written.</summary>
        public bool CanChangeSelectedBackup => CanWrite && SelectedBackup != null;

        public void LoadBackups()
        {
            string selected = SelectedBackup?.Backup.Path;
            BackupList.Clear();
            try
            {
                foreach (BackupInfo b in Backups.List(_setup)) BackupList.Add(new BackupRow { Backup = b });
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // unreadable backup folder: show none
            }
            SelectedBackup = BackupList.FirstOrDefault(r => r.Backup.Path == selected);
            OnPropertyChanged(nameof(HasBackups));
        }

        public bool CreateBackup(DateTime now, out BackupRow created, out string error)
        {
            created = null;
            error = !CanWrite ? "Settings are read only right now." : Backups.ValidateName(NewBackupName);
            if (error != null) return false;

            try
            {
                BackupInfo b = Backups.Create(_setup, NewBackupName, now);
                NewBackupName = "";
                LoadBackups();
                created = BackupList.FirstOrDefault(r => r.Backup.Path == b.Path);
                SelectedBackup = created;
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool DeleteBackup(BackupRow row, out string error)
        {
            error = !CanWrite ? "Settings are read only right now." : row == null ? "Select a backup first." : null;
            if (error != null) return false;

            try
            {
                Backups.Delete(_setup, row.Backup);
                LoadBackups();
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>Stages the backup and restarts TraXile, which applies it before the core starts.</summary>
        public bool RestoreBackup(BackupRow row, out string error)
        {
            error = !CanWrite ? "Settings are read only right now."
                : row == null ? "Select a backup first."
                : !row.CanRestore ? "This backup has no database (data.db) and cannot be restored."
                : IsGameRunning() ? "Path of Exile is running. Close it first, it keeps Client.txt open."
                : null;
            if (error != null) return false;

            try
            {
                Backups.PrepareRestore(_setup, row.Backup);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                error = ex.Message;
                return false;
            }
            _restart();
            return true;
        }
    }
}
