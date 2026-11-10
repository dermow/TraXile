using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    /// Tags overlay (classic: OverlayTags): the three configured tags (overlay.tags.tag1..3), highlighted
    /// while the current activity has them. Shows no times; the time lives in <see cref="StopwatchOverlay"/>.
    /// </summary>
    public sealed class TagsOverlay : OverlayWindow
    {
        private const double ChipWidth = 92;

        private readonly Border[] _chips = new Border[3];
        private readonly TextBlock[] _chipTexts = new TextBlock[3];

        public TagsOverlay(AppSetup setup, bool canWrite) : base(OverlaySettings.Tags, setup, canWrite)
        {
            Title = "TraXile tags";

            var row = new StackPanel { Orientation = Orientation.Horizontal };

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
                    Margin = new Thickness(i == 0 ? 0 : 4, 0, 0, 0),
                    Padding = new Thickness(6, 2, 6, 2),
                    BorderThickness = new Thickness(1),
                    CornerRadius = (CornerRadius)Application.Current.FindResource("ControlRadius"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = _chipTexts[i]
                };
                row.Children.Add(_chips[i]);
            }

            Body = row;
            Update(new TagSlotState[3]);
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

        public void Update(IReadOnlyList<TagSlotState> slots)
        {
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
