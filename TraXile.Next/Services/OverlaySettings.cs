using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>What the overlay settings need from the running overlays.</summary>
    public interface IOverlayHost
    {
        /// <summary>Re-reads opacity and tag slots.</summary>
        void ReloadSettings();

        /// <summary>Moves all overlays to their default place and saves it.</summary>
        void ResetPositions();
    }

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

        /// <summary>Open the overlay when TraXile starts ("True"/"False"; classic bug wrote overlay.stopwatch.default instead).</summary>
        public static string ShowDefaultKey(string id) => $"overlay.{id}.showDefault";

        public static bool ReadShowDefault(AppSetup setup, string id) =>
            bool.TryParse(setup.ReadSetting(ShowDefaultKey(id), "False"), out bool b) && b;

        /// <summary>Value of an empty tag slot, as the classic slot picker stores it.</summary>
        public const string EmptySlot = "None";

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

        /// <summary>Key of a tag slot of the tags overlay (1..3).</summary>
        public static string TagSlotKey(int slot) => $"overlay.tags.tag{slot}";

        /// <summary>Classic defaults of the three tag slots (Main.cs ReadSettings).</summary>
        public static readonly string[] DefaultTagSlots = { "blight", "expedition", null };

        /// <summary>The stored tag of each slot (display name like the classic app, or an ID); null = empty slot ("None" in the classic app).</summary>
        public static string[] ReadTagSlots(AppSetup setup) =>
            Enumerable.Range(1, 3).Select(i =>
            {
                string v = setup.ReadSetting(TagSlotKey(i), DefaultTagSlots[i - 1]);
                // the classic slot picker stores "None" for an empty slot
                return string.IsNullOrWhiteSpace(v) || v.Equals(EmptySlot, StringComparison.OrdinalIgnoreCase) ? null : v;
            }).ToArray();

        /// <summary>Finds the tag of a slot: by display name (what the classic app stores), then by ID.</summary>
        public static TagInfo ResolveTag(string stored, IEnumerable<TagInfo> tags)
        {
            if (string.IsNullOrWhiteSpace(stored)) return null;
            List<TagInfo> list = tags.ToList();
            return list.FirstOrDefault(t => string.Equals(t.Name, stored, StringComparison.OrdinalIgnoreCase))
                ?? list.FirstOrDefault(t => string.Equals(t.Id, stored, StringComparison.OrdinalIgnoreCase));
        }

        public static void WritePosition(AppSetup setup, string id, int x, int y)
        {
            setup.WriteSetting(XKey(id), x.ToString(CultureInfo.InvariantCulture));
            setup.WriteSetting(YKey(id), y.ToString(CultureInfo.InvariantCulture));
        }
    }
}
