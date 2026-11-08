using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Sound notification of the selected tag (classic: sound check box, sound picker, play,
    /// open audio folder and reload sounds on the tags tab). Saved together with the other tag fields.
    /// </summary>
    public sealed partial class TagsViewModel
    {
        private readonly ITagSoundPlayer _player;

        public TagSoundLibrary Sounds { get; }

        /// <summary>Files in the audio folder, plus the stored sound of the selected tag if its file is gone.</summary>
        public ObservableCollection<string> SoundFiles { get; } = new ObservableCollection<string>();

        public IRelayCommand PlaySoundCommand { get; private set; }
        public IRelayCommand ReloadSoundsCommand { get; private set; }
        public IRelayCommand ResetSoundCommand { get; private set; }

        private void InitSound()
        {
            PlaySoundCommand = new RelayCommand(PlaySound, () => TagSoundLibrary.IsSet(EditSoundId));
            ReloadSoundsCommand = new RelayCommand(ReloadSounds);
            ResetSoundCommand = new RelayCommand(ResetSound);
            ReloadSounds();
        }

        private bool _editSoundEnabled;
        public bool EditSoundEnabled
        {
            get => _editSoundEnabled;
            set
            {
                if (!SetProperty(ref _editSoundEnabled, value)) return;
                // Switching it on without a sound picks the first one, like an initial selection
                if (value && !TagSoundLibrary.IsSet(EditSoundId)) EditSoundId = SoundFiles.FirstOrDefault();
                NotifyEditor();
            }
        }

        private bool _reloadingSounds;
        private string _editSoundId;
        public string EditSoundId
        {
            get => _editSoundId;
            set
            {
                if (_reloadingSounds) return; // the combo box pushes null while the list is rebuilt
                if (!SetProperty(ref _editSoundId, value)) return;
                PlaySoundCommand?.NotifyCanExecuteChanged();
                NotifyEditor();
            }
        }

        public bool HasSoundFiles => SoundFiles.Count > 0;

        /// <summary>What is written to tx_tags.audio_file_name.</summary>
        private string StoredSoundId => TagSoundLibrary.IsSet(EditSoundId) ? EditSoundId : TagSoundLibrary.NoSound;

        private string SoundError =>
            EditSoundEnabled && !TagSoundLibrary.IsSet(EditSoundId) ? "Pick a sound or turn the sound off." : null;

        private bool SoundChanged => Selected != null &&
            (EditSoundEnabled != Selected.Tag.SoundEnabled ||
             StoredSoundId != (TagSoundLibrary.IsSet(Selected.Tag.SoundId) ? Selected.Tag.SoundId : TagSoundLibrary.NoSound));

        private void LoadSoundEditor()
        {
            _editSoundEnabled = Selected?.Tag.SoundEnabled ?? false;
            _editSoundId = TagSoundLibrary.IsSet(Selected?.Tag.SoundId) ? Selected.Tag.SoundId : null;
            EnsureListed(_editSoundId);
            OnPropertyChanged(nameof(EditSoundEnabled));
            OnPropertyChanged(nameof(EditSoundId));
            PlaySoundCommand?.NotifyCanExecuteChanged();
        }

        private void EnsureListed(string soundId)
        {
            if (TagSoundLibrary.IsSet(soundId) && !SoundFiles.Contains(soundId)) SoundFiles.Add(soundId);
        }

        private void PlaySound()
        {
            if (TagSoundLibrary.IsSet(EditSoundId)) _player?.Play(EditSoundId);
        }

        /// <summary>Re-reads the audio folder (after the user added files); keeps the current choice.</summary>
        public void ReloadSounds()
        {
            string keep = _editSoundId;
            _reloadingSounds = true;
            SoundFiles.Clear();
            foreach (string f in Sounds.List()) SoundFiles.Add(f);
            EnsureListed(keep);
            _editSoundId = keep;
            _reloadingSounds = false;
            OnPropertyChanged(nameof(EditSoundId)); // the combo box lost its item while the list was rebuilt
            OnPropertyChanged(nameof(HasSoundFiles));
        }

        /// <summary>Back to the default of a new tag: no sound.</summary>
        private void ResetSound()
        {
            EditSoundEnabled = false;
            EditSoundId = null;
        }
    }
}
