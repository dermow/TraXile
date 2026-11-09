# TraXile 3.0 ships as TraXile.exe, the core as TraXile.Core.dll

The WPF app (project `TraXile.Next`) builds `TraXile.exe` and the core library (project `TraXile`) builds `TraXile.Core.dll`, both installed to the same folder as 2.x. We kept the 2.x file name so that the upgrade does not break desktop and start menu shortcuts, taskbar pins and the updater, which closes running `TraXile` processes; a core named `TraXile.dll` next to `TraXile.exe` would have clashed with the app's assembly name. The project folders keep their names to avoid churn.
