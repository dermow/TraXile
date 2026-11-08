using System;
using System.IO;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Overview table under the time distribution (classic Statistics > Overview).</summary>
    public class DashboardOverviewTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-overview-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;

        public DashboardOverviewTests() => _setup = new AppSetup(_dir);

        public void Dispose()
        {
            _source.Dispose();
            if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
        }

        [Fact]
        public void One_row_per_type_in_range_with_count_and_time_plus_hideout()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.Month })
            {
                DateTime from = DateTime.Now.Date.AddDays(-29);
                var inRange = _source.GetHistory().Where(a => a.Started >= from).ToList();

                OverviewRow[] types = vm.Overview.Where(r => !r.IsHideout).ToArray();
                Assert.Equal(inRange.Select(a => a.Type).Distinct().Count(), types.Length);
                foreach (OverviewRow row in types)
                {
                    var ofType = inRange.Where(a => a.Type == row.Type).ToList();
                    Assert.Equal(ofType.Count, row.Count);
                    Assert.Equal(ofType.Sum(a => Math.Min(a.TotalSeconds, _source.GetTimeCap(a.Type))), row.Seconds);
                }

                Assert.True(vm.Overview.Last().IsHideout);
                Assert.Equal("–", vm.Overview.Last().CountText);
                Assert.Equal(1.0, vm.Overview.Sum(r => r.Fraction), 6);
            }
        }

        [Fact]
        public void Rows_are_ordered_by_time()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.All })
            {
                double[] seconds = vm.Overview.Where(r => !r.IsHideout).Select(r => r.Seconds).ToArray();
                Assert.Equal(seconds.OrderByDescending(s => s), seconds);
            }
        }

        [Fact]
        public void Without_hideout_the_shares_cover_the_activities_only()
        {
            using (var vm = new DashboardViewModel(_source, _setup, canWriteSettings: true))
            {
                vm.ShowHideout = false;

                Assert.DoesNotContain(vm.Overview, r => r.IsHideout);
                Assert.Equal(1.0, vm.Overview.Sum(r => r.Fraction), 6);
                Assert.Equal("False", _setup.ReadSetting(DashboardViewModel.ShowHideoutKey));
            }
        }

        [Fact]
        public void Hideout_setting_is_read_with_the_classic_default()
        {
            using (var vm = new DashboardViewModel(_source, _setup))
            {
                Assert.True(vm.ShowHideout); // classic default "true"
            }

            _setup.WriteSetting(DashboardViewModel.ShowHideoutKey, "False");
            using (var vm = new DashboardViewModel(_source, _setup))
            {
                Assert.False(vm.ShowHideout);
            }
        }

        [Fact]
        public void Read_only_does_not_write_the_setting()
        {
            using (var vm = new DashboardViewModel(_source, _setup, canWriteSettings: false))
            {
                vm.ShowHideout = false;
                Assert.Null(_setup.ReadSetting(DashboardViewModel.ShowHideoutKey));
            }
        }

        [Fact]
        public void Rows_follow_the_dashboard_time_range()
        {
            using (var vm = new DashboardViewModel(_source, _setup))
            {
                vm.Range = TimeRange.All;
                int all = vm.Overview.Where(r => !r.IsHideout).Sum(r => r.Count);
                vm.Range = TimeRange.Today;
                int today = vm.Overview.Where(r => !r.IsHideout).Sum(r => r.Count);

                Assert.Equal(_source.GetHistory().Count, all);
                Assert.Equal(_source.GetHistory().Count(a => a.Started >= DateTime.Today), today);
            }
        }
    }
}
