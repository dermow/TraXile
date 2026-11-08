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
    }
}
