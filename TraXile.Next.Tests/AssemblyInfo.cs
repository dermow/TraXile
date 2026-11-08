using Xunit;

// View models use DispatcherTimers and shared static caches; run tests one after another.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
