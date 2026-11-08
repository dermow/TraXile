using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace TraXile.Next.Views
{
    /// <summary>
    /// Placeholder for screens that are not yet migrated from the classic UI.
    /// Each page lists what will move there so the navigation structure can already be reviewed.
    /// </summary>
    public abstract class ComingSoonPage : Page
    {
        protected ComingSoonPage(string title, SymbolRegular icon, string description, params string[] features)
        {
            var stack = new StackPanel { Margin = new Thickness(36, 28, 36, 28), MaxWidth = 720, HorizontalAlignment = HorizontalAlignment.Left };

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            header.Children.Add(new SymbolIcon
            {
                Symbol = icon,
                FontSize = 28,
                Margin = new Thickness(0, 0, 12, 0),
                Foreground = (Brush)Application.Current.FindResource("AccentTextBrush")
            });
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 30,
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)Application.Current.FindResource("UiFont")
            };
            titleText.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            header.Children.Add(titleText);
            stack.Children.Add(header);

            var desc = new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 24) };
            desc.Style = (Style)Application.Current.FindResource("Body");
            desc.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorSecondaryBrush");
            stack.Children.Add(desc);

            var card = new Border { Style = (Style)Application.Current.FindResource("DashCard") };
            var list = new StackPanel();
            var pill = new Border
            {
                Style = (Style)Application.Current.FindResource("Chip"),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 14),
                Background = new SolidColorBrush(Color.FromArgb(40, 0xF5, 0xB9, 0x42)),
                Child = new TextBlock
                {
                    Text = "Migration in progress",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)Application.Current.FindResource("AccentTextBrush")
                }
            };
            list.Children.Add(pill);

            foreach (string f in features)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
                row.Children.Add(new SymbolIcon { Symbol = SymbolRegular.CheckmarkCircle24, FontSize = 16, Margin = new Thickness(0, 0, 10, 0), Opacity = 0.6 });
                var t = new TextBlock { Text = f, Style = (Style)Application.Current.FindResource("Body") };
                row.Children.Add(t);
                list.Children.Add(row);
            }
            card.Child = list;
            stack.Children.Add(card);

            Content = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }
    }

    public sealed class StatisticsPage : ComingSoonPage
    {
        public StatisticsPage() : base("Statistics", SymbolRegular.DataTrending24,
            "All counters (kills, deaths, cards, bosses, ...) with interactive trend charts.",
            "Searchable stat list with sparklines",
            "Zoomable time-series chart per stat",
            "League comparison") { }
    }

    public sealed class MappingPage : ComingSoonPage
    {
        public MappingPage() : base("Mapping", SymbolRegular.Map24,
            "Map tier distribution, average times and your fastest maps.",
            "Maps per tier and avg. time per tier",
            "Per-area statistics and personal bests",
            "Mechanic (tag) share over time") { }
    }

    public sealed class BossingPage : ComingSoonPage
    {
        public BossingPage() : base("Bossing", SymbolRegular.Trophy24,
            "Kill counts, tries and kill times for every pinnacle boss.",
            "Boss cards with kills / tries / best time",
            "Uber vs. normal comparison") { }
    }

    public sealed class LabHeistPage : ComingSoonPage
    {
        public LabHeistPage() : base("Labs & Heist", SymbolRegular.Shield24,
            "Labyrinth runs, enchant profit tracking and heist statistics.",
            "Labs per difficulty, average time",
            "Profit tracking table",
            "Heists by area level") { }
    }

    public sealed class TagsPage : ComingSoonPage
    {
        public TagsPage() : base("Tags", SymbolRegular.TagMultiple24,
            "Create and edit tags, colors and sound notifications.",
            "Color picker with live chip preview",
            "Sound notification per tag",
            "Overlay tag selection") { }
    }

    public sealed class SettingsPage : ComingSoonPage
    {
        public SettingsPage() : base("Settings", SymbolRegular.Settings24,
            "Client.txt location, time caps, overlays, backups and updates.",
            "Theme (light / dark / system) and accent color",
            "Time caps per activity type",
            "Overlay configuration, backups & restore") { }
    }
}
