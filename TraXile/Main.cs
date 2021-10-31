using log4net;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace TraXile
{
    public enum ACTIVITY_TYPES
    {
        MAP,
        HEIST,
        LABYRINTH,
        SIMULACRUM,
        BLIGHTED_MAP,
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
        CAMPAIGN
    }

    public partial class Main : Form
    {
        // DEBUG: CHANGE BEFORE RELEASE!!
        private readonly bool IS_IN_DEBUG_MODE = false;
        public bool SAFE_RELOAD_MODE;

        // App parameters
        private readonly string _dbPath;
        private readonly string _cachePath;
        private readonly string _myAppData;
        private bool _exit;
        private bool _listViewInitielaized;
        private bool _showGridInActLog;
        private bool _restoreMode;
        private bool _labDashboardUpdateRequested;
        private bool _showGridInStats;
        private bool _UpdateCheckDone;
        private bool _restoreOk = true;
        private bool _StartedFlag = false;
        private int _timeCapLab = 3600;
        private int _timeCapMap = 3600;
        private int _timeCapHeist = 3600;
        private int _actLogItemCount = 0;

        // GUI Update Flags
        private bool _uiFlagMapDashboard;
        private bool _labDashboardHideUnknown;
        private bool _uiFlagGlobalDashboard;
        private bool _uiFlagHeistDashboard;
        private bool _uiFlagActivityList;
        private bool _uiFlagStatsList;


        // Core Logic variables
        private string _currentArea;
        private string _currentInstanceEndpoint;
        private string _lastSimuEndpoint;
        private string _failedRestoreReason = "";
        private bool _eventQueueInitizalized;
        private bool _isMapZana;
        private bool _elderFightActive;
        private int _shaperKillsInFight;
        private int _nextAreaLevel;
        private int _currentAreaLevel;
        private int _lastHash = 0;
        private double _logLinesTotal;
        private double _logLinesRead;
        private bool _historyInitialized;
        private Dictionary<int, string> _dict;
        private Dictionary<string, int> _numericStats;
        private Dictionary<string, string> _statNamesLong;
        private List<string> _knownPlayerNames;
        private List<string> labs;
        private List<TrX_ActivityTag> _tags;
        private List<TrX_TrackedActivity> _eventHistory;
        private TrX_EventMapping _eventMapping;
        private TrX_DefaultMappings _defaultMappings;
        private TrX_DBManager _myDB;
        private ConcurrentQueue<TrX_TrackingEvent> _eventQueue;
        private TrX_TrackedActivity _currentActivity;
        private EVENT_TYPES _lastEventType;
        private Thread _logParseThread;
        private Thread _eventThread;
        private DateTime _inAreaSince;
        private DateTime _lastDeathTime;
        private DateTime _initStartTime;
        private DateTime _initEndTime;

        // Hideout time
        private DateTime _hoStart;
        private bool _trackingHO;

        // Other variables
        private LoadScreen _loadScreenWindow;
        private BindingList<string> _backups;
        private Dictionary<string, Label> _tagLabels, _tagLabelsConfig;
        private List<string> _parsedActivities;
        private readonly TrX_SettingsManager _mySettings;
        private TrX_ListViewManager _lvmStats, _lvmActlog;
        private TrX_Theme _myTheme;
        private ILog _log;
        private bool _showHideoutInPie;
        private bool _uiFlagBossDashboard;

        /// <summary>
        /// Setting Property for LogFilePath
        /// </summary>
        public string SettingPoeLogFilePath
        {
            get { return ReadSetting("poe_logfile_path", null); }
            set { AddUpdateAppSettings("poe_logfile_path", value); }
        }

        /// <summary>
        /// Tag list property
        /// </summary>
        public List<TrX_ActivityTag> Tags
        {
            get { return _tags; }
            set { _tags = value; }
        }


        /// <summary>
        /// Main Window Constructor
        /// </summary>
        public Main()
        {
            if (File.Exists(Application.StartupPath + @"\DEBUG_MODE_ON.txt"))
            {
                IS_IN_DEBUG_MODE = true;
            }

            if (IS_IN_DEBUG_MODE)
            {
                _myAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + APPINFO.NAME + "_Debug";
                Text += "!!!!! DEBUG MODE ON !!!!!!";
            }
            else
            {
                _myAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + APPINFO.NAME;
            }

            if (File.Exists(_myAppData + @"\IS_SAFE_RELOAD"))
            {
                SAFE_RELOAD_MODE = true;
                File.Delete(_myAppData + @"\IS_SAFE_RELOAD");
            }

            _dbPath = _myAppData + @"\data.db";
            _cachePath = _myAppData + @"\stats.cache";
            _mySettings = new TrX_SettingsManager(_myAppData + @"\config.xml");

            if (!Directory.Exists(_myAppData))
            {
                Directory.CreateDirectory(_myAppData);
            }

            Visible = false;
            InitializeComponent();

            Init();

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
        /// Check if a new version is available on GitHub and ask for update.
        /// </summary>
        /// <param name="b_notify_ok"></param>
        private void CheckForUpdate(bool b_notify_ok = false)
        {
            try
            {
                string updateURL = ReadSetting("update_check_url", "https://dermow.github.io/traxile-update-info/version.xml");
                string updateXMLNode = ReadSetting("update_path", "latest");

                WebClient webClient = new WebClient();
                Uri uri = new Uri(updateURL);
                string releases = webClient.DownloadString(uri);

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(releases);

                string sVersion;
                sVersion = xml.SelectSingleNode(string.Format("/version/{0}", updateXMLNode)).InnerText;

                StringBuilder sbChanges = new StringBuilder();

                foreach (XmlNode xn in xml.SelectNodes("/version/changes/chg"))
                {
                    sbChanges.AppendLine(" - " + xn.InnerText);
                }

                _log.Info(string.Format("UpdateCheck -> My version: {0}, Remote version: {1}", APPINFO.VERSION, sVersion));

                int MyMajor = Convert.ToInt32(APPINFO.VERSION.Split('.')[0]);
                int MyMinor = Convert.ToInt32(APPINFO.VERSION.Split('.')[1]);
                int MyPatch = Convert.ToInt32(APPINFO.VERSION.Split('.')[2]);

                int RemoteMajor = Convert.ToInt32(sVersion.Split('.')[0]);
                int RemoteMinor = Convert.ToInt32(sVersion.Split('.')[1]);
                int RemotePatch = Convert.ToInt32(sVersion.Split('.')[2]);

                bool bUpdate = false;
                if (RemoteMajor > MyMajor)
                {
                    bUpdate = true;
                }
                else if (RemoteMajor == MyMajor && RemoteMinor > MyMinor)
                {
                    bUpdate = true;
                }
                else if (RemoteMajor == MyMajor && RemoteMinor == MyMinor && RemotePatch > MyPatch)
                {
                    bUpdate = true;
                }

                if (bUpdate)
                {
                    _log.Info("UpdateCheck -> New version available");
                    StringBuilder sbMessage = new StringBuilder();
                    sbMessage.AppendLine(string.Format("There is a new version for TraXile available ({0} => {1})", APPINFO.VERSION, sVersion));
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
                        Process.Start(psi);
                    }
                }
                else
                {
                    _log.Info("UpdateCheck -> Already up to date :)");
                    if (b_notify_ok)
                        MessageBox.Show("Your version: " + APPINFO.VERSION
                            + Environment.NewLine + "Latest version: " + sVersion + Environment.NewLine + Environment.NewLine
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
        /// ONLY CALL ONCE! S
        /// </summary>
        private void Init()
        {
            Opacity = 0;

            _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _log.Info("Application started");

            _mySettings.LoadFromXml();

            _lvmStats = new TrX_ListViewManager(listViewStats);
            _lvmActlog = new TrX_ListViewManager(listViewActLog);

            _eventMapping = new TrX_EventMapping();
            _defaultMappings = new TrX_DefaultMappings();
            _parsedActivities = new List<string>();

            SaveVersion();
            CheckForUpdate();
            _UpdateCheckDone = true;
            ReadSettings();

            comboBoxShowMaxItems.SelectedItem = ReadSetting("actlog.maxitems", "500");

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

            listViewActLog.Columns[0].Width = 120;
            listViewActLog.Columns[1].Width = 50;
            listViewActLog.Columns[2].Width = 110;
            listViewActLog.Columns[3].Width = 100;
            listViewActLog.Columns[4].Width = 50;
            listViewStats.Columns[0].Width = 500;
            listViewStats.Columns[1].Width = 300;

            listViewTop10Maps.Columns[0].Width = 300;

            chartStats.ChartAreas[0].AxisX.LineColor = Color.Red;
            chartStats.ChartAreas[0].AxisY.LineColor = Color.Red;
            chartStats.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chartStats.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chartStats.ChartAreas[0].AxisX.Interval = 1;
            chartStats.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            chartStats.ChartAreas[0].AxisX.IntervalOffset = 1;
            chartStats.Series[0].XValueType = ChartValueType.DateTime;
            chartStats.Series[0].LabelForeColor = Color.White;
            chartStats.Series[0].LabelBackColor = Color.Black;
            chartStats.Series[0].LabelBorderColor = Color.Black;
            chartStats.Series[0].Color = Color.White;
            chartStats.Legends[0].Enabled = false;
            chartStats.Series[0].SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            chartStats.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chartStats.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

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

            var ca = chartStats.ChartAreas["ChartArea1"].CursorX;
            ca.IsUserEnabled = true;
            ca.IsUserSelectionEnabled = true;

            textBoxLogFilePath.Text = ReadSetting("PoELogFilePath");
            textBoxLogFilePath.Enabled = false;

            comboBoxTimeRangeStats.SelectedIndex = 1;

            _dict = new Dictionary<int, string>();
            _eventQueue = new ConcurrentQueue<TrX_TrackingEvent>();
            _eventHistory = new List<TrX_TrackedActivity>();
            _knownPlayerNames = new List<string>();
            _backups = new BindingList<string>();
            _currentArea = "-";
            _inAreaSince = DateTime.Now;
            _eventQueueInitizalized = false;
            _tagLabels = new Dictionary<string, Label>();
            _tagLabelsConfig = new Dictionary<string, Label>();
            _lastSimuEndpoint = "";
            _tags = new List<TrX_ActivityTag>();

            Text = APPINFO.NAME;

            if (IS_IN_DEBUG_MODE)
            {
                Text += " ---> !!!!! DEBUG MODE !!!!!";
            }

            _initStartTime = DateTime.Now;

            if (String.IsNullOrEmpty(SettingPoeLogFilePath))
            {
                FileSelectScreen fs = new FileSelectScreen(this)
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };
                fs.ShowDialog();
            }

            _loadScreenWindow = new LoadScreen
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None
            };
            _loadScreenWindow.Show(this);

            _myDB = new TrX_DBManager(_myAppData + @"\data.db", ref _log);
            InitDefaultTags();

            _lastEventType = EVENT_TYPES.APP_STARTED;
            InitNumStats();

            foreach (KeyValuePair<string, int> kvp in _numericStats)
            {
                ListViewItem lvi = new ListViewItem(GetStatLongName(kvp.Key));
                lvi.SubItems.Add("0");
                _lvmStats.AddLvItem(lvi, "stats_" + kvp.Key);
            }

            _eventQueue.Enqueue(new TrX_TrackingEvent(EVENT_TYPES.APP_STARTED) { EventTime = DateTime.Now, LogLine = "Application started." });

            ReadStatsCache();
            ReadKnownPlayers();
            LoadCustomTags();
            ResetMapHistory();
            LoadLayout();

            if (!_historyInitialized)
            {
                ReadActivityLogFromSQLite();
            }

            // Thread for Log Parsing and Enqueuing
            _logParseThread = new Thread(new ThreadStart(LogParsing))
            {
                IsBackground = true
            };
            _logParseThread.Start();

            // Thread for Queue processing / Dequeuing
            _eventThread = new Thread(new ThreadStart(EventHandling))
            {
                IsBackground = true
            };
            _eventThread.Start();

            // Request initial Dashboard update
            _labDashboardUpdateRequested = true;
            _uiFlagMapDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagStatsList = true;
            _uiFlagActivityList = true;
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
        ///  Initialize all default tags
        /// </summary>
        private void InitDefaultTags()
        {
            List<TrX_ActivityTag> tmpTags;
            tmpTags = new List<TrX_ActivityTag>
            {
                new TrX_ActivityTag("blight") { BackColor = Color.LightGreen, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("delirium") { BackColor = Color.WhiteSmoke, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("einhar") { BackColor = Color.Red, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("incursion") { BackColor = Color.GreenYellow, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("syndicate") { BackColor = Color.Gold, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("zana") { BackColor = Color.Blue, ForeColor = Color.White, ShowInListView = true },
                new TrX_ActivityTag("niko") { BackColor = Color.OrangeRed, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("zana-map") { BackColor = Color.Blue, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("expedition") { BackColor = Color.Turquoise, ForeColor = Color.Black, ShowInListView = true },
                new TrX_ActivityTag("rog") { BackColor = Color.Turquoise, ForeColor = Color.Black },
                new TrX_ActivityTag("gwennen") { BackColor = Color.Turquoise, ForeColor = Color.Black },
                new TrX_ActivityTag("dannig") { BackColor = Color.Turquoise, ForeColor = Color.Black },
                new TrX_ActivityTag("tujen") { BackColor = Color.Turquoise, ForeColor = Color.Black },
                new TrX_ActivityTag("karst") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("tibbs") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("isla") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("tullina") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("niles") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("nenet") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("vinderi") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("gianna") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("huck") { BackColor = Color.IndianRed, ForeColor = Color.Black }
            };

            foreach (TrX_ActivityTag tag in tmpTags)
            {
                try
                {
                    _myDB.DoNonQuery("insert into tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type, tag_show_in_lv) values " +
                                  "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'default', " + (tag.ShowInListView ? "1" : "0") + ")", false);
                    _log.Info("Default tag '" + tag.ID + "' added to database");
                }
                catch (SqliteException e)
                {
                    if (e.Message.Contains("SQLite Error 19"))
                    {
                        _log.Info("Default tag '" + tag.ID + "' already in database, nothing todo");
                    }
                    else
                    {
                        _log.Error(e.ToString());
                    }
                }

            }
        }

        /// <summary>
        /// Load user created tags
        /// </summary>
        private void LoadCustomTags()
        {
            SqliteDataReader sqlReader;
            sqlReader = _myDB.GetSQLReader("SELECT * FROM tx_tags ORDER BY tag_id DESC");

            while (sqlReader.Read())
            {
                string sID = sqlReader.GetString(0);
                string sType = sqlReader.GetString(4);
                TrX_ActivityTag tag = new TrX_ActivityTag(sID, sType != "custom")
                {
                    DisplayName = sqlReader.GetString(1),
                    BackColor = Color.FromArgb(Convert.ToInt32(sqlReader.GetString(2))),
                    ForeColor = Color.FromArgb(Convert.ToInt32(sqlReader.GetString(3))),
                    ShowInListView = sqlReader.GetInt32(5) == 1
                };
                _tags.Add(tag);
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

            for (int i = 0; i < _tags.Count; i++)
            {
                TrX_ActivityTag tag = _tags[i];
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

            for (int i = 0; i < _tags.Count; i++)
            {
                TrX_ActivityTag tag = _tags[i];
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
                    if (_currentActivity != null)
                    {
                        TrX_TrackedActivity mapToCheck = _isMapZana ? _currentActivity.ZanaMap : _currentActivity;

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
        /// Find matching tag for given display name
        /// </summary>
        /// <param name="s_display_name"></param>
        /// <returns></returns>
        public TrX_ActivityTag GetTagByDisplayName(string s_display_name)
        {
            foreach (TrX_ActivityTag t in _tags)
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
                if (_currentActivity != null)
                {
                    if (_isMapZana && _currentActivity.ZanaMap != null)
                    {
                        if (_currentActivity.ZanaMap.HasTag(tag.ID))
                        {
                            _currentActivity.ZanaMap.RemoveTag(tag.ID);
                        }
                        else
                        {
                            _currentActivity.ZanaMap.AddTag(tag.ID);
                        }
                    }
                    else
                    {
                        if (_currentActivity.HasTag(tag.ID))
                        {
                            _currentActivity.RemoveTag(tag.ID);
                        }
                        else
                        {
                            _currentActivity.AddTag(tag.ID);
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
            _eventQueueInitizalized = false;
            _lastHash = 0;
            SaveStatsCache();
            Application.Restart();
        }

        /// <summary>
        /// Initialize the stats
        /// </summary>
        private void InitNumStats()
        {
            _numericStats = new Dictionary<string, int>
            {
                { "AreaChanges", 0 },
                { "BaranStarted", 0 },
                { "BaranKilled", 0 },
                { "CampaignFinished", 0 },
                { "CatarinaTried", 0 },
                { "CatarinaKilled", 0 },
                { "TotalKilledCount", 0 },
                { "DroxStarted", 0 },
                { "DroxKilled", 0 },
                { "EinharCaptures", 0 },
                { "ElderTried", 0 },
                { "ElderKilled", 0 },
                { "ExpeditionEncounters", 0 },
                { "ExpeditionEncounters_Rog", 0 },
                { "ExpeditionEncounters_Tujen", 0 },
                { "ExpeditionEncounters_Gwennen", 0 },
                { "ExpeditionEncounters_Dannig", 0 },
                { "HideoutTimeSec", 0 },
                { "HighestLevel", 0 },
                { "HunterKilled", 0 },
                { "HunterStarted", 0 },
                { "LabsFinished", 0 },
                { "LabsStarted", 0 },
                { "LevelUps", 0 },
                { "MavenStarted", 0 },
                { "MavenKilled", 0 },
                { "TotalMapsDone", 0 },
                { "TotalHeistsDone", 0 },
                { "ShaperTried", 0 },
                { "ShaperKilled", 0 },
                { "SimulacrumCleared", 0 },
                { "SimulacrumStarted", 0 },
                { "SirusStarted", 0 },
                { "SirusKilled", 0 },
                { "TemplesDone", 0 },
                { "TrialMasterStarted", 0 },
                { "TrialMasterKilled", 0 },
                { "TrialMasterTookReward", 0 },
                { "TrialMasterVictory", 0 },
                { "TrialMasterSuccess", 0 },
                { "VeritaniaKilled", 0 },
                { "VeritaniaStarted", 0 },
            };

            _statNamesLong = new Dictionary<string, string>
            {
                { "TotalMapsDone", "Total maps done" },
                { "TotalHeistsDone", "Total heists done" },
                { "ElderKilled", "Elder killed" },
                { "ShaperKilled", "Shaper killed" },
                { "SirusStarted", "Sirus tried" },
                { "SirusKilled", "Sirus killed" },
                { "HunterKilled", "Hunter killed (not reliable*)" },
                { "HunterStarted", "Hunter tried" },
                { "VeritaniaKilled", "Veritania killed (not reliable*)" },
                { "VeritaniaStarted", "Veritania tried" },
                { "BaranStarted", "Baran tried" },
                { "BaranKilled", "Baran killed (not reliable*)" },
                { "DroxStarted", "Drox tried" },
                { "DroxKilled", "Drox killed (not reliable*)" },
                { "HighestLevel", "Highest level reached" },
                { "TrialMasterStarted", "Trialmaster-Fight tried" },
                { "TrialMasterKilled", "Trialmaster killed" },
                { "MavenStarted", "Maven tried" },
                { "MavenKilled", "Maven killed" },
                { "TotalKilledCount", "Death count" },
                { "EinharCaptures", "Einhar beasts captured" },
                { "TrialMasterTookReward", "Ultimatum: took rewards" },
                { "TrialMasterVictory", "Ultimatum: cleared all rounds" },
                { "TrialMasterSuccess", "Ultimatum: did not fail" },
                { "ShaperTried", "Shaper tried" },
                { "ElderTried", "Elder tried" },
                { "CatarinaTried", "Catarina tried" },
                { "CatarinaKilled", "Catarina killed" },
                { "LevelUps", "Level Ups" },
                { "SimulacrumStarted", "Simulacrum started" },
                { "SimulacrumCleared", "Simulacrum 100% done" },
                { "LabsFinished", "Finished labs" },
                { "TemplesDone", "Temples done" },
                { "LabsStarted", "Labs started" },
                { "ExpeditionEncounters", "Expedition encounters" },
                { "ExpeditionEncounters_Rog", "Expedition encounters: Rog" },
                { "ExpeditionEncounters_Tujen", "Expedition encounters: Tujen" },
                { "ExpeditionEncounters_Gwennen", "Expedition encounters: Gwennen" },
                { "ExpeditionEncounters_Dannig", "Expedition encounters: Dannig" },
                { "HideoutTimeSec", "Hideout time" },
                { "CampaignFinished", "Campaign finished" },
            };

            labs = new List<string>
            {
                "Unknown",
                "The Labyrinth",
                "The Merciless Labyrinth",
                "The Cruel Labyrinth",
                "Uber-Lab",
                "Advanced Uber-Lab"
            };

            foreach (string s in _defaultMappings.HEIST_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!_numericStats.ContainsKey("HeistsFinished_" + sName))
                    _numericStats.Add("HeistsFinished_" + sName, 0);
                if (!_statNamesLong.ContainsKey("HeistsFinished_" + sName))
                    _statNamesLong.Add("HeistsFinished_" + sName, "Heists done: " + sName);
            }

            foreach (string s in labs)
            {
                string sName = s.Replace("'", "");
                if (!_numericStats.ContainsKey("LabsCompleted_" + sName))
                    _numericStats.Add("LabsCompleted_" + sName, 0);
                if (!_statNamesLong.ContainsKey("LabsCompleted_" + sName))
                    _statNamesLong.Add("LabsCompleted_" + sName, "Labs completed: " + sName);
            }

            foreach (string s in _defaultMappings.MAP_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!_numericStats.ContainsKey("MapsFinished_" + sName))
                    _numericStats.Add("MapsFinished_" + sName, 0);
                if (!_statNamesLong.ContainsKey("MapsFinished_" + sName))
                    _statNamesLong.Add("MapsFinished_" + sName, "Maps done: " + sName);
            }

            for (int i = 0; i <= 16; i++)
            {
                string sShort = "MapTierFinished_T" + i.ToString();
                string sLong = i > 0 ? ("Maps done: T" + i.ToString()) : "Maps done: Tier unknown";
                if (!_numericStats.ContainsKey(sShort))
                    _numericStats.Add(sShort, 0);
                if (!_statNamesLong.ContainsKey(sShort))
                    _statNamesLong.Add(sShort, sLong);
            }

            foreach (string s in _defaultMappings.SIMU_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!_numericStats.ContainsKey("SimulacrumFinished_" + sName))
                    _numericStats.Add("SimulacrumFinished_" + sName, 0);
                if (!_statNamesLong.ContainsKey("SimulacrumFinished_" + sName))
                    _statNamesLong.Add("SimulacrumFinished_" + sName, "Simulacrum done: " + sName);
            }
        }


        /// <summary>
        /// Get longname for a stat
        /// </summary>
        /// <param name="s_key"></param>
        /// <returns></returns>
        private string GetStatLongName(string s_key)
        {
            if (_statNamesLong.ContainsKey(s_key))
            {
                return _statNamesLong[s_key];
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
            _myDB.DoNonQuery("drop table tx_stats");
            _myDB.DoNonQuery("create table if not exists tx_stats " +
                "(timestamp int, " +
                "stat_name text, " +
                "stat_value int)");

            InitNumStats();
            SaveStatsCache();

            _log.Info("Stats cleared.");
        }

        /// <summary>
        /// Clear the activity log
        /// </summary>
        private void ClearActivityLog()
        {
            _eventHistory.Clear();
            listViewActLog.Items.Clear();
            _myDB.DoNonQuery("drop table tx_activity_log");
            _myDB.DoNonQuery("create table if not exists tx_activity_log " +
                 "(timestamp int, " +
                 "act_type text, " +
                 "act_area text, " +
                 "act_stopwatch int, " +
                 "act_deathcounter int," +
                 "act_ulti_rounds int," +
                 "act_is_zana int," +
                 "act_tags" + ")");

            InitNumStats();
            SaveStatsCache();

            _log.Info("Activity log cleared.");
        }

        /// <summary>
        /// Read existing backups to list in GUI
        /// </summary>
        private void ReadBackupList()
        {
            if (Directory.Exists(_myAppData + @"\backups"))
            {
                foreach (string s in Directory.GetDirectories(_myAppData + @"\backups"))
                {
                    foreach (string s2 in Directory.GetDirectories(s))
                    {
                        string s_name = s2.Replace(_myAppData, "");

                        if (!_backups.Contains(s_name))
                            _backups.Add(s_name);
                    }
                }
            }
        }

        /// <summary>
        /// Track known players. Needed to find out if death events are for your own 
        /// char or not. If a player name enters your area, It could not be you :)
        /// </summary>
        /// <param name="s_name"></param>
        private void AddKnownPlayerIfNotExists(string s_name)
        {
            if (!_knownPlayerNames.Contains(s_name))
            {
                _knownPlayerNames.Add(s_name);
                _myDB.DoNonQuery("insert into tx_known_players (player_name) VALUES ('" + s_name + "')");
                _log.Info("KnownPlayerAdded -> name: " + s_name);
            }
        }

        /// <summary>
        /// Delete entry from activity log
        /// </summary>
        /// <param name="l_timestamp"></param>
        private void DeleteActLogEntry(long l_timestamp)
        {
            _myDB.DoNonQuery("delete from tx_activity_log where timestamp = " + l_timestamp.ToString());
        }

        /// <summary>
        /// Read list of known players
        /// </summary>
        private void ReadKnownPlayers()
        {
            SqliteDataReader sqlReader;
            sqlReader = _myDB.GetSQLReader("SELECT * FROM tx_known_players");

            while (sqlReader.Read())
            {
                _knownPlayerNames.Add(sqlReader.GetString(0));
            }
        }

        /// <summary>
        /// Save activity to database
        /// </summary>
        /// <param name="i_ts"></param>
        /// <param name="s_type"></param>
        /// <param name="s_area"></param>
        /// <param name="i_area_level"></param>
        /// <param name="i_stopwatch"></param>
        /// <param name="i_death_counter"></param>
        /// <param name="i_ulti_rounds"></param>
        /// <param name="b_zana"></param>
        /// <param name="l_tags"></param>
        private void SaveToActivityLog(long i_ts, string s_type, string s_area, int i_area_level, int i_stopwatch, int i_death_counter, int i_ulti_rounds, bool b_zana, List<string> l_tags, bool b_success, int i_pause_time = 0)
        {
            //replace ' in area
            s_area = s_area.Replace("'", "");
            string sTags = "";

            for (int i = 0; i < l_tags.Count; i++)
            {
                sTags += l_tags[i];
                if (i < (l_tags.Count - 1))
                    sTags += "|";
            }

            _myDB.DoNonQuery("insert into tx_activity_log " +
               "(timestamp, " +
               "act_type, " +
               "act_area, " +
               "act_area_level, " +
               "act_stopwatch, " +
               "act_deathcounter, " +
               "act_ulti_rounds," +
               "act_is_zana," +
               "act_tags," +
               "act_success," +
               "act_pause_time) VALUES (" +
               i_ts.ToString()
                 + ", '" + s_type
                 + "', '" + s_area
                 + "', '" + i_area_level.ToString()
                 + "', " + i_stopwatch
                 + ", " + i_death_counter
                 + ", " + i_ulti_rounds
                 + ", " + (b_zana ? "1" : "0")
                 + ", '" + sTags + "'"
                 + ", " + (b_success ? "1" : "0")
                 + ", " + i_pause_time.ToString()
                 + ")");

            _parsedActivities.Add(i_ts.ToString() + "_" + s_area);
        }

        /// <summary>
        /// get activity type object from string
        /// </summary>
        /// <param name="s_type"></param>
        /// <returns></returns>
        private ACTIVITY_TYPES GetActTypeFromString(string s_type)
        {
            switch (s_type)
            {
                case "map":
                    return ACTIVITY_TYPES.MAP;
                case "heist":
                    return ACTIVITY_TYPES.HEIST;
                case "simulacrum":
                    return ACTIVITY_TYPES.SIMULACRUM;
                case "blighted_map":
                    return ACTIVITY_TYPES.BLIGHTED_MAP;
                case "labyrinth":
                    return ACTIVITY_TYPES.LABYRINTH;
                case "delve":
                    return ACTIVITY_TYPES.DELVE;
                case "temple":
                    return ACTIVITY_TYPES.TEMPLE;
                case "maven_invitation":
                    return ACTIVITY_TYPES.MAVEN_INVITATION;
                case "atziri":
                    return ACTIVITY_TYPES.ATZIRI;
                case "uber_atziri":
                    return ACTIVITY_TYPES.UBER_ATZIRI;
                case "elder_fight":
                    return ACTIVITY_TYPES.ELDER_FIGHT;
                case "shaper_fight":
                    return ACTIVITY_TYPES.SHAPER_FIGHT;
                case "maven_fight":
                    return ACTIVITY_TYPES.MAVEN_FIGHT;
                case "sirus_fight":
                    return ACTIVITY_TYPES.SIRUS_FIGHT;
                case "campaign":
                    return ACTIVITY_TYPES.CAMPAIGN;
            }
            return ACTIVITY_TYPES.MAP;
        }

        /// <summary>
        /// Read the activity log from Database
        /// </summary>
        private void ReadActivityLogFromSQLite()
        {
            SqliteDataReader sqlReader;
            sqlReader = _myDB.GetSQLReader("SELECT * FROM tx_activity_log ORDER BY timestamp DESC");

            string[] arrTags;

            while (sqlReader.Read())
            {
                TimeSpan ts = TimeSpan.FromSeconds(sqlReader.GetInt32(3));
                string sType = sqlReader.GetString(1);
                ACTIVITY_TYPES aType = GetActTypeFromString(sType);

                TrX_TrackedActivity map = new TrX_TrackedActivity
                {
                    Started = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(sqlReader.GetInt32(0)).ToLocalTime(),
                    TimeStamp = sqlReader.GetInt32(0),
                    CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds),
                    TotalSeconds = Convert.ToInt32(ts.TotalSeconds),
                    Type = aType,
                    Area = sqlReader.GetString(2),
                    DeathCounter = sqlReader.GetInt32(4),
                    TrialMasterCount = sqlReader.GetInt32(5),
                    PausedTime = sqlReader.GetDouble(10)
                };

                try
                {
                    map.AreaLevel = sqlReader.GetInt32(8);
                }
                catch
                {
                    map.AreaLevel = 0;
                }

                try
                {
                    string sTags = sqlReader.GetString(7);
                    arrTags = sTags.Split('|');
                }
                catch
                {
                    arrTags = new string[0];
                }

                for (int i = 0; i < arrTags.Length; i++)
                {
                    map.AddTag(arrTags[i]);
                }

                if (!_parsedActivities.Contains(map.UniqueID))
                {
                    _eventHistory.Add(map);
                    _parsedActivities.Add(map.UniqueID);
                }


            }
            _historyInitialized = true;
            ResetMapHistory();
        }

        /// <summary>
        /// Main method for log parsing thread
        /// </summary>
        private void LogParsing()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (SettingPoeLogFilePath != null)
                {
                    ParseLogFile();

                }
            }
        }

        /// <summary>
        /// Get line count from Client.txt. Used for progress calculation
        /// </summary>
        /// <returns></returns>
        private int GetLogFileLineCount()
        {
            int iCount = 0;
            FileStream fs1 = new FileStream(SettingPoeLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TextReader reader1 = new StreamReader(fs1);
            while ((reader1.ReadLine()) != null)
            {
                iCount++;
            }
            reader1.Close();
            return iCount;
        }


        /// <summary>
        /// Parse the logfile
        /// </summary>
        private void ParseLogFile()
        {
            _log.Info("Started logfile parsing. Last hash was " + _lastHash.ToString());

            _logLinesTotal = Convert.ToDouble(GetLogFileLineCount());

            var fs = new FileStream(SettingPoeLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bool bNewContent = _lastHash == 0;

            using (StreamReader reader = new StreamReader(fs))
            {
                string line;
                int lineHash = 0;

                // Keep file open
                while (!_exit)
                {
                    line = reader.ReadLine();

                    if (line == null)
                    {
                        if (!_eventQueueInitizalized)
                        {
                            _currentActivity = null;
                            _isMapZana = false;
                            _initEndTime = DateTime.Now;
                            TimeSpan tsInitDuration = (_initEndTime - _initStartTime);
                            _eventQueue.Enqueue(new TrX_TrackingEvent(EVENT_TYPES.APP_READY)
                            {
                                EventTime = DateTime.Now,
                                LogLine = "Application initialized in "
                                  + Math.Round(tsInitDuration.TotalSeconds, 2) + " seconds."
                            });
                            _lastHash = lineHash;
                            SAFE_RELOAD_MODE = false;
                        }
                        _eventQueueInitizalized = true;
                        bNewContent = true;

                        Thread.Sleep(100);
                        continue;
                    }

                    lineHash = line.GetHashCode();

                    if (_dict.ContainsKey(lineHash))
                        continue;

                    if (lineHash == _lastHash || _lastHash == 0)
                    {
                        bNewContent = true;
                    }

                    if (!bNewContent)
                    {
                        _logLinesRead++;
                        continue;
                    }

                    _lastHash = lineHash;

                    foreach (KeyValuePair<string, EVENT_TYPES> kv in _eventMapping.MAP)
                    {
                        if (line.Contains(kv.Key))
                        {
                            if (!_dict.ContainsKey(lineHash))
                            {
                                TrX_TrackingEvent ev = new TrX_TrackingEvent(kv.Value)
                                {
                                    LogLine = line
                                };
                                try
                                {
                                    ev.EventTime = DateTime.Parse(line.Split(' ')[0] + " " + line.Split(' ')[1]);
                                }
                                catch
                                {
                                    ev.EventTime = DateTime.Now;
                                }
                                _dict.Add(lineHash, "init");

                                if (!_eventQueueInitizalized)
                                {
                                    HandleSingleEvent(ev, true);
                                }
                                else
                                {
                                    _eventQueue.Enqueue(ev);
                                }

                            }
                        }
                    }
                    _logLinesRead++;
                }
            }
        }

        /// <summary>
        /// Handle events - Read Queue
        /// </summary>
        private void EventHandling()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (_eventQueueInitizalized)
                {
                    while (_eventQueue.TryDequeue(out TrX_TrackingEvent deqEvent))
                    {
                        HandleSingleEvent(deqEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Check if a given area is a Map.
        /// </summary>
        /// <param name="sArea"></param>
        /// <param name="sSourceArea"></param>
        /// <returns></returns>
        private bool CheckIfAreaIsMap(string sArea, string sSourceArea = "")
        {
            // Laboratory could be map or heist...
            if (sArea == "Laboratory")
            {
                if (sSourceArea == "The Rogue Harbour")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            foreach (string s in _defaultMappings.MAP_AREAS)
            {
                if (s.Trim().Equals(sArea.Trim()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a given area is a Heist
        /// </summary>
        /// <param name="sArea"></param>
        /// <param name="sSourceArea"></param>
        /// <returns></returns>
        private bool CheckIfAreaIsHeist(string sArea, string sSourceArea = "")
        {
            // Laboratory could be map or heist...
            if (sArea == "Laboratory")
            {
                if (sSourceArea == "The Rogue Harbour")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            foreach (string s in _defaultMappings.HEIST_AREAS)
            {
                if (s.Trim().Equals(sArea.Trim()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Process a command entered via ingame chat
        /// </summary>
        /// <param name="s_command"></param>
        private void HandleChatCommand(string s_command)
        {
            _log.Info("ChatCommand -> " + s_command);
            string[] spl = s_command.Split(' ');
            string sMain = "";
            string sArgs = "";

            sMain = spl[0];

            if (spl.Length > 1)
            {
                sArgs = spl[1];
            }

            TrX_TrackedActivity currentAct = null;
            if (_currentActivity != null)
            {
                if (_isMapZana && _currentActivity.ZanaMap != null)
                {
                    currentAct = _currentActivity.ZanaMap;
                }
                else
                {
                    currentAct = _currentActivity;
                }
            }

            switch (sMain)
            {
                case "tag":
                    if (currentAct != null)
                    {
                        MethodInvoker mi = delegate
                        {
                            AddTagAutoCreate(sArgs, currentAct);
                        };
                        Invoke(mi);
                    }
                    break;
                case "untag":
                    if (currentAct != null)
                    {
                        MethodInvoker mi = delegate
                        {
                            RemoveTagFromActivity(sArgs, currentAct);
                        };
                        Invoke(mi);

                    }
                    break;
                case "pause":
                    if (_currentActivity != null)
                    {
                        if (_isMapZana && _currentActivity.ZanaMap != null)
                        {
                            if (!_currentActivity.ZanaMap.ManuallyPaused)
                            {
                                _currentActivity.ZanaMap.Pause();
                            }
                        }
                        else
                        {
                            if (!_currentActivity.ManuallyPaused)
                            {
                                _currentActivity.Pause();
                            }
                        }
                    }
                    break;
                case "resume":
                    if (_currentActivity != null)
                    {
                        if (_isMapZana && _currentActivity.ZanaMap != null)
                        {
                            if (_currentActivity.ZanaMap.ManuallyPaused)
                            {
                                _currentActivity.ZanaMap.Resume();
                            }
                        }
                        else
                        {
                            if (_currentActivity.ManuallyPaused)
                            {
                                _currentActivity.Resume();
                            }
                        }
                    }
                    break;
                case "finish":
                    if (currentAct != null && !_isMapZana)
                    {
                        MethodInvoker mi = delegate
                        {
                            FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, DateTime.Now);
                        };
                        Invoke(mi);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle area change. Core logic for nearly all tracking
        /// </summary>
        /// <param name="ev"></param>
        private void HandleAreaChangeEvent(TrX_TrackingEvent ev)
        {
            string sSourceArea = _currentArea;
            string sTargetArea = GetAreaNameFromEvent(ev);
            string sAreaName = GetAreaNameFromEvent(ev);
            bool bSourceAreaIsMap = CheckIfAreaIsMap(sSourceArea);
            bool bTargetAreaIsMap = CheckIfAreaIsMap(sTargetArea, sSourceArea);
            bool bTargetAreaIsHeist = CheckIfAreaIsHeist(sTargetArea, sSourceArea);
            bool bTargetAreaIsSimu = false;
            bool bTargetAreaMine = _defaultMappings.DELVE_AREAS.Contains(sTargetArea);
            bool bTargetAreaTemple = _defaultMappings.TEMPLE_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsLab = _defaultMappings.LAB_START_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsMI = _defaultMappings.MAVEN_INV_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsAtziri = _defaultMappings.ATZIRI_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsUberAtziri = _defaultMappings.UBER_ATZIRI_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsElder = _defaultMappings.ELDER_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsShaper = _defaultMappings.SHAPER_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsSirusFight = _defaultMappings.SIRUS_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsMavenFight = _defaultMappings.MAVEN_FIGHT_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsCampaign = _defaultMappings.CAMPAIGN_AREAS.Contains(sTargetArea);
            long lTS = ((DateTimeOffset)ev.EventTime).ToUnixTimeSeconds();

            _inAreaSince = ev.EventTime;

            IncrementStat("AreaChanges", ev.EventTime, 1);

            // Track the very first activity
            if ((!sTargetArea.Contains("Hideout")) && (!_defaultMappings.CAMP_AREAS.Contains(sTargetArea)))
            {
                _StartedFlag = false;
            }

            // Hideout?
            if (sTargetArea.Contains("Hideout"))
            {
                if (!_trackingHO)
                {
                    _hoStart = ev.EventTime;
                    _trackingHO = true;
                }
            }
            else
            {
                if (_trackingHO)
                {
                    int hoSeconds;
                    hoSeconds = Convert.ToInt32((ev.EventTime - _hoStart).TotalSeconds);
                    IncrementStat("HideoutTimeSec", ev.EventTime, hoSeconds);
                    _trackingHO = false;
                }
            }

            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
            {
                _currentActivity.LastEnded = ev.EventTime;
            }

            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.DELVE)
            {
                _currentActivity.LastEnded = ev.EventTime;
            }

            //Simu?
            if (_defaultMappings.SIMU_AREAS.Contains(sAreaName))
            {
                bTargetAreaIsSimu = true;
                if (_currentInstanceEndpoint != _lastSimuEndpoint)
                {
                    IncrementStat("SimulacrumStarted", ev.EventTime, 1);
                    _lastSimuEndpoint = _currentInstanceEndpoint;

                    _currentActivity = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        Type = ACTIVITY_TYPES.SIMULACRUM,
                        AreaLevel = _nextAreaLevel,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentInstanceEndpoint
                    };
                    _nextAreaLevel = 0;
                }
            }

            // Special calculation for Elder fight - he has no start dialoge.
            if (sAreaName == "Absence of Value and Meaning".Trim())
            {
                if (!_elderFightActive)
                {
                    IncrementStat("ElderTried", ev.EventTime, 1);
                }
                _elderFightActive = true;
            }

            ACTIVITY_TYPES actType = ACTIVITY_TYPES.MAP;
            if (bTargetAreaIsMap)
            {
                actType = ACTIVITY_TYPES.MAP;
            }
            else if (bTargetAreaIsHeist)
            {
                actType = ACTIVITY_TYPES.HEIST;
            }
            else if (bTargetAreaIsSimu)
            {
                actType = ACTIVITY_TYPES.SIMULACRUM;
            }
            else if (bTargetAreaIsLab)
            {
                actType = ACTIVITY_TYPES.LABYRINTH;
            }
            else if (bTargetAreaMine)
            {
                actType = ACTIVITY_TYPES.DELVE;
            }
            else if (bTargetAreaTemple)
            {
                actType = ACTIVITY_TYPES.TEMPLE;
            }
            else if (bTargetAreaIsMI)
            {
                actType = ACTIVITY_TYPES.MAVEN_INVITATION;
            }
            else if (bTargetAreaIsAtziri)
            {
                actType = ACTIVITY_TYPES.ATZIRI;
            }
            else if (bTargetAreaIsUberAtziri)
            {
                actType = ACTIVITY_TYPES.UBER_ATZIRI;
            }
            else if (bTargetAreaIsShaper)
            {
                actType = ACTIVITY_TYPES.SHAPER_FIGHT;
            }
            else if (bTargetAreaIsElder)
            {
                actType = ACTIVITY_TYPES.ELDER_FIGHT;
            }
            else if (bTargetAreaIsMavenFight)
            {
                actType = ACTIVITY_TYPES.MAVEN_FIGHT;
            }
            else if (bTargetAreaIsSirusFight)
            {
                actType = ACTIVITY_TYPES.SIRUS_FIGHT;
            }
            else if (bTargetAreaIsCampaign)
            {
                actType = ACTIVITY_TYPES.CAMPAIGN;
            }


            //Lab started?
            if (actType == ACTIVITY_TYPES.LABYRINTH && sSourceArea == "Aspirants Plaza")
            {
                string sLabName;

                switch (_nextAreaLevel)
                {
                    case 33:
                        sLabName = "The Labyrinth";
                        break;
                    case 55:
                        sLabName = "The Cruel Labyrinth";
                        break;
                    case 68:
                        sLabName = "The Merciless Labyrinth";
                        break;
                    case 75:
                        sLabName = "Uber-Lab";
                        break;
                    case 83:
                        sLabName = "Advanced Uber-Lab";
                        break;
                    default:
                        sLabName = "Unknown";
                        break;
                }

                // Finish activity
                if (_currentActivity != null)
                {
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                }

                _currentActivity = new TrX_TrackedActivity
                {
                    Area = sLabName,
                    AreaLevel = _nextAreaLevel,
                    Type = actType,
                    Started = ev.EventTime,
                    TimeStamp = lTS,
                    InstanceEndpoint = _currentInstanceEndpoint
                };

                IncrementStat("LabsStarted", ev.EventTime, 1);

            }

            //Lab Trial entered
            {
                if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LABYRINTH && sTargetArea == "Aspirants Trial")
                {
                    _currentActivity.TrialCount++;
                }
            }

            //Lab cancelled?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
            {
                if (sTargetArea.Contains("Hideout") || _defaultMappings.CAMP_AREAS.Contains(sTargetArea))
                {
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                }
            }

            // Delving?
            if ((_currentActivity == null || _currentActivity.Type != ACTIVITY_TYPES.DELVE) && actType == ACTIVITY_TYPES.DELVE)
            {
                // Finish activity
                if (_currentActivity != null)
                {
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                }

                _currentActivity = new TrX_TrackedActivity
                {
                    Area = "Azurite Mine",
                    Type = actType,
                    Started = ev.EventTime,
                    TimeStamp = lTS,
                    InstanceEndpoint = _currentInstanceEndpoint
                };
            }

            // Update Delve level
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.DELVE && actType == ACTIVITY_TYPES.DELVE)
            {
                if (_nextAreaLevel > _currentActivity.AreaLevel)
                {
                    _currentActivity.AreaLevel = _nextAreaLevel;
                }
            }

            // End Delving?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.DELVE && !bTargetAreaMine)
            {
                FinishActivity(_currentActivity, null, ACTIVITY_TYPES.DELVE, DateTime.Now);
            }


            //Campaign ?
            if (bTargetAreaIsCampaign)
            {
                // Do not track first town visit after login
                if (!_StartedFlag)
                {
                    if (_currentActivity != null)
                    {
                        if (sTargetArea != _currentActivity.Area || _currentInstanceEndpoint != _currentActivity.InstanceEndpoint)
                        {
                            _currentActivity.LastEnded = ev.EventTime;
                            FinishActivity(_currentActivity, sTargetArea, ACTIVITY_TYPES.CAMPAIGN, ev.EventTime);
                        }
                    }
                    else
                    {
                        _currentActivity = new TrX_TrackedActivity
                        {
                            Area = sTargetArea,
                            Type = ACTIVITY_TYPES.CAMPAIGN,
                            AreaLevel = _nextAreaLevel,
                            TimeStamp = lTS,
                            Started = ev.EventTime,
                            InstanceEndpoint = _currentInstanceEndpoint
                        };
                        _currentActivity.StartStopWatch();
                    }
                }
                else
                {
                    _StartedFlag = false;
                }


            }
            else
            {
                // Pause campaing when entering hideout
                if (sTargetArea.Contains("Hideout") && _currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.CAMPAIGN)
                {
                    _currentActivity.LastEnded = ev.EventTime;
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.OTHER, ev.EventTime);
                }
            }

            // PAUSE RESUME Handling
            if (bTargetAreaIsMap || bTargetAreaIsHeist || bTargetAreaIsSimu || bTargetAreaIsCampaign)
            {
                if (_currentActivity != null)
                {
                    if (_defaultMappings.CAMP_AREAS.Contains(sSourceArea) || sSourceArea.Contains("Hideout"))
                    {
                        if (sTargetArea == _currentActivity.Area && _currentInstanceEndpoint == _currentActivity.InstanceEndpoint)
                        {
                            _currentActivity.EndPauseTime(ev.EventTime);
                        }
                    }
                }
            }



            // Mechanisms that can be tracked with default logic:
            // One Area + Own instance
            bool enteringDefaultTrackableActivity =
                bTargetAreaIsMap ||
                bTargetAreaIsHeist ||
                bTargetAreaIsSimu ||
                bTargetAreaIsLab ||
                bTargetAreaMine ||
                bTargetAreaTemple ||
                bTargetAreaIsMI ||
                bTargetAreaIsUberAtziri ||
                bTargetAreaIsAtziri ||
                bTargetAreaIsElder ||
                bTargetAreaIsShaper ||
                bTargetAreaIsMavenFight ||
                bTargetAreaIsSirusFight;


            if (enteringDefaultTrackableActivity)
            {
                _elderFightActive = false;
                _shaperKillsInFight = 0;

                if (_currentActivity == null)
                {
                    _currentActivity = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        Type = actType,
                        AreaLevel = _nextAreaLevel,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentInstanceEndpoint
                    };
                    _nextAreaLevel = 0;
                }
                else
                {
                    if (bTargetAreaIsSimu || bTargetAreaIsMap)
                    {
                        _currentActivity.PortalsUsed++;
                    }
                }
                if (!_currentActivity.ManuallyPaused)
                    _currentActivity.StartStopWatch();

                if (bSourceAreaIsMap && bTargetAreaIsMap)
                {
                    if (!_isMapZana)
                    {
                        // entered Zana Map
                        _isMapZana = true;
                        _currentActivity.StopStopWatch();
                        if (_currentActivity.ZanaMap == null)
                        {
                            _currentActivity.ZanaMap = new TrX_TrackedActivity
                            {
                                Type = ACTIVITY_TYPES.MAP,
                                Area = sTargetArea,
                                AreaLevel = _nextAreaLevel,
                                Started = ev.EventTime,
                                TimeStamp = lTS,
                            };
                            _currentActivity.ZanaMap.AddTag("zana-map");
                            _nextAreaLevel = 0;
                        }
                        if (!_currentActivity.ZanaMap.ManuallyPaused)
                            _currentActivity.ZanaMap.StartStopWatch();
                    }
                    else
                    {
                        _isMapZana = false;

                        // leave Zana Map
                        if (_currentActivity.ZanaMap != null)
                        {
                            _isMapZana = false;
                            _currentActivity.ZanaMap.StopStopWatch();
                            _currentActivity.ZanaMap.LastEnded = ev.EventTime;
                            if (!_currentActivity.ManuallyPaused)
                                _currentActivity.StartStopWatch();
                        }
                    }
                }
                else
                {
                    _isMapZana = false; //TMP_DEBUG

                    // Do not track Lab-Trials
                    if ((!sSourceArea.Contains("Trial of")) && (_currentActivity.Type != ACTIVITY_TYPES.LABYRINTH) && (_currentActivity.Type != ACTIVITY_TYPES.DELVE))
                    {
                        if (sTargetArea != _currentActivity.Area || _currentInstanceEndpoint != _currentActivity.InstanceEndpoint)
                        {
                            FinishActivity(_currentActivity, sTargetArea, actType, ev.EventTime);
                        }
                    }
                }
            }
            else // ENTERING AN AREA WHICH IS NOT AN DEFAULT ACTIVITY
            {
                if (_currentActivity != null && _currentActivity.Type != ACTIVITY_TYPES.LABYRINTH && _currentActivity.Type != ACTIVITY_TYPES.CAMPAIGN)
                {
                    //TEST: Pause when left the source area
                    if (sSourceArea == _currentActivity.Area)
                    {
                        _currentActivity.StopStopWatch();
                        _currentActivity.LastEnded = ev.EventTime;

                        // PAUSE TIME
                        if (_defaultMappings.PAUSABLE_ACTIVITY_TYPES.Contains(_currentActivity.Type))
                        {
                            if (_defaultMappings.CAMP_AREAS.Contains(sTargetArea) || sTargetArea.Contains("Hideout"))
                            {
                                _currentActivity.StartPauseTime(ev.EventTime);
                            }
                        }
                    }

                    if (_currentActivity.ZanaMap != null)
                    {
                        if (sSourceArea == _currentActivity.ZanaMap.Area)
                        {
                            _currentActivity.ZanaMap.StopStopWatch();
                            _currentActivity.ZanaMap.LastEnded = ev.EventTime;
                        }
                    }

                }
            }

            _currentArea = sAreaName;
        }

        /// <summary>
        /// Handle player died event
        /// </summary>
        /// <param name="ev"></param>
        private void HandlePlayerDiedEvent(TrX_TrackingEvent ev)
        {
            string sPlayerName = ev.LogLine.Split(' ')[8];
            if (!_knownPlayerNames.Contains(sPlayerName))
            {
                IncrementStat("TotalKilledCount", ev.EventTime, 1);
                _lastDeathTime = DateTime.Now;

                // Lab?
                if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
                {
                    _currentActivity.DeathCounter = 1;
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                }

                if (!_defaultMappings.DEATH_COUNT_ENABLED_AREAS.Contains(_currentArea))
                {
                    return;
                }

                if (_currentActivity != null)
                {
                    if (_isMapZana)
                    {
                        if (_currentActivity.ZanaMap != null)
                        {
                            _currentActivity.ZanaMap.DeathCounter++;
                            _currentActivity.ZanaMap.LastEnded = ev.EventTime;
                        }
                    }
                    else
                    {
                        _currentActivity.DeathCounter++;
                    }
                }


            }

        }

        /// <summary>
        /// Handle single event. Routes more complex calcs to dedicated methods.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="bInit"></param>
        private void HandleSingleEvent(TrX_TrackingEvent ev, bool bInit = false)
        {
            try
            {
                switch (ev.EventType)
                {
                    case EVENT_TYPES.ABNORMAL_DISCONNECT:
                        if (_currentActivity != null)
                        {
                            _log.Info("Abnormal disconnect found in log. Finishing Map.");
                            FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                        }
                        break;

                    case EVENT_TYPES.POE_CLIENT_START:
                        _StartedFlag = true;

                        if (_currentActivity != null)
                        {
                            // Filter out non trackable ends, like town visits right before app close
                            if (_currentActivity.LastEnded.Year < 2000)
                            {
                                _currentActivity = null;
                            }
                            else
                            {
                                _currentActivity.IsFinished = true;
                                if (_currentActivity.ZanaMap != null)
                                {
                                    _currentActivity.ZanaMap.IsFinished = true;
                                }
                                FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                            }
                        }
                        if (_trackingHO)
                        {
                            _trackingHO = false;
                        }
                        break;
                    case EVENT_TYPES.CHAT_CMD_RECEIVED:
                        string sCmd = ev.LogLine.Split(new string[] { "::" }, StringSplitOptions.None)[1];
                        if (_eventQueueInitizalized)
                        {
                            HandleChatCommand(sCmd);
                        }
                        break;
                    case EVENT_TYPES.ENTERED_AREA:
                        HandleAreaChangeEvent(ev);
                        break;
                    case EVENT_TYPES.PLAYER_DIED:
                        HandlePlayerDiedEvent(ev);
                        break;
                    case EVENT_TYPES.ELDER_KILLED:
                        IncrementStat("ElderKilled", ev.EventTime, 1);
                        _elderFightActive = false;
                        break;
                    case EVENT_TYPES.SHAPER_KILLED:
                        // shaper has 3x the same kill dialogue
                        _shaperKillsInFight++;
                        if (_shaperKillsInFight == 3)
                        {
                            IncrementStat("ShaperKilled", ev.EventTime, 1);
                            _shaperKillsInFight = 0;
                        }
                        break;
                    case EVENT_TYPES.SIRUS_FIGHT_STARTED:
                        IncrementStat("SirusStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.SIRUS_KILLED:
                        IncrementStat("SirusKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.INSTANCE_CONNECTED:
                        _currentInstanceEndpoint = GetEndpointFromInstanceEvent(ev);
                        break;
                    case EVENT_TYPES.VERITANIA_FIGHT_STARTED:
                        IncrementStat("VeritaniaStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.BARAN_FIGHT_STARTED:
                        IncrementStat("BaranStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.DROX_FIGHT_STARTED:
                        IncrementStat("DroxStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.HUNTER_FIGHT_STARTED:
                        IncrementStat("HunterStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.MAVEN_FIGHT_STARTED:
                        IncrementStat("MavenStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.TRIALMASTER_STARTED:
                        IncrementStat("TrialMasterStarted", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.TRIALMASTER_VICTORY:
                        IncrementStat("TrialMasterSuccess", ev.EventTime, 1);
                        IncrementStat("TrialMasterVictory", ev.EventTime, 1);
                        if (_currentActivity != null)
                        {
                            _currentActivity.TrialMasterSuccess = true;
                            _currentActivity.TrialMasterFullFinished = true;
                        }
                        break;
                    case EVENT_TYPES.TRIALMASTER_TOOK_REWARD:
                        IncrementStat("TrialMasterTookReward", ev.EventTime, 1);
                        IncrementStat("TrialMasterSuccess", ev.EventTime, 1);
                        if (_currentActivity != null)
                        {
                            _currentActivity.TrialMasterSuccess = true;
                            _currentActivity.TrialMasterFullFinished = false;
                        }
                        break;
                    case EVENT_TYPES.MAVEN_KILLED:
                        IncrementStat("MavenKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.TRIALMASTER_KILLED:
                        IncrementStat("TrialMasterKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.VERITANIA_KILLED:
                        if (_lastEventType != EVENT_TYPES.VERITANIA_KILLED)
                        {
                            IncrementStat("VeritaniaKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.DROX_KILLED:
                        if (_lastEventType != EVENT_TYPES.DROX_KILLED)
                        {
                            IncrementStat("DroxKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.BARAN_KILLED:
                        if (_lastEventType != EVENT_TYPES.BARAN_KILLED)
                        {
                            IncrementStat("BaranKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.HUNTER_KILLED:
                        if (_lastEventType != EVENT_TYPES.HUNTER_KILLED)
                        {
                            IncrementStat("HunterKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.TRIALMASTER_ROUND_STARTED:
                        if (_currentActivity != null)
                        {
                            _currentActivity.TrialMasterCount += 1;
                        }
                        break;
                    case EVENT_TYPES.EINHAR_BEAST_CAPTURE:
                        IncrementStat("EinharCaptures", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.SHAPER_FIGHT_STARTED:
                        IncrementStat("ShaperTried", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.PARTYMEMBER_ENTERED_AREA:
                        AddKnownPlayerIfNotExists(ev.LogLine.Split(' ')[8]);
                        break;
                    case EVENT_TYPES.CATARINA_FIGHT_STARTED:
                        IncrementStat("CatarinaTried", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.CATARINA_KILLED:
                        IncrementStat("CatarinaKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.DELIRIUM_ENCOUNTER:

                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("delirium");
                            }
                            else
                            {
                                _currentActivity.AddTag("delirium");
                            }
                        }
                        break;
                    case EVENT_TYPES.BLIGHT_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("blight");
                            }
                            else
                            {
                                _currentActivity.AddTag("blight");
                            }
                        }
                        break;
                    case EVENT_TYPES.EINHAR_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("einhar");
                            }
                            else
                            {
                                _currentActivity.AddTag("einhar");
                            }
                        }
                        break;
                    case EVENT_TYPES.INCURSION_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("incursion");
                            }
                            else
                            {
                                _currentActivity.AddTag("incursion");
                            }
                        }
                        break;
                    case EVENT_TYPES.NIKO_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("niko");
                            }
                            else
                            {
                                _currentActivity.AddTag("niko");
                            }
                        }
                        break;
                    case EVENT_TYPES.ZANA_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            _currentActivity.AddTag("zana");
                        }
                        break;
                    case EVENT_TYPES.SYNDICATE_ENCOUNTER:
                        if (CheckIfAreaIsMap(_currentArea) && _currentActivity != null)
                        {
                            if (_isMapZana && _currentActivity.ZanaMap != null)
                            {
                                _currentActivity.ZanaMap.AddTag("syndicate");
                            }
                            else
                            {
                                _currentActivity.AddTag("syndicate");
                            }
                        }
                        break;
                    case EVENT_TYPES.LEVELUP:
                        bool bIsMySelf = true;
                        foreach (string s in _knownPlayerNames)
                        {
                            if (ev.LogLine.Contains(s))
                            {
                                bIsMySelf = false;
                                break;
                            }
                        }
                        if (bIsMySelf)
                        {
                            IncrementStat("LevelUps", ev.EventTime, 1);
                            string[] spl = ev.LogLine.Split(' ');
                            int iLevel = Convert.ToInt32(spl[spl.Length - 1]);
                            if (iLevel > _numericStats["HighestLevel"])
                            {
                                SetStat("HighestLevel", ev.EventTime, iLevel);
                            }
                        }
                        break;
                    case EVENT_TYPES.SIMULACRUM_FULLCLEAR:
                        IncrementStat("SimulacrumCleared", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.LAB_FINISHED:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
                        {
                            IncrementStat("LabsFinished", ev.EventTime, 1);
                            IncrementStat("LabsCompleted_" + _currentActivity.Area, ev.EventTime, 1);
                            _currentActivity.Success = true;
                            FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                        }
                        break;
                    case EVENT_TYPES.LAB_START_INFO_RECEIVED:

                        break;
                    case EVENT_TYPES.NEXT_AREA_LEVEL_RECEIVED:
                        string sLvl = ev.LogLine.Split(new string[] { "Generating level " }, StringSplitOptions.None)[1]
                            .Split(' ')[0];
                        _nextAreaLevel = Convert.ToInt32(sLvl);
                        _currentAreaLevel = _nextAreaLevel;
                        break;
                    case EVENT_TYPES.EXP_DANNIG_ENCOUNTER:
                        if (_currentActivity != null && !_currentActivity.HasTag("dannig") && _currentActivity.Type == ACTIVITY_TYPES.MAP)
                        {
                            IncrementStat("ExpeditionEncounters", ev.EventTime, 1);
                            IncrementStat("ExpeditionEncounters_Dannig", ev.EventTime, 1);
                            AddTagAutoCreate("expedition", _currentActivity);
                            _currentActivity.Tags.Add("expedition");
                            _currentActivity.Tags.Add("dannig");
                        }
                        break;
                    case EVENT_TYPES.EXP_GWENNEN_ENCOUNTER:
                        if (_currentActivity != null && !_currentActivity.HasTag("gwennen") && _currentActivity.Type == ACTIVITY_TYPES.MAP)
                        {
                            IncrementStat("ExpeditionEncounters", ev.EventTime, 1);
                            IncrementStat("ExpeditionEncounters_Gwennen", ev.EventTime, 1);
                            _currentActivity.Tags.Add("expedition");
                            _currentActivity.Tags.Add("gwennen");
                        }
                        break;
                    case EVENT_TYPES.EXP_TUJEN_ENCOUNTER:
                        if (_currentActivity != null && !_currentActivity.HasTag("tujen") && _currentActivity.Type == ACTIVITY_TYPES.MAP)
                        {
                            IncrementStat("ExpeditionEncounters", ev.EventTime, 1);
                            IncrementStat("ExpeditionEncounters_Tujen", ev.EventTime, 1);
                            _currentActivity.Tags.Add("expedition");
                            _currentActivity.Tags.Add("tujen");
                        }
                        break;
                    case EVENT_TYPES.EXP_ROG_ENCOUNTER:
                        if (_currentActivity != null && !_currentActivity.HasTag("rog") && _currentActivity.Type == ACTIVITY_TYPES.MAP)
                        {
                            IncrementStat("ExpeditionEncounters", ev.EventTime, 1);
                            IncrementStat("ExpeditionEncounters_Rog", ev.EventTime, 1);
                            _currentActivity.Tags.Add("expedition");
                            _currentActivity.Tags.Add("rog");
                        }
                        break;
                    case EVENT_TYPES.HEIST_GIANNA_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("gianna");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_HUCK_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("huck");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_ISLA_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("isla");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_NENET_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("nenet");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_NILES_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("niles");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_TIBBS_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("tibbs");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_TULLINA_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("tullina");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_VINDERI_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("vinderi");
                            }
                        }
                        break;
                    case EVENT_TYPES.HEIST_KARST_SPEAK:
                        if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.HEIST)
                        {
                            if (CheckIfAreaIsHeist(_currentArea, "The Rogue Harbour"))
                            {
                                _currentActivity.AddTag("karst");
                            }
                        }
                        break;
                    case EVENT_TYPES.CAMPAIGN_FINISHED:
                        IncrementStat("CampaignFinished", ev.EventTime, 1);
                        break;

                }

                // Sometimes conqueror fire their death speech twice...
                EVENT_TYPES[] checkConqTypes =
                    {
                    EVENT_TYPES.VERITANIA_FIGHT_STARTED,
                    EVENT_TYPES.VERITANIA_KILLED,
                    EVENT_TYPES.HUNTER_KILLED ,
                    EVENT_TYPES.HUNTER_FIGHT_STARTED,
                    EVENT_TYPES.HUNTER_KILLED,
                    EVENT_TYPES.BARAN_FIGHT_STARTED,
                    EVENT_TYPES.BARAN_KILLED,
                    EVENT_TYPES.DROX_FIGHT_STARTED,
                    EVENT_TYPES.DROX_KILLED,
                };

                if (checkConqTypes.Contains<EVENT_TYPES>(ev.EventType))
                {
                    _lastEventType = ev.EventType;
                }

                if (!bInit) TextLogEvent(ev);
            }
            catch (Exception ex)
            {
                _log.Warn("ParseError -> Ex.Message: " + ex.Message + ", LogLine: " + ev.LogLine);
                _log.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// Increment a stat with an defined value. Updates the database.
        /// 
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="dt"></param>
        /// <param name="i_value">default=1</param>
        private void IncrementStat(string s_key, DateTime dt, int i_value = 1)
        {
            _numericStats[s_key] += i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
            _uiFlagStatsList = true;
            _uiFlagBossDashboard = true;
        }

        /// <summary>
        /// Update a stat with a fixed value. Updates the database
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="dt"></param>
        /// <param name="i_value"></param>
        private void SetStat(string s_key, DateTime dt, int i_value)
        {
            _numericStats[s_key] = i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
            _uiFlagStatsList = true;
            _uiFlagBossDashboard = true;
        }

        /// <summary>
        /// Extract the instance endpoint from a log line.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        private string GetEndpointFromInstanceEvent(TrX_TrackingEvent ev)
        {
            return ev.LogLine.Split(new String[] { "Connecting to instance server at " }, StringSplitOptions.None)[1];
        }

        /// <summary>
        /// Finishs the current activity.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="sNextMap">next map to start. Set to null if there is none</param>
        /// <param name="sNextMapType"></param>
        /// <param name="dtNextMapStarted"></param>
        private void FinishActivity(TrX_TrackedActivity activity, string sNextMap, ACTIVITY_TYPES sNextMapType, DateTime dtNextMapStarted)
        {
            _log.Debug("Finishing activity: " + activity.UniqueID);

            _currentActivity.StopStopWatch();

            if (!SAFE_RELOAD_MODE)
            {
                TimeSpan ts;
                TimeSpan tsZana;
                int iSeconds;
                int iSecondsZana = 0;


                // Filter out invalid labs (discnnect etc)
                if (activity.Type == ACTIVITY_TYPES.LABYRINTH)
                {
                    if (activity.Area == "Unknown")
                    {
                        // no area changes logged for some old lab runs :/
                        activity.Success = activity.DeathCounter == 0;
                    }

                    // Labs must be successfull or death counter 1
                    if (activity.Success != true && activity.DeathCounter == 0)
                    {
                        _log.Info("Filtered out lab run [time=" + activity.Started + ", area: " + activity.Area + "]. Reason Success=False AND DeathCounter = 0. Maybe disconnect or game crash while lab.");
                        _currentActivity = null;
                        return;
                    }

                }


                if (!_eventQueueInitizalized)
                {
                    ts = (activity.LastEnded - activity.Started);
                    try
                    {
                        iSeconds = Convert.ToInt32(ts.TotalSeconds);
                        iSeconds -= Convert.ToInt32(activity.PausedTime);
                    }
                    catch
                    {
                        iSeconds = 0;
                    }

                    if (activity.ZanaMap != null)
                    {
                        tsZana = (activity.ZanaMap.LastEnded - activity.ZanaMap.Started);
                        iSecondsZana = Convert.ToInt32(tsZana.TotalSeconds);
                    }

                    // Filter out town activities without end date
                    if (activity.LastEnded.Year < 2000)
                    {
                        return;
                    }

                    // Filter out 0-second town visits
                    if (activity.Type == ACTIVITY_TYPES.CAMPAIGN && iSeconds == 0)
                    {
                        return;
                    }
                }
                else
                {
                    ts = activity.StopWatchTimeSpan;
                    iSeconds = Convert.ToInt32(ts.TotalSeconds);
                    if (activity.ZanaMap != null)
                    {
                        tsZana = activity.ZanaMap.StopWatchTimeSpan;
                        iSecondsZana = Convert.ToInt32(tsZana.TotalSeconds);
                    }
                }

                _currentActivity.TotalSeconds = iSeconds;
                if (!_eventHistory.Contains(_currentActivity))
                {
                    _eventHistory.Insert(0, _currentActivity);
                }

                TimeSpan tsMain = TimeSpan.FromSeconds(iSeconds);
                activity.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                          tsMain.Hours, tsMain.Minutes, tsMain.Seconds);

                if (!_parsedActivities.Contains(activity.UniqueID))
                {
                    AddMapLvItem(activity);

                    // Request GUI Update
                    _uiFlagActivityList = true;

                    // Save to DB
                    SaveToActivityLog(((DateTimeOffset)activity.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.Type), activity.Area, activity.AreaLevel, iSeconds, activity.DeathCounter, activity.TrialMasterCount, false, activity.Tags, activity.Success, Convert.ToInt32(activity.PausedTime));
                }


                if (activity.ZanaMap != null)
                {
                    TimeSpan tsZanaMap = TimeSpan.FromSeconds(iSecondsZana);
                    activity.ZanaMap.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                           tsZanaMap.Hours, tsZanaMap.Minutes, tsZanaMap.Seconds);
                    _eventHistory.Insert(0, _currentActivity.ZanaMap);

                    if (!_parsedActivities.Contains(activity.ZanaMap.UniqueID))
                    {
                        AddMapLvItem(activity.ZanaMap, true);

                        //Request GUI Update
                        _uiFlagActivityList = true;

                        //Save to DB
                        SaveToActivityLog(((DateTimeOffset)activity.ZanaMap.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.ZanaMap.Type), activity.ZanaMap.Area, activity.ZanaMap.AreaLevel, iSecondsZana, activity.ZanaMap.DeathCounter, activity.ZanaMap.TrialMasterCount, true, activity.ZanaMap
                            .Tags, activity.ZanaMap.Success, Convert.ToInt32(activity.ZanaMap.PausedTime));
                    }
                }

            }

            if (sNextMap != null)
            {
                _currentActivity = new TrX_TrackedActivity
                {
                    Area = sNextMap,
                    Type = sNextMapType,
                    AreaLevel = _nextAreaLevel,
                    InstanceEndpoint = _currentInstanceEndpoint,
                    Started = dtNextMapStarted,
                    TimeStamp = ((DateTimeOffset)dtNextMapStarted).ToUnixTimeSeconds()
                };
                _nextAreaLevel = 0;
                _currentActivity.StartStopWatch();
            }
            else
            {
                _currentActivity = null;
            }

            if (activity.Type == ACTIVITY_TYPES.HEIST)
            {
                IncrementStat("TotalHeistsDone", activity.Started, 1);
                IncrementStat("HeistsFinished_" + activity.Area, activity.Started, 1);
            }
            else if (activity.Type == ACTIVITY_TYPES.MAP)
            {
                IncrementStat("TotalMapsDone", activity.Started, 1);
                IncrementStat("MapsFinished_" + activity.Area, activity.Started, 1);
                IncrementStat("MapTierFinished_T" + activity.MapTier.ToString(), activity.Started, 1);

                if (activity.ZanaMap != null)
                {
                    IncrementStat("TotalMapsDone", activity.ZanaMap.Started, 1);
                    IncrementStat("MapsFinished_" + activity.ZanaMap.Area, activity.ZanaMap.Started, 1);
                    IncrementStat("MapTierFinished_T" + activity.ZanaMap.MapTier.ToString(), activity.ZanaMap.Started, 1);
                }
            }
            else if (activity.Type == ACTIVITY_TYPES.SIMULACRUM)
            {
                IncrementStat("SimulacrumFinished_" + activity.Area, activity.Started, 1);
            }
            else if (activity.Type == ACTIVITY_TYPES.TEMPLE)
            {
                IncrementStat("TemplesDone", activity.Started, 1);
            }

            if (_eventQueueInitizalized)
            {
                MethodInvoker mi = delegate
                {
                    RenderGlobalDashboard();
                    RenderHeistDashboard();
                    RenderLabDashboard();
                    RenderMappingDashboard();
                };
                Invoke(mi);

              
            }
        }

        /// <summary>
        /// Read the statistics cache
        /// </summary>
        private void ReadStatsCache()
        {
            if (File.Exists(_cachePath))
            {
                StreamReader r = new StreamReader(_cachePath);
                string line;
                string statID;
                int statValue;
                int iLine = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if (iLine == 0)
                    {
                        _lastHash = Convert.ToInt32(line.Split(';')[1]);
                    }
                    else
                    {
                        statID = line.Split(';')[0];
                        statValue = Convert.ToInt32(line.Split(';')[1]);
                        if (_numericStats.ContainsKey(statID))
                        {
                            _numericStats[line.Split(';')[0]] = statValue;
                            _log.Info("StatsCacheRead -> " + statID + "=" + statValue.ToString());
                        }
                        else
                        {
                            _log.Warn("StatsCacheRead -> Unknown stat '" + statID + "' in stats.cache, maybe from an older version.");
                        }

                    }

                    iLine++;
                }
                r.Close();
            }
        }

        /// <summary>
        /// Write the statistics cache
        /// </summary>
        private void SaveStatsCache()
        {
            StreamWriter wrt = new StreamWriter(_cachePath);
            wrt.WriteLine("last;" + _lastHash.ToString());
            foreach (KeyValuePair<string, int> kvp in _numericStats)
            {
                wrt.WriteLine(kvp.Key + ";" + kvp.Value);
            }
            wrt.Close();
        }

        /// <summary>
        /// Simply save the current app version to VERSION.txt
        /// </summary>
        private void SaveVersion()
        {
            StreamWriter wrt = new StreamWriter(_myAppData + @"\VERSION.txt");
            wrt.WriteLine(APPINFO.VERSION);
            wrt.Close();
        }

        /// <summary>
        /// Add event to EventLog
        /// </summary>
        /// <param name="ev"></param>
        private void TextLogEvent(TrX_TrackingEvent ev)
        {
            Invoke((MethodInvoker)delegate
            {
                textBoxLogView.Text += ev.ToString() + Environment.NewLine;
            });
        }

        /// <summary>
        /// Reset and reload the Activity-History ListView
        /// </summary>
        public void ResetMapHistory()
        {
            _lvmActlog.ClearLvItems();
            _lvmActlog.Columns.Clear();

            ColumnHeader
                chTime = new ColumnHeader() { Name = "actlog_time", Text = "Time", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_time.width", "60")) },
                chType = new ColumnHeader() { Name = "actlog_type", Text = "Type", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_type.width", "60")) },
                chArea = new ColumnHeader() { Name = "actlog_area", Text = "Area", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_area.width", "60")) },
                chLvl = new ColumnHeader() { Name = "actlog_lvl", Text = "Level/Tier", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_lvl.width", "60")) },
                chStopwatch = new ColumnHeader() { Name = "actlog_stopwatch", Text = "Stopwatch", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_stopwatch.width", "60")) },
                chDeath = new ColumnHeader() { Name = "actlog_death", Text = "Deaths", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_death.width", "60")) };


            _lvmActlog.Columns.Add(chTime);
            _lvmActlog.Columns.Add(chType);
            _lvmActlog.Columns.Add(chArea);
            _lvmActlog.Columns.Add(chLvl);
            _lvmActlog.Columns.Add(chStopwatch);
            _lvmActlog.Columns.Add(chDeath);


            foreach (TrX_ActivityTag tag in _tags)
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

        }

        /// <summary>
        /// Add maximum numberof ListViewItems to Listview
        /// TODO: filter before adding!!!
        /// </summary>
        private void AddActivityLvItems()
        {
            foreach (TrX_TrackedActivity act in _eventHistory)
            {
                AddMapLvItem(act, act.IsZana, -1, false);
            }
            _lvmActlog.FilterByRange(0, Convert.ToInt32(ReadSetting("actlog.maxitems", "500")));
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
                ListViewItem lvi = new ListViewItem(map.Started.ToString());
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

                if (bZana)
                    sName += " (Zana)";
                lvi.SubItems.Add(GetStringFromActType(map.Type));
                lvi.SubItems.Add(map.Area);
                lvi.SubItems.Add(sTier);
                lvi.SubItems.Add(map.StopWatchValue.ToString());
                lvi.SubItems.Add(map.DeathCounter.ToString());

                foreach (TrX_ActivityTag t in _tags)
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

            });
        }

        /// <summary>
        /// Find matching activity to Item name
        /// </summary>
        /// <param name="s_name"></param>
        /// <returns></returns>
        private TrX_TrackedActivity GetActivityFromListItemName(string s_name)
        {
            foreach (TrX_TrackedActivity ta in _eventHistory)
            {
                if (ta.UniqueID == s_name)
                    return ta;
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

        /// <summary>
        /// Extract an arae name out of a log line
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        private string GetAreaNameFromEvent(TrX_TrackingEvent ev)
        {
            string sArea = ev.LogLine.Split(new string[] { "You have entered" }, StringSplitOptions.None)[1]
                .Replace(".", "").Trim();
            return sArea.Replace("'", "");
        }

        /// <summary>
        /// Handle the GUI updates
        /// </summary>
        private void UpdateUI()
        {
            TimeSpan tsAreaTime = (DateTime.Now - _inAreaSince);
            checkBoxShowGridInAct.Checked = _showGridInActLog;
            checkBoxShowGridInStats.Checked = _showGridInStats;
            ReadBackupList();
            listBoxRestoreBackup.DataSource = _backups;


            if (_eventQueueInitizalized)
            {
                _loadScreenWindow.Close();
                Invoke((MethodInvoker)delegate
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
                                Environment.NewLine + _failedRestoreReason +
                                Environment.NewLine);

                        }
                    }

                    RenderTagsForTracking();
                    RenderTagsForConfig();
                    textBoxLogFilePath.Text = ReadSetting("poe_logfile_path");

                    labelItemCount.Text = "items: " + _actLogItemCount.ToString();


                    if (listViewStats.Items.Count > 0 && _uiFlagStatsList)
                    {
                        for (int i = 0; i < _numericStats.Count; i++)
                        {
                            KeyValuePair<string, int> kvp = _numericStats.ElementAt(i);

                            if (kvp.Key == "HideoutTimeSec")
                            {
                                _lvmStats.GetLvItem("stats_" + kvp.Key).SubItems[1].Text = Math.Round(((double)(kvp.Value / 60 / 60)), 1).ToString() + " hours";
                            }
                            else
                            {
                                _lvmStats.GetLvItem("stats_" + kvp.Key).SubItems[1].Text = kvp.Value.ToString();
                            }

                        }
                        _uiFlagStatsList = false;
                    }

                    if (!_listViewInitielaized)
                    {
                        DoSearch();
                        _listViewInitielaized = true;
                    }

                    labelCurrArea.Text = _currentArea;
                    labelCurrentAreaLvl.Text = _currentAreaLevel > 0 ? _currentAreaLevel.ToString() : "-";
                    labelLastDeath.Text = _lastDeathTime.Year > 2000 ? _lastDeathTime.ToString() : "-";

                    if (_currentArea.Contains("Hideout"))
                    {
                        labelCurrActivity.Text = "In Hideout";
                    }
                    else
                    {
                        if (_currentActivity != null)
                        {
                            labelCurrActivity.Text = _currentActivity.Type.ToString();
                        }
                        else
                        {
                            labelCurrActivity.Text = "Nothing";
                        }
                    }


                    if (_currentActivity != null)
                    {
                        string sTier = "";



                        if (_currentActivity.Type == ACTIVITY_TYPES.SIMULACRUM)
                        {
                            _currentActivity.AreaLevel = 75;
                        }

                        if (_currentActivity.AreaLevel > 0)
                        {
                            if (_currentActivity.Type == ACTIVITY_TYPES.MAP)
                            {
                                sTier = "T" + _currentActivity.MapTier.ToString();
                            }
                            else
                            {
                                sTier = "Lvl. " + _currentActivity.AreaLevel.ToString();
                            }
                        }


                        if (_isMapZana && _currentActivity.ZanaMap != null)
                        {
                            labelStopWatch.Text = _currentActivity.ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.ZanaMap.Area + " (" + sTier + ", Zana)";
                            labelTrackingDied.Text = _currentActivity.ZanaMap.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.Type).ToUpper();
                            pictureBoxStop.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = _currentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.Type).ToUpper();
                            labelTrackingDied.Text = _currentActivity.DeathCounter.ToString();
                            pictureBoxStop.Show();
                        }
                    }
                    else
                    {
                        labelTrackingDied.Text = "0";
                        labelTrackingArea.Text = "-";
                        labelStopWatch.Text = "00:00:00";
                        labelTrackingType.Text = "Enter an ingame activity to auto. start tracking.";
                    }

                    if(_uiFlagBossDashboard)
                    {
                        labelElderStatus.ForeColor = _numericStats["ElderKilled"] > 0 ? Color.Green : Color.Red;
                        labelElderStatus.Text = _numericStats["ElderKilled"] > 0 ? "Yes" : "No";
                        labelElderKillCount.Text = _numericStats["ElderKilled"].ToString() + "x";
                        labelElderTried.Text = _numericStats["ElderTried"].ToString() + "x";
                        labelShaperStatus.ForeColor = _numericStats["ShaperKilled"] > 0 ? Color.Green : Color.Red;
                        labelShaperStatus.Text = _numericStats["ShaperKilled"] > 0 ? "Yes" : "No";
                        labelShaperKillCount.Text = _numericStats["ShaperKilled"].ToString() + "x";
                        labelShaperTried.Text = _numericStats["ShaperTried"].ToString() + "x";
                        labelSirusStatus.ForeColor = _numericStats["SirusKilled"] > 0 ? Color.Green : Color.Red;
                        labelSirusStatus.Text = _numericStats["SirusKilled"] > 0 ? "Yes" : "No";
                        labelSirusKillCount.Text = _numericStats["SirusKilled"].ToString() + "x";
                        labelSirusTries.Text = _numericStats["SirusStarted"].ToString() + "x";
                        label80.ForeColor = _numericStats["CatarinaKilled"] > 0 ? Color.Green : Color.Red;
                        label80.Text = _numericStats["CatarinaKilled"] > 0 ? "Yes" : "No";
                        label78.Text = _numericStats["CatarinaKilled"].ToString() + "x";
                        label82.Text = _numericStats["CatarinaTried"].ToString() + "x";
                        labelVeritaniaStatus.ForeColor = _numericStats["VeritaniaKilled"] > 0 ? Color.Green : Color.Red;
                        labelVeritaniaKillCount.Text = _numericStats["VeritaniaKilled"].ToString() + "x";
                        labelVeritaniaStatus.Text = _numericStats["VeritaniaKilled"] > 0 ? "Yes" : "No";
                        labelVeritaniaTries.Text = _numericStats["VeritaniaStarted"].ToString() + "x";
                        labelHunterStatus.ForeColor = _numericStats["HunterKilled"] > 0 ? Color.Green : Color.Red;
                        labelHunterStatus.Text = _numericStats["HunterKilled"] > 0 ? "Yes" : "No";
                        labelHunterKillCount.Text = _numericStats["HunterKilled"].ToString() + "x";
                        labelHunterTries.Text = _numericStats["HunterStarted"].ToString() + "x";
                        labelDroxStatus.ForeColor = _numericStats["DroxKilled"] > 0 ? Color.Green : Color.Red;
                        labelDroxStatus.Text = _numericStats["DroxKilled"] > 0 ? "Yes" : "No";
                        labelDroxKillCount.Text = _numericStats["DroxKilled"].ToString() + "x";
                        labelDroxTries.Text = _numericStats["DroxStarted"].ToString() + "x";
                        labelBaranStatus.ForeColor = _numericStats["BaranKilled"] > 0 ? Color.Green : Color.Red;
                        labelBaranStatus.Text = _numericStats["BaranKilled"] > 0 ? "Yes" : "No";
                        labelBaranKillCount.Text = _numericStats["BaranKilled"].ToString() + "x";
                        labelBaranTries.Text = _numericStats["BaranStarted"].ToString() + "x";
                        labelTrialMasterStatus.ForeColor = _numericStats["TrialMasterKilled"] > 0 ? Color.Green : Color.Red;
                        labelTrialMasterStatus.Text = _numericStats["TrialMasterKilled"] > 0 ? "Yes" : "No";
                        labelTrialMasterKilled.Text = _numericStats["TrialMasterKilled"].ToString() + "x";
                        labelTrialMasterTried.Text = _numericStats["TrialMasterStarted"].ToString() + "x";
                        labelMavenStatus.ForeColor = _numericStats["MavenKilled"] > 0 ? Color.Green : Color.Red;
                        labelMavenStatus.Text = _numericStats["MavenKilled"] > 0 ? "Yes" : "No";
                        labelMavenKilled.Text = _numericStats["MavenKilled"].ToString() + "x";
                        labelMavenTried.Text = _numericStats["MavenStarted"].ToString() + "x";

                        _uiFlagBossDashboard = false;
                    }
                    


                    // MAP Dashbaord
                    if (_uiFlagMapDashboard)
                    {
                        RenderMappingDashboard();
                        _uiFlagMapDashboard = false;
                    }

                    // LAB Dashbaord
                    if (_labDashboardUpdateRequested)
                    {
                        RenderLabDashboard();
                        _labDashboardUpdateRequested = false;
                    }

                    // HEIST Dashbaord
                    if (_uiFlagHeistDashboard)
                    {
                        RenderHeistDashboard();
                        _uiFlagHeistDashboard = false;
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

                    // Activity List
                    if(_uiFlagActivityList)
                    {
                        DoSearch();
                        _uiFlagActivityList = false;
                    }

                    listView1.Columns[2].Width = listView1.Width;

                });
            }
        }

        /// <summary>
        /// Read all settings
        /// </summary>
        private void ReadSettings()
        {
            _showGridInActLog = Convert.ToBoolean(ReadSetting("ActivityLogShowGrid"));
            _showGridInStats = Convert.ToBoolean(ReadSetting("StatsShowGrid"));
            _timeCapLab = Convert.ToInt32(ReadSetting("TimeCapLab", "3600"));
            _timeCapMap = Convert.ToInt32(ReadSetting("TimeCapMap", "3600"));
            _timeCapHeist = Convert.ToInt32(ReadSetting("TimeCapHeist", "3600"));
            _labDashboardHideUnknown = Convert.ToBoolean(ReadSetting("dashboard_lab_hide_unknown", "false"));
            _showHideoutInPie = Convert.ToBoolean(ReadSetting("pie_chart_show_hideout", "true"));

            comboBoxTheme.SelectedItem = ReadSetting("theme", "Dark") == "Dark" ? "Dark" : "Light";
            textBoxMapCap.Text = _timeCapMap.ToString();
            textBoxLabCap.Text = _timeCapLab.ToString();
            textBoxHeistCap.Text = _timeCapHeist.ToString();
            listViewActLog.GridLines = _showGridInActLog;
            listViewStats.GridLines = _showGridInStats;
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
            _exit = true;
            if (_currentActivity != null)
                FinishActivity(_currentActivity, null, _currentActivity.Type, DateTime.Now);
            _log.Info("Exitting.");
            Application.Exit();
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

            foreach (string s in labs)
            {
                if (_labDashboardHideUnknown && s == "Unknown")
                    continue;

                labCounts.Add(s, 0);
                labAvgTimes.Add(s, 0);
                labBestTimes.Add(s, null);
            }

            // Lab counts
            foreach (TrX_TrackedActivity act in _eventHistory)
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

                foreach (TrX_TrackedActivity act in _eventHistory)
                {
                    if (act.Type == ACTIVITY_TYPES.LABYRINTH && act.DeathCounter == 0)
                    {
                        if (act.Area == s)
                        {
                            // Average
                            iCount++;

                            if (act.TotalSeconds < _timeCapLab)
                            {
                                iSum += act.TotalSeconds;
                            }
                            else
                            {
                                iSum += _timeCapLab;
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
                { ACTIVITY_TYPES.CAMPAIGN, Color.Turquoise }
            };

            double totalCount = 0;
            if (Convert.ToBoolean(ReadSetting("pie_chart_show_hideout", "true")))
            {
                totalCount += _numericStats["HideoutTimeSec"];
            }

            foreach (TrX_TrackedActivity act in _eventHistory)
            {
                int iCap = 3600;

                switch (act.Type)
                {
                    case ACTIVITY_TYPES.MAP:
                        iCap = _timeCapMap;
                        break;
                    case ACTIVITY_TYPES.LABYRINTH:
                        iCap = _timeCapLab;
                        break;
                    case ACTIVITY_TYPES.HEIST:
                        iCap = _timeCapHeist;
                        break;
                }

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
                double percentValHO = _numericStats["HideoutTimeSec"] / totalCount * 100;
                percentValHO = Math.Round(percentValHO, 2);
                TimeSpan tsDurationHO = TimeSpan.FromSeconds(_numericStats["HideoutTimeSec"]);
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

            foreach (string s in _defaultMappings.HEIST_AREAS)
            {
                tmpList.Add(new KeyValuePair<string, int>(s, _numericStats["HeistsFinished_" + s]));
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
            foreach (TrX_ActivityTag tg in Tags)
            {
                tmpListTags.Add(tg.ID, 0);
            }

            foreach (TrX_TrackedActivity act in _eventHistory)
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

                foreach (TrX_TrackedActivity act in _eventHistory)
                {
                    if (act.Type == ACTIVITY_TYPES.HEIST && act.AreaLevel == i)
                    {
                        iCount++;

                        if (act.TotalSeconds < _timeCapHeist)
                        {
                            iSum += act.TotalSeconds;
                        }
                        else
                        {
                            iSum += _timeCapHeist;
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
            Invoke(mi);
        }

        /// <summary>
        /// Render mapping dashboard
        /// </summary>
        public void RenderMappingDashboard()
        {
            List<KeyValuePair<string, int>> tmpList = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> top10 = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> tmpListTags = new Dictionary<string, int>();
            List<KeyValuePair<string, int>> top10Tags = new List<KeyValuePair<string, int>>();

            // MAP AREAS
            foreach (string s in _defaultMappings.MAP_AREAS)
            {
                tmpList.Add(new KeyValuePair<string, int>(s, _numericStats["MapsFinished_" + s]));
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
            foreach (TrX_ActivityTag tg in Tags)
            {
                tmpListTags.Add(tg.ID, 0);
            }

            foreach (TrX_TrackedActivity act in _eventHistory)
            {
                if (act.Type == ACTIVITY_TYPES.MAP)
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
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
            };

            for (int i = 0; i < 16; i++)
            {
                int iSum = 0;
                int iCount = 0;

                foreach (TrX_TrackedActivity act in _eventHistory)
                {
                    if (act.Type == ACTIVITY_TYPES.MAP && act.MapTier == (i + 1))
                    {
                        if (act.TotalSeconds < _timeCapMap)
                        {
                            iSum += act.TotalSeconds;
                        }
                        else
                        {
                            iSum += _timeCapMap;
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
                for (int i = 1; i <= 16; i++)
                {
                    chartMapTierCount.Series[0].Points.AddXY(i, _numericStats["MapTierFinished_T" + i.ToString()]);
                }

                chartMapTierAvgTime.Series[0].Points.Clear();
                for (int i = 0; i < tierAverages.Length; i++)
                {
                    chartMapTierAvgTime.Series[0].Points.AddXY(i + 1, Math.Round(tierAverages[i] / 60, 1));
                }
            };
            Invoke(mi);
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
        /// Refresh the statistics chart
        /// </summary>
        private void RefreshChart()
        {
            chartStats.Series[0].Points.Clear();
            switch (comboBoxTimeRangeStats.SelectedItem.ToString())
            {
                case "Last week":
                    chartStats.ChartAreas[0].AxisX.Interval = 1;
                    FillChart(7);
                    break;
                case "Last 2 weeks":
                    chartStats.ChartAreas[0].AxisX.Interval = 1;
                    FillChart(14);
                    break;
                case "Last 3 weeks":
                    chartStats.ChartAreas[0].AxisX.Interval = 2;
                    FillChart(21);
                    break;
                case "Last month":
                    chartStats.ChartAreas[0].AxisX.Interval = 3;
                    FillChart(31);
                    break;
                case "Last 2 month":
                    chartStats.ChartAreas[0].AxisX.Interval = 6;
                    FillChart(62);
                    break;
                case "Last 3 month":
                    chartStats.ChartAreas[0].AxisX.Interval = 9;
                    FillChart(93);
                    break;
                case "Last year":
                    chartStats.ChartAreas[0].AxisX.Interval = 30;
                    FillChart(365);
                    break;
                case "Last 2 years":
                    chartStats.ChartAreas[0].AxisX.Interval = 60;
                    FillChart(365 * 2);
                    break;
                case "Last 3 years":
                    chartStats.ChartAreas[0].AxisX.Interval = 90;
                    FillChart(365 * 3);
                    break;
                case "All time":
                    chartStats.ChartAreas[0].AxisX.Interval = 90;
                    FillChart(365 * 15);
                    break;
            }
        }

        /// <summary>
        /// Fill the sstatistics chart with data
        /// </summary>
        /// <param name="i_days_back"></param>
        private void FillChart(int i_days_back)
        {
            if (_lvmStats.listView.SelectedIndices.Count > 0)
            {
                chartStats.Series[0].Points.Clear();
                DateTime dtStart = DateTime.Now.AddDays(i_days_back * -1);
                string sStatName = _lvmStats.listView.SelectedItems[0].Name.Replace("stats_", "");

                DateTime dt1, dt2;
                SqliteDataReader sqlReader;
                long lTS1, lTS2;

                label38.Text = listViewStats.SelectedItems[0].Text;

                for (int i = 0; i <= i_days_back; i++)
                {
                    dt1 = dtStart.AddDays(i);
                    dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
                    dt2 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 23, 59, 59);
                    lTS1 = ((DateTimeOffset)dt1).ToUnixTimeSeconds();
                    lTS2 = ((DateTimeOffset)dt2).ToUnixTimeSeconds();


                    string sQuery;
                    if (sStatName == "HighestLevel")
                    {
                        sQuery = "SELECT stat_value FROM tx_stats WHERE stat_name = '" + sStatName + "' AND timestamp BETWEEN " + lTS1 + " AND " + lTS2;
                    }
                    else
                    {
                        sQuery = "SELECT COUNT(*) FROM tx_stats WHERE stat_name = '" + sStatName + "' AND timestamp BETWEEN " + lTS1 + " AND " + lTS2;
                    }

                    sqlReader = _myDB.GetSQLReader(sQuery);
                    while (sqlReader.Read())
                    {
                        chartStats.Series[0].Points.AddXY(dt1, sqlReader.GetInt32(0));
                    }
                }
            }
        }

        /// <summary>
        /// Get activity tag object for ID
        /// </summary>
        /// <param name="s_id"></param>
        /// <returns></returns>
        public TrX_ActivityTag GetTagByID(string s_id)
        {
            foreach (TrX_ActivityTag tag in _tags)
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
            string sBackupDir = _myAppData + @"/backups/" + s_name + @"/" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            System.IO.Directory.CreateDirectory(sBackupDir);

            if (System.IO.File.Exists(SettingPoeLogFilePath))
                System.IO.File.Copy(SettingPoeLogFilePath, sBackupDir + @"/Client.txt");
            if (System.IO.File.Exists(_cachePath))
                System.IO.File.Copy(_cachePath, sBackupDir + @"/stats.cache");
            if (System.IO.File.Exists(_dbPath))
                System.IO.File.Copy(_dbPath, sBackupDir + @"/data.db");
            if (System.IO.File.Exists("TraXile.exe.config"))
                System.IO.File.Copy("TraXile.exe.config", sBackupDir + @"/TraXile.exe.config");
        }

        /// <summary>
        /// Fully reset the application
        /// </summary>
        private void DoFullReset()
        {
            // Make logfile empty
            FileStream fs1 = new FileStream(SettingPoeLogFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            fs1.Close();
            ResetStats();
            ClearActivityLog();
            _lastHash = 0;
            File.Delete(_cachePath);
        }

        /// <summary>
        /// Open details for tracked activity
        /// </summary>
        /// <param name="ta"></param>
        private void OpenActivityDetails(TrX_TrackedActivity ta)
        {
            OpenChildWindow(new ActivityDetails(ta, this));
        }

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
            string sLine = "time;type;area;area_level;stopwatch;death_counter";
            wrt.WriteLine(sLine);

            for (int i = 0; i < _eventHistory.Count; i++)
            {
                tm = _eventHistory[i];
                sLine = "";
                sLine += tm.Started;
                sLine += ";" + tm.Type;
                sLine += ";" + tm.Area;
                sLine += ";" + tm.AreaLevel;
                sLine += ";" + tm.StopWatchValue;
                sLine += ";" + tm.DeathCounter;
                wrt.WriteLine(sLine);
            }
            wrt.Close();
        }

        /// <summary>
        /// Prepare backup restore before app restarts
        /// </summary>
        /// <param name="sPath"></param>
        private void PrepareBackupRestore(string sPath)
        {
            File.Copy(sPath + @"/stats.cache", _cachePath + ".restore");
            File.Copy(sPath + @"/data.db", _dbPath + ".restore");
            File.Copy(sPath + @"/Client.txt", Directory.GetParent(SettingPoeLogFilePath) + @"/_Client.txt.restore");
            _log.Info("Backup restore successfully prepared! Restarting Application");
            Application.Restart();
        }

        /// <summary>
        /// Check if backup restore is prepared and restore
        /// </summary>
        private void DoBackupRestoreIfPrepared()
        {
            if (File.Exists(_cachePath + ".restore"))
            {
                File.Delete(_cachePath);
                File.Move(_cachePath + ".restore", _cachePath);
                _log.Info("BackupRestored -> Source: _stats.cache.restore, Destination: " + _cachePath);
                _restoreMode = true;
            }

            if (File.Exists(_dbPath + ".restore"))
            {
                File.Delete(_dbPath);
                File.Move(_dbPath + ".restore", _dbPath);
                _log.Info("BackupRestored -> Source: _data.db.restore, Destination: data.db");
                _restoreMode = true;
            }

            try
            {
                if (File.Exists(Directory.GetParent(SettingPoeLogFilePath) + @"/_Client.txt.restore"))
                {
                    File.Delete(SettingPoeLogFilePath);
                    File.Move(Directory.GetParent(SettingPoeLogFilePath) + @"/_Client.txt.restore", SettingPoeLogFilePath);
                    _log.Info("BackupRestored -> Source: " + Directory.GetParent(SettingPoeLogFilePath) + @"/_Client.txt.restore" +
                        ", Destination: " + Directory.GetParent(SettingPoeLogFilePath) + @"/_Client.txt");
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
            foreach (TrX_ActivityTag tag in _tags)
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
            _tags.Add(tag);
            _myDB.DoNonQuery("INSERT INTO tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type, tag_show_in_lv) VALUES "
                + "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'custom', " + (tag.ShowInListView ? "1" : "0") + ")");

            listViewActLog.Columns.Add(tag.DisplayName);
            ResetMapHistory();
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
            if (String.IsNullOrEmpty(SettingPoeLogFilePath) || !_UpdateCheckDone)
            {
            }
            else
            {
                double dProgress = 0;
                if (!_eventQueueInitizalized)
                {
                    Hide();
                    if (_logLinesRead > 0)
                        dProgress = (_logLinesRead / _logLinesTotal) * 100;
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
                    tag = _tags[iIndex];
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
                    _myDB.DoNonQuery("UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString());
                }
            }
        }

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
                    _myDB.DoNonQuery("UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString());
                }
            }
        }

        private void UpdateTag(string s_id, string s_display_name, string s_forecolor, string s_backcolor, bool b_show_in_hist)
        {
            int iTagIndex = GetTagIndex(s_id);

            if (iTagIndex >= 0)
            {
                _tags[iTagIndex].DisplayName = s_display_name;
                _tags[iTagIndex].ForeColor = Color.FromArgb(Convert.ToInt32(s_forecolor));
                _tags[iTagIndex].BackColor = Color.FromArgb(Convert.ToInt32(s_backcolor));
                _tags[iTagIndex].ShowInListView = b_show_in_hist;

                _myDB.DoNonQuery("UPDATE tx_tags SET tag_display = '" + s_display_name + "', tag_forecolor = '" + s_forecolor + "', tag_bgcolor = '" + s_backcolor + "', " +
                    "tag_show_in_lv = " + (b_show_in_hist ? "1" : "0") + " WHERE tag_id = '" + s_id + "'");
            }

            RenderTagsForConfig(true);
            RenderTagsForTracking(true);
            ResetMapHistory();
        }

        private int GetTagIndex(string s_id)
        {
            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i].ID == s_id)
                {
                    return i;
                }
            }
            return -1;
        }

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

        private void DeleteTag(string s_id)
        {
            int iIndex = GetTagIndex(s_id);
            if (iIndex >= 0)
            {
                TrX_ActivityTag tag = _tags[iIndex];

                if (tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot delete a default tag!");
                }
                else
                {
                    DialogResult dr = MessageBox.Show("Do you really want to delete the tag '" + s_id + "'?", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        _tags.RemoveAt(iIndex);
                        _myDB.DoNonQuery("DELETE FROM tx_tags WHERE tag_id = '" + s_id + "' AND tag_type != 'default'");
                    }
                }
                RenderTagsForConfig(true);
                RenderTagsForTracking(true);
                ResetMapHistory();
            }
        }

        private void DeleteBackup(string s_path)
        {
            Directory.Delete(s_path, true);
            _backups.Remove(listBoxRestoreBackup.SelectedItem.ToString());
        }

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
                    foreach (TrX_TrackedActivity ta in _eventHistory)
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
                    foreach (TrX_TrackedActivity ta in _eventHistory)
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


        // =========> EVENT HANDLERS FOR GUI COMPONENTS
        // =======================================================

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxLogView.Text = "";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_eventQueueInitizalized)
            {
                SaveStatsCache();
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
            if (_currentActivity != null)
                _currentActivity.Resume();
        }

        private void pictureBox13_Click_1(object sender, EventArgs e)
        {
            if (_currentActivity != null)
                _currentActivity.Pause();
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewStats.SelectedItems.Count > 0)
                RefreshChart();
        }


        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            buttonRefreshChart.Focus();
            if (comboBoxTimeRangeStats.SelectedIndex > 5)
            {
                if (MessageBox.Show("Selecting more than 3 month could lead to high loading times. Continue?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (listViewStats.SelectedItems.Count > 0)
                        RefreshChart();
                }
                else
                {
                    comboBoxTimeRangeStats.SelectedIndex = 0;
                }
            }
            else
            {
                if (listViewStats.SelectedItems.Count > 0)
                    RefreshChart();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listViewStats.SelectedItems.Count > 0)
                RefreshChart();
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                if (_isMapZana && _currentActivity.ZanaMap != null)
                {
                    if (_currentActivity.ZanaMap.ManuallyPaused)
                    {
                        _currentActivity.ZanaMap.Resume();
                    }
                }
                else
                {
                    if (_currentActivity.ManuallyPaused)
                    {
                        _currentActivity.Resume();
                    }
                }
            }
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                if (_isMapZana && _currentActivity.ZanaMap != null)
                {
                    if (!_currentActivity.ZanaMap.ManuallyPaused)
                    {
                        _currentActivity.ZanaMap.Pause();
                    }
                }
                else
                {
                    if (!_currentActivity.ManuallyPaused)
                    {
                        _currentActivity.Pause();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listViewActLog.SelectedItems.Count == 1)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                long lTimestamp = _eventHistory[iIndex].TimeStamp;
                string sType = listViewActLog.Items[iIndex].SubItems[1].Text;
                string sArea = listViewActLog.Items[iIndex].SubItems[2].Text;

                if (MessageBox.Show("Do you really want to delete this Activity? " + Environment.NewLine
                    + Environment.NewLine
                    + "Type: " + sType + Environment.NewLine
                    + "Area: " + sArea + Environment.NewLine
                    + "Time: " + listViewActLog.Items[iIndex].SubItems[0].Text, "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    listViewActLog.Items.Remove(listViewActLog.SelectedItems[0]);
                    _eventHistory.RemoveAt(iIndex);
                    DeleteActLogEntry(lTimestamp);
                }
            }
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
                ClearActivityLog();
                ReloadLogFile();
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
                    ReloadLogFile();
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ResetMapHistory();
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
            listViewStats.GridLines = _showGridInStats;
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
                    PrepareBackupRestore(_myAppData + listBoxRestoreBackup.SelectedItem.ToString());
                }
            }
        }

        private void panelTags_SizeChanged(object sender, EventArgs e)
        {
            if (_eventQueueInitizalized)
                RenderTagsForTracking(true);
        }

        private void panelEditTags_SizeChanged(object sender, EventArgs e)
        {
            if (_eventQueueInitizalized)
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
                DeleteBackup(_myAppData + listBoxRestoreBackup.SelectedItem.ToString());
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

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearchStats.Text == String.Empty)
            {
                _lvmStats.Reset();
            }
            _lvmStats.ApplyFullTextFilter(textBoxSearchStats.Text);
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenChildWindow(new SearchHelp());
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox8.Text = "";
            DoSearch();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBoxSearchStats.Text = "";
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
                    string sBaseDir = new FileInfo(SettingPoeLogFilePath).DirectoryName;
                    File.Copy(SettingPoeLogFilePath, sBaseDir + @"\Client." + sDt + ".txt");
                    FileStream fs1 = new FileStream(SettingPoeLogFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                    _lastHash = 0;
                    SaveStatsCache();

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
            Process.Start(APPINFO.WIKI_URL);
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
            DoSearch();
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
                int iMap = Convert.ToInt32(textBoxMapCap.Text);
                int iHeist = Convert.ToInt32(textBoxHeistCap.Text);
                int iLab = Convert.ToInt32(textBoxLabCap.Text);

                if (iMap > 0 && iHeist > 0 && iLab > 0)
                {
                    _timeCapMap = iMap;
                    _timeCapLab = iLab;
                    _timeCapHeist = iHeist;

                    AddUpdateAppSettings("TimeCapMap", _timeCapMap.ToString());
                    AddUpdateAppSettings("TimeCapLab", _timeCapLab.ToString());
                    AddUpdateAppSettings("TimeCapHeist", _timeCapHeist.ToString());

                    RenderGlobalDashboard();
                    RenderMappingDashboard();
                    RenderHeistDashboard();
                    RenderLabDashboard();
                }
                else
                {
                    MessageBox.Show("Time cap values must be greater than 0");
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                MessageBox.Show("Invalid format for time cap. Only integers are supported");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Process.Start(APPINFO.WIKI_URL_SETTINGS);
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
                File.Create(_myAppData + @"\IS_SAFE_RELOAD");
                ReloadLogFile();
            }
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                FinishActivity(_currentActivity, null, _currentActivity.Type, DateTime.Now);
            }

        }

        private void pictureBox15_Click_1(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                FinishActivity(_currentActivity, null, _currentActivity.Type, DateTime.Now);
            }
        }

    }
}