using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TraXile.Next.Services;

namespace TraXile.Next.Overlays
{
    /// <summary>
    /// Simple stopwatch overlay (classic: OverlaySimpleStopwatch): the time of the current activity,
    /// with its activity color. Muted while paused or idle.
    /// </summary>
    public sealed class StopwatchOverlay : OverlayWindow
    {
        private readonly TextBlock _time;
        private readonly Rectangle _mark;

        public StopwatchOverlay(AppSetup setup, bool canWrite) : base(OverlaySettings.Stopwatch, setup, canWrite)
        {
            Title = "TraXile stopwatch";

            _mark = new Rectangle
            {
                Width = 7,
                Height = 7,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = new RotateTransform(45, 3.5, 3.5)
            };
            _time = new TextBlock
            {
                Style = (Style)Application.Current.FindResource("Metric"),
                FontSize = 22,
                MinWidth = 72, // no jumping width when hours appear
                VerticalAlignment = VerticalAlignment.Center
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(_mark);
            row.Children.Add(_time);
            Body = row;

            Update(new LiveState());
        }

        /// <summary>Time shown for a live state: the elapsed time of the current activity, 00:00 while idle.</summary>
        public static string TextFor(LiveState state) =>
            state != null && state.IsTracking ? ActivityInfo.FormatDuration((int)state.Elapsed.TotalSeconds) : "00:00";

        public void Update(LiveState state)
        {
            _time.Text = TextFor(state);

            bool active = state != null && state.IsTracking && !state.IsPaused;
            if (active) _time.SetResourceReference(TextBlock.ForegroundProperty, "TextBrush");
            else _time.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");

            if (state?.Activity != null) _mark.Fill = state.Activity.TypeBrush;
            else _mark.Fill = ActivityPalette.BrushOf(ActivityPalette.Other);
            ToolTip = state?.Activity != null ? $"{state.Activity.Area} · {state.Activity.TypeName}{(state.IsPaused ? " (paused)" : "")}" : "Waiting for activity";
        }
    }
}
