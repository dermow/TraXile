using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TraXile.Next.Services;

namespace TraXile.Next.Overlays
{
    /// <summary>
    /// Owns the overlay windows: opens and hides them (tray menu, title bar), places them at their saved
    /// position and feeds them the live state.
    /// </summary>
    public sealed class OverlayManager : IOverlayHost, IDisposable
    {
        private readonly AppSetup _setup;
        private readonly ITrackingSource _source;
        private readonly bool _canWrite;
        private readonly Func<Window> _mainWindow;
        private StopwatchOverlay _stopwatch;
        private TagsOverlay _tags;
        private LiveState _live = new LiveState();
        private string[] _tagSlots;

        /// <param name="canWrite">Positions may be saved (same rule as the settings).</param>
        /// <param name="mainWindow">Overlays without a saved position open next to it.</param>
        public OverlayManager(AppSetup setup, ITrackingSource source, bool canWrite, Func<Window> mainWindow)
        {
            _setup = setup;
            _source = source;
            _canWrite = canWrite;
            _mainWindow = mainWindow;
            _tagSlots = OverlaySettings.ReadTagSlots(setup);
        }

        /// <summary>Raised when an overlay was opened or hidden (menus update their check marks).</summary>
        public event EventHandler Changed;

        public bool IsStopwatchOpen => _stopwatch != null && _stopwatch.IsVisible;
        public bool IsTagsOpen => _tags != null && _tags.IsVisible;

        private IEnumerable<OverlayWindow> Windows => new OverlayWindow[] { _stopwatch, _tags }.Where(w => w != null);

        public void ToggleStopwatch()
        {
            if (IsStopwatchOpen) _stopwatch.Hide();
            else ShowStopwatch();
        }

        public void ToggleTags()
        {
            if (IsTagsOpen) _tags.Hide();
            else ShowTags();
        }

        public StopwatchOverlay ShowStopwatch()
        {
            if (_stopwatch == null)
            {
                _stopwatch = Init(new StopwatchOverlay(_setup, _canWrite), DefaultOffset(OverlaySettings.Stopwatch));
            }
            _stopwatch.Update(_live);
            return Open(_stopwatch);
        }

        public TagsOverlay ShowTags()
        {
            if (_tags == null)
            {
                _tags = Init(new TagsOverlay(_setup, _canWrite), DefaultOffset(OverlaySettings.Tags));
            }
            UpdateTags();
            return Open(_tags);
        }

        private T Init<T>(T overlay, double offsetX) where T : OverlayWindow
        {
            overlay.IsVisibleChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
            overlay.Place(DefaultPosition(offsetX));
            return overlay;
        }

        private T Open<T>(T overlay) where T : OverlayWindow
        {
            overlay.ApplyOpacity(OverlaySettings.ReadOpacity(_setup));
            overlay.Show();
            return overlay;
        }

        /// <summary>Opens the overlays that are set to "show by default" (on start, like the classic app).</summary>
        public void ShowDefaults()
        {
            if (OverlaySettings.ReadShowDefault(_setup, OverlaySettings.Stopwatch)) ShowStopwatch();
            if (OverlaySettings.ReadShowDefault(_setup, OverlaySettings.Tags)) ShowTags();
        }

        /// <summary>Feeds the latest live state (once per second, from the dashboard).</summary>
        public void Update(LiveState live)
        {
            _live = live ?? new LiveState();
            if (IsStopwatchOpen) _stopwatch.Update(_live);
            if (IsTagsOpen) UpdateTags();
        }

        private void UpdateTags()
        {
            IReadOnlyList<TagInfo> all = _source.GetTags();
            TagInfo[] slotTags = _tagSlots.Select(s => OverlaySettings.ResolveTag(s, all)).ToArray();
            _tags.Update(_live, TagsOverlay.SlotsFor(_live, slotTags));
        }

        /// <summary>Re-reads opacity and tag slots after the overlay settings changed.</summary>
        public void ReloadSettings()
        {
            _tagSlots = OverlaySettings.ReadTagSlots(_setup);
            int opacity = OverlaySettings.ReadOpacity(_setup);
            foreach (OverlayWindow w in Windows) w.ApplyOpacity(opacity);
            if (IsTagsOpen) UpdateTags();
        }

        /// <summary>Moves all overlays back to their default place next to the main window and saves it.</summary>
        public void ResetPositions()
        {
            // Overlays that were not opened yet are created (hidden), so their saved position is replaced too
            if (_stopwatch == null) _stopwatch = Init(new StopwatchOverlay(_setup, _canWrite), DefaultOffset(OverlaySettings.Stopwatch));
            if (_tags == null) _tags = Init(new TagsOverlay(_setup, _canWrite), DefaultOffset(OverlaySettings.Tags));
            _stopwatch.ResetPosition(DefaultPosition(DefaultOffset(OverlaySettings.Stopwatch)));
            _tags.ResetPosition(DefaultPosition(DefaultOffset(OverlaySettings.Tags)));
        }

        /// <summary>Classic defaults: the stopwatch at the main window's corner, the tags overlay 200 to the right.</summary>
        private static double DefaultOffset(string id) => id == OverlaySettings.Tags ? 200 : 0;

        private Point DefaultPosition(double offsetX)
        {
            Window main = _mainWindow?.Invoke();
            if (main == null || main.WindowState == WindowState.Minimized || !main.IsVisible || main.Left < -10000)
            {
                return new Point(SystemParameters.WorkArea.Left + 40 + offsetX, SystemParameters.WorkArea.Top + 40);
            }
            return new Point(main.Left + 40 + offsetX, main.Top + 48);
        }

        public void Dispose()
        {
            foreach (OverlayWindow w in Windows.ToList()) w.CloseForGood();
            _stopwatch = null;
            _tags = null;
        }
    }
}
