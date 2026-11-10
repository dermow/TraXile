using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace TraXile.Next.Views
{
    public partial class HelpPage : Page
    {
        public HelpPage()
        {
            InitializeComponent();
            DataContext = App.Help;
            Loaded += (s, e) => App.Help.Support?.Refresh(); // window size, settings and log may have changed
        }

        private async void CreateBundle_Click(object sender, RoutedEventArgs e)
        {
            if (await App.Help.Support.CreateBundleAsync(System.DateTime.Now)) OpenInExplorer(App.Help.Support.LastBundle, select: true);
        }

        private void CopySupport_Click(object sender, RoutedEventArgs e) => App.Help.Support.CopyToClipboard();

        private void OpenSupportFolder_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory(App.Help.Support.SupportFolder);
            OpenInExplorer(App.Help.Support.SupportFolder, select: false);
        }

        private static void OpenInExplorer(string path, bool select)
        {
            try
            {
                Process.Start("explorer.exe", select ? $"/select,\"{path}\"" : $"\"{path}\"");
            }
            catch (System.Exception)
            {
                // Explorer not available: the status line still shows where the bundle is
            }
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (System.Exception)
            {
                // no browser configured: nothing to do
            }
            e.Handled = true;
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (await App.Help.Update.CheckAsync() != null) App.ShowUpdateDialog(Window.GetWindow(this));
        }
    }
}
