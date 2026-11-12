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

                // own rows plus the types grouped into "Other" cover every type in range
                OverviewRow[] types = vm.Overview.Where(r => !r.IsHideout)
                                                 .SelectMany(r => r.IsOther ? r.Members : new[] { r }).ToArray();
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
                double[] seconds = vm.Overview.Where(r => !r.IsHideout && !r.IsOther).Select(r => r.Seconds).ToArray();
                Assert.Equal(seconds.OrderByDescending(s => s), seconds);
            }
        }

        [Fact]
        public void Small_types_are_grouped_into_one_other_row_before_hideout()
        {
            // total 100: MAP 60, HEIST 20, DELVE 3, TEMPLE 2, LAB 1, hideout 14
            var rows = DashboardViewModel.GroupTimeDistribution(new[]
            {
                (ACTIVITY_TYPES.DELVE, 3, 3.0),
                (ACTIVITY_TYPES.MAP, 10, 60.0),
                (ACTIVITY_TYPES.LABYRINTH, 1, 1.0),
                (ACTIVITY_TYPES.HEIST, 4, 20.0),
                (ACTIVITY_TYPES.TEMPLE, 2, 2.0),
            }, hideoutSeconds: 14);

            Assert.Equal(4, rows.Count); // Map, Heist, Other, Hideout
            Assert.Equal(ACTIVITY_TYPES.MAP, rows[0].Type);
            Assert.Equal(ACTIVITY_TYPES.HEIST, rows[1].Type);

            OverviewRow other = rows[2];
            Assert.True(other.IsOther);
            Assert.Null(other.Type);
            Assert.Equal(6, other.Count);
            Assert.Equal(6.0, other.Seconds);
            Assert.Equal(0.06, other.Fraction, 6);
            Assert.Equal(new[] { ACTIVITY_TYPES.DELVE, ACTIVITY_TYPES.TEMPLE, ACTIVITY_TYPES.LABYRINTH },
                         other.Members.Select(m => m.Type.Value));
            Assert.All(other.Members, m => Assert.True(m.IsMember));

            // sub-row shares are shares of the total time and add up to "Other"
            Assert.Equal(new[] { 0.03, 0.02, 0.01 }, other.Members.Select(m => Math.Round(m.Fraction, 6)));
            Assert.Equal(other.Fraction, other.Members.Sum(m => m.Fraction), 6);

            Assert.True(rows[3].IsHideout);
            Assert.Equal(1.0, rows.Sum(r => r.Fraction), 6);
        }

        [Fact]
        public void Other_expands_into_sub_rows_below_it_and_collapses_again()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.All })
            {
                OverviewRow other = vm.Overview.SingleOrDefault(r => r.IsOther);
                Assert.NotNull(other); // the demo data has small types

                Assert.False(vm.IsOtherExpanded);
                Assert.Equal(vm.Overview, vm.OverviewTable);
                Assert.False(other.IsExpanded);

                vm.ToggleOtherCommand.Execute(null);
                Assert.True(vm.IsOtherExpanded);
                int at = vm.OverviewTable.IndexOf(other);
                Assert.Equal(other.Members, vm.OverviewTable.Skip(at + 1).Take(other.Members.Count));
                Assert.Equal(vm.Overview.Count + other.Members.Count, vm.OverviewTable.Count);
                Assert.True(other.IsExpanded);

                vm.ToggleOtherCommand.Execute(null);
                Assert.False(vm.IsOtherExpanded);
                Assert.False(other.IsExpanded);
                Assert.Equal(vm.Overview, vm.OverviewTable);
            }
        }

        [Fact]
        public void Expansion_survives_refreshes_and_range_changes()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.All })
            {
                vm.ToggleOtherCommand.Execute(null);

                vm.Refresh();
                vm.ShowHideout = !vm.ShowHideout;
                vm.Range = TimeRange.Month;
                Assert.True(vm.Overview.Any(r => r.IsOther), "the month of demo data has small types");

                Assert.True(vm.IsOtherExpanded);
                OverviewRow other = vm.Overview.Single(r => r.IsOther);
                Assert.Contains(other.Members[0], vm.OverviewTable);
            }
        }

        [Fact]
        public void Expansion_ends_when_there_is_no_other_row()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.All })
            {
                vm.ToggleOtherCommand.Execute(null);
                vm.Filter.Type = ACTIVITY_TYPES.MAP; // a single type: nothing to group

                Assert.DoesNotContain(vm.Overview, r => r.IsOther);
                Assert.False(vm.IsOtherExpanded);
                Assert.Equal(vm.Overview, vm.OverviewTable);
            }
        }

        [Fact]
        public void Each_donut_slice_stands_for_its_overview_row()
        {
            using (var vm = new DashboardViewModel(_source, _setup) { Range = TimeRange.All })
            {
                Assert.Equal(vm.Overview.Count, vm.TimeSeries.Length);
                for (int i = 0; i < vm.TimeSeries.Length; i++)
                    Assert.Same(vm.Overview[i], vm.RowOfSlice(vm.TimeSeries[i]));
            }
        }

        [Fact]
        public void Small_hideout_keeps_its_own_row()
        {
            var rows = DashboardViewModel.GroupTimeDistribution(new[]
            {
                (ACTIVITY_TYPES.MAP, 10, 99.0),
            }, hideoutSeconds: 1);

            Assert.Equal(2, rows.Count);
            Assert.True(rows[1].IsHideout);
            Assert.DoesNotContain(rows, r => r.IsOther);
        }

        [Fact]
        public void A_lone_small_type_keeps_its_name()
        {
            var rows = DashboardViewModel.GroupTimeDistribution(new[]
            {
                (ACTIVITY_TYPES.MAP, 10, 97.0),
                (ACTIVITY_TYPES.DELVE, 1, 3.0),
            }, hideoutSeconds: 0);

            Assert.DoesNotContain(rows, r => r.IsOther);
            Assert.Equal(new ACTIVITY_TYPES?[] { ACTIVITY_TYPES.MAP, ACTIVITY_TYPES.DELVE }, rows.Select(r => r.Type));
            Assert.False(rows[1].IsMember);
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
