using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using log4net;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using TraXile.UI;


namespace TraXile
{
    public enum ACTIVITY_TYPES
    {
        MAP,
        HEIST,
        LABYRINTH,
        SIMULACRUM,
        DELVE,
        TEMPLE,
        MAVEN_INVITATION,
        ATZIRI,
        UBER_ATZIRI,
        ELDER_FIGHT,
        SHAPER_FIGHT,
        MAVEN_FIGHT,
        SIRUS_FIGHT,
        OTHER,
        CAMPAIGN,
        ABYSSAL_DEPTHS,
        VAAL_SIDEAREA,
        LAB_TRIAL,
        LOGBOOK,
        SAFEHOUSE,
        CATARINA_FIGHT,
        LOGBOOK_SIDE,
        BREACHSTONE,
        SEARING_EXARCH_FIGHT,
        BLACK_STAR_FIGHT,
        INFINITE_HUNGER_FIGHT,
        EATER_OF_WORLDS_FIGHT,
        TIMELESS_LEGION,
        LAKE_OF_KALANDRA,
        SANCTUM,
        TRIALMASTER_FIGHT,
        TANES_LABORATORY,
        ANCESTOR_TRIAL,
        INSCRIBED_ULTIMATUM,
        KINGSMARCH
    }

    /// <summary>
    /// Main UI
    /// </summary>
    public partial class Main : MaterialForm
    {
        // Core logic
        private TrX_CoreLogic _logic;

        // START FLAGS
        public readonly bool IS_IN_DEBUG_MODE = false;

        // Calendar
        public DateTimeFormatInfo _dateTimeFormatInfo;

        // App parameters
        private bool _listViewInitielaized;

        // Wether or not show the grid in listviess
        private bool _showGridInActLog;

        // Startup switch if a backup should be restored
        private bool _restoreMode;

        // Wether or not show grid in statistics
        private bool _showGridInStats;

        // Indicates if update check is already done
        private bool _UpdateCheckDone;

        // Indicates if last backup restore was succesful
        private bool _restoreOk = true;

        // Reason for last backup restore failure (if any)
        private string _failedRestoreReason;

        // Number of items in activity log
        private int _actLogItemCount = 0;

        // ID of the current selected stats entry
        private string _allStatsSelected;

        // Timestamp of the oldes entry in DB
        private long _oldestTimeStamp = 0;

        // Indicates if all autostarts are done (overlay)
        private bool _autoStartsDone = false;

        // Wether or not show Unknown labs levels in Dashboard
        private bool _labDashboardHideUnknown;

        // UI Update Flag: Lab Dashboard
        private bool _uiFlagLabDashboard;

        // UI Update Flag: Map Dashboard
        private bool _uiFlagMapDashboard;
        private bool _uiFlagStatisticsChart;

        // UI Update Flag: Global Dashboard
        private bool _uiFlagGlobalDashboard;

        // UI Update Flag: Heist Dashboard
        private bool _uiFlagHeistDashboard;

        // UI Update Flag: Activity List
        private bool _uiFlagActivityList;

        // UI Update Flag: All stats Dashboard
        private bool _uiFlagAllStatsDashboard;

        // UI Update Flag: Boss Dashboard
        private bool _uiFlagBossDashboard;

        // List of lab names
        private List<string> labs;

        // Current datasource for statistics (used for filters)
        private List<TrX_TrackedActivity> _statsDataSource;

        // Timestamp of entering the current ares
        private DateTime _inAreaSince;

        // Start date for statistic filter
        private DateTime _statsDate1;

        // End date for statistic filter
        private DateTime _statsDate2;

        // Windows: Load screen
        private LoadScreen _loadScreenWindow;

        // Window: Overlay
        private OverlayTags _tagsOverlay;

        // List of available backups
        private BindingList<string> _backups;

        // Mapping of tags to labels
        private Dictionary<string, Label> _tagLabels, _tagLabelsConfig;

        // Settings manager
        private readonly TrX_SettingsManager _mySettings;

        // Listview Manager: Activity log
        private TrX_ListViewManager _lvmActlog;

        // Current theme
        private TrX_Theme _myTheme;

        // List of League Info objects
        private List<TrX_LeagueInfo> _leagues;

        // Logger
        private ILog _log;

        // Wether or not to show hideout time in pie chart
        private bool _showHideoutInPie;

        // Visibility of the stopwatch overlay windows
        private int _overlayOpacity;

        // Show stopwatch overlay by default?
        private bool _stopwatchOverlayShowDefault;
        private bool _tagOverlayShowDefault;

        // Simple Stopwatch Overlay
        private OverlaySimpleStopwatch _stopwatchOverlay;

        // Default mappings
        private TrX_DefaultMappings _defaultMappings;

        // Time cap parameters for each activity type
        private Dictionary<ACTIVITY_TYPES, int> _timeCaps;
        private bool _exitting;

        // Property: Core logic to be accessible
        public TrX_CoreLogic Logic => _logic;

        // Property: Timecaps to be accessible
        public Dictionary<ACTIVITY_TYPES, int> TimeCaps => _timeCaps;

        // Profit tracker
        private TrX_ProfitTracking _profitTracking;

        // DataBInd
        private BindingSource _profitBinding;

        // Datasource for all stats
        private Dictionary<string, int> _dataSourceAllStats;

        // Filtered source for all stats
        private Dictionary<string, int> _filteredDataSourceAllStats;

        // Is filter bar visible?
        private bool filterBarShown = true;

        // UI Flag. Reset activity list
        private bool _uiFlagActivityListReset;

        // OVerlay tags toshow
        private string _overlayTag1, _overlayTag2, _overlayTag3;
        private int _minimumTimeCap;

        // Background worker All stats Dashboard
        private BackgroundWorker _workerAllStats;

        // Background worker All stats Dashboard
        private BackgroundWorker _workerAllStatsChart;

        // Chart results
        private List<KeyValuePair<long, int>> _allStatChartresults;

        // Chart dynamic config
        private int _allStatsChartInterval = 1;
        private DateTime _allStatsChartDT1;
        private DateTime _allStatsChartDT2;
        private bool _allStatsSearchActive;

        // Setting: Mimimize to tray
        private bool _minimizeToTray;
        private bool _uiFlagLeagueDashboard;
        private bool _uiFlagTagOverlay_TagsChanged;
        private bool _updateAvailable;
        private string _newVersion;
        private int _failedUIUpdates;
        private bool _criticalUIError;


        public MaterialSkinManager msm = MaterialSkinManager.Instance;
        private bool _splitterBackFromMinimizedWinddow;

        public void DoManualThemeAdjustments(Form form)
        {
            MaterialLabel labelRef = new MaterialLabel();

            label3.BackColor = msm.ColorScheme.PrimaryColor;
            label3.ForeColor = msm.ColorScheme.TextColor;
            label3.Font = new Font(labelRef.Font.FontFamily, 9, FontStyle.Regular);

            foreach (LinkLabel cnt in TrX_Helpers.GetAll(form, typeof(LinkLabel)))
            {
                cnt.Font = labelRef.Font;
                cnt.LinkColor = msm.ColorScheme.TextColor;
            }

            foreach (TableLayoutPanel cnt in TrX_Helpers.GetAll(form, typeof(TableLayoutPanel)))
            {
                cnt.BackColor = msm.BackdropColor;
            }

            foreach (MenuStrip cnt in TrX_Helpers.GetAll(form, typeof(MenuStrip)))
            {
                cnt.BackColor = msm.BackgroundColor;
            }

            foreach (ContextMenuStrip cnt in TrX_Helpers.GetAll(form, typeof(ContextMenuStrip)))
            {
                cnt.BackColor = msm.BackgroundColor;
            }

            foreach (MaterialCard cnt in TrX_Helpers.GetAll(form, typeof(MaterialCard)))
            {
                cnt.BackColor = msm.BackgroundColor;
            }

            // Always use the full list view size for last column
            foreach (MaterialListView cnt in TrX_Helpers.GetAll(form, typeof(MaterialListView)))
            {
                TrX_Helpers.AutoResizeLastColumn((MaterialListView)cnt);
            }

            foreach (Chart cnt in TrX_Helpers.GetAll(form, typeof(Chart)))
            {
                cnt.BackColor = msm.BackgroundColor;
                cnt.BackGradientStyle = GradientStyle.None;

                foreach (Legend l in cnt.Legends)
                {

                    if (cnt.Name == "chartGlobalDashboard")
                    {
                        l.BackColor = msm.BackgroundColor;
                        l.ForeColor = msm.ColorScheme.TextColor;
                    }
                    else
                    {
                        l.Enabled = false;
                    }

                }

                foreach (ChartArea chartArea in cnt.ChartAreas)
                {
                    chartArea.BackColor = msm.BackgroundColor;
                    chartArea.AxisX.LabelStyle.ForeColor = msm.ColorScheme.TextColor;
                    chartArea.AxisY.LabelStyle.ForeColor = msm.ColorScheme.TextColor;
                    chartArea.AxisX.MajorGrid.LineColor = msm.ColorScheme.TextColor;
                    chartArea.AxisY.MajorGrid.LineColor = msm.ColorScheme.TextColor;
                    chartArea.AxisY.LineColor = Color.Black;
                    chartArea.AxisX.LineColor = Color.Black;
                    chartArea.BackColor = msm.BackgroundColor;
                    chartArea.AxisX.MajorGrid.LineWidth = 0;
                    chartArea.AxisY.MajorGrid.LineWidth = 0;
                }

                foreach (Series series in cnt.Series)
                {
                    if (cnt.Name != "chartGlobalDashboard")
                    {
                        series.Color = msm.ColorScheme.PrimaryColor;
                    }
                    series.IsValueShownAsLabel = true;
                    series.LabelForeColor = msm.ColorScheme.TextColor;
                }
            }

            checkBox1.BackColor = msm.BackdropColor;
            tableLayoutPanel7.BackColor = msm.BackdropColor;

            materialTabSelector2.BackColor = msm.BackgroundColor;

            foreach (TabPage tp in materialTabControl2.TabPages)
            {
                tp.BackColor = msm.BackdropColor;
            }

            foreach (TabPage tp in materialTabControl1.TabPages)
            {
                tp.BackColor = msm.BackdropColor;
            }

            foreach (TabPage tp in materialTabControl3.TabPages)
            {
                tp.BackColor = msm.BackdropColor;
            }
        }

        /// <summary>
        /// Main Window Constructor
        /// </summary>
        public Main()
        {
            Thread.CurrentThread.Name = "MainThread";

            // Initialize Settings
            _mySettings = new TrX_SettingsManager(TrX_Static.CONFIG_PATH);
            _mySettings.LoadFromXml();

            // Create Appdata if not existing
            if (!Directory.Exists(TrX_Static.APPDATA_PATH))
            {
                CreateOwnAppDataSubdir();
            }

            // Create Metadata path if not existing
            if (!Directory.Exists(TrX_Static.METADATA_PATH))
            {
                CreateMetaDataSubdir();
            }

            // Invisible till initialization complete
            Visible = false;

            // Fallback default theme for updater
            _myTheme = new TrX_ThemeDark();

            string theme = _mySettings.ReadSetting("theme", TrX_Static.DEFAULT_THEME_NAME);

            if (theme == "Light")
            {
                msm.Theme = MaterialSkinManager.Themes.LIGHT;
            }
            else
            {
                msm.Theme = MaterialSkinManager.Themes.DARK;
            }

            msm.AddFormToManage(this);

            msm.ColorScheme = new ColorScheme(
            Primary.BlueGrey900,    // Primary
            Primary.BlueGrey900,    // Dark Primary
            Primary.Blue50,    // Light Primary
            Accent.Red200,    // Accent
            TextShade.WHITE         // Textfarbe
            );

            InitializeComponent();

            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.MaximizedBounds = new Rectangle(workingArea.X + 2, workingArea.Y + 5, workingArea.Width, workingArea.Height - 5);


            tableLayoutPanel_L0.RowStyles[1].Height = 16;
            linkLabel5.Text = "show filters";
            filterBarShown = false;

            Init();

            DoManualThemeAdjustments(this);

        }

        private void CreateOwnAppDataSubdir()
        {
            try
            {
                Directory.CreateDirectory(TrX_Static.APPDATA_PATH);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot create appdata directory: {exception.Message}");
                Application.Exit();
            }
        }

        private void CreateMetaDataSubdir()
        {
            try
            {
                Directory.CreateDirectory(TrX_Static.METADATA_PATH);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot create metadata directory: {exception.Message}");
            }
        }

        /// <summary>
        /// Get status of workers
        /// </summary>
        /// <returns>TRUE = at least onw worker not ready</returns>
        private bool GetWorkerStatus()
        {
            return _workerAllStats.IsBusy || _workerAllStatsChart.IsBusy;
        }

        private List<TrX_TrackedActivity> FilterActivitiesByAreaLevel(int level, string op, List<TrX_TrackedActivity> source)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            foreach (TrX_TrackedActivity act in source)
            {
                bool result = false;
                switch (op)
                {
                    case "=":
                        result = act.AreaLevel == level;
                        break;
                    case ">":
                        result = act.AreaLevel > level;
                        break;
                    case ">=":
                        result = act.AreaLevel >= level;
                        break;
                    case "<":
                        result = act.AreaLevel < level;
                        break;
                    case "<=":
                        result = act.AreaLevel <= level;
                        break;
                }

                if (result)
                {
                    results.Add(act);
                }
            }

            return results;
        }

        private List<TrX_TrackedActivity> FilterActivitiesByArea(string area, List<TrX_TrackedActivity> source)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            foreach (TrX_TrackedActivity act in source)
            {
                if (act.Area.ToLower() == area.ToLower() || act.AreaOrig.ToLower() == area.ToLower())
                {
                    results.Add(act);
                }
            }
            return results;
        }

        private List<TrX_TrackedActivity> FilterActivitiesByType(ACTIVITY_TYPES type, List<TrX_TrackedActivity> source)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            foreach (TrX_TrackedActivity act in source)
            {
                if (act.Type == type)
                {
                    results.Add(act);
                }
            }
            return results;
        }

        private List<TrX_TrackedActivity> FilterActivitiesByTags(List<string> tags, string mode, List<TrX_TrackedActivity> source)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            switch (mode)
            {
                case "OR":
                    foreach (TrX_TrackedActivity act in source)
                    {
                        foreach (string tag in tags)
                        {
                            if (act.HasTag(tag))
                            {
                                results.Add(act);
                                break;
                            }
                        }
                    }
                    break;
                case "NOT":
                    foreach (TrX_TrackedActivity act in source)
                    {
                        bool filterResult = true;
                        foreach (string tag in tags)
                        {
                            if (act.HasTag(tag))
                            {
                                filterResult = false;
                                break;
                            }
                        }

                        if (filterResult)
                        {
                            results.Add(act);
                        }
                    }
                    break;
                case "AND":
                    foreach (TrX_TrackedActivity act in source)
                    {
                        bool filterResult = true;
                        foreach (string tag in tags)
                        {
                            if (!act.HasTag(tag))
                            {
                                filterResult = false;
                                break;
                            }
                        }

                        if (filterResult)
                        {
                            results.Add(act);
                        }
                    }
                    break;
            }

            return results;
        }

        /// <summary>
        /// Filter the activitylist by tie range
        /// </summary>
        /// <param name="dt1">Start date/time</param>
        /// <param name="dt2">End date/time</param>
        /// <returns>List of filtered activities</returns>
        private List<TrX_TrackedActivity> FilterActivitiesByTimeRange(DateTime dt1, DateTime dt2, List<TrX_TrackedActivity> source)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            DateTime date1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute, dt1.Second, _dateTimeFormatInfo.Calendar);
            DateTime date2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, dt2.Minute, dt2.Second, _dateTimeFormatInfo.Calendar);


            foreach (TrX_TrackedActivity act in source)
            {
                if (act.Started >= date1 && act.Started <= date2)
                {
                    if (act.TotalSeconds > _minimumTimeCap)
                    {
                        results.Add(act);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get LeagueInfo obRect by league name
        /// </summary>
        /// <param name="name">League name</param>
        /// <returns>League info object or null</returns>
        private TrX_LeagueInfo GetLeagueByName(string name)
        {
            foreach (TrX_LeagueInfo li in _leagues)
            {
                if (li.Name == name)
                    return li;
            }
            return null;
        }

        private void ResetFilter(bool set = false)
        {
            comboBox_Filter_TimeRange.SelectedItem = "All";
            comboBox_Filter_Type.SelectedItem = "All";
            comboBox_Filter_Area.SelectedItem = "All";
            comboBox_Filter_Matching.SelectedItem = "OR";
            comboBox_Filter_Area_level_Operator.SelectedItem = "=";
            textBox12.Text = "00:00:00";
            textBox13.Text = "23:59:59";
            textBox_Filter_AreaLevel.Text = "";
            listBox_Filter_Tags.Items.Clear();

            if (set)
            {
                SetFilter(true);
            }
        }

        /// <summary>
        /// Apply the current timerange filter to the UI
        /// </summary>
        /// <param name="render">update dashboards?</param>
        private void SetFilter(bool render = false)
        {
            if (comboBox_Filter_TimeRange.SelectedItem == null || _statsDataSource == null)
                return;

            if (comboBox_Filter_Type.SelectedItem == null)
            {
                comboBox_Filter_Type.SelectedItem = "All";
            }

            if (comboBox_Filter_Area.SelectedItem == null)
            {
                comboBox_Filter_Area.SelectedItem = "All";
            }

            _statsDataSource.Clear();

            if (comboBox_Filter_TimeRange.SelectedItem.ToString() == "All")
            {
                _statsDataSource.AddRange(_logic.ActivityHistory);
                _statsDate1 = DateTimeOffset.FromUnixTimeSeconds(_oldestTimeStamp).DateTime;
                _statsDate2 = DateTime.Now;
                dateTimePicker1.Value = _statsDate1;
                dateTimePicker2.Value = _statsDate2;
            }
            else if (comboBox_Filter_TimeRange.SelectedItem.ToString().Contains("League:"))
            {
                string sLeague = comboBox_Filter_TimeRange.SelectedItem.ToString().Split(new string[] { "League: " }, StringSplitOptions.None)[1]
                    .Split(new string[] { " (" }, StringSplitOptions.None)[0];
                TrX_LeagueInfo li = GetLeagueByName(sLeague);

                DateTime dt1 = li.Start;
                DateTime dt2 = li.End;

                _statsDataSource = FilterActivitiesByTimeRange(dt1, dt2, _logic.ActivityHistory);
                _statsDate1 = dt1;
                _statsDate2 = dt2;
            }
            else
            {
                DateTime date1 = DateTime.Now;
                DateTime date2 = DateTime.Now;

                switch (comboBox_Filter_TimeRange.SelectedItem.ToString())
                {
                    case "Custom":
                        string s = $"{dateTimePicker1.Value.Date} {textBox12.Text.ToString()}";

                        try
                        {
                            DateTime tmpDate1 = dateTimePicker1.Value.Date;
                            DateTime tmpDate2 = tmpDate1.Add(TimeSpan.Parse(textBox12.Text));
                            DateTime tmpDate3 = dateTimePicker2.Value.Date;
                            DateTime tmpDate4 = tmpDate3.Add(TimeSpan.Parse(textBox13.Text));

                            date1 = tmpDate2;
                            date2 = tmpDate4;
                        }
                        catch (Exception ex)
                        {
                            _log.Error($"Could not add time to date filter: {ex.Message}");
                            _log.Debug(ex.ToString());
                            date1 = dateTimePicker1.Value;
                            date2 = dateTimePicker2.Value;
                        }

                        break;
                    case "Today":
                        DateTime date = DateTime.Now;
                        date1 = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
                        date2 = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
                        break;
                    case "Last 24 hours":
                        date1 = DateTime.Now.AddHours(-24);
                        date2 = DateTime.Now;
                        break;
                    case "Last 7 days":
                        date1 = DateTime.Now.AddDays(-7);
                        date2 = DateTime.Now;
                        break;
                    case "Last 30 days":
                        date1 = DateTime.Now.AddDays(-30);
                        date2 = DateTime.Now;
                        break;
                    case "Last year":
                        date1 = DateTime.Now.AddDays(-365);
                        date2 = DateTime.Now;
                        break;
                }

                _statsDataSource = FilterActivitiesByTimeRange(date1, date2, _logic.ActivityHistory);
                _statsDate1 = date1;
                _statsDate2 = date2;

                if (comboBox_Filter_TimeRange.SelectedItem.ToString() != "Custom")
                {
                    dateTimePicker1.Value = date1;
                    dateTimePicker2.Value = date2;
                }
            }

            // Apply Type filter
            if (comboBox_Filter_Type.SelectedItem.ToString() != "All")
            {
                _statsDataSource = FilterActivitiesByType((ACTIVITY_TYPES)Enum.Parse(typeof(ACTIVITY_TYPES), comboBox_Filter_Type.SelectedItem.ToString()), _statsDataSource);
            }

            // Apply Area filter
            if (comboBox_Filter_Area.SelectedItem.ToString() != "All")
            {
                _statsDataSource = FilterActivitiesByArea(comboBox_Filter_Area.SelectedItem.ToString(), _statsDataSource);
            }

            // Apply Area filter
            if (!string.IsNullOrEmpty(textBox_Filter_AreaLevel.Text))
            {
                try
                {
                    int lvl = Convert.ToInt32(textBox_Filter_AreaLevel.Text);
                    _statsDataSource = FilterActivitiesByAreaLevel(lvl, comboBox_Filter_Area_level_Operator.SelectedItem.ToString(), _statsDataSource);
                }
                catch (Exception ex)
                {
                    textBox_Filter_AreaLevel.Text = String.Empty;
                    MessageBox.Show(ex.Message);
                }
            }

            // Apply tag filter
            if (listBox_Filter_Tags.Items.Count > 0)
            {
                List<string> src = new List<string>();
                foreach (MaterialListBoxItem s in listBox_Filter_Tags.Items)
                {
                    src.Add(s.ToString());
                }

                _statsDataSource = FilterActivitiesByTags(src, comboBox_Filter_Matching.SelectedItem.ToString(), _statsDataSource);
            }

            if (render)
            {
                RequestDashboardUpdates();
                RequestActivityListReset();
            }
        }

        /// <summary>
        /// Find an activity entry by ID
        /// </summary>
        /// <param name="id">UniqueID of the activity</param>
        /// <returns>Index of the activity</returns>
        private int FindEventLogIndexByID(string id)
        {
            foreach (TrX_TrackedActivity act in _logic.ActivityHistory)
            {
                if (act.UniqueID == id)
                {
                    return _logic.ActivityHistory.IndexOf(act);
                }
            }
            return -1;
        }

        /// <summary>
        /// Download metadata such as League specific data 
        /// </summary>
        private void DownloadMetaData()
        {
            try
            {
                string updateBranch = ReadSetting("metadata_meta_branch", TrX_Static.METADATA_DEFAULT_BRANCH);
                string updateURL = $"{TrX_Static.METADATA_BASE_URL}/{updateBranch}/{TrX_Static.METADATA_XML_FILE}";

                WebClient webClient = new WebClient();
                Uri uri = new Uri(updateURL);
                string data = webClient.DownloadString(uri);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);
                xml.Save($@"{TrX_Static.METADATA_PATH}\{TrX_Static.METADATA_XML_FILE}");

                _log.Info($"Metadata successfully updated from '{updateURL}'");

            }
            catch (Exception ex)
            {
                _log.Error($"Could not update Metadata: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a new version is available on GitHub and ask for update.
        /// </summary>
        /// <param name="b_notify_ok"></param>
        private void CheckForUpdate(bool b_notify_ok = false, bool check_only = false)
        {
            try
            {
                string updateURL, updateBranch;

                updateBranch = ReadSetting("metadata_updates_branch", TrX_Static.UPDATE_DEFAULT_BRANCH);
                updateURL = $"{TrX_Static.METADATA_BASE_URL}/{updateBranch}/{TrX_Static.UPDATES_XML_FILE}";

                _log.Info($"Update check url: {updateURL}");

                WebClient webClient = new WebClient();
                Uri uri = new Uri(updateURL);
                string releases = webClient.DownloadString(uri);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(releases);

                string sVersion;
                sVersion = xml.SelectSingleNode("/version/latest").InnerText;

                StringBuilder sbChanges = new StringBuilder();
                List<string> changes = new List<string>();

                foreach (XmlNode xn in xml.SelectNodes($"/version/changelog/chg[@version='{sVersion}']"))
                {
                    sbChanges.AppendLine(" - " + xn.InnerText);
                    changes.Add(xn.InnerText);
                }

                _log.Info($"My version: {TrX_Static.VERSION}, Remote version: {sVersion}");

                int myMajor = TrX_Static.MAJOR;
                int myMinor = TrX_Static.MINOR;
                int myBuild = TrX_Static.BUILD;

                int remoteMajor = Convert.ToInt32(sVersion.Split('.')[0]);
                int remoteMinor = Convert.ToInt32(sVersion.Split('.')[1]);
                int remoteBuild = Convert.ToInt32(sVersion.Split('.')[2]);

                bool bUpdate = false;
                if (remoteMajor > myMajor)
                {
                    bUpdate = true;
                }
                else if (remoteMajor == myMajor && remoteMinor > myMinor)
                {
                    bUpdate = true;
                }
                else if (remoteMajor == myMajor && remoteMinor == myMinor && remoteBuild > myBuild)
                {
                    bUpdate = true;
                }

                if (bUpdate)
                {
                    _log.Info("UpdateCheck -> New version available");

                    _updateAvailable = true;
                    _newVersion = sVersion;

                    if (!check_only)
                    {
                        UpdateCheckForm updateCheckForm = new UpdateCheckForm(true, TrX_Static.VERSION, sVersion, changes);
                        msm.AddFormToManage(updateCheckForm);
                        DoManualThemeAdjustments(updateCheckForm);
                        updateCheckForm.SetState();
                        DialogResult res = updateCheckForm.ShowDialog();

                        if (res == DialogResult.OK)
                        {
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                Arguments = sVersion,
                                FileName = $@"{Application.StartupPath}\TraXile.Updater.exe"
                            };
                            Process.Start(psi).WaitForExit();
                        }
                    }
                }
                else
                {
                    _log.Info("UpdateCheck -> Already up to date :)");
                    if (b_notify_ok)
                    {
                        UpdateCheckForm updateCheckForm = new UpdateCheckForm(false, TrX_Static.VERSION, sVersion, changes);
                        msm.AddFormToManage(updateCheckForm);
                        DoManualThemeAdjustments(updateCheckForm);
                        updateCheckForm.SetState();
                        updateCheckForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Could not check for Update: {ex.Message}");
            }
        }

        private void CleanupMSIFiles()
        {
            string[] files = Directory.GetFiles($@"{TrX_Static.APPDATA_PATH}", "*.msi");

            foreach (string s in files)
            {
                _log.Info($"Found installer package: {s}");

                try
                {
                    FileInfo fi = new FileInfo(s);
                    double fileAgeHours = (DateTime.Now - fi.LastWriteTime).TotalHours;
                    if (fileAgeHours > 24)
                    {
                        _log.Info($"Age of {s} is {Math.Round(fileAgeHours, 1)} hours, deleting.");
                        File.Delete(s);

                        _log.Info($"Deleted: {s}");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error cleaning up {s}: {ex.Message}");
                    _log.Debug(ex);
                }
            }
        }

        /// <summary>
        /// Do main initialization
        /// </summary>
        private void Init()
        {
            _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _log.Info("Application started");

            Opacity = 0;
            //_mySettings.LoadFromXml();

            _overlayTag1 = null;
            _overlayTag2 = null;
            _overlayTag3 = null;

            label3.Text = "TraXile " + TrX_Static.VERSION;

            ReadSettings();

            // Backup restore mode?
            try
            {
                DoBackupRestoreIfPrepared();
            }
            catch (Exception ex)
            {
                _restoreMode = true;
                _restoreOk = false;
                _failedRestoreReason = ex.Message;
                _log.Error($"Restore failed: {ex.Message}");
                _log.Debug(ex.ToString());
            }

            _allStatChartresults = new List<KeyValuePair<long, int>>();

            _workerAllStats = new BackgroundWorker();
            _workerAllStats.DoWork += _workerAllStats_DoWork;
            _workerAllStats.RunWorkerCompleted += _workerAllStats_RunWorkerCompleted;

            _workerAllStatsChart = new BackgroundWorker();
            _workerAllStatsChart.DoWork += _workerAllStatsChart_DoWork;
            _workerAllStatsChart.RunWorkerCompleted += _workerAllStatsChart_RunWorkerCompleted;

            _logic = new TrX_CoreLogic(_minimumTimeCap);

            // Fixing the DateTimeFormatInfo to Gregorian Calendar, to avoid wrong timestamps with other calendars
            _dateTimeFormatInfo = DateTimeFormatInfo.GetInstance(new CultureInfo("en-CA"));
            _dateTimeFormatInfo.Calendar = new GregorianCalendar();
            _defaultMappings = new TrX_DefaultMappings();
            _loadScreenWindow = new LoadScreen();

            SaveVersion();
            DownloadMetaData();
            CheckForUpdate(false, false);
            CleanupMSIFiles();

            pictureBoxUpdateAvailable.Visible = _updateAvailable;
            linkLabelUpdateAvailable.Visible = _updateAvailable;


            _UpdateCheckDone = true;

            _logic.OnHistoryInitialized += Logic_OnHistoryInitialized;
            _logic.OnActivityFinished += Logic_OnActivityFinished;
            _logic.OnTagsUpdated += Logic_OnTagsUpdated;
            _logic.OnActivityStarted += _logic_OnActivityStarted;
            _logic.Start();

            // Data Sources
            _dataSourceAllStats = new Dictionary<string, int>();
            _filteredDataSourceAllStats = new Dictionary<string, int>();

            // Init profit tracker
            _profitTracking = new TrX_ProfitTracking(TrX_Static.LABDATA_XML_FULLPATH);
            _profitBinding = new BindingSource();
            _profitBinding.DataSource = _profitTracking.Data;

            _lvmActlog = new TrX_ListViewManager(listViewActLog);

            _leagues = new List<TrX_LeagueInfo>();
            _stopwatchOverlay = new OverlaySimpleStopwatch() { MainWindow = this, ID = "stopwatch" };
            _tagsOverlay = new OverlayTags() { MainWindow = this, ID = "tags" };

            _logic.ClientTxtPath = _mySettings.ReadSetting("poe_logfile_path");

            if (String.IsNullOrEmpty(_logic.ClientTxtPath) || !_logic.CheckForValidClientLogFile(_logic.ClientTxtPath))
            {
                FileSelectScreen fs = new FileSelectScreen(this)
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };
                DoManualThemeAdjustments(fs);
                fs.ShowDialog();
                _logic.ClientTxtPath = _mySettings.ReadSetting("poe_logfile_path");
            }

            comboBoxShowMaxItems.SelectedItem = ReadSetting("actlog.maxitems", "500");
            comboBox_Filter_TimeRange.SelectedIndex = 0;
            listViewActLog.Columns[0].Width = 120;
            listViewActLog.Columns[1].Width = 50;
            listViewActLog.Columns[2].Width = 110;
            listViewActLog.Columns[3].Width = 100;
            listViewActLog.Columns[4].Width = 50;
            listViewMapsByArea.Columns[0].Width = 300;

            comboBox10.SelectedItem = "5";

            labelTrackingType.Text = "not tracking";

            var ca = chart1.ChartAreas["ChartArea1"].CursorX;
            ca.IsUserEnabled = true;
            ca.IsUserSelectionEnabled = true;

            textBoxLogFilePath.Text = ReadSetting("PoELogFilePath");
            textBoxLogFilePath.Enabled = false;

            _backups = new BindingList<string>();
            _inAreaSince = DateTime.Now;
            _tagLabels = new Dictionary<string, Label>();
            _tagLabelsConfig = new Dictionary<string, Label>();
            _statsDataSource = new List<TrX_TrackedActivity>();
            _statsDate1 = DateTime.Now.AddYears(-100);
            _statsDate2 = DateTime.Now.AddDays(1);

            dataGridView1.ForeColor = Color.Black;

            _timeCaps = new Dictionary<ACTIVITY_TYPES, int>();

            int cap;
            foreach (ACTIVITY_TYPES t in (((ACTIVITY_TYPES[])Enum.GetValues(typeof(ACTIVITY_TYPES)))))
            {
                cap = Convert.ToInt32(_mySettings.ReadSetting(string.Format("TimeCap{0}", TrX_Helpers.CapitalFirstLetter(t.ToString())), "3600"));
                _timeCaps.Add(t, cap);
                dataGridView1.Rows.Add(new string[] { t.ToString(), cap.ToString() });
            }

            Text = TrX_Static.NAME;
            _loadScreenWindow = new LoadScreen
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None
            };
            _loadScreenWindow.Show(this);

            InitLeagueInfo();

            // Request initial Dashboard update
            _uiFlagLabDashboard = true;
            _uiFlagMapDashboard = true;
            _uiFlagBossDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagActivityListReset = true;
            _uiFlagAllStatsDashboard = true;
            _uiFlagLeagueDashboard = true;


            // Map filter
            comboBox3.Items.Add("All");
            foreach (string s in _defaultMappings.MapAreas)
            {
                comboBox3.Items.Add(s);
            }
            foreach (string type in Enum.GetNames(typeof(ACTIVITY_TYPES)))
            {
                comboBox_Filter_Type.Items.Add(type);
            }
            comboBox3.SelectedItem = "All";
            comboBox_Filter_Matching.SelectedItem = "OR";
            comboBox6.SelectedItem = "All";
            comboBox_Filter_Type.SelectedItem = "All";
            comboBox_Filter_Area.SelectedItem = "All";

            comboBoxStopWatchTag1.Items.Add("None");
            comboBoxStopWatchTag2.Items.Add("None");
            comboBoxStopWatchTag3.Items.Add("None");

            foreach (TrX_ActivityTag tag in _logic.Tags)
            {
                comboBox_Filter_Tags.Items.Add(tag.DisplayName);
                comboBoxStopWatchTag1.Items.Add(tag.ID);
                comboBoxStopWatchTag2.Items.Add(tag.ID);
                comboBoxStopWatchTag3.Items.Add(tag.ID);
            }

            comboBoxStopWatchTag1.SelectedItem = _overlayTag1 ?? "None";
            comboBoxStopWatchTag2.SelectedItem = _overlayTag2 ?? "None";
            comboBoxStopWatchTag3.SelectedItem = _overlayTag3 ?? "None";

            foreach (string s in _defaultMappings.AllAreas)
            {
                comboBox_Filter_Area.Items.Add(s);
            }

            lbl_filter.Visible = false;
            pictureBox32.Visible = false;

            ResetFilter();

            // Start UI Thread
            timer1.Enabled = true;
            timer1.Start();
        }


        private void _workerAllStatsChart_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MethodInvoker mi = delegate
            {
                if (!string.IsNullOrEmpty(_allStatsSelected))
                {
                    chart1.ChartAreas[0].AxisX.Interval = _allStatsChartInterval;
                    chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
                    chart1.ChartAreas[0].AxisX.IntervalOffset = _allStatsChartInterval;

                    label100.Text = $"{GetStatLongName(_allStatsSelected)} ({_allStatsChartDT1} - {_allStatsChartDT2})";
                    chart1.Series[0].Points.Clear();

                    foreach (KeyValuePair<long, int> kvp in _allStatChartresults)
                    {
                        chart1.Series[0].Points.AddXY(DateTimeOffset.FromUnixTimeSeconds(kvp.Key).DateTime, kvp.Value);
                    }
                }


            };
            BeginInvoke(mi);
        }

        private void _workerAllStatsChart_DoWork(object sender, DoWorkEventArgs e)
        {
            GenerateAllStatsData();
        }

        private void _workerAllStats_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listViewNF1.SuspendLayout();
            listViewNF1.VirtualListSize = _dataSourceAllStats.Count;
            listViewNF1.ResumeLayout();
            listViewNF1.Update();
            listViewNF1.ResumeLayout();
        }

        private void _workerAllStats_DoWork(object sender, DoWorkEventArgs e)
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dateTimeFormatInfo.Calendar);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dateTimeFormatInfo.Calendar);

            if (_dataSourceAllStats.Count == 0)
            {
                for (int i = 0; i < _logic.Stats.NumericStats.Keys.Count; i++)
                {
                    string sStatKey = _logic.Stats.NumericStats.Keys.ElementAt(i);
                    string sStatLong = GetStatLongName(sStatKey);

                    _dataSourceAllStats.Add(sStatKey, Convert.ToInt32(_logic.Stats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds())));
                }
            }
            else
            {
                for (int i = 0; i < _logic.Stats.NumericStats.Keys.Count; i++)
                {
                    string sStatKey = _logic.Stats.NumericStats.Keys.ElementAt(i);
                    _dataSourceAllStats[sStatKey] = Convert.ToInt32(_logic.Stats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds()));
                }
            }
        }

        /// <summary>
        /// Event Handler
        /// </summary>
        /// <param name="e"></param>
        private void _logic_OnActivityStarted(TrX_CoreLogicActivityEventArgs e)
        {
        }

        /// <summary>
        /// Eventhandler
        /// </summary>
        /// <param name="e"></param>
        private void Logic_OnTagsUpdated(TrX_CoreLogicGenericEventArgs e)
        {
            // PLACEHOLDER
        }

        /// <summary>
        /// Eventhandler
        /// </summary>
        /// <param name="e"></param>
        private void Logic_OnActivityFinished(TrX_CoreLogicActivityEventArgs e)
        {
            if (e.Logic.EventQueueInitialized)
            {
                AddMapLvItem(e.Activity);
                RequestDashboardUpdates();
                _uiFlagActivityList = true;
            }
        }

        /// <summary>
        /// Eventhandler
        /// </summary>
        /// <param name="e"></param>
        private void Logic_OnHistoryInitialized(TrX_CoreLogicGenericEventArgs e)
        {
            MethodInvoker mi = delegate
            {
                ResetMapHistory();
            };
            BeginInvoke(mi);
        }

        /// <summary>
        /// Setup league info list
        /// </summary>
        private void InitLeagueInfo()
        {
            _leagues.Clear();


            // Update league info from metadata
            XmlDocument xml = new XmlDocument();
            xml.Load(TrX_Static.METADATA_XML_FULLPATH);

            TrX_LeagueInfo currentLeague = null;

            foreach (XmlNode node in xml.SelectSingleNode("/metadata/leagues").SelectNodes("league"))
            {
                DateTime start = DateTime.Parse(node.Attributes["start"].Value);
                DateTime end = DateTime.Parse(node.Attributes["end"].Value);

                TrX_LeagueInfo li = new TrX_LeagueInfo(
                    node.Attributes["name"].Value,
                    Convert.ToInt32(node.Attributes["major"].Value),
                    Convert.ToInt32(node.Attributes["minor"].Value),
                    new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second, _dateTimeFormatInfo.Calendar),
                    new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, end.Second, _dateTimeFormatInfo.Calendar));

                if (Convert.ToBoolean(node.Attributes["current"].Value))
                {
                    currentLeague = li;
                }

                _leagues.Add(li);
            }

            List<TrX_LeagueInfo> litmp = new List<TrX_LeagueInfo>();
            litmp.AddRange(_leagues);
            litmp.Reverse();

            foreach (TrX_LeagueInfo li in litmp)
            {
                if (currentLeague != null && li.Name == currentLeague.Name)
                {
                    comboBox_Filter_TimeRange.Items.Add($"Current League: {li.Name} ({li.Version})");
                }
                else
                {
                    comboBox_Filter_TimeRange.Items.Add($"League: {li.Name} ({li.Version})");
                }

            }
        }

        /// <summary>
        /// Save the current GUI Layout to config
        /// </summary>
        private void SaveLayout()
        {
            foreach (ColumnHeader ch in listViewActLog.Columns)
            {
                if (ch.Width > 0)
                {
                    AddUpdateAppSettings($"layout.listview.cols.{ch.Name}.width", ch.Width.ToString());
                }
            }
            if (Width > 50 && Height > 50 && WindowState != FormWindowState.Maximized && WindowState != FormWindowState.Minimized)
            {
                AddUpdateAppSettings("layout.window.width", Width.ToString());
                AddUpdateAppSettings("layout.window.height", Height.ToString());

                if (splitContainer1.SplitterDistance > 50)
                {
                    AddUpdateAppSettings("layout.tracking.splitter_distance", splitContainer1.SplitterDistance.ToString());
                }
            }



        }

        /// <summary>
        /// Load GUI layout from config
        /// </summary>
        private void LoadLayout()
        {
            this.StartPosition = FormStartPosition.CenterScreen;

            foreach (ColumnHeader ch in listViewActLog.Columns)
            {
                int w = Convert.ToInt32(ReadSetting($"layout.listview.cols." + ch.Name + ".width"));
                if (w > 0)
                {
                    ch.Width = w;
                }
            }

            int iWidth = Convert.ToInt32(ReadSetting("layout.window.width", "1470"));
            int iHeight = Convert.ToInt32(ReadSetting("layout.window.height", "980"));

            if (iWidth > 50 && iHeight > 50)
            {
                Width = iWidth;
                Height = iHeight;
            }

            int iSplitterDistance = Convert.ToInt32(ReadSetting("layout.tracking.splitter_distance", "600"));

            if (iSplitterDistance > 50)
            {
                splitContainer1.SplitterDistance = iSplitterDistance;
            }
        }


        /// <summary>
        /// Render the taglist for Config Tab
        /// </summary>
        /// <param name="b_reinit"></param>
        private void RenderTagsForConfig(bool b_reinit = false)
        {
            if (b_reinit)
            {
                groupBox3.Controls.Clear();
                _tagLabelsConfig.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 23;

            int iX = iOffsetX;
            int iY = ioffsetY;
            int iLabelWidth = 115;
            int iMaxCols = 5;

            int iCols = (groupBox3.Width - 20) / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < _logic.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _logic.Tags[i];
                Label lbl = new Label
                {
                    Width = iLabelWidth,
                    Height = 35
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 40;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if (!_tagLabelsConfig.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.Name = $"lbl_tag_{tag.ID}";
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = msm.ColorScheme.PrimaryColor;
                    lbl.ForeColor = msm.ColorScheme.TextColor;
                    lbl.Font = materialButton1.Font;
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick1;
                    lbl.Location = new Point(iX, iY);
                    lbl.AutoSize = false;
                    lbl.MinimumSize = new Size(100, 18);
                    groupBox3.Controls.Add(lbl);
                    _tagLabelsConfig.Add(tag.ID, lbl);
                }

                iX += lbl.Width + 5;
                iCurrCols++;
            }
        }

        /// <summary>
        ///  Tag label moouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lbl_MouseClick1(object sender, MouseEventArgs e)
        {
            TrX_ActivityTag tag = GetTagByDisplayName(((Label)sender).Text);
            textBox4.Text = tag.ID;
            textBox5.Text = tag.DisplayName;
            checkBox4.Checked = tag.ShowInListView;
        }

        /// <summary>
        /// Tag label mouse leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tagLabel_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.None;
        }

        /// <summary>
        /// Tag Label mouse over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tagLabel_MouseOver(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
        }

        /// <summary>
        /// RenderTags in tracking tab
        /// </summary>
        /// <param name="b_reinit"></param>
        private void RenderTagsForTracking(bool b_reinit = false)
        {
            List<string> hideTags = new List<string>();
            hideTags.Add("zana");
            hideTags.Add("zana-map");

            if (b_reinit)
            {
                groupBoxTrackingTags.Controls.Clear();
                groupBoxTrackingTags.Controls.Add(materialLabel2);
                _tagLabels.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 55;
            int iLabelWidth = 120;
            int iMaxCols = 5;

            iMaxCols = (groupBoxTrackingTags.Width - 40) / iLabelWidth;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = groupBoxTrackingTags.Width / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < _logic.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _logic.Tags[i];
                
                if(hideTags.Contains(tag.ID))
                {
                    continue;
                }
                
                Label lbl = new Label
                {
                    Width = iLabelWidth,
                    Height = 35
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 40;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if (!_tagLabels.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.Name = $"lbl_tag_{tag.ID}";
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = Color.Gray;
                    lbl.ForeColor = Color.LightGray;
                    lbl.Location = new Point(iX, iY);
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick;
                    lbl.MinimumSize = new Size(100, 18);
                    lbl.Font = materialButton1.Font;

                    groupBoxTrackingTags.Controls.Add(lbl);
                    _tagLabels.Add(tag.ID, lbl);
                }
                else
                {
                    if (_logic.CurrentActivity != null)
                    {
                        TrX_TrackedActivity mapToCheck = _logic.IsMapZana ? _logic.CurrentActivity.SideArea_ZanaMap : _logic.CurrentActivity;

                        if (mapToCheck.Tags.Contains(tag.ID))
                        {
                            _tagLabels[tag.ID].BackColor = msm.ColorScheme.PrimaryColor;
                            _tagLabels[tag.ID].ForeColor = msm.ColorScheme.TextColor;
                        }
                        else
                        {
                            _tagLabels[tag.ID].BackColor = Color.Gray;
                            _tagLabels[tag.ID].ForeColor = Color.LightGray;
                        }
                    }
                    else
                    {
                        _tagLabels[tag.ID].BackColor = Color.Gray;
                        _tagLabels[tag.ID].ForeColor = Color.LightGray;
                    }
                }

                iX += lbl.Width + 5;
                iCurrCols++;
            }
        }

        /// <summary>
        /// RenderTags in tracking tab
        /// </summary>
        /// <param name="b_reinit"></param>
        private void RenderTagsForSummary(UI.SummaryForm targetSummaryWindow, Dictionary<string, int> dict, int total_count, bool b_reinit = false)
        {
            Control targetControl = targetSummaryWindow.TagCard;

            if (b_reinit)
            {
                targetControl.Controls.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 45;
            int iLabelWidth = 150;
            int iMaxCols = 5;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = (targetControl.Width -40) / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            foreach (KeyValuePair<string, int> kvp in dict)
            {
                TrX_ActivityTag tag = _logic.GetTagByID(kvp.Key);
                Label lbl = new Label
                {
                    Width = iLabelWidth,
                    Height = 35
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 40;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }


                double rate = ((double)kvp.Value / total_count);
                double percent = rate * 100;
                percent = Math.Round(percent, 1);

                lbl.Text = $"{tag.DisplayName}: {kvp.Value} ({percent}%)";
                lbl.Name = $"lbl_tag_{tag.ID}";
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.BackColor = msm.ColorScheme.PrimaryColor;
                lbl.ForeColor = msm.ColorScheme.TextColor;
                lbl.Location = new Point(iX, iY);
                lbl.MinimumSize = new Size(100, 18);
                lbl.Font = materialButton1.Font;
                lbl.Font = new Font(materialButton1.Font.FontFamily, 8, FontStyle.Regular);
                targetControl.Controls.Add(lbl);

                iX += lbl.Width + 5;
                iCurrCols++;
            }
        }

        /// <summary>
        /// Find matching tag for given display name
        /// </summary>
        /// <param name="s_display_name"></param>
        /// <returns></returns>
        public TrX_ActivityTag GetTagByDisplayName(string s_display_name)
        {
            foreach (TrX_ActivityTag t in _logic.Tags)
            {
                if (t.DisplayName == s_display_name)
                    return t;
            }

            return null;
        }

        /// <summary>
        /// Mouse click handler for label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lbl_MouseClick(object sender, MouseEventArgs e)
        {
            TrX_ActivityTag tag = GetTagByDisplayName(((Label)sender).Text);
            if (!tag.IsDefault)
            {
                if (_logic.CurrentActivity != null)
                {
                    if (_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null)
                    {
                        if (_logic.CurrentActivity.SideArea_ZanaMap.HasTag(tag.ID))
                        {
                            _logic.CurrentActivity.SideArea_ZanaMap.RemoveTag(tag.ID);
                        }
                        else
                        {
                            _logic.CurrentActivity.SideArea_ZanaMap.AddTag(tag.ID);
                        }
                    }
                    else
                    {
                        if (_logic.CurrentActivity.HasTag(tag.ID))
                        {
                            _logic.CurrentActivity.RemoveTag(tag.ID);
                        }
                        else
                        {
                            _logic.CurrentActivity.AddTag(tag.ID);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Reload the Poe Logfile
        /// </summary>
        public void ReloadLogFile()
        {
            ResetStats();
            Application.Restart();
        }

        /// <summary>
        /// Get longname for a stat
        /// </summary>
        /// <param name="s_key"></param>
        /// <returns></returns>
        private string GetStatLongName(string s_key)
        {
            if (_logic.StatNamesLong.ContainsKey(s_key))
            {
                return _logic.StatNamesLong[s_key];
            }
            else
            {
                return s_key;
            }
        }

        /// <summary>
        /// Reset stats
        /// </summary>
        public void ResetStats()
        {
            ClearStatsDB();
        }

        /// <summary>
        /// Empty stats DB
        /// </summary>
        private void ClearStatsDB()
        {
            _logic.Database.DoNonQuery("drop table tx_stats");
            _logic.Database.DoNonQuery("create table if not exists tx_stats " +
                "(timestamp int, " +
                "stat_name text, " +
                "stat_value int)");
            _log.Info("Stats cleared.");
        }

        /// <summary>
        /// Drop data and initiate the reload
        /// </summary>
        private void InitLogFileReload()
        {
            // Stop UI updates
            timer1.Stop();

            // Stop core logic
            _logic.Stop();

            // DROP Databases
            try
            {
                _logic.Database.DoNonQuery("DELETE FROM tx_activity_log");
                _logic.Database.DoNonQuery("DELETE FROM tx_stats");
                _logic.Database.DoNonQuery("UPDATE tx_kvstore SET value = '0' WHERE key = 'last_hash'");
                _logic.Database.Close();
            }
            catch (Exception ex)
            {
                _log.Error("cannot drop databases: " + ex.Message);
                _log.Debug(ex.ToString());
            }

            // Restart
            Application.Restart();
        }

        /// <summary>
        /// Read existing backups to list in GUI
        /// </summary>
        private void ReadBackupList()
        {
            if (Directory.Exists(TrX_Static.APPDATA_PATH + @"\backups"))
            {
                foreach (string s in Directory.GetDirectories(TrX_Static.APPDATA_PATH + @"\backups"))
                {
                    foreach (string s2 in Directory.GetDirectories(s))
                    {
                        string s_name = s2.Replace(TrX_Static.APPDATA_PATH, "");

                        if (!_backups.Contains(s_name))
                            _backups.Add(s_name);
                    }
                }
            }
        }

        /// <summary>
        /// Delete entry from activity log
        /// </summary>
        /// <param name="timestamp"></param>
        private void DeleteActLogEntry(long timestamp)
        {
            _logic.Database.DoNonQuery($"delete from tx_activity_log where timestamp = {timestamp}");
        }

        /// <summary>
        /// Simply save the current app version to VERSION.txt
        /// </summary>
        private void SaveVersion()
        {
            StreamWriter streamWriter = new StreamWriter($@"{TrX_Static.APPDATA_PATH}\VERSION.txt");
            streamWriter.WriteLine(TrX_Static.VERSION);
            streamWriter.Close();
        }

        /// <summary>
        /// Reset and reload the Activity-History ListView
        /// </summary>
        public void ResetMapHistory()
        {
            _lvmActlog.ClearLvItems();
            _lvmActlog.Columns.Clear();

            ColumnHeader
                chTime = new ColumnHeader() { Name = "actlog_time", Text = "Time", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_time.width", "150")) },
                chType = new ColumnHeader() { Name = "actlog_type", Text = "Type", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_type.width", "100")) },
                chArea = new ColumnHeader() { Name = "actlog_area", Text = "Area", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_area.width", "205")) },
                chLvl = new ColumnHeader() { Name = "actlog_lvl", Text = "Level/Tier", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_lvl.width", "60")) },
                chStopwatch = new ColumnHeader() { Name = "actlog_stopwatch", Text = "Stopwatch", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_stopwatch.width", "100")) },
                chDeath = new ColumnHeader() { Name = "actlog_death", Text = "Deaths", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_death.width", "60")) };

            _lvmActlog.Columns.Add(chTime);
            _lvmActlog.Columns.Add(chType);
            _lvmActlog.Columns.Add(chArea);
            _lvmActlog.Columns.Add(chLvl);
            _lvmActlog.Columns.Add(chStopwatch);
            _lvmActlog.Columns.Add(chDeath);

            foreach (TrX_ActivityTag tag in _logic.Tags)
            {
                if (tag.ShowInListView)
                {
                    ColumnHeader ch = new ColumnHeader()
                    {
                        Name = "actlog_tag_" + tag.ID,
                        Text = tag.DisplayName,
                        Width = Convert.ToInt32(ReadSetting($"layout.listview.cols.actlog_tag_{tag.ID}.width", "60"))
                    };
                    _lvmActlog.Columns.Add(ch);
                }
            }

            AddActivityLvItems();
        }

        /// <summary>
        /// Add maximum numberof ListViewItems to Listview
        /// TODO: filter before adding!!!
        /// </summary>
        private void AddActivityLvItems()
        {
            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                AddMapLvItem(act, act.IsZana, -1, false);
            }
        }

        /// <summary>
        /// Get matching image indax for an activity
        /// </summary>
        /// <param name="activity">Activity</param>
        /// <returns>numeric index in image list to use</returns>
        public int GetImageIndex(TrX_TrackedActivity activity)
        {
            int imageIndex = 0;
            // Calculate Image Index
            if (activity.Type == ACTIVITY_TYPES.MAP)
            {
                if (activity.MapTier > 0 && activity.MapTier <= 5)
                {
                    imageIndex = 0;
                }
                else if (activity.MapTier >= 6 && activity.MapTier <= 10)
                {
                    imageIndex = 1;
                }
                else if (activity.MapTier >= 11 && activity.MapTier <= 16)
                {
                    imageIndex = 2;
                }
                else if (activity.MapTier >= 17)
                {
                    imageIndex = 57;
                }
            }
            else if (activity.Type == ACTIVITY_TYPES.TEMPLE)
            {
                imageIndex = 3;
            }
            else if (activity.Type == ACTIVITY_TYPES.HEIST)
            {
                imageIndex = 4;
            }
            else if (activity.Type == ACTIVITY_TYPES.ABYSSAL_DEPTHS)
            {
                imageIndex = 5;
            }
            else if (activity.Type == ACTIVITY_TYPES.LABYRINTH || activity.Type == ACTIVITY_TYPES.LAB_TRIAL)
            {
                imageIndex = 6;
            }
            else if (activity.Type == ACTIVITY_TYPES.CAMPAIGN)
            {
                imageIndex = 7;
            }
            else if (activity.Type == ACTIVITY_TYPES.LOGBOOK || activity.Type == ACTIVITY_TYPES.LOGBOOK_SIDE)
            {
                imageIndex = 8;
            }
            else if (activity.Type == ACTIVITY_TYPES.VAAL_SIDEAREA)
            {
                imageIndex = 9;
            }
            else if (activity.Type == ACTIVITY_TYPES.CATARINA_FIGHT)
            {
                imageIndex = 10;
            }
            else if (activity.Type == ACTIVITY_TYPES.SAFEHOUSE)
            {
                imageIndex = 11;
            }
            else if (activity.Type == ACTIVITY_TYPES.DELVE)
            {
                imageIndex = 12;
            }
            else if (activity.Type == ACTIVITY_TYPES.MAVEN_INVITATION)
            {
                imageIndex = 13;
            }
            else if (activity.Type == ACTIVITY_TYPES.SIRUS_FIGHT)
            {
                imageIndex = 14;
            }
            else if (activity.Type == ACTIVITY_TYPES.ATZIRI)
            {
                imageIndex = 15;
            }
            else if (activity.Type == ACTIVITY_TYPES.UBER_ATZIRI)
            {
                imageIndex = 16;
            }
            else if (activity.Type == ACTIVITY_TYPES.ELDER_FIGHT)
            {
                imageIndex = 17;
            }
            else if (activity.Type == ACTIVITY_TYPES.SHAPER_FIGHT)
            {
                imageIndex = 18;
            }
            else if (activity.Type == ACTIVITY_TYPES.SIMULACRUM)
            {
                imageIndex = 19;
            }
            else if (activity.Type == ACTIVITY_TYPES.MAVEN_FIGHT)
            {
                imageIndex = 20;
            }
            else if (activity.Type == ACTIVITY_TYPES.BREACHSTONE)
            {
                if (activity.Area.Contains("Chayula"))
                {
                    switch (activity.AreaLevel)
                    {
                        // Normal
                        case 80:
                            imageIndex = 21;
                            break;
                        // Charged
                        case 81:
                            imageIndex = 41;
                            break;
                        // Enriched
                        case 82:
                            imageIndex = 40;
                            break;
                        // Pure
                        case 83:
                            imageIndex = 39;
                            break;
                        // Flawless
                        case 84:
                            imageIndex = 38;
                            break;
                    }
                }
                else if (activity.Area.Contains("Esh"))
                {
                    switch (activity.AreaLevel)
                    {
                        // Normal
                        case 70:
                            imageIndex = 22;
                            break;
                        // Charged
                        case 74:
                            imageIndex = 45;
                            break;
                        // Enriched
                        case 79:
                            imageIndex = 44;
                            break;
                        // Pure
                        case 81:
                            imageIndex = 43;
                            break;
                        // Flawless
                        case 84:
                            imageIndex = 42;
                            break;
                    }
                }
                else if (activity.Area.Contains("Xoph"))
                {
                    switch (activity.AreaLevel)
                    {
                        // Normal
                        case 70:
                            imageIndex = 23;
                            break;
                        // Charged
                        case 74:
                            imageIndex = 37;
                            break;
                        // Enriched
                        case 79:
                            imageIndex = 36;
                            break;
                        // Pure
                        case 81:
                            imageIndex = 35;
                            break;
                        // Flawless
                        case 84:
                            imageIndex =
                                34;
                            break;
                    }
                }
                else if (activity.Area.Contains("Uul-Netol"))
                {
                    switch (activity.AreaLevel)
                    {
                        // Normal
                        case 75:
                            imageIndex = 24;
                            break;
                        // Charged
                        case 78:
                            imageIndex = 33;
                            break;
                        // Enriched
                        case 81:
                            imageIndex = 32;
                            break;
                        // Pure
                        case 82:
                            imageIndex = 31;
                            break;
                        // Flawless
                        case 84:
                            imageIndex = 30;
                            break;
                    }
                }
                else if (activity.Area.Contains("Tul"))
                {
                    switch (activity.AreaLevel)
                    {
                        // Normal
                        case 70:
                            imageIndex = 25;
                            break;
                        // Charged
                        case 74:
                            imageIndex = 29;
                            break;
                        // Enriched
                        case 79:
                            imageIndex = 28;
                            break;
                        // Pure
                        case 81:
                            imageIndex = 27;
                            break;
                        // Flawless
                        case 84:
                            imageIndex = 26;
                            break;
                    }
                }
            }
            else if (activity.Type == ACTIVITY_TYPES.SEARING_EXARCH_FIGHT)
            {
                imageIndex = 46;
            }
            else if (activity.Type == ACTIVITY_TYPES.BLACK_STAR_FIGHT)
            {
                imageIndex = 47;
            }
            else if (activity.Type == ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT)
            {
                imageIndex = 48;
            }
            else if (activity.Type == ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT)
            {
                imageIndex = 49;
            }
            else if (activity.Type == ACTIVITY_TYPES.TIMELESS_LEGION)
            {
                imageIndex = 50;
            }
            else if (activity.Type == ACTIVITY_TYPES.LAKE_OF_KALANDRA)
            {
                imageIndex = 51;
            }
            else if (activity.Type == ACTIVITY_TYPES.SANCTUM)
            {
                imageIndex = 52;
            }
            else if (activity.Type == ACTIVITY_TYPES.TRIALMASTER_FIGHT)
            {
                imageIndex = 53;
            }
            else if (activity.Type == ACTIVITY_TYPES.TANES_LABORATORY)
            {
                imageIndex = 54;
            }
            else if (activity.Type == ACTIVITY_TYPES.ANCESTOR_TRIAL)
            {
                imageIndex = 55;
            }
            else if (activity.Type == ACTIVITY_TYPES.INSCRIBED_ULTIMATUM)
            {
                imageIndex = 56;
            }
            else if (activity.Type == ACTIVITY_TYPES.KINGSMARCH)
            {
                imageIndex = 58;
            }
            return imageIndex;
        }

        /// <summary>
        /// Add listview Item for single activity
        /// </summary>
        /// <param name="map"></param>
        /// <param name="bZana"></param>
        /// <param name="iPos"></param>
        /// <param name="b_display"></param>
        private void AddMapLvItem(TrX_TrackedActivity map, bool bZana = false, int iPos = 0, bool b_display = true)
        {
            Invoke((MethodInvoker)delegate
            {
                ListViewItem lvi = new ListViewItem($"  {map.Started}");
                string sName = map.Area;
                string sTier = "";

                if (map.AreaLevel == 0)
                {
                    sTier = "-";
                }
                else if (map.Type == ACTIVITY_TYPES.MAP)
                {
                    sTier = $"T{map.MapTier}";
                }
                else
                {
                    sTier = map.AreaLevel.ToString();
                }

                bool isZana = map.Tags.Contains("zana-map");

                lvi.SubItems.Add(TrX_Helpers.CapitalFirstLetter(map.Type.ToString()));
                lvi.SubItems.Add(map.Area);
                lvi.SubItems.Add(sTier);
                lvi.SubItems.Add(isZana ? map.CustomStopWatchValue : map.GetCappedStopwatchValue(_timeCaps[map.Type]));
                lvi.SubItems.Add(map.DeathCounter.ToString());

                // Calculate Image Index
                lvi.ImageIndex = GetImageIndex(map);

                foreach (TrX_ActivityTag t in _logic.Tags)
                {
                    if (t.ShowInListView)
                    {
                        lvi.SubItems.Add(map.Tags.Contains(t.ID) ? "X" : "");
                    }
                }

                if (iPos == -1)
                {
                    _lvmActlog.AddLvItem(lvi, map.UniqueID, b_display);
                }
                else
                {
                    _lvmActlog.InsertLvItem(lvi, map.UniqueID, iPos, b_display);
                }
                _actLogItemCount = _lvmActlog.CurrentItemCount;

                foreach (TrX_TrackedActivity act in map.GetSubActivities())
                {
                    AddMapLvItem(act);
                }
            });
        }

        /// <summary>
        /// Find matching activity to Item name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private TrX_TrackedActivity GetActivityFromListItemName(string name)
        {
            foreach (TrX_TrackedActivity activity in _logic.ActivityHistory)
            {
                if (activity.UniqueID == name)
                    return activity;
            }
            return null;
        }

        /// <summary>
        /// Convert an activity type to string
        /// </summary>
        /// <param name="a_type"></param>
        /// <returns></returns>
        private string GetStringFromActType(ACTIVITY_TYPES a_type)
        {
            return a_type.ToString().ToLower();
        }

        private void SetUILoading()
        {
            btt_remove_filter.Enabled = false;
            btt_apply_filter.Enabled = false;
            btt_apply_filter.BackColor = Color.Gray;
            btt_remove_filter.BackColor = Color.Gray;
            progressBar1.Show();

            lbl_background_update.Show();
            pictureBox33.Show();
        }

        private void SetUIReady()
        {
            btt_remove_filter.Enabled = true;
            btt_apply_filter.Enabled = true;
            btt_apply_filter.BackColor = Color.LightGreen;
            listViewNF1.Enabled = true;
            progressBar1.Hide();
            lbl_background_update.Hide();
            pictureBox33.Hide();
        }

        /// <summary>
        /// Handle the GUI updates
        /// </summary>
        private void UpdateUI()
        {
            bool workerStatus = GetWorkerStatus();

            if (workerStatus)
            {
                SetUILoading();
            }
            else
            {
                SetUIReady();
            }

            pictureBoxUpdateAvailable.Visible = _updateAvailable;
            linkLabelUpdateAvailable.Visible = _updateAvailable;
            linkLabelUpdateAvailable.Text = $"TraXile {_newVersion} available. Update now!";

            materialLabelSummary.Text = $"summary ({listViewActLog.SelectedIndices.Count})";
            materialLabelDelete.Text = $"delete ({listViewActLog.SelectedIndices.Count})";
            TimeSpan tsAreaTime = (DateTime.Now - _inAreaSince);
            checkBoxShowGridInAct.Checked = _showGridInActLog;
            checkBoxShowGridInStats.Checked = _showGridInStats;
            ReadBackupList();
            listBoxRestoreBackup.DataSource = _backups;

            if (_logic.EventQueueInitialized)
            {
                stopwatchsimpleToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
                tagOverlayToolStripMenuItem.Checked = _tagsOverlay.Visible;
                _loadScreenWindow.Close();
                MethodInvoker mi = delegate
                {
                    Show();

                    if (_restoreMode)
                    {
                        _restoreMode = false;
                        if (_restoreOk)
                        {
                            MessageBox.Show("Successfully restored from Backup!");
                        }
                        else
                        {
                            MessageBox.Show("Error restoring from backup: " +
                                Environment.NewLine +
                                Environment.NewLine +
                                _failedRestoreReason +
                                Environment.NewLine);
                        }
                    }

                    if (!_autoStartsDone)
                    {
                        if (_stopwatchOverlayShowDefault)
                        {
                            ActivateSimpleStopWatchOverlay();
                        }

                        if (_tagOverlayShowDefault)
                        {
                            ActivateTagOverlay();
                        }

                        _autoStartsDone = true;
                    }

                    RenderTagsForTracking();
                    RenderTagsForConfig();
                    textBoxLogFilePath.Text = ReadSetting("poe_logfile_path");
                    labelItemCount.Text = $"items: {_actLogItemCount}";

                    if (!_listViewInitielaized)
                    {
                        _oldestTimeStamp = _logic.Stats.GetOldestTimeStamp();
                        _listViewInitielaized = true;
                    }

                    labelAreaCurrent.Text = $"{_logic.CurrentArea}";
                    if(_logic.CurrentArea != "None")
                        labelCurrentArea.Text += $" (lvl. {_logic.CurrentAreaLevel})";

                    if (_logic.CurrentActivity != null)
                    {
                        string sTier = "";

                        if (_logic.CurrentActivity.Type == ACTIVITY_TYPES.SIMULACRUM)
                        {
                            _logic.CurrentActivity.AreaLevel = 75;
                        }

                        if (_logic.CurrentActivity.AreaLevel > 0)
                        {
                            if (_logic.CurrentActivity.Type == ACTIVITY_TYPES.MAP)
                            {
                                sTier = $"T{_logic.CurrentActivity.MapTier}";
                            }
                            else
                            {
                                sTier = $"Lvl. {_logic.CurrentActivity.AreaLevel}";
                            }
                        }

                        if (_logic.CurrentActivity != null && _logic.CurrentActivity.Type == ACTIVITY_TYPES.BREACHSTONE)
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.GetBreachStoneName(_logic.CurrentActivity.Area, _logic.CurrentActivity.AreaLevel);
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                        }
                        else if ((_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_ZanaMap.Area + " (" + sTier + ", Zana)";
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.SideArea_ZanaMap.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapVaalArea && _logic.CurrentActivity.SideArea_VaalArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_VaalArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_VaalArea.Area;
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.SideArea_VaalArea.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_VaalArea.Type));
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapAbyssArea && _logic.CurrentActivity.SideArea_AbyssArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_AbyssArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_AbyssArea.Area;
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.SideArea_AbyssArea.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_AbyssArea.Type));
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapLabTrial && _logic.CurrentActivity.SideArea_LabTrial != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_LabTrial.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_LabTrial.Area;
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.SideArea_LabTrial.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_LabTrial.Type));
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapSanctum && _logic.CurrentActivity.SideArea_Sanctum != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_Sanctum.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_Sanctum.Area;
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.SideArea_Sanctum.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_Sanctum.Type));
                            pictureBoxStop.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                            materialLabell_DeathCounter.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            pictureBoxStop.Show();
                        }
                    }
                    else
                    {
                        materialLabell_DeathCounter.Text = "0";
                        labelTrackingArea.Text = "not tracking";
                        labelStopWatch.Text = "00:00:00";
                        labelTrackingType.Text = "-";
                    }

                    if (_uiFlagAllStatsDashboard ||
                    _uiFlagBossDashboard ||
                    _uiFlagGlobalDashboard ||
                    _uiFlagHeistDashboard ||
                    _uiFlagLabDashboard ||
                    _uiFlagMapDashboard ||
                    _uiFlagActivityList
                    )
                    {
                        SetFilter();
                    }

                    DateTime dtRenderStart = DateTime.Now;

                    // League Stats
                    if (_uiFlagLeagueDashboard)
                    {
                        RenderLeagueDashboard();
                        _uiFlagLeagueDashboard = false;
                    }

                    // MAP Dashbaord
                    if (_uiFlagMapDashboard)
                    {
                        RenderMappingDashboard();
                        _uiFlagMapDashboard = false;
                    }

                    // LAB Dashbaord
                    if (_uiFlagLabDashboard)
                    {
                        RenderLabDashboard();
                        _uiFlagLabDashboard = false;
                    }

                    // HEIST Dashbaord
                    if (_uiFlagHeistDashboard)
                    {
                        RenderHeistDashboard();
                        _uiFlagHeistDashboard = false;
                    }

                    // AllStats Dashbaord
                    if (_uiFlagAllStatsDashboard)
                    {
                        RenderAllStatsDashboard();
                        _uiFlagAllStatsDashboard = false;
                    }

                    // Global Dashbaord
                    if (_uiFlagGlobalDashboard)
                    {
                        RenderGlobalDashboard();
                        _uiFlagGlobalDashboard = false;
                        if (checkBoxLabHideUnknown.Checked != _labDashboardHideUnknown)
                        {
                            checkBoxLabHideUnknown.Checked = _labDashboardHideUnknown;
                        }

                        if (checkBox1.Checked != _showHideoutInPie)
                        {
                            checkBox1.Checked = _showHideoutInPie;
                        }
                    }

                    // All stats chart
                    if (_uiFlagStatisticsChart)
                    {
                        UpdateAllStatsChart();
                        _uiFlagStatisticsChart = false;
                    }

                    // Rest act list
                    if (_uiFlagActivityListReset)
                    {
                        ResetMapHistory();
                        DoSearch();
                        _uiFlagActivityListReset = false;
                        _uiFlagActivityList = false;

                    }
                    else if (_uiFlagActivityList)
                    {
                        DoSearch();
                        _uiFlagActivityList = false;
                    }

                    listView1.Columns[2].Width = listView1.Width;

                };
                Invoke(mi);
            }

            UpdateOverlays();
        }

        private void UpdateOverlays()
        {
            UpdateStopwatchOverlay();
            UpdateTagsOverlay();
        }

        private void UpdateStopwatchOverlay()
        {
            _stopwatchOverlay.SetText(labelStopWatch.Text);
        }

        private void UpdateTagsOverlay()
        {
            if (_uiFlagTagOverlay_TagsChanged)
            {
                _tagsOverlay.Tag1 = GetTagByDisplayName(_overlayTag1);
                _tagsOverlay.Tag2 = GetTagByDisplayName(_overlayTag2);
                _tagsOverlay.Tag3 = GetTagByDisplayName(_overlayTag3);

                _uiFlagTagOverlay_TagsChanged = false;
            }

            _tagsOverlay.CurrentActivity = _logic.CurrentActivity;
            _tagsOverlay.UpdateOverlay();
        }

        /// <summary>
        /// Read all settings
        /// </summary>
        private void ReadSettings()
        {
            _showGridInActLog = Convert.ToBoolean(ReadSetting("ActivityLogShowGrid"));
            _showGridInStats = Convert.ToBoolean(ReadSetting("StatsShowGrid"));
            _minimizeToTray = Convert.ToBoolean(ReadSetting("MinimizeToTray"));
            _labDashboardHideUnknown = Convert.ToBoolean(ReadSetting("dashboard_lab_hide_unknown", "false"));
            _showHideoutInPie = Convert.ToBoolean(ReadSetting("pie_chart_show_hideout", "true"));
            _overlayOpacity = Convert.ToInt32(ReadSetting("overlay.general.opacity", "100"));
            _stopwatchOverlayShowDefault = Convert.ToBoolean(ReadSetting("overlay.stopwatch.showDefault", "false"));
            _tagOverlayShowDefault = Convert.ToBoolean(ReadSetting("overlay.tags.showDefault", "false"));
            comboBoxTheme.SelectedItem = ReadSetting("theme", "Dark") == "Dark" ? "Dark" : "Light";
            listViewActLog.GridLines = _showGridInActLog;
            trackBar1.Value = _overlayOpacity;
            checkBox5.Checked = _stopwatchOverlayShowDefault;
            checkBox3.Checked = _tagOverlayShowDefault;
            checkBoxMinimizeToTray.Checked = _minimizeToTray;
            label38.Text = _overlayOpacity.ToString() + "%";
            _overlayTag1 = ReadSetting("overlay.tags.tag1", "blight");
            _overlayTag2 = ReadSetting("overlay.tags.tag2", "expedition");
            _overlayTag3 = ReadSetting("overlay.tags.tag3", null);
            _uiFlagTagOverlay_TagsChanged = true;
            _minimumTimeCap = Convert.ToInt32(ReadSetting("TimeCapMinimum", "10"));
            textBox11.Text = _minimumTimeCap.ToString();
        }

        /// <summary>
        /// Read single setting
        /// </summary>
        /// <param name="key"></param>
        /// <param name="s_default"></param>
        /// <returns></returns>
        public string ReadSetting(string key, string s_default = null)
        {
            return _mySettings.ReadSetting(key, s_default);
        }

        /// <summary>
        /// Exit
        /// </summary>
        private void Exit()
        {
            if (!_exitting)
            {
                if (_logic.CurrentActivity != null)
                    _logic.FinishActivity(_logic.CurrentActivity, null, _logic.CurrentActivity.Type, DateTime.Now);
                _exitting = true;
                _log.Info("Exitting.");
                Application.Exit();
            }
        }

        /// <summary>
        /// Render Lab Dashboard
        /// </summary>
        public void RenderLabDashboard()
        {
            Dictionary<string, int> labCounts;
            Dictionary<string, double> labAvgTimes;
            Dictionary<string, TrX_TrackedActivity> labBestTimes;

            labCounts = new Dictionary<string, int>();
            labAvgTimes = new Dictionary<string, double>();
            labBestTimes = new Dictionary<string, TrX_TrackedActivity>();

            labs = new List<string>
            {
                "Unknown",
                "The Labyrinth",
                "The Merciless Labyrinth",
                "The Cruel Labyrinth",
                "Uber-Lab",
                "Advanced Uber-Lab"
            };

            foreach (string s in labs)
            {
                if (_labDashboardHideUnknown && s == "Unknown")
                    continue;

                labCounts.Add(s, 0);
                labAvgTimes.Add(s, 0);
                labBestTimes.Add(s, null);
            }

            // Lab counts
            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.LABYRINTH && act.DeathCounter == 0)
                {
                    if (labCounts.ContainsKey(act.Area))
                    {
                        labCounts[act.Area]++;
                    }
                }
            }

            // Avg lab times
            foreach (string s in labs)
            {
                if (!labs.Contains(s))
                    continue;

                if (_labDashboardHideUnknown && s == "Unknown")
                    continue;

                int iSum = 0;
                int iCount = 0;

                foreach (TrX_TrackedActivity act in _statsDataSource)
                {
                    if (act.Type == ACTIVITY_TYPES.LABYRINTH && act.DeathCounter == 0)
                    {
                        if (act.Area == s)
                        {
                            // Average
                            iCount++;

                            if (act.TotalSeconds < _timeCaps[ACTIVITY_TYPES.LABYRINTH])
                            {
                                iSum += act.TotalSeconds;
                            }
                            else
                            {
                                iSum += _timeCaps[ACTIVITY_TYPES.LABYRINTH];
                            }

                            // Top 
                            if (labBestTimes[s] == null)
                            {
                                labBestTimes[s] = act;
                            }
                            else
                            {
                                if (labBestTimes[s].TotalSeconds > act.TotalSeconds)
                                {
                                    labBestTimes[s] = act;
                                }
                            }
                        }
                    }
                }

                if (iSum > 0 && iCount > 0)
                {
                    labAvgTimes[s] = iSum / iCount;
                }
            }

            // UPdate Lab chart
            MethodInvoker mi = delegate
            {
                chartLabsDone.Series[0].Points.Clear();
                chartLabsAvgTime.Series[0].Points.Clear();
                listViewBestLabs.Items.Clear();
                foreach (KeyValuePair<string, int> kvp in labCounts)
                {
                    string sName = kvp.Key;
                    if (sName != "The Labyrinth")
                    {
                        sName = sName.Replace("The ", "").Replace(" Labyrinth", "");
                    }
                    if (sName == "Unknown")
                    {
                        sName += "*";
                    }

                    chartLabsDone.Series[0].Points.AddXY(sName, kvp.Value);
                    chartLabsAvgTime.Series[0].Points.AddXY(sName, Math.Round(labAvgTimes[kvp.Key] / 60, 1));

                    ListViewItem lvi = new ListViewItem(kvp.Key);

                    if (labBestTimes[kvp.Key] != null)
                    {
                        lvi.SubItems.Add(labBestTimes[kvp.Key].StopWatchValue);
                        lvi.SubItems.Add(labBestTimes[kvp.Key].Started.ToString());
                    }
                    else
                    {
                        lvi.SubItems.Add("-");
                        lvi.SubItems.Add("-");
                    }
                    listViewBestLabs.Items.Add(lvi);


                }
            };
            BeginInvoke(mi);

        }

        /// <summary>
        /// Render global dashboard
        /// </summary>
        public void RenderGlobalDashboard()
        {
            Dictionary<ACTIVITY_TYPES, double> typeList = new Dictionary<ACTIVITY_TYPES, double>
            {
                { ACTIVITY_TYPES.MAP, 0 },
                { ACTIVITY_TYPES.HEIST, 0 },
                { ACTIVITY_TYPES.DELVE, 0 },
                { ACTIVITY_TYPES.LABYRINTH, 0 },
                { ACTIVITY_TYPES.SIMULACRUM, 0 },
                { ACTIVITY_TYPES.TEMPLE, 0 },
                { ACTIVITY_TYPES.MAVEN_INVITATION, 0 },
                { ACTIVITY_TYPES.ATZIRI, 0 },
                { ACTIVITY_TYPES.UBER_ATZIRI, 0 },
                { ACTIVITY_TYPES.ELDER_FIGHT, 0 },
                { ACTIVITY_TYPES.SHAPER_FIGHT, 0 },
                { ACTIVITY_TYPES.MAVEN_FIGHT, 0 },
                { ACTIVITY_TYPES.SIRUS_FIGHT, 0 },
                { ACTIVITY_TYPES.CAMPAIGN, 0 },
                { ACTIVITY_TYPES.VAAL_SIDEAREA, 0 },
                { ACTIVITY_TYPES.ABYSSAL_DEPTHS, 0 },
                { ACTIVITY_TYPES.LAB_TRIAL, 0 },
                { ACTIVITY_TYPES.LOGBOOK, 0 },
                { ACTIVITY_TYPES.LOGBOOK_SIDE, 0 },
                { ACTIVITY_TYPES.CATARINA_FIGHT, 0 },
                { ACTIVITY_TYPES.SAFEHOUSE, 0 },
                { ACTIVITY_TYPES.BREACHSTONE, 0 },
                { ACTIVITY_TYPES.SEARING_EXARCH_FIGHT, 0 },
                { ACTIVITY_TYPES.BLACK_STAR_FIGHT, 0 },
                { ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT, 0 },
                { ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT, 0 },
                { ACTIVITY_TYPES.TIMELESS_LEGION, 0 },
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, 0 },
                { ACTIVITY_TYPES.SANCTUM, 0 },
                { ACTIVITY_TYPES.OTHER, 0 },
                { ACTIVITY_TYPES.TRIALMASTER_FIGHT, 0 },
                { ACTIVITY_TYPES.TANES_LABORATORY, 0 },
                { ACTIVITY_TYPES.ANCESTOR_TRIAL, 0},
                { ACTIVITY_TYPES.INSCRIBED_ULTIMATUM, 0},
                { ACTIVITY_TYPES.KINGSMARCH, 0}
            };

            Dictionary<ACTIVITY_TYPES, int> typeListCount = new Dictionary<ACTIVITY_TYPES, int>
            {
                { ACTIVITY_TYPES.MAP, 0 },
                { ACTIVITY_TYPES.HEIST, 0 },
                { ACTIVITY_TYPES.DELVE, 0 },
                { ACTIVITY_TYPES.LABYRINTH, 0 },
                { ACTIVITY_TYPES.SIMULACRUM, 0 },
                { ACTIVITY_TYPES.TEMPLE, 0 },
                { ACTIVITY_TYPES.MAVEN_INVITATION, 0 },
                { ACTIVITY_TYPES.ATZIRI, 0 },
                { ACTIVITY_TYPES.UBER_ATZIRI, 0 },
                { ACTIVITY_TYPES.ELDER_FIGHT, 0 },
                { ACTIVITY_TYPES.SHAPER_FIGHT, 0 },
                { ACTIVITY_TYPES.MAVEN_FIGHT, 0 },
                { ACTIVITY_TYPES.SIRUS_FIGHT, 0 },
                { ACTIVITY_TYPES.CAMPAIGN, 0 },
                { ACTIVITY_TYPES.VAAL_SIDEAREA, 0 },
                { ACTIVITY_TYPES.ABYSSAL_DEPTHS, 0 },
                { ACTIVITY_TYPES.LAB_TRIAL, 0 },
                { ACTIVITY_TYPES.LOGBOOK, 0 },
                { ACTIVITY_TYPES.LOGBOOK_SIDE, 0 },
                { ACTIVITY_TYPES.CATARINA_FIGHT, 0 },
                { ACTIVITY_TYPES.SAFEHOUSE, 0 },
                { ACTIVITY_TYPES.BREACHSTONE, 0 },
                { ACTIVITY_TYPES.SEARING_EXARCH_FIGHT, 0 },
                { ACTIVITY_TYPES.BLACK_STAR_FIGHT, 0 },
                { ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT, 0 },
                { ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT, 0 },
                { ACTIVITY_TYPES.TIMELESS_LEGION, 0 },
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, 0 },
                { ACTIVITY_TYPES.SANCTUM, 0 },
                { ACTIVITY_TYPES.OTHER, 0 },
                { ACTIVITY_TYPES.TRIALMASTER_FIGHT, 0 },
                { ACTIVITY_TYPES.TANES_LABORATORY, 0 },
                { ACTIVITY_TYPES.ANCESTOR_TRIAL, 0},
                { ACTIVITY_TYPES.INSCRIBED_ULTIMATUM, 0},
                { ACTIVITY_TYPES.KINGSMARCH, 0}
            };

            Dictionary<ACTIVITY_TYPES, Color> colorList = new Dictionary<ACTIVITY_TYPES, Color>
            {
                { ACTIVITY_TYPES.MAP, Color.Green },
                { ACTIVITY_TYPES.HEIST, Color.Red },
                { ACTIVITY_TYPES.DELVE, Color.Orange },
                { ACTIVITY_TYPES.LABYRINTH, Color.DarkTurquoise },
                { ACTIVITY_TYPES.SIMULACRUM, Color.Gray },
                { ACTIVITY_TYPES.TEMPLE, Color.MediumSeaGreen },
                { ACTIVITY_TYPES.MAVEN_INVITATION, Color.Violet },
                { ACTIVITY_TYPES.ATZIRI, Color.OrangeRed },
                { ACTIVITY_TYPES.UBER_ATZIRI, Color.OrangeRed },
                { ACTIVITY_TYPES.ELDER_FIGHT, Color.Gray },
                { ACTIVITY_TYPES.SHAPER_FIGHT, Color.MediumVioletRed },
                { ACTIVITY_TYPES.MAVEN_FIGHT, Color.Blue },
                { ACTIVITY_TYPES.SIRUS_FIGHT, Color.AntiqueWhite },
                { ACTIVITY_TYPES.OTHER, Color.Gray },
                { ACTIVITY_TYPES.CAMPAIGN, Color.Turquoise },
                { ACTIVITY_TYPES.VAAL_SIDEAREA, Color.DarkRed },
                { ACTIVITY_TYPES.ABYSSAL_DEPTHS, Color.LightGreen },
                { ACTIVITY_TYPES.LAB_TRIAL, Color.DarkTurquoise },
                { ACTIVITY_TYPES.LOGBOOK, Color.LightBlue },
                { ACTIVITY_TYPES.LOGBOOK_SIDE, Color.LightBlue },
                { ACTIVITY_TYPES.CATARINA_FIGHT, Color.Orange },
                { ACTIVITY_TYPES.BREACHSTONE, Color.PaleVioletRed },
                { ACTIVITY_TYPES.BLACK_STAR_FIGHT, Color.Red },
                { ACTIVITY_TYPES.SEARING_EXARCH_FIGHT, Color.Red },
                { ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT, Color.Blue },
                { ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT, Color.Blue },
                { ACTIVITY_TYPES.TIMELESS_LEGION, Color.BlueViolet },
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, Color.Silver },
                { ACTIVITY_TYPES.SANCTUM, Color.Purple },
                { ACTIVITY_TYPES.TRIALMASTER_FIGHT, Color.Red },
                { ACTIVITY_TYPES.TANES_LABORATORY, Color.LimeGreen },
                { ACTIVITY_TYPES.ANCESTOR_TRIAL, Color.Turquoise},
                { ACTIVITY_TYPES.INSCRIBED_ULTIMATUM, Color.MediumVioletRed},
                { ACTIVITY_TYPES.KINGSMARCH, Color.SaddleBrown }

            };
            double hideOutTime = 0;
            double totalCount = 0;
            if (Convert.ToBoolean(ReadSetting("pie_chart_show_hideout", "true")))
            {
                long ts1 = ((DateTimeOffset)_statsDate1).ToUnixTimeSeconds();
                long ts2 = ((DateTimeOffset)_statsDate2).ToUnixTimeSeconds();
                hideOutTime = _logic.Stats.GetIncrementValue("HideoutTimeSec", ts1, ts2);
                totalCount += hideOutTime;
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                try
                {
                    int iCap = _timeCaps[act.Type];

                    typeListCount[act.Type]++;

                    // Filter out
                    if (act.TotalSeconds < iCap)
                    {
                        typeList[act.Type] += act.TotalSeconds;
                        totalCount += act.TotalSeconds;
                    }
                    else
                    {
                        typeList[act.Type] += iCap;
                        totalCount += iCap;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }

            chartGlobalDashboard.Series[0].Points.Clear();
            listView1.Items.Clear();
            double dOther = 0;
            foreach (KeyValuePair<ACTIVITY_TYPES, double> kvp in typeList)
            {
                ListViewItem lvi;

                // calculate percent value
                double percentVal = 0;


                if (kvp.Value > 0)
                {
                    percentVal = kvp.Value / totalCount * 100;
                    percentVal = Math.Round(percentVal, 2);
                }

                if (percentVal >= 5)
                {
                    TimeSpan tsDuration = TimeSpan.FromSeconds(kvp.Value);
                    chartGlobalDashboard.Series[0].Points.AddXY(kvp.Key.ToString(), Math.Round(kvp.Value / 60 / 60, 1));
                    chartGlobalDashboard.Series[0].Points.Last().Color = colorList[kvp.Key];
                    chartGlobalDashboard.Series[0].Points.Last().Label = kvp.Value > 0 ? string.Format("{0} h", Math.Round(tsDuration.TotalHours, 1)) : " ";
                    chartGlobalDashboard.Series[0].Points.Last().LegendText = $"{kvp.Key} ({percentVal}%)";

                    lvi = new ListViewItem(kvp.Key.ToString());
                    lvi.SubItems.Add(typeListCount[kvp.Key].ToString());
                    lvi.SubItems.Add(Math.Round(kvp.Value / 60 / 60, 1).ToString() + " h");
                    lvi.SubItems.Add(percentVal + "%");
                    lvi.BackColor = colorList[kvp.Key];
                    listView1.Items.Add(lvi);
                }
                else
                {
                    dOther += kvp.Value;
                    lvi = new ListViewItem(kvp.Key.ToString());
                    lvi.SubItems.Add(typeListCount[kvp.Key].ToString());
                    lvi.SubItems.Add(Math.Round(kvp.Value / 60 / 60, 1).ToString() + " h");
                    lvi.SubItems.Add(percentVal + "%");
                    lvi.BackColor = colorList[ACTIVITY_TYPES.OTHER];

                    listView1.Items.Add(lvi);
                }
            }

            // Add Other
            double percentValOther = dOther / totalCount * 100;
            percentValOther = Math.Round(percentValOther, 2);
            TimeSpan tsDurationOther = TimeSpan.FromSeconds(dOther);
            chartGlobalDashboard.Series[0].Points.AddXY("Other", Math.Round(dOther / 60 / 60, 1));
            chartGlobalDashboard.Series[0].Points.Last().Color = colorList[ACTIVITY_TYPES.OTHER];
            chartGlobalDashboard.Series[0].Points.Last().Label = dOther > 0 ? $"{Math.Round(tsDurationOther.TotalHours, 1)} h" : " ";
            chartGlobalDashboard.Series[0].Points.Last().LegendText = $"Other ({percentValOther}%)";

            if (_showHideoutInPie)
            {
                // Add HO
                double percentValHO = hideOutTime / totalCount * 100;
                percentValHO = Math.Round(percentValHO, 2);
                TimeSpan tsDurationHO = TimeSpan.FromSeconds(hideOutTime);
                chartGlobalDashboard.Series[0].Points.AddXY("Hideout", Math.Round(tsDurationHO.TotalSeconds / 60 / 60, 1));
                chartGlobalDashboard.Series[0].Points.Last().Color = Color.Blue; ;
                chartGlobalDashboard.Series[0].Points.Last().Label = tsDurationHO.TotalSeconds > 0 ? string.Format("{0} h", Math.Round(tsDurationHO.TotalHours, 1)) : " ";
                chartGlobalDashboard.Series[0].Points.Last().LegendText = $"Hideout ({percentValHO}%)";

                ListViewItem lvi = new ListViewItem("HIDEOUT");
                lvi.SubItems.Add("-");
                lvi.SubItems.Add(Math.Round(tsDurationHO.TotalSeconds / 60 / 60, 1).ToString() + " h");
                lvi.SubItems.Add(percentValHO + "%");
                lvi.BackColor = Color.Blue;

                listView1.Items.Add(lvi);
            }

            label6.Text = String.Format($"Total time in activities: {Math.Round(totalCount / 60 / 60, 2)} hours");
            materialLabel10.Text = String.Format($"Total time in hideout: {Math.Round(hideOutTime / 60 / 60, 2)} hours");
        }

        /// <summary>
        /// Render Heist Dashboard
        /// </summary>
        public void RenderHeistDashboard()
        {
            List<KeyValuePair<string, int>> tmpList = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> top10 = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> tmpListTags = new Dictionary<string, int>();
            List<KeyValuePair<string, int>> top10Tags = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> countByArea = new Dictionary<string, int>();

            // MAP AREAS
            foreach (string s in _defaultMappings.HeistAreas)
            {
                countByArea.Add(s, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.HEIST)
                {
                    countByArea[act.Area]++;
                }
            }

            foreach (KeyValuePair<string, int> kvp in countByArea)
            {
                tmpList.Add(kvp);
            }

            tmpList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });
            tmpList.Reverse();
            top10.AddRange(tmpList);
            listView4.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in top10)
            {
                ListViewItem lvi = new ListViewItem(kvp.Key);
                lvi.SubItems.Add(kvp.Value.ToString());
                listView4.Items.Add(lvi);
            }

            // TAG CALC
            tmpList.Clear();
            foreach (TrX_ActivityTag tg in _logic.Tags)
            {
                tmpListTags.Add(tg.ID, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.HEIST)
                {
                    foreach (string s in act.Tags)
                    {
                        if (!String.IsNullOrEmpty(s))
                        {
                            if (tmpListTags.ContainsKey(s))
                            {
                                int iVal = tmpListTags[s];
                                tmpListTags[s]++;
                            }

                        }
                    }
                }
            }

            foreach (KeyValuePair<string, int> kvp in tmpListTags)
            {
                tmpList.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
            }

            tmpList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });
            tmpList.Reverse();
            top10Tags.AddRange(tmpList);
            listView5.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in top10Tags)
            {
                if (kvp.Value > 0)
                {
                    ListViewItem lvi = new ListViewItem(kvp.Key);
                    lvi.SubItems.Add(kvp.Value.ToString());
                    listView5.Items.Add(lvi);
                }

            }

            Dictionary<int, double> levelAvgs = new Dictionary<int, double>();
            Dictionary<int, int> levelCounts = new Dictionary<int, int>();
            for (int i = 67; i <= 83; i++)
            {
                int iCount = 0;
                int iSum = 0;

                foreach (TrX_TrackedActivity act in _statsDataSource)
                {
                    if (act.Type == ACTIVITY_TYPES.HEIST && act.AreaLevel == i)
                    {
                        iCount++;

                        if (act.TotalSeconds < _timeCaps[ACTIVITY_TYPES.HEIST])
                        {
                            iSum += act.TotalSeconds;
                        }
                        else
                        {
                            iSum += _timeCaps[ACTIVITY_TYPES.HEIST];
                        }

                    }
                }

                levelAvgs.Add(i, (iCount > 0 && iSum > 0) ? (iSum / iCount) : 0);
                levelCounts.Add(i, iCount);
            }

            MethodInvoker mi = delegate
            {
                chartHeistAvgTime.Series[0].Points.Clear();
                chartHeistByLevel.Series[0].Points.Clear();

                foreach (KeyValuePair<int, double> kvp in levelAvgs)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(kvp.Value);

                    chartHeistAvgTime.Series[0].Points.AddXY(kvp.Key, Math.Round(kvp.Value / 60, 1));
                    chartHeistByLevel.Series[0].Points.AddXY(kvp.Key, levelCounts[kvp.Key]);
                }
            };
            BeginInvoke(mi);
        }

        /// <summary>
        /// Update All-Stats Dashboard
        /// </summary>
        private void RenderAllStatsDashboard()
        {
            if (!_workerAllStats.IsBusy)
            {
                _workerAllStats.RunWorkerAsync();
            }
        }

        private void GenerateAllStatsData()
        {
            double days = (_allStatsChartDT2 - _allStatsChartDT1).TotalDays;

            if (!string.IsNullOrEmpty(_allStatsSelected))
            {
                _allStatChartresults.Clear();
                _allStatChartresults = _logic.Stats.GetByDayValues(_allStatsSelected, ((DateTimeOffset)_allStatsChartDT1).ToUnixTimeSeconds(), ((DateTimeOffset)_allStatsChartDT2).ToUnixTimeSeconds(), _allStatsChartInterval);
            }
        }

        /// <summary>
        /// Update chart in all stats dashboard
        /// </summary>
        private void UpdateAllStatsChart()
        {
            _allStatsChartDT1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dateTimeFormatInfo.Calendar);
            _allStatsChartDT2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dateTimeFormatInfo.Calendar);

            double days = (_allStatsChartDT2 - _allStatsChartDT1).TotalDays;
            _allStatsChartInterval = 1;

            if (days >= 365)
            {
                _allStatsChartInterval = 14;
            }
            else if (days > 150)
            {
                _allStatsChartInterval = 7;
            }
            else if (days > 30)
            {
                _allStatsChartInterval = 2;
            }

            if (!_workerAllStatsChart.IsBusy)
            {
                _workerAllStatsChart.RunWorkerAsync();
            }
        }

        public void RenderLeagueDashboard()
        {
            // Get last 5 leagues
            List<TrX_LeagueInfo> leagues, tmp;
            leagues = new List<TrX_LeagueInfo>();
            tmp = new List<TrX_LeagueInfo>();
            tmp.AddRange(_leagues);
            tmp.Reverse();
            leagues = tmp.GetRange(0, Convert.ToInt32(comboBox10.SelectedItem.ToString()));
            leagues.Reverse();

            // Stats dict
            Dictionary<TrX_LeagueInfo, double> totalActCount = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> totalActTime = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> totalMapsDone = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> totalMapsDone16 = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> avgMapDuration = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> avgMapDuration16 = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> deathCount = new Dictionary<TrX_LeagueInfo, double>();
            Dictionary<TrX_LeagueInfo, double> timeInCampaign = new Dictionary<TrX_LeagueInfo, double>();

            DateTime dt1, dt2;
            long ts1, ts2;
            foreach (TrX_LeagueInfo li in leagues)
            {
                dt1 = li.Start;
                dt2 = li.End;
                ts1 = ((DateTimeOffset)li.Start).ToUnixTimeSeconds();
                ts2 = ((DateTimeOffset)li.End).ToUnixTimeSeconds();

                List<TrX_TrackedActivity> _leagueActivities;
                _leagueActivities = FilterActivitiesByTimeRange(dt1, dt2, _logic.ActivityHistory);


                long totalActTimeLeague = 0;
                long totalMapsCount = 0;
                long totalMapsT16 = 0;
                long totalMapDuration = 0;
                long totalMapDuration16 = 0;
                long durationValue = 0;
                long campaignTime = 0;
                int death = 0;
                foreach (TrX_TrackedActivity act in _leagueActivities)
                {
                    if (act.TotalSeconds <= _timeCaps[act.Type])
                    {
                        durationValue = act.TotalSeconds;
                    }
                    else
                    {
                        durationValue = _timeCaps[act.Type];
                    }

                    totalActTimeLeague += durationValue;
                    if (act.Type == ACTIVITY_TYPES.MAP)
                    {
                        totalMapsCount++;
                        totalMapDuration += durationValue;
                        if (act.AreaLevel >= 83)
                        {
                            totalMapsT16++;
                            totalMapDuration16 += durationValue;
                        }
                    }

                    if (act.Type == ACTIVITY_TYPES.CAMPAIGN)
                    {
                        campaignTime += durationValue;
                    }

                    death += act.DeathCounter;
                }

                totalActCount[li] = _leagueActivities.Count;
                totalActTime[li] = totalActTimeLeague;
                totalMapsDone[li] = totalMapsCount;
                totalMapsDone16[li] = totalMapsT16;

                if (totalMapsCount > 0)
                {
                    avgMapDuration[li] = totalMapDuration / totalMapsCount;
                }
                else
                {
                    avgMapDuration[li] = 0;
                }

                if (totalMapDuration16 > 0)
                {
                    avgMapDuration16[li] = totalMapDuration16 / totalMapsT16;
                }
                else
                {
                    avgMapDuration16[li] = 0;
                }

                deathCount[li] = death;
                timeInCampaign[li] = campaignTime;
            }

            MethodInvoker mi = delegate
            {
                chartLeagueMapsDone.Series[0].Points.Clear();
                chartLeagueTotalActivities.Series[0].Points.Clear();
                chartLeagueActTime.Series[0].Points.Clear();
                chartLeagueMapT16.Series[0].Points.Clear();
                chartLeagueAvgMapT16.Series[0].Points.Clear();
                chartLeagueAvgMap.Series[0].Points.Clear();
                chartLeagueDeath.Series[0].Points.Clear();
                chartLeagueCampaign.Series[0].Points.Clear();

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in totalMapsDone)
                {
                    chartLeagueMapsDone.Series[0].Points.AddXY(kvp.Key.Name, kvp.Value);

                    DataPoint dp = chartLeagueMapsDone.Series[0].Points.Last();
                    dp.Label = kvp.Value > 0 ? kvp.Value.ToString() : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in totalMapsDone16)
                {
                    chartLeagueMapT16.Series[0].Points.AddXY(kvp.Key.Name, kvp.Value);

                    DataPoint dp = chartLeagueMapT16.Series[0].Points.Last();
                    dp.Label = kvp.Value > 0 ? kvp.Value.ToString() : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in totalActCount)
                {
                    chartLeagueTotalActivities.Series[0].Points.AddXY(kvp.Key.Name, kvp.Value);

                    DataPoint dp = chartLeagueTotalActivities.Series[0].Points.Last();
                    dp.Label = kvp.Value > 0 ? kvp.Value.ToString() : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in totalActTime)
                {
                    double hours = Math.Round(kvp.Value / 60 / 60, 1);

                    chartLeagueActTime.Series[0].Points.AddXY(kvp.Key.Name, hours);

                    DataPoint dp = chartLeagueActTime.Series[0].Points.Last();
                    dp.Label = hours > 0 ? $"{hours} hours" : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in avgMapDuration)
                {
                    double minutes = Math.Round(kvp.Value / 60, 1);

                    chartLeagueAvgMap.Series[0].Points.AddXY(kvp.Key.Name, minutes);

                    DataPoint dp = chartLeagueAvgMap.Series[0].Points.Last();
                    dp.Label = minutes > 0 ? $"{minutes} minutes" : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in avgMapDuration16)
                {
                    double minutes = Math.Round(kvp.Value / 60, 1);

                    chartLeagueAvgMapT16.Series[0].Points.AddXY(kvp.Key.Name, minutes);

                    DataPoint dp = chartLeagueAvgMapT16.Series[0].Points.Last();
                    dp.Label = minutes > 0 ? $"{minutes} minutes" : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in deathCount)
                {
                    chartLeagueDeath.Series[0].Points.AddXY(kvp.Key.Name, kvp.Value);

                    DataPoint dp = chartLeagueDeath.Series[0].Points.Last();
                    dp.Label = kvp.Value > 0 ? (kvp.Value).ToString() : "";
                }

                foreach (KeyValuePair<TrX_LeagueInfo, double> kvp in timeInCampaign)
                {
                    double hours = Math.Round(kvp.Value / 60 / 60, 1);

                    chartLeagueCampaign.Series[0].Points.AddXY(kvp.Key.Name, hours);

                    DataPoint dp = chartLeagueCampaign.Series[0].Points.Last();
                    dp.Label = hours > 0 ? $"{hours} hours" : "";
                }

            };
            BeginInvoke(mi);
        }

        /// <summary>
        /// Update the mapping dashboard
        /// </summary>
        public void RenderMappingDashboard()
        {
            List<KeyValuePair<string, int>> tmpList = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> top10 = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> tmpListTags = new Dictionary<string, int>();
            Dictionary<string, int> countByArea = new Dictionary<string, int>();
            Dictionary<int, int> countByTier = new Dictionary<int, int>();

            // MAP AREAS
            foreach (string s in _defaultMappings.MapAreas)
            {
                countByArea.Add(s, 0);
            }

            for (int i = 0; i <= 19; i++)
            {
                countByTier.Add(i, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.MAP)
                {
                    if (comboBox3.Text != "All" && comboBox3.Text != act.Area)
                        continue;

                    countByArea[act.Area]++;
                    countByTier[act.MapTier]++;
                }
            }

            foreach (KeyValuePair<string, int> kvp in countByArea)
            {
                tmpList.Add(kvp);
            }

            tmpList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });
            tmpList.Reverse();
            listViewMapsByArea.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in tmpList)
            {
                ListViewItem lvi = new ListViewItem(kvp.Key);
                lvi.SubItems.Add(kvp.Value.ToString());
                listViewMapsByArea.Items.Add(lvi);
            }

            // TAG CALC
            tmpList.Clear();
            foreach (TrX_ActivityTag tg in _logic.Tags)
            {
                tmpListTags.Add(tg.ID, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.MAP)
                {
                    if (comboBox3.Text != "All" && comboBox3.Text != act.Area)
                        continue;

                    foreach (string s in act.Tags)
                    {
                        if (!String.IsNullOrEmpty(s))
                        {
                            if (tmpListTags.ContainsKey(s))
                            {
                                int iVal = tmpListTags[s];
                                tmpListTags[s]++;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, int> kvp in tmpListTags)
            {
                tmpList.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
            }

            tmpList.Sort(
                delegate (KeyValuePair<string, int> pair1,
                KeyValuePair<string, int> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });
            tmpList.Reverse();
            listViewTaggingOverview.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in tmpList)
            {
                if (kvp.Value > 0)
                {
                    ListViewItem lvi = new ListViewItem(kvp.Key);
                    lvi.SubItems.Add(kvp.Value.ToString());
                    listViewTaggingOverview.Items.Add(lvi);
                }
            }

            double[] tierAverages = new double[]
            {
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
            };

            for (int i = 0; i < 16; i++)
            {
                int iSum = 0;
                int iCount = 0;

                foreach (TrX_TrackedActivity act in _statsDataSource)
                {
                    if (act.Type == ACTIVITY_TYPES.MAP && act.MapTier == (i + 1))
                    {
                        if (comboBox3.Text != "All" && comboBox3.Text != act.Area)
                            continue;

                        if (act.TotalSeconds < _timeCaps[ACTIVITY_TYPES.MAP])
                        {
                            iSum += act.TotalSeconds;
                        }
                        else
                        {
                            iSum += _timeCaps[ACTIVITY_TYPES.MAP];
                        }

                        iCount++;
                    }
                }

                if (iSum > 0 && iCount > 0)
                {
                    tierAverages[i] = (iSum / iCount);
                }
            }

            MethodInvoker mi = delegate
            {
                chartMapTierCount.Series[0].Points.Clear();
                for (int i = 1; i <= 19; i++)
                {
                    chartMapTierCount.Series[0].Points.AddXY(i, countByTier[i]);
                }

                chartMapTierAvgTime.Series[0].Points.Clear();
                for (int i = 0; i < tierAverages.Length; i++)
                {
                    chartMapTierAvgTime.Series[0].Points.AddXY(i + 1, Math.Round(tierAverages[i] / 60, 1));
                }
            };
            BeginInvoke(mi);
        }

        /// <summary>
        /// Add setting if not exists, otherwise update existing
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="b_log"></param>
        public void AddUpdateAppSettings(string key, string value)
        {
            _mySettings.AddOrUpdateSetting(key, value);
            _mySettings.WriteToXml();
        }

        /// <summary>
        /// Get activity tag object for ID
        /// </summary>
        /// <param name="s_id"></param>
        /// <returns></returns>
        public TrX_ActivityTag GetTagByID(string s_id)
        {
            foreach (TrX_ActivityTag tag in _logic.Tags)
            {
                if (tag.ID == s_id)
                    return tag;
            }
            return null;
        }

        /// <summary>
        /// Create a TraXile backup
        /// </summary>
        /// <param name="s_name"></param>
        private void CreateBackup(string s_name)
        {
            string sBackupDir = TrX_Static.APPDATA_PATH + @"/backups/" + s_name + @"/" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            System.IO.Directory.CreateDirectory(sBackupDir);

            if (System.IO.File.Exists(_logic.ClientTxtPath))
                System.IO.File.Copy(_logic.ClientTxtPath, sBackupDir + @"/Client.txt");
            if (System.IO.File.Exists(TrX_Static.CACHE_PATH))
                System.IO.File.Copy(TrX_Static.CACHE_PATH, sBackupDir + @"/stats.cache");
            if (System.IO.File.Exists(TrX_Static.DB_PATH))
                System.IO.File.Copy(TrX_Static.DB_PATH, sBackupDir + @"/data.db");
            if (System.IO.File.Exists(TrX_Static.APPDATA_PATH + @"\config.xml"))
                System.IO.File.Copy(TrX_Static.APPDATA_PATH + @"\config.xml", sBackupDir + @"/config.xml");
            if (System.IO.File.Exists(TrX_Static.APPDATA_PATH + @"\labdata.xml"))
                System.IO.File.Copy(TrX_Static.APPDATA_PATH + @"\labdata.xml", sBackupDir + @"/labdata.xml");
        }

        /// <summary>
        /// Fully reset the application
        /// </summary>
        private void DoFullReset()
        {
            // Make logfile empty
            FileStream fs1 = new FileStream(_logic.ClientTxtPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            fs1.Close();
            InitLogFileReload();
        }

        /// <summary>
        /// Open details for tracked activity
        /// </summary>
        /// <param name="ta"></param>
        private void OpenActivityDetails(TrX_TrackedActivity ta)
        {
            OpenChildWindow(new ActivityEntryDetails(ta, this, msm));
        }

        /// <summary>
        /// Open child window and apply theme to i´t
        /// </summary>
        /// <param name="form">Form to open</param>
        private void OpenChildWindow(Form form, bool handle_by_msm = false)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.ShowInTaskbar = false;
            form.Location = new Point(this.Location.X + 100, this.Location.Y + 100);
            form.Owner = this;

            if (handle_by_msm)
                msm.AddFormToManage((MaterialForm)form);

            DoManualThemeAdjustments(form);


            form.Show();
        }

        private List<TrX_TrackedActivity> GetSelectedActivities()
        {
            List<TrX_TrackedActivity> results = new List<TrX_TrackedActivity>();
            foreach (ListViewItem lvi in listViewActLog.SelectedItems)
            {
                results.Add(GetActivityFromListItemName(lvi.Name));
            }

            return results;
        }

        private List<TrX_TrackedActivity> GetExportActivitySouce(string sourceType)
        {
            List<TrX_TrackedActivity> results = new List<TrX_TrackedActivity>();
            switch (sourceType)
            {
                case "All entries":
                    results = _logic.ActivityHistory;
                    break;

                case "Filtered entries":
                    results = _statsDataSource;
                    break;

                case "Selected entries":
                    results = GetSelectedActivities();
                    break;
            }
            return results;
        }


        /// <summary>
        /// Export Activity log to CSV
        /// </summary>
        /// <param name="sPath"></param>
        public void WriteActivitiesToCSV(string sPath, string sourceType)
        {
            StreamWriter wrt = new StreamWriter(sPath);

            //Write headline
            string sLine = TrX_TrackedActivity.GetCSVHeadline();
            wrt.WriteLine(sLine);

            List<TrX_TrackedActivity> src = GetExportActivitySouce(sourceType);

            for (int i = 0; i < src.Count; i++)
            {
                wrt.WriteLine(src[i].ToCSVLine());
            }
            wrt.Close();
        }

        /// <summary>
        /// Export Activity log to CSV
        /// </summary>
        /// <param name="sPath"></param>
        public void WriteActivitiesToJSON(string sPath, string sourceType)
        {
            StreamWriter wrt = new StreamWriter(sPath);

            List<TrX_BackendSync_ActivityDocument> serializables;
            serializables = new List<TrX_BackendSync_ActivityDocument>();

            List<TrX_TrackedActivity> src = GetExportActivitySouce(sourceType);

            foreach (TrX_TrackedActivity act in src)
            {
                serializables.Add(act.GetSerializableObject());
            }

            wrt.WriteLine(JsonConvert.SerializeObject(serializables, Newtonsoft.Json.Formatting.Indented));

            wrt.Close();
        }

        /// <summary>
        /// Prepare backup restore before app restarts
        /// </summary>
        /// <param name="sPath"></param>
        private void PrepareBackupRestore(string sPath)
        {
            File.Copy(sPath + @"/data.db", TrX_Static.DB_PATH + ".restore");
            File.Copy(sPath + @"/Client.txt", Directory.GetParent(_logic.ClientTxtPath) + @"/_Client.txt.restore");
            File.Copy(sPath + @"/config.xml", TrX_Static.APPDATA_PATH + @"/config.xml.restore");
            if (File.Exists(sPath + @"/labdata.xml"))
            {
                File.Copy(sPath + @"/labdata.xml", TrX_Static.APPDATA_PATH + @"/labdata.xml.restore");
            }
            _log.Info("Backup restore successfully prepared! Restarting Application");
            Application.Restart();
        }

        /// <summary>
        /// Check if backup restore is prepared and restore
        /// </summary>
        private void DoBackupRestoreIfPrepared()
        {
            if (File.Exists(TrX_Static.DB_PATH + ".restore"))
            {
                File.Delete(TrX_Static.DB_PATH);
                File.Move(TrX_Static.DB_PATH + ".restore", TrX_Static.DB_PATH);
                _log.Info($"BackupRestored -> Source: _data.db.restore, Destination: {TrX_Static.DB_PATH}");
                _restoreMode = true;
            }

            if (File.Exists(TrX_Static.APPDATA_PATH + @"\config.xml.restore"))
            {
                File.Delete(TrX_Static.APPDATA_PATH + @"\config.xml");
                File.Move(TrX_Static.APPDATA_PATH + @"\config.xml.restore", TrX_Static.APPDATA_PATH + @"\config.xml");
                _log.Info($@"BackupRestored -> Source: config.xml.restore, Destination: {TrX_Static.APPDATA_PATH}\config.xml");
                _restoreMode = true;
            }

            if (File.Exists(TrX_Static.APPDATA_PATH + @"\labdata.xml.restore"))
            {
                try
                {
                    File.Delete(TrX_Static.APPDATA_PATH + @"\labdata.xml");
                }
                catch (Exception ex)
                {
                    _log.Warn($"cannot delete labdata.xml: {ex.Message}");
                }

                File.Move(TrX_Static.APPDATA_PATH + @"\labdata.xml.restore", TrX_Static.APPDATA_PATH + @"\labdata.xml");
                _log.Info($@"BackupRestored -> Source: labdata.xml.restore, Destination: {TrX_Static.APPDATA_PATH}\labdata.xml");
                _restoreMode = true;
            }

            try
            {
                string clientTxtPath;
                clientTxtPath = _mySettings.ReadSetting("poe_logfile_path", null);

                if (!string.IsNullOrEmpty(clientTxtPath) && File.Exists(Directory.GetParent(clientTxtPath) + @"/_Client.txt.restore"))
                {
                    File.Delete(clientTxtPath);
                    File.Move(Directory.GetParent(clientTxtPath) + @"/_Client.txt.restore", clientTxtPath);
                    _log.Info($@"BackupRestored -> Source: {Directory.GetParent(clientTxtPath)}/_Client.txt.restore" +
                        $@", Destination: {Directory.GetParent(clientTxtPath)}/_Client.txt");
                    _restoreMode = true;
                }

            }
            catch (Exception ex)
            {
                _log.Error("Could not restore Client.txt, please make sure that Path of Exile is not running.");
                _log.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// Check if a tag with given ID exists
        /// </summary>
        /// <param name="s_id"></param>
        /// <returns></returns>
        private bool CheckTagExists(string s_id)
        {
            foreach (TrX_ActivityTag tag in _logic.Tags)
            {
                if (tag.ID == s_id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add a new tag
        /// </summary>
        /// <param name="tag"></param>
        private void AddTag(TrX_ActivityTag tag)
        {
            _logic.Tags.Add(tag);
            _logic.Database.DoNonQuery("INSERT INTO tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type, tag_show_in_lv) VALUES "
                + $"('{tag.ID}', '{tag.DisplayName}', '{tag.BackColor.ToArgb()}', '{tag.ForeColor.ToArgb()}', 'custom', {(tag.ShowInListView ? "1" : "0")})");

            listViewActLog.Columns.Add(tag.DisplayName);
            comboBox_Filter_Tags.Items.Add(tag.DisplayName);
            ResetMapHistory();
            RequestHistoryUpdate();
            RenderTagsForConfig(true);
            RenderTagsForTracking(true);
        }

        /// <summary>
        /// Timer for GUI updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_logic.ClientTxtPath) || !_UpdateCheckDone)
            {
                double dProgress = 0;
                if (!_logic.EventQueueInitialized)
                {
                    Hide();
                    if (_logic.LogLinesRead > 0)
                        dProgress = (_logic.LogLinesRead / _logic.LogLinesTotal) * 100;
                    _loadScreenWindow.progressBar.Value = Convert.ToInt32(dProgress);
                    _loadScreenWindow.progressLabel.Text = "Parsing logfile. This could take a while the first time.";
                    _loadScreenWindow.progressLabel2.Text = Math.Round(dProgress, 2).ToString() + "%";
                }
                else
                {
                    try
                    {
                        UpdateUI();
                        _failedUIUpdates = 0;
                    }
                    catch (Exception ex)
                    {
                        _failedUIUpdates++;

                        if (_failedUIUpdates >= 20)
                        {
                            if (!_criticalUIError)
                            {
                                _criticalUIError = true;
                                MessageBox.Show("There was a critical Error updating the UI. TraXile will restart now. If this keeps happening, please contact me at: dermow@posteo.de.");
                            }
                        }
                        else
                        {
                            _log.Warn($"Cannot update UI: {ex.Message}, this should be uncritical.");
                            _log.Debug(ex.ToString());
                        }
                    }

                    Opacity = 100;
                }

            }
        }

        /// <summary>
        /// Add tag. Create if not exists
        /// </summary>
        /// <param name="s_id"></param>
        /// <param name="act"></param>
        public void AddTagAutoCreate(string s_id, TrX_TrackedActivity act)
        {
            int iIndex = GetTagIndex(s_id);
            TrX_ActivityTag tag;

            if (ValidateTagName(s_id))
            {
                if (iIndex < 0)
                {
                    tag = new TrX_ActivityTag(s_id, false)
                    {
                        BackColor = Color.White,
                        ForeColor = Color.Black
                    };
                    AddTag(tag);
                }
                else
                {
                    tag = _logic.Tags[iIndex];
                }

                if (!tag.IsDefault)
                {
                    act.AddTag(tag.ID);

                    string sTags = "";
                    // Update tags in DB // TODO
                    for (int i = 0; i < act.Tags.Count; i++)
                    {
                        sTags += act.Tags[i];
                        if (i < (act.Tags.Count - 1))
                            sTags += "|";
                    }
                    _logic.Database.DoNonQuery($"UPDATE tx_activity_log SET act_tags = '{sTags}' WHERE timestamp = {act.TimeStamp}");
                }
            }
        }

        /// <summary>
        /// Remove a tag from a given activity
        /// </summary>
        /// <param name="s_id"></param>
        /// <param name="act"></param>
        public void RemoveTagFromActivity(string s_id, TrX_TrackedActivity act)
        {
            TrX_ActivityTag tag = GetTagByID(s_id);
            if (tag != null && !tag.IsDefault)
            {
                act.RemoveTag(s_id);
                string sTags = "";

                // Update tags in DB // TODO
                for (int i = 0; i < act.Tags.Count; i++)
                {
                    sTags += act.Tags[i];
                    if (i < (act.Tags.Count - 1))
                        sTags += "|";
                    _logic.Database.DoNonQuery($"UPDATE tx_activity_log SET act_tags = '{sTags}' WHERE timestamp = {act.TimeStamp}");
                }
            }
        }

        /// <summary>
        /// Update a tag definition in UI and database
        /// </summary>
        /// <param name="s_id"></param>
        /// <param name="s_display_name"></param>
        /// <param name="s_forecolor"></param>
        /// <param name="s_backcolor"></param>
        /// <param name="b_show_in_hist"></param>
        private void UpdateTag(string s_id, string s_display_name, bool b_show_in_hist)
        {
            int iTagIndex = GetTagIndex(s_id);

            if (iTagIndex >= 0)
            {
                _logic.Tags[iTagIndex].DisplayName = s_display_name;
                _logic.Tags[iTagIndex].ShowInListView = b_show_in_hist;

                _logic.Database.DoNonQuery($"UPDATE tx_tags SET tag_display = '{s_display_name}', " +
                    $"tag_show_in_lv = {(b_show_in_hist ? "1" : "0")} WHERE tag_id = '{s_id}'");
            }

            RenderTagsForConfig(true);
            RenderTagsForTracking(true);
            ResetMapHistory();
            RequestHistoryUpdate();
        }

        /// <summary>
        /// Get index of a given tag in list
        /// </summary>
        /// <param name="s_id"></param>
        /// <returns></returns>
        private int GetTagIndex(string s_id)
        {
            for (int i = 0; i < _logic.Tags.Count; i++)
            {
                if (_logic.Tags[i].ID == s_id)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Check if a tag name is valid
        /// </summary>
        /// <param name="s_name"></param>
        /// <param name="b_showmessage"></param>
        /// <returns></returns>
        public bool ValidateTagName(string s_name, bool b_showmessage = false)
        {
            bool bValid = true;
            char[] invalid = new char[] { '=', ',', ';', ' ' };

            if (String.IsNullOrEmpty(s_name))
                bValid = false;

            foreach (char c in invalid)
            {
                if (s_name.Contains(c))
                {
                    bValid = false;
                }
            }

            if (bValid == false && b_showmessage)
            {
                MessageBox.Show("Sorry. this is not a valid tag ID!");
            }

            return bValid;
        }

        /// <summary>
        /// Delete a given tag from UI and database
        /// </summary>
        /// <param name="s_id"></param>
        private void DeleteTag(string s_id)
        {
            int iIndex = GetTagIndex(s_id);
            if (iIndex >= 0)
            {
                TrX_ActivityTag tag = _logic.Tags[iIndex];

                if (tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot delete a default tag!");
                }
                else
                {
                    DialogResult dr = MessageBox.Show($"Do you really want to delete the tag '{s_id}'?", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        _logic.Tags.RemoveAt(iIndex);
                        _logic.Database.DoNonQuery($"DELETE FROM tx_tags WHERE tag_id = '{s_id}' AND tag_type != 'default'");
                    }
                }
                RenderTagsForConfig(true);
                RenderTagsForTracking(true);
                ResetMapHistory();
                RequestHistoryUpdate();
            }
        }

        /// <summary>
        /// Delete a backup
        /// </summary>
        /// <param name="s_path"></param>
        private void DeleteBackup(string s_path)
        {
            Directory.Delete(s_path, true);
            _backups.Remove(listBoxRestoreBackup.SelectedItem.ToString());
        }

        /// <summary>
        /// Request update of history listview
        /// </summary>
        public void RequestHistoryUpdate()
        {
            _uiFlagActivityList = true;
        }

        /// <summary>
        /// Request updates for all Dashboards
        /// </summary>
        public void RequestDashboardUpdates()
        {
            _uiFlagBossDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagLabDashboard = true;
            _uiFlagAllStatsDashboard = true;
            _uiFlagMapDashboard = true;
            _uiFlagStatisticsChart = true;
            _uiFlagLeagueDashboard = true;
        }

        /// <summary>
        /// Excecute the search in activity list
        /// </summary>
        private void DoSearch()
        {
            if (textBox8.Text == String.Empty)
            {
                //_lvmActlog.Clear();
                _lvmActlog.FilterByRange(0, Convert.ToInt32(ReadSetting("actlog.maxitems", "500")));

            }
            else
            {
                _lvmActlog.ApplyFullTextFilter(textBox8.Text);
            }
            _actLogItemCount = _lvmActlog.CurrentItemCount;
        }

        /// <summary>
        /// Change current theme
        /// </summary>
        /// <param name="theme"></param>
        private void ChangeTheme(string theme)
        {
            if (theme == "Dark")
            {
                msm.Theme = MaterialSkinManager.Themes.DARK;
            }
            else
            {
                msm.Theme = MaterialSkinManager.Themes.LIGHT;
            }

            DoManualThemeAdjustments(this);

            AddUpdateAppSettings("theme", theme);
        }

        /// <summary>
        /// Pause the current activity
        /// </summary>
        public void PauseCurrentActivityOrSide()
        {
            if (_logic.CurrentActivity != null)
            {
                if (_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null)
                {
                    if (!_logic.CurrentActivity.SideArea_ZanaMap.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_ZanaMap.Pause();
                    }
                }
                else if (_logic.IsMapVaalArea && _logic.CurrentActivity.SideArea_VaalArea != null)
                {
                    if (!_logic.CurrentActivity.SideArea_VaalArea.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_VaalArea.Pause();
                    }
                }
                else if (_logic.IsMapSanctum && _logic.CurrentActivity.SideArea_Sanctum != null)
                {
                    if (!_logic.CurrentActivity.SideArea_Sanctum.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_Sanctum.Pause();
                    }
                }
                else if (_logic.IsMapAbyssArea && _logic.CurrentActivity.SideArea_AbyssArea != null)
                {
                    if (!_logic.CurrentActivity.SideArea_AbyssArea.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_AbyssArea.Pause();
                    }
                }
                else if (_logic.IsMapLabTrial && _logic.CurrentActivity.SideArea_LabTrial != null)
                {
                    if (!_logic.CurrentActivity.SideArea_LabTrial.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_LabTrial.Pause();
                    }
                }
                else if (_logic.IsMapLogbookSide && _logic.CurrentActivity.SideArea_LogbookSide != null)
                {
                    if (!_logic.CurrentActivity.SideArea_LogbookSide.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_LogbookSide.Pause();
                    }
                }
                else
                {
                    if (!_logic.CurrentActivity.ManuallyPaused)
                    {
                        _logic.CurrentActivity.Pause();
                    }
                }
            }
        }

        /// <summary>
        /// Resume current activity
        /// </summary>
        public void ResumeCurrentActivityOrSide()
        {
            if (_logic.CurrentActivity != null)
            {
                if (_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null)
                {
                    if (_logic.CurrentActivity.SideArea_ZanaMap.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_ZanaMap.Resume();
                    }
                }
                else if (_logic.IsMapVaalArea && _logic.CurrentActivity.SideArea_VaalArea != null)
                {
                    if (_logic.CurrentActivity.SideArea_VaalArea.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_VaalArea.Resume();
                    }
                }
                else if (_logic.IsMapAbyssArea && _logic.CurrentActivity.SideArea_AbyssArea != null)
                {
                    if (_logic.CurrentActivity.SideArea_AbyssArea.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_AbyssArea.Resume();
                    }
                }
                else if (_logic.IsMapLabTrial && _logic.CurrentActivity.SideArea_LabTrial != null)
                {
                    if (_logic.CurrentActivity.SideArea_LabTrial.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_LabTrial.Resume();
                    }
                }
                else if (_logic.IsMapLogbookSide && _logic.CurrentActivity.SideArea_LogbookSide != null)
                {
                    if (_logic.CurrentActivity.SideArea_LogbookSide.ManuallyPaused)
                    {
                        _logic.CurrentActivity.SideArea_LogbookSide.Resume();
                    }
                }
                else
                {
                    if (_logic.CurrentActivity.ManuallyPaused)
                    {
                        _logic.CurrentActivity.Resume();
                    }
                }
            }
        }

        private void SaveTimeCaps()
        {
            ACTIVITY_TYPES type;
            int value;
            string sett;

            sett = "TimeCapMinimum";
            int minTime = Convert.ToInt32(textBox11.Text);

            _mySettings.AddOrUpdateSetting(sett, minTime.ToString());

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                type = (ACTIVITY_TYPES)Enum.Parse(typeof(ACTIVITY_TYPES), row.Cells[0].Value.ToString());
                value = Convert.ToInt32(row.Cells[1].Value);
                sett = $"TimeCap{TrX_Helpers.CapitalFirstLetter(type.ToString())}";
                _timeCaps[type] = value;
                _mySettings.AddOrUpdateSetting(sett, value.ToString());
            }

            _mySettings.WriteToXml();
        }


        private void ActivateSimpleStopWatchOverlay()
        {
            try
            {
                _stopwatchOverlay.Show();
            }
            catch (ObjectDisposedException)
            {
                _stopwatchOverlay = new OverlaySimpleStopwatch() { MainWindow = this, ID = "stopwatch" };
            }

            _stopwatchOverlay.TopMost = true;
            _stopwatchOverlay.Opacity = _overlayOpacity / 100.0;
            _stopwatchOverlay.Location = new Point(Convert.ToInt32(ReadSetting("overlay.stopwatch.x", "0")), (Convert.ToInt32(ReadSetting("overlay.stopwatch.y", "0"))));
        }

        private void ActivateTagOverlay()
        {
            try
            {
                _tagsOverlay.Show();
            }
            catch (ObjectDisposedException)
            {
                _tagsOverlay = new OverlayTags() { MainWindow = this, ID = "tags" };
            }

            _tagsOverlay.TopMost = true;
            _tagsOverlay.Opacity = _overlayOpacity / 100.0;
            _tagsOverlay.Location = new Point(Convert.ToInt32(ReadSetting("overlay.tags.x", "0")), (Convert.ToInt32(ReadSetting("overlay.tags.y", "0"))));
        }

        private void DeleteActivities()
        {
            List<long> timestampsToDelete = new List<long>();
            List<string> lvItemsToDelete = new List<string>();

            foreach (ListViewItem lvi in _lvmActlog.listView.SelectedItems)
            {
                lvItemsToDelete.Add(lvi.Name);
                timestampsToDelete.Add(_logic.ActivityHistory[FindEventLogIndexByID(lvi.Name)].TimeStamp);
            }

            string msg;
            msg = $"Do you really want to delete {lvItemsToDelete.Count} activitie(s)?";

            if (MessageBox.Show(msg, "Delete?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            foreach (string itemName in lvItemsToDelete)
            {
                _lvmActlog.RemoveItemByName(itemName);
            }

            foreach (long ts in timestampsToDelete)
            {
                DeleteActLogEntry(ts);
            }
        }

        private void MainW_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveLayout();
            Exit();
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Exit();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new AboutForm());
        }

        private void pictureBox14_Click_1(object sender, EventArgs e)
        {
            if (_logic.CurrentActivity != null)
                _logic.CurrentActivity.Resume();
        }

        private void pictureBox13_Click_1(object sender, EventArgs e)
        {
            if (_logic.CurrentActivity != null)
                _logic.CurrentActivity.Pause();
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            ResumeCurrentActivityOrSide();
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            PauseCurrentActivityOrSide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listViewActLog.SelectedIndices.Count > 0)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                TrX_TrackedActivity act = GetActivityFromListItemName(listViewActLog.Items[iIndex].Name);
                if (act != null)
                    OpenActivityDetails(act);
            }
        }

        private void OpenActivityDetails()
        {
            if (listViewActLog.SelectedIndices.Count > 0)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                TrX_TrackedActivity act = GetActivityFromListItemName(listViewActLog.Items[iIndex].Name);
                if (act != null)
                    OpenActivityDetails(act);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenActivityDetails();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("With this action, the statistics will be set to 0 without reloading the log. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                ResetStats();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("WARNING: this action will reload the entire Client.txt. Manual data like paused activities or custom-tags on activities will be lost.");
            sb.AppendLine("Continue? TraXile will be restarted.");

            DialogResult dr = MessageBox.Show(sb.ToString(), "Warning", MessageBoxButtons.YesNo); ;
            if (dr == DialogResult.Yes)
            {
                InitLogFileReload();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("For this action, the application needs to be restarted. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                DialogResult dr2 = ofd.ShowDialog();
                if (dr2 == DialogResult.OK)
                {
                    AddUpdateAppSettings("poe_logfile_path", ofd.FileName);
                    InitLogFileReload();
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ResetMapHistory();
            RequestHistoryUpdate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _showGridInActLog = checkBoxShowGridInAct.Checked;
            AddUpdateAppSettings("ActivityLogShowGrid", checkBoxShowGridInAct.Checked.ToString());
            listViewActLog.GridLines = _showGridInActLog;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            _showGridInStats = checkBoxShowGridInStats.Checked;
            AddUpdateAppSettings("StatsShowGrid", checkBoxShowGridInStats.Checked.ToString());
        }

        private void button15_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("With this action your Path of Exile log will be flushed and all data and statistics in TraXile will be deleted." + Environment.NewLine
                + Environment.NewLine + "It is recommendet to create a backup first - using the 'Create Backup' function. Do you want to create a backup before reset?", "Warning", MessageBoxButtons.YesNoCancel);

            if (dr == DialogResult.Yes)
                CreateBackup("Auto_Backup");

            if (dr != DialogResult.Cancel)
            {
                DoFullReset();
                MessageBox.Show("Reset successful! The Application will be restarted now.");
                Application.Restart();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxBackupName.Text == "")
                {
                    textBoxBackupName.Text = "Default";
                }

                if (textBoxBackupName.Text.Contains("/") || textBoxBackupName.Text.Contains("."))
                {
                    MessageBox.Show("Please do not define a path in the field name");
                }
                else
                {
                    CreateBackup(textBoxBackupName.Text);
                    MessageBox.Show("Backup successfully created!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            DialogResult dr;

            if (Process.GetProcessesByName("PathOfExileSteam").Length > 0 ||
                Process.GetProcessesByName("PathOfExile").Length > 0)
            {
                MessageBox.Show("It seems that PathOfExile is running at the moment. Please close it first.");
            }
            else
            {
                dr = MessageBox.Show("Do you really want to restore the selected Backup? The Application will be restarted. Please make sure that your PathOfExile Client is not running.", "Warning", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    PrepareBackupRestore(TrX_Static.APPDATA_PATH + listBoxRestoreBackup.SelectedItem.ToString());
                }
            }
        }

        private void panelTags_SizeChanged(object sender, EventArgs e)
        {
            if (_logic != null && _logic.EventQueueInitialized)
                RenderTagsForTracking(true);
        }

        private void panelEditTags_SizeChanged(object sender, EventArgs e)
        {
            if (_logic != null && _logic.EventQueueInitialized)
                RenderTagsForConfig(true);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (ValidateTagName(textBox2.Text, true))
            {
                if (!CheckTagExists(textBox2.Text))
                {
                    AddTag(new TrX_ActivityTag(textBox2.Text, false) { DisplayName = textBox3.Text });
                    RenderTagsForConfig(true);
                    RenderTagsForTracking(true);
                    textBox2.Clear();
                }
                else
                {
                    MessageBox.Show("Tag '" + textBox2.Text + "' already exists.");
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = textBox2.Text;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            UpdateTag(textBox4.Text, textBox5.Text, checkBox4.Checked);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox5.Text = "";
        }

        private void button19_Click(object sender, EventArgs e)
        {
            DeleteTag(textBox4.Text);
            textBox4.Text = "";
            textBox5.Text = "";
        }

        private void button18_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you really want to delete the selected Backup?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                DeleteBackup(TrX_Static.APPDATA_PATH + listBoxRestoreBackup.SelectedItem.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            textBox5.Text = textBox4.Text;
        }

        private void chatCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new ChatCommandHelp(), true);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void contextMenuStrip1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void chatCommandsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new ChatCommandHelp(), true);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenChildWindow(new SearchHelp());
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox8.Text = "";
            _uiFlagActivityList = true;
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdate(true);
        }

        private void infoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new AboutForm());
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new AboutForm(), true);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Your current Client.txt will be renamed and a new one will be created. " + Environment.NewLine
                + "The renamed version can be deleted or backed up afterwards. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                try
                {
                    string sDt = DateTime.Now.ToString("yyyy-MM-dd-H-m-s");
                    string sBaseDir = new FileInfo(_logic.ClientTxtPath).DirectoryName;
                    File.Copy(_logic.ClientTxtPath, sBaseDir + @"\Client." + sDt + ".txt");
                    FileStream fs1 = new FileStream(_logic.ClientTxtPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    MessageBox.Show("Client.txt rolled and cleared successful. The Application will be restarted now.");
                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void wikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(TrX_Static.WIKI_URL);
        }

        private void listViewActLog_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            SaveLayout();
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AddUpdateAppSettings("actlog.maxitems", comboBoxShowMaxItems.SelectedItem.ToString());

            MethodInvoker mi = delegate { DoSearch(); };
            BeginInvoke(mi);
        }

        private void tabPage1_Enter(object sender, EventArgs e)
        {
            RenderMappingDashboard();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            _labDashboardHideUnknown = ((CheckBox)sender).Checked;
            AddUpdateAppSettings("dashboard_lab_hide_unknown", _labDashboardHideUnknown.ToString());
            RenderLabDashboard();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                SaveTimeCaps();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Process.Start(TrX_Static.WIKI_URL_SETTINGS);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ChangeTheme(comboBoxTheme.SelectedItem.ToString());
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            AddUpdateAppSettings("pie_chart_show_hideout", checkBox1.Checked.ToString());
            _showHideoutInPie = checkBox1.Checked;
            RenderGlobalDashboard();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("WARNING: this action will reload the entire Client.txt, which results in losing metadata like tags and pause times.");
            sb.AppendLine("Continue? TraXile will be restarted.");

            DialogResult dr = MessageBox.Show(sb.ToString(), "Warning", MessageBoxButtons.YesNo); ;
            if (dr == DialogResult.Yes)
            {
                ReloadLogFile();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_logic.EventQueueInitialized)
            {
                if (comboBox_Filter_TimeRange.SelectedItem.ToString() == "Custom")
                {
                    dateTimePicker1.Enabled = true;
                    dateTimePicker2.Enabled = true;
                    textBox12.Enabled = true;
                    textBox13.Enabled = true;
                }
                else if (comboBox_Filter_TimeRange.SelectedItem.ToString().Contains("League:"))
                {
                    string sLeague = comboBox_Filter_TimeRange.SelectedItem.ToString().Split(new string[] { "League: " }, StringSplitOptions.None)[1]
                        .Split(new string[] { " (" }, StringSplitOptions.None)[0];
                    TrX_LeagueInfo li = GetLeagueByName(sLeague);

                    DateTime dt1 = li.Start;
                    DateTime dt2 = li.End;

                    if (comboBox_Filter_TimeRange.SelectedItem.ToString() != "Custom")
                    {
                        dateTimePicker1.Value = dt1;
                        dateTimePicker2.Value = dt2;
                        textBox12.Text = dt1.TimeOfDay.ToString();
                        textBox13.Text = dt2.TimeOfDay.ToString();
                    }
                }
                else
                {
                    dateTimePicker1.Enabled = false;
                    dateTimePicker2.Enabled = false;
                    textBox12.Enabled = false;
                    textBox13.Enabled = false;
                }
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            RequestDashboardUpdates();
        }

        private void listViewNF1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewNF1.SelectedIndices.Count > 0)
            {
                _allStatsSelected = listViewNF1.Items[listViewNF1.SelectedIndices[0]].Name;
                UpdateAllStatsChart();
            }
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Clear();
            _allStatsSearchActive = false;
            listViewNF1.VirtualListSize = _dataSourceAllStats.Count;
        }

        private void checkBoxLabHideUnknown_CheckedChanged(object sender, EventArgs e)
        {
            _labDashboardHideUnknown = ((CheckBox)sender).Checked;
            AddUpdateAppSettings("dashboard_lab_hide_unknown", _labDashboardHideUnknown.ToString());
            RenderLabDashboard();
        }

        private void stopwatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OverlayTags tags = new OverlayTags();
            tags.MainWindow = this;
            tags.Height = 31;
            tags.Show();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (_stopwatchOverlay != null)
            {
                _stopwatchOverlay.Opacity = trackBar1.Value > 0 ? (trackBar1.Value / 100.0) : 0;
                _tagsOverlay.Opacity = trackBar1.Value > 0 ? (trackBar1.Value / 100.0) : 0;
            }
            _overlayOpacity = trackBar1.Value;
            label38.Text = _overlayOpacity.ToString() + "%";
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.default", checkBox5.Checked.ToString());
            _stopwatchOverlayShowDefault = checkBox5.Checked;
        }

        private void stopwatchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_stopwatchOverlay.Visible)
            {
                _stopwatchOverlay.Hide();
            }
            else
            {
                ActivateSimpleStopWatchOverlay();
            }

            tagOverlayToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
        }

        private void pictureBoxStop_Click(object sender, EventArgs e)
        {
            if (_logic.CurrentActivity != null)
            {
                _logic.FinishActivity(_logic.CurrentActivity, null, ACTIVITY_TYPES.MAP, new DateTime());
            }
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            lbl_filter.Visible = true;
            pictureBox32.Visible = true;
            lbl_filter.Text = "Your data is filtered!";

            RequestDashboardUpdates();
            RequestActivityListReset();
        }

        private void RequestActivityListReset()
        {
            _uiFlagActivityListReset = true;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Request update
            _uiFlagMapDashboard = true;
        }

        bool ExistsInMaterialListBox(MaterialListBox listBox, string searchText)
        {
            foreach (MaterialListBoxItem item in listBox.Items)
            {
                if (item.Text.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void button17_Click_2(object sender, EventArgs e)
        {
            if (comboBox_Filter_Tags.SelectedItem != null)
            {
                if (!ExistsInMaterialListBox(listBox_Filter_Tags, comboBox_Filter_Tags.SelectedText))
                {
                    MaterialListBoxItem item = new MaterialListBoxItem(comboBox_Filter_Tags.SelectedItem.ToString());
                    listBox_Filter_Tags.Items.Add(item);
                }
                comboBox_Filter_Tags.Text = "";
            }
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            if (listBox_Filter_Tags.SelectedItem != null)
            {
                listBox_Filter_Tags.Items.Remove(listBox_Filter_Tags.SelectedItem);
            }
        }

        private void comboBoxStopWatchTag1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.tag1", comboBoxStopWatchTag1.SelectedItem.ToString());
            _overlayTag1 = comboBoxStopWatchTag1.SelectedItem.ToString();
        }

        /// <summary>
        /// Build Summary for selected activities
        /// </summary>
        private void BuildAndShowSummary()
        {
            UI.SummaryForm summary = new UI.SummaryForm();
            DoManualThemeAdjustments(summary);

            double totalSeconds = 0;
            double avgDuration = 0;
            int count = 0;

            Dictionary<string, int> dictLabelCount = new Dictionary<string, int>();
            Dictionary<string, int> dictTypeCount = new Dictionary<string, int>();
            Dictionary<string, double> dictTypeDuration = new Dictionary<string, double>();

            if (_lvmActlog.listView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lvi in _lvmActlog.listView.SelectedItems)
                {
                    TrX_TrackedActivity activity = _logic.ActivityHistory[FindEventLogIndexByID(lvi.Name)];
                    totalSeconds += (activity.TotalSeconds > TimeCaps[activity.Type] ? TimeCaps[activity.Type] : activity.TotalSeconds);

                    if (dictTypeCount.ContainsKey(activity.Type.ToString()))
                    {
                        dictTypeCount[activity.Type.ToString()]++;
                    }
                    else
                    {
                        dictTypeCount.Add(activity.Type.ToString(), 1);
                    }

                    if (dictTypeDuration.ContainsKey(activity.Type.ToString()))
                    {
                        dictTypeDuration[activity.Type.ToString()] += (activity.TotalSeconds > TimeCaps[activity.Type] ? TimeCaps[activity.Type] : activity.TotalSeconds);
                    }
                    else
                    {
                        dictTypeDuration.Add(activity.Type.ToString(), (activity.TotalSeconds > TimeCaps[activity.Type] ? TimeCaps[activity.Type] : activity.TotalSeconds));
                    }

                    foreach (string tag in activity.Tags)
                    {
                        if (!string.IsNullOrEmpty(tag))
                        {
                            if (dictLabelCount.ContainsKey(tag))
                            {
                                dictLabelCount[tag]++;
                            }
                            else
                            {
                                dictLabelCount.Add(tag, 1);
                            }
                        }
                    }

                    count++;
                }

                avgDuration = Math.Round(totalSeconds / count, 0);

                TimeSpan totalDurationTS = TimeSpan.FromSeconds(totalSeconds);
                TimeSpan avgDurationTS = TimeSpan.FromSeconds(avgDuration);

                summary.CountLabel.Text = count.ToString();

                // print long version for durations over one day
                if (totalDurationTS.TotalDays >= 1)
                {
                    summary.DurationLabel.Text = $"{totalDurationTS.Days}d {totalDurationTS.Hours}h {totalDurationTS.Minutes}m {totalDurationTS.Seconds}s";
                }
                else
                {
                    summary.DurationLabel.Text = totalDurationTS.ToString();
                }

                summary.AverageLabel.Text = avgDurationTS.ToString();

                // Tags
                RenderTagsForSummary(summary, dictLabelCount, _lvmActlog.listView.SelectedItems.Count, false);

                // Types
                foreach (KeyValuePair<string, int> kvp in dictTypeCount)
                {
                    TimeSpan tsDuration = TimeSpan.FromSeconds(dictTypeDuration[kvp.Key]);
                    TimeSpan tsAvg = TimeSpan.FromSeconds(Math.Round(tsDuration.TotalSeconds / dictTypeCount[kvp.Key], 0));

                    ListViewItem listViewItem = new ListViewItem(kvp.Key);
                    listViewItem.SubItems.Add(kvp.Value.ToString());
                    listViewItem.SubItems.Add(tsDuration.ToString());
                    listViewItem.SubItems.Add(tsAvg.ToString());
                    summary.ListViewTypes.Items.Add(listViewItem);
                }

            }
            else
            {
                summary.CountLabel.Text = "You did not select any activities.";
            }

            OpenChildWindow(summary);
        }

        private void listViewNF1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            Dictionary<string, int> source = _allStatsSearchActive ? _filteredDataSourceAllStats : _dataSourceAllStats;

            if (source.Count > 0)
            {
                string longName = GetStatLongName(source.ElementAt(e.ItemIndex).Key);
                e.Item = new ListViewItem(longName);
                e.Item.SubItems.Add(source.ElementAt(e.ItemIndex).Value.ToString());
                e.Item.Name = source.ElementAt(e.ItemIndex).Key;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _allStatsSearchActive = !String.IsNullOrEmpty(textBox1.Text);

            if (_allStatsSearchActive)
            {
                _filteredDataSourceAllStats.Clear();
                foreach (KeyValuePair<string, int> kvp in _dataSourceAllStats)
                {
                    string statName = GetStatLongName(kvp.Key);
                    if (statName.ToLower().Contains(textBox1.Text.ToLower()))
                    {
                        _filteredDataSourceAllStats.Add(kvp.Key, kvp.Value);
                    }
                }

                listViewNF1.VirtualListSize = _filteredDataSourceAllStats.Count;
            }
            else
            {
                listViewNF1.VirtualListSize = _dataSourceAllStats.Count;
            }
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {
            _minimizeToTray = checkBoxMinimizeToTray.Checked;
            AddUpdateAppSettings("MinimizeToTray", _minimizeToTray.ToString());
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void SetMaximizedBounds()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.MaximizedBounds = new Rectangle(workingArea.X + 2, workingArea.Y + 5, workingArea.Width, workingArea.Height - 5);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            SetMaximizedBounds();

            if (this.WindowState == FormWindowState.Minimized)
            {
                if (_minimizeToTray)
                {
                    Hide();
                    this.ShowInTaskbar = false;
                }
            }
            label3.Location = new Point((this.Width - label3.Width) / 2, label3.Location.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RenderLeagueDashboard();
        }

        private void SaveOverlaySettings()
        {
            AddUpdateAppSettings("overlay.general.opacity", trackBar1.Value.ToString());
            AddUpdateAppSettings("overlay.stopwatch.showDefault", checkBox5.Checked.ToString());
            AddUpdateAppSettings("overlay.tags.showDefault", checkBox3.Checked.ToString());
            AddUpdateAppSettings("overlay.tags.tag1", comboBoxStopWatchTag1.SelectedItem.ToString());
            AddUpdateAppSettings("overlay.tags.tag2", comboBoxStopWatchTag2.SelectedItem.ToString());
            AddUpdateAppSettings("overlay.tags.tag3", comboBoxStopWatchTag3.SelectedItem.ToString());

            _overlayTag1 = comboBoxStopWatchTag1.SelectedItem.ToString();
            _overlayTag2 = comboBoxStopWatchTag2.SelectedItem.ToString();
            _overlayTag3 = comboBoxStopWatchTag3.SelectedItem.ToString();

            _uiFlagTagOverlay_TagsChanged = true;
        }

        private void stopwatchsimpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_stopwatchOverlay.Visible)
            {
                _stopwatchOverlay.Hide();
            }
            else
            {
                ActivateSimpleStopWatchOverlay();
            }

            stopwatchsimpleToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
            stopwatchToolStripMenuItem1.Checked = _stopwatchOverlay.Visible;
        }

        private void tagOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tagsOverlay.Visible)
            {
                _tagsOverlay.Hide();
            }
            else
            {
                ActivateTagOverlay();
            }

            tagOverlayToolStripMenuItem.Checked = _tagsOverlay.Visible;
            tagsToolStripMenuItemTagOverlay.Checked = _tagsOverlay.Visible;
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportActvityList exportChildWindow = new ExportActvityList(this, "csv");
            OpenChildWindow(exportChildWindow);
        }

        private void jSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportActvityList exportChildWindow = new ExportActvityList(this, "json");
            OpenChildWindow(exportChildWindow);
        }

        private void timerUpdateCheck_Tick(object sender, EventArgs e)
        {
            CheckForUpdate(false, true);
        }

        private void pictureBoxUpdateAvailable_Click(object sender, EventArgs e)
        {
            CheckForUpdate(true, false);
        }

        private void materialLabelSummary_Click(object sender, EventArgs e)
        {
            BuildAndShowSummary();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            _allStatsSearchActive = false;
            listViewNF1.VirtualListSize = _dataSourceAllStats.Count;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChangeTheme(comboBoxTheme.SelectedItem.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveOverlaySettings();
        }

        private void materialLabel2_Click(object sender, EventArgs e)
        {
            DeleteActivities();
        }

        private void buttonStartSearch_Click(object sender, EventArgs e)
        {
            DoSearch();
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            textBox8.Text = "";
            DoSearch();
        }

        private void ToggleFilters()
        {
            if (filterBarShown)
            {
                tableLayoutPanel_L0.RowStyles[1].Height = 16;
                linkLabel5.Text = "show filters";
                filterBarShown = false;
            }
            else
            {
                tableLayoutPanel_L0.RowStyles[1].Height = 160;
                linkLabel5.Text = "hide filters";
                filterBarShown = true;
            }
        }

        private void linkLabel5_LinkClicked(object sender, EventArgs e)
        {
            ToggleFilters();
        }

        private void pictureBox30_Click(object sender, EventArgs e)
        {
            ToggleFilters();
        }

        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Main_Shown(object sender, EventArgs e)
        {
            LoadLayout();
            label3.Location = new Point((this.Width - label3.Width) / 2, label3.Location.Y);
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            ResumeCurrentActivityOrSide();
        }

        private void pictureBoxPause_Click(object sender, EventArgs e)
        {
            PauseCurrentActivityOrSide();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                _splitterBackFromMinimizedWinddow = true;
            }

            if (this.WindowState == FormWindowState.Normal)
            {
                if (_splitterBackFromMinimizedWinddow)
                {
                    _splitterBackFromMinimizedWinddow = false;
                }
                else
                {
                    RenderTagsForTracking(true);
                }
            }
        }

        private void materialLabel49_Click(object sender, EventArgs e)
        {
            OpenActivityDetails();
        }

        private void linkLabelUpdateAvailable_Click(object sender, EventArgs e)
        {
            CheckForUpdate(false, false);
        }

        private void Main_Move(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.MaximizedBounds = new Rectangle(workingArea.X + 2, workingArea.Y + 5, workingArea.Width, workingArea.Height - 5);
        }

        private void comboBoxStopWatchTag2_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.tag2", comboBoxStopWatchTag2.SelectedItem.ToString());
            _overlayTag2 = comboBoxStopWatchTag2.SelectedItem.ToString();
        }

        private void comboBoxStopWatchTag3_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.tag3", comboBoxStopWatchTag3.SelectedItem.ToString());
            _overlayTag3 = comboBoxStopWatchTag3.SelectedItem.ToString();
        }

        private void button5_Click_2(object sender, EventArgs e)
        {
            ResetFilter(true);
            lbl_filter.Visible = false;
            pictureBox32.Visible = false;
        }

    }
}