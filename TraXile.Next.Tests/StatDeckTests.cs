using System;
using System.Collections.Generic;
using System.Linq;
using TraXile.Next.ViewModels;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>Shuffled deck behind the rotating Statistic tile on the dashboard.</summary>
    public class StatDeckTests
    {
        private static readonly string[] Keys = { "A", "B", "C", "D", "E" };

        [Fact]
        public void Every_statistic_is_shown_once_per_round()
        {
            var deck = new StatDeck(new Random(1));
            deck.Update(Keys);

            var round = new List<string> { deck.Current };
            for (int i = 1; i < Keys.Length; i++) round.Add(deck.Next());

            Assert.Equal(Keys, round.OrderBy(k => k));
        }

        [Fact]
        public void Never_the_same_statistic_twice_in_a_row()
        {
            var deck = new StatDeck(new Random(7));
            deck.Update(Keys);

            string last = deck.Current;
            for (int i = 0; i < 200; i++)
            {
                string next = deck.Next();
                Assert.NotEqual(last, next);
                last = next;
            }
        }

        [Fact]
        public void Current_stays_while_it_is_eligible()
        {
            var deck = new StatDeck(new Random(3));
            deck.Update(Keys);
            string current = deck.Current;

            bool changed = deck.Update(Keys.Concat(new[] { "F" }));

            Assert.False(changed);
            Assert.Equal(current, deck.Current);
        }

        [Fact]
        public void Moves_on_when_current_is_no_longer_eligible()
        {
            var deck = new StatDeck(new Random(3));
            deck.Update(Keys);
            string current = deck.Current;

            bool changed = deck.Update(Keys.Where(k => k != current));

            Assert.True(changed);
            Assert.NotNull(deck.Current);
            Assert.NotEqual(current, deck.Current);
        }

        [Fact]
        public void Removed_statistics_leave_the_round_and_new_ones_join_it()
        {
            var deck = new StatDeck(new Random(5));
            deck.Update(new[] { "A", "B", "C" });
            string first = deck.Current;
            string[] others = new[] { "A", "B", "C" }.Where(k => k != first).ToArray();

            // one not yet shown drops out, "X" comes in
            deck.Update(new[] { first, others[1], "X" });
            string[] rest = { deck.Next(), deck.Next() };

            Assert.Equal(new[] { others[1], "X" }.OrderBy(k => k), rest.OrderBy(k => k));
        }

        [Fact]
        public void Nothing_eligible_means_no_current()
        {
            var deck = new StatDeck(new Random(1));
            deck.Update(Keys);

            deck.Update(Array.Empty<string>());

            Assert.Null(deck.Current);
            Assert.Null(deck.Next());
            Assert.Equal(0, deck.Count);
        }

        [Fact]
        public void A_single_statistic_stays()
        {
            var deck = new StatDeck(new Random(1));
            deck.Update(new[] { "A" });

            Assert.Equal("A", deck.Current);
            Assert.Equal("A", deck.Next());
        }
    }
}
