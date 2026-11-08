using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class MappingPage : Page
    {
        public MappingPage()
        {
            InitializeComponent();
            DataContext = App.Mapping;
        }
    }
}
