using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace TraXile.Next.Shell
{
    /// <summary>Small informational dialogs (the full Help and About pages follow in phase 4).</summary>
    public static class Dialogs
    {
        /// <summary>The chat commands the core understands (same list as the classic help window).</summary>
        public static readonly (string Command, string Description)[] ChatCommands =
        {
            ("trax::tag <tag>", "Add a custom tag to the current activity"),
            ("trax::untag <tag>", "Remove a custom tag from the current activity"),
            ("trax::pause", "Pause the current tracking"),
            ("trax::resume", "Resume paused tracking"),
            ("trax::finish", "Finish the current activity manually"),
        };

        public static void ShowChatCommands(Window owner)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "Type these in the Path of Exile chat to control TraXile:",
                Style = (Style)Application.Current.FindResource("Body"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });
            foreach (var (command, description) in ChatCommands)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                row.ColumnDefinitions.Add(new ColumnDefinition());
                var cmd = new TextBlock { Text = command, Style = (Style)Application.Current.FindResource("Body"), FontWeight = FontWeights.SemiBold };
                var desc = new TextBlock { Text = description, Style = (Style)Application.Current.FindResource("Meta"), TextWrapping = TextWrapping.Wrap };
                Grid.SetColumn(desc, 1);
                row.Children.Add(cmd);
                row.Children.Add(desc);
                panel.Children.Add(row);
            }

            _ = new MessageBox { Title = "Chat commands", Content = panel, CloseButtonText = "Close", Owner = owner, MinWidth = 480 }.ShowDialogAsync();
        }

        public static void ShowAbout(Window owner)
        {
            string version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "3.0";
            _ = new MessageBox
            {
                Title = "About TraXile",
                Content = $"TraXile {version}\n\nTracks what you do in Path of Exile by reading the game's log.\nhttps://github.com/dermow/TraXile",
                CloseButtonText = "Close",
                Owner = owner
            }.ShowDialogAsync();
        }
    }
}
