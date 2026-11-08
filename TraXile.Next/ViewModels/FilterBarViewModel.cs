using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Entry of the time combo box: a fixed range or a league.</summary>
    public sealed class TimeOption
    {
        public string Label { get; set; }
        public FilterTime Time { get; set; }
        public LeagueInfo League { get; set; }
        public string Hint { get; set; }
        public override string ToString() => Label;
    }

    public sealed class NamedOption<T>
    {
        public string Label { get; set; }
        public T Value { get; set; }
        public override string ToString() => Label;
    }

    /// <summary>A tag in the filter's tag menu.</summary>
    public sealed class TagChoice : ObservableObject
    {
        private readonly GlobalFilter _filter;

        public TagChoice(GlobalFilter filter, TagInfo tag, int count)
        {
            _filter = filter;
            Tag = tag;
            Count = count;
        }

        public TagInfo Tag { get; }
        public int Count { get; }
        public string Name => Tag.Name;
        public Brush Background => Tag.Background;
        public Brush Foreground => Tag.Foreground;
        public string CountText => Count > 0 ? Count.ToString("N0") : "";

        public bool IsChecked
        {
            get => _filter.Tags.Contains(Tag.Id);
            set => _filter.SetTag(Tag.Id, value);
        }

        public void Sync() => OnPropertyChanged(nameof(IsChecked));
    }

    /// <summary>
    /// State of the shared filter bar (one instance for all filtered pages, so every bar shows the same filter).
    /// The options (types, areas, tags) come from the history; the filter itself lives in <see cref="GlobalFilter"/>.
    /// </summary>
    public sealed class FilterBarViewModel : ObservableObject, IDisposable
    {
        private readonly ITrackingSource _source;
        private readonly DispatcherTimer _refreshDebounce;
        private bool _syncing;

        private static readonly FilterOption AllTypes = new FilterOption { Label = "All types" };
        private static readonly FilterOption AllAreas = new FilterOption { Label = "All areas" };

        public FilterBarViewModel(GlobalFilter filter, ITrackingSource source)
        {
            Filter = filter;
            _source = source;

            ResetCommand = new RelayCommand(Reset);

            TimeOptions = BuildTimeOptions(filter.Leagues);
            LevelOperators = ((LevelOperator[])Enum.GetValues(typeof(LevelOperator)))
                .Select(o => new NamedOption<LevelOperator> { Label = GlobalFilter.OperatorSymbol(o), Value = o }).ToList();
            TagMatches = new[]
            {
                new NamedOption<TagMatch> { Label = "Any of them (OR)", Value = TagMatch.Any },
                new NamedOption<TagMatch> { Label = "All of them (AND)", Value = TagMatch.All },
                new NamedOption<TagMatch> { Label = "None of them (NOT)", Value = TagMatch.None },
            };

            _refreshDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _refreshDebounce.Tick += (s, e) => { _refreshDebounce.Stop(); RefreshOptions(); };
            _source.HistoryChanged += OnHistoryChanged;

            Filter.Changed += (s, e) => SyncFromFilter();
            RefreshOptions();
        }

        public GlobalFilter Filter { get; }
        public IRelayCommand ResetCommand { get; }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            _refreshDebounce.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshDebounce.Stop();
                _refreshDebounce.Start();
            }));
        }

        // ------------------------------------------------------------------
        // Time
        // ------------------------------------------------------------------

        public IReadOnlyList<TimeOption> TimeOptions { get; }

        private static IReadOnlyList<TimeOption> BuildTimeOptions(IReadOnlyList<LeagueInfo> leagues)
        {
            var list = new List<TimeOption>();
            foreach (FilterTime t in new[] { FilterTime.All, FilterTime.Today, FilterTime.Last24Hours, FilterTime.Last7Days,
                                             FilterTime.Last30Days, FilterTime.LastYear, FilterTime.Custom })
            {
                list.Add(new TimeOption { Label = GlobalFilter.TimeLabel(t), Time = t });
            }
            // newest league first, like the classic list
            foreach (LeagueInfo l in leagues.OrderByDescending(l => l.Start))
            {
                list.Add(new TimeOption
                {
                    Label = l.IsCurrent ? $"Current league: {l.Name}" : $"League: {l.Name}",
                    Hint = l.Version,
                    Time = FilterTime.League,
                    League = l
                });
            }
            return list;
        }

        public TimeOption SelectedTime
        {
            get => TimeOptions.FirstOrDefault(o => o.Time == Filter.Time && (o.Time != FilterTime.League || o.League == Filter.League))
                   ?? TimeOptions[0];
            set
            {
                if (value == null || _syncing) return;
                Filter.Batch(() =>
                {
                    Filter.Time = value.Time;
                    Filter.League = value.League;
                });
            }
        }

        public bool IsCustomTime => Filter.Time == FilterTime.Custom;

        public DateTime? CustomFromDate
        {
            get => Filter.CustomFrom.Date;
            set { if (value.HasValue) Filter.CustomFrom = value.Value.Date + Filter.CustomFrom.TimeOfDay; }
        }

        public DateTime? CustomToDate
        {
            get => Filter.CustomTo.Date;
            set { if (value.HasValue) Filter.CustomTo = value.Value.Date + Filter.CustomTo.TimeOfDay; }
        }

        private string _customFromTime;
        public string CustomFromTime
        {
            get => _customFromTime ?? Filter.CustomFrom.ToString("HH:mm", CultureInfo.InvariantCulture);
            set
            {
                _customFromTime = value;
                if (TryParseTime(value, out TimeSpan t)) Filter.CustomFrom = Filter.CustomFrom.Date + t;
                OnPropertyChanged(nameof(CustomFromTimeInvalid));
            }
        }

        private string _customToTime;
        public string CustomToTime
        {
            get => _customToTime ?? Filter.CustomTo.ToString("HH:mm", CultureInfo.InvariantCulture);
            set
            {
                _customToTime = value;
                if (TryParseTime(value, out TimeSpan t)) Filter.CustomTo = Filter.CustomTo.Date + t;
                OnPropertyChanged(nameof(CustomToTimeInvalid));
            }
        }

        public bool CustomFromTimeInvalid => _customFromTime != null && !TryParseTime(_customFromTime, out _);
        public bool CustomToTimeInvalid => _customToTime != null && !TryParseTime(_customToTime, out _);

        /// <summary>"HH:mm" or "HH:mm:ss" within one day.</summary>
        public static bool TryParseTime(string s, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(s)) return false;
            string[] formats = { @"h\:mm", @"hh\:mm", @"h\:mm\:ss", @"hh\:mm\:ss" };
            return TimeSpan.TryParseExact(s.Trim(), formats, CultureInfo.InvariantCulture, out time)
                   && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
        }

        // ------------------------------------------------------------------
        // Type, area, level
        // ------------------------------------------------------------------

        private IReadOnlyList<FilterOption> _typeOptions = new[] { AllTypes };
        public IReadOnlyList<FilterOption> TypeOptions { get => _typeOptions; private set => SetProperty(ref _typeOptions, value); }

        public FilterOption SelectedType
        {
            get => TypeOptions.FirstOrDefault(o => o.Value is ACTIVITY_TYPES t && Filter.Type == t) ?? AllTypes;
            set { if (!_syncing) Filter.Type = value?.Value as ACTIVITY_TYPES?; }
        }

        private IReadOnlyList<FilterOption> _areaOptions = new[] { AllAreas };
        public IReadOnlyList<FilterOption> AreaOptions { get => _areaOptions; private set => SetProperty(ref _areaOptions, value); }

        public FilterOption SelectedArea
        {
            get => AreaOptions.FirstOrDefault(o => o.Value is string s && string.Equals(s, Filter.Area, StringComparison.OrdinalIgnoreCase)) ?? AllAreas;
            set { if (!_syncing) Filter.Area = value?.Value as string; }
        }

        public IReadOnlyList<NamedOption<LevelOperator>> LevelOperators { get; }

        public NamedOption<LevelOperator> SelectedLevelOperator
        {
            get => LevelOperators.First(o => o.Value == Filter.LevelOperator);
            set { if (value != null && !_syncing) Filter.LevelOperator = value.Value; }
        }

        private string _levelText;
        /// <summary>Area level as typed; empty = no level filter.</summary>
        public string LevelText
        {
            get => _levelText ?? Filter.AreaLevel?.ToString(CultureInfo.InvariantCulture) ?? "";
            set
            {
                _levelText = value;
                if (string.IsNullOrWhiteSpace(value)) Filter.AreaLevel = null;
                else if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int lvl) && lvl >= 0) Filter.AreaLevel = lvl;
                OnPropertyChanged(nameof(LevelInvalid));
            }
        }

        public bool LevelInvalid => !string.IsNullOrWhiteSpace(_levelText) &&
            !(int.TryParse(_levelText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int l) && l >= 0);

        // ------------------------------------------------------------------
        // Tags
        // ------------------------------------------------------------------

        private IReadOnlyList<TagChoice> _tagChoices = Array.Empty<TagChoice>();
        public IReadOnlyList<TagChoice> TagChoices { get => _tagChoices; private set => SetProperty(ref _tagChoices, value); }

        public IReadOnlyList<NamedOption<TagMatch>> TagMatches { get; }

        public NamedOption<TagMatch> SelectedTagMatch
        {
            get => TagMatches.First(o => o.Value == Filter.TagMatch);
            set { if (value != null && !_syncing) Filter.TagMatch = value.Value; }
        }

        public bool HasTagFilter => Filter.Tags.Count > 0;

        public string TagButtonText
        {
            get
            {
                if (Filter.Tags.Count == 0) return "All tags";
                if (Filter.Tags.Count == 1) return Filter.TagName(Filter.Tags[0]);
                return $"{Filter.Tags.Count} tags";
            }
        }

        // ------------------------------------------------------------------
        // State
        // ------------------------------------------------------------------

        public void Reset()
        {
            _levelText = null;
            _customFromTime = null;
            _customToTime = null;
            Filter.Reset();
            SyncFromFilter(); // also when the filter itself was already empty (clears invalid texts)
        }

        public bool IsActive => Filter.IsActive;
        public string Description => Filter.Description;

        /// <summary>Re-reads types, areas and tags from the history (also after a theme switch: new brushes).</summary>
        public void RefreshOptions()
        {
            IReadOnlyList<ActivityInfo> all = _source.GetHistory();
            IReadOnlyList<TagInfo> tags = _source.GetTags();

            var typeList = new List<FilterOption> { AllTypes };
            typeList.AddRange(all.GroupBy(a => a.Type)
                .OrderByDescending(g => g.Count())
                .Select(g => new FilterOption
                {
                    Label = ActivityPalette.DisplayName(g.Key),
                    Value = g.Key,
                    Brush = ActivityPalette.Brush(g.Key),
                    Count = g.Count()
                }));

            var areaList = new List<FilterOption> { AllAreas };
            areaList.AddRange(all.Where(a => !string.IsNullOrEmpty(a.Area))
                .GroupBy(a => a.Area, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new FilterOption { Label = g.Key, Value = g.Key, Count = g.Count() }));

            var tagCounts = all.SelectMany(a => a.Tags).GroupBy(t => t.Id).ToDictionary(g => g.Key, g => g.Count());
            var names = tags.ToDictionary(t => t.Id, t => t.Name);
            Filter.TagName = id => names.TryGetValue(id, out string n) ? n : id;
            TagChoices = tags
                .Select(t => new TagChoice(Filter, t, tagCounts.TryGetValue(t.Id, out int c) ? c : 0))
                .OrderByDescending(t => t.Count).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _syncing = true;
            try
            {
                TypeOptions = typeList;
                AreaOptions = areaList;
            }
            finally { _syncing = false; }
            SyncFromFilter();
        }

        private void SyncFromFilter()
        {
            // Typed texts stay as typed (also while invalid) unless the filter now says something else
            if (!Filter.AreaLevel.HasValue && !LevelInvalid) _levelText = null;
            if (Filter.AreaLevel.HasValue && (!int.TryParse(_levelText, out int typed) || typed != Filter.AreaLevel.Value)) _levelText = null;
            if (Filter.Time != FilterTime.Custom) { _customFromTime = null; _customToTime = null; }

            _syncing = true;
            try
            {
                OnPropertyChanged(nameof(SelectedTime));
                OnPropertyChanged(nameof(IsCustomTime));
                OnPropertyChanged(nameof(CustomFromDate));
                OnPropertyChanged(nameof(CustomToDate));
                OnPropertyChanged(nameof(CustomFromTime));
                OnPropertyChanged(nameof(CustomToTime));
                OnPropertyChanged(nameof(SelectedType));
                OnPropertyChanged(nameof(SelectedArea));
                OnPropertyChanged(nameof(SelectedLevelOperator));
                OnPropertyChanged(nameof(LevelText));
                OnPropertyChanged(nameof(LevelInvalid));
                OnPropertyChanged(nameof(SelectedTagMatch));
                OnPropertyChanged(nameof(HasTagFilter));
                OnPropertyChanged(nameof(TagButtonText));
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(Description));
                foreach (TagChoice t in TagChoices) t.Sync();
            }
            finally { _syncing = false; }
        }

        public void Dispose()
        {
            _source.HistoryChanged -= OnHistoryChanged;
            _refreshDebounce.Stop();
        }
    }
}
