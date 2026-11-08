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
    ///   --page log             open the given page first (dashboard, log, settings, ..., gallery, firstrun, firstrun-empty)
    ///   --select 0,2           preselect activity log rows (for screenshots)
    ///   --loading              show the loading state (demo, for screenshots)
    /// </summary>
    public partial class App : Application
    {
        public static ITrackingSource Source { get; private set; }
        public static DashboardViewModel Dashboard { get; private set; }
        public static ActivityLogViewModel ActivityLog { get; private set; }
        public static LoadingViewModel Loading { get; private set; }
        public static AppInstance Instance { get; private set; }
        public static Shell.TrayIcon Tray { get; private set; }

        /// <summary>Setting MinimizeToTray (shared with the classic app): minimizing hides the window in the tray.</summary>
        public static bool MinimizeToTray { get; set; }

        private static string[] _args = Array.Empty<string>();
        private static bool _restarted;
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
            _args = args;
            _restarted = args.Any(a => a.Equals(AppInstance.RestartedArg, StringComparison.OrdinalIgnoreCase));
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

            if (ScreenshotPath != null && StartPage != null && StartPage.StartsWith("firstrun", StringComparison.OrdinalIgnoreCase))
            {
                ScreenshotFirstRun(withCandidates: !StartPage.EndsWith("-empty", StringComparison.OrdinalIgnoreCase));
                return;
            }

            // Only one TraXile.Next: a second start brings the running one to the front
            if (ScreenshotPath == null)
            {
                Instance = new AppInstance();
                if (!Instance.TryClaimInstance(_restarted ? TimeSpan.FromSeconds(15) : TimeSpan.Zero))
                {
                    Shutdown();
                    return;
                }
                Instance.ShowRequested += (s, a) => Dispatcher.BeginInvoke(new Action(() => Tray?.ShowWindow()));
                bool.TryParse(AppSetup.Default.ReadSetting("MinimizeToTray", "false"), out bool toTray);
                MinimizeToTray = toTray;
            }

            // Dialogs may open before the main window exists; don't let closing them end the app
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Source = CreateSource(demo, out RestoreResult restore);
            if (Source is DemoTrackingSource demoSource && args.Any(a => a.Equals("--loading", StringComparison.OrdinalIgnoreCase)))
            {
                demoSource.SimulateLoading = true; // screenshot of the loading state
            }
            Source.Start();
            Loading = new LoadingViewModel(Source);

            // The UI is English -> format numbers/dates accordingly. Only done for the UI thread and
            // only after the core has loaded its data, so the core's parsing behaviour is unchanged.
            var en = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = en;
            System.Threading.Thread.CurrentThread.CurrentUICulture = en;
            Dashboard = new DashboardViewModel(Source);
            Dashboard.SetTheme(IsDark);
            ActivityLog = new ActivityLogViewModel(Source);

            if (restore.HasRestored && !restore.HasFailed)
            {
                Dashboard.AddNotice("Backup restored", $"Your backup was applied ({restore.RestoredFiles.Count} files).");
            }
            else if (restore.HasFailed)
            {
                Dashboard.AddNotice("Restore failed", restore.Error, isError: true);
            }

            var window = new MainWindow();
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();

            if (ScreenshotPath == null)
            {
                Tray = new Shell.TrayIcon(window);
                Tray.ExitRequested += (s, a) => Shutdown();
            }
        }

        /// <summary>
        /// Restarts TraXile.Next (after reload, reset or restore): the new process waits until this one has
        /// stopped the core and released its locks.
        /// </summary>
        public static void Restart()
        {
            AppInstance.StartNewProcess(_args);
            Current.Shutdown();
        }

        /// <summary>--screenshot x.png --page firstrun[-empty]: render the first-run window with sample paths.</summary>
        private void ScreenshotFirstRun(bool withCandidates)
        {
            string[] samples = withCandidates
                ? new[]
                {
                    @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile\logs\Client.txt",
                    @"D:\Games\Grinding Gear Games\Path of Exile\logs\Client.txt",
                }
                : new string[0];
            var vm = new FirstRunViewModel(new AppSetup(System.IO.Path.GetTempPath()), samples);
            var window = new Views.FirstRunWindow(vm);
            MainWindow = window;
            Screenshot.PrepareOffscreen(window, 0, window.ContentRoot);
            window.Show();
            Screenshot.Schedule(window, ScreenshotPath, TimeSpan.FromSeconds(1));
        }

        private static ITrackingSource CreateSource(bool demo, out RestoreResult restore)
        {
            restore = RestoreResult.Nothing;
            if (demo) return new DemoTrackingSource();

            // Both UIs would parse Client.txt and write the same SQLite database: hold the classic app's lock
            if (Instance == null || !Instance.TryClaimClassicLock(_restarted ? TimeSpan.FromSeconds(15) : TimeSpan.Zero))
            {
                return new DemoTrackingSource(
                    "The classic TraXile is running. Close it to enable live tracking here — showing demo data meanwhile.");
            }

            AppSetup setup = AppSetup.Default;

            // A backup restore prepared by this or the classic app is applied before the core opens the database
            restore = BackupRestore.ApplyIfPrepared(setup);

            if (!setup.HasValidClientTxt)
            {
                // First start (or the configured Client.txt is gone): let the user pick it
                var firstRun = new Views.FirstRunWindow(new FirstRunViewModel(setup, AppSetup.DetectClientTxt()));
                if (firstRun.ShowDialog() != true)
                {
                    return new DemoTrackingSource("No Client.txt selected yet — showing demo data. Restart TraXile to choose it.");
                }
            }

            try
            {
                LiveTrackingSource live = LiveTrackingSource.TryCreate(setup, out string reason);
                return (ITrackingSource)live ?? new DemoTrackingSource(reason);
            }
            catch (Exception ex)
            {
                return new DemoTrackingSource($"Could not start live tracking ({ex.Message}) — showing demo data.");
            }
        }

        private static ResourceDictionary _tokens;

        private static SkiaSharp.SKTypeface _chartTypeface;
        private static SkiaSharp.SKTypeface ChartTypeface
        {
            get
            {
                if (_chartTypeface == null)
                {
                    var info = GetResourceStream(new Uri("pack://application:,,,/TraXile.Next;component/Fonts/InstrumentSans-Regular.ttf"));
                    if (info != null) _chartTypeface = SkiaSharp.SKTypeface.FromStream(info.Stream);
                }
                return _chartTypeface;
            }
        }

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
                // Chart labels, legends and tool tips in Instrument Sans like the rest of the UI
                if (ChartTypeface != null) c.HasGlobalSKTypeface(ChartTypeface);
            });

            Dashboard?.SetTheme(dark);
            ActivityLog?.Refresh(); // re-reads the activity colors of the new theme
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dashboard?.Dispose();
            ActivityLog?.Dispose();
            Source?.Dispose();
            Loading?.Dispose();
            Tray?.Dispose();
            Instance?.Dispose(); // last: the core is stopped, a restarted process may take over
            base.OnExit(e);
        }
    }
}
