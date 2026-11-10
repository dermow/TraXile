using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class DivCardsTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter();
        private readonly DivCardsViewModel _vm;

        public DivCardsTests() => _vm = new DivCardsViewModel(_source, _filter);

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        [Fact]
        public void Lists_cards_most_drawn_first_with_first_and_last_draw()
        {
            Assert.True(_vm.Items.Count > 3);
            int[] counts = _vm.Items.Select(c => c.Count).ToArray();
            Assert.Equal(counts.OrderByDescending(c => c), counts);
            Assert.All(_vm.Items, c => Assert.True(c.FirstDraw <= c.LastDraw));
            Assert.Equal($"{counts.Sum():N0} cards · {counts.Length:N0} different", _vm.Summary);
        }

        [Fact]
        public void Search_is_case_insensitive()
        {
            _vm.SearchText = "rain of";
            Assert.Equal("Rain of Chaos", _vm.Items.Single().Name);

            _vm.SearchText = "does not exist";
            Assert.True(_vm.IsEmpty);
            Assert.Equal("No divination cards match your search.", _vm.EmptyText);
        }

        [Fact]
        public void Time_range_of_the_filter_applies()
        {
            int all = _vm.Items.Sum(c => c.Count);
            _filter.Time = FilterTime.Last7Days;

            int week = _vm.Items.Sum(c => c.Count);
            Assert.True(week > 0 && week < all);
            DateTime from = DateTime.Now.AddDays(-7).AddMinutes(-1);
            Assert.All(_vm.Items, c => Assert.True(c.FirstDraw >= from));
        }

        [Fact]
        public void Every_column_is_sortable_and_a_second_click_flips_the_direction()
        {
            _vm.SortBy(DivCardSortColumn.Name);
            Assert.False(_vm.SortDescending);
            string[] names = _vm.Items.Select(c => c.Name).ToArray();
            Assert.Equal(names.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase), names);

            _vm.SortBy(DivCardSortColumn.Name);
            Assert.True(_vm.SortDescending);
            names = _vm.Items.Select(c => c.Name).ToArray();
            Assert.Equal(names.OrderByDescending(n => n, StringComparer.CurrentCultureIgnoreCase), names);

            _vm.SortBy(DivCardSortColumn.LastDraw);
            Assert.True(_vm.SortDescending);
            DateTime[] last = _vm.Items.Select(c => c.LastDraw).ToArray();
            Assert.Equal(last.OrderByDescending(d => d), last);

            _vm.SortBy(DivCardSortColumn.FirstDraw);
            _vm.SortBy(DivCardSortColumn.FirstDraw);
            Assert.False(_vm.SortDescending);
            DateTime[] first = _vm.Items.Select(c => c.FirstDraw).ToArray();
            Assert.Equal(first.OrderBy(d => d), first);

            _vm.SortBy(DivCardSortColumn.Count);
            _vm.SortBy(DivCardSortColumn.Count);
            int[] counts = _vm.Items.Select(c => c.Count).ToArray();
            Assert.Equal(counts.OrderBy(c => c), counts);
        }

        [Fact]
        public void Sort_order_survives_search_and_refresh()
        {
            _vm.SortBy(DivCardSortColumn.Name);
            _vm.SearchText = "a";
            _filter.Time = FilterTime.Last7Days;

            Assert.Equal(DivCardSortColumn.Name, _vm.SortColumn);
            string[] names = _vm.Items.Select(c => c.Name).ToArray();
            Assert.Equal(names.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase), names);
        }

        /// <summary>Live source stand-in whose card list grows when a card is "drawn".</summary>
        private sealed class DrawingSource : ITrackingSource, IDivCardSource
        {
            private readonly DemoTrackingSource _demo = new DemoTrackingSource();
            public int Drawn;
            public int HistoryReads;

            public void Draw()
            {
                Drawn++;
                // the core raises it on its worker thread
                var t = new System.Threading.Thread(() => DivCardsChanged?.Invoke(this, EventArgs.Empty));
                t.Start();
                t.Join();
            }

            public event EventHandler DivCardsChanged;
            public IReadOnlyList<DivCardInfo> GetDivCards(DateTime from, DateTime to) =>
                Drawn == 0 ? Array.Empty<DivCardInfo>()
                    : new[] { new DivCardInfo { Name = "The Gemcutter", Count = Drawn, FirstDraw = DateTime.Now, LastDraw = DateTime.Now } };

            public bool IsDemo => false;
            public string Notice => null;
            public event EventHandler HistoryChanged { add { } remove { } }
            public void Start() { }
            public bool IsLoading => false;
            public double LoadProgress => 1;
            public IReadOnlyList<ActivityInfo> GetHistory() { HistoryReads++; return _demo.GetHistory(); }
            public LiveState GetLiveState() => _demo.GetLiveState();
            public double GetHideoutSeconds(DateTime from, DateTime to) => 0;
            public int GetTimeCap(ACTIVITY_TYPES type) => 3600;
            public void TogglePause() { }
            public void StopCurrentActivity() { }
            public void ToggleCurrentActivityTag(string tagId) { }
            public void CreateTag(string id, string displayName) { }
            public void UpdateTag(TagInfo changed) { }
            public void DeleteTag(string id) { }
            public IReadOnlyList<TagInfo> GetTags() => _demo.GetTags();
            public void AddTag(string activityId, string tagId) { }
            public void RemoveTag(string activityId, string tagId) { }
            public void DeleteActivities(IReadOnlyCollection<string> activityIds) { }
            public void Dispose() => _demo.Dispose();
        }

        /// <summary>Runs the dispatcher of this thread until <paramref name="done"/> or the timeout.</summary>
        private static bool PumpUntil(Func<bool> done, TimeSpan timeout)
        {
            DateTime end = DateTime.Now + timeout;
            while (!done() && DateTime.Now < end)
            {
                var frame = new DispatcherFrame();
                var tick = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
                tick.Tick += (s, e) => { tick.Stop(); frame.Continue = false; };
                tick.Start();
                Dispatcher.PushFrame(frame);
            }
            return done();
        }

        [Fact]
        public void A_drawn_card_shows_up_without_rebuilding_the_history_pages()
        {
            var source = new DrawingSource();
            using (var vm = new DivCardsViewModel(source, new GlobalFilter()))
            {
                Assert.True(vm.IsEmpty);

                source.Draw();
                source.Draw();
                Assert.True(PumpUntil(() => vm.Items.Count == 1 && vm.Items[0].Count == 2, TimeSpan.FromSeconds(3)));
                Assert.Equal("2 cards · 1 different", vm.Summary);
                Assert.Equal(0, source.HistoryReads); // the div card page does not need the Activities
            }
            source.Dispose();
        }

        [Fact]
        public void Csv_export_uses_the_classic_layout()
        {
            string path = Path.Combine(Path.GetTempPath(), $"traxile-cards-{Guid.NewGuid():N}.csv");
            try
            {
                DivCardsViewModel.ExportCsv(path, _vm.Items);

                Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, File.ReadAllBytes(path).Take(3)); // UTF-8 BOM like File.WriteAllText(.., Encoding.UTF8)
                string[] lines = File.ReadAllLines(path, Encoding.UTF8);
                Assert.Equal("Divination Card; Count; Last draw; First draw", lines[0]);
                Assert.Equal(_vm.Items.Count + 1, lines.Length);
                DivCardInfo c = _vm.Items[0];
                Assert.Equal($"{c.Name};{c.Count};{c.LastDraw.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)};" +
                             c.FirstDraw.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), lines[1]);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
