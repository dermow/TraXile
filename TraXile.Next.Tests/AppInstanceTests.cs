using System;
using System.Threading;
using System.Threading.Tasks;
using TraXile.Next.Services;
using Xunit;

namespace TraXile.Next.Tests
{
    public class AppInstanceTests
    {
        // Unique names per test so tests never touch the real app's locks
        private static string Name(string what) => $"TraXile.Next.Test.{what}.{Guid.NewGuid():N}";

        /// <summary>Mutexes are per thread: a second instance must be simulated on another thread.</summary>
        private static T OnOtherThread<T>(Func<T> f) => Task.Factory.StartNew(f, TaskCreationOptions.LongRunning).Result;

        [Fact]
        public void Only_one_instance_can_run()
        {
            string own = Name("own"), classic = Name("classic");
            using (var first = new AppInstance(own, classic))
            {
                Assert.True(first.TryClaimInstance(TimeSpan.Zero));

                bool second = OnOtherThread(() =>
                {
                    using (var other = new AppInstance(own, classic)) return other.TryClaimInstance(TimeSpan.Zero);
                });

                Assert.False(second);
            }
        }

        [Fact]
        public void Second_start_asks_the_running_instance_to_show_itself()
        {
            string own = Name("own"), classic = Name("classic");
            using (var first = new AppInstance(own, classic))
            using (var shown = new ManualResetEventSlim())
            {
                first.ShowRequested += (s, e) => shown.Set();
                first.TryClaimInstance(TimeSpan.Zero);

                OnOtherThread(() =>
                {
                    using (var other = new AppInstance(own, classic)) return other.TryClaimInstance(TimeSpan.Zero);
                });

                Assert.True(shown.Wait(TimeSpan.FromSeconds(5)));
            }
        }

        [Fact]
        public void Instance_is_free_again_after_dispose()
        {
            string own = Name("own"), classic = Name("classic");
            var first = new AppInstance(own, classic);
            first.TryClaimInstance(TimeSpan.Zero);
            first.Dispose();

            bool second = OnOtherThread(() =>
            {
                using (var other = new AppInstance(own, classic)) return other.TryClaimInstance(TimeSpan.Zero);
            });

            Assert.True(second);
        }

        [Fact]
        public void Running_classic_app_blocks_live_tracking()
        {
            string own = Name("own"), classic = Name("classic");
            using (var classicApp = new Mutex(false, classic))
            {
                Assert.True(classicApp.WaitOne(0)); // the classic TraXile holds its lock

                bool claimed = OnOtherThread(() =>
                {
                    using (var next = new AppInstance(own, classic)) return next.TryClaimClassicLock(TimeSpan.Zero);
                });

                Assert.False(claimed);
                classicApp.ReleaseMutex();
            }
        }

        [Fact]
        public void Live_TraXile_Next_keeps_the_classic_app_out()
        {
            string own = Name("own"), classic = Name("classic");
            using (var next = new AppInstance(own, classic))
            {
                Assert.True(next.TryClaimClassicLock(TimeSpan.Zero));

                // what the classic Program.Main does
                bool classicStarts = OnOtherThread(() =>
                {
                    using (var m = new Mutex(false, classic)) return m.WaitOne(0, false);
                });

                Assert.False(classicStarts);
            }
        }

        [Fact]
        public void Lock_of_a_crashed_process_can_be_taken_over()
        {
            string own = Name("own"), classic = Name("classic");
            OnOtherThread(() =>
            {
                var m = new Mutex(false, own);
                m.WaitOne(0);
                return true; // thread ends without releasing -> abandoned
            });

            using (var next = new AppInstance(own, classic))
            {
                Assert.True(next.TryClaimInstance(TimeSpan.FromSeconds(1)));
            }
        }

        [Fact]
        public void Restart_keeps_arguments_and_adds_the_marker_once()
        {
            Assert.Equal(new[] { "--light", "--restarted" }, AppInstance.RestartArguments(new[] { "--light" }));
            Assert.Equal(new[] { "--light", "--restarted" }, AppInstance.RestartArguments(new[] { "--restarted", "--light" }));
        }
    }
}
