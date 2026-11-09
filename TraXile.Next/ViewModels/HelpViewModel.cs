using System.Collections.Generic;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>A help entry: something to type and what it does.</summary>
    public sealed class HelpEntry
    {
        public HelpEntry(string input, string description)
        {
            Input = input;
            Description = description;
        }

        public string Input { get; }
        public string Description { get; }
    }

    /// <summary>A labelled web link.</summary>
    public sealed class HelpLink
    {
        public HelpLink(string label, string url)
        {
            Label = label;
            Url = url;
        }

        public string Label { get; }
        public string Url { get; }
    }

    /// <summary>
    /// Help page (classic ChatCommandHelp, SearchHelp and AboutForm): chat commands, activity log search, version,
    /// links and credits.
    /// </summary>
    public sealed class HelpViewModel
    {
        public HelpViewModel(UpdateViewModel update, SupportViewModel support)
        {
            Update = update;
            Support = support;
        }

        public UpdateViewModel Update { get; }

        /// <summary>Support info and bundle (classic DebugInfo window).</summary>
        public SupportViewModel Support { get; }

        public string Version => UpdateChecker.AppVersion;

        /// <summary>The chat commands the core understands (TrX_CoreLogic, "trax::" prefix).</summary>
        public static IReadOnlyList<HelpEntry> ChatCommands { get; } = new[]
        {
            new HelpEntry("trax::tag <tag>", "Add a custom tag to the current activity. A tag that does not exist yet is created."),
            new HelpEntry("trax::untag <tag>", "Remove a custom tag from the current activity."),
            new HelpEntry("trax::pause", "Pause the tracking of the current activity."),
            new HelpEntry("trax::resume", "Resume paused tracking."),
            new HelpEntry("trax::finish", "Finish the current activity now."),
        };

        /// <summary>
        /// Examples for the search of the activity log. Each word must be found (case-insensitive) in the area, the
        /// activity type, the tier / level or a tag of an activity (ActivityLogViewModel.Matches).
        /// </summary>
        public static IReadOnlyList<HelpEntry> SearchExamples { get; } = new[]
        {
            new HelpEntry("strand", "Activities whose area, type, tier or tag contains \"strand\"."),
            new HelpEntry("t16", "Maps of tier 16."),
            new HelpEntry("map blight", "Several words: an activity must match all of them, here maps with the Blight tag."),
            new HelpEntry("expedition", "Activities with the Expedition tag and Expedition logbooks."),
        };

        /// <summary>Notes on the search below the examples.</summary>
        public static IReadOnlyList<string> SearchNotes { get; } = new[]
        {
            "Upper and lower case do not matter, and part of a word is enough.",
            "The search always looks at all activities, also those beyond the \"Newest …\" limit of the list.",
            "It combines with the filter bar (time, type, area, level, tags) above the list.",
        };

        public static IReadOnlyList<HelpLink> Links { get; } = new[]
        {
            new HelpLink("Project page", "https://github.com/dermow/TraXile"),
            new HelpLink("Wiki", TrX_Static.WIKI_URL),
            new HelpLink("Report an issue", TrX_Static.ISSUE_URL),
            new HelpLink("Support me on Patreon", "https://www.patreon.com/c/dermow"),
        };

        /// <summary>Special thanks (classic AboutForm).</summary>
        public static IReadOnlyList<HelpLink> Credits { get; } = new[]
        {
            new HelpLink("esturnat, who found the name TraXile and does excellent testing", "https://www.youtube.com/@esturnat"),
            new HelpLink("Mr__Khan, for his input to the labyrinth features and great testing support", "https://www.twitch.tv/mr__khan"),
            new HelpLink("Michi, for a lot of design work including the logo and artwork", "https://github.com/seibmich"),
        };
    }
}
