using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TraXile.Next.Services;

namespace TraXile.Next.Overlays
{
    /// <summary>
    /// Base of the in-game overlays (classic: BaseOverlay): borderless, transparent and always on top.
    /// Right-click opens Move / Fix position and Close; while movable the overlay is dragged with the left
    /// mouse button and its position is saved when it is fixed again, in screen pixels like the classic app.
    /// Closing only hides it.
    /// </summary>
    public abstract class OverlayWindow : Window
    {
        private readonly AppSetup _setup;
        private readonly bool _canWrite;
        private readonly Border _frame;
        private readonly MenuItem _moveItem;
        private bool _allowClose;

        protected OverlayWindow(string id, AppSetup setup, bool canWrite)
        {
            Id = id;
            _setup = setup;
            _canWrite = canWrite;

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            FontFamily = (FontFamily)Application.Current.FindResource("UiFont");

            _frame = new Border
            {
                CornerRadius = (CornerRadius)Application.Current.FindResource("SurfaceRadius"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 6, 12, 6),
            };
            _frame.SetResourceReference(Border.BackgroundProperty, "SurfaceBrush");
            _frame.SetResourceReference(Border.BorderBrushProperty, "BorderStrongBrush");
            SetResourceReference(ForegroundProperty, "TextBrush");
            Content = _frame;

            var menu = new ContextMenu();
            _moveItem = new MenuItem { Header = "Move" };
            _moveItem.Click += (s, e) => IsMovable = !IsMovable;
            menu.Items.Add(_moveItem);
            var close = new MenuItem { Header = "Close" };
            close.Click += (s, e) => { IsMovable = false; Hide(); }; // keeps a position that was just moved
            menu.Items.Add(close);
            ContextMenu = menu;

            MouseLeftButtonDown += (s, e) =>
            {
                if (IsMovable && e.ButtonState == MouseButtonState.Pressed) DragMove();
            };
        }

        /// <summary>Overlay ID in the setting keys (overlay.{id}.x).</summary>
        public string Id { get; }

        protected UIElement Body
        {
            get => _frame.Child;
            set => _frame.Child = value;
        }

        private bool _isMovable;

        /// <summary>Movable: highlighted and draggable. Fixing it again saves the position.</summary>
        public bool IsMovable
        {
            get => _isMovable;
            set
            {
                if (_isMovable == value) return;
                _isMovable = value;
                _moveItem.Header = value ? "Fix position" : "Move";
                Cursor = value ? Cursors.SizeAll : null;
                if (value)
                {
                    _frame.SetResourceReference(Border.BackgroundProperty, "AccentSurfaceBrush");
                    _frame.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                }
                else
                {
                    _frame.SetResourceReference(Border.BackgroundProperty, "SurfaceBrush");
                    _frame.SetResourceReference(Border.BorderBrushProperty, "BorderStrongBrush");
                    SavePosition();
                }
            }
        }

        /// <summary>Opacity in percent (overlay.general.opacity).</summary>
        public void ApplyOpacity(int percent) =>
            Opacity = Math.Max(OverlaySettings.MinOpacity, Math.Min(100, percent)) / 100.0;

        // Saved position (screen pixels) that is applied once more after the first render: WPF may still
        // rescale/move the window when it lands on a monitor with a different DPI than where it was created.
        private (int X, int Y)? _pendingPixels;

        /// <summary>
        /// Places the overlay at its saved position (screen pixels, also written by the classic app) or at
        /// <paramref name="fallback"/> (device-independent units) if there is none or it is on no monitor.
        /// Pixels are applied with SetWindowPos, so mixed-DPI multi-monitor setups need no DIP conversion.
        /// </summary>
        public void Place(Point fallback)
        {
            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            (int X, int Y)? saved = OverlaySettings.ReadPosition(_setup, Id);
            if (saved.HasValue && IsOnScreen(saved.Value.X, saved.Value.Y, MonitorRects()))
            {
                _pendingPixels = saved;
                MoveToPixels(hwnd, saved.Value.X, saved.Value.Y);
            }
            else
            {
                _pendingPixels = null;
                Left = fallback.X;
                Top = fallback.Y;
            }
        }

        /// <summary>Forgets the saved position and moves the overlay to <paramref name="fallback"/>.</summary>
        public void ResetPosition(Point fallback)
        {
            new WindowInteropHelper(this).EnsureHandle();
            _pendingPixels = null;
            Left = fallback.X;
            Top = fallback.Y;
            SavePosition();
        }

        /// <summary>Moves the overlay out of sight without saving (screenshot mode renders it off-screen).</summary>
        public void MoveOffScreen()
        {
            _pendingPixels = null;
            Left = -30000;
            Top = -30000;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (_pendingPixels is (int x, int y)) MoveToPixels(new WindowInteropHelper(this).Handle, x, y);
            _pendingPixels = null;
        }

        private void SavePosition()
        {
            if (!_canWrite) return;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out RECT r)) return;
            OverlaySettings.WritePosition(_setup, Id, r.Left, r.Top);
        }

        private static void MoveToPixels(IntPtr hwnd, int x, int y) =>
            SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

        /// <summary>
        /// True if the top-left corner plus a little of the overlay (screen pixels) is on one of the
        /// <paramref name="monitors"/> - checked per monitor, so gaps between monitors of different size count as off-screen.
        /// </summary>
        public static bool IsOnScreen(int x, int y, IEnumerable<Rect> monitors)
        {
            var probe = new Point(x + 20, y + 10);
            return monitors.Any(m => m.Contains(probe));
        }

        /// <summary>Bounds of all monitors in screen pixels.</summary>
        private static List<Rect> MonitorRects()
        {
            var rects = new List<Rect>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdc, ref RECT r, IntPtr data) =>
            {
                rects.Add(new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top));
                return true;
            }, IntPtr.Zero);
            return rects;
        }

        /// <summary>Closes for real (app exit); otherwise closing only hides the overlay.</summary>
        public void CloseForGood()
        {
            IsMovable = false; // saves a position that was moved but not fixed yet
            _allowClose = true;
            Close();
        }

        private const uint SWP_NOSIZE = 0x0001, SWP_NOZORDER = 0x0004, SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc callback, IntPtr data);

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnClosing(e);
        }
    }
}
