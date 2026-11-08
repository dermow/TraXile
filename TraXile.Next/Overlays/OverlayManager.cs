using System;
using System.Windows;
using TraXile.Next.Services;

namespace TraXile.Next.Overlays
{
    /// <summary>
    /// Owns the overlay windows: opens and hides them (tray menu, title bar), places them at their saved
    /// position and feeds them the live state.
    /// </summary>
    public sealed class OverlayManager : IDisposable
    {
        private readonly AppSetup _setup;
        private readonly bool _canWrite;
        private readonly Func<Window> _mainWindow;
        private StopwatchOverlay _stopwatch;
        private LiveState _live = new LiveState();

        /// <param name="canWrite">Positions may be saved (same rule as the settings).</param>
        /// <param name="mainWindow">Overlays without a saved position open next to it.</param>
        public OverlayManager(AppSetup setup, bool canWrite, Func<Window> mainWindow)
        {
            _setup = setup;
            _canWrite = canWrite;
            _mainWindow = mainWindow;
        }

        /// <summary>Raised when an overlay was opened or hidden (menus update their check marks).</summary>
        public event EventHandler Changed;

        public bool IsStopwatchOpen => _stopwatch != null && _stopwatch.IsVisible;

        public void ToggleStopwatch()
        {
            if (IsStopwatchOpen) _stopwatch.Hide();
            else ShowStopwatch();
        }

        public StopwatchOverlay ShowStopwatch()
        {
            if (_stopwatch == null)
            {
                _stopwatch = new StopwatchOverlay(_setup, _canWrite);
                _stopwatch.IsVisibleChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
                _stopwatch.Place(DefaultPosition(0)); // classic: at the main window's corner
            }
            _stopwatch.ApplyOpacity(OverlaySettings.ReadOpacity(_setup));
            _stopwatch.Update(_live);
            _stopwatch.Show();
            return _stopwatch;
        }

        /// <summary>Feeds the latest live state (once per second, from the dashboard).</summary>
        public void Update(LiveState live)
        {
            _live = live ?? new LiveState();
            if (IsStopwatchOpen) _stopwatch.Update(_live);
        }

        /// <summary>Default place of an overlay: top-left of the main window, the tags overlay 200 to the right.</summary>
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
            _stopwatch?.CloseForGood();
            _stopwatch = null;
        }
    }
}
