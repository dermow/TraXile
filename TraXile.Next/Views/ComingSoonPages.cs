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
            var app = Application.Current;
            var stack = new StackPanel { Margin = (Thickness)app.FindResource("PagePadding"), MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };

            stack.Children.Add(new TextBlock { Text = title, Style = (Style)app.FindResource("PageTitle") });
            stack.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 24),
                Style = (Style)app.FindResource("Meta")
            });

            // Empty state: icon, what will move here, nothing to click yet
            var card = new Border { Style = (Style)app.FindResource("Card"), Padding = new Thickness(24) };
            var list = new StackPanel();

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
            var mark = new SymbolIcon { Symbol = icon, FontSize = 20, Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            mark.SetResourceReference(SymbolIcon.ForegroundProperty, "AccentTextBrush");
            header.Children.Add(mark);
            var label = new TextBlock { Text = "MIGRATION IN PROGRESS", Style = (Style)app.FindResource("Label"), VerticalAlignment = VerticalAlignment.Center };
            label.SetResourceReference(TextBlock.ForegroundProperty, "AccentTextBrush");
            header.Children.Add(label);
            list.Children.Add(header);

            foreach (string f in features)
            {
                var row = new Border { Style = (Style)app.FindResource("RowCard"), Margin = new Thickness(0, 0, 0, 4), Padding = new Thickness(16, 10, 16, 10) };
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                var check = new SymbolIcon { Symbol = SymbolRegular.Circle20, FontSize = 14, Margin = new Thickness(0, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
                check.SetResourceReference(SymbolIcon.ForegroundProperty, "TextMutedBrush");
                content.Children.Add(check);
                content.Children.Add(new TextBlock { Text = f, Style = (Style)app.FindResource("Body"), VerticalAlignment = VerticalAlignment.Center });
                row.Child = content;
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

    public sealed class LabPage : ComingSoonPage
    {
        public LabPage() : base("Lab", SymbolRegular.Shield24,
            "Labyrinth runs and enchant profit tracking.",
            "Labs per difficulty and average time",
            "Profit tracking table") { }
    }

    public sealed class HeistPage : ComingSoonPage
    {
        public HeistPage() : base("Heist", SymbolRegular.LockClosed24,
            "Heist statistics by area and area level.",
            "Heists by area level",
            "Average time per heist") { }
    }

    public sealed class LeaguesPage : ComingSoonPage
    {
        public LeaguesPage() : base("Leagues", SymbolRegular.Trophy24,
            "Your activities and statistics per league.",
            "League overview with play time",
            "Compare two leagues") { }
    }

    public sealed class DivCardsPage : ComingSoonPage
    {
        public DivCardsPage() : base("Div Cards", SymbolRegular.Collections24,
            "Divination cards you found while playing.",
            "Cards found per league",
            "Where each card dropped") { }
    }
    public sealed class TagsPage : ComingSoonPage
    {
        public TagsPage() : base("Tags", SymbolRegular.TagMultiple24,
            "Create and edit tags, colors and sound notifications.",
            "Color picker with live chip preview",
            "Sound notification per tag",
            "Overlay tag selection") { }
    }
}
