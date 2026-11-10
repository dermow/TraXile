using System.Windows;
using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            DataContext = App.Dashboard;
        }

        private void ViewAll_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.RootNavigation.Navigate(typeof(ActivityLogPage));
        }
    }
}
