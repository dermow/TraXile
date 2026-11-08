using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Loading state while the core reads Client.txt after start (classic app: LoadScreen).
    /// Polls the source until the history is complete.
    /// </summary>
    public sealed class LoadingViewModel : ObservableObject, IDisposable
    {
        private readonly ITrackingSource _source;
        private readonly DispatcherTimer _timer;

        public LoadingViewModel(ITrackingSource source)
        {
            _source = source;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _timer.Tick += (s, e) => Update();
            Update();
            if (IsLoading) _timer.Start();
        }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

        private double _progress;
        public double Progress { get => _progress; private set => SetProperty(ref _progress, value); }

        public string PercentText => $"{Progress:P0}";

        public void Update()
        {
            IsLoading = _source.IsLoading;
            Progress = _source.LoadProgress;
            OnPropertyChanged(nameof(PercentText));
            if (!IsLoading) _timer?.Stop();
        }

        public void Dispose() => _timer.Stop();
    }
}
