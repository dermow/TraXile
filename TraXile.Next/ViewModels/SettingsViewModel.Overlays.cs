using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Overlay settings (classic: Settings → Overlays). Written only to the keys the apps actually read:
    /// overlay.general.opacity, overlay.{stopwatch|tags}.showDefault and overlay.tags.tag1..3. The classic app
    /// wrote overlay.stopwatch.default and overlay.stopwatch.tag1..3 by mistake; those are never written here.
    /// </summary>
    public sealed partial class SettingsViewModel
    {
        private IOverlayHost _overlays;

        public IRelayCommand ResetOverlayPositionsCommand { get; private set; }

        partial void InitOverlays()
        {
            _overlayOpacity = OverlaySettings.ReadOpacity(_setup);
            _stopwatchShowDefault = OverlaySettings.ReadShowDefault(_setup, OverlaySettings.Stopwatch);
            _tagsShowDefault = OverlaySettings.ReadShowDefault(_setup, OverlaySettings.Tags);
            ResetOverlayPositionsCommand = new RelayCommand(() => _overlays?.ResetPositions(), () => CanWrite);
            LoadTagSlotOptions();
        }

        private int _overlayOpacity;
        /// <summary>Opacity of all overlays in percent.</summary>
        public int OverlayOpacity
        {
            get => _overlayOpacity;
            set
            {
                value = Math.Max(OverlaySettings.MinOpacity, Math.Min(100, value));
                if (!SetProperty(ref _overlayOpacity, value)) return;
                if (CanWrite) _setup.WriteSetting(OverlaySettings.OpacityKey, value.ToString());
                _overlays?.ReloadSettings();
            }
        }

        public int MinOverlayOpacity => OverlaySettings.MinOpacity;

        private bool _stopwatchShowDefault;
        /// <summary>Open the stopwatch overlay when TraXile starts.</summary>
        public bool StopwatchShowDefault
        {
            get => _stopwatchShowDefault;
            set
            {
                if (SetProperty(ref _stopwatchShowDefault, value) && CanWrite)
                    _setup.WriteSetting(OverlaySettings.ShowDefaultKey(OverlaySettings.Stopwatch), value.ToString());
            }
        }

        private bool _tagsShowDefault;
        /// <summary>Open the tags overlay when TraXile starts.</summary>
        public bool TagsShowDefault
        {
            get => _tagsShowDefault;
            set
            {
                if (SetProperty(ref _tagsShowDefault, value) && CanWrite)
                    _setup.WriteSetting(OverlaySettings.ShowDefaultKey(OverlaySettings.Tags), value.ToString());
            }
        }

        // ------------------------------------------------------------------
        // Tag slots of the tags overlay
        // ------------------------------------------------------------------

        /// <summary>"None" plus the display names of all tags (the classic app stores display names).</summary>
        public IReadOnlyList<string> TagSlotOptions { get; private set; } = Array.Empty<string>();

        private readonly string[] _tagSlots = new string[3];

        public string TagSlot1 { get => _tagSlots[0]; set => SetTagSlot(0, value); }
        public string TagSlot2 { get => _tagSlots[1]; set => SetTagSlot(1, value); }
        public string TagSlot3 { get => _tagSlots[2]; set => SetTagSlot(2, value); }

        /// <summary>Re-reads the tags (they may have been added or renamed) and the stored slots.</summary>
        public void LoadTagSlotOptions()
        {
            IReadOnlyList<TagInfo> tags = _source.GetTags();
            var options = new List<string> { OverlaySettings.EmptySlot };
            options.AddRange(tags.Select(t => t.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

            string[] stored = OverlaySettings.ReadTagSlots(_setup);
            for (int i = 0; i < 3; i++)
            {
                // Stored as display name or ID (classic defaults are IDs); show the tag's current name
                TagInfo tag = OverlaySettings.ResolveTag(stored[i], tags);
                string shown = tag?.Name ?? stored[i] ?? OverlaySettings.EmptySlot;
                if (!options.Contains(shown)) options.Add(shown); // unknown tag: keep it selectable as it is
                _tagSlots[i] = shown;
            }

            TagSlotOptions = options;
            OnPropertyChanged(nameof(TagSlotOptions));
            OnPropertyChanged(nameof(TagSlot1));
            OnPropertyChanged(nameof(TagSlot2));
            OnPropertyChanged(nameof(TagSlot3));
        }

        private void SetTagSlot(int index, string value)
        {
            // The combo box pushes null while its options are replaced
            if (value == null || value == _tagSlots[index]) return;

            _tagSlots[index] = value;
            OnPropertyChanged("TagSlot" + (index + 1));
            if (CanWrite) _setup.WriteSetting(OverlaySettings.TagSlotKey(index + 1), value);
            _overlays?.ReloadSettings();
        }
    }
}
