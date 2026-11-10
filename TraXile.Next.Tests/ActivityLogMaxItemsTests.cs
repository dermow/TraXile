using System;
using System.IO;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>"Max items" of the activity log (classic setting actlog.maxitems).</summary>
    public class ActivityLogMaxItemsTests : IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource(); // ~600 activities
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-maxitems-" + Guid.NewGuid().ToString("N"));
        private readonly AppSetup _setup;

        public ActivityLogMaxItemsTests()
        {
            _setup = new AppSetup(_dir);
        }

        public void Dispose()
        {
            _source.Dispose();
            if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
        }

        [Fact]
        public void Classic_default_is_500_newest()
        {
            using (var vm = new ActivityLogViewModel(_source, null, _setup))
            {
                Assert.Equal(500, vm.MaxItems);
                Assert.Equal(500, vm.Items.Count);
                Assert.True(vm.IsLimited);
                Assert.Equal(_source.GetHistory().Take(500).Select(a => a.Id), vm.Items.Select(a => a.Id));
                Assert.Equal(_source.GetHistory().Count, vm.MatchingItems.Count);
                Assert.StartsWith("Newest 500 of ", vm.Summary);
            }
        }

        [Fact]
        public void Stored_value_is_used_and_a_change_is_written()
        {
            _setup.WriteSetting(ActivityLogViewModel.MaxItemsKey, "100");
            using (var vm = new ActivityLogViewModel(_source, null, _setup, canWriteSettings: true))
            {
                Assert.Equal(100, vm.Items.Count);

                vm.SelectedMaxItems = vm.MaxItemsOptions.Single(o => o.Value == 50);

                Assert.Equal(50, vm.Items.Count);
                Assert.Equal("50", _setup.ReadSetting(ActivityLogViewModel.MaxItemsKey));
            }
        }

        [Fact]
        public void All_is_stored_as_a_number_the_classic_app_can_read()
        {
            using (var vm = new ActivityLogViewModel(_source, null, _setup, canWriteSettings: true))
            {
                vm.MaxItems = ActivityLogViewModel.AllItems;

                Assert.Equal(_source.GetHistory().Count, vm.Items.Count);
                Assert.False(vm.IsLimited);
                Assert.True(int.TryParse(_setup.ReadSetting(ActivityLogViewModel.MaxItemsKey), out _));
            }
        }

        [Fact]
        public void Read_only_does_not_write_the_setting()
        {
            using (var vm = new ActivityLogViewModel(_source, null, _setup, canWriteSettings: false))
            {
                vm.MaxItems = 50;

                Assert.Equal(50, vm.Items.Count);
                Assert.Null(_setup.ReadSetting(ActivityLogViewModel.MaxItemsKey));
            }
        }

        [Fact]
        public void Search_ignores_the_limit()
        {
            using (var vm = new ActivityLogViewModel(_source, null, _setup))
            {
                vm.MaxItems = 50;
                vm.SearchText = "map";
                vm.Refresh();

                Assert.True(vm.Items.Count > 50);
                Assert.False(vm.IsLimited);
            }
        }

        [Fact]
        public void Limit_applies_after_the_global_filter()
        {
            var filter = new GlobalFilter();
            using (var vm = new ActivityLogViewModel(_source, filter, _setup))
            {
                vm.MaxItems = 50;
                filter.Type = ACTIVITY_TYPES.MAP;

                Assert.Equal(50, vm.Items.Count);
                Assert.All(vm.Items, a => Assert.Equal(ACTIVITY_TYPES.MAP, a.Type));
                Assert.Equal(_source.GetHistory().Count(a => a.Type == ACTIVITY_TYPES.MAP), vm.MatchingItems.Count);
            }
        }

        [Theory]
        [InlineData("garbage")]
        [InlineData("0")]
        [InlineData("-5")]
        public void Invalid_stored_values_fall_back_to_500(string stored)
        {
            _setup.WriteSetting(ActivityLogViewModel.MaxItemsKey, stored);
            using (var vm = new ActivityLogViewModel(_source, null, _setup))
            {
                Assert.Equal(500, vm.MaxItems);
            }
        }
    }
}
