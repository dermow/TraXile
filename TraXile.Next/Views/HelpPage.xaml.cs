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
