using System.Linq;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    public class TagsViewModelTests
    {
        private readonly DemoTrackingSource _source = new DemoTrackingSource();
        private readonly TagsViewModel _vm;

        public TagsViewModelTests()
        {
            _vm = new TagsViewModel(_source);
        }

        private TagRow Row(string id) => _vm.Tags.SingleOrDefault(r => r.Id == id);

        [Fact]
        public void Lists_custom_tags_first_then_default_tags_with_usage()
        {
            Assert.Equal(_source.GetTags().Count, _vm.Tags.Count);
            int firstDefault = _vm.Tags.ToList().FindIndex(r => r.IsDefault);
            Assert.All(_vm.Tags.Take(firstDefault), r => Assert.False(r.IsDefault));
            Assert.All(_vm.Tags.Skip(firstDefault), r => Assert.True(r.IsDefault));
            Assert.Equal(new[] { "Bricked", "Juiced", "Party" }, _vm.CustomTags.Select(r => r.Name));

            int expected = _source.GetHistory().Count(a => a.Tags.Any(t => t.Id == "blight"));
            Assert.Equal(expected, Row("blight").Usage);
        }

        [Fact]
        public void Adds_a_custom_tag_and_selects_it()
        {
            _vm.NewId = "boss-rush";
            Assert.Equal("boss-rush", _vm.NewName); // display name follows the ID
            _vm.NewName = "Boss rush";

            Assert.True(_vm.Add(out string error), error);

            TagInfo created = _source.GetTags().Single(t => t.Id == "boss-rush");
            Assert.Equal("Boss rush", created.Name);
            Assert.False(created.IsDefault);
            Assert.Equal("boss-rush", _vm.Selected.Id);
            Assert.Equal("", _vm.NewId);
        }

        [Theory]
        [InlineData("", "Enter an ID")]
        [InlineData("two words", "must not contain")]
        [InlineData("a|b", "must not contain")]
        [InlineData("it's", "must not contain")]
        [InlineData("JUICED", "already exists")]
        public void Rejects_invalid_or_existing_ids(string id, string message)
        {
            _vm.NewId = id;
            _vm.NewName = "Something new";

            Assert.False(_vm.Add(out string error));
            Assert.Contains(message, error);
            Assert.Equal(error, _vm.AddError);
        }

        [Fact]
        public void Rejects_an_existing_display_name()
        {
            _vm.NewId = "juiced2";
            _vm.NewName = "juiced";

            Assert.False(_vm.Add(out string error));
            Assert.Contains("already exists", error);
        }

        [Fact]
        public void Renames_and_sets_show_in_history()
        {
            _vm.Selected = Row("party");
            _vm.EditName = "Party play";
            _vm.EditShowInHistory = true;
            Assert.True(_vm.HasChanges);

            Assert.True(_vm.Save(out string error), error);

            TagInfo tag = _source.GetTags().Single(t => t.Id == "party");
            Assert.Equal("Party play", tag.Name);
            Assert.True(tag.ShowInHistory);
            Assert.False(_vm.HasChanges);
            Assert.Equal("party", _vm.Selected.Id);
        }

        [Fact]
        public void Default_tags_can_be_renamed_but_not_deleted()
        {
            _vm.Selected = Row("blight");
            Assert.False(_vm.CanDelete);
            Assert.False(_vm.DeleteSelected());

            _vm.EditName = "Blighted";
            Assert.True(_vm.Save(out _));
            Assert.Equal("Blighted", Row("blight").Name);
        }

        [Fact]
        public void Rename_to_an_existing_name_is_rejected()
        {
            _vm.Selected = Row("party");
            _vm.EditName = "Juiced";

            Assert.True(_vm.HasEditError);
            Assert.False(_vm.Save(out _));
            Assert.Equal("Party", Row("party").Name);
        }

        [Fact]
        public void Discard_restores_the_stored_values()
        {
            _vm.Selected = Row("party");
            _vm.EditName = "Something else";

            _vm.RevertCommand.Execute(null);

            Assert.Equal("Party", _vm.EditName);
            Assert.False(_vm.HasChanges);
        }

        [Fact]
        public void Deletes_a_custom_tag()
        {
            _vm.Selected = Row("juiced");
            Assert.True(_vm.CanDelete);

            Assert.True(_vm.DeleteSelected());

            Assert.Null(Row("juiced"));
            Assert.DoesNotContain(_source.GetTags(), t => t.Id == "juiced");
            Assert.Null(_vm.Selected);
            Assert.DoesNotContain(_source.GetHistory().SelectMany(a => a.Tags), t => t.Id == "juiced");
        }

        [Fact]
        public void Refresh_keeps_selection_and_unsaved_input()
        {
            _vm.Selected = Row("party");
            _vm.EditName = "Typing…";

            _vm.Refresh();

            Assert.Equal("party", _vm.Selected.Id);
            Assert.Equal("Typing…", _vm.EditName);
        }
    }
}
