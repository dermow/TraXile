using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using log4net;
using System.Windows.Forms.DataVisualization.Charting;

namespace TraXile
{
    enum EVENT_TYPES
    {
        APP_STARTED,
        APP_READY,
        ENTERED_AREA,
        PLAYER_DIED,
        DEATH_REASON_RECEIVED,
        ELDER_KILLED,
        SHAPER_KILLED,
        SHAPER_FIGHT_STARTED,
        INSTANCE_CONNECTED,
        SIRUS_KILLED,
        SIRUS_FIGHT_STARTED,
        VERITANIA_FIGHT_STARTED,
        VERITANIA_KILLED,
        BARAN_FIGHT_STARTED,
        BARAN_KILLED,
        DROX_FIGHT_STARTED,
        DROX_KILLED,
        HUNTER_FIGHT_STARTED,
        HUNTER_KILLED,
        TRIALMASTER_STARTED,
        TRIALMASTER_KILLED,
        MAVEN_FIGHT_STARTED,
        MAVEN_KILLED,
        TRIALMASTER_SPEECH,
        TRIALMASTER_ROUND_STARTED,
        EINHAR_BEAST_CAPTURE,
        TRIALMASTER_TOOK_REWARD,
        TRIALMASTER_VICTORY,
        UBER_ELDER_STARTED,
        UBER_ELDER_KILLED,
        PARTYMEMBER_ENTERED_AREA,
        PARTYMEMBER_LEFT_AREA,
        CATARINA_FIGHT_STARTED,
        CATARINA_KILLED
    }

    public partial class MainW : Form
    {
        public string sPoELogFilePath;
        private string sCurrentArea;
        private string sCurrentInstanceEndpoint;
        private int iLastHash = 0;
        private double dLogLinesTotal;
        private double dLogLinesRead;
        private Thread thParseLog;
        private Thread thEvents;
        private DateTime dtInAreaSince;
        private DateTime dtLastDeath;
        private DateTime dtInitStart;
        private DateTime dtInitEnd;
        private EVENT_TYPES lastEvType;
        private TrackedMap currentMap;
        private string sLastDeathReason;
        private bool bEventQInitialized;
        private bool bIsMapZana;
        private bool bExit;
        private bool bElderFightActive;
        private int iShaperKillsInFight;
        private SqliteConnection dbconn;
        private bool bHistoryInitialized;
        List<string> mapList, heistList, knownPlayers;
        Dictionary<string, EVENT_TYPES> eventMap;
        Dictionary<int, string> dict;
        Dictionary<string, int> numStats;
        Dictionary<string, string> statNamesLong;
        Dictionary<string, ListViewItem> statLvItems;
        LoadScreen loadScreen;
        List<TrackedMap> mapHistory;
        ConcurrentQueue<TrackedEvent> eventQueue;
        ILog log;
        
        public MainW()
        {
            this.Visible = false;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            this.Opacity = 0;

            listView1.Columns[0].Width = 120;
            listView1.Columns[1].Width = 50;
            listView1.Columns[2].Width = 110;
            listView1.Columns[3].Width = 100;
            listView1.Columns[4].Width = 50;
            listView2.Columns[0].Width = 500;
            listView2.Columns[1].Width = 300;

            chart1.ChartAreas[0].AxisX.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisY.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.Red;
            chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.Red;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 1;
            chart1.Series[0].XValueType = ChartValueType.DateTime;
            chart1.Legends[0].Enabled = false;

            var ca = chart1.ChartAreas["ChartArea1"].CursorX;
            ca.IsUserEnabled = true;
            ca.IsUserSelectionEnabled = true;
            
            comboBox1.SelectedIndex = 1;

            dict = new Dictionary<int, string>();
            eventQueue = new ConcurrentQueue<TrackedEvent>();
            mapHistory = new List<TrackedMap>();
            knownPlayers = new List<string>();
            statLvItems = new Dictionary<string, ListViewItem>();
            sCurrentArea = "-";
            dtInAreaSince = DateTime.Now;
            sLastDeathReason = "-";
            bEventQInitialized = false;

            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Application started");

            this.Text = APPINFO.NAME + " " + APPINFO.VERSION;
            dtInitStart = DateTime.Now;

            ReadSettings();

            if(sPoELogFilePath == null)
            {
                FileSelectScreen fs = new FileSelectScreen(this);
                fs.ShowDialog();
            }

            loadScreen = new LoadScreen();
            loadScreen.Show(this);

            BuildmapList();
            InitDatabase();
            
            lastEvType = EVENT_TYPES.APP_STARTED;
            InitNumStats();
                      
                        
            eventQueue.Enqueue(new TrackedEvent(EVENT_TYPES.APP_STARTED) { EventTime = DateTime.Now, LogLine = "Application started." });

            ReadStatsCache();
            BuildEventMap();
            ReadKnownPlayers();

            // Thread for Log Parsing and Enqueuing
            thParseLog = new Thread(new ThreadStart(LogParsing))
            {
                IsBackground = true
            };
            thParseLog.Start();

            // Thread for Queue processing / Dequeuing
            thEvents = new Thread(new ThreadStart(HandleEvents))
            {
                IsBackground = true
            };
            thEvents.Start();
        }

        public void ReloadLogFile()
        {
            ResetStats();
            this.bEventQInitialized = false;
            iLastHash = 0;
            SaveStatsCache();
            Application.Restart();
        }

        private void InitNumStats()
        {
            numStats = new Dictionary<string, int>
            {
                { "TotalMapsDone", 0 },
                { "TotalHeistsDone", 0 },
                { "TotalKilledCount", 0 },
                { "ElderTried", 0 },
                { "ElderKilled", 0 },
                { "ShaperTried", 0 },
                { "ShaperKilled", 0 },
                { "SirusStarted", 0 },
                { "SirusKilled", 0 },
                { "HunterKilled", 0 },
                { "HunterStarted", 0 },
                { "VeritaniaKilled", 0 },
                { "VeritaniaStarted", 0 },
                { "BaranStarted", 0 },
                { "BaranKilled", 0 },
                { "DroxStarted", 0 },
                { "DroxKilled", 0 },
                { "TrialMasterStarted", 0 },
                { "TrialMasterKilled", 0 },
                { "MavenStarted", 0 },
                { "MavenKilled", 0 },
                { "EinharCaptures", 0 },
                { "TrialMasterTookReward", 0 },
                { "TrialMasterVictory", 0 },
                { "TrialMasterSuccess", 0 },
                { "CatarinaTried", 0 },
                { "CatarinaKilled", 0 }
            };

            statNamesLong = new Dictionary<string, string>
            {
                { "TotalMapsDone", "Total maps done" },
                { "TotalHeistsDone", "Total heists done" },
                { "ElderKilled", "Killed the Elder" },
                { "ShaperKilled", "Killed the Shaper" },
                { "SirusStarted", "Tried Sirus" },
                { "SirusKilled", "Killed Sirus" },
                { "HunterKilled", "Killed Al-Hezmin, the Hunter (not reliable*)" },
                { "HunterStarted", "Tried Al-Hezmin, the Hunter" },
                { "VeritaniaKilled", "Killed Veritania, the Redeemer (not reliable*)" },
                { "VeritaniaStarted", "Tried Veritania, the Redeemer" },
                { "BaranStarted", "Tried Baran, the Crusader" },
                { "BaranKilled", "Killed Baran, the Crusader (not reliable*)" },
                { "DroxStarted", "Tried Drox, the Warlord" },
                { "DroxKilled", "Killed Drox, the Warlord (not reliable*)" },
                { "TrialMasterStarted", "Tried Trialmaster-Fight" },
                { "TrialMasterKilled", "Killed the Trialmaster" },
                { "MavenStarted", "Tried the Maven" },
                { "MavenKilled", "Killed the Maven" },
                { "TotalKilledCount", "Death count" },
                { "EinharCaptures", "Einhar beasts captured" },
                { "TrialMasterTookReward", "Ultimatum: took rewards" },
                { "TrialMasterVictory", "Ultimatum: cleared all rounds" },
                { "TrialMasterSuccess", "Ultimatum: did not fail" },
                { "ShaperTried", "Tried the Shaper" },
                { "ElderTried", "Tried the Elder" },
                { "CatarinaTried", "Tried Catarina" },
                { "CatarinaKilled", "Killed Catarina" }
            };

            foreach (string s in mapList)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("MapsFinished_" + sName))
                    numStats.Add("MapsFinished_" + sName, 0);
                if (!statNamesLong.ContainsKey("MapsFinished_" + sName))
                    statNamesLong.Add("MapsFinished_" + sName, "Maps finished: " + sName);
            }
            foreach (string s in heistList)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("HeistsFinished_" + sName))
                    numStats.Add("HeistsFinished_" + sName, 0);
                if (!statNamesLong.ContainsKey("HeistsFinished_" + sName))
                    statNamesLong.Add("HeistsFinished_" + sName, "Heists finished: " + sName);
            }
        }

        private string GetStatShortName(string s_key)
        {
            foreach(KeyValuePair<string,string> kvp in statNamesLong)
            {
                if (kvp.Value == s_key)
                    return kvp.Key;
            }
            return null;
        }

        private string GetStatLongName(string s_key)
        {
            if(statNamesLong.ContainsKey(s_key))
            {
                return statNamesLong[s_key];
            }
            else
            {
                return s_key;
            }
        }

        public void ResetStats()
        {
            this.ClearStatsDB();
        }

        private void ClearStatsDB()
        {
            SqliteCommand cmd;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "drop table tx_stats";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_stats " +
                "(timestamp int, " +
                "stat_name text, " +
                "stat_value int)";
            cmd.ExecuteNonQuery();

            InitNumStats();
            SaveStatsCache();
        }

        private void InitDatabase()
        {
            dbconn = new SqliteConnection("Data Source=data.db");
            dbconn.Open();

            //Create Tables
            SqliteCommand cmd;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_activity_log " +
                "(timestamp int, " +
                "act_type text, " +
                "act_area text, " +
                "act_stopwatch int, " +
                "act_deathcounter int," +
                "act_ulti_rounds int," +
                "act_is_zana int)";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_stats " +
                "(timestamp int, " +
                "stat_name text, " +
                "stat_value int)";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_stats_cache " +
                "(" +
                "stat_name text, " +
                "stat_value int)";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_known_players " +
                "(" +
                "player_name )";
            cmd.ExecuteNonQuery();
        }

        private void AddKnownPlayerIfNotExists(string s_name)
        {
            if(!knownPlayers.Contains(s_name))
            {
                knownPlayers.Add(s_name);
                SqliteCommand cmd;
                cmd = dbconn.CreateCommand();
                cmd.CommandText = "insert into tx_known_players (player_name) VALUES ('" + s_name + "')";
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteActLogEntry(long l_timestamp)
        {
            SqliteCommand cmd;
            cmd = dbconn.CreateCommand();
            cmd.CommandText = "delete from tx_activity_log where timestamp = " + l_timestamp.ToString();
            cmd.ExecuteNonQuery();
        }

        private void ReadKnownPlayers()
        {
            SqliteDataReader sqlReader;
            SqliteCommand cmd;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tx_known_players";
            sqlReader = cmd.ExecuteReader();

            while (sqlReader.Read())
            {
                knownPlayers.Add(sqlReader.GetString(0));
            }
        }

        private void SaveToActivityLog(long i_ts, string s_type, string s_area, int i_stopwatch, int i_death_counter, int i_ulti_rounds, bool b_zana)
        {
            //replace ' in area
            s_area = s_area.Replace("'", "");

            SqliteCommand cmd;
            cmd = dbconn.CreateCommand();
            cmd.CommandText = "insert into tx_activity_log " +
               "(timestamp, " +
               "act_type, " +
               "act_area, " +
               "act_stopwatch, " +
               "act_deathcounter, " +
               "act_ulti_rounds," +
               "act_is_zana) VALUES (" +
               i_ts.ToString() 
                 + ", '" + s_type 
                 + "', '" + s_area 
                 + "', " + i_stopwatch 
                 + ", " + i_death_counter 
                 + ", " + i_ulti_rounds 
                 + ", " + (b_zana ? "1" : "0") + ")";

            cmd.ExecuteNonQuery();
        }

        private void BuildEventMap()
        {
            eventMap = new Dictionary<string, EVENT_TYPES>
            {
                // Generic events
                { "You have entered", EVENT_TYPES.ENTERED_AREA },
                { "has joined the area", EVENT_TYPES.PARTYMEMBER_ENTERED_AREA },
                { "has left the area", EVENT_TYPES.PARTYMEMBER_LEFT_AREA },
                { "Player died", EVENT_TYPES.DEATH_REASON_RECEIVED },
                { "has been slain", EVENT_TYPES.PLAYER_DIED },
                { "Zana, Master Cartographer: So that was the Elder... what terrible", EVENT_TYPES.ELDER_KILLED },
                { "The Shaper: I see you, little mouse... you don't belong here. Run along!", EVENT_TYPES.SHAPER_FIGHT_STARTED },
                { "The Shaper: Irrelevant!", EVENT_TYPES.SHAPER_KILLED },
                { "The Shaper: Imperfect!", EVENT_TYPES.SHAPER_KILLED },
                { "The Shaper: Insubstantial!", EVENT_TYPES.SHAPER_KILLED },
                { "The Shaper: Insignificant!", EVENT_TYPES.SHAPER_KILLED },
                { "The Shaper: Inconsequential!", EVENT_TYPES.SHAPER_KILLED },
                { "Connecting to instance server at", EVENT_TYPES.INSTANCE_CONNECTED },
                { "Zana, Master Cartographer: I'm sorry... Sirus... This was all my fault. I'm sorry. I'm so, so sorry.", EVENT_TYPES.SIRUS_KILLED },
                { "Sirus, Awakener of Worlds: Did you really think this would work?", EVENT_TYPES.SIRUS_FIGHT_STARTED },

                // Veritania Fight Events
                { "You and I both know this isn't over.", EVENT_TYPES.VERITANIA_KILLED },
                { "The roots have taken hold. We shall see each other again.", EVENT_TYPES.VERITANIA_KILLED },
                { "You insist on dooming yourself...", EVENT_TYPES.VERITANIA_KILLED },
                { "The thrill of victory shall forever be your prison...", EVENT_TYPES.VERITANIA_KILLED },
                { "The end of the cycle of madness... until next time...", EVENT_TYPES.VERITANIA_KILLED },
                { "I thought myself better than you... but you and I are bound to waltz together forever...", EVENT_TYPES.VERITANIA_KILLED },
                { "My only consolation... is that I am not alone in this torture... see you soon...", EVENT_TYPES.VERITANIA_KILLED },
                { "You stand victorious, your heart's desire... the Atlas provides... your addiction...", EVENT_TYPES.VERITANIA_KILLED },
                { "This is my punishment. Come receive yours, exile.", EVENT_TYPES.VERITANIA_FIGHT_STARTED },
                { "This time, I may show mercy.", EVENT_TYPES.VERITANIA_FIGHT_STARTED },
                { "The truth is cold. Let me show you.", EVENT_TYPES.VERITANIA_FIGHT_STARTED },
                { "I think I shall torture you this time.", EVENT_TYPES.VERITANIA_FIGHT_STARTED },
                { "Welcome! Time has no meaning here. Your pain will be endless.", EVENT_TYPES.VERITANIA_FIGHT_STARTED },

                // Al-Hezmin Fight Events
                { "Fun... for you...", EVENT_TYPES.HUNTER_KILLED },
                { "This is no longer enjoyable...", EVENT_TYPES.HUNTER_KILLED },
                { "This is not the wondrous land we imagined. We should have listened to Zana.", EVENT_TYPES.HUNTER_KILLED },
                { "Not... possible...", EVENT_TYPES.HUNTER_KILLED },
                { "You've killed me a thousand times, but those were just dreams...", EVENT_TYPES.HUNTER_KILLED },
                { "Molten One, save me from this unending nightmare!", EVENT_TYPES.HUNTER_KILLED },
                { "Let me out... please, let me out...", EVENT_TYPES.HUNTER_KILLED },
                { "I awaken from this nightmare... at last...", EVENT_TYPES.HUNTER_KILLED },
                { "There is no true safety from you, is there?", EVENT_TYPES.HUNTER_FIGHT_STARTED },
                { "Come, monster. Do your worst.", EVENT_TYPES.HUNTER_FIGHT_STARTED },
                { "I will never go down without a fight, beast.", EVENT_TYPES.HUNTER_FIGHT_STARTED },
                { "The spectre out of the mist comes to haunt me yet again.", EVENT_TYPES.HUNTER_FIGHT_STARTED },
                { "With your death, this nightmare may finally end.", EVENT_TYPES.HUNTER_FIGHT_STARTED },

                // Drox Fight Events
                { "You can bend the law, but never break it..", EVENT_TYPES.DROX_KILLED },
                { "You reject my lesson at your own peril...", EVENT_TYPES.DROX_KILLED },
                { "Law and order are not enough, it seems...", EVENT_TYPES.DROX_KILLED },
                { "You have not yet witnessed... the peak of my power...", EVENT_TYPES.DROX_KILLED },
                { "Your strength... is the law...", EVENT_TYPES.DROX_KILLED },
                { "My kingdom is yours... if you can keep it...", EVENT_TYPES.DROX_KILLED },
                { "Time takes even the greatest of men...", EVENT_TYPES.DROX_KILLED },
                { "Veritania... my kingdom for... Veritania...", EVENT_TYPES.DROX_KILLED },
                { "I shall crush all who oppose my will!", EVENT_TYPES.DROX_FIGHT_STARTED },
                { "Killing you will be utterly satisfying.", EVENT_TYPES.DROX_FIGHT_STARTED },
                { "You've made lawless criminals of us both. Let me show you what I have learned", EVENT_TYPES.DROX_FIGHT_STARTED },
                { "I wasted so much time trying to enforce the law. Those rules were nothing but a cage.", EVENT_TYPES.DROX_FIGHT_STARTED },
                { "I've killed countless thousands. What's one more?", EVENT_TYPES.DROX_FIGHT_STARTED },

                // Baran Fight Events
                { "No minion of Sin could be this powerful... what are you?", EVENT_TYPES.BARAN_KILLED },
                { "... yours is no righteous strength. Deceiver! You are no servant of Innocence!", EVENT_TYPES.BARAN_KILLED },
                { "You reject... my wisdom...? Blasphemer...", EVENT_TYPES.BARAN_KILLED },
                { "I see now... what you are... even God has his opposite...", EVENT_TYPES.BARAN_KILLED },
                { "Kirac... sent you? You must keep him out of the Atlas... I cannot be saved... ", EVENT_TYPES.BARAN_KILLED },
                { "Tell Zana...", EVENT_TYPES.BARAN_KILLED },
                { "... tell my brother... that I cannot be saved...", EVENT_TYPES.BARAN_KILLED },
                { "Keep Kirac... out of the Atlas... at all costs...", EVENT_TYPES.BARAN_KILLED },
                { "A brief moment of sanity... it never lasts...", EVENT_TYPES.BARAN_KILLED },
                { "Shadow can never overtake light, demon.", EVENT_TYPES.BARAN_FIGHT_STARTED },
                { "How dare you question God?!", EVENT_TYPES.BARAN_FIGHT_STARTED },
                { "You seek Forgiveness, but you do not deserve it.", EVENT_TYPES.BARAN_FIGHT_STARTED },
                { "I am the Fire of Creation, and I shall purify your soul.", EVENT_TYPES.BARAN_FIGHT_STARTED },
                { "I shall cast you into a pit of infinite tortures.", EVENT_TYPES.BARAN_FIGHT_STARTED },

                // Trialmaster
                { "The Trialmaster: Time to end this!", EVENT_TYPES.TRIALMASTER_STARTED },
                { "The Trialmaster: ...and my service continues. Chaos laughs, mortal, and fortune is yours.", EVENT_TYPES.TRIALMASTER_KILLED },

                // Maven
                { "The Maven: ...my collection.", EVENT_TYPES.MAVEN_FIGHT_STARTED },
                { "The Maven: I apologize to the toy. I did not realise... you... were like me...", EVENT_TYPES.MAVEN_KILLED },

                //Einhar
                { "Great job, Exile! Einhar will take the captured beast to the Menagerie.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "The First Ones look upon this capture with pride, Exile. You hunt well.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "Survivor! You are well prepared for the end. This is a fine capture.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "Haha! You are captured, stupid beast.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "You have been captured, beast. You will be a survivor, or you will be food.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "This one is captured. Einhar will take it.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },

                // Ultimatum
                /* { "The Trialmaster: Kill them all.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Slay everything.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Leave none alive.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Bring death.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Defend what is yours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Make your stand.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Hold your ground... if you can.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Stem the tide.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Try not to die.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Live.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Cling to life.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Outlive my expectations.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Begin the conquest!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Take what is yours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Let none stop you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Conquer all!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Go forth and destroy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Destroy that which you seek.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Destroyer, begin!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Destroy everything.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: You will fall to ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Ruin seeks you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Ruin arrives suddenly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Ruin's burden weighs twice as heavy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Ruin surrounds you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Ruin hides in every corner.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Best be quick...", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Time is your enemy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Never enough time.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The sun is setting on your success.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Mortality seeks you in kind.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: No defence will be enough.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Death is already creeping upon you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: You are more frail than you know.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Death is swift.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Frailty races through your bones.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your enemies hasten.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The tide of death is rapid.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: A quicker resistance.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: They move with utmost haste.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Speed upon speed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: They accelerate beyond reason.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Fight the slowing of your own heart.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Can you feel death coiling around your heart?", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your heart already beats more slowly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your arteries constrict.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your blood weakens.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Death spreads within your veins.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Choking vapors seek the living.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Avoid the miasma, if you can.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Miasma spreads.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Rare is the breath of living air.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The vapors are inescapable.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Contamination moves swiftly on the wind.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Slay them amongst the flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Fire seeks what it will.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The wind conspires with the flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The flames race toward you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Flames beget more flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Fire always spreads.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The dead reach out with an icy grip.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Vengeance is cold indeed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Death's cold grip is inescapable.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: From death, hate grasps for all life.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Hate begets hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Be careful how quickly you slay...", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The storm approaches.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Lightning descends.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The storm comes swiftly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Lightning gives little warning.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Lightning always strikes twice.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Thunder and lightning abound.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your foes bring summer's flame.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The fire of summer burns within.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your opponents bring winter's hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: A battlefield chilled by winter's hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your adversaries bring spring's thunder.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Spring's thunder. Wrath itself.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your opponents bring night's madness.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Night's madness drains away all hope.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Steel and pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Shrapnel and shards abound.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Relentless assault.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Be torn to shreds.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Step quickly lest the detonations end you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: They seek to surprise you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Beware the bite of the chained beast.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Stray too far and find pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Nature hungers at the fringes.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The hungry beasts are numerous.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: A horde of hungry beasts.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Hunger begets savagery.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Quick jaws make quick work of prey.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: A weight upon the soul.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Heart and spirit, burdened.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: You shall suffer your own bile.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your epithets, echoed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your thoughts crackle with pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Lightning lurks within you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The blood sours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your blood betrays you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your reach exceeds your grasp.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Be diminished.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Your own volleys turn against you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: Confront that which you release.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "The Trialmaster: The safe choice.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },
                { "The Trialmaster: The expected choice.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },
                { "The Trialmaster: So be it, as disappointing as it is.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },
                { "The Trialmaster: Honestly, it's about time. Congratulations, challenger. Sincerely.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: You... you won? I honestly didn't expect that of you.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Thank you for not disappointing my master yet again.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Your series of losses has finally ended.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: I was not certain you had it in you.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Fair enough. Luck is luck.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: You have restored your pride.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Win some... lose most.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: You have stemmed the tide of losses.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: You have redeemed yourself.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Thus, the vagaries of chance.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Keeping it interesting, I see.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: A meagre turnaround.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: That makes two in a row!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: A second victory!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: I can practically see your ego swelling.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Don't get too eager.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Impressive, challenger.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Your series of victories continues.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Luck can only carry you so far....", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: For now, the victor remains the victor!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Not bad... for a mortal.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: This will not continue. I am unconcerned.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: This is no longer amusing.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: This series of victories is astonishing.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: How do you keep winning?!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: I suspect I am being made a fool of!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: I should have become a priest of Yaomac instead...", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: You win... again.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: This is insufferable...", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "The Trialmaster: Take your prize and go.", EVENT_TYPES.TRIALMASTER_VICTORY } */
                {"Catarina, Master of Undeath: You found me at last... Very resourceful, Jun. I too am resourceful. Witness.", EVENT_TYPES.CATARINA_FIGHT_STARTED },
                {"Catarina, Master of Undeath: Don't do this, Exile. You can still join us. It's your chance to bring back anyone you've ever loved or cared about.", EVENT_TYPES.CATARINA_KILLED }
                
            };

        }

        private void ReadActivityLogFromSQLite()
        {
            SqliteDataReader sqlReader;
            SqliteCommand cmd;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tx_activity_log";
            sqlReader = cmd.ExecuteReader();

            while(sqlReader.Read())
            {
                TrackedMap map = new TrackedMap
                {
                    Started = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(sqlReader.GetInt32(0)),
                    TimeStamp = sqlReader.GetInt32(0),
                    Type = sqlReader.GetString(1),
                    Area = sqlReader.GetString(2),
                    DeathCounter = sqlReader.GetInt32(4),
                    TrialMasterCount = sqlReader.GetInt32(5)
                };
                //mapHistory
                mapHistory.Insert(0, map);

                TimeSpan ts = TimeSpan.FromSeconds(sqlReader.GetInt32(3));
                AddMapLvItem(map, false, String.Format("{0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds));
                
            }
            bHistoryInitialized = true;
        }

        private void LogParsing()
        {
            while(true)
            {
                Thread.Sleep(1000);
                if(sPoELogFilePath != null)
                {
                    ParseLogFile();
                  
                }
            }
        }

        private int GetLogFileLineCount()
        {
            int iCount = 0;
            FileStream fs1 = new FileStream(sPoELogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TextReader reader1 = new StreamReader(fs1);
            while ((reader1.ReadLine()) != null)
            {
                iCount++;
            }
            reader1.Close();
            return iCount;
        }

        private void ParseLogFile()
        {
            dLogLinesTotal = Convert.ToDouble(GetLogFileLineCount());

            var fs = new FileStream(sPoELogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bool bNewContent = iLastHash == 0;

            using (StreamReader reader = new StreamReader(fs))
            {
                string line;

                // Keep file open
                while(!bExit)
                {
                    line = reader.ReadLine();

                    if (line == null)
                    {
                        if (!bEventQInitialized)
                        {
                            currentMap = null;
                            bIsMapZana = false;
                            dtInitEnd = DateTime.Now;
                            TimeSpan tsInitDuration = (dtInitEnd - dtInitStart);
                            eventQueue.Enqueue(new TrackedEvent(EVENT_TYPES.APP_READY) 
                            { 
                                EventTime = DateTime.Now, 
                                LogLine = "Application initialized in " 
                                  + Math.Round(tsInitDuration.TotalSeconds, 2) + " seconds." 
                            });                        
                        }
                        bEventQInitialized = true;

                        Thread.Sleep(100);
                        continue;
                    }
              
                    int lineHash = line.GetHashCode();
                    if (dict.ContainsKey(lineHash))
                        continue;

                    if(lineHash == iLastHash)
                    {
                        bNewContent = true;
                    }

                    if (!bNewContent)
                    {
                        dLogLinesRead++;
                        continue;
                    }

                    iLastHash = lineHash;

                    foreach (KeyValuePair<string,EVENT_TYPES> kv in eventMap)
                    {
                        if (line.Contains(kv.Key))
                        {
                            if (!dict.ContainsKey(lineHash))
                            {
                                TrackedEvent ev = new TrackedEvent(kv.Value)
                                {
                                    LogLine = line
                                };
                                try
                                {
                                    ev.EventTime = DateTime.Parse(line.Split(' ')[0]);
                                }
                                catch
                                {
                                    ev.EventTime = DateTime.Now;
                                }
                                dict.Add(lineHash, "init");

                                if(!bEventQInitialized)
                                {
                                    HandleSingleEvent(ev, true);
                                }
                                else
                                {
                                    eventQueue.Enqueue(ev);
                                }
                                
                            }
                        }
                    }
                    dLogLinesRead++;
                }
            }
        }

        private void HandleEvents()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (bEventQInitialized)
                {
                    while (eventQueue.TryDequeue(out TrackedEvent deqEvent))
                    {
                        HandleSingleEvent(deqEvent);
                    }
                }
            }
        }

        private bool CheckIfAreaIsMap(string sArea, string sSourceArea = "")
        {
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

            foreach (string s in mapList)
            {
               if (s.Trim().Equals(sArea.Trim()))
                    return true;
            }
            return false;
        }

        private bool CheckIfAreaIsHeist(string sArea, string sSourceArea = "")
        {
            if (sArea == "Laboratory")
            {
                if(sSourceArea == "The Rogue Harbour")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            foreach (string s in heistList)
            {
                if (s.Trim().Equals(sArea.Trim()))
                    return true;
            }
            return false;
        }

        private void HandleSingleEvent(TrackedEvent ev, bool bInit = false)
        {
            switch (ev.EventType)
            {
                case EVENT_TYPES.ENTERED_AREA:

                    string sSourceArea = sCurrentArea;
                    string sTargetArea = GetAreaNameFromEvent(ev);
                    string sAreaName = GetAreaNameFromEvent(ev);
                    bool bSourceAreaIsMap = CheckIfAreaIsMap(sSourceArea);
                    bool bTargetAreaIsMap = CheckIfAreaIsMap(sTargetArea, sSourceArea);
                    bool bTargetAreaIsHeist = CheckIfAreaIsHeist(sTargetArea, sSourceArea);

                    dtInAreaSince = ev.EventTime;

                    // Special calculation for Elder fight - he has no start dialoge.
                    if(sAreaName == "Absence of Value and Meaning".Trim())
                    {
                        if(!bElderFightActive) // && (sCurrentInstanceEndpoint != sCurrentElderEndpoint))
                        {
                            IncrementStat("ElderTried", ev.EventTime, 1);
                        }
                        bElderFightActive = true;
                    }

                    if(bTargetAreaIsMap || bTargetAreaIsHeist)
                    {
                        bElderFightActive = false;
                        iShaperKillsInFight = 0;

                        if(currentMap == null)
                        {
                            currentMap = new TrackedMap
                            {
                                Area = sTargetArea,
                                Type = bTargetAreaIsHeist ? "Heist" : "Map",
                                Started = DateTime.Parse(ev.LogLine.Split(' ')[0] + " " + ev.LogLine.Split(' ')[1]),
                                InstanceEndpoint = sCurrentInstanceEndpoint
                            };
                        }
                        if(!currentMap.Paused)
                            currentMap.StartStopWatch();

                        if(bSourceAreaIsMap && bTargetAreaIsMap)
                        {
                            if(!bIsMapZana)
                            {
                                // entered Zana Map
                                bIsMapZana = true;
                                currentMap.StopStopWatch();
                                if(currentMap.ZanaMap == null)
                                {
                                    currentMap.ZanaMap = new TrackedMap
                                    {
                                        Type = "Map",
                                        Area = sTargetArea,
                                        Started = DateTime.Now
                                        
                                    };
                                }
                                if(!currentMap.ZanaMap.Paused)
                                    currentMap.ZanaMap.StartStopWatch();
                            }
                            else
                            {
                                // leave Zana Map
                                if(currentMap.ZanaMap != null)
                                {
                                    bIsMapZana = false;
                                    currentMap.ZanaMap.StopStopWatch();
                                    if(!currentMap.Paused)
                                        currentMap.StartStopWatch();
                                }
                            }
                        }
                        else
                        {
                            // Do not track Lab-Trials
                            if(!sSourceArea.Contains("Trial of"))
                            {
                                if (sTargetArea != currentMap.Area || sCurrentInstanceEndpoint != currentMap.InstanceEndpoint)
                                {
                                    FinishMap(currentMap, sTargetArea, bTargetAreaIsHeist ? "Heist" : "Map", DateTime.Parse(ev.LogLine.Split(' ')[0] + " " + ev.LogLine.Split(' ')[1]));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentMap != null)
                            currentMap.StopStopWatch();
                    }

                    sCurrentArea = sAreaName;
                    break;
                case EVENT_TYPES.PLAYER_DIED:
                    string sPlayerName = ev.LogLine.Split(' ')[8];
                    if(!knownPlayers.Contains(sPlayerName))
                    {
                        IncrementStat("TotalKilledCount", ev.EventTime, 1);
                        dtLastDeath = DateTime.Now;
                        sLastDeathReason = GetDeathReasonromEvent(ev);
                        if (currentMap != null)
                        {
                            if (bIsMapZana)
                            {
                                if (currentMap.ZanaMap != null)
                                {
                                    currentMap.ZanaMap.DeathCounter++;
                                }

                            }
                            else
                            {
                                currentMap.DeathCounter++;
                            }
                        }
                    }
                   
                    break;
                case EVENT_TYPES.DEATH_REASON_RECEIVED:
                    sLastDeathReason = GetDeathReasonromEvent(ev);
                    break;
                case EVENT_TYPES.ELDER_KILLED:
                    IncrementStat("ElderKilled", ev.EventTime, 1);
                    bElderFightActive = false;
                    break;
                case EVENT_TYPES.SHAPER_KILLED:
                    // shaper has 3x the same kill dialogue
                    iShaperKillsInFight++;
                    if(iShaperKillsInFight == 3)
                    {
                        IncrementStat("ShaperKilled", ev.EventTime, 1);
                        iShaperKillsInFight = 0;
                    }
                    break;
                case EVENT_TYPES.SIRUS_FIGHT_STARTED:
                    IncrementStat("SirusStarted", ev.EventTime, 1);
                    break;
                case EVENT_TYPES.SIRUS_KILLED:
                    IncrementStat("SirusKilled", ev.EventTime, 1);
                    break;
                case EVENT_TYPES.INSTANCE_CONNECTED:
                    sCurrentInstanceEndpoint = GetEndpointFromInstanceEvent(ev);
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
                    if (currentMap != null)
                    {
                        currentMap.TrialMasterSuccess = true;
                        currentMap.TrialMasterFullFinished = true;
                    }
                    break;
                case EVENT_TYPES.TRIALMASTER_TOOK_REWARD:
                    IncrementStat("TrialMasterTookReward", ev.EventTime, 1);
                    IncrementStat("TrialMasterSuccess", ev.EventTime, 1);
                    if (currentMap != null)
                    {
                        currentMap.TrialMasterSuccess = true;
                        currentMap.TrialMasterFullFinished = false;
                    }
                    break;
                case EVENT_TYPES.MAVEN_KILLED:
                    IncrementStat("MavenKilled", ev.EventTime, 1);
                    break;
                case EVENT_TYPES.TRIALMASTER_KILLED:
                    IncrementStat("TrialMasterKilled", ev.EventTime, 1);
                    break;
                case EVENT_TYPES.VERITANIA_KILLED:
                    if(lastEvType != EVENT_TYPES.VERITANIA_KILLED)
                    {
                        IncrementStat("VeritaniaKilled", ev.EventTime, 1);
                    }
                    break;
                case EVENT_TYPES.DROX_KILLED:
                    if (lastEvType != EVENT_TYPES.DROX_KILLED)
                    {
                        IncrementStat("DroxKilled", ev.EventTime, 1);
                    }
                    break;
                case EVENT_TYPES.BARAN_KILLED:
                    if (lastEvType != EVENT_TYPES.BARAN_KILLED)
                    {
                        IncrementStat("BaranKilled", ev.EventTime, 1);
                    }
                    break;
                case EVENT_TYPES.HUNTER_KILLED:
                    if (lastEvType != EVENT_TYPES.HUNTER_KILLED)
                    {
                        IncrementStat("HunterKilled", ev.EventTime, 1);
                    }
                    break;
                case EVENT_TYPES.TRIALMASTER_ROUND_STARTED:
                    if(currentMap != null)
                    {
                        currentMap.TrialMasterCount += 1;
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
                lastEvType = ev.EventType;
            }

            if (!bInit) TextLogEvent(ev);
        }

        private void IncrementStat(string s_key, DateTime dt, int i_value = 1)
        {
            numStats[s_key] += i_value;

            SqliteCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = "INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + numStats[s_key] + ")";
            cmd.ExecuteNonQuery();
        }

        private string GetEndpointFromInstanceEvent(TrackedEvent ev)
        {
            return ev.LogLine.Split(new String[] { "Connecting to instance server at "}, StringSplitOptions.None)[1];
        }

        private void FinishMap(TrackedMap map, string sNextMap, string sNextMapType, DateTime dtNextMapStarted)
        {
            currentMap.StopStopWatch();
            mapHistory.Insert(0, currentMap);

            if(bEventQInitialized)
            {
                TextLog("Map finished (" + map.Area + "): " + currentMap.StopWatchValue.ToString());
                AddMapLvItem(map);
                SaveToActivityLog(((DateTimeOffset)map.Started).ToUnixTimeSeconds(), map.Type, map.Area, Convert.ToInt32(map.StopWatchTimeSpan.TotalSeconds), map.DeathCounter, map.TrialMasterCount, false);
                if (map.ZanaMap != null)
                {
                    AddMapLvItem(map.ZanaMap, true);
                    SaveToActivityLog(((DateTimeOffset)map.ZanaMap.Started).ToUnixTimeSeconds(), map.Type, map.ZanaMap.Area, Convert.ToInt32(map.ZanaMap.StopWatchTimeSpan.TotalSeconds), map.ZanaMap.DeathCounter, map.ZanaMap.TrialMasterCount, true);
                }
            }
           
            if(sNextMap != null)
            {
                currentMap = new TrackedMap
                {
                    Area = sNextMap,
                    Type = sNextMapType,
                    InstanceEndpoint = sCurrentInstanceEndpoint,
                    Started = dtNextMapStarted
                };
                currentMap.StartStopWatch();
            }
            else
            {
                currentMap = null;
            }

            if(map.Type == "Heist")
            {
                IncrementStat("TotalHeistsDone", map.Started, 1);
                IncrementStat("HeistsFinished_" + map.Area, map.Started, 1);
            }
            else
            {
                IncrementStat("TotalMapsDone", map.Started, 1);
                IncrementStat("MapsFinished_" + map.Area, map.Started, 1);
            }
        }

        //TODO: replace with DB table
        private void ReadStatsCache()
        {
            if(File.Exists("stats.cache"))
            {
                StreamReader r = new StreamReader("stats.cache");
                string line;
                int iLine = 0;
                while ((line = r.ReadLine()) != null)
                {
                    if (iLine == 0)
                    {
                        iLastHash = Convert.ToInt32(line.Split(';')[1]);
                    }
                    else
                    {
                        numStats[line.Split(';')[0]] = Convert.ToInt32(line.Split(';')[1]);
                    }

                    iLine++;
                }
                r.Close();
            }
        }

        private void SaveStatsCache()
        {
            StreamWriter wrt = new StreamWriter("stats.cache");
            wrt.WriteLine("last;" + iLastHash.ToString());
            foreach(KeyValuePair<string, int> kvp in numStats)
            {
                wrt.WriteLine(kvp.Key + ";" + kvp.Value);
            }
            wrt.Close();
        }

        private void TextLogEvent(TrackedEvent ev)
        {
            this.Invoke((MethodInvoker)delegate
            {
                textBoxLogView.Text += "[" + ev.EventTime.ToString() + "] " + ev.EventType.ToString() + " - line: " + ev.LogLine + Environment.NewLine;
            });
        }

        private void TextLog(string sTxt)
        {
            this.Invoke((MethodInvoker)delegate
            {
                textBoxLogView.Text += "[" + DateTime.Now.ToString() + "] " + sTxt + Environment.NewLine;
            });
        }
                
        private void AddMapLvItem(TrackedMap map, bool bZana = false, string custom_duration = "")
        {
            this.Invoke((MethodInvoker)delegate
            {
                ListViewItem lvi = new ListViewItem(map.Started.ToString());
                string sName = map.Area;
                if (bZana)
                    sName += " (Zana)";
                lvi.SubItems.Add(map.Type);
                lvi.SubItems.Add(map.Area);
                lvi.SubItems.Add(custom_duration == "" ? map.StopWatchValue.ToString() : custom_duration);
                lvi.SubItems.Add(map.DeathCounter.ToString());

                listView1.Items.Insert(0, lvi);
            });
        }

        private string GetAreaNameFromEvent(TrackedEvent ev)
        {
            return ev.LogLine.Split(new string[] { "You have entered" }, StringSplitOptions.None)[1]
                .Replace(".", "").Trim();
        }

        private string GetDeathReasonromEvent(TrackedEvent ev)
        {
            try
            {
                return ev.LogLine.Split(new string[] { "killer=" }, StringSplitOptions.None)[1].Split(' ')[0];
            }
            catch
            {
                return "unknown";
            }
            
        }

        private void UpdateGUI()
        {
            TimeSpan tsAreaTime = (DateTime.Now - this.dtInAreaSince);

            if(bEventQInitialized)
            {
                loadScreen.Close();
                this.Invoke((MethodInvoker)delegate
                {
                    this.Show();

                    //Set statistic lv items
                    if(listView2.Items.Count == 0)
                    {
                        foreach(KeyValuePair<string,int> kvp in numStats)
                        {
                            ListViewItem lvi = new ListViewItem(GetStatLongName(kvp.Key));
                            lvi.SubItems.Add("0");
                            statLvItems.Add(kvp.Key, lvi);
                            listView2.Items.Add(lvi);
                        }
                    }
                    else
                    {
                        for(int i = 0; i < numStats.Count; i++)
                        {
                            KeyValuePair<string, int> kvp = numStats.ElementAt(i);
                            statLvItems[kvp.Key].SubItems[1].Text = kvp.Value.ToString();
                        }
                    }

                    if (!bHistoryInitialized)
                    {
                        ReadActivityLogFromSQLite();
                    }

                    
                    labelCurrArea.Text = sCurrentArea;
                    labelLastDeath.Text = dtLastDeath.Year > 2000 ? dtLastDeath.ToString() : "-";
                    labelLastDeathReason.Text = sLastDeathReason;
                    
                    if(sCurrentArea.Contains("Hideout"))
                    {
                        labelCurrActivity.Text = "In Hideout";
                    }
                    else
                    {
                        if (currentMap != null)
                        {
                            if (currentMap.Type == "Map")
                            {
                                labelCurrActivity.Text = "Mapping";
                            }
                            else
                            {
                                labelCurrActivity.Text = "In Heist";
                            }
                        }
                        else
                        {
                            labelCurrActivity.Text = "Nothing";
                        }
                    }


                    if (currentMap != null)
                    {
                        if (bIsMapZana && currentMap.ZanaMap != null)
                        {
                            labelStopWatch.Text = currentMap.ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = currentMap.ZanaMap.Area + " (Zana)";
                            labelTrackingDied.Text = currentMap.ZanaMap.DeathCounter.ToString();
                            
                            /*if(currentMap.ZanaMap.TrialMasterCount == 0)
                            {
                                labelUltimatum.Text = "Not started";
                            }
                            else
                            {
                                if(currentMap.ZanaMap.TrialMasterFullFinished)
                                {
                                    labelUltimatum.Text = "Completed, " + currentMap.ZanaMap.TrialMasterCount.ToString() + " rounds.";
                                }
                                else if(currentMap.ZanaMap.TrialMasterSuccess)
                                {
                                    labelUltimatum.Text = "Took rewards, " + currentMap.ZanaMap.TrialMasterCount.ToString() + " rounds.";
                                }
                                else
                                {
                                    labelUltimatum.Text = "Started, " + currentMap.ZanaMap.TrialMasterCount.ToString() + " rounds.";
                                }
                            }*/
                        }
                        else
                        {
                            labelStopWatch.Text = currentMap.StopWatchValue.ToString();
                            if (currentMap.Paused) labelStopWatch.Text += " (||)";
                            labelTrackingArea.Text = currentMap.Area;
                            labelTrackingType.Text = currentMap.Type;
                            labelTrackingDied.Text = currentMap.DeathCounter.ToString();

                            /*if (currentMap.TrialMasterCount == 0)
                            {
                                labelUltimatum.Text = "Not started";
                            }
                            else
                            {
                                if (currentMap.TrialMasterFullFinished)
                                {
                                    labelUltimatum.Text = "Completed, " + currentMap.TrialMasterCount.ToString() + " rounds.";
                                }
                                else if (currentMap.TrialMasterSuccess)
                                {
                                    labelUltimatum.Text = "Took rewards, " + currentMap.TrialMasterCount.ToString() + " rounds.";
                                }
                                else
                                {
                                    labelUltimatum.Text = "Started, " + currentMap.TrialMasterCount.ToString() + " rounds.";
                                }
                            }*/
                        }
                    }
                    else
                    {
                        labelTrackingDied.Text = "0";
                        labelTrackingArea.Text = "Not tracking";
                        labelStopWatch.Text = "00:00:00";
                    }

                    labelElderStatus.ForeColor = numStats["ElderKilled"] > 0 ? Color.Green : Color.Red;
                    labelElderStatus.Text = numStats["ElderKilled"] > 0 ? "killed" : "not yet killed";
                    labelElderKillCount.Text = numStats["ElderKilled"].ToString() + "x";

                    labelShaperStatus.ForeColor = numStats["ShaperKilled"] > 0 ? Color.Green : Color.Red;
                    labelShaperStatus.Text = numStats["ShaperKilled"] > 0 ? "killed" : "not yet killed";
                    labelShaperKillCount.Text = numStats["ShaperKilled"].ToString() + "x"; ;

                    labelSirusStatus.ForeColor = numStats["SirusKilled"] > 0 ? Color.Green : Color.Red;
                    labelSirusStatus.Text = numStats["SirusKilled"] > 0 ? "killed" : "not yet killed";
                    labelSirusKillCount.Text = numStats["SirusKilled"].ToString() + "x";
                    labelSirusTries.Text = numStats["SirusStarted"].ToString() + "x";

                    labelCataStatus.ForeColor = numStats["CatarinaKilled"] > 0 ? Color.Green : Color.Red;
                    labelCataStatus.Text = numStats["CatarinaKilled"] > 0 ? "killed" : "not yet killed";
                    labelCataKilled.Text = numStats["CatarinaKilled"].ToString() + "x";
                    labelCataTried.Text = numStats["CatarinaTried"].ToString() + "x";

                    labelVeritaniaStatus.ForeColor = numStats["VeritaniaKilled"] > 0 ? Color.Green : Color.Red;
                    labelVeritaniaKillCount.Text = numStats["VeritaniaKilled"].ToString() + "x";
                    labelVeritaniaStatus.Text = numStats["VeritaniaKilled"] > 0 ? "killed" : "not yet killed";
                    labelVeritaniaTries.Text = numStats["VeritaniaStarted"].ToString() + "x";

                    labelHunterStatus.ForeColor = numStats["HunterKilled"] > 0 ? Color.Green : Color.Red;
                    labelHunterStatus.Text = numStats["HunterKilled"] > 0 ? "killed" : "not yet killed";
                    labelHunterKillCount.Text = numStats["HunterKilled"].ToString() + "x";
                    labelHunterTries.Text = numStats["HunterStarted"].ToString() + "x";

                    labelDroxStatus.ForeColor = numStats["DroxKilled"] > 0 ? Color.Green : Color.Red;
                    labelDroxStatus.Text = numStats["DroxKilled"] > 0 ? "killed" : "not yet killed";
                    labelDroxKillCount.Text = numStats["DroxKilled"].ToString() + "x";
                    labelDroxTries.Text = numStats["DroxStarted"].ToString() + "x";

                    labelBaranStatus.ForeColor = numStats["BaranKilled"] > 0 ? Color.Green : Color.Red;
                    labelBaranStatus.Text = numStats["BaranKilled"] > 0 ? "killed" : "not yet killed";
                    labelBaranKillCount.Text = numStats["BaranKilled"].ToString() + "x";
                    labelBaranTries.Text = numStats["BaranStarted"].ToString() + "x";

                    labelTrialMasterStatus.ForeColor = numStats["TrialMasterKilled"] > 0 ? Color.Green : Color.Red;
                    labelTrialMasterStatus.Text = numStats["TrialMasterKilled"] > 0 ? "killed" : "not yet killed";
                    labelTrialMasterKilled.Text = numStats["TrialMasterKilled"].ToString() + "x";
                    labelTrialMasterTried.Text = numStats["TrialMasterStarted"].ToString() + "x";

                    labelMavenStatus.ForeColor = numStats["MavenKilled"] > 0 ? Color.Green : Color.Red;
                    labelMavenStatus.Text = numStats["MavenKilled"] > 0 ? "killed" : "not yet killed";
                    labelMavenKilled.Text = numStats["MavenKilled"].ToString() + "x";
                    labelMavenTried.Text = numStats["MavenStarted"].ToString() + "x";
                });
            }
        }

        private void ReadSettings()
        {
            this.sPoELogFilePath = ReadSetting("PoELogFilePath");
        }

        public string ReadSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key] ?? null;
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }

        private void Exit()
        {
            bExit = true;
            if (currentMap != null)
                FinishMap(currentMap, null, currentMap.Type, DateTime.Now);
            Application.Exit();
        }

        public void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void RefreshChart()
        {
            chart1.Series[0].Points.Clear();
            switch (comboBox1.SelectedItem.ToString())
            {
                case "Last week":
                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    FillChart(7);
                    break;
                case "Last 2 weeks":
                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    FillChart(14);
                    break;
                case "Last 3 weeks":
                    chart1.ChartAreas[0].AxisX.Interval = 2;
                    FillChart(21);
                    break;
                case "Last month":
                    chart1.ChartAreas[0].AxisX.Interval = 3;
                    FillChart(31);
                    break;
                case "Last 2 month":
                    chart1.ChartAreas[0].AxisX.Interval = 6;
                    FillChart(62);
                    break;
                case "Last 3 month":
                    chart1.ChartAreas[0].AxisX.Interval = 9;
                    FillChart(93);
                    break;
                case "Last year":
                    chart1.ChartAreas[0].AxisX.Interval = 30;
                    FillChart(365);
                    break;
                case "Last 2 years":
                    chart1.ChartAreas[0].AxisX.Interval = 60;
                    FillChart(365 * 2);
                    break;
                case "Last 3 years":
                    chart1.ChartAreas[0].AxisX.Interval = 90;
                    FillChart(365 * 3);
                    break;
                case "All time":
                    chart1.ChartAreas[0].AxisX.Interval = 90;
                    FillChart(365 * 15);
                    break;
            }
        }

        private void FillChart(int i_days_back)
        {
            chart1.Series[0].Points.Clear();
            DateTime dtStart = DateTime.Now.AddDays(i_days_back * -1);
            string sStatName = GetStatShortName(listView2.SelectedItems[0].Text);

            DateTime dt1, dt2;
            SqliteDataReader sqlReader;
            SqliteCommand cmd;
            long lTS1, lTS2;

            label38.Text = listView2.SelectedItems[0].Text;

            for (int i = 0; i <= i_days_back; i++)
            {
                dt1 = dtStart.AddDays(i);
                dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
                dt2 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 23, 59, 59);
                lTS1 = ((DateTimeOffset)dt1).ToUnixTimeSeconds();
                lTS2 = ((DateTimeOffset)dt2).ToUnixTimeSeconds();
                cmd = dbconn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM tx_stats WHERE stat_name = '" + sStatName + "' AND timestamp BETWEEN " + lTS1 + " AND " + lTS2;
                sqlReader = cmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    chart1.Series[0].Points.AddXY(dt1, sqlReader.GetInt32(0));
                }
            }
        }

        private void BuildmapList()
        {
            mapList = new List<string>
            {
                "Volcano",
                "Pen",
                "Arcade",
                "Jungle Valley",
                "Coves",
                "Peninsula",
                "Grotto",
                "Frozen Cabins",
                "Fields",
                "Wharf",
                "Underground Sea",
                "Crater",
                "Underground River",
                "Gardens",
                "Infested Valley",
                "Tropical Island",
                "Moon Temple",
                "Tower",
                "Arena",
                "Promenade",
                "Lair",
                "Spider Forest",
                "Defiled Cathedral",
                "Sunken City",
                "Graveyard",
                "Arid Lake",
                "Strand",
                "Glacier",
                "Canyon",
                "Forking River",
                "Sulphur Vents",
                "Toxic Sewer",
                "Ancient City",
                "Ashen Wood",
                "Cemetery",
                "Lava Chamber",
                "Laboratory",
                "Overgrown Ruin",
                "Vaal Pyramid",
                "Geode",
                "Courtyard",
                "Mud Geyser",
                "Shore",
                "Temple",
                "Belfry",
                "Pier",
                "Orchard",
                "Factory",
                "Primordial Blocks",
                "Plaza",
                "Basilica",
                "Reef",
                "Lookout",
                "Desert",
                "Marshes",
                "Iceberg",
                "Cage",
                "Leyline",
                "Courthouse",
                "Channel",
                "Academy",
                "Ramparts",
                "Dunes",
                "Bone Crypt",
                "Museum",
                "Wasteland",
                "Precinct",
                "Primordial Pool",
                "Crystal Ore",
                "Arsenal",
                "Crimson Temple",
                "Cells",
                "Chateau",
                "Lighthouse",
                "Haunted Mansion",
                "Atoll",
                "Armoury",
                "Waste Pool",
                "Shrine",
                "Desert Spring",
                "Palace",
                "Carcass",
                "The Beachhead",
                "Malformation",
                "Silo",
                "Waterways",
                "Dark Forest",
                "Alleyways",
                "Dry Sea",
                "Racecourse",
                "Dungeon",
                "Relic Chambers",
                "Spider Lair",
                "Mausoleum",
                "Mineral Pools",
                "Overgrown Shrine",
                "Stagnation",
                "Forbidden Woods",
                "Phantasmagoria",
                "Scriptorium",
                "Ghetto",
                "The Beachhead",
                "Flooded Mine",
                "Fungal Hollow",
                "Port",
                "Grave Trough",
                "Cold River",
                "Conservatory",
                "Ivory Temple",
                "Crimson Township",
                "Coral Ruins",
                "Siege",
                "Shipyard",
                "Dig",
                "Maze",
                "Plateau",
                "Cursed Crypt",
                "Park",
                "Beach",
                "Excavation",
                "City Square",
                "Barrows",
                "Thicket",
                "Residence",
                "Sepulchre",
                "Mesa",
                "Caldera",
                "Core",
                "Colosseum",
                "Acid Caverns",
                "Bramble Valley",
                "Foundry",
                "Bazaar",
                "Estuary",
                "Vault",
                "Arachnid Tomb",
                "Bog",
                "Colonnade",
                "Summit",
                "Castle Ruins",
                "Villa",
                "Terrace",
                "Lava Lake",
                "The Beachhead",
                "Burial Chambers",
                "Arachnid Nest",
                "Pit",
                "Necropolis",
                "Pit of the Chimera",
                "Lair of the Hydra",
                "Maze of the Minotaur",
                "Forge of the Phoenix",
                "Vaal Temple",
                "The Temple of Atzoatl"
            };

            heistList = new List<string>
            {
                "Bunker",
                "Smuggler's Den",
                "Laboratory",
                "Repository",
                "Tunnels",
                "Prohibited Library",
                "Underbelly",
                "Records Office",
                "Mansion"
            };

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (sPoELogFilePath == null)
            {
            }
            else
            {
                double dProgress = 0;
                if (!bEventQInitialized)
                {
                    this.Hide();
                    if (dLogLinesRead > 0)
                        dProgress = (dLogLinesRead / dLogLinesTotal) * 100;
                    loadScreen.progressBar.Value = Convert.ToInt32(dProgress);
                    loadScreen.progressLabel.Text = "Parsing logfile. This could take a while the first time.";
                    loadScreen.progressLabel2.Text = Convert.ToInt32(dProgress) + "%";
                }
                else
                {
                    UpdateGUI();
                    this.Opacity = 100;
                }
             
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxLogView.Text = "";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if(bEventQInitialized)
            {
                SaveStatsCache();
            }
        }

        private void MainW_FormClosing(object sender, FormClosingEventArgs e)
        {
            Exit();
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Exit();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About f1 = new About();
            f1.Show();
        }

        private void pictureBox14_Click_1(object sender, EventArgs e)
        {
            if (currentMap != null)
                currentMap.Resume();
        }

        private void pictureBox13_Click_1(object sender, EventArgs e)
        {
            if (currentMap != null)
                currentMap.Pause();
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
                RefreshChart();
        }
       

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Focus();
            if(listView2.SelectedItems.Count > 0)
                RefreshChart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
                RefreshChart();
        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {
            if (currentMap != null && currentMap.Paused)
                currentMap.Resume();
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            if (currentMap != null && !currentMap.Paused)
                currentMap.Pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 1)
            {
                int iIndex = listView1.SelectedIndices[0];
                long lTimestamp = mapHistory[iIndex].TimeStamp;
                string sType = listView1.Items[iIndex].SubItems[1].Text;
                string sArea = listView1.Items[iIndex].SubItems[2].Text;

                if (MessageBox.Show("Do you really want to delete this Activity? " + Environment.NewLine
                    + Environment.NewLine
                    + "Type: " + sType + Environment.NewLine
                    + "Area: " + sArea + Environment.NewLine
                    + "Time: " + listView1.Items[iIndex].SubItems[0].Text, "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    listView1.Items.Remove(listView1.SelectedItems[0]);
                    mapHistory.RemoveAt(iIndex);
                    DeleteActLogEntry(lTimestamp);
                }
            }
        }

#pragma warning disable IDE1006 // Benennungsstile
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
        {
            Settings stt = new Settings(this);
            stt.ShowDialog();
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            if (currentMap != null)
                FinishMap(currentMap, null, "Map", DateTime.Now);
        }

        private void pictureBox15_Click_1(object sender, EventArgs e)
        {
            if (currentMap != null)
            {
                FinishMap(currentMap, null, "Map", DateTime.Now);
            }
        }
       
    }
}