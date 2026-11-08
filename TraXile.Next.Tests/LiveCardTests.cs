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
