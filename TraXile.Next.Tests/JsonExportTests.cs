using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TraXile.Next.Services;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>JSON export of the activity log, compared with the classic Main.WriteActivitiesToJSON.</summary>
    public class JsonExportTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "traxile-json-" + Guid.NewGuid().ToString("N"));

        public JsonExportTests() => Directory.CreateDirectory(_dir);

        public void Dispose() => Directory.Delete(_dir, recursive: true);

        private static TrX_TrackedActivity CoreActivity(string area, ACTIVITY_TYPES type, DateTime started, int seconds, int level, int deaths, params string[] tags)
        {
            var a = new TrX_TrackedActivity
            {
                Area = area,
                Type = type,
                Started = started,
                TimeStamp = ((DateTimeOffset)started).ToUnixTimeSeconds(),
                TotalSeconds = seconds,
                AreaLevel = level,
                DeathCounter = deaths
            };
            foreach (string t in tags) a.AddTag(t);
            return a;
        }

        /// <summary>The classic implementation (Main.WriteActivitiesToJSON), which lives in a WinForms form.</summary>
        private static void ClassicWrite(string path, IEnumerable<TrX_TrackedActivity> src)
        {
            StreamWriter wrt = new StreamWriter(path);
            var serializables = new List<TrX_BackendSync_ActivityDocument>();
            foreach (TrX_TrackedActivity act in src) serializables.Add(act.GetSerializableObject());
            wrt.WriteLine(JsonConvert.SerializeObject(serializables, Formatting.Indented));
            wrt.Close();
        }

        [Fact]
        public void Output_equals_the_classic_export()
        {
            var known = new Dictionary<string, TrX_ActivityTag>
            {
                ["blight"] = new TrX_ActivityTag("blight"),
                ["juiced"] = new TrX_ActivityTag("juiced", false)
            };
            TrX_TrackedActivity[] core =
            {
                CoreActivity("Strand", ACTIVITY_TYPES.MAP, new DateTime(2026, 11, 8, 21, 17, 3), 389, 83, 0, "blight", "juiced"),
                // apostrophe (the core's Area strips it, the export keeps it) and a deleted tag
                CoreActivity("Maven's Crucible", ACTIVITY_TYPES.MAVEN_FIGHT, new DateTime(2026, 11, 7, 18, 0, 59), 245, 85, 2, "deleted-tag"),
                CoreActivity("Bunker", ACTIVITY_TYPES.HEIST, new DateTime(2025, 1, 2, 3, 4, 5), 0, 83, 0)
            };
            string classic = Path.Combine(_dir, "classic.json"), next = Path.Combine(_dir, "next.json");

            ClassicWrite(classic, core);
            ActivityLogViewModel.ExportJson(next, core.Select(a => LiveTrackingSource.Convert(a, id => known.TryGetValue(id, out var t) ? t : null)));

            Assert.Equal(File.ReadAllBytes(classic), File.ReadAllBytes(next));
            string json = File.ReadAllText(next);
            Assert.Contains("\"AreaName\": \"Maven's Crucible\"", json);
            Assert.Contains("\"deleted-tag\"", json);
        }

        [Fact]
        public void Export_of_demo_activities_round_trips()
        {
            using (var source = new DemoTrackingSource())
            {
                ActivityInfo[] activities = source.GetHistory().Take(20).ToArray();
                string path = Path.Combine(_dir, "demo.json");

                ActivityLogViewModel.ExportJson(path, activities);

                byte[] bytes = File.ReadAllBytes(path);
                Assert.NotEqual(0xEF, bytes[0]); // no BOM, like the classic StreamWriter
                var docs = JsonConvert.DeserializeObject<List<TrX_BackendSync_ActivityDocument>>(File.ReadAllText(path));
                Assert.Equal(activities.Length, docs.Count);
                for (int i = 0; i < activities.Length; i++)
                {
                    Assert.Equal(activities[i].Id, docs[i].Identifier);
                    Assert.Equal(activities[i].Type.ToString(), docs[i].ActivityType);
                    Assert.Equal(activities[i].TotalSeconds, docs[i].DurationSec);
                    Assert.Equal(activities[i].Tags.Select(t => t.Id), docs[i].Tags);
                    Assert.Equal(activities[i].Started, docs[i].StartTime);
                }
            }
        }

        [Fact]
        public void Empty_list_gives_an_empty_array()
        {
            string path = Path.Combine(_dir, "empty.json");
            ActivityLogViewModel.ExportJson(path, Array.Empty<ActivityInfo>());
            Assert.Equal("[]", File.ReadAllText(path).Trim());
        }
    }
}
