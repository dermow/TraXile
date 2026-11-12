using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;

namespace TraXile.Next.Views
{
    /// <summary>
    /// Chart tooltip drawn as a regular WPF <see cref="ToolTip"/> (theme style, data template) instead of
    /// the LiveCharts canvas tooltip, so it can hold structured content such as a small table.
    /// </summary>
    internal sealed class SliceTooltip : IChartTooltip
    {
        private readonly ToolTip _tip;
        private readonly Func<ChartPoint, object> _contentOf;
        private object _shown;

        /// <summary>The WPF tooltip (screenshots).</summary>
        internal ToolTip Control => _tip;

        /// <param name="template">Renders the content of the hovered point.</param>
        /// <param name="contentOf">Content for a hovered point; null shows nothing for it.</param>
        public SliceTooltip(DataTemplate template, Func<ChartPoint, object> contentOf)
        {
            _contentOf = contentOf;
            _tip = new ToolTip
            {
                ContentTemplate = template,
                Placement = PlacementMode.Mouse,
                IsHitTestVisible = false,
                MaxWidth = 600, // the theme's tooltip width would clip a table
            };
        }

        public void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
        {
            if (!_tip.Dispatcher.CheckAccess())
            {
                List<ChartPoint> points = foundPoints.ToList();
                _tip.Dispatcher.BeginInvoke(new Action(() => Show(points, chart)));
                return;
            }

            object content = foundPoints.Select(_contentOf).FirstOrDefault(c => c != null);
            if (content == null)
            {
                Close();
                return;
            }
            if (ReferenceEquals(content, _shown) && _tip.IsOpen) return; // keep it where it opened

            _shown = content;
            _tip.PlacementTarget = chart.View as UIElement;
            _tip.Content = content;
            _tip.IsOpen = false; // reopen: places it at the mouse again
            _tip.IsOpen = true;
        }

        public void Hide(Chart chart)
        {
            if (_tip.Dispatcher.CheckAccess()) Close();
            else _tip.Dispatcher.BeginInvoke(new Action(Close));
        }

        private void Close()
        {
            _shown = null;
            _tip.IsOpen = false;
        }
    }
}
