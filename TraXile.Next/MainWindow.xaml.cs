using System;
using System.IO;
using System.Linq;
using System.Windows;
using TraXile.Next.Views;
using Wpf.Ui.Controls;

namespace TraXile.Next
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadingOverlay.DataContext = App.Loading;
            UpdateButton.DataContext = App.Update;

            ModeText.Text = App.Source.IsDemo ? "DEMO" : "● LIVE";
            if (!App.Source.IsDemo)
            {
                ModeText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "StatusSuccessBrush");
            }
            UpdateThemeIcon();
            App.ThemeChanged += (s, e) => UpdateThemeIcon();

            if (App.ScreenshotPath != null)
            {
                Screenshot.PrepareOffscreen(this, 1620, RootGrid); // tall enough for the whole dashboard
            }

            Loaded += (s, e) =>
            {
                if (string.Equals(App.StartPage, "gallery", StringComparison.OrdinalIgnoreCase))
                {
                    // Not part of the navigation: show it in place of the page content
                    RootNavigation.Navigate(typeof(DashboardPage));
                    RootNavigation.ReplaceContent(new StyleGalleryPage());
                }
                else
                {
                    RootNavigation.Navigate(StartPageType(App.StartPage));
                }
                if (App.ScreenshotPath != null && App.ScrollOffset > 0)
                {
                    // once the page has navigated in: scroll its outer scroll viewer (screenshots of lower sections)
                    var scroll = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                    scroll.Tick += (_, __) =>
                    {
                        scroll.Stop();
                        FindScrollViewer(RootNavigation)?.ScrollToVerticalOffset(App.ScrollOffset);
                    };
                    scroll.Start();
                }
                // give charts time to finish their intro animations
                if (App.ScreenshotPath != null) Screenshot.Schedule(this, App.ScreenshotPath, TimeSpan.FromSeconds(3));
            };
        }

        /// <summary>The scroll viewer that scrolls the page (the one with the most to scroll).</summary>
        private static System.Windows.Controls.ScrollViewer FindScrollViewer(DependencyObject root) =>
            Descendants(root).OfType<System.Windows.Controls.ScrollViewer>()
                .OrderByDescending(s => s.ScrollableHeight).FirstOrDefault(s => s.ScrollableHeight > 0);

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                yield return child;
                foreach (DependencyObject d in Descendants(child)) yield return d;
            }
        }

        /// <summary>
        /// Page for --page: "log" or the start of a page name ("settings", "tags", "lab", ...); dashboard otherwise.
        /// </summary>
        private Type StartPageType(string name)
        {
            if (string.IsNullOrEmpty(name)) return typeof(DashboardPage);
            if (name.Equals("log", StringComparison.OrdinalIgnoreCase)) return typeof(ActivityLogPage);

            var items = RootNavigation.MenuItems.OfType<NavigationViewItem>()
                .Concat(RootNavigation.FooterMenuItems.OfType<NavigationViewItem>());
            return items.Select(i => i.TargetPageType)
                       .FirstOrDefault(t => t != null && t.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                   ?? typeof(DashboardPage);
        }

        /// <summary>Opens a page of the navigation (tray menu: Help).</summary>
        public void ShowPage(Type pageType) => RootNavigation.Navigate(pageType);

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            // Like the classic app: minimized to the notification area instead of the taskbar
            if (WindowState == WindowState.Minimized && App.MinimizeToTray && App.Tray != null)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }

        private void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            StopwatchOverlayItem.IsChecked = App.Overlays.IsStopwatchOpen;
            TagsOverlayItem.IsChecked = App.Overlays.IsTagsOpen;
            OverlayMenu.PlacementTarget = OverlayButton;
            OverlayMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            OverlayMenu.IsOpen = true;
        }

        private void StopwatchOverlay_Click(object sender, RoutedEventArgs e) => App.Overlays.ToggleStopwatch();

        private void TagsOverlay_Click(object sender, RoutedEventArgs e) => App.Overlays.ToggleTags();

        private void UpdateButton_Click(object sender, RoutedEventArgs e) => App.ShowUpdateDialog(this);

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            App.SetTheme(!App.IsDark);
        }

        private void UpdateThemeIcon()
        {
            ThemeIcon.Symbol = App.IsDark ? SymbolRegular.WeatherSunny24 : SymbolRegular.WeatherMoon24;
        }
    }
}
