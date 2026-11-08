using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Wpf.Ui.Appearance;

namespace TraXile.Next
{
    /// <summary>
    /// Command line:
    ///   --demo                 use sample data instead of the real tracker
    ///   --light / --dark       start with the given theme (default: dark)
    ///   --screenshot file.png  render the main window off-screen to a PNG and exit (implies --demo)
    ///   --page log             open the given page first (dashboard, log)
    ///   --select 0,2           preselect activity log rows (for screenshots)
    /// </summary>
    public partial class App : Application
    {
        public static ITrackingSource Source { get; private set; }
        public static DashboardViewModel Dashboard { get; private set; }
        public static ActivityLogViewModel ActivityLog { get; private set; }
        public static bool IsDark { get; private set; } = true;
        public static string ScreenshotPath { get; private set; }

        /// <summary>Page to open first (--page dashboard|log), mainly for screenshots.</summary>
        public static string StartPage { get; private set; }

        /// <summary>Rows to preselect in the activity log (--select 0,2), for screenshots.</summary>
        public static int[] SelectRows { get; private set; } = Array.Empty<int>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = e.Args;
            int shotIdx = Array.FindIndex(args, a => a.Equals("--screenshot", StringComparison.OrdinalIgnoreCase));
            if (shotIdx >= 0 && shotIdx + 1 < args.Length) ScreenshotPath = args[shotIdx + 1];
            int pageIdx = Array.FindIndex(args, a => a.Equals("--page", StringComparison.OrdinalIgnoreCase));
            if (pageIdx >= 0 && pageIdx + 1 < args.Length) StartPage = args[pageIdx + 1];
            int selIdx = Array.FindIndex(args, a => a.Equals("--select", StringComparison.OrdinalIgnoreCase));
            if (selIdx >= 0 && selIdx + 1 < args.Length)
            {
                SelectRows = args[selIdx + 1].Split(',').Select(s => int.TryParse(s, out int i) ? i : -1).Where(i => i >= 0).ToArray();
            }

            bool demo = ScreenshotPath != null || args.Any(a => a.Equals("--demo", StringComparison.OrdinalIgnoreCase));
            IsDark = !args.Any(a => a.Equals("--light", StringComparison.OrdinalIgnoreCase));

            ApplyTheme(IsDark);

            Source = CreateSource(demo);
            Source.Start();

            // The UI is English -> format numbers/dates accordingly. Only done for the UI thread and
            // only after the core has loaded its data, so the core's parsing behaviour is unchanged.
            var en = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = en;
            System.Threading.Thread.CurrentThread.CurrentUICulture = en;
            Dashboard = new DashboardViewModel(Source);
            Dashboard.SetTheme(IsDark);
            ActivityLog = new ActivityLogViewModel(Source);

            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }

        private static ITrackingSource CreateSource(bool demo)
        {
            if (demo) return new DemoTrackingSource();

            // Both UIs would parse Client.txt and write the same SQLite database
            if (Process.GetProcessesByName("TraXile").Length > 0)
            {
                return new DemoTrackingSource(
                    "The classic TraXile is running. Close it to enable live tracking here — showing demo data meanwhile.");
            }

            try
            {
                LiveTrackingSource live = LiveTrackingSource.TryCreate(out string reason);
                return (ITrackingSource)live ?? new DemoTrackingSource(reason);
            }
            catch (Exception ex)
            {
                return new DemoTrackingSource($"Could not start live tracking ({ex.Message}) — showing demo data.");
            }
        }

        private static ResourceDictionary _tokens;

        /// <summary>
        /// Switches between the dark and light variant of the design system (docs/design.md).
        /// WPF-UI's own theme is applied first; the token dictionary is then (re-)added as the last merged
        /// dictionary, so its values win over the library defaults.
        /// </summary>
        public static void ApplyTheme(bool dark)
        {
            IsDark = dark;
            ActivityPalette.IsDark = dark;
            ApplicationThemeManager.Apply(dark ? ApplicationTheme.Dark : ApplicationTheme.Light,
                Wpf.Ui.Controls.WindowBackdropType.None, updateAccent: false);

            var merged = Current.Resources.MergedDictionaries;
            if (_tokens != null) merged.Remove(_tokens);
            _tokens = DesignTokens.CreateDictionary(dark);
            merged.Add(_tokens);

            // Coral accent for WPF-UI's accent brushes (selection pills, focus, toggles)
            ApplicationAccentColorManager.Apply(
                DesignTokens.Get("Accent").For(dark),
                DesignTokens.Get("BtnPrimaryBg").For(dark),
                DesignTokens.Get("BtnPrimaryBg").For(dark),
                DesignTokens.Get("BtnPrimaryHover").For(dark));

            // Solid warm background instead of Mica
            Current.MainWindow?.SetResourceReference(Window.BackgroundProperty, "BgBrush");

            LiveCharts.Configure(c =>
            {
                if (dark) c.AddDarkTheme(); else c.AddLightTheme();
            });

            Dashboard?.SetTheme(dark);
            ActivityLog?.Refresh(); // re-reads the activity colors of the new theme
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dashboard?.Dispose();
            ActivityLog?.Dispose();
            Source?.Dispose();
            base.OnExit(e);
        }
    }
}
