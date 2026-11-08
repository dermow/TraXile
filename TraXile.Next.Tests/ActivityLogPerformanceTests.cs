using System;
using System.Diagnostics;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace TraXile.Next.Tests
{
    /// <summary>
    /// Guards against the activity log getting slow with big histories (heavy players have 20k+ activities).
    /// The budgets are generous on purpose: they catch accidental O(n²) work, not small regressions.
    /// </summary>
    public class ActivityLogPerformanceTests : IDisposable
    {
        private const int ActivityCount = 20000;
        private static readonly TimeSpan RefreshBudget = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan SelectionBudget = TimeSpan.FromMilliseconds(250);

        private readonly ITestOutputHelper _output;
        private readonly DemoTrackingSource _source = DemoTrackingSource.WithActivityCount(ActivityCount);
        private readonly ActivityLogViewModel _vm;

        public ActivityLogPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
            _vm = new ActivityLogViewModel(_source); // warm-up (JIT, brush cache)
        }

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        [Fact]
        public void Fixture_has_the_requested_size()
        {
            Assert.Equal(ActivityCount, _vm.Items.Count);
        }

        [Fact]
        public void Refresh_stays_within_budget()
        {
            AssertFast("Refresh", RefreshBudget, () => _vm.Refresh());
        }

        [Fact]
        public void Search_stays_within_budget()
        {
            _vm.SearchText = "temple t16";
            AssertFast("Search", RefreshBudget, () => _vm.Refresh());
            Assert.NotEmpty(_vm.Items);
        }

        [Fact]
        public void Type_filter_stays_within_budget()
        {
            GlobalFilter filter = _vm.Filter;
            // Setting the same value again is a no-op, so measure a filter/unfilter round trip
            AssertFast("Type filter on and off", RefreshBudget, () => { filter.Type = ACTIVITY_TYPES.MAP; filter.Type = null; });
        }

        [Fact]
        public void Selecting_all_activities_stays_within_budget()
        {
            AssertFast("Select all", SelectionBudget, () => _vm.SetSelection(_vm.Items));
            Assert.Equal(ActivityCount, _vm.SelectedCount);
        }

        private void AssertFast(string what, TimeSpan budget, Action action)
        {
            // Best of three, so a single GC pause or a busy build agent does not fail the test
            TimeSpan best = TimeSpan.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                if (sw.Elapsed < best) best = sw.Elapsed;
            }

            _output.WriteLine($"{what} with {ActivityCount:N0} activities: {best.TotalMilliseconds:N0} ms (budget {budget.TotalMilliseconds:N0} ms)");
            Assert.True(best <= budget, $"{what} took {best.TotalMilliseconds:N0} ms, budget is {budget.TotalMilliseconds:N0} ms");
        }
    }
}
