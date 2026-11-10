using System.Collections.Generic;
using System.Linq;
using TraXile.Next.Services;
using Xunit;

namespace TraXile.Next.Tests
{
    public class TagSoundTests
    {
        private sealed class RecordingPlayer : ITagSoundPlayer
        {
            public List<string> Played { get; } = new List<string>();
            public void Play(string soundId) => Played.Add(soundId);
        }

        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly RecordingPlayer _player = new RecordingPlayer();
        private readonly TagSoundNotifier _notifier;

        private static readonly TagInfo Blight = new TagInfo { Id = "blight", Name = "Blight", IsDefault = true, SoundEnabled = true, SoundId = "default_1.mp3" };
        private static readonly TagInfo Juiced = new TagInfo { Id = "juiced", Name = "Juiced", SoundEnabled = true, SoundId = "default_2.mp3" };
        private static readonly TagInfo Silent = new TagInfo { Id = "silent", Name = "Silent", SoundEnabled = false, SoundId = "default_3.mp3" };

        public TagSoundTests()
        {
            _notifier = new TagSoundNotifier(_source, _player);
        }

        private static LiveState Live(string activityId, params TagInfo[] tags) => new LiveState
        {
            IsTracking = true,
            Activity = new ActivityInfo { Id = activityId, Tags = tags },
            ActiveTags = tags
        };

        [Fact]
        public void Tags_already_active_on_app_start_stay_silent()
        {
            _notifier.Observe(Live("a", Blight, Juiced));
            _notifier.Observe(Live("a", Blight, Juiced));

            Assert.Empty(_player.Played);
        }

        [Fact]
        public void Tags_active_while_the_log_is_read_stay_silent()
        {
            _source.SimulateLoading = true;
            _notifier.Observe(Live("old"));
            _notifier.Observe(Live("old", Blight));
            _source.SimulateLoading = false;
            _notifier.Observe(Live("a", Blight));

            Assert.Empty(_player.Played);
        }

        [Fact]
        public void Sound_plays_once_per_activation()
        {
            _notifier.Observe(Live("a"));

            _notifier.Observe(Live("a", Juiced));   // click on the live card / chat command
            _notifier.Observe(Live("a", Juiced));   // still active: no repeat
            _notifier.Observe(Live("a"));           // removed
            _notifier.Observe(Live("a", Juiced));   // activated again

            Assert.Equal(new[] { "default_2.mp3", "default_2.mp3" }, _player.Played);
        }

        [Fact]
        public void Default_tags_set_by_the_tracker_play_their_sound()
        {
            _notifier.Observe(Live("a"));
            _notifier.Observe(Live("a", Blight));

            Assert.Equal(new[] { "default_1.mp3" }, _player.Played);
        }

        [Fact]
        public void Tags_without_sound_stay_silent()
        {
            _notifier.Observe(Live("a"));
            _notifier.Observe(Live("a", Silent));

            Assert.Empty(_player.Played);
        }

        [Fact]
        public void A_new_activity_plays_its_tags_but_returning_from_a_side_area_does_not()
        {
            _notifier.Observe(Live("map1"));
            _notifier.Observe(Live("map1", Blight));          // played
            _notifier.Observe(Live("vaal-side-area"));        // effective activity is the side area
            _notifier.Observe(Live("map1", Blight));          // back in the map: no replay
            _notifier.Observe(new LiveState());               // idle in hideout
            _notifier.Observe(Live("map2", Juiced));          // next map starts with a tag: played

            Assert.Equal(new[] { "default_1.mp3", "default_2.mp3" }, _player.Played);
        }

        [Fact]
        public void Starting_idle_then_entering_a_tagged_activity_plays()
        {
            _notifier.Observe(new LiveState());
            _notifier.Observe(Live("a", Juiced));

            Assert.Single(_player.Played);
        }
    }
}
