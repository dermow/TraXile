# TraXile 3.0 ships on .NET 10, self-contained

All projects (core, app, tests, updater) move from .NET Framework 4.7.2 to `net10.0-windows` (x64 only) as part of 3.0, on a dedicated branch off `feature/ui-rework`, instead of after 3.0 as ADR 0002 planned. .NET 10 is the newest LTS (supported until Nov 2028), and doing both changes before the first 3.0 release spares users a second disruptive upgrade.

The app is published **self-contained** (folder layout, no single-file, no trimming), with the updater published into the same folder so both share one runtime. The deciding reason: the 2.x updater already on users' machines downloads and runs `Setup.msi` directly, and a plain MSI cannot install the .NET Desktop Runtime prerequisite. A framework-dependent 3.0 would therefore not start after an in-app update from 2.x.

## Considered Options

- **Framework-dependent + runtime prerequisite**: smaller downloads and runtime patched by Microsoft Update, but breaks the 2.x in-app update path (only the `Setup.exe` bootstrapper installs prerequisites).
- **Framework-dependent + MSI launch condition**: aborts with a download hint for 2.x updaters; rejected as too rough for the one-time 3.0 upgrade.
- **Single-file**: slower first start (native libraries get extracted) and `log4net.config`/`audio\` stay external anyway.

## Consequences

- TraXile owns runtime servicing: every regular release ships the latest .NET 10 patch, and runtime CVEs affecting what TraXile uses (networking/HTTP, TLS) justify an out-of-band release, which narrowly extends the hotfix exception of ADR 0003.
- The runtime version is pinned centrally (`Directory.Build.props`) so app and updater always share the same runtime files.
- The installer syncs the publish folder instead of listing files individually.
