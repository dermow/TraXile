using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>One tag in the list of the Tags page.</summary>
    public sealed class TagRow
    {
        public TagInfo Tag { get; set; }
        public int Usage { get; set; }

        public string Id => Tag.Id;
        public string Name => Tag.Name;
        public bool IsDefault => Tag.IsDefault;
        public bool ShowInHistory => Tag.ShowInHistory;
        public bool SoundEnabled => Tag.SoundEnabled;
        public Brush Background => Tag.Background;
        public Brush Foreground => Tag.Foreground;
        public string UsageText => Usage == 1 ? "1 activity" : $"{Usage:N0} activities";
    }

    /// <summary>
    /// Tags page: list all tags, add custom tags, rename, "show in history" and delete
    /// (classic: Settings → Tags, Main.cs AddTag / UpdateTag / DeleteTag).
    /// </summary>
    public sealed class TagsViewModel : ObservableObject
    {
        /// <summary>Characters the classic app rejects in IDs, plus ' (SQL) and | (separator in the activity log).</summary>
        private static readonly char[] InvalidIdChars = { '=', ',', ';', ' ', '\'', '|' };

        private readonly ITrackingSource _source;

        public TagsViewModel(ITrackingSource source)
        {
            _source = source;
            AddCommand = new RelayCommand(() => Add(out _));
            SaveCommand = new RelayCommand(() => Save(out _), () => HasChanges);
            RevertCommand = new RelayCommand(LoadEditor, () => HasChanges);
            Refresh();
        }

        public bool IsDemo => _source.IsDemo;

        /// <summary>All tags: custom tags first, then default tags, each by display name (grouped by IsDefault in the view).</summary>
        public ObservableCollection<TagRow> Tags { get; } = new ObservableCollection<TagRow>();

        public IReadOnlyList<TagRow> CustomTags => Tags.Where(r => !r.IsDefault).ToList();
        public IReadOnlyList<TagRow> DefaultTags => Tags.Where(r => r.IsDefault).ToList();

        public bool HasCustomTags => Tags.Any(r => !r.IsDefault);

        /// <summary>Re-reads tags and usage (on navigation and after every change).</summary>
        public void Refresh()
        {
            string selectedId = Selected?.Id;
            _refreshing = true; // the list pushes null into Selected while it is rebuilt

            var usage = new Dictionary<string, int>();
            foreach (ActivityInfo a in _source.GetHistory())
            {
                foreach (TagInfo t in a.Tags) usage[t.Id] = usage.TryGetValue(t.Id, out int n) ? n + 1 : 1;
            }

            List<TagRow> rows = _source.GetTags()
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .Select(t => new TagRow { Tag = t, Usage = usage.TryGetValue(t.Id, out int n) ? n : 0 })
                .ToList();

            Tags.Clear();
            foreach (TagRow r in rows.Where(r => !r.IsDefault).Concat(rows.Where(r => r.IsDefault))) Tags.Add(r);
            _refreshing = false;

            OnPropertyChanged(nameof(HasCustomTags));

            // Keep the selection; the editor keeps unsaved input for the same tag
            TagRow again = rows.FirstOrDefault(r => r.Id == selectedId);
            _selected = again;
            OnPropertyChanged(nameof(Selected));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(SelectedKind));
            if (again == null) LoadEditor();
            else NotifyEditor();
        }

        private IEnumerable<TagRow> AllRows => Tags;

        private bool NameTaken(string name, string exceptId) =>
            AllRows.Any(r => r.Id != exceptId && string.Equals(r.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

        // ------------------------------------------------------------------
        // Add custom tag
        // ------------------------------------------------------------------

        public IRelayCommand AddCommand { get; }

        private string _newId = "";
        /// <summary>ID used in chat commands (trax::tag &lt;id&gt;) and stored with the activities.</summary>
        public string NewId
        {
            get => _newId;
            set
            {
                string old = _newId;
                if (!SetProperty(ref _newId, value ?? "")) return;
                // Like the classic form: the display name follows the ID until it is edited
                if (string.IsNullOrEmpty(_newName) || _newName == old) NewName = _newId;
                AddError = null;
            }
        }

        private string _newName = "";
        public string NewName
        {
            get => _newName;
            set { if (SetProperty(ref _newName, value ?? "")) AddError = null; }
        }

        private string _addError;
        public string AddError
        {
            get => _addError;
            private set { if (SetProperty(ref _addError, value)) OnPropertyChanged(nameof(HasAddError)); }
        }

        public bool HasAddError => !string.IsNullOrEmpty(AddError);

        public static string ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Enter an ID.";
            if (id.IndexOfAny(InvalidIdChars) >= 0) return "The ID must not contain spaces or any of = , ; ' |";
            return null;
        }

        public bool Add(out string error)
        {
            string id = NewId.Trim();
            string name = string.IsNullOrWhiteSpace(NewName) ? id : NewName.Trim();

            error = ValidateId(id);
            if (error == null && AllRows.Any(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase))) error = $"A tag with the ID \"{id}\" already exists.";
            if (error == null && NameTaken(name, null)) error = $"A tag named \"{name}\" already exists.";
            AddError = error;
            if (error != null) return false;

            _source.CreateTag(id, name);
            _newId = _newName = "";
            OnPropertyChanged(nameof(NewId));
            OnPropertyChanged(nameof(NewName));
            Refresh();
            Selected = AllRows.FirstOrDefault(r => r.Id == id);
            return true;
        }

        // ------------------------------------------------------------------
        // Edit selected tag
        // ------------------------------------------------------------------

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand RevertCommand { get; }

        private bool _refreshing;
        private TagRow _selected;
        public TagRow Selected
        {
            get => _selected;
            set
            {
                if (_refreshing) return;
                if (!SetProperty(ref _selected, value)) return;
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(SelectedKind));
                LoadEditor();
            }
        }

        public bool HasSelection => Selected != null;
        public bool CanDelete => Selected != null && !Selected.IsDefault;
        public string SelectedKind => Selected == null ? "" : Selected.IsDefault ? "default tag" : "custom tag";

        private string _editName = "";
        public string EditName
        {
            get => _editName;
            set { if (SetProperty(ref _editName, value ?? "")) NotifyEditor(); }
        }

        private bool _editShowInHistory;
        public bool EditShowInHistory
        {
            get => _editShowInHistory;
            set { if (SetProperty(ref _editShowInHistory, value)) NotifyEditor(); }
        }

        public string EditError =>
            Selected == null ? null :
            string.IsNullOrWhiteSpace(EditName) ? "Enter a display name." :
            NameTaken(EditName, Selected.Id) ? $"A tag named \"{EditName.Trim()}\" already exists." : null;

        public bool HasEditError => EditError != null;

        public bool HasChanges => Selected != null &&
            (EditName.Trim() != Selected.Name || EditShowInHistory != Selected.ShowInHistory);

        /// <summary>Applies the selected tag to the editor (also discards unsaved input).</summary>
        private void LoadEditor()
        {
            _editName = Selected?.Name ?? "";
            _editShowInHistory = Selected?.ShowInHistory ?? false;
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditShowInHistory));
            NotifyEditor();
        }

        private void NotifyEditor()
        {
            OnPropertyChanged(nameof(EditError));
            OnPropertyChanged(nameof(HasEditError));
            OnPropertyChanged(nameof(HasChanges));
            SaveCommand?.NotifyCanExecuteChanged();
            RevertCommand?.NotifyCanExecuteChanged();
        }

        public bool Save(out string error)
        {
            error = Selected == null ? "Select a tag first." : EditError;
            if (error != null) return false;
            if (!HasChanges) return true;

            TagInfo t = Selected.Tag;
            _source.UpdateTag(new TagInfo
            {
                Id = t.Id,
                Name = EditName.Trim(),
                ShowInHistory = EditShowInHistory,
                SoundEnabled = t.SoundEnabled,
                SoundId = t.SoundId,
            });
            Refresh();
            LoadEditor();
            return true;
        }

        /// <summary>Deletes the selected custom tag (the page asks first).</summary>
        public bool DeleteSelected()
        {
            if (!CanDelete) return false;
            _source.DeleteTag(Selected.Id);
            Selected = null;
            Refresh();
            return true;
        }
    }
}
