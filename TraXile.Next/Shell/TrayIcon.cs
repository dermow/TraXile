using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace TraXile.Next.Shell
{
    /// <summary>
    /// Notification area icon like the classic app: double-click restores the window, right-click opens a menu
    /// (Open, Chat commands, About, Overlays, Exit). The menu is a WPF context menu, so it follows the design tokens.
    /// </summary>
    public sealed class TrayIcon : IDisposable
    {
        private readonly Window _owner;
        private readonly Forms.NotifyIcon _icon;
        private readonly ContextMenu _menu;

        public TrayIcon(Window owner)
        {
            _owner = owner;
            _icon = new Forms.NotifyIcon
            {
                Text = "TraXile",
                Icon = LoadIcon(),
                Visible = true
            };
            _icon.MouseDoubleClick += (s, e) => { if (e.Button == Forms.MouseButtons.Left) ShowWindow(); };
            _icon.MouseUp += (s, e) => { if (e.Button == Forms.MouseButtons.Right) OpenMenu(); };

            _menu = BuildMenu();
        }

        /// <summary>Raised when "Exit" is chosen.</summary>
        public event EventHandler ExitRequested;

        private static Icon LoadIcon()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            catch (Exception)
            {
                return SystemIcons.Application;
            }
        }

        private ContextMenu BuildMenu()
        {
            var menu = new ContextMenu();

            var open = new MenuItem { Header = "Open TraXile", FontWeight = FontWeights.SemiBold };
            open.Click += (s, e) => ShowWindow();
            menu.Items.Add(open);
            menu.Items.Add(new Separator());

            var chat = new MenuItem { Header = "Chat commands" };
            chat.Click += (s, e) => { ShowWindow(); Dialogs.ShowChatCommands(_owner); };
            menu.Items.Add(chat);

            var about = new MenuItem { Header = "About" };
            about.Click += (s, e) => { ShowWindow(); Dialogs.ShowAbout(_owner); };
            menu.Items.Add(about);

            // Overlays move over in phase 2 (tickets "Stopwatch overlay", "Tags overlay")
            var overlays = new MenuItem { Header = "Overlays" };
            overlays.Items.Add(new MenuItem { Header = "Stopwatch (coming soon)", IsEnabled = false });
            overlays.Items.Add(new MenuItem { Header = "Tags (coming soon)", IsEnabled = false });
            menu.Items.Add(overlays);

            menu.Items.Add(new Separator());
            var exit = new MenuItem { Header = "Exit" };
            exit.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            menu.Items.Add(exit);

            return menu;
        }

        private void OpenMenu()
        {
            // A popup only closes on an outside click if its process owns the foreground window
            IntPtr handle = new WindowInteropHelper(_owner).EnsureHandle();
            SetForegroundWindow(handle);
            _menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            _menu.IsOpen = true;
        }

        /// <summary>Brings the main window back from the tray / minimized state.</summary>
        public void ShowWindow()
        {
            _owner.Show();
            _owner.ShowInTaskbar = true;
            if (_owner.WindowState == WindowState.Minimized) _owner.WindowState = WindowState.Normal;
            _owner.Activate();
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
