using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TraXile.Core")]
[assembly: AssemblyDescription("Tracking core of TraXile, the tracking tool for Path of Exile")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("dermow")]
[assembly: AssemblyProduct("TraXile")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("2d010e92-b659-4567-b1d0-9f6474d124e2")]

// Same version as the app (TraXile.Next.csproj); TrX_Static.VERSION reads it
[assembly: AssemblyVersion("3.0.0")]
[assembly: AssemblyFileVersion("3.0.0")]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

// The UI (TraXile.Next, assembly "TraXile") uses the core's internal helpers (settings, paths)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TraXile")]
