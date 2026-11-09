using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TraXile.Next.ViewModels;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace TraXile.Next.Views
{
    public partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _vm;

        public SettingsPage()
        {
            InitializeComponent();
            _vm = App.Settings;
            DataContext = _vm;
            Loaded += (s, e) =>
            {
                _vm.LoadTagSlotOptions(); // tags may have been added or renamed meanwhile
                _vm.LoadBackups();        // the classic app may have created some
            };
        }

        /// <summary>Asks before an action that restarts TraXile; true if confirmed.</summary>
        private async System.Threading.Tasks.Task<bool> ConfirmAsync(string title, string text, string action, bool danger = false)
        {
            var box = new MessageBox
            {
                Title = title,
                Content = text,
                PrimaryButtonText = action,
                PrimaryButtonAppearance = danger ? Wpf.Ui.Controls.ControlAppearance.Danger : Wpf.Ui.Controls.ControlAppearance.Primary,
                CloseButtonText = "Cancel",
                Owner = Window.GetWindow(this)
            };
            return await box.ShowDialogAsync() == MessageBoxResult.Primary;
        }

        private void ShowError(string title, string text) =>
            _ = new MessageBox { Title = title, Content = text, CloseButtonText = "OK", Owner = Window.GetWindow(this) }.ShowDialogAsync();

        private async void ChangeClientTxt_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select the Client.txt of Path of Exile",
                Filter = "Path of Exile log (Client.txt)|Client.txt|Text files (*.txt)|*.txt",
                CheckFileExists = true
            };
            if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

            bool ok = await ConfirmAsync("Use this log?",
                "TraXile restarts and reads the new log from the start. Activities are rebuilt from it; " +
                "tags and pauses you added by hand are lost.",
                "Restart and read log");
            if (!ok) return;

            if (!_vm.ChangeClientTxt(dlg.FileName, out string error)) ShowError("Could not change the log", error);
        }

        private async void Reload_Click(object sender, RoutedEventArgs e)
        {
            if (!await ConfirmAsync("Reload the log?",
                "TraXile restarts and reads Client.txt again from the start. All activities are rebuilt; " +
                "tags and pauses you added by hand are lost.", "Restart and reload")) return;

            if (!_vm.ReloadLogfile(out string error)) ShowError("Reload failed", error);
        }

        private async void Shrink_Click(object sender, RoutedEventArgs e)
        {
            if (!await ConfirmAsync("Shrink the log?",
                "A copy of Client.txt is kept next to it (Client.<date>.txt) and the original is emptied. " +
                "Your history stays. TraXile restarts afterwards.", "Shrink and restart")) return;

            if (!_vm.ShrinkLogfile(System.DateTime.Now, out _, out string error)) ShowError("Shrinking failed", error);
        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            var box = new MessageBox
            {
                Title = "Reset everything?",
                Content = "All activities and statistics are deleted and Client.txt is emptied. This cannot be undone.\n\n" +
                          "It is recommended to create a backup first.",
                PrimaryButtonText = "Back up and reset",
                SecondaryButtonText = "Reset without backup",
                CloseButtonText = "Cancel",
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,
                Owner = Window.GetWindow(this)
            };
            MessageBoxResult choice = await box.ShowDialogAsync();
            if (choice == MessageBoxResult.None) return;

            bool backup = choice == MessageBoxResult.Primary;
            if (!_vm.FullReset(backup, System.DateTime.Now, out _, out string error)) ShowError("Reset failed", error);
        }

        private void SaveTimeCaps_Click(object sender, RoutedEventArgs e)
        {
            if (!_vm.SaveTimeCaps(out string error)) ShowError("Time caps not saved", error);
        }

        // ------------------------------------------------------------------
        // Backups
        // ------------------------------------------------------------------

        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.CreateBackup(System.DateTime.Now, out _, out string error)) return;
            ShowError("Backup not created", error);
        }

        private void BackupName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            BackupNameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            CreateBackup_Click(sender, e);
            e.Handled = true;
        }

        private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            BackupRow row = _vm.SelectedBackup;
            if (row == null) return;
            if (!await ConfirmAsync($"Restore \"{row.Name}\" from {row.CreatedText}?",
                "Your current activities, statistics and settings are replaced by the backup. TraXile restarts and applies it " +
                "before tracking starts. Make sure Path of Exile is closed.\n\nTip: create a backup of the current state first.",
                "Restore and restart", danger: true)) return;

            if (!_vm.RestoreBackup(row, out string error)) ShowError("Restore not possible", error);
        }

        private async void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            BackupRow row = _vm.SelectedBackup;
            if (row == null) return;
            if (!await ConfirmAsync($"Delete \"{row.Name}\" from {row.CreatedText}?", "The backup folder is deleted. This cannot be undone.",
                "Delete backup", danger: true)) return;

            if (!_vm.DeleteBackup(row, out string error)) ShowError("Backup not deleted", error);
        }

        private void OpenBackupFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.IO.Directory.CreateDirectory(_vm.BackupFolder);
                // Shell execute: opens the folder in Explorer (not the default on .NET)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_vm.BackupFolder) { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                ShowError("Could not open the backup folder", ex.Message);
            }
        }

        private void RestartNow_Click(object sender, RoutedEventArgs e) => _vm.RestartNow();
    }
}