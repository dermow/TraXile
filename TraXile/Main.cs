using log4net;
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
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Globalization;

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
        // Core logic
        private TrX_CoreLogic _logic;

        // START FLAGS
        public readonly bool IS_IN_DEBUG_MODE = false;
        public bool SAFE_RELOAD_MODE;

        // Calendar
        public DateTimeFormatInfo _dtfi;

        // App parameters
        private readonly string _dbPath;
        private readonly string _cachePath;
        private readonly string _myAppData;
        private bool _listViewInitielaized;
        private bool _showGridInActLog;
        private bool _restoreMode;
        private bool _uiFlagLabDashboard;
        private bool _showGridInStats;
        private bool _UpdateCheckDone;
        private bool _restoreOk = true;
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
        private bool _historyInitialized;
        private List<string> _knownPlayerNames;
        private List<string> labs;
        private TrX_DefaultMappings _defaultMappings;
        private List<TrX_TrackedActivity> _statsDataSource;
        private ConcurrentQueue<TrX_TrackingEvent> _eventQueue;
        private TrX_TrackedActivity _prevActivityOverlay;
        private DateTime _inAreaSince;
        private DateTime _statsDate1;
        private DateTime _statsDate2;
       
        // Other variables
        private LoadScreen _loadScreenWindow;
        private StopWatchOverlay _stopwatchOverlay;
        private BindingList<string> _backups;
        private Dictionary<string, Label> _tagLabels, _tagLabelsConfig;
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
            set { AddUpdateAppSettings("poe_logfile_path", value); _logic.ClientTxtPath = value; }
        }

        public TrX_CoreLogic Logic
        {
            get { return _logic; }
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

            DateTime date1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 0, 0, 0, _dtfi.Calendar);
            DateTime date2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, 23, 59, 59, _dtfi.Calendar);

            _statsDate1 = date1;
            _statsDate2 = date2;

            foreach (TrX_TrackedActivity act in _logic.ActivityHistory)
            {
                if (act.Started >= date1 && act.Started <= date2)
                {
                    results.Add(act);
                }
            }

            return results;
        }

        private TrX_LeagueInfo GetLeagueByName(string name)
        {
            foreach (TrX_LeagueInfo li in _leagues)
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
                _statsDataSource.AddRange(_logic.ActivityHistory);
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

                if (comboBox1.SelectedItem.ToString() != "Custom")
                {
                    dateTimePicker1.Value = date1;
                    dateTimePicker2.Value = date2;
                }

            }

            if (render)
            {
                RequestDashboardUpdates();
            }

        }

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

            // Fixing the DateTimeFormatInfo to Gregorian Calendar, to avoid wrong timestamps with other calendars
            _dtfi = DateTimeFormatInfo.GetInstance(new CultureInfo("en-CA"));
            _dtfi.Calendar = new GregorianCalendar();
            _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _log.Info("Application started");
            _mySettings.LoadFromXml();
            
            _logic = new TrX_CoreLogic(SettingPoeLogFilePath);
            _logic.OnHistoryInitialized += _logic_OnHistoryInitialized;
            _logic.OnActivityFinished += _logic_OnActivityFinished;

            _lvmActlog = new TrX_ListViewManager(listViewActLog);
            _lvmAllStats = new TrX_ListViewManager(listViewNF1);
            comboBox1.SelectedIndex = 0;

            _defaultMappings = new TrX_DefaultMappings();
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


            _eventQueue = new ConcurrentQueue<TrX_TrackingEvent>();
            _knownPlayerNames = new List<string>();
            _backups = new BindingList<string>();
            _inAreaSince = DateTime.Now;
            _tagLabels = new Dictionary<string, Label>();
            _tagLabelsConfig = new Dictionary<string, Label>();
            _statsDataSource = new List<TrX_TrackedActivity>();
            _statsDate1 = DateTime.Now.AddYears(-100);
            _statsDate2 = DateTime.Now.AddDays(1);



            Text = APPINFO.NAME;

            if (IS_IN_DEBUG_MODE)
            {
                Text += " ---> !!!!! DEBUG MODE !!!!!";
            }


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
            InitLeagueInfo();

            _eventQueue.Enqueue(new TrX_TrackingEvent(EVENT_TYPES.APP_STARTED) { EventTime = DateTime.Now, LogLine = "Application started." });

            ResetMapHistory();
            LoadLayout();

            // Request initial Dashboard update
            _uiFlagLabDashboard = true;
            _uiFlagMapDashboard = true;
            _uiFlagBossDashboard = true;
            _uiFlagHeistDashboard = true;
            _uiFlagGlobalDashboard = true;
            _uiFlagActivityList = true;
            _uiFlagAllStatsDashboard = true;
        }

        private void _logic_OnActivityFinished(TrX_CoreLogicActivityEventArgs e)
        {
            if(e.Logic.EventQueueInitialized)
            {
                AddMapLvItem(e.Activity);
                RequestDashboardUpdates();
                _uiFlagActivityList = true;
            }
        }

        private void _logic_OnHistoryInitialized(TrX_CoreLogicGenericEventArgs e)
        {
            MethodInvoker mi = delegate
            {
                ResetMapHistory();
            };
            BeginInvoke(mi);
            
        }

        private void InitLeagueInfo()
        {
            _leagues.Clear();
            _leagues.Add(new TrX_LeagueInfo("Harbinger", 3, 0, new DateTime(2017, 8, 4, 20, 0, 0), new DateTime(2017, 12, 4, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Abyss", 3, 1, new DateTime(2017, 12, 8, 20, 0, 0), new DateTime(2018, 2, 26, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Bestiary", 3, 2, new DateTime(2018, 3, 2, 20, 0, 0), new DateTime(2018, 5, 28, 22, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Incursion", 3, 3, new DateTime(2018, 6, 1, 20, 0, 0), new DateTime(2018, 8, 27, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Delve", 3, 4, new DateTime(2018, 8, 31, 20, 0, 0), new DateTime(2018, 12, 3, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Betrayal", 3, 5, new DateTime(2018, 12, 7, 20, 0, 0), new DateTime(2019, 3, 4, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Synthesis", 3, 6, new DateTime(2019, 3, 8, 20, 0, 0), new DateTime(2019, 6, 3, 22, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Legion", 3, 7, new DateTime(2019, 6, 7, 20, 0, 0), new DateTime(2019, 9, 3, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Blight", 3, 8, new DateTime(2019, 9, 6, 20, 0, 0), new DateTime(2019, 12, 9, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Metamorph", 3, 9, new DateTime(2019, 12, 13, 20, 0, 0), new DateTime(2020, 3, 9, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Delirium", 3, 10, new DateTime(2020, 3, 13, 20, 0, 0), new DateTime(2020, 6, 15, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Harvest", 3, 11, new DateTime(2020, 6, 19, 20, 0, 0), new DateTime(2020, 9, 14, 22, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Heist", 3, 12, new DateTime(2020, 9, 18, 20, 0, 0), new DateTime(2021, 1, 11, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Ritual", 3, 13, new DateTime(2021, 1, 15, 20, 0, 0), new DateTime(2021, 1, 15, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Ultimatum", 3, 14, new DateTime(2021, 4, 16, 20, 0, 0), new DateTime(2021, 07, 19, 22, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Expedition", 3, 15, new DateTime(2021, 7, 23, 20, 0, 0), new DateTime(2021, 10, 19, 21, 0, 0, _dtfi.Calendar)));
            _leagues.Add(new TrX_LeagueInfo("Scourge", 3, 16, new DateTime(2021, 10, 22, 20, 0, 0), new DateTime(2022, 01, 31, 21, 0, 0, _dtfi.Calendar)));

            List<TrX_LeagueInfo> litmp = new List<TrX_LeagueInfo>();
            litmp.AddRange(_leagues);
            litmp.Reverse();

            foreach (TrX_LeagueInfo li in litmp)
            {
                comboBox1.Items.Add(string.Format("League: {0} ({1})", li.Name, li.Version));
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
        /// Clear the activity log
        /// </summary>
        private void ClearActivityLog()
        {
            _logic.ActivityHistory.Clear();
            listViewActLog.Items.Clear();
            _logic.Database.DoNonQuery("drop table tx_activity_log");
            _logic.Database.DoNonQuery("create table if not exists tx_activity_log " +
                 "(timestamp int, " +
                 "act_type text, " +
                 "act_area text, " +
                 "act_stopwatch int, " +
                 "act_deathcounter int," +
                 "act_ulti_rounds int," +
                 "act_is_zana int," +
                 "act_logic.Tags" + ")");
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
                _logic.Database.DoNonQuery("insert into tx_known_players (player_name) VALUES ('" + s_name + "')");
                _log.Info("KnownPlayerAdded -> name: " + s_name);
            }
        }

        /// <summary>
        /// Delete entry from activity log
        /// </summary>
        /// <param name="l_timestamp"></param>
        private void DeleteActLogEntry(long l_timestamp)
        {
            _logic.Database.DoNonQuery("delete from tx_activity_log where timestamp = " + l_timestamp.ToString());
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
            BeginInvoke((MethodInvoker)delegate
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
        }

        /// <summary>
        /// Add maximum numberof ListViewItems to Listview
        /// TODO: filter before adding!!!
        /// </summary>
        private void AddActivityLvItems()
        {
            foreach (TrX_TrackedActivity act in _logic.ActivityHistory)
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

            if (string.IsNullOrEmpty(breachStoneQuality))
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
            else if (map.Type == ACTIVITY_TYPES.BREACHSTONE)
            {
                if (map.Area.Contains("Chayula"))
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
                else if (map.Area.Contains("Esh"))
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
                else if (map.Area.Contains("Xoph"))
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
                else if (map.Area.Contains("Uul-Netol"))
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
                else if (map.Area.Contains("Tul"))
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
            BeginInvoke((MethodInvoker)delegate
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

            });
        }

        /// <summary>
        /// Find matching activity to Item name
        /// </summary>
        /// <param name="s_name"></param>
        /// <returns></returns>
        private TrX_TrackedActivity GetActivityFromListItemName(string s_name)
        {
            foreach (TrX_TrackedActivity ta in _logic.ActivityHistory)
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

            if (_logic.EventQueueInitialized)
            {
                stopwatchToolStripMenuItem.Checked = _stopwatchOverlay.Visible;
                stopwatchToolStripMenuItem1.Checked = _stopwatchOverlay.Visible;
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
                                Environment.NewLine + 
                                Environment.NewLine);

                        }
                    }

                    if (!_autoStartsDOne)
                    {
                        if (_stopwatchOverlayShowDefault)
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
                        _oldestTimeStamp = _logic.Stats.GetOldestTimeStamp();
                        _listViewInitielaized = true;
                    }

                    label80.Text = string.Format("{0} (lvl. {1})",_logic.CurrentArea, _logic.CurrentAreaLevel);

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
                            labelTrackingArea.Text = GetBreachStoneName(_logic.CurrentActivity.Area, _logic.CurrentActivity.AreaLevel);
                            labelTrackingDied.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity)];
                        }
                        else if ((_logic.IsMapZana && _logic.CurrentActivity.SideArea_ZanaMap != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_ZanaMap.Area + " (" + sTier + ", Zana)";
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_ZanaMap.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.Type).ToUpper();
                            pictureBoxStop.Hide();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_ZanaMap)];
                        }
                        else if ((_logic.IsMapVaalArea && _logic.CurrentActivity.SideArea_VaalArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_VaalArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_VaalArea.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_VaalArea.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.SideArea_VaalArea.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_VaalArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapAbyssArea && _logic.CurrentActivity.SideArea_AbyssArea != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_AbyssArea.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_AbyssArea.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_AbyssArea.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.SideArea_AbyssArea.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_AbyssArea)];
                            pictureBoxStop.Hide();
                        }
                        else if ((_logic.IsMapLabTrial && _logic.CurrentActivity.SideArea_LabTrial != null))
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.SideArea_LabTrial.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.SideArea_LabTrial.Area;
                            labelTrackingDied.Text = _logic.CurrentActivity.SideArea_LabTrial.DeathCounter.ToString();
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.SideArea_LabTrial.Type).ToUpper();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity.SideArea_LabTrial)];
                            pictureBoxStop.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = _logic.CurrentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = _logic.CurrentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = GetStringFromActType(_logic.CurrentActivity.Type).ToUpper();
                            labelTrackingDied.Text = _logic.CurrentActivity.DeathCounter.ToString();
                            pictureBox10.Image = imageList2.Images[GetImageIndex(_logic.CurrentActivity)];
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

                    if (_uiFlagAllStatsDashboard ||
                    _uiFlagBossDashboard ||
                    _uiFlagGlobalDashboard ||
                    _uiFlagHeistDashboard ||
                    _uiFlagLabDashboard ||
                    _uiFlagMapDashboard)
                    {
                        SetTimeRangeFilter();
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

                    //Bossing
                    if (_uiFlagBossDashboard)
                    {
                        RenderBossingDashboard();
                        _uiFlagBossDashboard = false;
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
                    if (_uiFlagActivityList)
                    {
                        DoSearch();
                        _uiFlagActivityList = false;
                    }

                    listView1.Columns[2].Width = listView1.Width;

                });
            }

            _stopwatchOverlay.UpdateStopWatch(labelStopWatch.Text,
                           _logic.OverlayPrevActivity != null ? _logic.OverlayPrevActivity.StopWatchValue : "00:00:00",
                           _logic.CurrentActivity != null ? GetImageIndex(_logic.CurrentActivity) : 0,
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
            checkBox3.Checked = Convert.ToBoolean(ReadSetting("statistics_auto_refresh", "false"));
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
            if (_logic.CurrentActivity != null)
                _logic.FinishActivity(_logic.CurrentActivity, null, _logic.CurrentActivity.Type, DateTime.Now);
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
                hideOutTime = _logic.Stats.GetIncrementValue("HideoutTimeSec", ts1, ts2);
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
            BeginInvoke(mi);
        }

        private int LevelToTier(int level)
        {
            switch (level)
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
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dtfi.Calendar);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dtfi.Calendar);
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
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dtfi.Calendar);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dtfi.Calendar);

            if (listViewNF1.Items.Count == 0)
            {
                for (int i = 0; i < _logic.Stats.NumericStats.Keys.Count; i++)
                {
                    string sStatKey = _logic.Stats.NumericStats.Keys.ElementAt(i);
                    string sStatLong = GetStatLongName(sStatKey);

                    ListViewItem lvi = new ListViewItem(sStatLong);
                    lvi.Name = "allstats_" + sStatKey;
                    lvi.SubItems.Add(_logic.Stats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds()).ToString());
                    _lvmAllStats.AddLvItem(lvi, lvi.Name, true);
                }
            }
            else
            {
                for (int i = 0; i < _logic.Stats.NumericStats.Keys.Count; i++)
                {
                    string sStatKey = _logic.Stats.NumericStats.Keys.ElementAt(i);

                    // TODO: Optimize performance
                    _lvmAllStats.GetLvItem("allstats_" + sStatKey).SubItems[1].Text = _logic.Stats.GetIncrementValue(sStatKey, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds()).ToString();
                }
            }

        }

        private void UpdateAllStatsChart()
        {
            DateTime dt1 = new DateTime(_statsDate1.Year, _statsDate1.Month, _statsDate1.Day, 0, 0, 0, _dtfi.Calendar);
            DateTime dt2 = new DateTime(_statsDate2.Year, _statsDate2.Month, _statsDate2.Day, 23, 59, 59, _dtfi.Calendar);

            int interval = 1;
            double days = (dt2 - dt1).TotalDays;

            if (days >= 365)
            {
                interval = 14;
            }
            else if (days > 150)
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
                    List<KeyValuePair<long, int>> results = _logic.Stats.GetByDayValues(_allStatsSelected, ((DateTimeOffset)dt1).ToUnixTimeSeconds(), ((DateTimeOffset)dt2).ToUnixTimeSeconds(), 1);
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

            for (int i = 0; i <= 19; i++)
            {
                countByTier.Add(i, 0);
            }

            foreach (TrX_TrackedActivity act in _statsDataSource)
            {
                if (act.Type == ACTIVITY_TYPES.MAP)
                {
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
            string sBackupDir = _myAppData + @"/backups/" + s_name + @"/" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            System.IO.Directory.CreateDirectory(sBackupDir);

            if (System.IO.File.Exists(SettingPoeLogFilePath))
                System.IO.File.Copy(SettingPoeLogFilePath, sBackupDir + @"/Client.txt");
            if (System.IO.File.Exists(_cachePath))
                System.IO.File.Copy(_cachePath, sBackupDir + @"/stats.cache");
            if (System.IO.File.Exists(_dbPath))
                System.IO.File.Copy(_dbPath, sBackupDir + @"/data.db");
            if (System.IO.File.Exists(_myAppData + @"\config.xml"))
                System.IO.File.Copy(_myAppData + @"\config.xml", sBackupDir + @"/config.xml");
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

            for (int i = 0; i < _logic.ActivityHistory.Count; i++)
            {
                tm = _logic.ActivityHistory[i];
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
        }

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
            _uiFlagMapDashboard = true;
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

        // =========> EVENT HANDLERS FOR GUI COMPONENTS
        // =======================================================

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxLogView.Text = "";
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
            if (listViewActLog.SelectedItems.Count == 1)
            {
                int iIndex = FindEventLogIndexByID(listViewActLog.SelectedItems[0].Name);
                long lTimestamp = _logic.ActivityHistory[iIndex].TimeStamp;
                string sType = listViewActLog.SelectedItems[0].SubItems[1].Text;
                string sArea = listViewActLog.SelectedItems[0].SubItems[2].Text;

                if (MessageBox.Show("Do you really want to delete this Activity? " + Environment.NewLine
                    + Environment.NewLine
                    + "Type: " + sType + Environment.NewLine
                    + "Area: " + sArea + Environment.NewLine
                    + "Time: " + listViewActLog.SelectedItems[0].SubItems[0].Text, "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _lvmActlog.RemoveItemByName(listViewActLog.SelectedItems[0].Name);
                    _logic.ActivityHistory.RemoveAt(iIndex);
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
            if (_logic.EventQueueInitialized)
                RenderTagsForTracking(true);
        }

        private void panelEditTags_SizeChanged(object sender, EventArgs e)
        {
            if (_logic.EventQueueInitialized)
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
                    RequestDashboardUpdates();
                }
            }
        }


        private void button5_Click_1(object sender, EventArgs e)
        {
            RequestDashboardUpdates();

        }

        private void listViewNF1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lvmAllStats.listView.SelectedItems.Count > 0)
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
            try
            {
                _stopwatchOverlay.Show();
            }
            catch(ObjectDisposedException e)
            {
                _stopwatchOverlay = new StopWatchOverlay(this, imageList2);
            }
            
            _stopwatchOverlay.TopMost = true;
            _stopwatchOverlay.Opacity = _stopwatchOverlayOpacity / 100.0;
            _stopwatchOverlay.Location = new Point(Convert.ToInt32(ReadSetting("overlay.stopwatch.x", "0")), (Convert.ToInt32(ReadSetting("overlay.stopwatch.y", "0"))));
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
            if(_logic.CurrentActivity != null)
            {
                _logic.FinishActivity(_logic.CurrentActivity, null, ACTIVITY_TYPES.MAP, new DateTime());
            }
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            RequestDashboardUpdates();
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {
            AddUpdateAppSettings("statistics_auto_refresh", checkBox3.Checked.ToString());
        }
    }
}