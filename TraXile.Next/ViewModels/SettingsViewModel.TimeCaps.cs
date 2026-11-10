using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>Time cap of one activity type: activities longer than this count with the cap in statistics.</summary>
    public sealed class TimeCapRow : ObservableObject
    {
        public TimeCapRow(ACTIVITY_TYPES type, int seconds)
        {
            Type = type;
            Saved = seconds;
            _text = seconds.ToString();
        }

        public ACTIVITY_TYPES Type { get; }
        public string Name => ActivityPalette.DisplayName(Type);
        public Brush Brush => ActivityPalette.Brush(Type);

        /// <summary>Value currently stored in config.xml.</summary>
        public int Saved { get; internal set; }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (!SetProperty(ref _text, value)) return;
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(DurationText));
            }
        }

        public bool IsValid => SettingsViewModel.TryParseSeconds(Text, 1, out _);
        public int Seconds => SettingsViewModel.TryParseSeconds(Text, 1, out int s) ? s : Saved;
        public bool IsChanged => IsValid && Seconds != Saved;
        public string DurationText => IsValid ? ActivityInfo.FormatDuration(Seconds) : "invalid";
    }

    /// <summary>
    /// Time caps (classic: SaveTimeCaps). Stored as TimeCapMinimum and TimeCap&lt;Type&gt; in seconds.
    /// Caps per type apply right away; the minimum time is used by the core when it starts, so it needs a restart.
    /// </summary>
    public sealed partial class SettingsViewModel
    {
        public const string TimeCapMinimumKey = "TimeCapMinimum";
        public const int DefaultTimeCap = 3600;
        public const int DefaultMinimum = 10;

        /// <summary>Config key of the cap of a type, e.g. MAVEN_FIGHT -> TimeCapMaven_fight (same as the classic app).</summary>
        public static string TimeCapKey(ACTIVITY_TYPES type) => "TimeCap" + TrX_Helpers.CapitalFirstLetter(type.ToString());

        public static bool TryParseSeconds(string text, int min, out int seconds) =>
            int.TryParse(text?.Trim(), out seconds) && seconds >= min && seconds <= 24 * 3600;

        public IReadOnlyList<TimeCapRow> TimeCaps { get; private set; } = Array.Empty<TimeCapRow>();

        private int _savedMinimum;

        private string _minimumText = DefaultMinimum.ToString();
        public string MinimumText
        {
            get => _minimumText;
            set { if (SetProperty(ref _minimumText, value)) OnPropertyChanged(nameof(IsMinimumValid)); }
        }

        public bool IsMinimumValid => TryParseSeconds(MinimumText, 0, out _);

        private bool _needsRestart;
        /// <summary>The minimum time changed: it applies after a restart.</summary>
        public bool NeedsRestart { get => _needsRestart; private set => SetProperty(ref _needsRestart, value); }

        private string _timeCapStatus;
        public string TimeCapStatus { get => _timeCapStatus; private set => SetProperty(ref _timeCapStatus, value); }

        partial void InitTimeCaps()
        {
            _savedMinimum = int.TryParse(_setup.ReadSetting(TimeCapMinimumKey), out int m) ? m : DefaultMinimum;
            _minimumText = _savedMinimum.ToString();

            TimeCaps = Enum.GetValues(typeof(ACTIVITY_TYPES)).Cast<ACTIVITY_TYPES>()
                .Select(t => new TimeCapRow(t, int.TryParse(_setup.ReadSetting(TimeCapKey(t)), out int c) ? c : DefaultTimeCap))
                .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Validates and stores all time caps; changed caps are handed to the running core.</summary>
        public bool SaveTimeCaps(out string error)
        {
            error = null;
            if (!CanWrite) { error = "Settings are read only right now."; return false; }
            if (!IsMinimumValid) { error = "The minimum time must be a number of seconds (0 or more)."; return false; }
            TimeCapRow invalid = TimeCaps.FirstOrDefault(r => !r.IsValid);
            if (invalid != null) { error = $"{invalid.Name}: the time cap must be a number of seconds (1 to 86400)."; return false; }

            int minimum = int.Parse(MinimumText.Trim());
            if (minimum != _savedMinimum)
            {
                _setup.WriteSetting(TimeCapMinimumKey, minimum.ToString());
                _savedMinimum = minimum;
                NeedsRestart = true;
            }

            int changed = 0;
            foreach (TimeCapRow row in TimeCaps.Where(r => r.IsChanged))
            {
                _setup.WriteSetting(TimeCapKey(row.Type), row.Seconds.ToString());
                row.Saved = row.Seconds;
                Core?.SetTimeCap(row.Type, row.Seconds);
                changed++;
            }

            TimeCapStatus = NeedsRestart
                ? "Saved. The minimum time applies after a restart."
                : changed > 0 ? $"Saved {changed} time cap{(changed == 1 ? "" : "s")}." : "Nothing changed.";
            return true;
        }

        public void RestartNow() => _restart();
    }
}
