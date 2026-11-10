using System.Windows;
using Microsoft.Win32;
using TraXile.Next.ViewModels;
using Wpf.Ui.Controls;

namespace TraXile.Next.Views
{
    /// <summary>
    /// Shown before the main window when no Client.txt is configured. DialogResult is true when a
    /// Client.txt was saved and live tracking can start, false for demo data.
    /// </summary>
    public partial class FirstRunWindow : FluentWindow
    {
        private readonly FirstRunViewModel _vm;

        public FirstRunWindow(FirstRunViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;
        }

        public System.Windows.Controls.Panel ContentRoot => RootGrid;

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select the Client.txt of Path of Exile",
                Filter = "Path of Exile log (Client.txt)|Client.txt|Text files (*.txt)|*.txt",
                CheckFileExists = true
            };
            if (dlg.ShowDialog(this) == true) _vm.AddManual(dlg.FileName);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.Confirm()) DialogResult = true;
        }

        private void Demo_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
