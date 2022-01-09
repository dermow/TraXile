using log4net;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TraXile
{
    /// <summary>
    /// History initizalized event handler
    /// </summary>
    public delegate void Trx_GenericEventHandler(TrX_CoreLogicGenericEventArgs e);

    /// <summary>
    /// Event handler for activity based events
    /// </summary>
    /// <param name="e"></param>
    public delegate void TrX_ActivityEventHandler(TrX_CoreLogicActivityEventArgs e);

    /// <summary>
    /// Event args for generic events
    /// </summary>
    public class TrX_CoreLogicGenericEventArgs : EventArgs
    {
        TrX_CoreLogic _logic;
        public TrX_CoreLogicGenericEventArgs(TrX_CoreLogic logic)
        {
            _logic = logic;
        }

        public TrX_CoreLogic Logic
        {
            get { return _logic; }
        }
    }

    /// <summary>
    /// Event args for Activity based events
    /// </summary>
    public class TrX_CoreLogicActivityEventArgs : EventArgs
    {
        TrX_CoreLogic _logic;
        TrX_TrackedActivity _activity;
        public TrX_CoreLogicActivityEventArgs(TrX_CoreLogic logic, TrX_TrackedActivity activity)
        {
            _logic = logic;
            _activity = activity;
        }

        public TrX_CoreLogic Logic
        {
            get { return _logic; }
        }

        public TrX_TrackedActivity Activity
        {
            get { return _activity; }
        }
    }

    /// <summary>
    /// Core parsing logic
    /// </summary>
    public class TrX_CoreLogic
    {
        // START FLAGS
        public readonly bool IS_IN_DEBUG_MODE = false;
        public bool SAFE_RELOAD_MODE;

        // Calendar
        public DateTimeFormatInfo _dateTimeFormatInfo;
        public GregorianCalendar _myCalendar;

        // App parameters
        private readonly string _dbPath;
        private readonly string _cachePath;
        private readonly string _myAppData;
        private bool _exit;
        private bool _StartedFlag = false;
        private bool _nextAreaIsExp = false;

        // Core Logic variables
        private string _currentArea;
        private string _currentInstanceEndpoint;
        private string _lastSimuEndpoint;
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
        private long _oldestTimeStamp;
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
        private DateTime _initStartTime;
        private DateTime _initEndTime;
        private string _lastShaperInstance;
        private string _lastElderInstance;

        // Hideout time
        private DateTime _hoStart;
        private bool _trackingHO;

        // Other variables
        private List<string> _parsedActivities;
        private readonly TrX_SettingsManager _mySettings;
        private ILog _log;

        private string _clientTxtPath;

        // Events
        public event Trx_GenericEventHandler OnHistoryInitialized;
        public event TrX_ActivityEventHandler OnActivityFinished;
        public event Trx_GenericEventHandler OnTagsUpdated;



        /// <summary>
        /// Main Window Constructor
        /// </summary>
        public TrX_CoreLogic(string logpath)
        {
            _clientTxtPath = logpath;

            if (File.Exists(Application.StartupPath + @"\DEBUG_MODE_ON.txt"))
            {
                IS_IN_DEBUG_MODE = true;
            }

            if (IS_IN_DEBUG_MODE)
            {
                _myAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + APPINFO.NAME + "_Debug";
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

            Init();
        }

        private int FindEventLogIndexByID(string id)
        {
            foreach (TrX_TrackedActivity act in _eventHistory)
            {
                if (act.UniqueID == id)
                {
                    return _eventHistory.IndexOf(act);
                }
            }
            return -1;
        }

        /// <summary>
        /// Do main initialization
        /// ONLY CALL ONCE! S
        /// </summary>
        private void Init()
        {
            // Fixing the DateTimeFormatInfo to Gregorian Calendar, to avoid wrong timestamps with other calendars
            _dateTimeFormatInfo = DateTimeFormatInfo.GetInstance(new CultureInfo("en-CA"));
            _dateTimeFormatInfo.Calendar = new GregorianCalendar();
            _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _log.Info("Application started");
            _eventMapping = new TrX_EventMapping();
            _defaultMappings = new TrX_DefaultMappings();
            _parsedActivities = new List<string>();
            _dict = new Dictionary<int, string>();
            _eventQueue = new ConcurrentQueue<TrX_TrackingEvent>();
            _eventHistory = new List<TrX_TrackedActivity>();
            _knownPlayerNames = new List<string>();
            _currentArea = "-";
            _eventQueueInitizalized = false;
            _lastSimuEndpoint = "";
            _tags = new List<TrX_ActivityTag>();
            _statsDataSource = new List<TrX_TrackedActivity>();
            _initStartTime = DateTime.Now;
            _myDB = new TrX_DBManager(_myAppData + @"\data.db", ref _log);
            _myStats = new TrX_StatsManager(_myDB);
            _lastEventTypeConq = EVENT_TYPES.APP_STARTED;

            InitDefaultTags();
            InitNumStats();
            ReadKnownPlayers();
            LoadCustomTags();
            SaveVersion();

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
                catch (Exception ex)
                {
                    _log.Fatal(string.Format("Unable to restore stats.cache from DB: {0}", ex.Message));
                    _log.Debug(ex.ToString());
                    _log.Info("Forcing full logfile reload for stats.");

                    // Drop statistic values and enable stat_reload mode
                    _myDB.DoNonQuery("DELETE FROM tx_stats WHERE timestamp > 0");
                    SAFE_RELOAD_MODE = true;
                }
            }

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
        }

        /// <summary>
        /// Stop logic
        /// </summary>
        /// <param name="timeout">timeout to wait for exit</param>
        public void Stop(int timeout = 2000)
        {
            _exit = true;
            int i = 0;

            // Wait for threads to finish
            while (_eventThread.IsAlive || _logParseThread.IsAlive)
            {
                Thread.Sleep(1);
                i++;
                if (i > timeout)
                {
                    break;
                }
            }

            if (_eventThread.IsAlive)
            {
                _eventThread.Abort();
            }
            if (_logParseThread.IsAlive)
            {
                _logParseThread.Abort();
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


        // Self-Healing for stats.cache
        private void RestoreStatsCacheFromDB()
        {
            SqliteDataReader dr1 = _myDB.GetSQLReader("select value from tx_kvstore where key='last_hash'");
            while (dr1.Read())
            {
                _lastHash = Convert.ToInt32(dr1.GetString(0));
            }

            List<string> stats = new List<string>();
            foreach (KeyValuePair<string, int> kvp in _myStats.NumericStats)
            {
                stats.Add(kvp.Key);
            }

            foreach (string s in stats)
            {
                SqliteDataReader dr2 = _myDB.GetSQLReader(string.Format("select stat_value from tx_stats where stat_name ='{0}' order by timestamp desc limit 1", s));
                while (dr2.Read())
                {
                    _myStats.NumericStats[s] = dr2.GetInt32(0);
                }
            }

            _log.Info("stats.cache successfully restored.");
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

            _log.Info("Stats cleared.");
        }

        /// <summary>
        /// Clear the activity log
        /// </summary>
        private void ClearActivityLog()
        {
            _eventHistory.Clear();
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
            _log.Info("Activity log cleared.");
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

        }

        /// <summary>
        /// Main method for log parsing thread
        /// </summary>
        private void LogParsing()
        {
            while (!_exit)
            {
                Thread.Sleep(1000);
                if (_clientTxtPath != null)
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
            FileStream fs1 = new FileStream(_clientTxtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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

            var fs = new FileStream(_clientTxtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bool bNewContent = _lastHash == 0;

            using (StreamReader reader = new StreamReader(fs))
            {
                string line;
                int lineHash = 0;
                DateTime lastEvTime = new DateTime();

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

                            // Trigger ready event
                            OnHistoryInitialized(new TrX_CoreLogicGenericEventArgs(this));
                        }
                        _eventQueueInitizalized = true;

                        bNewContent = true;

                        SaveStatsCache();
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
                                    DateTime dt = DateTime.Parse(line.Split(' ')[0] + " " + line.Split(' ')[1], _dateTimeFormatInfo);
                                    ev.EventTime = dt;
                                    lastEvTime = ev.EventTime;
                                }
                                catch
                                {
                                    ev.EventTime = lastEvTime;
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
            _oldestTimeStamp = _myStats.GetOldestTimeStamp();
        }

        /// <summary>
        /// Handle events - Read Queue
        /// </summary>
        private void EventHandling()
        {
            while (!_exit)
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
                        AddTagAutoCreate(sArgs, currentAct);
                    }
                    break;
                case "untag":
                    if (currentAct != null)
                    {
                        RemoveTagFromActivity(sArgs, currentAct);
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
                        FinishActivity(_currentActivity, null, ACTIVITY_TYPES.MAP, DateTime.Now);
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
            IncrementStat("AreaChanges", ev.EventTime, 1);


            // Calculate Instance change based statistics:
            // ===========================================

            // Shaper
            if (bTargetAreaIsShaper && _currentInstanceEndpoint != _lastShaperInstance)
            {
                IncrementStat("ShaperTried", ev.EventTime, 1);
                _lastShaperInstance = _currentInstanceEndpoint;
            }

            // Elder
            if (bTargetAreaIsElder && _currentInstanceEndpoint != _lastElderInstance)
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
            else if (bTargetAreaIsBreachStone)
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
                {
                    if (IS_IN_DEBUG_MODE)
                        _currentActivity.DebugEndEventLine = ev.LogLine;
                    FinishActivity(_currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                }
            }

            // Vaal Side area entered?
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && actType == ACTIVITY_TYPES.VAAL_SIDEAREA)
            {
                if (_currentActivity.SideArea_VaalArea == null)
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
            if (_currentActivity != null && _currentActivity.Type == ACTIVITY_TYPES.MAP && bSourceAreaIsVaal)
            {
                if (_currentActivity.SideArea_VaalArea != null)
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
                {
                    if (IS_IN_DEBUG_MODE)
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
            {
                if (IS_IN_DEBUG_MODE)
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

            if (isMapDeviceActivity)
            {
                if (!bTargetAreaIsShaper)
                {
                    _shaperKillsInFight = 0;
                }

                if (!bTargetAreaIsElder)
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
                            if (IS_IN_DEBUG_MODE) _currentActivity.DebugEndEventLine = ev.LogLine;
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
                    if (_currentActivity.Type == ACTIVITY_TYPES.BREACHSTONE && _defaultMappings.BREACHSTONE_AREAS.Contains(sSourceArea))
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
                                    if (_currentActivity.SideArea_ZanaMap.LastEnded.Year < 2000)
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
                        if (ev.LogLine.Contains("Expedition"))
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

            foreach (TrX_TrackedActivity act in _eventHistory)
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
        public void FinishActivity(TrX_TrackedActivity activity, string sNextMap, ACTIVITY_TYPES sNextMapType, DateTime dtNextMapStarted)
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

                if (isValid)
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
                            //Save to DB
                            SaveToActivityLog(((DateTimeOffset)activity.SideArea_LabTrial.Started).ToUnixTimeSeconds(), GetStringFromActType(activity.SideArea_LabTrial.Type), activity.SideArea_LabTrial.Area, activity.SideArea_LabTrial.AreaLevel, iSecondsLabTrial, activity.SideArea_LabTrial.DeathCounter, activity.SideArea_LabTrial.TrialMasterCount, true, activity.SideArea_LabTrial
                                .Tags, activity.SideArea_LabTrial.Success, Convert.ToInt32(activity.SideArea_LabTrial.PausedTime));
                        }
                    }

                    // Trigger event
                    OnActivityFinished(new TrX_CoreLogicActivityEventArgs(this, activity));
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
        /// Add a new tag
        /// </summary>
        /// <param name="tag"></param>
        private void AddTag(TrX_ActivityTag tag)
        {
            _tags.Add(tag);
            _myDB.DoNonQuery("INSERT INTO tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type, tag_show_in_lv) VALUES "
                + "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'custom', " + (tag.ShowInListView ? "1" : "0") + ")");
            // Trigger event
            OnTagsUpdated(new TrX_CoreLogicGenericEventArgs(this));
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

            if (true) // TODO
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

        // PROPERTIES
        public List<TrX_ActivityTag> Tags
        {
            get { return _tags; }
            set { _tags = value; }
        }

        public TrX_TrackedActivity CurrentActivity
        {
            get { return _currentActivity; }
        }

        public List<TrX_TrackedActivity> ActivityHistory
        {
            get { return _eventHistory; }
        }

        public string ClientTxtPath
        {
            get { return _clientTxtPath; }
            set { _clientTxtPath = value; }
        }

        public bool EventQueueInitialized
        {
            get { return _eventQueueInitizalized; }
        }

        public double LogLinesTotal
        {
            get { return _logLinesTotal; }
        }

        public double LogLinesRead
        {
            get { return _logLinesRead; }
        }

        public bool IsMapZana
        {
            get { return _isMapZana; }
        }

        public bool IsMapVaalArea
        {
            get { return _isMapVaalArea; }
        }

        public bool IsMapLogbookSide
        {
            get { return _isMapLogbookSide; }
        }

        public bool IsMapLabTrial
        {
            get { return _isMapLabTrial; }
        }

        public bool IsMapAbyssArea
        {
            get { return _isMapAbyssArea; }
        }

        public string CurrentArea
        {
            get { return _currentArea; }
        }

        public int CurrentAreaLevel
        {
            get { return _currentAreaLevel; }
        }

        public TrX_TrackedActivity OverlayPrevActivity
        {
            get { return _prevActivityOverlay; }
        }

        public TrX_DBManager Database
        {
            get { return _myDB; }
        }

        public TrX_StatsManager Stats
        {
            get { return _myStats; }
        }

        public Dictionary<string, string> StatNamesLong
        {
            get { return _statNamesLong; }
        }

    }
}
