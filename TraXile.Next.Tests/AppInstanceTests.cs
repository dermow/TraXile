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
            string own = Name("own");
            using (var first = new AppInstance(own))
            {
                Assert.True(first.TryClaimInstance(TimeSpan.Zero));

                bool second = OnOtherThread(() =>
                {
                    using (var other = new AppInstance(own)) return other.TryClaimInstance(TimeSpan.Zero);
                });

                Assert.False(second);
            }
        }

        [Fact]
        public void Second_start_asks_the_running_instance_to_show_itself()
        {
            string own = Name("own");
            using (var first = new AppInstance(own))
            using (var shown = new ManualResetEventSlim())
            {
                first.ShowRequested += (s, e) => shown.Set();
                first.TryClaimInstance(TimeSpan.Zero);

                OnOtherThread(() =>
                {
                    using (var other = new AppInstance(own)) return other.TryClaimInstance(TimeSpan.Zero);
                });

                Assert.True(shown.Wait(TimeSpan.FromSeconds(5)));
            }
        }

        [Fact]
        public void Instance_is_free_again_after_dispose()
        {
            string own = Name("own");
            var first = new AppInstance(own);
            first.TryClaimInstance(TimeSpan.Zero);
            first.Dispose();

            bool second = OnOtherThread(() =>
            {
                using (var other = new AppInstance(own)) return other.TryClaimInstance(TimeSpan.Zero);
            });

            Assert.True(second);
        }

        [Fact]
        public void Lock_of_a_crashed_process_can_be_taken_over()
        {
            string own = Name("own");
            OnOtherThread(() =>
            {
                var m = new Mutex(false, own);
                m.WaitOne(0);
                return true; // thread ends without releasing -> abandoned
            });

            using (var next = new AppInstance(own))
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
