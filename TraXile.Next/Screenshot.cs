using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TraXile.Next
{
    /// <summary>
    /// Off-screen screenshots for visual review (--screenshot file.png): the window is placed far outside the
    /// desktop, rendered to a PNG after the intro animations and the app exits.
    /// </summary>
    public static class Screenshot
    {
        public static void PrepareOffscreen(Window window, double height, Panel root)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -30000;
            window.Top = -30000;
            window.ShowActivated = false;
            window.ShowInTaskbar = false;
            if (height > 0) window.Height = height;

            // RenderTargetBitmap only captures the content, not the window background
            root?.SetResourceReference(Panel.BackgroundProperty, "BgBrush");
        }

        /// <summary>Saves the window content after <paramref name="delay"/> and shuts the app down.</summary>
        public static void Schedule(Window window, string path, TimeSpan delay)
        {
            var timer = new DispatcherTimer { Interval = delay };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                try
                {
                    Save((FrameworkElement)window.Content, path);
                }
                finally
                {
                    Application.Current.Shutdown();
                }
            };
            timer.Start();
        }

        public static void Save(FrameworkElement root, string path)
        {
            const double scale = 1.25;
            int w = (int)(root.ActualWidth * scale);
            int h = (int)(root.ActualHeight * scale);

            var rtb = new RenderTargetBitmap(w, h, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            rtb.Render(root);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using (var fs = File.Create(path))
            {
                encoder.Save(fs);
            }
        }
    }
}
