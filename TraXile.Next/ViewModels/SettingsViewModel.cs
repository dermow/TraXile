using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TraXile.Next.Services;

namespace TraXile.Next.ViewModels
{
    /// <summary>
    /// Settings page. Every value is stored in config.xml under the key the classic 2.x app uses,
    /// so both apps read the same settings.
    /// </summary>
    public sealed partial class SettingsViewModel : ObservableObject
    {
        public const string ThemeKey = "theme";
        public const string MinimizeToTrayKey = "MinimizeToTray";

        private readonly AppSetup _setup;
        private readonly ITrackingSource _source;
        private readonly Action<bool> _applyTheme;
        private readonly Action _restart;

        /// <param name="canWrite">False while the classic app owns the data (or in forced demo mode): read only.</param>
        /// <param name="applyTheme">Switches the running UI to dark (true) or light.</param>
        /// <param name="restart">Restarts the app (after a change that needs the core to start again).</param>
        public SettingsViewModel(AppSetup setup, ITrackingSource source, bool canWrite, bool isDark,
            Action<bool> applyTheme, Action restart)
        {
            _setup = setup;
            _source = source;
            CanWrite = canWrite;
            _isDark = isDark;
            _applyTheme = applyTheme;
            _restart = restart;
            bool.TryParse(setup.ReadSetting(MinimizeToTrayKey, "false"), out _minimizeToTray);
            InitTimeCaps();
        }

        /// <summary>Settings can be changed (the classic TraXile is not running, no forced demo).</summary>
        public bool CanWrite { get; }

        public bool IsReadOnly => !CanWrite;

        private ICoreMaintenance Core => _source as ICoreMaintenance;

        // ------------------------------------------------------------------
        // General
        // ------------------------------------------------------------------

        public string ClientTxtPath => _setup.ClientTxtPath;

        public string ClientTxtText => string.IsNullOrEmpty(ClientTxtPath) ? "Not selected yet" : ClientTxtPath;

        /// <summary>
        /// Uses another Client.txt: saves it, clears the parsed history and restarts, so the new log is read
        /// from the start (classic: change log file path).
        /// </summary>
        public bool ChangeClientTxt(string path, out string error)
        {
            error = null;
            if (!CanWrite) { error = "Settings are read only right now."; return false; }
            if (!AppSetup.IsValidClientTxt(path)) { error = "This file does not exist."; return false; }

            _setup.SaveClientTxtPath(path);
            OnPropertyChanged(nameof(ClientTxtPath));
            OnPropertyChanged(nameof(ClientTxtText));
            Core?.StopAndClearHistory();
            _restart();
            return true;
        }

        private bool _isDark;
        public bool IsDark
        {
            get => _isDark;
            set
            {
                if (!SetProperty(ref _isDark, value)) return;
                OnPropertyChanged(nameof(IsLight));
                _applyTheme(value);
                if (CanWrite) _setup.WriteSetting(ThemeKey, value ? "Dark" : "Light");
            }
        }

        public bool IsLight { get => !IsDark; set => IsDark = !value; }

        /// <summary>Called when the theme was switched elsewhere (title bar button).</summary>
        public void SyncTheme(bool dark)
        {
            if (SetProperty(ref _isDark, dark, nameof(IsDark))) OnPropertyChanged(nameof(IsLight));
        }

        private bool _minimizeToTray;
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (!SetProperty(ref _minimizeToTray, value)) return;
                if (CanWrite) _setup.WriteSetting(MinimizeToTrayKey, value.ToString()); // "True"/"False" like the classic app
                App.MinimizeToTray = value;
            }
        }

        /// <summary>Theme from config.xml ("Dark" or "Light", default dark like the classic app).</summary>
        public static bool ReadIsDark(AppSetup setup) =>
            !string.Equals(setup.ReadSetting(ThemeKey, "Dark"), "Light", StringComparison.OrdinalIgnoreCase);

        partial void InitTimeCaps();
    }
}
