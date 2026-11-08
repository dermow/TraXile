using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TraXile.Next.Views;
using Wpf.Ui.Controls;

namespace TraXile.Next
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ModeText.Text = App.Source.IsDemo ? "DEMO" : "● LIVE";
            if (!App.Source.IsDemo)
            {
                ModeText.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "StatusSuccessBrush");
            }
            UpdateThemeIcon();

            if (App.ScreenshotPath != null)
            {
                PrepareOffscreenRender();
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
                    bool log = string.Equals(App.StartPage, "log", StringComparison.OrdinalIgnoreCase);
                    RootNavigation.Navigate(log ? typeof(ActivityLogPage) : typeof(DashboardPage));
                }
                if (App.ScreenshotPath != null) ScheduleScreenshot();
            };
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            App.ApplyTheme(!App.IsDark);
            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            ThemeIcon.Symbol = App.IsDark ? SymbolRegular.WeatherSunny24 : SymbolRegular.WeatherMoon24;
        }

        // ------------------------------------------------------------------
        // Off-screen screenshot support (used for docs / visual review)
        // ------------------------------------------------------------------

        private void PrepareOffscreenRender()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -30000;
            Top = -30000;
            ShowActivated = false;
            ShowInTaskbar = false;
            Height = 1620; // capture the whole dashboard without scrolling

            // RenderTargetBitmap only captures the content, not the window background
            RootGrid.SetResourceReference(System.Windows.Controls.Panel.BackgroundProperty, "BgBrush");
        }

        private void ScheduleScreenshot()
        {
            // Give charts time to finish their intro animations
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                try
                {
                    SaveScreenshot(App.ScreenshotPath);
                }
                finally
                {
                    Application.Current.Shutdown();
                }
            };
            timer.Start();
        }

        private void SaveScreenshot(string path)
        {
            const double scale = 1.25;
            var root = (FrameworkElement)Content;
            int w = (int)(root.ActualWidth * scale);
            int h = (int)(root.ActualHeight * scale);

            var rtb = new RenderTargetBitmap(w, h, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            rtb.Render(root);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using (var fs = File.Create(path))
            {
                encoder.Save(fs);
            }
        }
    }
}
