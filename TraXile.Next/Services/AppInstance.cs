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
    /// <item>only one TraXile runs; a second start asks the running one to show its window and exits</item>
    /// <item>restart: start a new process that waits for the lock, then shut down</item>
    /// </list>
    /// </summary>
    public sealed class AppInstance : IDisposable
    {

        public const string DefaultMutexName = "TraXile.Next.SingleInstance.7e1d2c54-3f0b-4a8e-9b6d-2a51c0f4e913";

        /// <summary>Argument of a restarted process: wait for the old process to release the locks.</summary>
        public const string RestartedArg = "--restarted";

        private readonly string _ownName;
        private Mutex _own;
        private EventWaitHandle _showSignal;
        private RegisteredWaitHandle _showWait;

        public AppInstance(string ownMutexName = DefaultMutexName)
        {
            _ownName = ownMutexName;
        }

        /// <summary>Raised (on a thread pool thread) when another start asked this instance to show itself.</summary>
        public event EventHandler ShowRequested;

        public bool OwnsInstance => _own != null;

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

        /// <summary>Starts a new TraXile process; the caller shuts down afterwards, which releases the lock.</summary>
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
