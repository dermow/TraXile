using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
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
    }

    /// <summary>
    /// Main UI
    /// </summary>
    public partial class Main : Form
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

        // UI Update Flag: Lab enchants
        private bool _uiFlagEnchants;

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
        private StopWatchOverlay _stopwatchOverlay;

        // List of available backups
        private BindingList<string> _backups;

        // Mapping of tags to labels
        private Dictionary<string, Label> _tagLabels, _tagLabelsConfig;

        // Settings manager
        private readonly TrX_SettingsManager _mySettings;

        // Listview Manager: Activity log
        private TrX_ListViewManager _lvmActlog;
        
        // ListView Manager: All stats
        private TrX_ListViewManager _lvmAllStats;

        // Current theme
        private TrX_Theme _myTheme;

        // List of League Info objects
        private List<TrX_LeagueInfo> _leagues;

        // Logger
        private ILog _log;

        // Wether or not to show hideout time in pie chart
        private bool _showHideoutInPie;

        // Visibility of the stopwatch overlay windows
        private int _stopwatchOverlayOpacity;

        // Show stopwatch overlay by default?
        private bool _stopwatchOverlayShowDefault;
        
        // Default mappings
        private TrX_DefaultMappings _defaultMappings;

        // Time cap parameters for each activity type
        private Dictionary<ACTIVITY_TYPES, int> _timeCaps;
        private bool _exitting;

        // Property: Core logic to be accessible
        public TrX_CoreLogic Logic => _logic;

        // Property: Timecaps to be accessible
        public Dictionary<ACTIVITY_TYPES, int> TimeCaps => _timeCaps;

        // User Controls labruns
        private List<UI.UserControlLabRun> _labRunControls;

        // Current labrun user control
        private UI.UserControlLabRun _currentLabrunControl;

        // Enchant info ID
        private int _selectedEnchantID = 0;

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

        /// <summary>
        /// Main Window Constructor
        /// </summary>
        public Main()
        {
            Thread.CurrentThread.Name = "MainThread";

            // Initialize Settings
            _mySettings = new TrX_SettingsManager(TrX_AppInfo.CONFIG_PATH);

            // Create Appdata if not existing
            if (!Directory.Exists(TrX_AppInfo.APPDATA_PATH))
            {
                Directory.CreateDirectory(TrX_AppInfo.APPDATA_PATH);
            }

            // Create Metadata path if not existing
            if (!Directory.Exists(TrX_AppInfo.APPDATA_PATH + @"\metadata"))
            {
                Directory.CreateDirectory(TrX_AppInfo.APPDATA_PATH + @"\metadata");
            }

            // Invisible till initialization complete
            Visible = false;
            

            InitializeComponent();
            Init();

            // Set Theme
            if (ReadSetting("theme", "Dark") == "Light")
            {
                _myTheme = new TrX_ThemeLight();
                _myTheme.Apply(this);
            }
            else
            {
                _myTheme = new TrX_ThemeDark();
                _myTheme.Apply(this);
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
                if(act.Type == type)
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

            switch(mode)
            {
                case "OR":
                    foreach(TrX_TrackedActivity act in source)
                    {
                        foreach(string tag in tags)
                        {
                            if(act.HasTag(tag))
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

                        if(filterResult)
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

            DateTime date1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 0, 0, 0, _dateTimeFormatInfo.Calendar);
            DateTime date2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, 23, 59, 59, _dateTimeFormatInfo.Calendar);

            _statsDate1 = date1;
            _statsDate2 = date2;

            foreach (TrX_TrackedActivity act in source)
            {
                if (act.Started >= date1 && act.Started <= date2)
                {
                    if(act.TotalSeconds > _minimumTimeCap)
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
            comboBox1.SelectedItem = "All";
            comboBox7.SelectedItem = "All";
            comboBox8.SelectedItem = "All";
            comboBox4.SelectedItem = "OR";
            comboBox9.SelectedItem = "=";
            textBox10.Text = "";
            listBox3.Items.Clear();

            if(set)
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
            if (comboBox1.SelectedItem == null || _statsDataSource == null)
                return;

            if(comboBox7.SelectedItem == null)
            {
                comboBox7.SelectedItem = "All";
            }

            if (comboBox8.SelectedItem == null)
            {
                comboBox8.SelectedItem = "All";
            }

            _statsDataSource.Clear();

            if (comboBox1.SelectedItem.ToString() == "All")
            {
                _statsDataSource.AddRange(_logic.ActivityHistory);
                _statsDate1 = DateTimeOffset.FromUnixTimeSeconds(_oldestTimeStamp).DateTime;
                _statsDate2 = DateTime.Now;
                dateTimePicker1.Value = _statsDate1;
                dateTimePicker2.Value = _statsDate2;
            }
            else if (comboBox1.SelectedItem.ToString().Contains("League:"))
            {
                string sLeague = comboBox1.SelectedItem.ToString().Split(new string[] { "League: " }, StringSplitOptions.None)[1]
                    .Split(new string[] { " (" }, StringSplitOptions.None)[0];
                TrX_LeagueInfo li = GetLeagueByName(sLeague);

                DateTime dt1 = li.Start;
                DateTime dt2 = li.End;

                _statsDataSource = FilterActivitiesByTimeRange(dt1, dt2, _logic.ActivityHistory);

                if (comboBox1.SelectedItem.ToString() != "Custom")
                {
                    dateTimePicker1.Value = dt1;
                    dateTimePicker2.Value = dt2;
                }
            }
            else
            {
                DateTime date1 = DateTime.Now;
                DateTime date2 = DateTime.Now;

                switch (comboBox1.SelectedItem.ToString())
                {
                    case "Custom":
                        date1 = dateTimePicker1.Value;
                        date2 = dateTimePicker2.Value;
                        break;
                    case "Today":
                        date1 = DateTime.Now;
                        date2 = DateTime.Now;
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

                if (comboBox1.SelectedItem.ToString() != "Custom")
                {
                    dateTimePicker1.Value = date1;
                    dateTimePicker2.Value = date2;
                }
            }

            // Apply Type filter
            if(comboBox7.SelectedItem.ToString() != "All")
            {
                _statsDataSource = FilterActivitiesByType((ACTIVITY_TYPES)Enum.Parse(typeof(ACTIVITY_TYPES), comboBox7.SelectedItem.ToString()), _statsDataSource);
            }

            // Apply Area filter
            if (comboBox8.SelectedItem.ToString() != "All")
            {
                _statsDataSource = FilterActivitiesByArea(comboBox8.SelectedItem.ToString(), _statsDataSource);
            }

            // Apply Area filter
            if (!string.IsNullOrEmpty(textBox10.Text))
            {
                try
                {
                    int lvl = Convert.ToInt32(textBox10.Text);
                    _statsDataSource = FilterActivitiesByAreaLevel(lvl, comboBox9.SelectedItem.ToString(), _statsDataSource);
                }
                catch(Exception ex)
                {
                    textBox10.Text = String.Empty;
                    MessageBox.Show(ex.Message);
                }
            }

            // Apply tag filter
            if (listBox3.Items.Count > 0)
            {
                List<string> src = new List<string>();
                foreach (string s in listBox3.Items)
                {
                    src.Add(s);
                }

                _statsDataSource = FilterActivitiesByTags(src, comboBox4.SelectedItem.ToString(), _statsDataSource);
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
                string updateBranch = ReadSetting("metadata_meta_branch", "main");
                string updateURL = "https://raw.githubusercontent.com/dermow/traxile-metadata/" + updateBranch + "/metadata.xml";

                WebClient webClient = new WebClient();
                Uri uri = new Uri(updateURL);
                string data = webClient.DownloadString(uri);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);
                xml.Save(TrX_AppInfo.APPDATA_PATH + @"\metadata\metadata.xml");

                _log.Info("Metadata successfully updated from '" + updateURL + "'");
                
            }
            catch (Exception ex)
            {
                _log.Error("Could not update Metadata: " + ex.Message);
            }
        }

        /// <summary>
        /// Check if a new version is available on GitHub and ask for update.
        /// </summary>
        /// <param name="b_notify_ok"></param>
        private void CheckForUpdate(bool b_notify_ok = false)
        {
            try
            {
                string updateURL, updateBranch; 

                updateBranch = ReadSetting("metadata_updates_branch", "main");
                updateURL = "https://raw.githubusercontent.com/dermow/traxile-metadata/" + updateBranch + "/versions.xml";

                WebClient webClient = new WebClient();
                Uri uri = new Uri(updateURL);
                string releases = webClient.DownloadString(uri);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(releases);

                string sVersion;
                sVersion = xml.SelectSingleNode(string.Format("/version/{0}", "latest")).InnerText;

                StringBuilder sbChanges = new StringBuilder();

                foreach (XmlNode xn in xml.SelectNodes(string.Format("/version/changelog/chg[@version='{0}']", sVersion)))
                {
                    sbChanges.AppendLine(" - " + xn.InnerText);
                }

                _log.Info(string.Format("UpdateCheck -> My version: {0}, Remote version: {1}", TrX_AppInfo.VERSION, sVersion));

                int myMajor = Assembly.GetExecutingAssembly().GetName().Version.Major;
                int myMinor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
                int myBuild = Assembly.GetExecutingAssembly().GetName().Version.Build;

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
                    StringBuilder sbMessage = new StringBuilder();
                    sbMessage.AppendLine(string.Format("There is a new version for TraXile available ({0} => {1})", TrX_AppInfo.VERSION, sVersion));
                    sbMessage.AppendLine();
                    sbMessage.AppendLine(string.Format("Changelog: ", sVersion));
                    sbMessage.AppendLine("===========");
                    sbMessage.AppendLine(sbChanges.ToString());
                    sbMessage.AppendLine();
                    sbMessage.AppendLine("Do you want to update now?");

                    if (MessageBox.Show(sbMessage.ToString(), "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            Arguments = sVersion,
                            FileName = Application.StartupPath + @"\TraXile.Updater.exe"
                        };
                        Process.Start(psi).WaitForExit();
                    }
                }
                else
                {
                    _log.Info("UpdateCheck -> Already up to date :)");
                    if (b_notify_ok)
                        MessageBox.Show(
                            "================="
                            + Environment.NewLine + "Your version: " + TrX_AppInfo.VERSION
                            + Environment.NewLine + "Latest version: " + sVersion + Environment.NewLine
                            + "================="  + Environment.NewLine + Environment.NewLine
                            + "Your version is already up to date :)");
                }
            }
            catch (Exception ex)
            {
                _log.Error("Could not check for Update: " + ex.Message);
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
            _mySettings.LoadFromXml();

            _overlayTag1 = null;
            _overlayTag2 = null;
            _overlayTag3 = null;

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
                _log.Error("FailedRestore -> " + ex.Message);
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
            //_logic.MinimumCap = _minimumTimeCap;

            // Fixing the DateTimeFormatInfo to Gregorian Calendar, to avoid wrong timestamps with other calendars
            _dateTimeFormatInfo = DateTimeFormatInfo.GetInstance(new CultureInfo("en-CA"));
            _dateTimeFormatInfo.Calendar = new GregorianCalendar();
            _defaultMappings = new TrX_DefaultMappings();
            _loadScreenWindow = new LoadScreen();


            SaveVersion();
            DownloadMetaData();
            CheckForUpdate();
            _UpdateCheckDone = true;
            

            _logic.OnHistoryInitialized += Logic_OnHistoryInitialized;
            _logic.OnActivityFinished += Logic_OnActivityFinished;
            _logic.OnTagsUpdated += Logic_OnTagsUpdated;
            _logic.LabEnchantsReceived += _logic_LabEnchantsReceived;
            _logic.OnActivityStarted += _logic_OnActivityStarted;
            _logic.LabbieConnector.LabbieLogPath = ReadSetting("labbie.path", null);
            _logic.Start();

            // Data Sources
            _dataSourceAllStats = new Dictionary<string, int>();
            _filteredDataSourceAllStats = new Dictionary<string, int>();

            // Init profit tracker
            _profitTracking = new TrX_ProfitTracking(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml");
            _profitTracking.DataFilePath = TrX_AppInfo.APPDATA_PATH + @"\labdata.xml";
            _profitBinding = new BindingSource();
            _profitBinding.DataSource = _profitTracking.Data;
            
            dataGridView2.DataSource = _profitBinding.DataSource;
            dataGridView2.ForeColor = Color.Black;
            dataGridView2.Columns[5].ReadOnly = true;

            UpdateProfitSummary();
            SetProfitFilter();
                
            _lvmActlog = new TrX_ListViewManager(listViewActLog);
          
            _leagues = new List<TrX_LeagueInfo>();
            _stopwatchOverlay = new StopWatchOverlay(this, imageList2);
            
            _logic.ClientTxtPath = _mySettings.ReadSetting("poe_logfile_path");

            if (String.IsNullOrEmpty(_logic.ClientTxtPath))
            {
                FileSelectScreen fs = new FileSelectScreen(this)
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };
                fs.ShowDialog();
                _logic.ClientTxtPath = _mySettings.ReadSetting("poe_logfile_path");
            }

            comboBoxShowMaxItems.SelectedItem = ReadSetting("actlog.maxitems", "500");
            comboBox1.SelectedIndex = 0;
            listViewActLog.Columns[0].Width = 120;
            listViewActLog.Columns[1].Width = 50;
            listViewActLog.Columns[2].Width = 110;
            listViewActLog.Columns[3].Width = 100;
            listViewActLog.Columns[4].Width = 50;
            listViewTop10Maps.Columns[0].Width = 300;
            
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisY.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 1;
            chart1.Series[0].XValueType = ChartValueType.DateTime;
            chart1.Series[0].LabelForeColor = Color.White;
            chart1.Series[0].LabelBackColor = Color.Black;
            chart1.Series[0].LabelBorderColor = Color.Black;
            chart1.Series[0].Color = Color.White;
            chart1.Legends[0].Enabled = false;
            chart1.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartMapTierCount.BackColor = Color.Black;
            chartMapTierCount.ChartAreas[0].BackColor = Color.Black;
            chartMapTierCount.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartMapTierCount.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartMapTierCount.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartMapTierCount.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartMapTierCount.ChartAreas[0].AxisX.Interval = 1;
            chartMapTierCount.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
            chartMapTierCount.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartMapTierCount.Series[0].XValueType = ChartValueType.Int32;
            chartMapTierCount.Legends[0].Enabled = false;
            chartMapTierCount.Series[0].IsValueShownAsLabel = true;
            chartMapTierCount.Series[0].LabelForeColor = Color.White;
            chartMapTierCount.Series[0].Color = Color.White;
            chartMapTierCount.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartMapTierCount.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartMapTierCount.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartMapTierAvgTime.BackColor = Color.Black;
            chartMapTierAvgTime.ChartAreas[0].BackColor = Color.Black;
            chartMapTierAvgTime.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartMapTierAvgTime.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartMapTierAvgTime.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartMapTierAvgTime.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartMapTierAvgTime.ChartAreas[0].AxisX.Interval = 1;
            chartMapTierAvgTime.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
            chartMapTierAvgTime.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartMapTierAvgTime.Series[0].XValueType = ChartValueType.Int32;
            chartMapTierAvgTime.Series[0].YValueType = ChartValueType.Double;
            chartMapTierAvgTime.Legends[0].Enabled = false;
            chartMapTierAvgTime.Series[0].IsValueShownAsLabel = true;
            chartMapTierAvgTime.Series[0].LabelForeColor = Color.White;
            chartMapTierAvgTime.Series[0].Color = Color.White;
            chartMapTierAvgTime.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartMapTierAvgTime.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartMapTierAvgTime.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartLabsDone.BackColor = Color.Black;
            chartLabsDone.ChartAreas[0].BackColor = Color.Black;
            chartLabsDone.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartLabsDone.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartLabsDone.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartLabsDone.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartLabsDone.ChartAreas[0].AxisX.Interval = 1;
            chartLabsDone.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartLabsDone.Series[0].XValueType = ChartValueType.String;
            chartLabsDone.Series[0].YValueType = ChartValueType.Double;
            chartLabsDone.Legends[0].Enabled = false;
            chartLabsDone.Series[0].IsValueShownAsLabel = true;
            chartLabsDone.Series[0].LabelForeColor = Color.White;
            chartLabsDone.Series[0].Color = Color.White;
            chartLabsDone.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartLabsDone.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartLabsDone.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartLabsAvgTime.BackColor = Color.Black;
            chartLabsAvgTime.ChartAreas[0].BackColor = Color.Black;
            chartLabsAvgTime.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartLabsAvgTime.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartLabsAvgTime.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartLabsAvgTime.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartLabsAvgTime.ChartAreas[0].AxisX.Interval = 1;
            chartLabsAvgTime.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartLabsAvgTime.Series[0].XValueType = ChartValueType.String;
            chartLabsAvgTime.Series[0].YValueType = ChartValueType.Double;
            chartLabsAvgTime.Legends[0].Enabled = false;
            chartLabsAvgTime.Series[0].IsValueShownAsLabel = true;
            chartLabsAvgTime.Series[0].LabelForeColor = Color.White;
            chartLabsAvgTime.Series[0].Color = Color.White;
            chartLabsAvgTime.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartLabsAvgTime.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartLabsAvgTime.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartHeistByLevel.BackColor = Color.Black;
            chartHeistByLevel.ChartAreas[0].BackColor = Color.Black;
            chartHeistByLevel.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartHeistByLevel.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartHeistByLevel.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartHeistByLevel.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartHeistByLevel.ChartAreas[0].AxisX.Interval = 1;
            chartHeistByLevel.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartHeistByLevel.Series[0].XValueType = ChartValueType.String;
            chartHeistByLevel.Series[0].YValueType = ChartValueType.Double;
            chartHeistByLevel.Legends[0].Enabled = false;
            chartHeistByLevel.Series[0].IsValueShownAsLabel = true;
            chartHeistByLevel.Series[0].LabelForeColor = Color.White;
            chartHeistByLevel.Series[0].Color = Color.White;
            chartHeistByLevel.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartHeistByLevel.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartHeistByLevel.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartHeistAvgTime.BackColor = Color.Black;
            chartHeistAvgTime.ChartAreas[0].BackColor = Color.Black;
            chartHeistAvgTime.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartHeistAvgTime.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartHeistAvgTime.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartHeistAvgTime.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartHeistAvgTime.ChartAreas[0].AxisX.Interval = 1;
            chartHeistAvgTime.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartHeistAvgTime.Series[0].XValueType = ChartValueType.String;
            chartHeistAvgTime.Series[0].YValueType = ChartValueType.Double;
            chartHeistAvgTime.Legends[0].Enabled = false;
            chartHeistAvgTime.Series[0].IsValueShownAsLabel = true;
            chartHeistAvgTime.Series[0].LabelForeColor = Color.White;
            chartHeistAvgTime.Series[0].Color = Color.White;
            chartHeistAvgTime.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartHeistAvgTime.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartHeistAvgTime.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            chartGlobalDashboard.BackColor = Color.Black;
            chartGlobalDashboard.ChartAreas[0].BackColor = Color.Black;
            chartGlobalDashboard.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartGlobalDashboard.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartGlobalDashboard.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartGlobalDashboard.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartGlobalDashboard.ChartAreas[0].AxisX.Interval = 1;
            chartGlobalDashboard.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartGlobalDashboard.Series[0].XValueType = ChartValueType.String;
            chartGlobalDashboard.Series[0].YValueType = ChartValueType.Double;
            chartGlobalDashboard.Legends[0].Enabled = true;
            chartGlobalDashboard.Series[0].IsValueShownAsLabel = true;
            chartGlobalDashboard.Series[0].LabelForeColor = Color.White;
            chartGlobalDashboard.Series[0].Color = Color.White;
            chartGlobalDashboard.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartGlobalDashboard.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartGlobalDashboard.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

            pictureBox10.Image = imageList2.Images[0];
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
                dataGridView1.Rows.Add(new string[] { t.ToString(),  cap.ToString()});
            }

            Text = TrX_AppInfo.NAME;
            _loadScreenWindow = new LoadScreen
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None
            };
            _loadScreenWindow.Show(this);

            InitLeagueInfo();
            ResetLabRuns();
            LoadLayout();

            // Add all enchants
            foreach(TrX_LabEnchant en in _logic.LabbieConnector.KnownEnchants)
            {
                comboBox2.Items.Add(en.Text);
            }

            // Request initial Dashboard update
            _uiFlagLabDashboard = true;
            _uiFlagMapDashboard = true;
            _uiFlagBossDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagActivityListReset = true;
            _uiFlagAllStatsDashboard = true;

            // Set Tooltips. Mapping in UI/Trx_HelpDefinitions.cs
            foreach(KeyValuePair<string,string> kvp in TrX_HelpDefinitions.ToolTips)
            {
                Control cnt = this.Controls.Find(kvp.Key, true)[0];

                ToolTip toolTip = new ToolTip();
                toolTip.AutoPopDelay = 30000;
                toolTip.SetToolTip(cnt, kvp.Value);
            }

            // Map filter
            comboBox3.Items.Add("All");
            foreach(string s in _defaultMappings.MapAreas)
            {
                comboBox3.Items.Add(s);
            }
            foreach (string type in Enum.GetNames(typeof(ACTIVITY_TYPES)))
            {
                comboBox7.Items.Add(type);
            }
            comboBox3.SelectedItem = "All";
            comboBox4.SelectedItem = "OR";
            comboBox6.SelectedItem = "All";
            comboBox7.SelectedItem = "All";
            comboBox8.SelectedItem = "All";

            comboBoxStopWatchTag1.Items.Add("None");
            comboBoxStopWatchTag2.Items.Add("None");
            comboBoxStopWatchTag3.Items.Add("None");

            foreach (TrX_ActivityTag tag in _logic.Tags)
            {
                comboBox5.Items.Add(tag.DisplayName);
                comboBoxStopWatchTag1.Items.Add(tag.ID);
                comboBoxStopWatchTag2.Items.Add(tag.ID);
                comboBoxStopWatchTag3.Items.Add(tag.ID);
            }

            comboBoxStopWatchTag1.SelectedItem = _overlayTag1 != null ? _overlayTag1 : "None";
            comboBoxStopWatchTag2.SelectedItem = _overlayTag2 != null ? _overlayTag2 : "None";
            comboBoxStopWatchTag3.SelectedItem = _overlayTag3 != null ? _overlayTag3 : "None";

            foreach (string s in _defaultMappings.AllAreas)
            {
                comboBox8.Items.Add(s);
            }

            lbl_filter.Visible = false;
            pictureBox32.Visible = false;

            ResetFilter();

            //tableLayoutPanelMain.RowStyles[1].Height = 0;

            // Start UI Thread
            timer1.Enabled = true;
            timer1.Start();

            // Add dummy Labrun for testing
            // ==============================
            bool addDummyLab = false;
            bool addDummyEnchants = true;
            if(addDummyLab)
            {
                AddDummyLabForTesting(addDummyEnchants, 3);
            }
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

                    label100.Text = string.Format("{0} ({1} - {2})", GetStatLongName(_allStatsSelected), _allStatsChartDT1, _allStatsChartDT2);
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
        /// Method for testing and debugging only
        /// </summary>
        /// <param name="addDummyEnchants"></param>
        private void AddDummyLabForTesting(bool addDummyEnchants = false, int enchant_count = 3)
        {
            TrX_TrackedLabrun lr = new TrX_TrackedLabrun();
            lr.Area = "Uber-Lab";
            lr.Started = DateTime.Now;
            lr.TimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            lr.Type = ACTIVITY_TYPES.LABYRINTH;
            lr.StartStopWatch();
            if (addDummyEnchants)
            {
                for(int i = 0; i < enchant_count; i++)
                {
                    lr.Enchants.Add(new TrX_LabEnchant() { ID = 2, Text = "Dummy Enchant 1" });
                }
            }
            UI.UserControlLabRun uclr = new UI.UserControlLabRun(lr, this, true);
            uclr.Dock = DockStyle.Fill;
            foreach (string s in comboBox2.Items)
            {
                uclr.EnchantCombo.Items.Add(s);
            }
            panel23.Controls.Add(uclr);
            _currentLabrunControl = uclr;
            _logic.CurrentLab = lr;
            _logic.CurrentActivity = lr;

           
        }

        /// <summary>
        /// Event Handler
        /// </summary>
        /// <param name="e"></param>
        private void _logic_OnActivityStarted(TrX_CoreLogicActivityEventArgs e)
        {
            if(_logic.EventQueueInitialized)
            {
                // Start new lab
                if (e.Activity.Type == ACTIVITY_TYPES.LABYRINTH)
                {
                    MethodInvoker mi = delegate
                    {
                        UI.UserControlLabRun ulr = new UI.UserControlLabRun((TrX_TrackedLabrun)e.Activity, this, true);
                        ulr.Dock = DockStyle.Fill;
                        foreach (string s in comboBox2.Items)
                        {
                            ulr.EnchantCombo.Items.Add(s);
                        }
                        _myTheme.Apply(ulr);

                        if (_currentLabrunControl != null)
                        {
                            panel23.Controls.Remove(_currentLabrunControl);
                            _currentLabrunControl = null;
                        }

                        _currentLabrunControl = ulr;
                        panel23.Controls.Add(ulr);
                    };
                    BeginInvoke(mi);
                }
            }
           
        }

        /// <summary>
        /// Event Handler
        /// </summary>
        /// <param name="e"></param>
        private void _logic_LabEnchantsReceived(TrX_LabbieEventArgs e)
        {
            _uiFlagEnchants = true;

            if(_currentLabrunControl != null)
            {
                MethodInvoker mi = delegate
                {
                    foreach (TrX_LabEnchant enchant in e.Enchants)
                    {
                        _currentLabrunControl.AddEnchant(enchant);
                        if(!comboBox2.Items.Contains(enchant.Text))
                        {
                            comboBox2.Items.Add(enchant.Text);
                        }
                    }
                };
                BeginInvoke(mi);
            }
            
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
            xml.Load(TrX_AppInfo.APPDATA_PATH + @"\metadata\metadata.xml");

            TrX_LeagueInfo currentLeague = null;

            foreach(XmlNode node in xml.SelectSingleNode("/metadata/leagues").SelectNodes("league"))
            {
                DateTime start = DateTime.Parse(node.Attributes["start"].Value);
                DateTime end = DateTime.Parse(node.Attributes["end"].Value);

                TrX_LeagueInfo li = new TrX_LeagueInfo(
                    node.Attributes["name"].Value,
                    Convert.ToInt32(node.Attributes["major"].Value),
                    Convert.ToInt32(node.Attributes["minor"].Value),
                    new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second, _dateTimeFormatInfo.Calendar),
                    new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, end.Second, _dateTimeFormatInfo.Calendar));

                if(Convert.ToBoolean(node.Attributes["current"].Value))
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
                    comboBox1.Items.Add(string.Format("Current League: {0} ({1})", li.Name, li.Version));
                }
                else
                {
                    comboBox1.Items.Add(string.Format("League: {0} ({1})", li.Name, li.Version));
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
                    AddUpdateAppSettings("layout.listview.cols." + ch.Name + ".width", ch.Width.ToString());
                }
            }
            if (Width > 50 && Height > 50)
            {
                AddUpdateAppSettings("layout.window.width", Width.ToString());
                AddUpdateAppSettings("layout.window.height", Height.ToString());
            }
        }

        /// <summary>
        /// Load GUI layout from config
        /// </summary>
        private void LoadLayout()
        {
            foreach (ColumnHeader ch in listViewActLog.Columns)
            {
                int w = Convert.ToInt32(ReadSetting("layout.listview.cols." + ch.Name + ".width"));
                if (w > 0)
                {
                    ch.Width = w;
                }
            }

            int iWidth = Convert.ToInt32(ReadSetting("layout.window.width"));
            int iHeight = Convert.ToInt32(ReadSetting("layout.window.height"));

            if (iWidth > 50 && iHeight > 50)
            {
                Width = iWidth;
                Height = iHeight;
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
            int iLabelWidth = 100;
            int iMaxCols = 5;

            int iCols = (groupBox3.Width - 20) / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < _logic.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _logic.Tags[i];
                Label lbl = new Label
                {
                    Width = iLabelWidth
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if (!_tagLabelsConfig.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.Name = "lbl_tag_" + tag.ID;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = tag.BackColor;
                    lbl.ForeColor = tag.ForeColor;
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick1;
                    lbl.Location = new Point(iX, iY);

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
            label63.ForeColor = tag.ForeColor;
            label63.BackColor = tag.BackColor;
            label63.Text = tag.DisplayName;
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
            if (b_reinit)
            {
                groupBoxTrackingTags.Controls.Clear();
                _tagLabels.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 20;
            int iLabelWidth = 100;
            int iMaxCols = 5;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = groupBoxTrackingTags.Width / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < _logic.Tags.Count; i++)
            {
                TrX_ActivityTag tag = _logic.Tags[i];
                Label lbl = new Label
                {
                    Width = iLabelWidth
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if (!_tagLabels.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.Name = "lbl_tag_" + tag.ID;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = Color.Gray;
                    lbl.ForeColor = Color.LightGray;
                    lbl.Location = new Point(iX, iY);
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick;

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
                            _tagLabels[tag.ID].BackColor = tag.BackColor;
                            _tagLabels[tag.ID].ForeColor = tag.ForeColor;
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
        private void RenderTagsForSummary(UI.SummaryWindow targetSummaryWindow, Dictionary<string,int> dict, bool b_reinit = false)
        {
            Control targetControl = targetSummaryWindow.groupBox2;

            if (b_reinit)
            {
                targetControl.Controls.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 20;
            int iLabelWidth = 100;
            int iMaxCols = 5;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = targetControl.Width / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            foreach(KeyValuePair<string,int> kvp in dict)
            {
                TrX_ActivityTag tag = _logic.GetTagByID(kvp.Key);
                Label lbl = new Label
                {
                    Width = iLabelWidth
                };

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                lbl.Text = string.Format("{0}: {1}", tag.DisplayName, kvp.Value);
                lbl.Name = "lbl_tag_" + tag.ID;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.BackColor = tag.BackColor;
                lbl.ForeColor = tag.ForeColor;
                lbl.Location = new Point(iX, iY);
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
        /// Mouse over handler for label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lbl_MouseHover(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            if (Directory.Exists(TrX_AppInfo.APPDATA_PATH + @"\backups"))
            {
                foreach (string s in Directory.GetDirectories(TrX_AppInfo.APPDATA_PATH + @"\backups"))
                {
                    foreach (string s2 in Directory.GetDirectories(s))
                    {
                        string s_name = s2.Replace(TrX_AppInfo.APPDATA_PATH, "");

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
            _logic.Database.DoNonQuery("delete from tx_activity_log where timestamp = " + timestamp.ToString());
        }

        /// <summary>
        /// Simply save the current app version to VERSION.txt
        /// </summary>
        private void SaveVersion()
        {
            StreamWriter streamWriter = new StreamWriter(TrX_AppInfo.APPDATA_PATH + @"\VERSION.txt");
            streamWriter.WriteLine(TrX_AppInfo.VERSION);
            streamWriter.Close();
        }

        private void ResetLabRuns()
        {
            int labsToShow = 5;

            panel22.Controls.Clear();

            if(_logic.LabHistory.Count > 0)
            {
                if (_logic.LabHistory.Count < labsToShow)
                {
                    labsToShow = _logic.LabHistory.Count;
                }
                int y = 0;
                foreach (TrX_TrackedLabrun labrun in _logic.LabHistory.GetRange(0, labsToShow))
                {
                    UI.UserControlLabRun lr = new UI.UserControlLabRun(labrun, this);
                    lr.Location = new Point(0, y);
                    panel22.Controls.Add(lr);
                    y += lr.Height + 20;
                }
            }
           
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
                        Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_tag_" + tag.ID + ".width", "60"))
                    };
                    _lvmActlog.Columns.Add(ch);
                }
            }

            AddActivityLvItems();
            _log.Debug("2 _lvmActLog.MasterList.Count = " + _lvmActlog.MasterList.Count);
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
            //_lvmActlog.FilterByRange(0, Convert.ToInt32(ReadSetting("actlog.maxitems", "500")));
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
                else if (activity.MapTier >= 11)
                {
                    imageIndex = 2;
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
                ListViewItem lvi = new ListViewItem(" " + map.Started.ToString());
                string sName = map.Area;
                string sTier = "";

                if (map.AreaLevel == 0)
                {
                    sTier = "-";
                }
                else if (map.Type == ACTIVITY_TYPES.MAP)
                {
                    sTier = "T" + map.MapTier.ToString();
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

            if(workerStatus)
            {
                SetUILoading();
            }
            else
            {
                SetUIReady();
            }

            btt_summary.Text = String.Format("summary ({0})", listViewActLog.SelectedIndices.Count);


            TimeSpan tsAreaTime = (DateTime.Now - _inAreaSince);
            checkBoxShowGridInAct.Checked = _showGridInActLog;
            checkBoxShowGridInStats.Checked = _showGridInStats;
            ReadBackupList();
            listBoxRestoreBackup.DataSource = _backups;

            if (_logic.EventQueueInitialized)
            {
                stopwatchToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
                stopwatchToolStripMenuItem1.Checked = _stopwatchOverlay.Visible;
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
                            ActivateStopWatchOverlay();

                        }
                        _autoStartsDone = true;
                    }

                    RenderTagsForTracking();
                    RenderTagsForConfig();
                    textBoxLogFilePath.Text = ReadSetting("poe_logfile_path");

                    labelItemCount.Text = "items: " + _actLogItemCount.ToString();

                    if (!_listViewInitielaized)
                    {
                        _oldestTimeStamp = _logic.Stats.GetOldestTimeStamp();
                        _listViewInitielaized = true;
                    }

                    label80.Text = string.Format("{0} (lvl. {1})", _logic.CurrentArea, _logic.CurrentAreaLevel);

                    if (_logic.CurrentArea.Contains("Hideout") && !(_logic.CurrentArea.Contains("Syndicate")))
                    {
                        label102.Text = "In Hideout";
                    }
                    else
                    {
                        if (_logic.CurrentActivity != null)
                        {
                            label102.Text = _logic.CurrentActivity.Type.ToString();
                        }
                        else
                        {
                            label102.Text = "Nothing";
                        }
                    }

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
                                sTier = "T" + _logic.CurrentActivity.MapTier.ToString();
                            }
                            else
                            {
                                sTier = "Lvl. " + _logic.CurrentActivity.AreaLevel.ToString();
                            }
                        }

                        if (_logic.CurrentActivity != null && _logic.CurrentActivity.Type == ACTIVITY_TYPES.BREACHSTONE)
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.GetBreachStoneName(_logic.CurrentActivity.Area, _logic.CurrentActivity.AreaLevel);
                            labelTrackingDied.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity)];
                        }
                        else if ((_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_ZanaMap.Area + " (" + sTier + ", Zana)";
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_ZanaMap.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                            pictureBoxStop.Hide();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_ZanaMap)];
                        }
                        else if ((_logic.IsMapVaalArea && _logic.CurrentActivity.SideArea_VaalArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_VaalArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_VaalArea.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_VaalArea.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_VaalArea.Type));
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_VaalArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapAbyssArea && _logic.CurrentActivity.SideArea_AbyssArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_AbyssArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_AbyssArea.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_AbyssArea.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_AbyssArea.Type));
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_AbyssArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapLabTrial && _logic.CurrentActivity.SideArea_LabTrial != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_LabTrial.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_LabTrial.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_LabTrial.DeathCounter.ToString();
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.SideArea_LabTrial.Type));
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_LabTrial)];
                            pictureBoxStop.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = TrX_Helpers.CapitalFirstLetter(GetStringFromActType(_logic.CurrentActivity.Type));
                            labelTrackingDied.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity)];
                            pictureBoxStop.Show();
                        }
                    }
                    else
                    {
                        labelTrackingDied.Text = "0";
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

                    // Lab farming tab
                    if(_logic.CurrentLab != null && _currentLabrunControl != null)
                    {
                        _currentLabrunControl.UpdateInfo();
                    }

                    // MAP Dashbaord
                    if (_uiFlagMapDashboard)
                    {
                        dtRenderStart = DateTime.Now;
                        
                        RenderMappingDashboard();
                        _uiFlagMapDashboard = false;

                        _log.Debug(string.Format("Updated 'MapDashboard' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // LAB Dashbaord
                    if (_uiFlagLabDashboard)
                    {
                        dtRenderStart = DateTime.Now;

                        RenderLabDashboard();
                        _uiFlagLabDashboard = false;

                        _log.Debug(string.Format("Updated 'LabDashboard' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // HEIST Dashbaord
                    if (_uiFlagHeistDashboard)
                    {
                        dtRenderStart = DateTime.Now;

                        RenderHeistDashboard();
                        _uiFlagHeistDashboard = false;

                        _log.Debug(string.Format("Updated 'HeistDashboard' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // AllStats Dashbaord
                    if (_uiFlagAllStatsDashboard)
                    {
                        dtRenderStart = DateTime.Now;

                        RenderAllStatsDashboard();
                        _uiFlagAllStatsDashboard = false;

                        _log.Debug(string.Format("Updated 'AllStatsDashboard' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    //Bossing
                    if (_uiFlagBossDashboard)
                    {
                        dtRenderStart = DateTime.Now;

                        RenderBossingDashboard();
                        _uiFlagBossDashboard = false;

                        _log.Debug(string.Format("Updated 'BossDashboard' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // Global Dashbaord
                    if (_uiFlagGlobalDashboard)
                    {
                        dtRenderStart = DateTime.Now;

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

                        _log.Debug(string.Format("Updated 'ActivityOverview' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // All stats chart
                    if(_uiFlagStatisticsChart)
                    {
                        dtRenderStart = DateTime.Now;

                        UpdateAllStatsChart();
                        _uiFlagStatisticsChart = false;

                        _log.Debug(string.Format("Updated 'AllStatsChart' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    // Rest act list
                    if (_uiFlagActivityListReset)
                    {
                        _log.Debug("_lvmActLog.MasterList.Count = " + _lvmActlog.MasterList.Count);
                        ResetMapHistory();
                        DoSearch();
                        _uiFlagActivityListReset = false;
                        _uiFlagActivityList = false;
                        
                    }
                    else if (_uiFlagActivityList)
                    {
                        dtRenderStart = DateTime.Now;

                        DoSearch();
                        _uiFlagActivityList = false;

                        _log.Debug(string.Format("Updated 'ActivityList' in {0}ms", (DateTime.Now - dtRenderStart).TotalMilliseconds));
                    }

                    listView1.Columns[2].Width = listView1.Width;

                };
                Invoke(mi);
            }

            // Update stopwatchOverlay
            _stopwatchOverlay.UpdateStopWatch(labelStopWatch.Text,
                           _logic.OverlayPrevActivity != null ? _logic.OverlayPrevActivity.StopWatchValue : "00:00:00",
                           _logic.CurrentActivity != null ? GetImageIndex(_logic.CurrentActivity) : 0,
                           _logic.OverlayPrevActivity != null ? GetImageIndex(_logic.OverlayPrevActivity) : 0);
            // Update stopwatchOverlay tags
            bool tag1Status = false, 
                 tag2Status = false, 
                 tag3Status = false;


            if(_logic.CurrentActivity != null)
            {
                if(_overlayTag1 != null)
                {
                    tag1Status = _logic.CurrentActivity.HasTag(_overlayTag1);
                }
                if (_overlayTag2 != null)
                {
                    tag2Status = _logic.CurrentActivity.HasTag(_overlayTag2);
                }
                if (_overlayTag3 != null)
                {
                    tag3Status = _logic.CurrentActivity.HasTag(_overlayTag3);
                }
            }

            _stopwatchOverlay.UpdateTagStatus(
                _logic.GetTagByID(_overlayTag1),
                _logic.GetTagByID(_overlayTag2), 
                _logic.GetTagByID(_overlayTag3), 
                tag1Status, 
                tag2Status, 
                tag3Status);

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
            _stopwatchOverlayOpacity = Convert.ToInt32(ReadSetting("overlay.stopwatch.opacity", "100"));
            _stopwatchOverlayShowDefault = Convert.ToBoolean(ReadSetting("overlay.stopwatch.default", "false"));
            comboBoxTheme.SelectedItem = ReadSetting("theme", "Dark") == "Dark" ? "Dark" : "Light";
            listViewActLog.GridLines = _showGridInActLog;
            trackBar1.Value = _stopwatchOverlayOpacity;
            checkBox2.Checked = _stopwatchOverlayShowDefault;
            checkBoxMinimizeToTray.Checked = _minimizeToTray;
            label38.Text = _stopwatchOverlayOpacity.ToString() + "%";
            //checkBox3.Checked = Convert.ToBoolean(ReadSetting("statistics_auto_refresh", "false"));
            textBox9.Text = ReadSetting("lab.profittracking.filter.text", "");
            radioButton1.Checked = ReadSetting("lab.profittracking.filter.state", "all") == "all";
            radioButton2.Checked = ReadSetting("lab.profittracking.filter.state", "all") == "open";
            radioButton3.Checked = ReadSetting("lab.profittracking.filter.state", "all") == "sold";
            textBox6.Text = ReadSetting("labbie.path", "");
            _overlayTag1 = ReadSetting("overlay.stopwatch.tag1", "blight");
            _overlayTag2 = ReadSetting("overlay.stopwatch.tag2", "expedition");
            _overlayTag3 = ReadSetting("overlay.stopwatch.tag3", null);
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
            if(!_exitting)
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
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, 0 }
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
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, 0 }
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
                { ACTIVITY_TYPES.LAKE_OF_KALANDRA, Color.Silver }
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
                    chartGlobalDashboard.Series[0].Points.Last().LegendText = string.Format("{0} ({1}%)", kvp.Key.ToString(), percentVal);

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
            chartGlobalDashboard.Series[0].Points.Last().Label = dOther > 0 ? string.Format("{0} h", Math.Round(tsDurationOther.TotalHours, 1)) : " ";
            chartGlobalDashboard.Series[0].Points.Last().LegendText = string.Format("{0} ({1}%)", "Other", percentValOther);

            if (_showHideoutInPie)
            {
                // Add HO
                double percentValHO = hideOutTime / totalCount * 100;
                percentValHO = Math.Round(percentValHO, 2);
                TimeSpan tsDurationHO = TimeSpan.FromSeconds(hideOutTime);
                chartGlobalDashboard.Series[0].Points.AddXY("Hideout", Math.Round(tsDurationHO.TotalSeconds / 60 / 60, 1));
                chartGlobalDashboard.Series[0].Points.Last().Color = Color.Blue; ;
                chartGlobalDashboard.Series[0].Points.Last().Label = tsDurationHO.TotalSeconds > 0 ? string.Format("{0} h", Math.Round(tsDurationHO.TotalHours, 1)) : " ";
                chartGlobalDashboard.Series[0].Points.Last().LegendText = string.Format("{0} ({1}%)", "Hideout", percentValHO);

                ListViewItem lvi = new ListViewItem("HIDEOUT");
                lvi.SubItems.Add("-");
                lvi.SubItems.Add(Math.Round(tsDurationHO.TotalSeconds / 60 / 60, 1).ToString() + " h");
                lvi.SubItems.Add(percentValHO + "%");
                lvi.BackColor = Color.Blue;

                listView1.Items.Add(lvi);
            }

            label46.Text = String.Format("Total play time: {0} hours", Math.Round(totalCount / 60 / 60, 2));
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
        /// Update bossing dashboard
        /// </summary>
        private void RenderBossingDashboard()
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dateTimeFormatInfo.Calendar);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dateTimeFormatInfo.Calendar);
            long ts1 = ((DateTimeOffset)dt1).ToUnixTimeSeconds();
            long ts2 = ((DateTimeOffset)dt2).ToUnixTimeSeconds();

            double baranTried = 0,
                baranKilled = 0,
                droxTried = 0,
                droxKilled = 0,
                veriTried = 0,
                veriKilled = 0,
                hunterTried = 0,
                hunterKilled = 0,
                elderTried = 0,
                elderKilled = 0,
                shaperTried = 0,
                shaperKilled = 0,
                cataTried = 0,
                cataKilled = 0,
                sirusTried = 0,
                sirusKilled = 0,
                mavenTried = 0,
                mavenKilled = 0,
                tmTried = 0,
                tmKilled = 0,
                exarchTried = 0,
                exarchKilled = 0,
                blackStarTried = 0,
                blackStarKilled = 0,
                eaterTried = 0,
                eaterKilled = 0,
                hungerTried = 0,
                hungerKilled = 0;

            baranTried = _logic.Stats.GetIncrementValue("BaranStarted", ts1, ts2);
            baranKilled = _logic.Stats.GetIncrementValue("BaranKilled", ts1, ts2);
            veriTried = _logic.Stats.GetIncrementValue("VeritaniaStarted", ts1, ts2);
            veriKilled = _logic.Stats.GetIncrementValue("VeritaniaKilled", ts1, ts2);
            droxTried = _logic.Stats.GetIncrementValue("DroxStarted", ts1, ts2);
            droxKilled = _logic.Stats.GetIncrementValue("DroxKilled", ts1, ts2);
            hunterTried = _logic.Stats.GetIncrementValue("HunterStarted", ts1, ts2);
            hunterKilled = _logic.Stats.GetIncrementValue("HunterKilled", ts1, ts2);
            elderTried = _logic.Stats.GetIncrementValue("ElderTried", ts1, ts2);
            elderKilled = _logic.Stats.GetIncrementValue("ElderKilled", ts1, ts2);
            shaperTried = _logic.Stats.GetIncrementValue("ShaperTried", ts1, ts2);
            shaperKilled = _logic.Stats.GetIncrementValue("ShaperKilled", ts1, ts2);
            cataTried = _logic.Stats.GetIncrementValue("CatarinaTried", ts1, ts2);
            cataKilled = _logic.Stats.GetIncrementValue("CatarinaKilled", ts1, ts2);
            sirusTried = _logic.Stats.GetIncrementValue("SirusStarted", ts1, ts2);
            sirusKilled = _logic.Stats.GetIncrementValue("SirusKilled", ts1, ts2);
            mavenTried = _logic.Stats.GetIncrementValue("MavenStarted", ts1, ts2);
            mavenKilled = _logic.Stats.GetIncrementValue("MavenKilled", ts1, ts2);
            tmTried = _logic.Stats.GetIncrementValue("TrialMasterStarted", ts1, ts2);
            tmKilled = _logic.Stats.GetIncrementValue("TrialMasterKilled", ts1, ts2);
            exarchTried = _logic.Stats.GetIncrementValue("SearingExarchTried", ts1, ts2);
            exarchKilled = _logic.Stats.GetIncrementValue("SearingExarchKilled", ts1, ts2);
            blackStarTried = _logic.Stats.GetIncrementValue("BlackStarTried", ts1, ts2);
            blackStarKilled = _logic.Stats.GetIncrementValue("BlackStarKilled", ts1, ts2);
            hungerTried = _logic.Stats.GetIncrementValue("InfiniteHungerTried", ts1, ts2);
            hungerKilled = _logic.Stats.GetIncrementValue("InfiniteHungerKilled", ts1, ts2);
            eaterTried = _logic.Stats.GetIncrementValue("EaterOfWorldsTried", ts1, ts2);
            eaterKilled = _logic.Stats.GetIncrementValue("EaterOfWorldsKilled", ts1, ts2);


            MethodInvoker mi = delegate
            {
                // Baran
                labelBaranStatus.Text = baranKilled > 0 ? "Yes" : "No";
                labelBaranStatus.ForeColor = baranKilled > 0 ? Color.Green : Color.Red;
                labelBaranTries.Text = baranTried.ToString();
                labelBaranKillCount.Text = baranKilled.ToString();

                // Veritania
                labelVeritaniaStatus.Text = veriKilled > 0 ? "Yes" : "No";
                labelVeritaniaStatus.ForeColor = veriKilled > 0 ? Color.Green : Color.Red;
                labelVeritaniaTries.Text = veriTried.ToString();
                labelVeritaniaKillCount.Text = veriKilled.ToString();

                // Drox
                labelDroxStatus.Text = droxKilled > 0 ? "Yes" : "No";
                labelDroxStatus.ForeColor = droxKilled > 0 ? Color.Green : Color.Red;
                labelDroxTries.Text = droxTried.ToString();
                labelDroxKillCount.Text = droxKilled.ToString();

                // Hunter
                labelHunterStatus.Text = hunterKilled > 0 ? "Yes" : "No";
                labelHunterStatus.ForeColor = hunterKilled > 0 ? Color.Green : Color.Red;
                labelHunterTries.Text = hunterTried.ToString();
                labelHunterKillCount.Text = hunterKilled.ToString();

                // Elder
                labelElderStatus.Text = elderKilled > 0 ? "Yes" : "No";
                labelElderStatus.ForeColor = elderKilled > 0 ? Color.Green : Color.Red;
                labelElderTried.Text = elderTried.ToString();
                labelElderKillCount.Text = elderKilled.ToString();

                // Shaper
                labelShaperStatus.Text = shaperKilled > 0 ? "Yes" : "No";
                labelShaperStatus.ForeColor = shaperKilled > 0 ? Color.Green : Color.Red;
                labelShaperTried.Text = shaperTried.ToString();
                labelShaperKillCount.Text = shaperKilled.ToString();

                // Sirus
                labelSirusStatus.Text = sirusKilled > 0 ? "Yes" : "No";
                labelSirusStatus.ForeColor = sirusKilled > 0 ? Color.Green : Color.Red;
                labelSirusTries.Text = sirusTried.ToString();
                labelSirusKillCount.Text = sirusKilled.ToString();

                // Maven
                labelMavenStatus.Text = mavenKilled > 0 ? "Yes" : "No";
                labelMavenStatus.ForeColor = mavenKilled > 0 ? Color.Green : Color.Red;
                labelMavenTried.Text = mavenTried.ToString();
                labelMavenKilled.Text = mavenKilled.ToString();

                // Catarina
                labelCataStatus.Text = cataKilled > 0 ? "Yes" : "No";
                labelCataStatus.ForeColor = cataKilled > 0 ? Color.Green : Color.Red;
                labelCataTried.Text = cataTried.ToString();
                labelCataKilled.Text = cataKilled.ToString();

                // TrialMaster
                labelTrialMasterStatus.Text = tmKilled > 0 ? "Yes" : "No";
                labelTrialMasterStatus.ForeColor = tmKilled > 0 ? Color.Green : Color.Red;
                labelTrialMasterTried.Text = tmTried.ToString();
                labelTrialMasterKilled.Text = tmKilled.ToString();

                // Exarch
                labelExarchStatus.Text = exarchKilled > 0 ? "Yes" : "No";
                labelExarchStatus.ForeColor = exarchKilled > 0 ? Color.Green : Color.Red;
                labelExarchTried.Text = exarchTried.ToString();
                labelExarchKilled.Text = exarchKilled.ToString();

                // Black Star
                labelBlackstarStatus.Text = blackStarKilled > 0 ? "Yes" : "No";
                labelBlackstarStatus.ForeColor = blackStarKilled > 0 ? Color.Green : Color.Red;
                labelBlackStarTried.Text = blackStarTried.ToString();
                labelBlackStarKilled.Text = blackStarKilled.ToString();

                // Infinite Hunger
                labelHungerStatus.Text = hungerKilled > 0 ? "Yes" : "No";
                labelHungerStatus.ForeColor = hungerKilled > 0 ? Color.Green : Color.Red;
                labelHungerTried.Text = hungerTried.ToString();
                labelHungerKilled.Text = hungerKilled.ToString();

                // Eater of Worlds
                labelEaterStatus.Text = eaterKilled > 0 ? "Yes" : "No";
                labelEaterStatus.ForeColor = eaterKilled > 0 ? Color.Green : Color.Red;
                labelEaterTried.Text = eaterTried.ToString();
                labelEaterKilled.Text = eaterKilled.ToString();
            };
            BeginInvoke(mi);
        }

        /// <summary>
        /// Update All-Stats Dashboard
        /// </summary>
        private void RenderAllStatsDashboard()
        {
            if(!_workerAllStats.IsBusy)
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

            if(!_workerAllStatsChart.IsBusy)
            {
                _workerAllStatsChart.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Update the mapping dashboard
        /// </summary>
        public void RenderMappingDashboard()
        {
            List<KeyValuePair<string, int>> tmpList = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> top10 = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> tmpListTags = new Dictionary<string, int>();
            List<KeyValuePair<string, int>> top10Tags = new List<KeyValuePair<string, int>>();
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
            top10.AddRange(tmpList.GetRange(0, 10));
            listViewTop10Maps.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in top10)
            {
                ListViewItem lvi = new ListViewItem(kvp.Key);
                lvi.SubItems.Add(kvp.Value.ToString());
                listViewTop10Maps.Items.Add(lvi);
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
            top10Tags.AddRange(tmpList);
            listViewTaggingOverview.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in top10Tags)
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
            string sBackupDir = TrX_AppInfo.APPDATA_PATH + @"/backups/" + s_name + @"/" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            System.IO.Directory.CreateDirectory(sBackupDir);

            if (System.IO.File.Exists(_logic.ClientTxtPath))
                System.IO.File.Copy(_logic.ClientTxtPath, sBackupDir + @"/Client.txt");
            if (System.IO.File.Exists(TrX_AppInfo.CACHE_PATH))
                System.IO.File.Copy(TrX_AppInfo.CACHE_PATH, sBackupDir + @"/stats.cache");
            if (System.IO.File.Exists(TrX_AppInfo.DB_PATH))
                System.IO.File.Copy(TrX_AppInfo.DB_PATH, sBackupDir + @"/data.db");
            if (System.IO.File.Exists(TrX_AppInfo.APPDATA_PATH + @"\config.xml"))
                System.IO.File.Copy(TrX_AppInfo.APPDATA_PATH + @"\config.xml", sBackupDir + @"/config.xml");
            if (System.IO.File.Exists(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml"))
                System.IO.File.Copy(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml", sBackupDir + @"/labdata.xml");
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
            OpenChildWindow(new ActivityDetails(ta, this));
        }

        /// <summary>
        /// Open child window and apply theme to i´t
        /// </summary>
        /// <param name="form">Form to open</param>
        private void OpenChildWindow(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.ShowInTaskbar = false;
            form.Location = new Point(this.Location.X + 100, this.Location.Y + 100);
            form.Owner = this;
            _myTheme.Apply(form);
            form.Show();
        }

        /// <summary>
        /// Export Activity log to CSV
        /// </summary>
        /// <param name="sPath"></param>
        public void WriteActivitiesToCSV(string sPath)
        {
            StreamWriter wrt = new StreamWriter(sPath);
            TrX_TrackedActivity tm;

            //Write headline
            string sLine = TrX_TrackedActivity.GetCSVHeadline();
            wrt.WriteLine(sLine);

            for (int i = 0; i < _statsDataSource.Count; i++)
            {
                wrt.WriteLine(_statsDataSource[i].ToCSVLine());
            }
            wrt.Close();
        }

        /// <summary>
        /// Prepare backup restore before app restarts
        /// </summary>
        /// <param name="sPath"></param>
        private void PrepareBackupRestore(string sPath)
        {
            File.Copy(sPath + @"/data.db", TrX_AppInfo.DB_PATH + ".restore");
            File.Copy(sPath + @"/Client.txt", Directory.GetParent(_logic.ClientTxtPath) + @"/_Client.txt.restore");
            File.Copy(sPath + @"/config.xml", TrX_AppInfo.APPDATA_PATH + @"/config.xml.restore");
            if(File.Exists(sPath + @"/labdata.xml"))
            {
                File.Copy(sPath + @"/labdata.xml", TrX_AppInfo.APPDATA_PATH + @"/labdata.xml.restore");
            }
            _log.Info("Backup restore successfully prepared! Restarting Application");
            Application.Restart();
        }

        /// <summary>
        /// Check if backup restore is prepared and restore
        /// </summary>
        private void DoBackupRestoreIfPrepared()
        {
            if (File.Exists(TrX_AppInfo.DB_PATH + ".restore"))
            {
                File.Delete(TrX_AppInfo.DB_PATH);
                File.Move(TrX_AppInfo.DB_PATH + ".restore", TrX_AppInfo.DB_PATH);
                _log.Info("BackupRestored -> Source: _data.db.restore, Destination: " + TrX_AppInfo.DB_PATH);
                _restoreMode = true;
            }

            if (File.Exists(TrX_AppInfo.APPDATA_PATH + @"\config.xml.restore"))
            {
                File.Delete(TrX_AppInfo.APPDATA_PATH + @"\config.xml");
                File.Move(TrX_AppInfo.APPDATA_PATH + @"\config.xml.restore", TrX_AppInfo.APPDATA_PATH + @"\config.xml");
                _log.Info("BackupRestored -> Source: config.xml.restore, Destination: " + TrX_AppInfo.APPDATA_PATH + @"\config.xml");
                _restoreMode = true;
            }

            if (File.Exists(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml.restore"))
            {
                try
                {
                    File.Delete(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml");
                }
                catch(Exception ex)
                {
                    _log.Warn("cannot delete labdata.xml: " + ex.Message);
                }
                
                File.Move(TrX_AppInfo.APPDATA_PATH + @"\labdata.xml.restore", TrX_AppInfo.APPDATA_PATH + @"\labdata.xml");
                _log.Info("BackupRestored -> Source: labdata.xml.restore, Destination: " + TrX_AppInfo.APPDATA_PATH + @"\labdata.xml");
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
                    _log.Info("BackupRestored -> Source: " + Directory.GetParent(clientTxtPath) + @"/_Client.txt.restore" +
                        ", Destination: " + Directory.GetParent(clientTxtPath) + @"/_Client.txt");
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
                + "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'custom', " + (tag.ShowInListView ? "1" : "0") + ")");

            listViewActLog.Columns.Add(tag.DisplayName);
            comboBox5.Items.Add(tag.DisplayName);
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
                    UpdateUI();
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
                    _logic.Database.DoNonQuery("UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString());
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
                    _logic.Database.DoNonQuery("UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString());
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
        private void UpdateTag(string s_id, string s_display_name, string s_forecolor, string s_backcolor, bool b_show_in_hist)
        {
            int iTagIndex = GetTagIndex(s_id);

            if (iTagIndex >= 0)
            {
                _logic.Tags[iTagIndex].DisplayName = s_display_name;
                _logic.Tags[iTagIndex].ForeColor = Color.FromArgb(Convert.ToInt32(s_forecolor));
                _logic.Tags[iTagIndex].BackColor = Color.FromArgb(Convert.ToInt32(s_backcolor));
                _logic.Tags[iTagIndex].ShowInListView = b_show_in_hist;

                _logic.Database.DoNonQuery("UPDATE tx_tags SET tag_display = '" + s_display_name + "', tag_forecolor = '" + s_forecolor + "', tag_bgcolor = '" + s_backcolor + "', " +
                    "tag_show_in_lv = " + (b_show_in_hist ? "1" : "0") + " WHERE tag_id = '" + s_id + "'");
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
                    DialogResult dr = MessageBox.Show("Do you really want to delete the tag '" + s_id + "'?", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        _logic.Tags.RemoveAt(iIndex);
                        _logic.Database.DoNonQuery("DELETE FROM tx_tags WHERE tag_id = '" + s_id + "' AND tag_type != 'default'");
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
            else if (textBox8.Text.Contains("tags=="))
            {
                List<string> itemNames = new List<string>();
                try
                {
                    string[] sTagFilter = textBox8.Text.Split(new string[] { "==" }, StringSplitOptions.None)[1].Split(',');
                    int iMatched = 0;
                    foreach (TrX_TrackedActivity ta in _logic.ActivityHistory)
                    {
                        iMatched = 0;
                        foreach (string tag in sTagFilter)
                        {
                            if (ta.HasTag(tag))
                            {
                                iMatched++;
                            }
                            else
                            {
                                iMatched = 0;
                                break;
                            }
                        }
                        if (iMatched > 0)
                        {
                            itemNames.Add(ta.UniqueID);
                        }
                    }
                    _lvmActlog.FilterByNameList(itemNames);
                }
                catch { }
            }
            else if (textBox8.Text.Contains("tags="))
            {
                List<string> itemNames = new List<string>();
                try
                {
                    string[] sTagFilter = textBox8.Text.Split('=')[1].Split(',');
                    int iMatched = 0;
                    foreach (TrX_TrackedActivity ta in _logic.ActivityHistory)
                    {
                        iMatched = 0;
                        foreach (string tag in sTagFilter)
                        {
                            if (ta.HasTag(tag))
                            {
                                iMatched++;
                            }
                        }
                        if (iMatched > 0)
                        {
                            itemNames.Add(ta.UniqueID);
                        }
                    }
                    _lvmActlog.FilterByNameList(itemNames);
                }
                catch { }
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
                _myTheme = new TrX_ThemeDark();
            }
            else
            {
                _myTheme = new TrX_ThemeLight();
            }

            _myTheme.Apply(this);
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
                sett = string.Format("TimeCap{0}", TrX_Helpers.CapitalFirstLetter(type.ToString()));
                _timeCaps[type] = value;
                _mySettings.AddOrUpdateSetting(sett, value.ToString());
            }

            _mySettings.WriteToXml();
        }

        private void ActivateStopWatchOverlay()
        {
            try
            {
                _stopwatchOverlay.Show();
            }
            catch (ObjectDisposedException)
            {
                _stopwatchOverlay = new StopWatchOverlay(this, imageList2);
            }

            _stopwatchOverlay.TopMost = true;
            _stopwatchOverlay.Opacity = _stopwatchOverlayOpacity / 100.0;
            _stopwatchOverlay.Location = new Point(Convert.ToInt32(ReadSetting("overlay.stopwatch.x", "0")), (Convert.ToInt32(ReadSetting("overlay.stopwatch.y", "0"))));
        }

        public void SaveCurrentLabRun()
        {
            if(_currentLabrunControl != null)
            {
                List<TrX_EnchantNote> notes = _currentLabrunControl.GetEnchantNotes();

                foreach (int i in _currentLabrunControl.GetSelectedEnchants())
                {
                    TrX_LabEnchant en = _logic.LabbieConnector.GetEnchantByID(i);

                    if(en != null)
                    {
                        List<TrX_EnchantNote> notes2 = _currentLabrunControl.GetEnchantNotes(en.ID);
                        _logic.CurrentLab.EnchantsTaken.Add(en);

                        DataRow row = _profitTracking.Data.NewRow();
                        row["Time"] = DateTime.Now;
                        row["Enchant"] = en.Text;
                        row["Base"] = "";
                        row["Base Cost (Exalts)"] = 0;
                        row["Sold for (Exalts)"] = 0;
                        row["Profit (Exalts)"] = 0;
                        row["State"] = "open";
                        row["Note"] = notes2.Count > 0 ? notes2[0].Note : "";
                        _profitTracking.Data.Rows.InsertAt(row, 0);
                    }
                }
                _logic.SaveCurrentLabRun();
                _logic.SaveEnchantNoteList(notes);

                if (_logic.CurrentLab != null && _logic.CurrentActivity != null && _logic.CurrentLab == _logic.CurrentActivity)
                {
                    _logic.FinishActivity(_logic.CurrentLab, null, ACTIVITY_TYPES.BREACHSTONE, DateTime.Now);
                }

                MethodInvoker mi = delegate
                {
                    panel23.Controls.Remove(_currentLabrunControl);
                };
                BeginInvoke(mi);
            }
        }

        public void SelectEnchant(int id, bool jump = false)
        {
            TrX_LabEnchant enchant = _logic.LabbieConnector.GetEnchantByID(id);
            comboBox2.SelectedItem = enchant.Text;

            if(jump)
            {
                tabControl1.SelectedIndex = 1;
            }
        }


        public void SetEnchantInfoPage(TrX_EnchantInfo enchantInfo, TrX_LabEnchant enchant)
        {
            label72.Text = enchant.Text;
            label94.Text = enchantInfo.Found.ToString();
            label95.Text = enchantInfo.Taken.ToString();
            label96.Text = enchantInfo.LastFound.Year > 2000 ? enchantInfo.LastFound.ToString() : "-";

            listBox1.Items.Clear();
            foreach (TrX_EnchantNote note in enchantInfo.EnchantNotes)
            {
                listBox1.Items.Add(string.Format("{0}: {1}{2}", DateTimeOffset.FromUnixTimeSeconds(note.LabTimeStamp).DateTime, note.Note, Environment.NewLine));
            }

            listBox2.Items.Clear();
            foreach(string s in enchantInfo.History)
            {
                listBox2.Items.Add(s);
            }

        }

        private void UpdateProfitSummary()
        {
            label110.Text = Math.Round(_profitTracking.BaseCosts, 1).ToString() + " ex.";
            label111.Text = Math.Round(_profitTracking.Income, 1).ToString() + "ex.";
            lbl_profit.Text = Math.Round(_profitTracking.Profit, 1).ToString() + "ex.";
            lbl_profit.ForeColor = _profitTracking.Profit >= 0 ? Color.LimeGreen : Color.Red;
            label108.Text = _profitTracking.BaseCount.ToString();
            label109.Text = _profitTracking.BasesSold.ToString();
        }

        private void SaveLabProfitTab()
        {
            try
            {
                _profitTracking.Calculate();
                _profitTracking.Save();
                UpdateProfitSummary();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
            }
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
            msg = string.Format("Do you really want to delete {0} activitie(s)?", lvItemsToDelete.Count);

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

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_logic.EventQueueInitialized)
            {
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
            AboutW f1 = new AboutW();
            f1.Show();
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

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            ResumeCurrentActivityOrSide();
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            PauseCurrentActivityOrSide();
        }

       

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteActivities();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new ExportActvityList(this));
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

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewActLog.SelectedIndices.Count > 0)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                TrX_TrackedActivity act = GetActivityFromListItemName(listViewActLog.Items[iIndex].Name);
                if (act != null)
                    OpenActivityDetails(act);
            }
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
                    PrepareBackupRestore(TrX_AppInfo.APPDATA_PATH + listBoxRestoreBackup.SelectedItem.ToString());
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

        private void button11_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            label63.BackColor = colorDialog1.Color;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            label63.ForeColor = colorDialog1.Color;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            UpdateTag(textBox4.Text, textBox5.Text, label63.ForeColor.ToArgb().ToString(), label63.BackColor.ToArgb().ToString(), checkBox4.Checked);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox5.Text = "";
            label63.BackColor = Color.White;
            label63.ForeColor = Color.Black;
            label63.Text = "MyCustomTag";
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            label63.Text = textBox5.Text;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            DeleteTag(textBox4.Text);
            textBox4.Text = "";
            textBox5.Text = "";
            label63.BackColor = Color.White;
            label63.ForeColor = Color.Black;
            label63.Text = "MyCustomTag";
        }

        private void button18_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you really want to delete the selected Backup?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                DeleteBackup(TrX_AppInfo.APPDATA_PATH + listBoxRestoreBackup.SelectedItem.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            textBox5.Text = textBox4.Text;
        }

        private void chatCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new ChatCommandHelp());
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
            OpenChildWindow(new ChatCommandHelp());
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
            OpenChildWindow(new AboutW());
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenChildWindow(new AboutW());
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
            Process.Start(TrX_AppInfo.WIKI_URL);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            DoSearch();
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
            catch(Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Process.Start(TrX_AppInfo.WIKI_URL_SETTINGS);
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
            sb.AppendLine("WARNING: this action will reload the entire Client.txt, keeping your manual data like custom tags and pauses.");
            sb.AppendLine("Continue? TraXile will be restarted.");

            DialogResult dr = MessageBox.Show(sb.ToString(), "Warning", MessageBoxButtons.YesNo); ;
            if (dr == DialogResult.Yes)
            {
                File.Create(TrX_AppInfo.APPDATA_PATH + @"\IS_SAFE_RELOAD");
                ReloadLogFile();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_logic.EventQueueInitialized)
            {
                if (comboBox1.SelectedItem.ToString() == "Custom")
                {
                    dateTimePicker1.Enabled = true;
                    dateTimePicker2.Enabled = true;
                }
                else
                {
                    dateTimePicker1.Enabled = false;
                    dateTimePicker2.Enabled = false;
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
            if (_stopwatchOverlay.Visible)
            {
                _stopwatchOverlay.Hide();
            }
            else
            {
                ActivateStopWatchOverlay();
            }

            stopwatchToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            _stopwatchOverlay.Opacity = trackBar1.Value > 0 ? (trackBar1.Value / 100.0) : 0;
            AddUpdateAppSettings("overlay.stopwatch.opacity", trackBar1.Value.ToString());
            _stopwatchOverlayOpacity = trackBar1.Value;
            label38.Text = _stopwatchOverlayOpacity.ToString() + "%";
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.default", checkBox2.Checked.ToString());
            _stopwatchOverlayShowDefault = checkBox2.Checked;
        }

        private void stopwatchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_stopwatchOverlay.Visible)
            {
                _stopwatchOverlay.Hide();
            }
            else
            {
                ActivateStopWatchOverlay();
            }

            stopwatchToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
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

        private void button7_Click_1(object sender, EventArgs e)
        {
            SaveCurrentLabRun();
            ResetLabRuns();
            SaveLabProfitTab();
        }
       

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            TrX_LabEnchant en;
            TrX_EnchantInfo info;
            
            en = _logic.LabbieConnector.GetEnchantObjectForText(comboBox2.SelectedItem.ToString());

            if(en != null)
            {
                info = _logic.GetEnchantInfo(en.ID);
                _selectedEnchantID = en.ID;
                SetEnchantInfoPage(info, en);
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            TrX_EnchantNote note;
            DateTime dt;
            dt = DateTime.Now;
            note = new TrX_EnchantNote(_selectedEnchantID, textBox7.Text, (int)((DateTimeOffset)dt).ToUnixTimeSeconds());
            _logic.SaveEnchantNoteList(new List<TrX_EnchantNote>() { note });
            listBox1.Items.Add(string.Format("{0}: {1}", dt, note.Note));
            textBox7.Clear();
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();
            sfd.ShowDialog();

            if(sfd.SelectedPath != null)
            {
                textBox6.Text = sfd.SelectedPath;
                _mySettings.AddOrUpdateSetting("labbie.path", sfd.SelectedPath);
                _mySettings.WriteToXml();
                if(_logic.LabbieConnector != null)
                {
                    _logic.LabbieConnector.LabbieLogPath = sfd.SelectedPath;
                    if(!_logic.LabbieConnector.IsStarted)
                    {
                        _logic.LabbieConnector.Start();
                    }
                }
            }
        }

        private void dataGridView2_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            //SaveLabProfitTab();
        }

        private void dataGridView2_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                SaveLabProfitTab();
            }
        }

        private void dataGridView2_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Time"].Value = DateTime.Now;
            e.Row.Cells["Base Cost (Exalts)"].Value = 0;
            e.Row.Cells["Sold for (Exalts)"].Value = 0;
            e.Row.Cells["Profit (Exalts)"].Value = 0;
            e.Row.Cells["State"].Value = "open";
        }

        private void button9_Click_3(object sender, EventArgs e)
        {
            SaveLabProfitTab();
        }

        private void SetProfitFilter()
        {
            string textFilter = "Enchant LIKE '%'";
            string stateFilter = "State LIKE '%'";

            if (!String.IsNullOrEmpty(textBox9.Text))
            {
                textFilter = string.Format("Enchant LIKE '%{0}%' OR Note LIKE '%{0}%' OR Base LIKE '%{0}%'", textBox9.Text);
            }

            // All
            if (radioButton1.Checked)
            {
                stateFilter = "State LIKE '%'";
                _mySettings.AddOrUpdateSetting("lab.profittracking.filter.state", "all");
            }

            // Open
            if (radioButton2.Checked)
            {
                stateFilter = "State LIKE '%open%'";
                _mySettings.AddOrUpdateSetting("lab.profittracking.filter.state", "open");
            }

            // Sold
            if (radioButton3.Checked)
            {
                stateFilter = "State LIKE '%sold%'";
                _mySettings.AddOrUpdateSetting("lab.profittracking.filter.state", "sold");
            }

            string filter = string.Format("({0}) AND ({1})", textFilter, stateFilter);
            _profitTracking.Data.DefaultView.RowFilter = filter;

            // Settings
            _mySettings.AddOrUpdateSetting("lab.profittracking.filter.text", textBox9.Text);
            _mySettings.WriteToXml();
        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            SetProfitFilter();
        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            textBox9.Text = "";
            radioButton1.Checked = true;
            SetProfitFilter();
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tabCtl1.SelectedTab = tabPage3;
            tabControl2.SelectedTab = tabPage2;
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel3.Text);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel4.Text);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Request update
            _uiFlagMapDashboard = true;
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(filterBarShown)
            {
                tableLayoutPanelMain.RowStyles[1].Height = 0;
                linkLabel5.Text = "show filters";
                filterBarShown = false;
            }
            else
            {
                tableLayoutPanelMain.RowStyles[1].Height = 130;
                linkLabel5.Text = "hide filters";
                filterBarShown = true;
            }
        }

        private void button17_Click_2(object sender, EventArgs e)
        {
            if(comboBox5.SelectedItem != null)
            {
                if (!listBox3.Items.Contains(comboBox5.SelectedItem.ToString()))
                {
                    listBox3.Items.Add(comboBox5.SelectedItem);
                }
                comboBox5.Text = "";
            }
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            if(listBox3.SelectedItem != null)
            {
                listBox3.Items.Remove(listBox3.SelectedItem);
            }
        }

        private void comboBoxStopWatchTag1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddUpdateAppSettings("overlay.stopwatch.tag1", comboBoxStopWatchTag1.SelectedItem.ToString());
            _overlayTag1 = comboBoxStopWatchTag1.SelectedItem.ToString();
        }

        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }
       

        /// <summary>
        /// Build Summary for selected activities
        /// </summary>
        private void BuildAndShowSummary()
        {
            UI.SummaryWindow summary = new UI.SummaryWindow();
            _myTheme.Apply(summary);

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
                        if(!string.IsNullOrEmpty(tag))
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
                    summary.DurationLabel.Text = string.Format("{0}d {1}h {2}m {3}s", totalDurationTS.Days, totalDurationTS.Hours, totalDurationTS.Minutes, totalDurationTS.Seconds);
                }
                else
                {
                    summary.DurationLabel.Text = totalDurationTS.ToString();
                }

                summary.AverageLabel.Text = avgDurationTS.ToString();

                // Tags
                RenderTagsForSummary(summary, dictLabelCount, false);

                // Types
                foreach(KeyValuePair<string, int> kvp in dictTypeCount)
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

            summary.Show();
        }

        private void btt_summary_Click(object sender, EventArgs e)
        {
            BuildAndShowSummary();
        }

        private void listViewNF1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            Dictionary<string, int> source = _allStatsSearchActive ? _filteredDataSourceAllStats : _dataSourceAllStats;

            if(source.Count > 0)
            {
                string longName = GetStatLongName(source.ElementAt(e.ItemIndex).Key);
                e.Item = new ListViewItem(longName);
                e.Item.SubItems.Add(source.ElementAt(e.ItemIndex).Value.ToString());
                e.Item.Name = source.ElementAt(e.ItemIndex).Key;
            }
        }

        private void listViewActLog_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btt_summary.Text = "summary (" + listViewActLog.SelectedIndices.Count + ")";
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

        private void Main_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (_minimizeToTray)
                {
                    Hide();
                    this.ShowInTaskbar = false;
                }
            }
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