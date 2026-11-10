using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Actions on the live card of the dashboard.</summary>
    public class LiveCardTests : System.IDisposable
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly DashboardViewModel _vm;

        public LiveCardTests()
        {
            _vm = new DashboardViewModel(_source);
        }

        public void Dispose()
        {
            _vm.Dispose();
            _source.Dispose();
        }

        [Fact]
        public void Stop_finishes_the_current_activity_and_adds_it_to_the_history()
        {
            string currentId = _vm.Live.Activity.Id;
            int before = _source.GetHistory().Count;
            Assert.True(_vm.StopCommand.CanExecute(null));

            _vm.StopCommand.Execute(null);

            Assert.False(_vm.Live.IsTracking);
            Assert.Equal("Waiting for activity", _vm.LiveTitle);
            Assert.Equal(before + 1, _source.GetHistory().Count);
            ActivityInfo finished = _source.GetHistory().First();
            Assert.Equal(currentId, finished.Id);
            Assert.True(finished.TotalSeconds > 0);
            Assert.Equal(currentId, _vm.Recent.First().Id); // dashboard refreshed right away
        }

        [Fact]
        public void Live_card_shows_active_default_tags_read_only_and_all_custom_tags_as_toggles()
        {
            Assert.All(_vm.LiveTags.Where(c => c.Tag.IsDefault), c => Assert.True(c.IsActive));
            Assert.Contains(_vm.LiveTags, c => c.Tag.Id == "delirium" && !c.CanToggle);

            string[] custom = _source.GetTags().Where(t => !t.IsDefault).Select(t => t.Id).ToArray();
            Assert.Equal(custom.OrderBy(x => x), _vm.LiveTags.Where(c => c.CanToggle).Select(c => c.Tag.Id).OrderBy(x => x));
            Assert.True(_vm.LiveTags.Single(c => c.Tag.Id == "juiced").IsActive);
            Assert.False(_vm.LiveTags.Single(c => c.Tag.Id == "party").IsActive);
        }

        [Fact]
        public void Toggling_custom_tags_changes_the_current_activity()
        {
            _vm.ToggleTagCommand.Execute(_vm.LiveTags.Single(c => c.Tag.Id == "party"));
            _vm.ToggleTagCommand.Execute(_vm.LiveTags.Single(c => c.Tag.Id == "juiced"));

            Assert.True(_vm.LiveTags.Single(c => c.Tag.Id == "party").IsActive);
            Assert.False(_vm.LiveTags.Single(c => c.Tag.Id == "juiced").IsActive);
            Assert.Contains(_source.GetLiveState().ActiveTags, t => t.Id == "party");
        }

        [Fact]
        public void Default_tags_cannot_be_toggled()
        {
            _vm.ToggleTagCommand.Execute(_vm.LiveTags.Single(c => c.Tag.Id == "delirium"));
            _source.ToggleCurrentActivityTag("expedition");

            Assert.Contains(_source.GetLiveState().ActiveTags, t => t.Id == "delirium");
            Assert.Contains(_source.GetLiveState().ActiveTags, t => t.Id == "expedition");
        }

        [Fact]
        public void Toggled_tags_are_kept_when_the_activity_finishes()
        {
            _vm.ToggleTagCommand.Execute(_vm.LiveTags.Single(c => c.Tag.Id == "bricked"));

            _vm.StopCommand.Execute(null);

            ActivityInfo finished = _source.GetHistory().First();
            Assert.Contains(finished.Tags, t => t.Id == "bricked");
            Assert.Empty(_vm.LiveTags); // idle: nothing to tag
        }

        [Fact]
        public void Stop_is_disabled_without_a_current_activity()
        {
            _vm.StopCommand.Execute(null);

            Assert.False(_vm.StopCommand.CanExecute(null));
            int count = _source.GetHistory().Count;
            _source.StopCurrentActivity(); // no-op
            Assert.Equal(count, _source.GetHistory().Count);
        }
    }
}
