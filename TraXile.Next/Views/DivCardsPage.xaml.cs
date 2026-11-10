using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TraXile.Next.ViewModels;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace TraXile.Next.Views
{
    public partial class DivCardsPage : Page
    {
        private readonly DivCardsViewModel _vm;

        public DivCardsPage()
        {
            InitializeComponent();
            _vm = App.DivCards;
            DataContext = _vm;
        }

        private void SortHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && Enum.TryParse(b.CommandParameter as string, out DivCardSortColumn column))
                _vm.SortBy(column);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.Items.Count == 0) return;

            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"TraXile_DivinationCards_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv"
            };
            if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

            try
            {
                DivCardsViewModel.ExportCsv(dlg.FileName, _vm.Items);
            }
            catch (Exception ex)
            {
                _ = new MessageBox { Title = "Export failed", Content = ex.Message, CloseButtonText = "OK" }.ShowDialogAsync();
            }
        }
    }
}
