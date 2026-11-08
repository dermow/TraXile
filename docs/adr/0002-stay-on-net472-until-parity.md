# Stay on .NET Framework 4.7.2 until UI parity

The new WPF UI targets .NET Framework 4.7.2, although .NET 8 would be the modern choice, so that it can reference the existing, battle-tested core without changes. Migrating the core to .NET 8 (removing WinForms dependencies, replacing `System.Data.SQLite`, new deployment) is a separate step after 3.0, so the UI rework and the runtime migration never happen at the same time.
