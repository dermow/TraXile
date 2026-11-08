using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TraXile.Next.ViewModels;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace TraXile.Next.Views
{
    public partial class TagsPage : Page
    {
        private readonly TagsViewModel _vm;

        public TagsPage()
        {
            InitializeComponent();
            _vm = App.Tags;
            DataContext = _vm;

            // Tags may have been created meanwhile (chat command trax::tag, activity log)
            Loaded += (s, e) =>
            {
                _vm.Refresh();
                // --select 2: preselect a tag (screenshots)
                if (App.ScreenshotPath != null && App.SelectRows.Length > 0 && App.SelectRows[0] < _vm.Tags.Count)
                {
                    _vm.Selected = _vm.Tags[App.SelectRows[0]];
                }
            };
        }

        private void NewTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            // push the text of the focused box before adding
            (sender as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (_vm.Add(out _)) NewIdBox.Focus();
            e.Handled = true;
        }

        private void EditName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            (sender as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            _vm.Save(out _);
            e.Handled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            TagRow tag = _vm.Selected;
            if (tag == null || tag.IsDefault) return;

            string usage = tag.Usage > 0
                ? $"It is used by {tag.UsageText}; they keep it in the database, but it is not shown any more."
                : "No activity uses it.";
            var box = new MessageBox
            {
                Title = $"Delete the tag \"{tag.Name}\"?",
                Content = usage,
                PrimaryButtonText = "Delete tag",
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,
                CloseButtonText = "Cancel",
                Owner = Window.GetWindow(this)
            };
            if (await box.ShowDialogAsync() == MessageBoxResult.Primary) _vm.DeleteSelected();
        }
    }
}
