using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
