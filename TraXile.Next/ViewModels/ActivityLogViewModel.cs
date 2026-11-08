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

        /// <param name="filter">The global filter; a private, empty one if null (tests).</param>
        public ActivityLogViewModel(ITrackingSource source, GlobalFilter filter = null)
        {
            _source = source;
            Filter = filter ?? new GlobalFilter();
            Filter.Changed += OnFilterChanged;

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

            List<ActivityInfo> list = q.ToList();
            Items = list;

            int totalSec = list.Sum(a => a.TotalSeconds);
            Summary = list.Count == _all.Count
                ? $"{list.Count:N0} activities · {totalSec / 3600.0:N1} h tracked"
                : $"{list.Count:N0} of {_all.Count:N0} activities · {totalSec / 3600.0:N1} h";

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

        public string SelectionDuration =>
            ActivityInfo.FormatDuration(Items.Where(a => _selectedIds.Contains(a.Id)).Sum(a => a.TotalSeconds));

        private IReadOnlyList<TagInfo> _availableTags = Array.Empty<TagInfo>();
        /// <summary>Custom tags that can still be added to the selection.</summary>
        public IReadOnlyList<TagInfo> AvailableTags { get => _availableTags; private set => SetProperty(ref _availableTags, value); }

        public bool HasAvailableTags => AvailableTags.Count > 0;

        private void UpdateSelectionState()
        {
            ActivityInfo[] selected = Items.Where(a => _selectedIds.Contains(a.Id)).ToArray();
            SelectedActivity = selected.Length == 1 ? selected[0] : null;

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
            OnPropertyChanged(nameof(SelectionDuration));
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
