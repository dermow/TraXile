using System;
using System.Collections.Generic;
using System.Linq;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Shuffled deck of Statistic keys for the rotating dashboard tile: every eligible Statistic is shown once
    /// before the deck is reshuffled, so none repeats until all have been shown.
    /// </summary>
    public sealed class StatDeck
    {
        private readonly Random _random;
        private readonly HashSet<string> _pool = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Keys not yet shown in this round, the next one first.</summary>
        private readonly List<string> _remaining = new List<string>();

        public StatDeck(Random random = null) => _random = random ?? new Random();

        /// <summary>The Statistic currently shown; null while none is eligible.</summary>
        public string Current { get; private set; }

        /// <summary>Number of eligible Statistics.</summary>
        public int Count => _pool.Count;

        /// <summary>
        /// Replaces the eligible pool. New keys join the running round at a random place, keys that are no longer
        /// eligible leave it. <see cref="Current"/> stays while it is still eligible, otherwise the deck moves on.
        /// </summary>
        /// <returns>True if <see cref="Current"/> changed.</returns>
        public bool Update(IEnumerable<string> eligible)
        {
            var pool = new HashSet<string>(eligible ?? Enumerable.Empty<string>(), StringComparer.Ordinal);

            _remaining.RemoveAll(k => !pool.Contains(k));
            foreach (string key in pool.Where(k => !_pool.Contains(k)).OrderBy(k => k, StringComparer.Ordinal))
            {
                if (key == Current) continue;
                _remaining.Insert(_random.Next(_remaining.Count + 1), key);
            }
            _pool.Clear();
            _pool.UnionWith(pool);

            if (Current != null && _pool.Contains(Current)) return false;
            string before = Current;
            Next();
            return before != Current;
        }

        /// <summary>Moves on to the next Statistic; starts a new, reshuffled round when all have been shown.</summary>
        public string Next()
        {
            if (_pool.Count == 0)
            {
                _remaining.Clear();
                return Current = null;
            }
            if (_remaining.Count == 0)
            {
                // New round; the one just shown goes last, so it never comes twice in a row
                List<string> round = _pool.Where(k => k != Current).OrderBy(k => k, StringComparer.Ordinal).ToList();
                Shuffle(round);
                if (Current != null && _pool.Contains(Current)) round.Add(Current);
                _remaining.AddRange(round);
            }
            Current = _remaining[0];
            _remaining.RemoveAt(0);
            return Current;
        }

        private void Shuffle(List<string> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
