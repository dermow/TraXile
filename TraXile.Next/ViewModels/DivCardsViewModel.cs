using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Div Cards page (classic Main.RenderDivCards): every divination card drawn in the filter's time range with
    /// count, last and first draw, a search and the CSV export. Only the time range of the global filter applies:
    /// the core records cards without the activity they dropped in.
    /// </summary>
    public sealed class DivCardsViewModel : AnalyticsViewModel
    {
        private readonly IDivCardSource _cards;
        private readonly Func<DateTime> _now;
        private IReadOnlyList<DivCardInfo> _all = Array.Empty<DivCardInfo>();

        public DivCardsViewModel(ITrackingSource source, GlobalFilter filter, Func<DateTime> now = null) : base(source, filter)
        {
            _cards = source as IDivCardSource;
            _now = now ?? (() => DateTime.Now);
            Refresh();
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

        private IReadOnlyList<DivCardInfo> _items = Array.Empty<DivCardInfo>();
        /// <summary>Cards matching the search, most drawn first.</summary>
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
            Items = term.Length == 0
                ? _all
                : _all.Where(c => c.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

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
