using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace TraXile.Next.Views
{
    /// <summary>
    /// Shows every base style of the design system (not in the navigation; open with --page gallery).
    /// </summary>
    public partial class StyleGalleryPage : Page
    {
        public StyleGalleryPage()
        {
            InitializeComponent();

            SnapshotRun.Text = $"token snapshot {DesignTokens.SnapshotDate} · {(App.IsDark ? "dark" : "light")}";

            Swatches.ItemsSource = DesignTokens.All
                .Where(t => t.Css != "(derived)")
                .Select(t => new { t.Key, t.Css, Brush = new SolidColorBrush(t.For(App.IsDark)) })
                .ToList();

            SampleGrid.ItemsSource = new[]
            {
                new { Area = "Crimson Temple", Level = "T16", Duration = "4:12" },
                new { Area = "Bunker", Level = "Lvl 83", Duration = "6:40" },
                new { Area = "Forbidden Sanctum", Level = "Lvl 83", Duration = "18:02" },
                new { Area = "Strand", Level = "T16", Duration = "3:55" },
            };
            SampleGrid.SelectedIndex = 1;
        }
    }
}
