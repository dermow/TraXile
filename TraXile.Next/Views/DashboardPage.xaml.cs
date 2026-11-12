using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using TraXile.Next.ViewModels;

namespace TraXile.Next.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            DataContext = App.Dashboard;

            // the slices show the same tooltip as the overview table rows
            Donut.Tooltip = new SliceTooltip((DataTemplate)FindResource("TimeShareTip"), RowOf);
            Donut.HoveredPointsChanged += Donut_HoveredPointsChanged;

            // screenshots: --select 0 = "Other" expanded, 1 = tooltip of its table row, 2 = hover + click on its donut slice;
            // an opened tooltip is saved next to the screenshot as *.tooltip.png
            if (App.ScreenshotPath != null)
            {
                if (App.SelectRows.Contains(0) && ViewModel?.IsOtherExpanded == false) ViewModel.ToggleOtherCommand.Execute(null);
                if (App.SelectRows.Contains(1)) Loaded += (s, e) => ScreenshotRowTooltip();
                if (App.SelectRows.Contains(2)) Loaded += (s, e) => After(1.5, ScreenshotSliceTooltip); // once the donut is measured
            }
        }

        private DashboardViewModel ViewModel => DataContext as DashboardViewModel;

        private OverviewRow RowOf(ChartPoint point) => ViewModel?.RowOfSlice(point.Context.Series);

        private bool IsOtherSlice(IEnumerable<ChartPoint> points) =>
            points != null && points.Any(p => RowOf(p)?.IsOther == true);

        private void Donut_HoveredPointsChanged(IChartView chart, IEnumerable<ChartPoint> newItems, IEnumerable<ChartPoint> oldItems)
        {
            Donut.Cursor = IsOtherSlice(newItems) ? Cursors.Hand : null;
        }

        private void Donut_DataPointerDown(IChartView chart, IEnumerable<ChartPoint> points)
        {
            if (IsOtherSlice(points)) ViewModel?.ToggleOtherCommand.Execute(null);
        }

        private void OverviewRow_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is OverviewRow { IsOther: true })
                ViewModel?.ToggleOtherCommand.Execute(null);
        }

        private void ViewAll_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as MainWindow)?.RootNavigation.Navigate(typeof(ActivityLogPage));
        }

        // ------------------------------------------------------------------
        // Screenshots
        // ------------------------------------------------------------------

        private void ScreenshotRowTooltip()
        {
            FrameworkElement row = FindVisuals<FrameworkElement>(OverviewItems)
                .FirstOrDefault(f => f.DataContext is OverviewRow { IsOther: true } && f.ToolTip is ToolTip);
            if (row == null) return;
            var tip = (ToolTip)row.ToolTip;
            tip.PlacementTarget = row; // like the tooltip service: the popup takes its data context from here
            tip.IsOpen = true;
            After(1, () => SaveTooltip(tip));
        }

        private void ScreenshotSliceTooltip()
        {
            ISeries slice = ViewModel?.TimeSeries.FirstOrDefault(s => ViewModel.RowOfSlice(s)?.IsOther == true);
            if (slice == null) return;
            List<ChartPoint> points = slice.Fetch(Donut.CoreChart).ToList();
            Donut.Tooltip.Show(points, Donut.CoreChart);
            Donut_DataPointerDown(Donut, points);
            After(1, () => SaveTooltip((Donut.Tooltip as SliceTooltip)?.Control));
        }

        private static void SaveTooltip(ToolTip tip)
        {
            if (tip == null) return;
            // the popup's root: the tooltip as drawn, with its chrome
            var root = PresentationSource.FromVisual(tip)?.RootVisual as FrameworkElement ?? tip;
            Screenshot.Save(root, System.IO.Path.ChangeExtension(App.ScreenshotPath, ".tooltip.png"));
        }

        private static void After(double seconds, Action action)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            timer.Tick += (s, e) => { timer.Stop(); action(); };
            timer.Start();
        }

        private static IEnumerable<T> FindVisuals<T>(DependencyObject root) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is T t) yield return t;
                foreach (T d in FindVisuals<T>(child)) yield return d;
            }
        }
    }
}
