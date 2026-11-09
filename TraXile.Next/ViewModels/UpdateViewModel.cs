using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Update state of the app: checks for a new version at start and then periodically (classic timerUpdateCheck),
    /// shows the "update available" hint in the title bar and starts the updater.
    /// </summary>
    public sealed class UpdateViewModel : ObservableObject, IDisposable
    {
        /// <summary>Time between two background checks.</summary>
        public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        private readonly Func<Task<UpdateInfo>> _check;
        private readonly Func<string, string> _startUpdater; // version -> error (null = started)
        private DispatcherTimer _timer;

        /// <param name="check">Downloads and parses the version info; null if the check failed.</param>
        /// <param name="startUpdater">Starts the updater for a version and returns an error text, or null on success.</param>
        public UpdateViewModel(string currentVersion, Func<Task<UpdateInfo>> check, Func<string, string> startUpdater)
        {
            CurrentVersion = currentVersion;
            _check = check;
            _startUpdater = startUpdater;
        }

        public string CurrentVersion { get; }

        private UpdateInfo _info;
        /// <summary>Result of the last successful check (null before / if offline).</summary>
        public UpdateInfo Info
        {
            get => _info;
            private set
            {
                if (!SetProperty(ref _info, value)) return;
                OnPropertyChanged(nameof(IsUpdateAvailable));
                OnPropertyChanged(nameof(LatestVersion));
                OnPropertyChanged(nameof(Changes));
                OnPropertyChanged(nameof(HasChanges));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(HintText));
            }
        }

        public bool IsUpdateAvailable => Info?.IsAvailable == true;
        public string LatestVersion => Info?.LatestVersion ?? "";
        public IReadOnlyList<string> Changes => Info?.Changes ?? Array.Empty<string>();
        public bool HasChanges => Changes.Count > 0;

        /// <summary>Short text for the title bar button.</summary>
        public string HintText => IsUpdateAvailable ? $"Update {LatestVersion}" : "";

        private bool _isChecking;
        public bool IsChecking
        {
            get => _isChecking;
            private set { if (SetProperty(ref _isChecking, value)) OnPropertyChanged(nameof(StatusText)); }
        }

        private bool _lastCheckFailed;

        /// <summary>One line about the update state (Help page).</summary>
        public string StatusText
        {
            get
            {
                if (IsChecking) return "Checking for updates…";
                if (IsUpdateAvailable) return $"TraXile {LatestVersion} is available.";
                if (Info != null && !_lastCheckFailed) return "You are up to date.";
                if (_lastCheckFailed) return "Could not check for updates (offline?).";
                return "Not checked yet.";
            }
        }

        /// <summary>Checks once; keeps the last result when the check fails (null is returned then).</summary>
        public async Task<UpdateInfo> CheckAsync()
        {
            if (IsChecking) return Info;
            IsChecking = true;
            try
            {
                UpdateInfo result = await _check();
                _lastCheckFailed = result == null;
                if (result != null) Info = result;
                OnPropertyChanged(nameof(StatusText));
                return result;
            }
            finally
            {
                IsChecking = false;
            }
        }

        /// <summary>
        /// Checks again every <see cref="CheckInterval"/>. Like the classic app these checks only update the
        /// title bar hint; the update dialog opens at start or on request.
        /// </summary>
        public void StartPeriodicCheck()
        {
            if (_timer != null) return;
            _timer = new DispatcherTimer { Interval = CheckInterval };
            _timer.Tick += async (s, e) => await CheckAsync();
            _timer.Start();
        }

        /// <summary>Shows a known update without checking (screenshots).</summary>
        public void SetInfo(UpdateInfo info) => Info = info;

        /// <summary>Starts the updater for the latest version; returns an error text or null when it was started.</summary>
        public string Install()
        {
            if (!IsUpdateAvailable) return "No update available.";
            return _startUpdater(LatestVersion);
        }

        public void Dispose() => _timer?.Stop();
    }
}
