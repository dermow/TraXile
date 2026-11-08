# TraXile.Next — modern UI (preview)

A from-scratch WPF user interface for TraXile with a Windows 11 Fluent look
(Mica, sidebar navigation, cards) and modern animated charts.
The classic WinForms app keeps working unchanged; screens are migrated one by one.

## Stack

| Concern  | Choice | Why |
|----------|--------|-----|
| UI       | WPF + [WPF-UI](https://github.com/lepoco/wpfui) 4.3 | Native Fluent controls, Mica, NavigationView |
| Charts   | [LiveCharts2](https://livecharts.dev) 2.0.5 (SkiaSharp) | Animated, rounded, tooltips, theming |
| MVVM     | CommunityToolkit.Mvvm | Observable objects and commands |
| Core     | Existing `TraXile` project (referenced directly) | Log parsing, SQLite, stats stay battle-tested |

Target framework is **.NET Framework 4.7.2** so the existing core can be referenced
without any changes (`InternalsVisibleTo("TraXile.Next")` exposes settings and paths).

## Architecture

```
Views (XAML)  ->  ViewModels  ->  ITrackingSource
                                   ├─ LiveTrackingSource  (wraps TrX_CoreLogic)
                                   └─ DemoTrackingSource  (sample data)
```

* `ITrackingSource` provides immutable snapshots (`ActivityInfo`, `LiveState`), so views
  never touch core objects that the parser threads are changing.
* Core events are debounced and marshalled to the UI thread in the view model.
* `ActivityPalette` gives each activity type the same color in every chart and list.

## Running

```
TraXile.Next.exe                         # live mode (falls back to demo, see below)
TraXile.Next.exe --demo                  # sample data
TraXile.Next.exe --light                 # light theme
TraXile.Next.exe --screenshot out.png    # render off-screen to PNG and exit (demo data)
TraXile.Next.exe --screenshot out.png --page log --select 0,2   # screenshot of the activity log with rows selected
```

Live mode needs an existing TraXile setup (config.xml with the Client.txt path and data.db).
It **refuses to run live while the classic TraXile is open**, because both would parse
Client.txt and write the same database. In that case it shows demo data and a notice.

## Migration status

| Classic tab / window       | New page        | Status |
|----------------------------|-----------------|--------|
| Tracking + global dashboard| Dashboard       | ✅ prototype (live card, KPIs, charts, recent runs) |
| Activity log + filters     | Activity log    | ✅ search, type/tag/time filters, sortable virtualized grid, tagging (single + bulk), CSV export, delete |
| All stats                  | Statistics      | ⏳ placeholder |
| Mapping dashboard          | Mapping         | ⏳ placeholder |
| Bossing dashboard          | Bossing         | ⏳ placeholder |
| Labs / Heist / Profit      | Labs & Heist    | ⏳ placeholder |
| Tag config                 | Tags            | ⏳ placeholder |
| Settings, backups, updates | Settings        | ⏳ placeholder |
| Overlays (stopwatch, tags) | —               | ⏳ to do (WPF transparent windows) |
| Activity details window    | Side panel      | ✅ facts, tags, side areas (in the activity log) |

Suggested order: Settings (so the new app can run standalone, including the
first-run Client.txt picker) → Overlays → remaining dashboards → installer switch.

## Known gaps / notes

* Settings are read-only for now; configure TraXile in the classic app.
* Deleting activities removes them from the database immediately but only hides them in the running
  core (its history list is not thread-safe); they are fully gone after the next start.
* JSON export of the classic app is not ported yet (CSV only).
* log4net 3.1.0 is pinned to match the core; it has a moderate advisory
  (GHSA-4f7c-pmjv-c25w). Upgrade both projects together.
* The core logs through log4net; without a `log4net.config` next to the exe, logging is off.
