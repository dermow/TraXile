# TraXile.Next — modern UI (preview)

A from-scratch WPF user interface for TraXile in the design system of exile-tools.com
(see [docs/design.md](../docs/design.md)), with sidebar navigation and modern animated charts.
The classic WinForms app keeps working unchanged; screens are migrated one by one.

## Stack

| Concern  | Choice | Why |
|----------|--------|-----|
| UI       | WPF + [WPF-UI](https://github.com/lepoco/wpfui) 4.3 | Controls and NavigationView, restyled with the design tokens |
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
TraXile.Next.exe --screenshot out.png --page gallery           # all base styles of the design system
TraXile.Next.exe --screenshot out.png --page firstrun          # first-run screen (also: firstrun-empty)
TraXile.Next.exe --screenshot out.png --page overlay-stopwatch # stopwatch overlay (also: overlay-stopwatch-move)
TraXile.Next.exe --screenshot out.png --page tags --select 0   # Tags page with the first tag selected
```

## Tests

```
dotnet test TraXile.Next.Tests
```

`TraXile.Next.Tests` (xUnit, net472) checks the view models against `DemoTrackingSource` without
any UI, including time budgets for a 20k activity history.

Live mode uses the same data folder as the classic app (`%APPDATA%\TraXile`: config.xml, data.db).
Without a configured Client.txt a first-run screen offers the detected Steam / standalone installations
and a file picker; the choice is written to config.xml, so the classic app reads it too.
It **refuses to run live while the classic TraXile is open**, because both would parse
Client.txt and write the same database. In that case it shows demo data and a notice.

## Migration status

The plan and its status live in the GitHub issues with the label `ui-rework`, grouped by milestone:

1. [3.0 - Phase 1: Standalone](https://github.com/dermow/TraXile/milestone/10) - design system, navigation, settings, first run
2. [3.0 - Phase 2: Daily tracking](https://github.com/dermow/TraXile/milestone/11) - tags, overlays, live card actions, global filter
3. [3.0 - Phase 3: Analytics](https://github.com/dermow/TraXile/milestone/12) - Statistics, Mapping, Lab, Heist, Leagues, Div Cards
4. [3.0 - Phase 4: Switch](https://github.com/dermow/TraXile/milestone/13) - updater, installer, removal of the WinForms UI

Navigation: Dashboard, Activity log, Statistics, Mapping, Lab, Heist, Leagues, Div Cards, Tags, and Settings in
the footer. Pages that are not migrated yet show a placeholder.

## Known gaps / notes

* Deleting activities removes them from the database immediately but only hides them in the running
  core (its history list is not thread-safe); they are fully gone after the next start.
* JSON export of the classic app is not ported yet (CSV only).
* log4net 3.1.0 is pinned to match the core; it has a moderate advisory
  (GHSA-4f7c-pmjv-c25w). Upgrade both projects together.
* The core logs through log4net; without a `log4net.config` next to the exe, logging is off.
