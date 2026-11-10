using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Sound per tag on the Tags page.</summary>
    public class TagSoundSettingsTests : IDisposable
    {
        private sealed class RecordingPlayer : ITagSoundPlayer
        {
            public List<string> Played { get; } = new List<string>();
            public void Play(string soundId) => Played.Add(soundId);
        }

        private readonly string _audio = Path.Combine(Path.GetTempPath(), "traxile-audio-" + Guid.NewGuid().ToString("N"));
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly RecordingPlayer _player = new RecordingPlayer();
        private readonly TagsViewModel _vm;

        public TagSoundSettingsTests()
        {
            Directory.CreateDirectory(_audio);
            foreach (string f in new[] { "default_1.mp3", "default_3.mp3", "Bell.wav" }) File.WriteAllText(Path.Combine(_audio, f), "");
            _vm = new TagsViewModel(_source, _player, new TagSoundLibrary(_audio));
        }

        public void Dispose()
        {
            try { Directory.Delete(_audio, recursive: true); } catch (IOException) { }
        }

        private TagInfo Stored(string id) => _source.GetTags().Single(t => t.Id == id);
        private void Select(string id) => _vm.Selected = _vm.Tags.Single(r => r.Id == id);

        [Fact]
        public void Lists_the_files_of_the_audio_folder()
        {
            Assert.Equal(new[] { "Bell.wav", "default_1.mp3", "default_3.mp3" }, _vm.SoundFiles);
        }

        [Fact]
        public void Shows_the_stored_sound_of_the_selected_tag()
        {
            Select("bricked");

            Assert.True(_vm.EditSoundEnabled);
            Assert.Equal("default_3.mp3", _vm.EditSoundId);
            Assert.False(_vm.HasChanges);
        }

        [Fact]
        public void Turning_the_sound_on_picks_a_file_and_saves_it()
        {
            Select("party");
            Assert.False(_vm.EditSoundEnabled);

            _vm.EditSoundEnabled = true;
            Assert.Equal("Bell.wav", _vm.EditSoundId);
            _vm.EditSoundId = "default_1.mp3";

            Assert.True(_vm.Save(out string error), error);
            Assert.True(Stored("party").SoundEnabled);
            Assert.Equal("default_1.mp3", Stored("party").SoundId);
            Assert.True(_vm.Tags.Single(r => r.Id == "party").SoundEnabled); // list icon
        }

        [Fact]
        public void Play_previews_the_chosen_sound()
        {
            Select("bricked");
            _vm.EditSoundId = "Bell.wav";

            _vm.PlaySoundCommand.Execute(null);

            Assert.Equal(new[] { "Bell.wav" }, _player.Played);
        }

        [Fact]
        public void Reset_turns_the_sound_off_and_stores_none()
        {
            Select("bricked");

            _vm.ResetSoundCommand.Execute(null);
            Assert.True(_vm.HasChanges);
            Assert.True(_vm.Save(out _));

            Assert.False(Stored("bricked").SoundEnabled);
            Assert.Equal(TagSoundLibrary.NoSound, Stored("bricked").SoundId);
            Assert.False(_vm.PlaySoundCommand.CanExecute(null));
        }

        [Fact]
        public void Reload_finds_new_files_and_keeps_the_choice()
        {
            Select("bricked");
            File.WriteAllText(Path.Combine(_audio, "my_own.mp3"), "");

            _vm.ReloadSoundsCommand.Execute(null);

            Assert.Contains("my_own.mp3", _vm.SoundFiles);
            Assert.Equal("default_3.mp3", _vm.EditSoundId);
            Assert.False(_vm.HasChanges);
        }

        [Fact]
        public void A_stored_sound_whose_file_is_gone_stays_selectable()
        {
            File.Delete(Path.Combine(_audio, "default_3.mp3"));
            _vm.ReloadSoundsCommand.Execute(null);

            Select("bricked");

            Assert.Equal("default_3.mp3", _vm.EditSoundId);
            Assert.Contains("default_3.mp3", _vm.SoundFiles);
        }

        [Fact]
        public void Sound_on_without_any_file_cannot_be_saved()
        {
            var empty = new TagsViewModel(_source, _player, new TagSoundLibrary(Path.Combine(_audio, "missing")));
            empty.Selected = empty.Tags.Single(r => r.Id == "party");

            empty.EditSoundEnabled = true;

            Assert.False(empty.HasSoundFiles);
            Assert.True(empty.HasEditError);
            Assert.False(empty.Save(out _));
        }
    }
}
