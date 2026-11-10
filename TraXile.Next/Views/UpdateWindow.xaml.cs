using System.Windows;
using TraXile.Next.ViewModels;
using Wpf.Ui.Controls;

namespace TraXile.Next.Views
{
    /// <summary>
    /// Update dialog (classic UpdateCheckForm): installed and latest version, the changelog of the latest version and
    /// "Install update", which starts the updater and closes TraXile. Without an update it only confirms "up to date".
    /// </summary>
    public partial class UpdateWindow : FluentWindow
    {
        private readonly UpdateViewModel _vm;

        public UpdateWindow(UpdateViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;

            if (vm.IsUpdateAvailable)
            {
                Headline.Text = $"TraXile {vm.LatestVersion} is available";
            }
            else
            {
                StateLabel.Text = "NO UPDATE";
                StateLabel.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "StatusSuccessBrush");
                Headline.Text = "You are up to date";
                InstallButton.Visibility = Visibility.Collapsed;
                CloseButton.Content = "Close";
                CloseButton.Appearance = ControlAppearance.Primary;
            }
        }

        public System.Windows.Controls.Panel ContentRoot => RootGrid;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            string error = _vm.Install();
            if (error != null)
            {
                ErrorText.Text = error;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            // The updater downloads the installer and closes TraXile; stop the core cleanly before that happens
            Close();
            Application.Current.Shutdown();
        }
    }
}
