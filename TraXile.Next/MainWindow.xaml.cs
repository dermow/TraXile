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
                // give charts time to finish their intro animations
                if (App.ScreenshotPath != null) Screenshot.Schedule(this, App.ScreenshotPath, TimeSpan.FromSeconds(3));
            };
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
