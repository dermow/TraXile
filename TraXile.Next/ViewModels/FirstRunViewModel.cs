using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>One Client.txt the user can pick.</summary>
    public sealed class ClientTxtCandidate : ObservableObject
    {
        public ClientTxtCandidate(string path, string source)
        {
            Path = path;
            Source = source;
        }

        public string Path { get; }

        /// <summary>"Steam", "Standalone" or "Selected manually".</summary>
        public string Source { get; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    /// <summary>
    /// First start without a configured Client.txt: offer the detected installations and a manual pick,
    /// then write the choice to config.xml (compatible with the classic app).
    /// </summary>
    public sealed class FirstRunViewModel : ObservableObject
    {
        private readonly AppSetup _setup;

        public FirstRunViewModel(AppSetup setup, IEnumerable<string> detected)
        {
            _setup = setup;
            foreach (string path in detected ?? Enumerable.Empty<string>())
            {
                Add(new ClientTxtCandidate(path, SourceOf(path)));
            }
            if (Candidates.Count > 0) Candidates[0].IsSelected = true;
        }

        public ObservableCollection<ClientTxtCandidate> Candidates { get; } = new ObservableCollection<ClientTxtCandidate>();

        public bool HasCandidates => Candidates.Count > 0;

        public ClientTxtCandidate Selected => Candidates.FirstOrDefault(c => c.IsSelected);

        public bool CanStart => Selected != null && AppSetup.IsValidClientTxt(Selected.Path);

        private string _error;
        public string Error { get => _error; private set { SetProperty(ref _error, value); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(Error);

        /// <summary>Adds a manually selected file (or selects it if it is already in the list).</summary>
        public bool AddManual(string path)
        {
            if (!AppSetup.IsValidClientTxt(path))
            {
                Error = "This file does not exist.";
                return false;
            }
            if (!string.Equals(System.IO.Path.GetFileName(path), "Client.txt", StringComparison.OrdinalIgnoreCase))
            {
                Error = "Please select the file Client.txt from the logs folder of Path of Exile.";
                return false;
            }

            Error = null;
            string full = System.IO.Path.GetFullPath(path);
            ClientTxtCandidate existing = Candidates.FirstOrDefault(c => string.Equals(c.Path, full, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                existing = new ClientTxtCandidate(full, "Selected manually");
                Add(existing);
            }
            Select(existing);
            return true;
        }

        public void Select(ClientTxtCandidate candidate)
        {
            foreach (ClientTxtCandidate c in Candidates) c.IsSelected = c == candidate;
        }

        /// <summary>Saves the selected Client.txt to config.xml.</summary>
        public bool Confirm()
        {
            if (!CanStart) return false;
            try
            {
                _setup.SaveClientTxtPath(Selected.Path);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Error = $"Could not save the setting: {ex.Message}";
                return false;
            }
        }

        private void Add(ClientTxtCandidate c)
        {
            c.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(ClientTxtCandidate.IsSelected)) return;
                OnPropertyChanged(nameof(Selected));
                OnPropertyChanged(nameof(CanStart));
            };
            Candidates.Add(c);
            OnPropertyChanged(nameof(HasCandidates));
        }

        private static string SourceOf(string path) =>
            path.IndexOf(@"\steamapps\", StringComparison.OrdinalIgnoreCase) >= 0 ? "Steam" : "Standalone";
    }
}
