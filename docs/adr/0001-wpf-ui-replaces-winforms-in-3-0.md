# The WPF UI (TraXile.Next) replaces the WinForms UI in 3.0

TraXile 3.0 ships only the new WPF UI; the classic WinForms UI is removed and the `TraXile` project becomes a pure core library (log parsing, database, statistics). We rejected keeping both UIs side by side because they would parse the same Client.txt and write the same database, forcing a "never run both" lock and double maintenance for no user benefit. Version 3.0 aims at strict feature parity with the classic UI; new features (e.g. a bossing page, profit tracking view) come after 3.0.
