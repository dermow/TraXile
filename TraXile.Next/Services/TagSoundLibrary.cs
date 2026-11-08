using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TraXile.Next.Services
{
    /// <summary>
    /// The sound files a tag can play: every file in the audio folder next to the exe
    /// (the same folder the classic app and <see cref="TrX_SoundManager"/> use). Users can add their own files.
    /// </summary>
    public sealed class TagSoundLibrary
    {
        /// <summary>Stored as the sound of tags without one (column default of tx_tags.audio_file_name).</summary>
        public const string NoSound = "none";

        public TagSoundLibrary(string folder)
        {
            Folder = folder;
        }

        public static TagSoundLibrary NextToExe() =>
            new TagSoundLibrary(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio"));

        public string Folder { get; }

        /// <summary>File names (as stored with the tag), sorted; empty if the folder is missing.</summary>
        public IReadOnlyList<string> List()
        {
            try
            {
                if (!Directory.Exists(Folder)) return Array.Empty<string>();
                return Directory.GetFiles(Folder)
                    .Select(Path.GetFileName)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>True if the tag has a real sound file (not empty and not <see cref="NoSound"/>).</summary>
        public static bool IsSet(string soundId) =>
            !string.IsNullOrWhiteSpace(soundId) && !string.Equals(soundId, NoSound, StringComparison.OrdinalIgnoreCase);
    }
}
