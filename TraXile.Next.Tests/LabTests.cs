using System;
using System.IO;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class LabTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly GlobalFilter _filter = new GlobalFilter();
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-lab-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;

        public LabTests() => _setup = new AppSetup(_dir);

        public void Dispose()
        {
            _source.Dispose();
            if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
        }

        private ActivityInfo[] Labs => _source.GetHistory().Where(a => a.Type == ACTIVITY_TYPES.LABYRINTH).ToArray();

        [Fact]
        public void One_row_per_lab_counting_deathless_runs_only()
        {
            using (var vm = new LabViewModel(_source, _filter, _setup))
            {
                Assert.Equal(LabViewModel.Labs, vm.Rows.Select(r => r.Name));
                foreach (LabRow row in vm.Rows)
                {
                    ActivityInfo[] clean = Labs.Where(a => a.Area == row.Name && a.Deaths == 0).ToArray();
                    Assert.Equal(clean.Length, row.Count);
                    if (clean.Length > 0)
                    {
                        Assert.Equal(clean.Min(a => a.TotalSeconds), row.Best.TotalSeconds);
                        int cap = _source.GetTimeCap(ACTIVITY_TYPES.LABYRINTH);
                        Assert.Equal(clean.Average(a => Math.Min(a.TotalSeconds, cap)), row.AverageSeconds, 6);
                    }
                }
                Assert.Equal(Labs.Length, vm.Runs.Count); // the run list also shows runs with deaths
            }
        }

        [Fact]
        public void A_run_with_deaths_does_not_count()
        {
            ActivityInfo run = Labs.First(a => a.Deaths == 0);
            using (var vm = new LabViewModel(_source, _filter, _setup))
            {
                int before = vm.Rows.Single(r => r.Name == run.Area).Count;
                run.Deaths = 1; // demo activities are mutable snapshots
                vm.Refresh();
                Assert.Equal(before - 1, vm.Rows.Single(r => r.Name == run.Area).Count);
            }
        }

        [Fact]
        public void Hide_unknown_drops_the_row_and_its_runs_and_is_stored()
        {
            using (var vm = new LabViewModel(_source, _filter, _setup, canWriteSettings: true))
            {
                vm.HideUnknown = true;

                Assert.DoesNotContain(vm.Rows, r => r.Name == "Unknown");
                Assert.DoesNotContain(vm.Runs, a => a.Area == "Unknown");
                Assert.Equal("True", _setup.ReadSetting(LabViewModel.HideUnknownKey));
            }
            using (var vm = new LabViewModel(_source, _filter, _setup))
            {
                Assert.True(vm.HideUnknown);
            }
        }

        [Theory]
        [InlineData("The Labyrinth", "The Labyrinth")]
        [InlineData("The Merciless Labyrinth", "Merciless")]
        [InlineData("Advanced Uber-Lab", "Adv. Uber-Lab")]
        [InlineData("Unknown", "Unknown*")]
        public void Short_names_like_the_classic_chart(string lab, string expected)
        {
            Assert.Equal(expected, LabViewModel.ShortName(lab));
        }

        [Fact]
        public void Global_filter_applies()
        {
            using (var vm = new LabViewModel(_source, _filter, _setup))
            {
                _filter.Time = FilterTime.Today;
                Assert.Equal(Labs.Count(a => a.Started >= DateTime.Today), vm.Runs.Count);
            }
        }
    }
}
