using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class LeaguesPage : Page
    {
        public LeaguesPage()
        {
            InitializeComponent();
            DataContext = App.Leagues;
        }
    }
}
