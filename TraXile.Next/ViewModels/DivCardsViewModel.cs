using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>The sortable columns of the Div Cards list.</summary>
    public enum DivCardSortColumn { Name, Count, LastDraw, FirstDraw }

    /// <summary>
    /// Div Cards page (classic Main.RenderDivCards): every divination card drawn in the filter's time range with
    /// count, last and first draw, a search and the CSV export. Only the time range of the global filter applies:
    /// the core records cards without the activity they dropped in.
    /// </summary>
    public sealed class DivCardsViewModel : AnalyticsViewModel
    {
        private readonly IDivCardSource _cards;
        private readonly Func<DateTime> _now;
        private readonly DispatcherTimer _drawDebounce;
        private IReadOnlyList<DivCardInfo> _all = Array.Empty<DivCardInfo>();

        /// <summary>How long after a draw the list is re-read (draws come in bursts while decks are opened).</summary>
        public static readonly TimeSpan DrawDebounce = TimeSpan.FromMilliseconds(250);

        public DivCardsViewModel(ITrackingSource source, GlobalFilter filter, Func<DateTime> now = null) : base(source, filter)
        {
            _cards = source as IDivCardSource;
            _now = now ?? (() => DateTime.Now);

            // Only this page re-reads on a draw; the query is cheap, so a short debounce is enough
            _drawDebounce = new DispatcherTimer { Interval = DrawDebounce };
            _drawDebounce.Tick += (s, e) => { _drawDebounce.Stop(); Refresh(); };
            if (_cards != null) _cards.DivCardsChanged += OnDivCardsChanged;

            Refresh();
        }

        private void OnDivCardsChanged(object sender, EventArgs e)
        {
            _drawDebounce.Dispatcher.BeginInvoke(new Action(() =>
            {
                _drawDebounce.Stop();
                _drawDebounce.Start();
            }));
        }

        public override void Dispose()
        {
            if (_cards != null) _cards.DivCardsChanged -= OnDivCardsChanged;
            _drawDebounce.Stop();
            base.Dispose();
        }

        /// <summary>The filter's time range; "All" means everything up to now.</summary>
        public (DateTime from, DateTime to) Window()
        {
            TimeWindow w = Filter.Window;
            return (w.From ?? new DateTime(2000, 1, 1), w.To ?? _now());
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value ?? "")) ApplySearch(); }
        }

        private DivCardSortColumn _sortColumn = DivCardSortColumn.Count;
        /// <summary>The column the list is sorted by; most drawn first by default.</summary>
        public DivCardSortColumn SortColumn { get => _sortColumn; private set => SetProperty(ref _sortColumn, value); }

        private bool _sortDescending = true;
        public bool SortDescending { get => _sortDescending; private set => SetProperty(ref _sortDescending, value); }

        /// <summary>
        /// Sorts by <paramref name="column"/>. The same column again flips the direction; a new column starts
        /// ascending for the card name and descending (largest / newest first) for the others.
        /// </summary>
        public void SortBy(DivCardSortColumn column)
        {
            if (column == SortColumn) SortDescending = !SortDescending;
            else
            {
                SortColumn = column;
                SortDescending = column != DivCardSortColumn.Name;
            }
            ApplySearch();
        }

        private IEnumerable<DivCardInfo> Sort(IEnumerable<DivCardInfo> cards)
        {
            IOrderedEnumerable<DivCardInfo> sorted;
            switch (SortColumn)
            {
                case DivCardSortColumn.Name:
                    sorted = SortDescending
                        ? cards.OrderByDescending(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
                        : cards.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase);
                    return sorted; // names are unique: no tie-break needed
                case DivCardSortColumn.LastDraw:
                    sorted = SortDescending ? cards.OrderByDescending(c => c.LastDraw) : cards.OrderBy(c => c.LastDraw);
                    break;
                case DivCardSortColumn.FirstDraw:
                    sorted = SortDescending ? cards.OrderByDescending(c => c.FirstDraw) : cards.OrderBy(c => c.FirstDraw);
                    break;
                default:
                    sorted = SortDescending ? cards.OrderByDescending(c => c.Count) : cards.OrderBy(c => c.Count);
                    break;
            }
            return sorted.ThenBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase); // stable, readable tie-break
        }

        private IReadOnlyList<DivCardInfo> _items = Array.Empty<DivCardInfo>();
        /// <summary>Cards matching the search, in the chosen sort order.</summary>
        public IReadOnlyList<DivCardInfo> Items { get => _items; private set => SetProperty(ref _items, value); }

        private string _summary = "";
        public string Summary { get => _summary; private set => SetProperty(ref _summary, value); }

        public bool IsEmpty => Items.Count == 0;
        public string EmptyText => _all.Count == 0
            ? "No divination cards in this time range."
            : "No divination cards match your search.";

        public override void Refresh()
        {
            (DateTime from, DateTime to) = Window();
            _all = _cards?.GetDivCards(from, to) ?? (IReadOnlyList<DivCardInfo>)Array.Empty<DivCardInfo>();
            ApplySearch();
        }

        protected override void OnThemeChanged() { } // no charts

        private void ApplySearch()
        {
            string term = SearchText.Trim();
            IEnumerable<DivCardInfo> matches = term.Length == 0
                ? _all
                : _all.Where(c => c.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
            Items = Sort(matches).ToList();

            int total = Items.Sum(c => c.Count);
            Summary = $"{total:N0} cards · {Items.Count:N0} different";
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(EmptyText));
        }

        /// <summary>
        /// CSV in the classic layout (Main.ExportDivCards): "Divination Card; Count; Last draw; First draw", UTF-8
        /// with BOM. Dates are written as yyyy-MM-dd HH:mm:ss local time (the classic wrote the UTC time in the
        /// machine's date format).
        /// </summary>
        public static void ExportCsv(string path, IEnumerable<DivCardInfo> cards)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Divination Card; Count; Last draw; First draw");
            foreach (DivCardInfo c in cards)
            {
                sb.Append(c.Name.Replace(";", ",")).Append(';')
                  .Append(c.Count.ToString(CultureInfo.InvariantCulture)).Append(';')
                  .Append(c.LastDraw.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(';')
                  .Append(c.FirstDraw.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                  .AppendLine();
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
