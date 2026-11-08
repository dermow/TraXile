using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class HeistPage : Page
    {
        public HeistPage()
        {
            InitializeComponent();
            DataContext = App.Heist;
        }
    }
}
