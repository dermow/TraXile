using System;
using System.Globalization;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Overlay settings in config.xml under the keys the classic app reads (Main.cs ReadSettings and
    /// BaseOverlay): positions are screen pixels of the top-left corner, opacity is a percentage.
    /// </summary>
    public static class OverlaySettings
    {
        public const string Stopwatch = "stopwatch";
        public const string Tags = "tags";

        public const string OpacityKey = "overlay.general.opacity";

        /// <summary>Lowest opacity TraXile.Next applies, so an overlay never becomes invisible (the classic slider allowed 0).</summary>
        public const int MinOpacity = 10;

        public static string XKey(string id) => $"overlay.{id}.x";
        public static string YKey(string id) => $"overlay.{id}.y";

        /// <summary>Overlay opacity in percent (default 100), clamped to <see cref="MinOpacity"/>..100.</summary>
        public static int ReadOpacity(AppSetup setup)
        {
            int value = int.TryParse(setup.ReadSetting(OpacityKey, "100"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 100;
            return Math.Max(MinOpacity, Math.Min(100, value));
        }

        /// <summary>Stored position in screen pixels, or null if none was saved (or it is unreadable).</summary>
        public static (int X, int Y)? ReadPosition(AppSetup setup, string id)
        {
            string x = setup.ReadSetting(XKey(id));
            string y = setup.ReadSetting(YKey(id));
            if (int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out int px) &&
                int.TryParse(y, NumberStyles.Integer, CultureInfo.InvariantCulture, out int py))
            {
                return (px, py);
            }
            return null;
        }

        public static void WritePosition(AppSetup setup, string id, int x, int y)
        {
            setup.WriteSetting(XKey(id), x.ToString(CultureInfo.InvariantCulture));
            setup.WriteSetting(YKey(id), y.ToString(CultureInfo.InvariantCulture));
        }
    }
}
