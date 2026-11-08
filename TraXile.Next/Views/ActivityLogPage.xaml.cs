using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace TraXile.Next.Views
{
    public partial class ActivityLogPage : Page
    {
        private readonly ActivityLogViewModel _vm;
        private bool _syncingSelection;

        // Sorting is lost when ItemsSource is replaced -> remember and re-apply it
        private string _sortMember;
        private ListSortDirection _sortDirection;

        public ActivityLogPage()
        {
            InitializeComponent();
            _vm = App.ActivityLog;
            DataContext = _vm;

            Loaded += (s, e) =>
            {
                _vm.ItemsRefreshed += OnItemsRefreshed;
                RestoreGridState();

                foreach (int row in App.SelectRows)
                {
                    if (row < Grid.Items.Count) Grid.SelectedItems.Add(Grid.Items[row]);
                }
            };
            Unloaded += (s, e) => _vm.ItemsRefreshed -= OnItemsRefreshed;

            PreviewKeyDown += Page_PreviewKeyDown;
        }

        // ------------------------------------------------------------------
        // Selection & sorting
        // ------------------------------------------------------------------

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_syncingSelection) return;
            _vm.SetSelection(Grid.SelectedItems.Cast<ActivityInfo>());
        }

        private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Default behaviour toggles ascending/descending; we only record the result
            _sortMember = e.Column.SortMemberPath;
            _sortDirection = e.Column.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }

        private void OnItemsRefreshed(object sender, EventArgs e)
        {
            // Let the grid pick up the new ItemsSource first
            Dispatcher.BeginInvoke(new Action(RestoreGridState), DispatcherPriority.Loaded);
        }

        private void RestoreGridState()
        {
            if (_sortMember != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(Grid.ItemsSource);
                if (view != null)
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(_sortMember, _sortDirection));
                }
                foreach (DataGridColumn col in Grid.Columns)
                {
                    col.SortDirection = col.SortMemberPath == _sortMember ? _sortDirection : (ListSortDirection?)null;
                }
            }

            var ids = new HashSet<string>(_vm.SelectedIds);
            _syncingSelection = true;
            try
            {
                Grid.SelectedItems.Clear();
                foreach (ActivityInfo a in _vm.Items)
                {
                    if (ids.Contains(a.Id)) Grid.SelectedItems.Add(a);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        // ------------------------------------------------------------------
        // Keyboard
        // ------------------------------------------------------------------

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && SearchBox.IsKeyboardFocusWithin && !string.IsNullOrEmpty(_vm.SearchText))
            {
                _vm.SearchText = "";
                e.Handled = true;
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _vm.HasSelection)
            {
                e.Handled = true;
                ConfirmAndDelete();
            }
        }

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        private void Delete_Click(object sender, RoutedEventArgs e) => ConfirmAndDelete();

        private async void ConfirmAndDelete()
        {
            int count = _vm.SelectedCount;
            if (count == 0) return;

            var box = new MessageBox
            {
                Title = count == 1 ? "Delete activity?" : $"Delete {count:N0} activities?",
                Content = count == 1
                    ? $"\"{_vm.SelectedActivity?.Area}\" will be removed from your history and statistics. This cannot be undone."
                    : $"{count:N0} activities will be removed from your history and statistics. This cannot be undone.",
                PrimaryButtonText = "Delete",
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,
                CloseButtonText = "Cancel",
                Owner = Window.GetWindow(this)
            };

            if (await box.ShowDialogAsync() == MessageBoxResult.Primary)
            {
                _vm.DeleteSelected();
            }
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu
            {
                PlacementTarget = (UIElement)sender,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
            };

            foreach (TagInfo tag in _vm.AvailableTags)
            {
                var swatch = new Border
                {
                    Width = 10,
                    Height = 10,
                    CornerRadius = new CornerRadius(5),
                    Background = tag.Background
                };
                var item = new MenuItem { Header = tag.Name, Icon = swatch, Command = _vm.AddTagCommand, CommandParameter = tag };
                menu.Items.Add(item);
            }

            if (menu.Items.Count > 0) menu.IsOpen = true;
        }

        // Classic export sources: all entries, filtered entries, selected entries; formats CSV and JSON
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu { PlacementTarget = (UIElement)sender, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom };
            string filtered = _vm.HasActiveFilters ? "Filtered activities" : "Shown activities";
            AddExportItems(menu, $"{filtered} ({_vm.MatchingItems.Count:N0})", () => _vm.MatchingItems, "TraXile_Activities");
            menu.Items.Add(new Separator());
            AddExportItems(menu, $"All activities ({_vm.AllActivities.Count:N0})", () => _vm.AllActivities, "TraXile_Activities_All");
            menu.IsOpen = true;
        }

        private void ExportSelected_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu { PlacementTarget = (UIElement)sender, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom };
            AddExportItems(menu, $"{_vm.SelectedCount:N0} selected activities", () => _vm.SelectedItems.ToList(), "TraXile_Activities_Selected");
            menu.IsOpen = true;
        }

        private void AddExportItems(ContextMenu menu, string label, Func<IReadOnlyCollection<ActivityInfo>> activities, string baseName)
        {
            menu.Items.Add(new MenuItem { Header = label.ToUpperInvariant(), Style = (Style)FindResource("MenuSectionHeader") });
            foreach (ExportFormat format in new[] { ExportFormat.Csv, ExportFormat.Json })
            {
                var item = new MenuItem { Header = format == ExportFormat.Csv ? "Export as CSV" : "Export as JSON" };
                item.Click += (s, a) => Export(activities(), baseName, format);
                menu.Items.Add(item);
            }
        }

        private enum ExportFormat { Csv, Json }

        private void Export(IReadOnlyCollection<ActivityInfo> activities, string baseName, ExportFormat format)
        {
            if (activities.Count == 0) return;

            string ext = format == ExportFormat.Csv ? "csv" : "json";
            var dlg = new SaveFileDialog
            {
                Filter = format == ExportFormat.Csv ? "CSV files (*.csv)|*.csv" : "JSON files (*.json)|*.json",
                FileName = $"{baseName}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.{ext}"
            };
            if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

            try
            {
                if (format == ExportFormat.Csv) ActivityLogViewModel.ExportCsv(dlg.FileName, activities);
                else ActivityLogViewModel.ExportJson(dlg.FileName, activities);
            }
            catch (Exception ex)
            {
                _ = new MessageBox { Title = "Export failed", Content = ex.Message, CloseButtonText = "OK" }.ShowDialogAsync();
            }
        }
    }
}
