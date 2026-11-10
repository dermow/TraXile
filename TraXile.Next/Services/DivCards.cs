using System;
using System.Collections.Generic;

namespace TraXile.Next.Services
{
    /// <summary>A divination card drawn in a time window (core table tx_divcards).</summary>
    public sealed class DivCardInfo
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public DateTime FirstDraw { get; set; }
        public DateTime LastDraw { get; set; }

        public string FirstText => FirstDraw.ToString("yyyy-MM-dd HH:mm");
        public string LastText => LastDraw.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>The divination cards the core records from the log. Not part of every tracking source.</summary>
    public interface IDivCardSource
    {
        /// <summary>Cards drawn in [from, to], most drawn first (classic TrX_DataBackend.GetDivCardStats).</summary>
        IReadOnlyList<DivCardInfo> GetDivCards(DateTime from, DateTime to);

        /// <summary>
        /// A card was drawn (raised on a worker thread). Separate from HistoryChanged on purpose: a draw does not
        /// change the Activities, and rebuilding every page per draw freezes the UI while a stack of decks is opened.
        /// </summary>
        event EventHandler DivCardsChanged;
    }
}
