# TraXile.Next design system

TraXile.Next follows the design system of exile-tools.com so the app and the website that presents it look
like one product (see [ADR 0004](adr/0004-adopt-exile-tools-design-system.md)).

## Source and snapshot

| | |
|---|---|
| Source | `https://dev.exile-tools.com/assets/css/foundation.css` and `components.css` (segmented control from `pages.css`) |
| Snapshot | **2026-11-08** (`DesignTokens.SnapshotDate`) |
| Sync | deliberately, through a ticket. The CSS is not pulled at build time because `dev.` is still changing. |

To sync: compare the theme blocks (`:root[data-theme="dark"]`, `:root[data-theme="light"]`) of the current
`foundation.css` with the tables below, update `TraXile.Next/Theme/DesignTokens.cs`, bump `SnapshotDate`
and check the style gallery in both themes.

## How it is built

* `Theme/DesignTokens.cs` is the single source of the token values. For every token it creates a
  `<Key>Color` and a frozen `<Key>Brush` resource, and points WPF-UI's own brush keys at the tokens
  (`DesignTokens.WpfUiBrushes`), so the library controls follow the design system without being retemplated.
* `App.ApplyTheme(dark)` applies WPF-UI's theme, then adds the token dictionary of the theme as the **last**
  merged dictionary (so it wins over the library defaults) and sets the coral accent. A theme switch only
  swaps this dictionary; views must therefore reference colors with `{DynamicResource ...}`.
* `Theme/Styles.xaml` holds fonts, shape and all base styles (theme-independent).
* The style gallery shows every base style:
  `TraXile.Next.exe --screenshot gallery.png --page gallery [--light]`.

## Color tokens

Use the WPF resource, never a literal color. `Brush` resources: `{DynamicResource BgBrush}`;
`Color` resources: `{DynamicResource BgColor}`.

### Surfaces and text

| WPF resource | CSS token | Dark | Light | Use |
|---|---|---|---|---|
| `Bg` | `--bg` | `#131113` | `#F6F1EC` | Window and page background (solid, no Mica) |
| `Surface` | `--surface` | `#1C191B` | `#FFFCF9` | Quiet panel: cards, filter bar, inputs |
| `SurfaceRaised` | `--surface-raised` | `#252124` | `#ECE6E0` | Menus, drop-downs, tool tips, pressed state |
| `SurfaceRow` | `--surface-row` | `#1B181A` | `#FCF9F5` | Row cards (list entries, grid rows) |
| `SurfaceRowHover` | `--surface-row-hover` | `#242022` | `#FFFFFF` | Row card under the pointer |
| `Sunken` | `--footer-bg` | `#0F0D0E` | `#ECE5DE` | Areas that sink below the page |
| `Text` | `--text` | `#F2ECE6` | `#221B1C` | All text |
| `TextMuted` | `--text-muted` | `#ADA5A1` | `#5E5557` | Secondary text, captions, labels |
| `Border` | `--border` | `#3A3437` | `#DAD2CB` | The rare rule that is still needed |
| `BorderStrong` | `--border-strong` | `#514A4D` | `#ADA39B` | Outlines of inputs and secondary buttons |

### Accent (coral) - reserved for UI accents, never an activity type color

| WPF resource | CSS token | Dark | Light | Use |
|---|---|---|---|---|
| `Accent` | `--accent` | `#D95C65` | `#C74450` | Active marks, selection edge, chart line |
| `AccentHover` | `--accent-hover` | `#E7797D` | `#9D3846` | Accent under the pointer |
| `AccentText` | `--accent-text` | `#E7797D` | `#9D3846` | Accent for small text and icons |
| `AccentSurface` | `--accent-surface` | `#3A1C23` | `#FBF0EF` | Selected row, danger hover |
| `OnAccent` | `--on-accent` | `#FFFFFF` | `#FFFFFF` | Text on filled coral |
| `BtnPrimaryBg` | `--btn-primary-bg` | `#C74450` | `#9D3846` | Primary button, checked segment, toggles |
| `BtnPrimaryHover` | `--btn-primary-hover` | `#B53F4C` | `#7A2D3A` | |
| `BtnPrimaryActive` | `--btn-primary-active` | `#9D3846` | `#5E2430` | |
| `Focus` | `--focus` | `#E7797D` | `#9D3846` | Focus outline (2 px) |
| `Selection` | `::selection` | `#9D3846` | `#9D3846` | Selected text |
| `CoralHighlight` ... `CoralDeep` | `--coral-*` | | | The five coral anchors (theme-independent) |

### Status, material and data colors

| WPF resource | CSS token | Dark | Light | Use |
|---|---|---|---|---|
| `StatusSuccess` | `--status-success` | `#74AE6C` | `#3E7637` | "Live", faster than average |
| `StatusWarning` | `--status-warning` | `#D3A64A` | `#8A6416` | Warnings |
| `StatusDanger` | `--status-danger` | `#E7797D` | `#9D3846` | Destructive actions, errors |
| `Brass` | `--brass` | `#B8935A` | `#8A6A35` | Hairline light, chart markers (deaths) |
| `BrassText` | `--brass-text` | `#CDAE7C` | `#6E5228` | Brass for text |
| `DataRust` | `--gem-str` | `#D07457` | `#A44A30` | Data color |
| `DataGreen` | `--gem-dex` | `#74AE6C` | `#3E7637` | Data color |
| `DataBlue` | `--gem-int` | `#6F93D6` | `#3B5EA6` | Data color |
| `DataOchre` | `--status-warning` | `#D3A64A` | `#8A6416` | Data color |
| `DataNeutral` | `--gem-none` | `#ADA5A1` | `#5E5557` | Data color for "other" |

### Depth and derived values

| WPF resource | Source | Dark | Light |
|---|---|---|---|
| `Shadow` | `--shadow-color` | `#000000` | `#463024` |
| `Scrim` | `--scrim` | black 55 % | `#191A1C` 40 % |
| `HoverVeil` | derived | text 8 % | text 6 % |
| `PressVeil` | derived | text 4 % | text 3 % |
| `TextDisabled` | derived | text-muted 50 % | text-muted 50 % |

## Typography

Three families, bundled as TTF in `TraXile.Next/Fonts` (SIL OFL 1.1, licence files next to them and copied to
the output folder). WPF cannot use the website's variable WOFF2 files, so static instances are bundled.

| Resource | Family | Use | Never |
|---|---|---|---|
| `DisplayFont` | Orbitron (SemiBold 600, Bold 700) | Page titles (`PageTitle`), small labels (`Label`, `Overline`), the wordmark | Figures, tables, running text, forms, values that change (its digits are proportional) |
| `HeadFont` | Familjen Grotesk (400-700) | Headings `H2`-`H4`, `CardTitle`, navigation, segmented control, prominent figures (`Metric`) | |
| `UiFont` | Instrument Sans (400-700, italic) | All text and UI (`Body`, `Meta`, `Caption`, inputs, buttons, grids) | |

One component speaks with at most two of them: Orbitron for the name, Instrument Sans for what is read.

| Style | Font | Size | Weight |
|---|---|---|---|
| `PageTitle` | Orbitron | 26 | SemiBold |
| `Label` / `Overline` | Orbitron | 12 (floor for this face) | SemiBold |
| `H2` | Familjen Grotesk | 22 | SemiBold |
| `H3` | Familjen Grotesk | 18 | SemiBold |
| `H4` / `CardTitle` | Familjen Grotesk | 16 | SemiBold |
| `Metric` | Familjen Grotesk, tabular numerals | 28 | SemiBold |
| `Body` / `Num` | Instrument Sans (`Num`: tabular) | 14 | Regular |
| `Meta` | Instrument Sans, `TextMuted` | 13 | Regular |
| `Caption` | Instrument Sans, `TextMuted` | 12 (smallest size in use) | Regular |

Sizes are a desktop scale of the website's type scale (web body 16 px -> app body 14). WPF has no letter
spacing, so the website's `0.05em` label tracking is not reproduced.

## Shape and spacing

* Controls (buttons, inputs, combo boxes, segments, chips): **2 px** corners (`ControlRadius`).
* Surfaces (cards, row cards, dialogs, menus): **4 px** corners (`SurfaceRadius`).
* Everything sits on a **4 px grid**: 4, 8, 12, 16, 24, 32, 48. Page padding `PagePadding` = 32/24.
* **Surfaces instead of hairlines.** Content is separated by space and by surfaces of slightly different
  brightness (`Bg` -> `Surface` / `SurfaceRow` -> `SurfaceRowHover`), not by rules. Cards have no border.
* Depth comes from very soft shadows (`RowShadow`) on row cards only.

## Base styles (`Theme/Styles.xaml`)

| Element | Style | Notes |
|---|---|---|
| Buttons | implicit for `ui:Button`, variant by `Appearance` | `Primary` filled coral, `Secondary` outlined (default), `Transparent` text/icon button, `Danger` outlined in `StatusDanger`. 36 px high, `SmallButton` 30 px. Lifts 1 px under the pointer, 45 % opacity when disabled. |
| Text box | implicit for `ui:TextBox` | WPF-UI template, `Surface` background, `BorderStrong` outline, `Focus` when focused |
| Combo box | implicit for `ComboBox` | as text box; drop-down on `SurfaceRaised` |
| Toggle switch / check box | implicit | filled with `BtnPrimaryBg` when on |
| Segmented control | `SegmentedGroup` (border) + `Segment` (radio button) | Familjen Grotesk 13; checked option filled with `BtnPrimaryBg` |
| Card | `Card` (alias `DashCard`) | quiet panel on `Surface`, 4 px, padding 20 |
| Row card | `RowCard`, `LiveRowCard` | `SurfaceRow`, soft shadow, 8 px gap; live variant brightens under the pointer |
| Notice | `Notice` | `Surface` with a 3 px accent mark on the left |
| Data grid | implicit for `DataGrid` (`CardRow`, `CardCell`, `CardColumnHeader`) | every row is a row card (4 px gap, no grid lines); selected = `AccentSurface` + 3 px coral edge; headers muted 12 px with a coral sort arrow. Rows have no shadow (performance with large lists). |
| Navigation | implicit for `ui:NavigationViewItem` | Familjen Grotesk 15 SemiBold, muted; selected item on `SurfaceRow` with the coral indicator |
| Title bar | `MainWindow` | solid `Bg`, Orbitron wordmark, mode badge as `Label` |
| Scroll bars | WPF-UI, colors from tokens | thumb `BorderStrong` |
| Dialogs | `ui:MessageBox`, colors from tokens | content on `Surface`, button footer on `SurfaceRow`, `Scrim` behind |
| Focus | `FocusRing` | 2 px `Focus` outline, offset 2 px |

## Motion

Tactile response only (160-260 ms, at most a 1-4 px shift, no bounce): buttons lift 1 px under the pointer,
rows brighten. No atmospheric drift or energy trace in the app.
