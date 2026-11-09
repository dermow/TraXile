using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TraXile.Next.Overlays;
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
    ///   --page log             open the given page first (dashboard, log, settings, ..., gallery, firstrun, firstrun-empty,
    ///                          overlay-stopwatch[-move], overlay-tags[-move])
    ///   --select 0,2           preselect activity log rows (for screenshots)
    ///   --scroll 1600          scroll the page down before the screenshot
    ///   --loading              show the loading state (demo, for screenshots)
    ///   --filter last7days,map set the global filter (for screenshots, see ApplyFilterArg)
    /// </summary>
    public partial class App : Application
    {
        public static ITrackingSource Source { get; private set; }
        public static DashboardViewModel Dashboard { get; private set; }
        public static ActivityLogViewModel ActivityLog { get; private set; }
        public static LoadingViewModel Loading { get; private set; }
        public static SettingsViewModel Settings { get; private set; }
        public static TagsViewModel Tags { get; private set; }

        /// <summary>The global filter of all analytics pages (in memory only).</summary>
        public static GlobalFilter Filter { get; private set; }

        /// <summary>State of the filter bar, shared by all filtered pages.</summary>
        public static FilterBarViewModel FilterBar { get; private set; }

        public static StatisticsViewModel Statistics { get; private set; }
        public static MappingViewModel Mapping { get; private set; }
        public static HeistViewModel Heist { get; private set; }
        public static LabViewModel Lab { get; private set; }
        public static DivCardsViewModel DivCards { get; private set; }
        public static LeaguesViewModel Leagues { get; private set; }

        /// <summary>The analytics pages (on the global filter), for theme switches and shutdown.</summary>
        private static AnalyticsViewModel[] AnalyticsPages => new AnalyticsViewModel[] { Statistics, Mapping, Heist, Lab, DivCards, Leagues };

        /// <summary>Settings may be written (live data of this app, not forced demo / classic app running).</summary>
        public static bool CanWriteSettings { get; private set; }

        /// <summary>Raised after the theme changed (title bar button or settings page).</summary>
        public static event EventHandler ThemeChanged;
        public static AppInstance Instance { get; private set; }
        public static Shell.TrayIcon Tray { get; private set; }
        public static OverlayManager Overlays { get; private set; }

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

        /// <summary>Scroll the page down by this many units before the screenshot (--scroll 1600).</summary>
        public static double ScrollOffset { get; private set; }

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
            int scrollIdx = Array.FindIndex(args, a => a.Equals("--scroll", StringComparison.OrdinalIgnoreCase));
            if (scrollIdx >= 0 && scrollIdx + 1 < args.Length && double.TryParse(args[scrollIdx + 1], out double scroll)) ScrollOffset = scroll;

            bool demo = ScreenshotPath != null || args.Any(a => a.Equals("--demo", StringComparison.OrdinalIgnoreCase));
            bool forcedLight = args.Any(a => a.Equals("--light", StringComparison.OrdinalIgnoreCase));
            bool forcedDark = args.Any(a => a.Equals("--dark", StringComparison.OrdinalIgnoreCase));
            IsDark = forcedLight ? false : forcedDark ? true : demo || SettingsViewModel.ReadIsDark(AppSetup.Default);

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
            CanWriteSettings = Source is LiveTrackingSource || (Instance != null && Instance.OwnsClassicLock);
            Dashboard = new DashboardViewModel(Source, AppSetup.Default, CanWriteSettings && ScreenshotPath == null);
            Dashboard.SetTheme(IsDark);
            if (ScreenshotPath == null)
            {
                var tagSounds = new TagSoundNotifier(Source, new CoreTagSoundPlayer());
                Dashboard.LiveUpdated += (s, live) => tagSounds.Observe(live);
            }
            IReadOnlyList<LeagueInfo> leagues = Source.IsDemo
                ? LeagueCatalog.Demo(DateTime.Now)
                : LeagueCatalog.Load(TrX_Static.METADATA_XML_FULLPATH);
            Filter = new GlobalFilter(leagues);
            FilterBar = new FilterBarViewModel(Filter, Source);
            ApplyFilterArg(args);
            ActivityLog = new ActivityLogViewModel(Source, Filter, AppSetup.Default, CanWriteSettings && ScreenshotPath == null);
            Tags = new TagsViewModel(Source, ScreenshotPath == null ? new CoreTagSoundPlayer() : null);
            Statistics = new StatisticsViewModel(Source, Filter);
            Mapping = new MappingViewModel(Source, Filter);
            Heist = new HeistViewModel(Source, Filter);
            Lab = new LabViewModel(Source, Filter, AppSetup.Default, CanWriteSettings && ScreenshotPath == null);
            DivCards = new DivCardsViewModel(Source, Filter);
            Leagues = new LeaguesViewModel(Source, Filter);
            foreach (AnalyticsViewModel page in AnalyticsPages) page.SetTheme(IsDark);

            // Screenshots use the default overlay settings, not the ones of this machine
            AppSetup overlaySetup = ScreenshotPath == null ? AppSetup.Default
                : new AppSetup(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "traxile-screenshot-" + Guid.NewGuid().ToString("N")));
            Overlays = new OverlayManager(overlaySetup, Source, CanWriteSettings && ScreenshotPath == null, () => MainWindow);
            Dashboard.LiveUpdated += (s, live) => Overlays.Update(live);

            Settings = new SettingsViewModel(AppSetup.Default, Source, CanWriteSettings, IsDark, d => SetTheme(d, persist: false), Restart, Overlays);

            if (restore.HasRestored && !restore.HasFailed)
            {
                Dashboard.AddNotice("Backup restored", $"Your backup was applied ({restore.RestoredFiles.Count} files).");
            }
            else if (restore.HasFailed)
            {
                Dashboard.AddNotice("Restore failed", restore.Error, isError: true);
            }

            if (ScreenshotPath != null && StartPage != null && StartPage.StartsWith("overlay", StringComparison.OrdinalIgnoreCase))
            {
                ScreenshotOverlay(StartPage);
                return;
            }

            var window = new MainWindow();
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();

            if (ScreenshotPath == null)
            {
                Tray = new Shell.TrayIcon(window, Overlays);
                Tray.ExitRequested += (s, a) => Shutdown();
                Overlays.ShowDefaults(); // "show by default" overlays open with the app, like the classic one
            }
        }

        /// <summary>
        /// --filter last7days,map,tag:juiced,level>=83,league (for screenshots): comma-separated parts of the
        /// global filter: a <see cref="FilterTime"/> name, an activity type, "tag:&lt;id&gt;", "level&lt;op&gt;&lt;n&gt;",
        /// "area:&lt;name&gt;", "and" / "not" for the tag match, "league" = current league.
        /// </summary>
        private static void ApplyFilterArg(string[] args)
        {
            int idx = Array.FindIndex(args, a => a.Equals("--filter", StringComparison.OrdinalIgnoreCase));
            if (idx < 0 || idx + 1 >= args.Length) return;

            Filter.Batch(() =>
            {
                foreach (string raw in args[idx + 1].Split(','))
                {
                    string p = raw.Trim();
                    if (p.Equals("league", StringComparison.OrdinalIgnoreCase))
                    {
                        Filter.Time = FilterTime.League;
                        Filter.League = LeagueCatalog.Current(Filter.Leagues);
                    }
                    else if (p.StartsWith("tag:", StringComparison.OrdinalIgnoreCase)) Filter.SetTag(p.Substring(4), true);
                    else if (p.StartsWith("area:", StringComparison.OrdinalIgnoreCase)) Filter.Area = p.Substring(5);
                    else if (p.Equals("and", StringComparison.OrdinalIgnoreCase)) Filter.TagMatch = TagMatch.All;
                    else if (p.Equals("not", StringComparison.OrdinalIgnoreCase)) Filter.TagMatch = TagMatch.None;
                    else if (p.StartsWith("level", StringComparison.OrdinalIgnoreCase))
                    {
                        string rest = p.Substring(5);
                        string[] ops = { ">=", "<=", ">", "<", "=" };
                        LevelOperator[] values = { LevelOperator.GreaterOrEqual, LevelOperator.LessOrEqual, LevelOperator.Greater, LevelOperator.Less, LevelOperator.Equal };
                        for (int i = 0; i < ops.Length; i++)
                        {
                            if (rest.StartsWith(ops[i]) && int.TryParse(rest.Substring(ops[i].Length), out int lvl))
                            {
                                Filter.LevelOperator = values[i];
                                Filter.AreaLevel = lvl;
                                break;
                            }
                        }
                    }
                    else if (Enum.TryParse(p, true, out FilterTime time)) Filter.Time = time;
                    else if (Enum.TryParse(p, true, out ACTIVITY_TYPES type)) Filter.Type = type;
                }
            });
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

        /// <summary>
        /// --screenshot x.png --page overlay-stopwatch[-move] | overlay-tags[-move]: render an overlay with the demo live state
        /// (transparent around the overlay, like over the game).
        /// </summary>
        private void ScreenshotOverlay(string page)
        {
            OverlayWindow overlay = page.StartsWith("overlay-tags", StringComparison.OrdinalIgnoreCase)
                ? (OverlayWindow)Overlays.ShowTags() : Overlays.ShowStopwatch();
            overlay.IsMovable = page.EndsWith("-move", StringComparison.OrdinalIgnoreCase);
            Overlays.Update(Source.GetLiveState());
            overlay.Left = -30000;
            overlay.Top = -30000;
            Screenshot.Schedule(overlay, ScreenshotPath, TimeSpan.FromSeconds(1));
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
            FilterBar?.RefreshOptions();
            if (Statistics != null) foreach (AnalyticsViewModel page in AnalyticsPages) page.SetTheme(dark);
            Settings?.SyncTheme(dark);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>Switches the theme and (unless told otherwise) stores it in config.xml like the classic app.</summary>
        public static void SetTheme(bool dark, bool persist = true)
        {
            ApplyTheme(dark);
            if (persist && CanWriteSettings) AppSetup.Default.WriteSetting(SettingsViewModel.ThemeKey, dark ? "Dark" : "Light");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Dashboard?.Dispose();
            ActivityLog?.Dispose();
            FilterBar?.Dispose();
            if (Statistics != null) foreach (AnalyticsViewModel page in AnalyticsPages) page?.Dispose();
            Source?.Dispose();
            Loading?.Dispose();
            Tray?.Dispose();
            Overlays?.Dispose();
            Instance?.Dispose(); // last: the core is stopped, a restarted process may take over
            base.OnExit(e);
        }
    }
}
