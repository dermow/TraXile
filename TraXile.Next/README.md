# TraXile.Next — the TraXile 3.0 app

The WPF user interface of TraXile 3.0 in the design system of exile-tools.com
(see [docs/design.md](../docs/design.md)), with sidebar navigation and modern animated charts.
It replaced the WinForms UI of 2.x ([ADR 0001](../docs/adr/0001-wpf-ui-replaces-winforms-in-3-0.md)).
The project is still called TraXile.Next, but it builds **`TraXile.exe`**, so shortcuts, taskbar pins and the
updater of a 2.x installation keep working after the upgrade.

## Projects

| Project | Output | Content |
|---------|--------|---------|
| `TraXile` | `TraXile.Core.dll` (class library) | Tracking core: log parsing, SQLite database, statistics, tag sounds |
| `TraXile.Next` | `TraXile.exe` | This app |
| `TraXile.Updater` | `TraXile.Updater.exe` | Downloads and runs the installer of a new release |
| `TraXile.Next.Tests` | | xUnit tests |

Release builds: `MSBuild TraXile.sln /p:Configuration=Release` (Visual Studio's MSBuild; the updater's WinForms
resources do not build with `dotnet build -c Release`). The installer takes its files from
`TraXile.Next\bin\Release\net472`.

## Stack

| Concern  | Choice | Why |
|----------|--------|-----|
| UI       | WPF + [WPF-UI](https://github.com/lepoco/wpfui) 4.3 | Controls and NavigationView, restyled with the design tokens |
| Charts   | [LiveCharts2](https://livecharts.dev) 2.0.5 (SkiaSharp) | Animated, rounded, tooltips, theming |
| MVVM     | CommunityToolkit.Mvvm | Observable objects and commands |
| Core     | `TraXile` project (`TraXile.Core.dll`) | Log parsing, SQLite, stats stay battle-tested |

Target framework is **.NET Framework 4.7.2** ([ADR 0002](../docs/adr/0002-stay-on-net472-until-parity.md));
`InternalsVisibleTo("TraXile")` in the core exposes settings and paths to the app.

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
TraXile.exe                              # live mode (falls back to demo, see below)
TraXile.exe      --demo                  # sample data
TraXile.exe      --light                 # light theme
TraXile.exe      --screenshot out.png    # render off-screen to PNG and exit (demo data)
TraXile.exe      --screenshot out.png --page log --select 0,2   # screenshot of the activity log with rows selected
TraXile.exe      --screenshot out.png --page gallery           # all base styles of the design system
TraXile.exe      --screenshot out.png --page firstrun          # first-run screen (also: firstrun-empty)
TraXile.exe      --screenshot out.png --page overlay-stopwatch # stopwatch overlay (also: overlay-stopwatch-move)
TraXile.exe      --screenshot out.png --page overlay-tags      # tags overlay (also: overlay-tags-move)
TraXile.exe      --screenshot out.png --page tags --select 0   # Tags page with the first tag selected
TraXile.exe      --screenshot out.png --page settings --scroll 1500 # lower part of a long page
TraXile.exe      --screenshot out.png --page log --filter last7days,map,level>=83,tag:juiced # with the global filter set
                                                               # (also: league, custom, area:<name>, and, not)
TraXile.exe      --screenshot out.png --update                 # title bar with the "update available" hint
TraXile.exe      --screenshot out.png --page update            # update dialog (also: update-none)
```

## Updates

At start and then every hour the app reads `versions.xml` from the metadata server
(`{METADATA_BASE_URL}/{metadata_updates_branch}/versions.xml`, like the classic app). A newer version shows a
button in the title bar; at start the update dialog opens directly. "Install update" starts
`TraXile.Updater.exe <version>` (built and copied next to the exe), which downloads the release's `Setup.msi`,
closes TraXile and runs the installer. Installer packages older than a day are removed from `%APPDATA%\TraXile`.
The league metadata (`metadata.xml`) is downloaded at every start as well.

## Installer

`TraXile_Setup.aip` (Advanced Installer, freeware project) packages `TraXile.Next\bin\Release\net472` without
`*.pdb`, `*.dylib` and the x86/arm64 SkiaSharp folders (the app is x64 only; the package refuses 32-bit Windows).
It installs per user to `%APPDATA%\dermow\TraXile` like 2.x, with desktop and start menu shortcuts to
`TraXile.exe`. Same UpgradeCode as 2.x, so installing 3.0 removes 2.x first (major upgrade); the data folder
`%APPDATA%\TraXile` is not touched by the installer.

The freeware edition cannot synchronize folders: when the build output gets new files (a new package), add them
with `AdvancedInstaller.com /edit TraXile_Setup.aip /AddFile APPDIR <file>`. For a release:
`AdvancedInstaller.com /edit TraXile_Setup.aip /SetVersion <x.y.z>` (new ProductCode) and
`AdvancedInstaller.com /build TraXile_Setup.aip` → `TraXile_Setup-SetupFiles\TraXile_Setup.msi`. The updater
downloads the release asset `Setup.msi`.

## Tests

```
dotnet test TraXile.Next.Tests
```

`TraXile.Next.Tests` (xUnit, net472) checks the view models against `DemoTrackingSource` without
any UI, including time budgets for a 20k activity history.

Live mode uses the data folder of TraXile 2.x unchanged (`%APPDATA%\TraXile`: config.xml, data.db, backups,
overlay positions), so an upgrade keeps everything. Without a configured Client.txt a first-run screen offers the
detected Steam / standalone installations and a file picker; the choice is written to config.xml.

## Migration status

The plan and its status live in the GitHub issues with the label `ui-rework`, grouped by milestone:

1. [3.0 - Phase 1: Standalone](https://github.com/dermow/TraXile/milestone/10) - design system, navigation, settings, first run
2. [3.0 - Phase 2: Daily tracking](https://github.com/dermow/TraXile/milestone/11) - tags, overlays, live card actions, global filter
3. [3.0 - Phase 3: Analytics](https://github.com/dermow/TraXile/milestone/12) - Statistics, Mapping, Lab, Heist, Leagues, Div Cards
4. [3.0 - Phase 4: Switch](https://github.com/dermow/TraXile/milestone/13) - updater, installer, removal of the WinForms UI

Navigation: Dashboard, Activity log, Statistics, Mapping, Lab, Heist, Leagues, Div Cards, Tags, and Help and
Settings in the footer. The Help page replaces the classic Chat commands, Search help and About windows
(`--page help`). Its Support section replaces the classic DebugInfo window: copy the support info, or create
`%APPDATA%\TraXile\Support\TraXile_Support_Bundle_<date>.zip` (data folder without backups, Client.txt,
`debug_info.txt`, and a listing of the app folder instead of a copy of its binaries). Secrets in the settings are masked.

## Known gaps / notes

* Deleting activities removes them from the database immediately but only hides them in the running
  core (its history list is not thread-safe); they are fully gone after the next start.
* log4net 3.1.0 is pinned in the core and the app; it has a moderate advisory
  (GHSA-4f7c-pmjv-c25w). Upgrade both projects together.
* The core logs through log4net; without a `log4net.config` next to the exe, logging is off.
