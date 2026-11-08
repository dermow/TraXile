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
