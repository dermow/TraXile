using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>One lab difficulty on the lab page.</summary>
    public sealed class LabRow
    {
        public string Name { get; set; }
        /// <summary>Short name for chart labels ("Merciless", "Uber-Lab", "Unknown*").</summary>
        public string ShortName { get; set; }
        public int Count { get; set; }
        public double AverageSeconds { get; set; }
        public ActivityInfo Best { get; set; }

        public string Average => Count > 0 ? ActivityInfo.FormatDuration((int)AverageSeconds) : "–";
        public string BestTime => Best?.Duration ?? "–";
        public string BestDate => Best?.Started.ToString("yyyy-MM-dd HH:mm") ?? "–";
    }

    /// <summary>
    /// Lab page (classic Main.RenderLabDashboard) on the global filter: labs done and average time per lab, best run
    /// per lab and the list of lab activities. Like the classic dashboard only runs without deaths count, and times
    /// are capped at the labyrinth time cap.
    /// </summary>
    public sealed class LabViewModel : AnalyticsViewModel
    {
        /// <summary>Classic setting: leave out runs whose lab is unknown.</summary>
        public const string HideUnknownKey = "dashboard_lab_hide_unknown";

        /// <summary>The lab names of the core, in classic order.</summary>
        public static readonly string[] Labs =
        {
            "Unknown", "The Labyrinth", "The Merciless Labyrinth", "The Cruel Labyrinth", "Uber-Lab", "Advanced Uber-Lab"
        };

        private readonly AppSetup _settings;
        private readonly bool _canWriteSettings;

        public LabViewModel(ITrackingSource source, GlobalFilter filter, AppSetup settings = null, bool canWriteSettings = false)
            : base(source, filter)
        {
            _settings = settings;
            _canWriteSettings = canWriteSettings;
            _hideUnknown = bool.TryParse(settings?.ReadSetting(HideUnknownKey, "false"), out bool hide) && hide;
            Refresh();
        }

        private bool _hideUnknown;
        public bool HideUnknown
        {
            get => _hideUnknown;
            set
            {
                if (!SetProperty(ref _hideUnknown, value)) return;
                if (_canWriteSettings) _settings?.WriteSetting(HideUnknownKey, value.ToString());
                Refresh();
            }
        }

        private IReadOnlyList<LabRow> _rows = Array.Empty<LabRow>();
        /// <summary>One row per lab (classic order), counting deathless runs only.</summary>
        public IReadOnlyList<LabRow> Rows { get => _rows; private set => SetProperty(ref _rows, value); }

        private IReadOnlyList<ActivityInfo> _runs = Array.Empty<ActivityInfo>();
        /// <summary>All lab activities in the filter (also those with deaths), newest first.</summary>
        public IReadOnlyList<ActivityInfo> Runs { get => _runs; private set => SetProperty(ref _runs, value); }

        public bool HasData => Runs.Count > 0;

        private string _summary = "";
        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        private ISeries[] _countSeries = Array.Empty<ISeries>();
        public ISeries[] CountSeries { get => _countSeries; private set => SetProperty(ref _countSeries, value); }

        private ISeries[] _timeSeries = Array.Empty<ISeries>();
        public ISeries[] TimeSeries { get => _timeSeries; private set => SetProperty(ref _timeSeries, value); }

        private Axis[] _countXAxes = { new Axis() };
        public Axis[] CountXAxes { get => _countXAxes; private set => SetProperty(ref _countXAxes, value); }

        private Axis[] _timeXAxes = { new Axis() };
        public Axis[] TimeXAxes { get => _timeXAxes; private set => SetProperty(ref _timeXAxes, value); }

        private Axis[] _countYAxes = { new Axis() };
        public Axis[] CountYAxes { get => _countYAxes; private set => SetProperty(ref _countYAxes, value); }

        private Axis[] _timeYAxes = { new Axis() };
        public Axis[] TimeYAxes { get => _timeYAxes; private set => SetProperty(ref _timeYAxes, value); }

        public static string ShortName(string lab)
        {
            if (lab == "Unknown") return "Unknown*";
            if (lab == "The Labyrinth") return lab;
            if (lab == "Advanced Uber-Lab") return "Adv. Uber-Lab"; // keeps the chart labels apart
            return lab.Replace("The ", "").Replace(" Labyrinth", "");
        }

        public override void Refresh()
        {
            List<ActivityInfo> runs = FilteredHistory().Where(a => a.Type == ACTIVITY_TYPES.LABYRINTH).ToList();
            if (HideUnknown) runs = runs.Where(a => a.Area != "Unknown").ToList();
            int cap = Source.GetTimeCap(ACTIVITY_TYPES.LABYRINTH);
            int Capped(ActivityInfo a) => cap > 0 && a.TotalSeconds > cap ? cap : a.TotalSeconds;

            Rows = Labs.Where(l => !(HideUnknown && l == "Unknown"))
                .Select(l =>
                {
                    List<ActivityInfo> clean = runs.Where(a => a.Area == l && a.Deaths == 0).ToList();
                    return new LabRow
                    {
                        Name = l,
                        ShortName = ShortName(l),
                        Count = clean.Count,
                        AverageSeconds = clean.Count > 0 ? clean.Average(Capped) : 0,
                        Best = clean.OrderBy(a => a.TotalSeconds).FirstOrDefault()
                    };
                }).ToList();
            Runs = runs;

            int clean_ = Rows.Sum(r => r.Count);
            Summary = runs.Count == 0 ? "No lab runs in the filtered activities."
                : $"{runs.Count:N0} lab runs · {clean_:N0} without deaths";
            OnPropertyChanged(nameof(HasData));
            BuildCharts();
        }

        private void BuildCharts()
        {
            SkiaSharp.SKColor color = ActivityPalette.Sk(ACTIVITY_TYPES.LABYRINTH);
            string[] labels = Rows.Select(r => r.ShortName).ToArray();
            CountSeries = new[] { Columns("Labs done", Rows.Select(r => (double)r.Count), color, v => $"{v:N0} labs") };
            TimeSeries = new[] { Columns("Average time", Rows.Select(r => Math.Round(r.AverageSeconds / 60.0, 1)), color, v => $"{v:0.0} min") };
            CountXAxes = new[] { CategoryAxis(labels) };
            TimeXAxes = new[] { CategoryAxis(labels) };
            CountYAxes = new[] { ValueAxis() };
            TimeYAxes = new[] { ValueAxis(v => $"{v:0.#} min") };
        }

        protected override void OnThemeChanged() => BuildCharts();
    }
}
