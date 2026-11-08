using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TraXile.Next.Views
{
    /// <summary>
    /// The shared filter bar with the "filter active" banner. Binds to <see cref="App.FilterBar"/> unless the
    /// hosting page sets another DataContext on it.
    /// </summary>
    public partial class GlobalFilterBar : UserControl
    {
        public GlobalFilterBar()
        {
            InitializeComponent();
            if (App.FilterBar != null) DataContext = App.FilterBar;
        }

        private void TagsButton_Click(object sender, RoutedEventArgs e)
        {
            // A context menu is not part of the visual tree: hand over the items explicitly
            TagsMenu.DataContext = DataContext;
            TagsMenu.ItemsSource = (DataContext as ViewModels.FilterBarViewModel)?.TagChoices;
            TagsMenu.PlacementTarget = TagsButton;
            TagsMenu.Placement = PlacementMode.Bottom;
            TagsMenu.IsOpen = true;
        }
    }
}
