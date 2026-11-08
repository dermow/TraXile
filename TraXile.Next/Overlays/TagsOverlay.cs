using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TraXile.Next.Services;

namespace TraXile.Next.Overlays
{
    /// <summary>One tag slot of the tags overlay.</summary>
    public sealed class TagSlotState
    {
        public TagInfo Tag { get; set; }
        public bool IsActive { get; set; }
        public string Text => Tag?.Name ?? "—";
    }

    /// <summary>
    /// Tags overlay (classic: OverlayTags): the time of the current activity and of the previous one of the
    /// same type, and the three configured tags (overlay.tags.tag1..3), highlighted while the current
    /// activity has them.
    /// </summary>
    public sealed class TagsOverlay : OverlayWindow
    {
        private const double ChipWidth = 92;

        private readonly Rectangle _mark;
        private readonly TextBlock _current;
        private readonly TextBlock _previous;
        private readonly Border[] _chips = new Border[3];
        private readonly TextBlock[] _chipTexts = new TextBlock[3];

        public TagsOverlay(AppSetup setup, bool canWrite) : base(OverlaySettings.Tags, setup, canWrite)
        {
            Title = "TraXile tags";

            var row = new StackPanel { Orientation = Orientation.Horizontal };

            _mark = new Rectangle
            {
                Width = 7,
                Height = 7,
                Margin = new Thickness(0, 0, 9, 0),
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransform = new RotateTransform(45, 3.5, 3.5)
            };
            row.Children.Add(_mark);

            _current = new TextBlock
            {
                Style = (Style)Application.Current.FindResource("Metric"),
                FontSize = 18,
                MinWidth = 54,
                VerticalAlignment = VerticalAlignment.Center
            };
            row.Children.Add(_current);

            var prevLabel = new TextBlock
            {
                Text = "PREV",
                Style = (Style)Application.Current.FindResource("Label"),
                FontSize = 10,
                Margin = new Thickness(12, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            prevLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
            row.Children.Add(prevLabel);

            _previous = new TextBlock
            {
                Style = (Style)Application.Current.FindResource("Num"),
                FontSize = 13,
                MinWidth = 40,
                VerticalAlignment = VerticalAlignment.Center
            };
            _previous.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
            row.Children.Add(_previous);

            for (int i = 0; i < 3; i++)
            {
                _chipTexts[i] = new TextBlock
                {
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _chips[i] = new Border
                {
                    Width = ChipWidth,
                    Margin = new Thickness(i == 0 ? 12 : 4, 0, 0, 0),
                    Padding = new Thickness(6, 2, 6, 2),
                    BorderThickness = new Thickness(1),
                    CornerRadius = (CornerRadius)Application.Current.FindResource("ControlRadius"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = _chipTexts[i]
                };
                row.Children.Add(_chips[i]);
            }

            Body = row;
            Update(new LiveState(), new TagSlotState[3]);
        }

        /// <summary>The three slots for the current live state; a slot is active while the current activity has its tag.</summary>
        public static TagSlotState[] SlotsFor(LiveState state, IReadOnlyList<TagInfo> slotTags)
        {
            var active = new HashSet<string>(state != null && state.IsTracking ? state.ActiveTags.Select(t => t.Id) : Enumerable.Empty<string>());
            return Enumerable.Range(0, 3).Select(i =>
            {
                TagInfo tag = slotTags != null && i < slotTags.Count ? slotTags[i] : null;
                return new TagSlotState { Tag = tag, IsActive = tag != null && active.Contains(tag.Id) };
            }).ToArray();
        }

        public void Update(LiveState state, IReadOnlyList<TagSlotState> slots)
        {
            bool tracking = state != null && state.IsTracking;
            _current.Text = StopwatchOverlay.TextFor(state);
            _current.SetResourceReference(TextBlock.ForegroundProperty, tracking && !state.IsPaused ? "TextBrush" : "TextMutedBrush");
            _mark.Fill = state?.Activity != null ? state.Activity.TypeBrush : ActivityPalette.BrushOf(ActivityPalette.Other);

            ActivityInfo prev = tracking ? state.PreviousActivity : null;
            _previous.Text = prev != null ? prev.Duration : "—";
            _previous.ToolTip = prev != null ? $"Previous {prev.TypeName.ToLowerInvariant()}: {prev.Area}" : null;

            for (int i = 0; i < 3; i++)
            {
                TagSlotState slot = slots != null && i < slots.Count ? slots[i] : null;
                _chipTexts[i].Text = slot?.Text ?? "—";
                if (slot != null && slot.IsActive)
                {
                    _chips[i].Background = slot.Tag.Background;
                    _chips[i].BorderBrush = slot.Tag.Background;
                    _chipTexts[i].Foreground = slot.Tag.Foreground;
                }
                else
                {
                    _chips[i].Background = Brushes.Transparent;
                    _chips[i].SetResourceReference(Border.BorderBrushProperty, "BorderStrongBrush");
                    _chipTexts[i].SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
                }
            }
        }
    }
}
