using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TraXile.Next.Services
{
    /// <summary>
    /// Process-wide locks and restart:
    /// <list type="bullet">
    /// <item>only one TraXile.Next runs; a second start asks the running one to show its window and exits</item>
    /// <item>while TraXile.Next tracks live it also holds the classic TraXile's own single-instance mutex, so the
    /// classic app refuses to start (both would parse Client.txt and write the same database), and a running
    /// classic app keeps TraXile.Next in demo mode</item>
    /// <item>restart: start a new process that waits for the locks, then shut down</item>
    /// </list>
    /// </summary>
    public sealed class AppInstance : IDisposable
    {
        /// <summary>Mutex name of the classic TraXile (TraXile/Program.cs).</summary>
        public const string ClassicMutexName = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        public const string DefaultMutexName = "TraXile.Next.SingleInstance.7e1d2c54-3f0b-4a8e-9b6d-2a51c0f4e913";

        /// <summary>Argument of a restarted process: wait for the old process to release the locks.</summary>
        public const string RestartedArg = "--restarted";

        private readonly string _ownName;
        private readonly string _classicName;
        private Mutex _own;
        private Mutex _classic;
        private EventWaitHandle _showSignal;
        private RegisteredWaitHandle _showWait;

        public AppInstance(string ownMutexName = DefaultMutexName, string classicMutexName = ClassicMutexName)
        {
            _ownName = ownMutexName;
            _classicName = classicMutexName;
        }

        /// <summary>Raised (on a thread pool thread) when another start asked this instance to show itself.</summary>
        public event EventHandler ShowRequested;

        public bool OwnsInstance => _own != null;
        public bool OwnsClassicLock => _classic != null;

        /// <summary>Claims the single instance. False if another TraXile.Next runs (it is asked to show itself).</summary>
        public bool TryClaimInstance(TimeSpan wait)
        {
            _own = TryAcquire(_ownName, wait);
            if (_own == null)
            {
                SignalRunningInstance();
                return false;
            }

            _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, _ownName + ".Show");
            _showWait = ThreadPool.RegisterWaitForSingleObject(_showSignal,
                (state, timedOut) => ShowRequested?.Invoke(this, EventArgs.Empty), null, Timeout.Infinite, executeOnlyOnce: false);
            return true;
        }

        /// <summary>Takes the classic app's lock. False if the classic TraXile is running.</summary>
        public bool TryClaimClassicLock(TimeSpan wait)
        {
            _classic = TryAcquire(_classicName, wait);
            return _classic != null;
        }

        private void SignalRunningInstance()
        {
            try
            {
                using (EventWaitHandle signal = EventWaitHandle.OpenExisting(_ownName + ".Show")) signal.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // the other instance is just starting or shutting down
            }
        }

        private static Mutex TryAcquire(string name, TimeSpan wait)
        {
            var mutex = new Mutex(false, name);
            try
            {
                if (mutex.WaitOne(wait, false)) return mutex;
            }
            catch (AbandonedMutexException)
            {
                return mutex; // previous owner crashed: the lock is ours now
            }
            mutex.Dispose();
            return null;
        }

        /// <summary>Arguments for the restarted process: the current ones (minus a previous restart marker) plus the marker.</summary>
        public static string[] RestartArguments(IEnumerable<string> currentArgs) =>
            currentArgs.Where(a => !a.Equals(RestartedArg, StringComparison.OrdinalIgnoreCase))
                       .Concat(new[] { RestartedArg })
                       .ToArray();

        /// <summary>Starts a new TraXile.Next process; the caller shuts down afterwards, which releases the locks.</summary>
        public static void StartNewProcess(IEnumerable<string> currentArgs)
        {
            string exe = Process.GetCurrentProcess().MainModule.FileName;
            string args = string.Join(" ", RestartArguments(currentArgs).Select(Quote));
            Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = false });
        }

        private static string Quote(string arg) => arg.Contains(" ") ? "\"" + arg.Replace("\"", "\\\"") + "\"" : arg;

        public void Dispose()
        {
            _showWait?.Unregister(null);
            _showSignal?.Dispose();
            Release(ref _classic);
            Release(ref _own);
        }

        private static void Release(ref Mutex mutex)
        {
            if (mutex == null) return;
            try { mutex.ReleaseMutex(); } catch (ApplicationException) { /* not owned by this thread */ }
            mutex.Dispose();
            mutex = null;
        }
    }
}
