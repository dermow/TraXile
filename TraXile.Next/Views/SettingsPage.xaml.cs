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
    }
}
