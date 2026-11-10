using System;
using System.ComponentModel;
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

        /// <summary>
        /// Places the overlay at its saved position (screen pixels, also written by the classic app) or at
        /// <paramref name="fallback"/> (device-independent units) if there is none or it is off all screens.
        /// </summary>
        public void Place(Point fallback)
        {
            Matrix fromDevice = DeviceTransform(toDevice: false);
            (int X, int Y)? saved = OverlaySettings.ReadPosition(_setup, Id);
            Point p = saved.HasValue ? PixelsToDips(saved.Value.X, saved.Value.Y, fromDevice) : fallback;
            if (!IsOnScreen(p, VirtualScreen)) p = fallback;
            Left = p.X;
            Top = p.Y;
        }

        /// <summary>Forgets the saved position and moves the overlay to <paramref name="fallback"/>.</summary>
        public void ResetPosition(Point fallback)
        {
            Left = fallback.X;
            Top = fallback.Y;
            SavePosition();
        }

        private void SavePosition()
        {
            if (!_canWrite) return;
            Point px = DeviceTransform(toDevice: true).Transform(new Point(Left, Top));
            OverlaySettings.WritePosition(_setup, Id, (int)Math.Round(px.X), (int)Math.Round(px.Y));
        }

        /// <summary>Screen pixels -> device-independent units of this window (classic positions are pixels).</summary>
        public static Point PixelsToDips(int x, int y, Matrix fromDevice) => fromDevice.Transform(new Point(x, y));

        /// <summary>True if the top-left corner (plus a little of the overlay) is on the desktop.</summary>
        public static bool IsOnScreen(Point topLeft, Rect desktop) =>
            topLeft.X >= desktop.Left - 20 && topLeft.Y >= desktop.Top - 10 &&
            topLeft.X <= desktop.Right - 40 && topLeft.Y <= desktop.Bottom - 20;

        private static Rect VirtualScreen => new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);

        private Matrix DeviceTransform(bool toDevice)
        {
            new WindowInteropHelper(this).EnsureHandle();
            CompositionTarget target = PresentationSource.FromVisual(this)?.CompositionTarget;
            if (target == null) return Matrix.Identity;
            return toDevice ? target.TransformToDevice : target.TransformFromDevice;
        }

        /// <summary>Closes for real (app exit); otherwise closing only hides the overlay.</summary>
        public void CloseForGood()
        {
            _allowClose = true;
            Close();
        }

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
