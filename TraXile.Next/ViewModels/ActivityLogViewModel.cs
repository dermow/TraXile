using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Entry of a filter combo box ("All types", "Map", ...).</summary>
    public sealed class FilterOption
    {
        public string Label { get; set; }
        public object Value { get; set; }
        public Brush Brush { get; set; }
        public int Count { get; set; }
        public string CountText => Count > 0 ? Count.ToString("N0") : "";
        public override string ToString() => Label;
    }

    /// <summary>
    /// Activity log: full history with search, the global filter, details, tagging, export and delete.
    /// Kept alive for the whole app session so the search survives page navigation.
    /// </summary>
    public sealed class ActivityLogViewModel : ObservableObject, IDisposable
    {
        private readonly ITrackingSource _source;
        private readonly DispatcherTimer _refreshDebounce;
        private readonly DispatcherTimer _searchDebounce;
        private IReadOnlyList<ActivityInfo> _all = Array.Empty<ActivityInfo>();
        private IReadOnlyList<TagInfo> _allTags = Array.Empty<TagInfo>();
        private HashSet<string> _selectedIds = new HashSet<string>();

        /// <summary>Classic setting: how many of the newest activities the list shows without a search.</summary>
        public const string MaxItemsKey = "actlog.maxitems";

        /// <summary>Stored for "all" (a number, so the classic app can still read it).</summary>
        public const int AllItems = 1000000;

        private readonly AppSetup _settings;
        private readonly bool _canWriteSettings;

        /// <param name="filter">The global filter; a private, empty one if null (tests).</param>
        /// <param name="settings">config.xml for <see cref="MaxItemsKey"/>; classic default (500) if null.</param>
        /// <param name="canWriteSettings">False while the classic app owns the data: the choice is not stored.</param>
        public ActivityLogViewModel(ITrackingSource source, GlobalFilter filter = null, AppSetup settings = null, bool canWriteSettings = false)
        {
            _source = source;
            _settings = settings;
            _canWriteSettings = canWriteSettings;
            Filter = filter ?? new GlobalFilter();
            Filter.Changed += OnFilterChanged;

            MaxItemsOptions = new[] { 50, 100, 500, 1000, 1500, AllItems }
                .Select(n => new NamedOption<int> { Label = n == AllItems ? "All" : $"Newest {n:N0}", Value = n })
                .ToList();
            int stored = int.TryParse(settings?.ReadSetting(MaxItemsKey, "500"), out int m) && m > 0 ? m : 500;
            _maxItems = stored >= AllItems ? AllItems : stored;

            ClearFiltersCommand = new RelayCommand(ClearFilters);
            AddTagCommand = new RelayCommand<TagInfo>(AddTagToSelection);
            RemoveTagCommand = new RelayCommand<TagInfo>(RemoveTagFromSelected);

            _refreshDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _refreshDebounce.Tick += (s, e) => { _refreshDebounce.Stop(); Refresh(); };

            _searchDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            _searchDebounce.Tick += (s, e) => { _searchDebounce.Stop(); ApplyFilter(); };

            _source.HistoryChanged += OnHistoryChanged;
            Refresh();
        }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            _refreshDebounce.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshDebounce.Stop();
                _refreshDebounce.Start();
            }));
        }

        // ------------------------------------------------------------------
        // Commands
        // ------------------------------------------------------------------

        public IRelayCommand ClearFiltersCommand { get; }
        public IRelayCommand<TagInfo> AddTagCommand { get; }
        public IRelayCommand<TagInfo> RemoveTagCommand { get; }

        // ------------------------------------------------------------------
        // Filters
        // ------------------------------------------------------------------

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value ?? ""))
                {
                    _searchDebounce.Stop();
                    _searchDebounce.Start();
                }
            }
        }

        /// <summary>The global filter (time, type, area, level, tags) shared with the other analytics pages.</summary>
        public GlobalFilter Filter { get; }

        private void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchText) || Filter.IsActive;

        public IReadOnlyList<NamedOption<int>> MaxItemsOptions { get; }

        private int _maxItems;
        /// <summary>
        /// Without a search the list shows only the newest <see cref="MaxItems"/> matches; a search looks at all
        /// of them (classic DoSearch).
        /// </summary>
        public int MaxItems
        {
            get => _maxItems;
            set
            {
                if (value <= 0 || !SetProperty(ref _maxItems, value)) return;
                OnPropertyChanged(nameof(SelectedMaxItems));
                if (_canWriteSettings) _settings?.WriteSetting(MaxItemsKey, value.ToString(CultureInfo.InvariantCulture));
                ApplyFilter();
            }
        }

        /// <summary>The option of <see cref="MaxItems"/>; a stored value that is not in the list shows as custom entry.</summary>
        public NamedOption<int> SelectedMaxItems
        {
            get => MaxItemsOptions.FirstOrDefault(o => o.Value == MaxItems)
                   ?? new NamedOption<int> { Label = $"Newest {MaxItems:N0}", Value = MaxItems };
            set { if (value != null) MaxItems = value.Value; }
        }

        /// <summary>Clears the search and resets the global filter.</summary>
        private void ClearFilters()
        {
            _searchText = "";
            OnPropertyChanged(nameof(SearchText));
            if (Filter.IsActive) Filter.Reset(); // applies via Changed
            else ApplyFilter();
        }

        // ------------------------------------------------------------------
        // Result list
        // ------------------------------------------------------------------

        private IReadOnlyList<ActivityInfo> _items = Array.Empty<ActivityInfo>();
        /// <summary>Filtered activities, newest first.</summary>
        public IReadOnlyList<ActivityInfo> Items { get => _items; private set => SetProperty(ref _items, value); }

        private string _summary = "";
        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        /// <summary>All activities that match filter and search, also those beyond <see cref="MaxItems"/>.</summary>
        public IReadOnlyList<ActivityInfo> MatchingItems { get; private set; } = Array.Empty<ActivityInfo>();

        private bool _isLimited;
        /// <summary>More activities match than <see cref="MaxItems"/> allows (only without a search).</summary>
        public bool IsLimited { get => _isLimited; private set => SetProperty(ref _isLimited, value); }

        public bool IsEmpty => Items.Count == 0;
        public string EmptyText => _all.Count == 0
            ? "No activities tracked yet. Start playing and they will show up here."
            : "No activities match your filters.";

        /// <summary>Raised after <see cref="Items"/> was replaced, so the view can restore its selection.</summary>
        public event EventHandler ItemsRefreshed;

        public void Refresh()
        {
            _all = _source.GetHistory();
            _allTags = _source.GetTags();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<ActivityInfo> q = Filter.Apply(_all);

            string[] terms = SearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length > 0) q = q.Where(a => terms.All(term => Matches(a, term)));

            List<ActivityInfo> matched = q.ToList();
            bool limited = terms.Length == 0 && matched.Count > MaxItems;
            List<ActivityInfo> list = limited ? matched.Take(MaxItems).ToList() : matched; // newest first
            MatchingItems = matched;
            Items = list;

            int totalSec = matched.Sum(a => a.TotalSeconds);
            string hours = $"{totalSec / 3600.0:N1} h";
            if (limited)
            {
                Summary = matched.Count == _all.Count
                    ? $"Newest {list.Count:N0} of {matched.Count:N0} activities · {hours} tracked"
                    : $"Newest {list.Count:N0} of {matched.Count:N0} matching activities · {hours}";
            }
            else
            {
                Summary = matched.Count == _all.Count
                    ? $"{matched.Count:N0} activities · {hours} tracked"
                    : $"{matched.Count:N0} of {_all.Count:N0} activities · {hours}";
            }
            IsLimited = limited;

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(EmptyText));
            OnPropertyChanged(nameof(HasActiveFilters));

            // Drop selected entries that are no longer visible
            var visible = new HashSet<string>(list.Select(a => a.Id));
            _selectedIds.IntersectWith(visible);
            UpdateSelectionState();

            ItemsRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private static bool Matches(ActivityInfo a, string term)
        {
            bool Has(string s) => s != null && s.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
            return Has(a.Area) || Has(a.TypeName) || Has(a.LevelText) || a.Tags.Any(t => Has(t.Name));
        }

        // ------------------------------------------------------------------
        // Selection & details
        // ------------------------------------------------------------------

        public IReadOnlyCollection<string> SelectedIds => _selectedIds;

        /// <summary>Called by the view when the grid selection changed.</summary>
        public void SetSelection(IEnumerable<ActivityInfo> selected)
        {
            _selectedIds = new HashSet<string>(selected.Where(a => a != null).Select(a => a.Id));
            UpdateSelectionState();
        }

        private ActivityInfo _selectedActivity;
        /// <summary>The single selected activity (null if none or several are selected).</summary>
        public ActivityInfo SelectedActivity { get => _selectedActivity; private set => SetProperty(ref _selectedActivity, value); }

        public int SelectedCount => _selectedIds.Count;
        public bool HasSelection => SelectedCount > 0;
        public bool HasSingleSelection => SelectedCount == 1;
        public bool HasMultiSelection => SelectedCount > 1;
        public string SelectionText => $"{SelectedCount:N0} activities selected";

        private SelectionSummary _selectionSummary = SelectionSummary.Empty;
        /// <summary>Count, times, types and tags of the selected activities (classic SummaryForm).</summary>
        public SelectionSummary SelectionSummary { get => _selectionSummary; private set => SetProperty(ref _selectionSummary, value); }

        private IReadOnlyList<TagInfo> _availableTags = Array.Empty<TagInfo>();
        /// <summary>Custom tags that can still be added to the selection.</summary>
        public IReadOnlyList<TagInfo> AvailableTags { get => _availableTags; private set => SetProperty(ref _availableTags, value); }

        public bool HasAvailableTags => AvailableTags.Count > 0;

        private void UpdateSelectionState()
        {
            ActivityInfo[] selected = Items.Where(a => _selectedIds.Contains(a.Id)).ToArray();
            SelectedActivity = selected.Length == 1 ? selected[0] : null;
            SelectionSummary = selected.Length > 1 ? SelectionSummary.Create(selected, _source.GetTimeCap) : SelectionSummary.Empty;

            // A tag is offered as long as at least one selected activity does not have it yet
            AvailableTags = _allTags
                .Where(t => !t.IsDefault && selected.Any(a => a.Tags.All(x => x.Id != t.Id)))
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(HasSingleSelection));
            OnPropertyChanged(nameof(HasMultiSelection));
            OnPropertyChanged(nameof(SelectionText));
            OnPropertyChanged(nameof(HasAvailableTags));
        }

        // ------------------------------------------------------------------
        // Editing
        // ------------------------------------------------------------------

        private void AddTagToSelection(TagInfo tag)
        {
            if (tag == null || tag.IsDefault) return;
            foreach (string id in _selectedIds.ToArray()) _source.AddTag(id, tag.Id);
            Refresh();
        }

        private void RemoveTagFromSelected(TagInfo tag)
        {
            if (tag == null || tag.IsDefault || SelectedActivity == null) return;
            _source.RemoveTag(SelectedActivity.Id, tag.Id);
            Refresh();
        }

        public void DeleteSelected()
        {
            if (_selectedIds.Count == 0) return;
            _source.DeleteActivities(_selectedIds.ToArray());
            _selectedIds.Clear();
            Refresh();
        }

        /// <summary>
        /// Export activities as CSV in the same format as the classic TraXile.
        /// </summary>
        public static void ExportCsv(string path, IEnumerable<ActivityInfo> activities)
        {
            var sb = new StringBuilder();
            sb.AppendLine("time; type; area; area_level; stopwatch (seconds); death_counter; tags");
            foreach (ActivityInfo a in activities)
            {
                sb.Append(a.Started.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(';')
                  .Append(a.Type).Append(';')
                  .Append(a.Area?.Replace(";", ",")).Append(';')
                  .Append(a.AreaLevel).Append(';')
                  .Append(a.TotalSeconds).Append(';')
                  .Append(a.Deaths).Append(';')
                  .Append(string.Join(",", a.Tags.Select(t => t.Id)))
                  .AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// Export activities as JSON exactly like the classic TraXile (Main.WriteActivitiesToJSON): the core's
        /// <see cref="TrX_BackendSync_ActivityDocument"/> per activity, indented by Newtonsoft.Json, UTF-8 without BOM.
        /// </summary>
        public static void ExportJson(string path, IEnumerable<ActivityInfo> activities)
        {
            List<TrX_BackendSync_ActivityDocument> docs = activities.Select(a => new TrX_BackendSync_ActivityDocument
            {
                AreaName = a.AreaRaw,
                AreaLevel = a.AreaLevel,
                DurationSec = a.TotalSeconds,
                Tags = a.TagIds.ToList(),
                ActivityType = a.Type.ToString(),
                Identifier = a.Id,
                DeathCounter = a.Deaths,
                StartTime = a.Started
            }).ToList();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(docs, Newtonsoft.Json.Formatting.Indented));
            }
        }

        /// <summary>The whole history (classic export "All entries").</summary>
        public IReadOnlyList<ActivityInfo> AllActivities => _all;

        public IEnumerable<ActivityInfo> SelectedItems => Items.Where(a => _selectedIds.Contains(a.Id));

        public void Dispose()
        {
            _source.HistoryChanged -= OnHistoryChanged;
            Filter.Changed -= OnFilterChanged;
            _refreshDebounce.Stop();
            _searchDebounce.Stop();
        }
    }
}
