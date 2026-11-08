using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>A row of a "count by ..." list (maps, areas, tags): label or tag chip, count and a bar relative to the top row.</summary>
    public sealed class CountRow
    {
        public string Label { get; set; }
        /// <summary>Shown as chip instead of <see cref="Label"/> when set.</summary>
        public TagInfo Tag { get; set; }
        public int Count { get; set; }
        /// <summary>Count relative to the largest row, 0..1 (bar length).</summary>
        public double Fraction { get; set; }
        /// <summary>Optional second value, e.g. the average time.</summary>
        public string Detail { get; set; }

        public bool HasTag => Tag != null;
        public string CountText => Count.ToString("N0");

        /// <summary>Rows sorted by count (then label), with bars relative to the largest count; zero counts are left out.</summary>
        public static List<CountRow> Build(IEnumerable<CountRow> rows)
        {
            List<CountRow> list = rows.Where(r => r.Count > 0)
                .OrderByDescending(r => r.Count).ThenBy(r => r.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();
            int max = list.Count > 0 ? list[0].Count : 0;
            foreach (CountRow r in list) r.Fraction = max > 0 ? (double)r.Count / max : 0;
            return list;
        }

        /// <summary>How often each known tag occurs on the activities (classic "count by tag").</summary>
        public static List<CountRow> ByTag(IEnumerable<ActivityInfo> activities) =>
            Build(activities.SelectMany(a => a.Tags.GroupBy(t => t.Id).Select(g => g.First()))
                .GroupBy(t => t.Id)
                .Select(g => new CountRow { Label = g.First().Name, Tag = g.First(), Count = g.Count() }));
    }

    /// <summary>
    /// Base of the analytics pages (statistics, mapping, lab, heist, div cards): rebuilds when the history or the
    /// global filter changes and provides chart colors from the design tokens of the current theme.
    /// </summary>
    public abstract class AnalyticsViewModel : ObservableObject, IDisposable
    {
        private readonly DispatcherTimer _refreshDebounce;
        private bool _isDark = true;

        protected AnalyticsViewModel(ITrackingSource source, GlobalFilter filter)
        {
            Source = source;
            Filter = filter ?? new GlobalFilter();

            _refreshDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _refreshDebounce.Tick += (s, e) => { _refreshDebounce.Stop(); Refresh(); };

            Source.HistoryChanged += OnHistoryChanged;
            Filter.Changed += OnFilterChanged;
        }

        protected ITrackingSource Source { get; }
        public GlobalFilter Filter { get; }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            _refreshDebounce.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshDebounce.Stop();
                _refreshDebounce.Start();
            }));
        }

        private void OnFilterChanged(object sender, EventArgs e) => Refresh();

        /// <summary>Re-reads the data and rebuilds everything.</summary>
        public abstract void Refresh();

        /// <summary>The activities that pass the global filter, newest first.</summary>
        protected List<ActivityInfo> FilteredHistory() => Filter.Apply(Source.GetHistory());

        // ------------------------------------------------------------------
        // Theme
        // ------------------------------------------------------------------

        public bool IsDark => _isDark;

        public void SetTheme(bool dark)
        {
            if (_isDark == dark && _themeSet) return;
            _isDark = dark;
            _themeSet = true;
            OnThemeChanged();
        }

        private bool _themeSet;

        /// <summary>Chart paints are created per build: rebuild them (default: everything).</summary>
        protected virtual void OnThemeChanged() => Refresh();

        protected SKColor Token(string key) => ActivityPalette.Sk(DesignTokens.Get(key).For(_isDark));
        protected SKColor TextMuted => Token("TextMuted");
        protected SKColor GridColor => Token("Border");
        protected SKColor AccentColor => Token("Accent");

        /// <summary>Category axis with muted labels and no grid lines (bar charts).</summary>
        protected Axis CategoryAxis(IList<string> labels) => new Axis
        {
            Labels = labels,
            LabelsPaint = new SolidColorPaint(TextMuted),
            TextSize = 11,
            SeparatorsPaint = null,
            TicksPaint = null,
            MinStep = 1,
            ForceStepToMin = labels.Count <= 31,
        };

        /// <summary>Value axis starting at 0 with token grid lines.</summary>
        protected Axis ValueAxis(Func<double, string> labeler = null) => new Axis
        {
            MinLimit = 0,
            LabelsPaint = new SolidColorPaint(TextMuted),
            TextSize = 11,
            SeparatorsPaint = new SolidColorPaint(GridColor, 1),
            Labeler = labeler ?? (v => v.ToString("N0")),
        };

        /// <summary>Simple one-color column series (2 px corners like the design system).</summary>
        protected ISeries Columns(string name, IEnumerable<double> values, SKColor color, Func<double, string> tooltip = null) =>
            new ColumnSeries<double>
            {
                Name = name,
                Values = values.ToArray(),
                Fill = new SolidColorPaint(color),
                Stroke = null,
                Rx = 2,
                Ry = 2,
                MaxBarWidth = 34,
                Padding = 4,
                YToolTipLabelFormatter = p => tooltip != null ? tooltip(p.Coordinate.PrimaryValue) : p.Coordinate.PrimaryValue.ToString("N0"),
            };

        public virtual void Dispose()
        {
            Source.HistoryChanged -= OnHistoryChanged;
            Filter.Changed -= OnFilterChanged;
            _refreshDebounce.Stop();
        }
    }
}
