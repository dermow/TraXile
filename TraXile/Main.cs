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
        CAMPAIGN,
        ABYSSAL_DEPTHS,
        VAAL_SIDEAREA,
        LAB_TRIAL,
        LOGBOOK,
        SAFEHOUSE,
        CATARINA_FIGHT,
        LOGBOOK_SIDE,
        BREACHSTONE
    }

    public partial class Main : Form
    {
        // START FLAGS
        public readonly bool IS_IN_DEBUG_MODE = false;
        public bool SAFE_RELOAD_MODE;

        // App parameters
        private readonly string _dbPath;
        private readonly string _cachePath;
        private readonly string _myAppData;
        private bool _exit;
        private bool _listViewInitielaized;
        private bool _showGridInActLog;
        private bool _restoreMode;
        private bool _uiFlagLabDashboard;
        private bool _showGridInStats;
        private bool _UpdateCheckDone;
        private bool _restoreOk = true;
        private bool _StartedFlag = false;
        private bool _nextAreaIsExp = false;
        private int _timeCapLab = 3600;
        private int _timeCapMap = 3600;
        private int _timeCapHeist = 3600;
        private int _actLogItemCount = 0;
        private string _allStatsSelected;
        private long _oldestTimeStamp = 0;
        private bool _autoStartsDOne = false;

        // GUI Update Flags
        private bool _uiFlagMapDashboard;
        private bool _labDashboardHideUnknown;
        private bool _uiFlagGlobalDashboard;
        private bool _uiFlagHeistDashboard;
        private bool _uiFlagActivityList;
        private bool _uiFlagAllStatsDashboard;


        // Core Logic variables
        private string _currentArea;
        private string _currentInstanceEndpoint;
        private string _lastSimuEndpoint;
        private string _failedRestoreReason = "";
        private bool _eventQueueInitizalized;
        private bool _isMapZana;
        private bool _isMapVaalArea;
        private bool _isMapAbyssArea;
        private bool _isMapLabTrial;
        private bool _isMapLogbookSide;
        private int _shaperKillsInFight;
        private int _nextAreaLevel;
        private int _currentAreaLevel;
        private int _lastHash = 0;
        private double _logLinesTotal;
        private double _logLinesRead;
        private bool _historyInitialized;
        private Dictionary<int, string> _dict;
        private Dictionary<string, string> _statNamesLong;
        private List<string> _knownPlayerNames;
        private List<string> labs;
        private List<TrX_ActivityTag> _tags;
        private List<TrX_TrackedActivity> _eventHistory;
        private TrX_EventMapping _eventMapping;
        private TrX_DefaultMappings _defaultMappings;
        private TrX_DBManager _myDB;
        private TrX_StatsManager _myStats;
        private List<TrX_TrackedActivity> _statsDataSource;
        private ConcurrentQueue<TrX_TrackingEvent> _eventQueue;
        public TrX_TrackedActivity _currentActivity;
        private TrX_TrackedActivity _prevActivity;
        private TrX_TrackedActivity _prevActivityOverlay;
        private EVENT_TYPES _lastEventTypeConq;
        private Thread _logParseThread;
        private Thread _eventThread;
        private DateTime _inAreaSince;
        private DateTime _lastDeathTime;
        private DateTime _initStartTime;
        private DateTime _initEndTime;
        private DateTime _statsDate1;
        private DateTime _statsDate2;
        private string _lastShaperInstance;
        private string _lastElderInstance;

        // Hideout time
        private DateTime _hoStart;
        private bool _trackingHO;

        // Other variables
        private LoadScreen _loadScreenWindow;
        private StopWatchOverlay _stopwatchOverlay;
        private BindingList<string> _backups;
        private Dictionary<string, Label> _tagLabels, _tagLabelsConfig;
        private List<string> _parsedActivities;
        private readonly TrX_SettingsManager _mySettings;
        private TrX_ListViewManager _lvmActlog, _lvmAllStats;
        private TrX_Theme _myTheme;
        private List<TrX_LeagueInfo> _leagues;
        private ILog _log;
        private bool _showHideoutInPie;
        private int _stopwatchOverlayOpacity;
        private bool _stopwatchOverlayShowDefault;
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


        private List<TrX_TrackedActivity> FilterActivitiesByTimeRange(DateTime dt1, DateTime dt2)
        {
            List<TrX_TrackedActivity> results;
            results = new List<TrX_TrackedActivity>();

            DateTime date1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 0, 0, 0);
            DateTime date2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, 23, 59, 59);

            _statsDate1 = date1;
            _statsDate2 = date2;

            foreach (TrX_TrackedActivity act in _eventHistory)
            {
                if(act.Started >= date1 && act.Started <= date2)
                {
                    results.Add(act);
                }
            }

            return results;
        }

        private TrX_LeagueInfo GetLeagueByName(string name)
        {
            foreach(TrX_LeagueInfo li in _leagues)
            {
                if (li.Name == name)
                    return li;
            }
            return null;
        }


        private void SetTimeRangeFilter(bool render = false)
        {
            if (comboBox1.SelectedItem == null || _statsDataSource == null)
                return;

            _statsDataSource.Clear();
            if (comboBox1.SelectedItem.ToString() == "All")
            {
                _statsDataSource.AddRange(_eventHistory);
                _statsDate1 = DateTimeOffset.FromUnixTimeSeconds(_oldestTimeStamp).DateTime;
                _statsDate2 = DateTime.Now;
                dateTimePicker1.Value = _statsDate1;
                dateTimePicker2.Value = _statsDate2;
            }
            else if (comboBox1.SelectedItem.ToString().Contains("League:"))
            {
                string sLeague = comboBox1.SelectedItem.ToString().Split(' ')[1];
                TrX_LeagueInfo li = GetLeagueByName(sLeague);

                DateTime dt1 = li.Start;
                DateTime dt2 = li.End;

                _statsDataSource = FilterActivitiesByTimeRange(dt1, dt2);

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

                _statsDataSource = FilterActivitiesByTimeRange(date1, date2);

                if(comboBox1.SelectedItem.ToString() != "Custom")
                {
                    dateTimePicker1.Value = date1;
                    dateTimePicker2.Value = date2;
                }

            }

            if(render)
            {
                RenderMappingDashboard();
                RenderHeistDashboard();
                RenderLabDashboard();
                RenderGlobalDashboard();
                RenderAllStatsDashboard();
                UpdateAllStatsChart();
                RenderBossingDashboard();
            }
            
        }

        private int FindEventLogIndexByID(string id)
        {
            foreach(TrX_TrackedActivity act in _eventHistory)
            {
                if(act.UniqueID == id)
                {
                    return _eventHistory.IndexOf(act);
                }
            }
            return -1;
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

                foreach (XmlNode xn in xml.SelectNodes(string.Format("/version/changelog/chg[@version='{0}']", sVersion)))
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

            _lvmActlog = new TrX_ListViewManager(listViewActLog);
            _lvmAllStats = new TrX_ListViewManager(listViewNF1);
            comboBox1.SelectedIndex = 0;

            _eventMapping = new TrX_EventMapping();
            _defaultMappings = new TrX_DefaultMappings();
            _parsedActivities = new List<string>();
            _leagues = new List<TrX_LeagueInfo>();
            _stopwatchOverlay = new StopWatchOverlay(this, imageList2);
            
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

            var ca = chart1.ChartAreas["ChartArea1"].CursorX;
            ca.IsUserEnabled = true;
            ca.IsUserSelectionEnabled = true;

            textBoxLogFilePath.Text = ReadSetting("PoELogFilePath");
            textBoxLogFilePath.Enabled = false;


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
            _statsDataSource = new List<TrX_TrackedActivity>();
            _statsDate1 = DateTime.Now.AddYears(-100);
            _statsDate2 = DateTime.Now.AddDays(1);

            

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
            _myStats = new TrX_StatsManager(_myDB);
            InitDefaultTags();

            _lastEventTypeConq = EVENT_TYPES.APP_STARTED;
            InitNumStats();
            InitLeagueInfo();

            _eventQueue.Enqueue(new TrX_TrackingEvent(EVENT_TYPES.APP_STARTED) { EventTime = DateTime.Now, LogLine = "Application started." });

            if (ReadStatsCache() == false) // when stats cache could not be reasd
            {
                _log.Info("unable to load stats.cache -> trying to restore from DB");
                try
                {
                    RestoreStatsCacheFromDB();
                }
                // SEVERE ERROR DURING RESTORE -> This should never happen
                // It means, stats.cache AND kvstore in db is corrupted
                catch(Exception ex)
                {
                    _log.Fatal(string.Format("Unable to restore stats.cache from DB: {0}", ex.Message));
                    _log.Debug(ex.ToString());
                    _log.Info("Forcing full logfile reload for stats.");

                    // Drop statistic values and enable stat_reload mode
                    _myDB.DoNonQuery("DELETE FROM tx_stats WHERE timestamp > 0");
                    SAFE_RELOAD_MODE = true;                    
                }
                
            }
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
            _uiFlagLabDashboard = true;
            _uiFlagMapDashboard = true;
            _uiFlagBossDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagActivityList = true;
            _uiFlagAllStatsDashboard = true;
        }

        private void InitLeagueInfo()
        {
            _leagues.Clear();
            _leagues.Add(new TrX_LeagueInfo("Harbinger", 3, 0, new DateTime(2017, 8, 4, 20, 0, 0), new DateTime(2017, 12, 4, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Abyss", 3, 1, new DateTime(2017, 12, 8, 20, 0, 0), new DateTime(2018, 2, 26, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Bestiary", 3, 2, new DateTime(2018, 3, 2, 20, 0, 0), new DateTime(2018, 5, 28, 22, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Incursion", 3, 3, new DateTime(2018, 6, 1, 20, 0, 0), new DateTime(2018, 8, 27, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Delve", 3, 4, new DateTime(2018, 8, 31, 20, 0, 0), new DateTime(2018, 12, 3, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Betrayal", 3, 5, new DateTime(2018, 12, 7, 20, 0, 0), new DateTime(2019, 3, 4, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Synthesis", 3, 6, new DateTime(2019, 3, 8, 20, 0, 0), new DateTime(2019, 6, 3, 22, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Legion", 3, 7, new DateTime(2019, 6, 7, 20, 0, 0), new DateTime(2019, 9, 3, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Blight", 3, 8, new DateTime(2019, 9, 6, 20, 0, 0), new DateTime(2019, 12, 9, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Metamorph", 3, 9, new DateTime(2019, 12, 13, 20, 0, 0), new DateTime(2020, 3, 9, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Delirium", 3, 10, new DateTime(2020, 3, 13, 20, 0, 0), new DateTime(2020, 6, 15, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Harvest", 3, 11, new DateTime(2020, 6, 19, 20, 0, 0), new DateTime(2020, 9, 14, 22, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Heist", 3, 12, new DateTime(2020, 9, 18, 20, 0, 0), new DateTime(2021, 1, 11, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Ritual", 3, 13, new DateTime(2021, 1, 15, 20, 0, 0), new DateTime(2021, 1, 15, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Ultimatum", 3, 14, new DateTime(2021, 4, 16, 20, 0, 0), new DateTime(2021, 07, 19, 22, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Expedition", 3, 15, new DateTime(2021, 7, 23, 20, 0, 0), new DateTime(2021, 10, 19, 21, 0, 0)));
            _leagues.Add(new TrX_LeagueInfo("Scourge", 3, 16, new DateTime(2021, 10, 22, 20, 0, 0), new DateTime(2022, 01, 31, 21, 0, 0)));

            List<TrX_LeagueInfo> litmp = new List<TrX_LeagueInfo>();
            litmp.AddRange(_leagues);
            litmp.Reverse();

            foreach(TrX_LeagueInfo li in litmp)
            {
                comboBox1.Items.Add(string.Format("League: {0} ({1})", li.Name, li.Version));
            }
        }

        // Self-Healing for stats.cache
        private void RestoreStatsCacheFromDB()
        {
            SqliteDataReader dr1 = _myDB.GetSQLReader("select value from tx_kvstore where key='last_hash'");
            while (dr1.Read())
            {
                _lastHash = Convert.ToInt32(dr1.GetString(0));
            }

            List<string> stats = new List<string>();
            foreach(KeyValuePair<string, int> kvp in _myStats.NumericStats)
            {
                stats.Add(kvp.Key);
            }

            foreach (string s in stats)
            {
                SqliteDataReader dr2 = _myDB.GetSQLReader(string.Format("select stat_value from tx_stats where stat_name ='{0}' order by timestamp desc limit 1", s));
                while(dr2.Read())
                {
                    _myStats.NumericStats[s] = dr2.GetInt32(0);
                }
            }

            _log.Info("stats.cache successfully restored.");
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
                new TrX_ActivityTag("huck") { BackColor = Color.IndianRed, ForeColor = Color.Black },
                new TrX_ActivityTag("vaal-area") { BackColor = Color.DarkRed, ForeColor = Color.White },
                new TrX_ActivityTag("lab-trial") { BackColor = Color.DarkTurquoise, ForeColor = Color.Black },
                new TrX_ActivityTag("abyss-depths") { BackColor = Color.ForestGreen, ForeColor = Color.Black },
                new TrX_ActivityTag("exp-side-area") { BackColor = Color.Turquoise, ForeColor = Color.Black },
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
                        TrX_TrackedActivity mapToCheck = _isMapZana ? _currentActivity.SideArea_ZanaMap : _currentActivity;

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
                    if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                    {
                        if (_currentActivity.SideArea_ZanaMap.HasTag(tag.ID))
                        {
                            _currentActivity.SideArea_ZanaMap.RemoveTag(tag.ID);
                        }
                        else
                        {
                            _currentActivity.SideArea_ZanaMap.AddTag(tag.ID);
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
            _myStats.NumericStats = new Dictionary<string, int>
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
                { "Suicides", 0 },
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
                { "Suicides", "Suicides" }
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
                if (!_myStats.NumericStats.ContainsKey("HeistsFinished_" + sName))
                    _myStats.NumericStats.Add("HeistsFinished_" + sName, 0);
                if (!_statNamesLong.ContainsKey("HeistsFinished_" + sName))
                    _statNamesLong.Add("HeistsFinished_" + sName, "Heists done: " + sName);
            }

            foreach (string s in labs)
            {
                string sName = s.Replace("'", "");
                if (!_myStats.NumericStats.ContainsKey("LabsCompleted_" + sName))
                    _myStats.NumericStats.Add("LabsCompleted_" + sName, 0);
                if (!_statNamesLong.ContainsKey("LabsCompleted_" + sName))
                    _statNamesLong.Add("LabsCompleted_" + sName, "Labs completed: " + sName);
            }

            foreach (string s in _defaultMappings.MAP_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!_myStats.NumericStats.ContainsKey("MapsFinished_" + sName))
                    _myStats.NumericStats.Add("MapsFinished_" + sName, 0);
                if (!_statNamesLong.ContainsKey("MapsFinished_" + sName))
                    _statNamesLong.Add("MapsFinished_" + sName, "Maps done: " + sName);
            }

            for (int i = 0; i <= 19; i++)
            {
                string sShort = "MapTierFinished_T" + i.ToString();
                string sLong = i > 0 ? ("Maps done: T" + i.ToString()) : "Maps done: Tier unknown";
                if (!_myStats.NumericStats.ContainsKey(sShort))
                    _myStats.NumericStats.Add(sShort, 0);
                if (!_statNamesLong.ContainsKey(sShort))
                    _statNamesLong.Add(sShort, sLong);
            }

            foreach (string s in _defaultMappings.SIMU_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!_myStats.NumericStats.ContainsKey("SimulacrumFinished_" + sName))
                    _myStats.NumericStats.Add("SimulacrumFinished_" + sName, 0);
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
                case "vaal_sidearea":
                    return ACTIVITY_TYPES.VAAL_SIDEAREA;
                case "abyssal_depths":
                    return ACTIVITY_TYPES.ABYSSAL_DEPTHS;
                case "lab_trial":
                    return ACTIVITY_TYPES.LAB_TRIAL;
                case "logbook":
                    return ACTIVITY_TYPES.LOGBOOK;
                case "logbook_side":
                    return ACTIVITY_TYPES.LOGBOOK_SIDE;
                case "catarina_fight":
                    return ACTIVITY_TYPES.CATARINA_FIGHT;
                case "safehouse":
                    return ACTIVITY_TYPES.SAFEHOUSE;
                case "breachstone":
                    return ACTIVITY_TYPES.BREACHSTONE;
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

            // Get oldest TS
            _oldestTimeStamp = _myStats.GetOldestTimeStamp();

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
                if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                {
                    currentAct = _currentActivity.SideArea_ZanaMap;
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
                    PauseCurrentActivityOrSide();
                    break;
                case "resume":
                    ResumeCurrentActivityOrSide();
                    break;
                case "finish":
                    if (currentAct != null && !_isMapZana)
                    {
                        MethodInvoker mi = delegate
                        {
                            FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, DateTime.Now);
                            _prevActivityOverlay = _currentActivity;
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
            bool bSourceAreaIsVaal = _defaultMappings.VAAL_AREAS.Contains(sSourceArea);
            bool bSourceAreaIsAbyss = _defaultMappings.ABYSS_AREAS.Contains(sSourceArea);
            bool bSourceAreaIsLabTrial = sSourceArea.Contains("Trial of");
            bool bSourceAreaIsLogbookSide = _defaultMappings.LOGBOOK_SIDE_AREAS.Contains(sSourceArea);
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
            bool bTargetAreaIsLabTrial = sTargetArea.Contains("Trial of");
            bool bTargetAreaIsAbyssal = _defaultMappings.ABYSS_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsVaal = _defaultMappings.VAAL_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsLogbook = _defaultMappings.LOGBOOK_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsLogBookSide = _defaultMappings.LOGBOOK_SIDE_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsCata = _defaultMappings.CATARINA_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsSafehouse = _defaultMappings.SAFEHOUSE_AREAS.Contains(sTargetArea);
            bool bTargetAreaIsBreachStone = _defaultMappings.BREACHSTONE_AREAS.Contains(sTargetArea);
            long lTS = ((DateTimeOffset)ev.EventTime).ToUnixTimeSeconds();

            _inAreaSince = ev.EventTime;

            IncrementStat("AreaChanges", ev.EventTime, 1);


            // Calculate Instance change based statistics:
            // ===========================================

            // Shaper
            if(bTargetAreaIsShaper && _currentInstanceEndpoint != _lastShaperInstance)
            {
                IncrementStat("ShaperTried", ev.EventTime, 1);
                _lastShaperInstance = _currentInstanceEndpoint;
            }

            // Elder
            if(bTargetAreaIsElder && _currentInstanceEndpoint != _lastElderInstance)
            {
                _log.Debug("ELDER_TRIED");
                IncrementStat("ElderTried", ev.EventTime, 1);
                _lastElderInstance = _currentInstanceEndpoint;
            }

            // Track the very first activity
            if ((!sTargetArea.Contains("Hideout")) && (!_defaultMappings.CAMP_AREAS.Contains(sTargetArea)))
            {
                _StartedFlag = false;
            }

            // Hideout?
            if (sTargetArea.Contains("Hideout") && !sTargetArea.Contains("Syndicate"))
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

                    _prevActivityOverlay = GetLastActivityByType(ACTIVITY_TYPES.SIMULACRUM);

                    _nextAreaLevel = 0;
                }
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
            else if (bTargetAreaIsAbyssal)
            {
                actType = ACTIVITY_TYPES.ABYSSAL_DEPTHS;
            }
            else if (bTargetAreaIsLabTrial)
            {
                actType = ACTIVITY_TYPES.LAB_TRIAL;
            }
            else if (bTargetAreaIsVaal)
            {
                actType = ACTIVITY_TYPES.VAAL_SIDEAREA;
            }
            else if (bTargetAreaIsLogbook)
            {
                actType = ACTIVITY_TYPES.LOGBOOK;
            }
            else if (bTargetAreaIsLogBookSide)
            {
                actType = ACTIVITY_TYPES.LOGBOOK_SIDE;
            }
            else if (bTargetAreaIsCata)
            {
                actType = ACTIVITY_TYPES.CATARINA_FIGHT;
            }
            else if (bTargetAreaIsSafehouse)
            {
                actType = ACTIVITY_TYPES.SAFEHOUSE;
            }
            else if(bTargetAreaIsBreachStone)
            {
                actType = ACTIVITY_TYPES.BREACHSTONE;
            }
                       
            // Special handling for logbook cemetery + vaal temple
            if (bTargetAreaIsLogbook && bTargetAreaIsMap)
            {
                actType = _nextAreaIsExp ? ACTIVITY_TYPES.LOGBOOK : ACTIVITY_TYPES.MAP;
                _nextAreaIsExp = false;
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
                    if (IS_IN_DEBUG_MODE)
                        _currentActivity.DebugEndEventLine = ev.LogLine;
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

                _prevActivityOverlay = GetLastActivityByType(actType);

                IncrementStat("LabsStarted", ev.EventTime, 1);

            }

            //Aspirants Trial entered
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
                {if (IS_IN_DEBUG_MODE)
                        _currentActivity.DebugEndEventLine = ev.LogLine;
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                }
            }

            // Vaal Side area entered?
            if(_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && actType == ACTIVITY_TYPES.VAAL_SIDEAREA)
            {
                if(_currentActivity.SideArea_VaalArea == null)
                {
                    _currentActivity.SideArea_VaalArea = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        AreaLevel = _nextAreaLevel,
                        Type = actType,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentActivity.InstanceEndpoint
                    };
                    _currentActivity.AddTag("vaal-area");
                }
                _currentActivity.SideArea_VaalArea.StartStopWatch();
                _currentActivity.SideArea_VaalArea.EndPauseTime(ev.EventTime);
                _isMapVaalArea = true;
            }
            else
            {
                _isMapVaalArea = false;
            }

            // Left Vaal Side area?
            if(_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && bSourceAreaIsVaal)
            {
                if(_currentActivity.SideArea_VaalArea != null)
                {
                    _currentActivity.SideArea_VaalArea.LastEnded = ev.EventTime;
                    _currentActivity.SideArea_VaalArea.StopStopWatch();
                    _currentActivity.SideArea_VaalArea.StartPauseTime(ev.EventTime);
                }
            }

            // Logbook Side area entered?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LOGBOOK && actType == ACTIVITY_TYPES.LOGBOOK_SIDE)
            {
                if (_currentActivity.SideArea_LogbookSide == null)
                {
                    _currentActivity.SideArea_LogbookSide = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        AreaLevel = _nextAreaLevel,
                        Type = actType,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentActivity.InstanceEndpoint
                    };
                    _currentActivity.AddTag("exp-side-area");

                }
                _currentActivity.SideArea_LogbookSide.StartStopWatch();
                _currentActivity.SideArea_LogbookSide.EndPauseTime(ev.EventTime);
                _isMapLogbookSide = true;
            }
            else
            {
                _isMapLogbookSide = false;
            }

            // Left Logbook Side area?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.LOGBOOK && bSourceAreaIsLogbookSide)
            {
                if (_currentActivity.SideArea_LogbookSide != null)
                {
                    _currentActivity.SideArea_LogbookSide.LastEnded = ev.EventTime;
                    _currentActivity.SideArea_LogbookSide.StopStopWatch();
                    _currentActivity.SideArea_LogbookSide.StartPauseTime(ev.EventTime);
                }
            }

            // Abyss Side area entered?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && actType == ACTIVITY_TYPES.ABYSSAL_DEPTHS)
            {
                if (_currentActivity.SideArea_AbyssArea == null)
                {
                    _currentActivity.SideArea_AbyssArea = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        AreaLevel = _nextAreaLevel,
                        Type = actType,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentActivity.InstanceEndpoint
                    };
                    _currentActivity.AddTag("abyss-depths");
                }
                _currentActivity.SideArea_AbyssArea.StartStopWatch();
                _currentActivity.SideArea_AbyssArea.EndPauseTime(ev.EventTime);
                _isMapAbyssArea = true;
            }
            else
            {
                _isMapAbyssArea = false;
            }

            // Left Abyss Side area?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && bSourceAreaIsAbyss)
            {
                if (_currentActivity.SideArea_AbyssArea != null)
                {
                    _currentActivity.SideArea_AbyssArea.LastEnded = ev.EventTime;
                    _currentActivity.SideArea_AbyssArea.StopStopWatch();
                    _currentActivity.SideArea_AbyssArea.StartPauseTime(ev.EventTime);
                }
            }

            // Lab Side area entered?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && actType == ACTIVITY_TYPES.LAB_TRIAL)
            {
                if (_currentActivity.SideArea_LabTrial == null)
                {
                    _currentActivity.SideArea_LabTrial = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        AreaLevel = _nextAreaLevel,
                        Type = actType,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentActivity.InstanceEndpoint
                    };
                    _currentActivity.AddTag("lab-trial");
                }
                _currentActivity.SideArea_LabTrial.StartStopWatch();
                _currentActivity.SideArea_LabTrial.EndPauseTime(ev.EventTime);
                _isMapLabTrial = true;
            }
            else
            {
                _isMapLabTrial = false;
            }

            // Left Abyss Side area?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && bSourceAreaIsLabTrial)
            {
                if (_currentActivity.SideArea_LabTrial != null)
                {
                    _currentActivity.SideArea_LabTrial.LastEnded = ev.EventTime;
                    _currentActivity.SideArea_LabTrial.StopStopWatch();
                    _currentActivity.SideArea_LabTrial.StartPauseTime(ev.EventTime);
                }
            }

            // Delving?
            if ((_currentActivity == null || _currentActivity.Type != ACTIVITY_TYPES.DELVE) && actType == ACTIVITY_TYPES.DELVE)
            {
                // Finish activity
                if (_currentActivity != null)
                {if (IS_IN_DEBUG_MODE)
                        _currentActivity.DebugEndEventLine = ev.LogLine;
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

                _prevActivityOverlay = GetLastActivityByType(actType);
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
            {if (IS_IN_DEBUG_MODE)
                    _currentActivity.DebugEndEventLine = ev.LogLine;
                FinishActivity(_currentActivity, null, ACTIVITY_TYPES.DELVE, DateTime.Now);
            }


            //Campaign ?
            if (bTargetAreaIsCampaign)
            {
                // Do not track first camp visit after portal activity
                // Do not finish when logging in other char
                bool bFromActivity = bSourceAreaIsMap
                    || sSourceArea == "Aspirants Trial"
                    || _defaultMappings.SIMU_AREAS.Contains(sSourceArea)
                    || _defaultMappings.LOGBOOK_AREAS.Contains(sSourceArea)
                    || _defaultMappings.TEMPLE_AREAS.Contains(sSourceArea)
                    || _defaultMappings.ATZIRI_AREAS.Contains(sSourceArea)
                    || _defaultMappings.CATARINA_AREAS.Contains(sSourceArea)
                    || _defaultMappings.ELDER_AREAS.Contains(sSourceArea)
                    || _defaultMappings.MAVEN_FIGHT_AREAS.Contains(sSourceArea)
                    || _defaultMappings.SAFEHOUSE_AREAS.Contains(sSourceArea)
                    || _defaultMappings.SHAPER_AREAS.Contains(sSourceArea)
                    || _defaultMappings.SIRUS_AREAS.Contains(sSourceArea)
                    || _defaultMappings.UBER_ATZIRI_AREAS.Contains(sSourceArea);

                // Do not track first town visit after login
                if (!_StartedFlag && !bFromActivity)
                {
                    if (_currentActivity != null)
                    {

                        if (sTargetArea != _currentActivity.Area || _currentInstanceEndpoint != _currentActivity.InstanceEndpoint)
                        {
                            _currentActivity.LastEnded = ev.EventTime;
                            if (IS_IN_DEBUG_MODE)
                                _currentActivity.DebugEndEventLine = ev.LogLine;
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

                        _prevActivityOverlay = GetLastActivityByType(actType);
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
            if (bTargetAreaIsMap || bTargetAreaIsHeist || bTargetAreaIsSimu || bTargetAreaIsCampaign || bTargetAreaIsBreachStone)
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
                bTargetAreaIsSirusFight ||
                bTargetAreaIsLogbook ||
                bTargetAreaIsSafehouse ||
                bTargetAreaIsCata ||
                bTargetAreaIsBreachStone;

            // Check if opened activity needs to be opened on Mapdevice
            bool isMapDeviceActivity =
                bTargetAreaIsMap ||
                bTargetAreaIsAtziri ||
                bTargetAreaIsCata ||
                bTargetAreaIsElder ||
                bTargetAreaIsShaper ||
                bTargetAreaIsSimu ||
                bTargetAreaIsSafehouse ||
                bTargetAreaIsSirusFight ||
                bTargetAreaTemple ||
                bTargetAreaIsMI ||
                bTargetAreaIsMavenFight ||
                bTargetAreaIsLogbook ||
                bTargetAreaIsBreachStone;

            if(isMapDeviceActivity)
            {
                if(!bTargetAreaIsShaper)
                {
                    _shaperKillsInFight = 0;
                }

                if(!bTargetAreaIsElder)
                {
                    _lastElderInstance = "";
                }
            }

            if (enteringDefaultTrackableActivity)
            {
                if (_currentActivity == null)
                {
                    _currentActivity = new TrX_TrackedActivity
                    {
                        Area = sTargetArea,
                        Type = actType,
                        AreaLevel = _nextAreaLevel,
                        Started = ev.EventTime,
                        TimeStamp = lTS,
                        InstanceEndpoint = _currentInstanceEndpoint,
                        DebugStartEventLine = ev.LogLine
                    };
                    _nextAreaLevel = 0;

                    _prevActivityOverlay = GetLastActivityByType(actType);
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
                        if (_currentActivity.SideArea_ZanaMap == null)
                        {
                            _currentActivity.SideArea_ZanaMap = new TrX_TrackedActivity
                            {
                                Type = ACTIVITY_TYPES.MAP,
                                Area = sTargetArea,
                                AreaLevel = _nextAreaLevel,
                                Started = ev.EventTime,
                                TimeStamp = lTS,
                            };
                            _currentActivity.SideArea_ZanaMap.AddTag("zana-map");
                            _nextAreaLevel = 0;
                        }
                        if (!_currentActivity.SideArea_ZanaMap.ManuallyPaused)
                            _currentActivity.SideArea_ZanaMap.StartStopWatch();
                    }
                    else
                    {
                        _isMapZana = false;

                        // leave Zana Map
                        if (_currentActivity.SideArea_ZanaMap != null)
                        {
                            _isMapZana = false;
                            _currentActivity.SideArea_ZanaMap.StopStopWatch();
                            _currentActivity.SideArea_ZanaMap.LastEnded = ev.EventTime;
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
                            if(IS_IN_DEBUG_MODE)_currentActivity.DebugEndEventLine = ev.LogLine;
                            FinishActivity(_currentActivity, sTargetArea, actType, ev.EventTime);
                        }
                    }
                }
            }
            else // ENTERING AN AREA WHICH IS NOT AN DEFAULT ACTIVITY
            {
                // Set endtime when logouts
                if (_currentActivity != null && _currentActivity.Type != ACTIVITY_TYPES.LABYRINTH && _currentActivity.Type != ACTIVITY_TYPES.CAMPAIGN)
                {
                    if(_currentActivity.Type == ACTIVITY_TYPES.BREACHSTONE && _defaultMappings.BREACHSTONE_AREAS.Contains(sSourceArea))
                    {
                        _currentActivity.StopStopWatch();
                        _currentActivity.LastEnded = ev.EventTime;
                    }

                    //TEST: Pause when left the source area
                    if (sSourceArea == _currentActivity.Area || _defaultMappings.CAMP_AREAS.Contains(sTargetArea))
                    {
                        _currentActivity.StopStopWatch();
                        _currentActivity.LastEnded = ev.EventTime;

                        // PAUSE TIME
                        if (_defaultMappings.PAUSABLE_ACTIVITY_TYPES.Contains(_currentActivity.Type))
                        {
                            if (_defaultMappings.CAMP_AREAS.Contains(sTargetArea) || (sTargetArea.Contains("Hideout") && !sTargetArea.Contains("Syndicate")))
                            {
                                _currentActivity.StartPauseTime(ev.EventTime);
                            }
                        }
                    }

                    if (_currentActivity.SideArea_ZanaMap != null)
                    {
                        if (sSourceArea == _currentActivity.SideArea_ZanaMap.Area)
                        {
                            _currentActivity.SideArea_ZanaMap.StopStopWatch();
                            _currentActivity.SideArea_ZanaMap.LastEnded = ev.EventTime;
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
                    if (IS_IN_DEBUG_MODE) _currentActivity.DebugEndEventLine = ev.LogLine;
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
                        if (_currentActivity.SideArea_ZanaMap != null)
                        {
                            _currentActivity.SideArea_ZanaMap.DeathCounter++;
                            _currentActivity.SideArea_ZanaMap.LastEnded = ev.EventTime;
                        }
                    }
                    else if (_isMapVaalArea)
                    {
                        if (_currentActivity.SideArea_VaalArea != null)
                        {
                            _currentActivity.SideArea_VaalArea.DeathCounter++;
                            _currentActivity.SideArea_VaalArea.LastEnded = ev.EventTime;
                        }
                    }
                    else if (_isMapAbyssArea)
                    {
                        if (_currentActivity.SideArea_AbyssArea != null)
                        {
                            _currentActivity.SideArea_AbyssArea.DeathCounter++;
                            _currentActivity.SideArea_AbyssArea.LastEnded = ev.EventTime;
                        }
                    }
                    else if (_isMapLabTrial)
                    {
                        if (_currentActivity.SideArea_LabTrial != null)
                        {
                            _currentActivity.SideArea_LabTrial.DeathCounter++;
                            _currentActivity.SideArea_LabTrial.LastEnded = ev.EventTime;
                        }
                    }
                    else if (_isMapLogbookSide)
                    {
                        if (_currentActivity.SideArea_LogbookSide != null)
                        {
                            _currentActivity.SideArea_LogbookSide.DeathCounter++;
                            _currentActivity.SideArea_LogbookSide.LastEnded = ev.EventTime;
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
                            if (IS_IN_DEBUG_MODE) _currentActivity.DebugEndEventLine = ev.LogLine;
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
                                if (_currentActivity.SideArea_ZanaMap != null)
                                {
                                    if(_currentActivity.SideArea_ZanaMap.LastEnded.Year < 2000)
                                    {
                                        _currentActivity.SideArea_ZanaMap = null;
                                    }
                                    else
                                    {
                                        _currentActivity.SideArea_ZanaMap.IsFinished = true;
                                    }
                                    
                                }
                                if (_currentActivity.SideArea_VaalArea != null)
                                {
                                    if (_currentActivity.SideArea_VaalArea.LastEnded.Year < 2000)
                                    {
                                        _currentActivity.SideArea_VaalArea = null;
                                    }
                                    else
                                    {
                                        _currentActivity.SideArea_VaalArea.IsFinished = true;
                                    }

                                }
                                if (_currentActivity.SideArea_LogbookSide != null)
                                {
                                    if (_currentActivity.SideArea_LogbookSide.LastEnded.Year < 2000)
                                    {
                                        _currentActivity.SideArea_LogbookSide = null;
                                    }
                                    else
                                    {
                                        _currentActivity.SideArea_LogbookSide.IsFinished = true;
                                    }

                                }
                                if (_currentActivity.SideArea_AbyssArea != null)
                                {
                                    if (_currentActivity.SideArea_AbyssArea.LastEnded.Year < 2000)
                                    {
                                        _currentActivity.SideArea_AbyssArea = null;
                                    }
                                    else
                                    {
                                        _currentActivity.SideArea_AbyssArea.IsFinished = true;
                                    }

                                }
                                if (_currentActivity.SideArea_LabTrial != null)
                                {
                                    if (_currentActivity.SideArea_LabTrial.LastEnded.Year < 2000)
                                    {
                                        _currentActivity.SideArea_LabTrial = null;
                                    }
                                    else
                                    {
                                        _currentActivity.SideArea_LabTrial.IsFinished = true;
                                    }

                                }
                                if (IS_IN_DEBUG_MODE) _currentActivity.DebugEndEventLine = ev.LogLine;
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
                    case EVENT_TYPES.PLAYER_SUICIDE:
                        IncrementStat("Suicides", ev.EventTime, 1);
                        HandlePlayerDiedEvent(ev);
                        break;
                    case EVENT_TYPES.ELDER_KILLED:
                        _log.Debug("Elder killed on instance: " + _currentActivity.InstanceEndpoint);
                        IncrementStat("ElderKilled", ev.EventTime, 1);
                        _lastElderInstance = ""; //TODO: could be a problem if protaled back to already finished elder fight. But fixes multi fights on same instance...
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
                        if (_lastEventTypeConq != EVENT_TYPES.VERITANIA_KILLED)
                        {
                            IncrementStat("VeritaniaKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.DROX_KILLED:
                        if (_lastEventTypeConq != EVENT_TYPES.DROX_KILLED)
                        {
                            IncrementStat("DroxKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.BARAN_KILLED:
                        if (_lastEventTypeConq != EVENT_TYPES.BARAN_KILLED)
                        {
                            IncrementStat("BaranKilled", ev.EventTime, 1);
                        }
                        break;
                    case EVENT_TYPES.HUNTER_KILLED:
                        if (_lastEventTypeConq != EVENT_TYPES.HUNTER_KILLED)
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
                        //IncrementStat("ShaperTried", ev.EventTime, 1);
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("delirium");
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("blight");
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("einhar");
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("incursion");
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("niko");
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
                            if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                            {
                                _currentActivity.SideArea_ZanaMap.AddTag("syndicate");
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
                            if (iLevel > _myStats.NumericStats["HighestLevel"])
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
                            if (IS_IN_DEBUG_MODE) _currentActivity.DebugEndEventLine = ev.LogLine;
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
                        // Logbook check for cemetery & vaal temple
                        if(ev.LogLine.Contains("Expedition"))
                        {
                            _nextAreaIsExp = true;
                        }
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
                    case EVENT_TYPES.NEXT_CEMETERY_IS_LOGBOOK:
                        _nextAreaIsExp = true;
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
                    _lastEventTypeConq = ev.EventType;
                }

                if (!bInit || ev.EventType == EVENT_TYPES.CATARINA_FIGHT_STARTED) TextLogEvent(ev);
            }
            catch (Exception ex)
            {
                _log.Warn("Error -> Ex.Message: " + ex.Message + ", LogLine: " + ev.LogLine);
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
            _myStats.IncrementStat(s_key, dt, i_value);
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
            _myStats.SetStat(s_key, dt, i_value);
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

        private TrX_TrackedActivity GetLastActivityByType(ACTIVITY_TYPES type)
        {
            List<TrX_TrackedActivity> list = new List<TrX_TrackedActivity>();

            foreach(TrX_TrackedActivity act in _eventHistory)
            {
                if (act.Type == type)
                    list.Add(act);
            }

            if (list.Count > 0)
                return list[0];

            return null;
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

            bool isValid = true;

            if (!SAFE_RELOAD_MODE)
            {
                TimeSpan ts;
                TimeSpan tsZana;
                TimeSpan tsVaal;
                TimeSpan tsAbyss;
                TimeSpan tsLabTrial;
                TimeSpan tsLogbookSide;
                int iSeconds;
                int iSecondsZana = 0;
                int iSecondsVaal = 0;
                int iSecondsAbyss = 0;
                int iSecondsLabTrial = 0;
                int iSecondsLogbookSide = 0;


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

                    if (activity.SideArea_ZanaMap != null)
                    {
                        tsZana = (activity.SideArea_ZanaMap.LastEnded - activity.SideArea_ZanaMap.Started);
                        iSecondsZana = Convert.ToInt32(tsZana.TotalSeconds);
                    }

                    if (activity.SideArea_VaalArea != null)
                    {
                        tsVaal = (activity.SideArea_VaalArea.LastEnded - activity.SideArea_VaalArea.Started);
                        iSecondsVaal = Convert.ToInt32(tsVaal.TotalSeconds);
                    }

                    if (activity.SideArea_LogbookSide != null)
                    {
                        tsLogbookSide = (activity.SideArea_LogbookSide.LastEnded - activity.SideArea_LogbookSide.Started);
                        iSecondsLogbookSide = Convert.ToInt32(tsLogbookSide.TotalSeconds);
                    }

                    if (activity.SideArea_AbyssArea != null)
                    {
                        tsAbyss = (activity.SideArea_AbyssArea.LastEnded - activity.SideArea_AbyssArea.Started);
                        iSecondsAbyss = Convert.ToInt32(tsAbyss.TotalSeconds);
                    }

                    if (activity.SideArea_LabTrial != null)
                    {
                        tsLabTrial = (activity.SideArea_LabTrial.LastEnded - activity.SideArea_LabTrial.Started);
                        iSecondsLabTrial = Convert.ToInt32(tsLabTrial.TotalSeconds);
                    }

                    // Filter out town activities without end date
                    if (activity.LastEnded.Year < 2000)
                    {
                        isValid = false;
                    }

                    // Filter out 0-second town visits
                    if (activity.Type == ACTIVITY_TYPES.CAMPAIGN && iSeconds == 0)
                    {

                        isValid = false;
                    }
                }
                else
                {
                    ts = activity.StopWatchTimeSpan;
                    iSeconds = Convert.ToInt32(ts.TotalSeconds);
                    if (activity.SideArea_ZanaMap != null)
                    {
                        tsZana = activity.SideArea_ZanaMap.StopWatchTimeSpan;
                        iSecondsZana = Convert.ToInt32(tsZana.TotalSeconds);
                    }
                }

                if(isValid)
                {
                    _currentActivity.TotalSeconds = iSeconds;
                    _prevActivity = _currentActivity;

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


                    if (activity.SideArea_ZanaMap != null)
                    {
                        TimeSpan tsZanaMap = TimeSpan.FromSeconds(iSecondsZana);
                        activity.SideArea_ZanaMap.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                               tsZanaMap.Hours, tsZanaMap.Minutes, tsZanaMap.Seconds);
                        _eventHistory.Insert(0, _currentActivity.SideArea_ZanaMap);

                        if (!_parsedActivities.Contains(activity.SideArea_ZanaMap.UniqueID))
                        {
                            AddMapLvItem(activity.SideArea_ZanaMap, true);

                            //Request GUI Update
                            _uiFlagActivityList = true;

                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_ZanaMap.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_ZanaMap.Type), activity.SideArea_ZanaMap.Area, activity.SideArea_ZanaMap.AreaLevel, iSecondsZana, activity.SideArea_ZanaMap.DeathCounter, activity.SideArea_ZanaMap.TrialMasterCount, true, activity.SideArea_ZanaMap
                                .Tags, activity.SideArea_ZanaMap.Success, Convert.ToInt32(activity.SideArea_ZanaMap.PausedTime));
                        }
                    }

                    if (activity.SideArea_VaalArea != null)
                    {
                        TimeSpan tsVaalMap = TimeSpan.FromSeconds(iSecondsVaal);
                        activity.SideArea_VaalArea.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                               tsVaalMap.Hours, tsVaalMap.Minutes, tsVaalMap.Seconds);
                        _eventHistory.Insert(0, _currentActivity.SideArea_VaalArea);

                        if (!_parsedActivities.Contains(activity.SideArea_VaalArea.UniqueID))
                        {
                            AddMapLvItem(activity.SideArea_VaalArea, true);

                            //Request GUI Update
                            _uiFlagActivityList = true;

                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_VaalArea.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_VaalArea.Type), activity.SideArea_VaalArea.Area, activity.SideArea_VaalArea.AreaLevel, iSecondsVaal, activity.SideArea_VaalArea.DeathCounter, activity.SideArea_VaalArea.TrialMasterCount, true, activity.SideArea_VaalArea
                                .Tags, activity.SideArea_VaalArea.Success, Convert.ToInt32(activity.SideArea_VaalArea.PausedTime));
                        }
                    }

                    if (activity.SideArea_LogbookSide != null)
                    {
                        TimeSpan tsLBSide = TimeSpan.FromSeconds(iSecondsLogbookSide);
                        activity.SideArea_LogbookSide.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                               tsLBSide.Hours, tsLBSide.Minutes, tsLBSide.Seconds);
                        _eventHistory.Insert(0, _currentActivity.SideArea_LogbookSide);

                        if (!_parsedActivities.Contains(activity.SideArea_LogbookSide.UniqueID))
                        {
                            AddMapLvItem(activity.SideArea_LogbookSide, true);

                            //Request GUI Update
                            _uiFlagActivityList = true;

                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_LogbookSide.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_LogbookSide.Type), activity.SideArea_LogbookSide.Area, activity.SideArea_LogbookSide.AreaLevel, iSecondsVaal, activity.SideArea_LogbookSide.DeathCounter, activity.SideArea_LogbookSide.TrialMasterCount, true, activity.SideArea_LogbookSide
                                .Tags, activity.SideArea_LogbookSide.Success, Convert.ToInt32(activity.SideArea_LogbookSide.PausedTime));
                        }
                    }

                    if (activity.SideArea_AbyssArea != null)
                    {
                        TimeSpan tsAbyssMap = TimeSpan.FromSeconds(iSecondsAbyss);
                        activity.SideArea_AbyssArea.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                               tsAbyssMap.Hours, tsAbyssMap.Minutes, tsAbyssMap.Seconds);
                        _eventHistory.Insert(0, _currentActivity.SideArea_AbyssArea);

                        if (!_parsedActivities.Contains(activity.SideArea_AbyssArea.UniqueID))
                        {
                            AddMapLvItem(activity.SideArea_AbyssArea, true);

                            //Request GUI Update
                            _uiFlagActivityList = true;

                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_AbyssArea.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_AbyssArea.Type), activity.SideArea_AbyssArea.Area, activity.SideArea_AbyssArea.AreaLevel, iSecondsAbyss, activity.SideArea_AbyssArea.DeathCounter, activity.SideArea_AbyssArea.TrialMasterCount, true, activity.SideArea_AbyssArea
                                .Tags, activity.SideArea_AbyssArea.Success, Convert.ToInt32(activity.SideArea_AbyssArea.PausedTime));
                        }
                    }

                    if (activity.SideArea_LabTrial != null)
                    {
                        TimeSpan tsLabTrial2 = TimeSpan.FromSeconds(iSecondsLabTrial);
                        activity.SideArea_LabTrial.CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}",
                               tsLabTrial2.Hours, tsLabTrial2.Minutes, tsLabTrial2.Seconds);
                        _eventHistory.Insert(0, _currentActivity.SideArea_LabTrial);

                        if (!_parsedActivities.Contains(activity.SideArea_LabTrial.UniqueID))
                        {
                            AddMapLvItem(activity.SideArea_LabTrial, true);

                            //Request GUI Update
                            _uiFlagActivityList = true;

                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_LabTrial.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_LabTrial.Type), activity.SideArea_LabTrial.Area, activity.SideArea_LabTrial.AreaLevel, iSecondsLabTrial, activity.SideArea_LabTrial.DeathCounter, activity.SideArea_LabTrial.TrialMasterCount, true, activity.SideArea_LabTrial
                                .Tags, activity.SideArea_LabTrial.Success, Convert.ToInt32(activity.SideArea_LabTrial.PausedTime));
                        }
                    }

                }
                else
                {
                    _log.Warn(string.Format("Filtered out invalid activity: {0}, Started: {1}, LastEnded: {2}, Type: {3}, Instance: {4}", activity.UniqueID, activity.Started, activity.LastEnded, activity.Type, activity.InstanceEndpoint));
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

                    if (activity.SideArea_ZanaMap != null)
                    {
                        IncrementStat("TotalMapsDone", activity.SideArea_ZanaMap.Started, 1);
                        IncrementStat("MapsFinished_" + activity.SideArea_ZanaMap.Area, activity.SideArea_ZanaMap.Started, 1);
                        IncrementStat("MapTierFinished_T" + activity.SideArea_ZanaMap.MapTier.ToString(), activity.SideArea_ZanaMap.Started, 1);
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
            }

            if (sNextMap != null)
            {
                _prevActivityOverlay = GetLastActivityByType(sNextMapType);

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

            if (_eventQueueInitizalized)
            {
                MethodInvoker mi = delegate
                {
                    RenderGlobalDashboard();
                    RenderHeistDashboard();
                    RenderLabDashboard();
                    RenderMappingDashboard();
                    RenderAllStatsDashboard();
                    RenderBossingDashboard();
                };
                Invoke(mi);

              
            }
        }

        /// <summary>
        /// Read the statistics cache
        /// </summary>
        private bool ReadStatsCache()
        {
            if (File.Exists(_cachePath))
            {
                StreamReader r = new StreamReader(_cachePath);

                try
                {
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
                            if (_myStats.NumericStats.ContainsKey(statID))
                            {
                                _myStats.NumericStats[line.Split(';')[0]] = statValue;
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
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    r.Close();
                }
            }
            else
            {
                return true;
            }
               
        }

        /// <summary>
        /// Write the statistics cache
        /// </summary>
        private void SaveStatsCache()
        {
            // DB
            _myDB.DoNonQuery(string.Format("UPDATE tx_kvstore SET value='{0}' where key='last_hash'", _lastHash));

            StreamWriter wrt = new StreamWriter(_cachePath);
            wrt.WriteLine("last;" + _lastHash.ToString());
            foreach (KeyValuePair<string, int> kvp in _myStats.NumericStats)
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

        public string GetBreachStoneName(string s_ara, int i_area_level)
        {
            string breachLoard = "";
            string breachStoneQuality = "";

            switch (s_ara)
            {
                case "Chayulas Domain":
                    breachLoard = "Chayulas";
                    switch (i_area_level)
                    {
                        // Normal
                        case 80:
                            breachStoneQuality = "";
                            break;
                        // Charged
                        case 81:
                            breachStoneQuality = "Charged";
                            break;
                        // Enriched
                        case 82:
                            breachStoneQuality = "Enriched";
                            break;
                        // Pure
                        case 83:
                            breachStoneQuality = "Pure";
                            break;
                        // Flawless
                        case 84:
                            breachStoneQuality = "Flawless";
                            break;
                    }
                    break;
                case "Eshs Domain":
                    breachLoard = "Eshs";
                    switch (i_area_level)
                    {
                        // Normal
                        case 70:
                            breachStoneQuality = "";
                            break;
                        // Charged
                        case 74:
                            breachStoneQuality = "Charged";
                            break;
                        // Enriched
                        case 79:
                            breachStoneQuality = "Enriched";
                            break;
                        // Pure
                        case 81:
                            breachStoneQuality = "Pure";
                            break;
                        // Flawless
                        case 84:
                            breachStoneQuality = "Flawless";
                            break;
                    }
                    break;
                case "Xophs Domain":
                    breachLoard = "Xophs";
                    switch (i_area_level)
                    {
                        // Normal
                        case 70:
                            breachStoneQuality = "";
                            break;
                        // Charged
                        case 74:
                            breachStoneQuality = "Charged";
                            break;
                        // Enriched
                        case 79:
                            breachStoneQuality = "Enriched";
                            break;
                        // Pure
                        case 81:
                            breachStoneQuality = "Pure";
                            break;
                        // Flawless
                        case 84:
                            breachStoneQuality = "Flawless";
                            break;
                    }
                    break;
                case "Uul-Netols Domain":
                    breachLoard = "Uul-Netols";
                    switch (i_area_level)
                    {
                        // Normal
                        case 75:
                            breachStoneQuality = "";
                            break;
                        // Charged
                        case 78:
                            breachStoneQuality = "Charged";
                            break;
                        // Enriched
                        case 81:
                            breachStoneQuality = "Enriched";
                            break;
                        // Pure
                        case 82:
                            breachStoneQuality = "Pure";
                            break;
                        // Flawless
                        case 84:
                            breachStoneQuality = "Flawless";
                            break;
                    }
                    break;
                case "Tuls Domain":
                    breachLoard = "Tuls";
                    switch (i_area_level)
                    {
                        // Normal
                        case 70:
                            breachStoneQuality = "";
                            break;
                        // Charged
                        case 74:
                            breachStoneQuality = "Charged";
                            break;
                        // Enriched
                        case 79:
                            breachStoneQuality = "Enriched";
                            break;
                        // Pure
                        case 81:
                            breachStoneQuality = "Pure";
                            break;
                        // Flawless
                        case 84:
                            breachStoneQuality = "Flawless";
                            break;
                    }
                    break;
            }
            
            if(string.IsNullOrEmpty(breachStoneQuality))
            {
                return string.Format("{0} {1}", breachLoard, "Breachstone");
            }
            else
            {
                return string.Format("{0} {1} {2}", breachLoard, breachStoneQuality, "Breachstone");
            }
        }

        public int GetImageIndex(TrX_TrackedActivity map)
        {
            int iIndex = 0;
            // Calculate Image Index
            if (map.Type == ACTIVITY_TYPES.MAP)
            {
                if (map.MapTier > 0 && map.MapTier <= 5)
                {
                    iIndex = 0;
                }
                else if (map.MapTier >= 6 && map.MapTier <= 10)
                {
                    iIndex = 1;
                }
                else if (map.MapTier >= 11)
                {
                    iIndex = 2;
                }
            }
            else if (map.Type == ACTIVITY_TYPES.TEMPLE)
            {
                iIndex = 3;
            }
            else if (map.Type == ACTIVITY_TYPES.HEIST)
            {
                iIndex = 4;
            }
            else if (map.Type == ACTIVITY_TYPES.ABYSSAL_DEPTHS)
            {
                iIndex = 5;
            }
            else if (map.Type == ACTIVITY_TYPES.LABYRINTH || map.Type == ACTIVITY_TYPES.LAB_TRIAL)
            {
                iIndex = 6;
            }
            else if (map.Type == ACTIVITY_TYPES.CAMPAIGN)
            {
                iIndex = 7;
            }
            else if (map.Type == ACTIVITY_TYPES.LOGBOOK || map.Type == ACTIVITY_TYPES.LOGBOOK_SIDE)
            {
                iIndex = 8;
            }
            else if (map.Type == ACTIVITY_TYPES.VAAL_SIDEAREA)
            {
                iIndex = 9;
            }
            else if (map.Type == ACTIVITY_TYPES.CATARINA_FIGHT)
            {
                iIndex = 10;
            }
            else if (map.Type == ACTIVITY_TYPES.SAFEHOUSE)
            {
                iIndex = 11;
            }
            else if (map.Type == ACTIVITY_TYPES.DELVE)
            {
                iIndex = 12;
            }
            else if (map.Type == ACTIVITY_TYPES.MAVEN_INVITATION)
            {
                iIndex = 13;
            }
            else if (map.Type == ACTIVITY_TYPES.SIRUS_FIGHT)
            {
                iIndex = 14;
            }
            else if (map.Type == ACTIVITY_TYPES.ATZIRI)
            {
                iIndex = 15;
            }
            else if (map.Type == ACTIVITY_TYPES.UBER_ATZIRI)
            {
                iIndex = 16;
            }
            else if (map.Type == ACTIVITY_TYPES.ELDER_FIGHT)
            {
                iIndex = 17;
            }
            else if (map.Type == ACTIVITY_TYPES.SHAPER_FIGHT)
            {
                iIndex = 18;
            }
            else if (map.Type == ACTIVITY_TYPES.SIMULACRUM)
            {
                iIndex = 19;
            }
            else if (map.Type == ACTIVITY_TYPES.MAVEN_FIGHT)
            {
                iIndex = 20;
            }
            else if(map.Type == ACTIVITY_TYPES.BREACHSTONE)
            {
                if(map.Area.Contains("Chayula"))
                {
                    switch (map.AreaLevel)
                    {
                        // Normal
                        case 80:
                            iIndex = 21;
                            break;
                        // Charged
                        case 81:
                            iIndex = 41;
                            break;
                        // Enriched
                        case 82:
                            iIndex = 40;
                            break;
                        // Pure
                        case 83:
                            iIndex = 39;
                            break;
                        // Flawless
                        case 84:
                            iIndex = 38;
                            break;
                    }
                }
                else if(map.Area.Contains("Esh"))
                {
                    switch (map.AreaLevel)
                    {
                        // Normal
                        case 70:
                            iIndex = 22;
                            break;
                        // Charged
                        case 74:
                            iIndex = 45;
                            break;
                        // Enriched
                        case 79:
                            iIndex = 44;
                            break;
                        // Pure
                        case 81:
                            iIndex = 43;
                            break;
                        // Flawless
                        case 84:
                            iIndex = 42;
                            break;
                    }
                }
                else if(map.Area.Contains("Xoph"))
                {
                    switch (map.AreaLevel)
                    {
                        // Normal
                        case 70:
                            iIndex = 23;
                            break;
                        // Charged
                        case 74:
                            iIndex = 37;
                            break;
                        // Enriched
                        case 79:
                            iIndex = 36;
                            break;
                        // Pure
                        case 81:
                            iIndex = 35;
                            break;
                        // Flawless
                        case 84:
                            iIndex =
                                34;
                            break;
                    }
                }
                else if(map.Area.Contains("Uul-Netol"))
                {
                    switch (map.AreaLevel)
                    {
                        // Normal
                        case 75:
                            iIndex = 24;
                            break;
                        // Charged
                        case 78:
                            iIndex = 33;
                            break;
                        // Enriched
                        case 81:
                            iIndex = 32;
                            break;
                        // Pure
                        case 82:
                            iIndex = 31;
                            break;
                        // Flawless
                        case 84:
                            iIndex = 30;
                            break;
                    }
                }
                else if(map.Area.Contains("Tul"))
                {
                    switch (map.AreaLevel)
                    {
                        // Normal
                        case 70:
                            iIndex = 25;
                            break;
                        // Charged
                        case 74:
                            iIndex = 29;
                            break;
                        // Enriched
                        case 79:
                            iIndex = 28;
                            break;
                        // Pure
                        case 81:
                            iIndex = 27;
                            break;
                        // Flawless
                        case 84:
                            iIndex = 26;
                            break;
                    }
                }
            }
            return iIndex;
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

                if (bZana)
                    sName += " (Zana)";
                lvi.SubItems.Add(GetStringFromActType(map.Type));
                lvi.SubItems.Add(map.Area);
                lvi.SubItems.Add(sTier);
                lvi.SubItems.Add(map.StopWatchValue.ToString());
                lvi.SubItems.Add(map.DeathCounter.ToString());

                // Calculate Image Index
                lvi.ImageIndex = GetImageIndex(map);

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
                stopwatchToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
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

                    if(!_autoStartsDOne)
                    {
                        if(_stopwatchOverlayShowDefault)
                        {
                            ActivateStopWatchOverlay();

                        }

                        _autoStartsDOne = true;
                    }

                    RenderTagsForTracking();
                    RenderTagsForConfig();
                    textBoxLogFilePath.Text = ReadSetting("poe_logfile_path");

                    labelItemCount.Text = "items: " + _actLogItemCount.ToString();

                    if (!_listViewInitielaized)
                    {
                        DoSearch();
                        _listViewInitielaized = true;
                    }

                    labelCurrArea.Text = _currentArea;
                    labelCurrentAreaLvl.Text = _currentAreaLevel > 0 ? _currentAreaLevel.ToString() : "-";
                    labelLastDeath.Text = _lastDeathTime.Year > 2000 ? _lastDeathTime.ToString() : "-";

                    if (_currentArea.Contains("Hideout") && !(_currentArea.Contains("Syndicate")))
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

                        if(_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.BREACHSTONE)
                        {
                            labelStopWatch.Text = _currentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = GetBreachStoneName(_currentActivity.Area, _currentActivity.AreaLevel);
                            labelTrackingDied.Text = _currentActivity.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity)];
                        }
                        else if ((_isMapZana && _currentActivity.SideArea_ZanaMap != null))
                        {
                            labelStopWatch.Text = _currentActivity.SideArea_ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.SideArea_ZanaMap.Area + " (" + sTier + ", Zana)";
                            labelTrackingDied.Text = _currentActivity.SideArea_ZanaMap.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.Type).ToUpper();
                            pictureBoxStop.Hide();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity.SideArea_ZanaMap)];
                        }
                        else if((_isMapVaalArea && _currentActivity.SideArea_VaalArea != null))
                        {
                            labelStopWatch.Text = _currentActivity.SideArea_VaalArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.SideArea_VaalArea.Area;
                            labelTrackingDied.Text = _currentActivity.SideArea_VaalArea.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.SideArea_VaalArea.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity.SideArea_VaalArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_isMapAbyssArea && _currentActivity.SideArea_AbyssArea != null))
                        {
                            labelStopWatch.Text = _currentActivity.SideArea_AbyssArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.SideArea_AbyssArea.Area;
                            labelTrackingDied.Text = _currentActivity.SideArea_AbyssArea.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.SideArea_AbyssArea.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity.SideArea_AbyssArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_isMapLabTrial && _currentActivity.SideArea_LabTrial != null))
                        {
                            labelStopWatch.Text = _currentActivity.SideArea_LabTrial.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.SideArea_LabTrial.Area;
                            labelTrackingDied.Text = _currentActivity.SideArea_LabTrial.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.SideArea_LabTrial.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity.SideArea_LabTrial)];
                            pictureBoxStop.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = _currentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _currentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = GetStringFromActType(_currentActivity.Type).ToUpper();
                            labelTrackingDied.Text = _currentActivity.DeathCounter.ToString();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_currentActivity)];
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

                    // MAP Dashbaord
                    if (_uiFlagMapDashboard)
                    {
                        SetTimeRangeFilter();
                        RenderMappingDashboard();
                        _uiFlagMapDashboard = false;
                    }

                    // LAB Dashbaord
                    if (_uiFlagLabDashboard)
                    {
                        SetTimeRangeFilter();
                        RenderLabDashboard();
                        _uiFlagLabDashboard = false;
                    }

                    // HEIST Dashbaord
                    if (_uiFlagHeistDashboard)
                    {
                        SetTimeRangeFilter();
                        RenderHeistDashboard();
                        _uiFlagHeistDashboard = false;
                    }

                    // AllStats Dashbaord
                    if (_uiFlagAllStatsDashboard)
                    {
                        SetTimeRangeFilter();
                        RenderAllStatsDashboard();
                        _uiFlagAllStatsDashboard = false;
                    }

                    //Bossing
                    if(_uiFlagBossDashboard)
                    {
                        SetTimeRangeFilter();
                        RenderBossingDashboard();
                        _uiFlagBossDashboard = true;
                    }

                    // Global Dashbaord
                    if (_uiFlagGlobalDashboard)
                    {
                        SetTimeRangeFilter();
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

            _stopwatchOverlay.UpdateStopWatch(labelStopWatch.Text,
                           _prevActivityOverlay != null ? _prevActivityOverlay.StopWatchValue : "00:00:00",
                           _currentActivity != null ? GetImageIndex(_currentActivity) : 0,
                           _prevActivityOverlay != null ? GetImageIndex(_prevActivityOverlay) : 0);
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
            _stopwatchOverlayOpacity = Convert.ToInt32(ReadSetting("overlay.stopwatch.opacity", "100"));
            _stopwatchOverlayShowDefault = Convert.ToBoolean(ReadSetting("overlay.stopwatch.default", "false"));

            comboBoxTheme.SelectedItem = ReadSetting("theme", "Dark") == "Dark" ? "Dark" : "Light";
            textBoxMapCap.Text = _timeCapMap.ToString();
            textBoxLabCap.Text = _timeCapLab.ToString();
            textBoxHeistCap.Text = _timeCapHeist.ToString();
            listViewActLog.GridLines = _showGridInActLog;
            trackBar1.Value = _stopwatchOverlayOpacity;
            checkBox2.Checked = _stopwatchOverlayShowDefault;
            label38.Text = _stopwatchOverlayOpacity.ToString() + "%";
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
                { ACTIVITY_TYPES.VAAL_SIDEAREA, 0 },
                { ACTIVITY_TYPES.ABYSSAL_DEPTHS, 0 },
                { ACTIVITY_TYPES.LAB_TRIAL, 0 },
                { ACTIVITY_TYPES.LOGBOOK, 0 },
                { ACTIVITY_TYPES.LOGBOOK_SIDE, 0 },
                { ACTIVITY_TYPES.CATARINA_FIGHT, 0 },
                { ACTIVITY_TYPES.SAFEHOUSE, 0 },
                { ACTIVITY_TYPES.BREACHSTONE, 0 },
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
            };
            double hideOutTime = 0;
            double totalCount = 0;
            if (Convert.ToBoolean(ReadSetting("pie_chart_show_hideout", "true")))
            {
                long ts1 = ((DateTimeOffset)_statsDate1).ToUnixTimeSeconds();
                long ts2 = ((DateTimeOffset)_statsDate2).ToUnixTimeSeconds();
                hideOutTime = _myStats.GetIncrementValue("HideoutTimeSec", ts1, ts2);
                totalCount += hideOutTime;
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
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
                double percentValHO =  hideOutTime / totalCount * 100;
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
            foreach (string s in _defaultMappings.HEIST_AREAS)
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

            foreach (KeyValuePair<string,int> kvp in countByArea)
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
            foreach (TrX_ActivityTag tg in Tags)
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

        private int LevelToTier(int level)
        {
            switch(level)
            {
                case 68:
                    return 1;
                case 69:
                    return 2;
                case 70:
                    return 3;
                case 71:
                    return 4;
                case 72:
                    return 5;
                case 73:
                    return 6;
                case 74:
                    return 7;
                case 75:
                    return 8;
                case 76:
                    return 9;
                case 77:
                    return 10;
                case 78:
                    return 11;
                case 79:
                    return 12;
                case 80:
                    return 13;
                case 81:
                    return 14;
                case 82:
                    return 15;
                case 83:
                    return 16;
                default:
                    return 0;
            }
        }

        private string TierToName(int tier)
        {
            if (tier == 0)
            {
                return "Unknown";
            }
            else
            {
                return "T" + tier;
            }
        }

        private void RenderBossingDashboard()
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59);
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
                tmKilled = 0;

            baranTried = _myStats.GetIncrementValue("BaranStarted", ts1, ts2);
            baranKilled = _myStats.GetIncrementValue("BaranKilled", ts1, ts2);
            veriTried = _myStats.GetIncrementValue("VeritaniaStarted", ts1, ts2);
            veriKilled = _myStats.GetIncrementValue("VeritaniaKilled", ts1, ts2);
            droxTried = _myStats.GetIncrementValue("DroxStarted", ts1, ts2);
            droxKilled = _myStats.GetIncrementValue("DroxKilled", ts1, ts2);
            hunterTried = _myStats.GetIncrementValue("HunterStarted", ts1, ts2);
            hunterKilled = _myStats.GetIncrementValue("HunterKilled", ts1, ts2);
            elderTried = _myStats.GetIncrementValue("ElderTried", ts1, ts2);
            elderKilled = _myStats.GetIncrementValue("ElderKilled", ts1, ts2);
            shaperTried = _myStats.GetIncrementValue("ShaperTried", ts1, ts2);
            shaperKilled = _myStats.GetIncrementValue("ShaperKilled", ts1, ts2);
            cataTried = _myStats.GetIncrementValue("CatarinaTried", ts1, ts2);
            cataKilled = _myStats.GetIncrementValue("CatarinaKilled", ts1, ts2);
            sirusTried = _myStats.GetIncrementValue("SirusStarted", ts1, ts2);
            sirusKilled = _myStats.GetIncrementValue("SirusKilled", ts1, ts2);
            mavenTried = _myStats.GetIncrementValue("MavenStarted", ts1, ts2);
            mavenKilled = _myStats.GetIncrementValue("MavenKilled", ts1, ts2);
            tmTried = _myStats.GetIncrementValue("TrialMasterStarted", ts1, ts2);
            tmKilled = _myStats.GetIncrementValue("TrialMasterKilled", ts1, ts2);



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
            };
            BeginInvoke(mi);


        }

        private void RenderAllStatsDashboard()
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59);


            MethodInvoker mi = delegate
            {
                if(listViewNF1.Items.Count == 0)
                {
                    for(int i = 0; i < _myStats.NumericStats.Keys.Count; i++)
                    {
                        string sStatKey = _myStats.NumericStats.Keys.ElementAt(i);
                        string sStatLong = GetStatLongName(sStatKey);

                        
                        ListViewItem lvi = new ListViewItem(sStatLong);
                        lvi.Name = "allstats_" + sStatKey;
                        lvi.SubItems.Add(_myStats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds()).ToString());
                        _lvmAllStats.AddLvItem(lvi, lvi.Name, true);
                    }
                }
                else
                {
                    for (int i = 0; i < _myStats.NumericStats.Keys.Count; i++)
                    {
                        string sStatKey = _myStats.NumericStats.Keys.ElementAt(i);
                        string sStatLong = GetStatLongName(sStatKey);

                        ListViewItem lvi = _lvmAllStats.GetLvItem("allstats_" + sStatKey);
                        _lvmAllStats.GetLvItem("allstats_" + sStatKey).SubItems[1].Text = _myStats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds()).ToString();
                    }
                }
            };

            

            this.Invoke(mi);
        }

        private void UpdateAllStatsChart()
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59);

            int interval = 1;
            double days = (dt2 - dt1).TotalDays;

            if(days >= 365)
            {
                interval = 14;
            }
            else if(days > 150)
            {
                interval = 7;
            }
            else if (days > 30)
            {
                interval = 2;
            }

            MethodInvoker mi = delegate
            {
                if (!string.IsNullOrEmpty(_allStatsSelected))
                {
                    chart1.ChartAreas[0].AxisX.Interval = interval;
                    chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
                    chart1.ChartAreas[0].AxisX.IntervalOffset = interval;

                    label100.Text = string.Format("{0} ({1} - {2})", GetStatLongName(_allStatsSelected), dt1, dt2);
                    chart1.Series[0].Points.Clear();
                    List<KeyValuePair<long, int>> results = _myStats.GetByDayValues(_allStatsSelected, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds(), 1);
                    foreach (KeyValuePair<long, int> kvp in results)
                    {
                        chart1.Series[0].Points.AddXY(DateTimeOffset.FromUnixTimeSeconds(kvp.Key).DateTime, kvp.Value);
                    }
                }
            };
            BeginInvoke(mi);
          
        }

        public void RenderMappingDashboard()
        {
            List<KeyValuePair<string, int>> tmpList = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> top10 = new List<KeyValuePair<string, int>>();
            Dictionary<string, int> tmpListTags = new Dictionary<string, int>();
            List<KeyValuePair<string, int>> top10Tags = new List<KeyValuePair<string, int>>();

            Dictionary<string, int> countByArea = new Dictionary<string, int>();
            Dictionary<int, int> countByTier = new Dictionary<int, int>();

            // MAP AREAS
            foreach (string s in _defaultMappings.MAP_AREAS)
            {
                countByArea.Add(s, 0);
            }

            for(int i = 0; i <= 19; i++)
            {
                countByTier.Add(i, 0);
            }

            foreach(TrX_TrackedActivity act in _statsDataSource)
            {
                if(act.Type == ACTIVITY_TYPES.MAP)
                {
                    countByArea[act.Area]++;
                    countByTier[act.MapTier]++;
                }
            }

            foreach(KeyValuePair<string,int> kvp in countByArea)
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
            foreach (TrX_ActivityTag tg in Tags)
            {
                tmpListTags.Add(tg.ID, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
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

        public void RequestDashboardUpdates()
        {
            _uiFlagBossDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagLabDashboard = true;
            _uiFlagAllStatsDashboard = true;
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

        public void PauseCurrentActivityOrSide()
        {
            if (_currentActivity != null)
            {
                if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                {
                    if (!_currentActivity.SideArea_ZanaMap.ManuallyPaused)
                    {
                        _currentActivity.SideArea_ZanaMap.Pause();
                    }
                }
                else if (_isMapVaalArea && _currentActivity.SideArea_VaalArea != null)
                {
                    if (!_currentActivity.SideArea_VaalArea.ManuallyPaused)
                    {
                        _currentActivity.SideArea_VaalArea.Pause();
                    }
                }
                else if (_isMapAbyssArea && _currentActivity.SideArea_AbyssArea != null)
                {
                    if (!_currentActivity.SideArea_AbyssArea.ManuallyPaused)
                    {
                        _currentActivity.SideArea_AbyssArea.Pause();
                    }
                }
                else if (_isMapLabTrial && _currentActivity.SideArea_LabTrial != null)
                {
                    if (!_currentActivity.SideArea_LabTrial.ManuallyPaused)
                    {
                        _currentActivity.SideArea_LabTrial.Pause();
                    }
                }
                else if (_isMapLogbookSide && _currentActivity.SideArea_LogbookSide != null)
                {
                    if (!_currentActivity.SideArea_LogbookSide.ManuallyPaused)
                    {
                        _currentActivity.SideArea_LogbookSide.Pause();
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

        public void ResumeCurrentActivityOrSide()
        {
            if (_currentActivity != null)
            {
                if (_isMapZana && _currentActivity.SideArea_ZanaMap != null)
                {
                    if (_currentActivity.SideArea_ZanaMap.ManuallyPaused)
                    {
                        _currentActivity.SideArea_ZanaMap.Resume();
                    }
                }
                else if (_isMapVaalArea && _currentActivity.SideArea_VaalArea != null)
                {
                    if (_currentActivity.SideArea_VaalArea.ManuallyPaused)
                    {
                        _currentActivity.SideArea_VaalArea.Resume();
                    }
                }
                else if (_isMapAbyssArea && _currentActivity.SideArea_AbyssArea != null)
                {
                    if (_currentActivity.SideArea_AbyssArea.ManuallyPaused)
                    {
                        _currentActivity.SideArea_AbyssArea.Resume();
                    }
                }
                else if (_isMapLabTrial && _currentActivity.SideArea_LabTrial != null)
                {
                    if (_currentActivity.SideArea_LabTrial.ManuallyPaused)
                    {
                        _currentActivity.SideArea_LabTrial.Resume();
                    }
                }
                else if (_isMapLogbookSide && _currentActivity.SideArea_LogbookSide != null)
                {
                    if (_currentActivity.SideArea_LogbookSide.ManuallyPaused)
                    {
                        _currentActivity.SideArea_LogbookSide.Resume();
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
            if (listViewActLog.SelectedItems.Count == 1)
            {
                int iIndex = FindEventLogIndexByID(listViewActLog.SelectedItems[0].Name);
                long lTimestamp = _eventHistory[iIndex].TimeStamp;
                string sType = listViewActLog.SelectedItems[0].SubItems[1].Text;
                string sArea = listViewActLog.SelectedItems[0].SubItems[2].Text;

                if (MessageBox.Show("Do you really want to delete this Activity? " + Environment.NewLine
                    + Environment.NewLine
                    + "Type: " + sType + Environment.NewLine
                    + "Area: " + sArea + Environment.NewLine
                    + "Time: " + listViewActLog.SelectedItems[0].SubItems[0].Text, "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _lvmActlog.RemoveItemByName(listViewActLog.SelectedItems[0].Name);
                    _eventHistory.RemoveAt(iIndex);
                    DeleteActLogEntry(lTimestamp);
                }

                RequestDashboardUpdates();
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenChildWindow(new SearchHelp());
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox8.Text = "";
            DoSearch();
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
                    RenderBossingDashboard();
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem.ToString() == "Custom")
            {
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
            }
            else
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
                SetTimeRangeFilter(true);
            }
        }


        private void button5_Click_1(object sender, EventArgs e)
        {
            SetTimeRangeFilter(true);
        }

        private void listViewNF1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_lvmAllStats.listView.SelectedItems.Count > 0)
            {
                _allStatsSelected = _lvmAllStats.listView.SelectedItems[0].Name.Replace("allstats_", "");
                UpdateAllStatsChart();
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            _lvmAllStats.ApplyFullTextFilter(textBox1.Text);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Clear();
            _lvmAllStats.Reset();
        }

        private void checkBoxLabHideUnknown_CheckedChanged(object sender, EventArgs e)
        {
            _labDashboardHideUnknown = ((CheckBox)sender).Checked;
            AddUpdateAppSettings("dashboard_lab_hide_unknown", _labDashboardHideUnknown.ToString());
            RenderLabDashboard();
        }

        private void ActivateStopWatchOverlay()
        {
            _stopwatchOverlay.Show();
            _stopwatchOverlay.TopMost = true;
            _stopwatchOverlay.Opacity = _stopwatchOverlayOpacity / 100.0;
            _stopwatchOverlay.Location = new Point(Convert.ToInt32(ReadSetting("overlay.stopwatch.x", "0")), (Convert.ToInt32(ReadSetting("overlay.stopwatch.y", "0"))));
        }

        private void stopwatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_stopwatchOverlay.Visible)
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

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                _prevActivityOverlay = _currentActivity;
                FinishActivity(_currentActivity, null, _currentActivity.Type, DateTime.Now);
            }

        }

        private void pictureBox15_Click_1(object sender, EventArgs e)
        {
            if (_currentActivity != null)
            {
                _prevActivityOverlay = _currentActivity;
                FinishActivity(_currentActivity, null, _currentActivity.Type, DateTime.Now);
            }
        }

    }
}