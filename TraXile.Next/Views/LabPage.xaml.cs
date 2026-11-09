using System.Windows.Controls;

namespace TraXile.Next.Views
{
    public partial class LabPage : Page
    {
        public LabPage()
        {
            InitializeComponent();
            DataContext = App.Lab;
        }
    }
}
