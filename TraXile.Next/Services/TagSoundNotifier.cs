using System.Collections.Generic;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>Plays a tag sound (a file in the audio folder next to the exe).</summary>
    public interface ITagSoundPlayer
    {
        void Play(string soundId);
    }

    /// <summary>Plays sounds with the core's sound manager (NAudio), like the classic app.</summary>
    public sealed class CoreTagSoundPlayer : ITagSoundPlayer
    {
        private readonly TrX_SoundManager _manager = new TrX_SoundManager();

        public void Play(string soundId) => _manager.PlaySound(soundId); // logs and swallows errors
    }

    /// <summary>
    /// Plays the sound of a tag whenever it becomes active on the current activity — by a click on the
    /// live card, a chat command or as a default tag set by the tracker (classic: RenderTagsForTracking).
    /// Fed with every live state update. Tags that are already active when TraXile starts (or while the
    /// log is read for the first time) stay silent.
    /// </summary>
    public sealed class TagSoundNotifier
    {
        // Remembered per activity, so returning from a side area to the map does not replay the map's tags
        private const int RememberedActivities = 16;

        private readonly ITrackingSource _source;
        private readonly ITagSoundPlayer _player;
        private readonly Dictionary<string, HashSet<string>> _activeTags = new Dictionary<string, HashSet<string>>();
        private readonly Queue<string> _order = new Queue<string>();
        private bool _primed;

        public TagSoundNotifier(ITrackingSource source, ITagSoundPlayer player)
        {
            _source = source;
            _player = player;
        }

        public void Observe(LiveState state)
        {
            if (_source.IsLoading)
            {
                // The "current" activity is a historic one while the log is parsed
                _primed = false;
                _activeTags.Clear();
                _order.Clear();
                return;
            }

            if (state == null || !state.IsTracking || state.Activity?.Id == null)
            {
                _primed = true;
                return;
            }

            string id = state.Activity.Id;
            var now = new HashSet<string>(state.ActiveTags.Select(t => t.Id));
            bool known = _activeTags.TryGetValue(id, out HashSet<string> before);
            Remember(id, now);

            if (!_primed)
            {
                _primed = true; // app start: tags that are already active stay silent
                return;
            }

            foreach (TagInfo tag in state.ActiveTags)
            {
                bool activated = !known || !before.Contains(tag.Id);
                if (activated && tag.SoundEnabled && TagSoundLibrary.IsSet(tag.SoundId))
                {
                    _player.Play(tag.SoundId);
                }
            }
        }

        private void Remember(string activityId, HashSet<string> tags)
        {
            if (!_activeTags.ContainsKey(activityId))
            {
                _order.Enqueue(activityId);
                while (_order.Count > RememberedActivities) _activeTags.Remove(_order.Dequeue());
            }
            _activeTags[activityId] = tags;
        }
    }
}
