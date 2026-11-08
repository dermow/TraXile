using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class StatisticsPage : Page
    {
        public StatisticsPage()
        {
            InitializeComponent();
            DataContext = App.Statistics;
        }
    }
}
