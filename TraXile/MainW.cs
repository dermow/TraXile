using log4net;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TraXile
{
    public enum ACTIVITY_TYPES
    {
        MAP,
        HEIST,
        LABYRINTH,
        SIMULACRUM,
        BLIGHTED_MAP,
        DELVE
    }

    public partial class MainW : Form
    {
        private string sPoELogFilePath;
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
        private TrackedActivity currentActivity;
        private bool bEventQInitialized;
        private bool bIsMapZana;
        private bool bExit;
        private bool bElderFightActive;
        private bool bSettingActivityLogShowGrid;
        private bool bRestoreMOde;
        private int iShaperKillsInFight;
        private int iNextAreaLevel;
        private int iCurrentAreaLevel;
        private SqliteConnection dbconn;
        private bool bHistoryInitialized;
        List<string> knownPlayers;
        BindingList<string> backups;
        Dictionary<int, string> dict;
        Dictionary<string, int> numStats;
        Dictionary<string, string> statNamesLong;
        Dictionary<string, ListViewItem> statLvItems;
        LoadScreen loadScreen;
        List<TrackedActivity> eventHistory;
        ConcurrentQueue<TrackedEvent> eventQueue;
        public List<ActivityTag> tags;
        Dictionary<string, Label> tagLabels, tagLabelsConfig;
        EventMapping evMap;
        AreaMapping areaMap;
        ILog log;
        private bool bSettingStatsShowGrid;
        private string sLastSimuEndpoint;

        private ListViewManager lvmStats, lvmActLog;
        private bool bRestoreOk = true;
        private string sFailedRestoreReason = "";

        public MainW()
        {
            this.Visible = false;
            InitializeComponent();
            Init();
        }

        private void CheckForUpdate(bool b_notify_ok = false)
        {
            try
            {
                const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "Unity web player");
                Uri uri = new Uri(string.Format(GITHUB_API, "dermow", "TraXile"));
                string releases = webClient.DownloadString(uri);
                int iIndex = releases.IndexOf("tag_name");
                string sVersion =  releases.Substring(iIndex + 11, 5);

                // Check if Updater needs an update
                if(File.Exists("TraXile.Updater.exe.Update"))
                {
                    try
                    {
                        File.Delete("Traxile.Updater.exe");
                        File.Move("TraXile.Updater.exe.Update", "TraXile.Updater.exe");
                        log.Info("TraXile.Updater updated successfully.");
                    }
                    catch(Exception ex)
                    {
                        log.Error("Error updating TraXile.Updater: " + ex.Message);
                    }
                    
                }

                if(Convert.ToInt32(sVersion.Replace(".", "")) > Convert.ToInt32(APPINFO.VERSION.Replace(".", "")))
                {
                    if(MessageBox.Show("There is a new version available for TraXile (current=" + APPINFO.VERSION + ", new=" + sVersion + ")"
                        + Environment.NewLine + Environment.NewLine + "Start Update now?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if(File.Exists("TraXile.Updater.exe"))
                        {
                            Process.Start("TraXile.Updater.exe");
                            Application.Exit();
                        }
                    }
                }
                else
                {
                    if(b_notify_ok)
                        MessageBox.Show("Your version: " + APPINFO.VERSION 
                            + Environment.NewLine + "Latest version: " + sVersion + Environment.NewLine + Environment.NewLine 
                            + "Your version is already up to date :)");
                }
            }
            catch(Exception ex)
            {
                log.Error("Could not check for Update: " + ex.Message);
            }
        }

        private void Init()
        {
            this.Opacity = 0;

            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Application started");

            lvmStats = new ListViewManager(listViewStats);
            lvmActLog = new ListViewManager(listViewActLog);

            evMap = new EventMapping();
            areaMap = new AreaMapping();

            SaveVersion();
            CheckForUpdate();
            ReadSettings();
            
            try
            {
                DoBackupRestoreIfPrepared();        
            }
            catch(Exception ex)
            {
                bRestoreMOde = true;
                bRestoreOk = false;
                sFailedRestoreReason = ex.Message;
                log.Error("FailedRestore -> " + ex.Message);
                log.Debug(ex.ToString());
            }

            listViewActLog.Columns[0].Width = 120;
            listViewActLog.Columns[1].Width = 50;
            listViewActLog.Columns[2].Width = 110;
            listViewActLog.Columns[3].Width = 100;
            listViewActLog.Columns[4].Width = 50;
            listViewStats.Columns[0].Width = 500;
            listViewStats.Columns[1].Width = 300;

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

            textBox1.Text = ReadSetting("PoELogFilePath");
            textBox1.Enabled = false;

            comboBox1.SelectedIndex = 1;

            dict = new Dictionary<int, string>();
            eventQueue = new ConcurrentQueue<TrackedEvent>();
            eventHistory = new List<TrackedActivity>();
            knownPlayers = new List<string>();
            backups = new BindingList<string>();
            statLvItems = new Dictionary<string, ListViewItem>();
            sCurrentArea = "-";
            dtInAreaSince = DateTime.Now;
            bEventQInitialized = false;
            tagLabels = new Dictionary<string, Label>();
            tagLabelsConfig = new Dictionary<string, Label>();
            sLastSimuEndpoint = "";
            tags = new List<ActivityTag>();

           

            this.Text = APPINFO.NAME + " " + APPINFO.VERSION;
            dtInitStart = DateTime.Now;

           

            if(sPoELogFilePath == null)
            {
                FileSelectScreen fs = new FileSelectScreen(this);
                fs.ShowDialog();
            }

            loadScreen = new LoadScreen();
            loadScreen.Show(this);

            InitDatabase();
            
            lastEvType = EVENT_TYPES.APP_STARTED;
            InitNumStats();

            foreach (KeyValuePair<string, int> kvp in numStats)
            {
                ListViewItem lvi = new ListViewItem(GetStatLongName(kvp.Key));
                lvi.SubItems.Add("0");
                lvmStats.AddLvItem(lvi, "stats_" + kvp.Key);
            }

            eventQueue.Enqueue(new TrackedEvent(EVENT_TYPES.APP_STARTED) { EventTime = DateTime.Now, LogLine = "Application started." });

            ReadStatsCache();
            ReadKnownPlayers();
            LoadCustomTags();
            ResetMapHistory();
            LoadLayout();

            // Thread for Log Parsing and Enqueuing
            thParseLog = new Thread(new ThreadStart(LogParsing))
            {
                IsBackground = true
            };
            thParseLog.Start();

            // Thread for Queue processing / Dequeuing
            thEvents = new Thread(new ThreadStart(EventHandling))
            {
                IsBackground = true
            };
            thEvents.Start();
        }

        private void SaveLayout()
        {
            foreach(ColumnHeader ch in listViewActLog.Columns)
            {
                if(ch.Width > 0)
                {
                    AddUpdateAppSettings("layout.listview.cols." + ch.Name + ".width", ch.Width.ToString());
                }
            }
            if(this.Width > 50 && this.Height > 50)
            {
                AddUpdateAppSettings("layout.window.width", this.Width.ToString());
                AddUpdateAppSettings("layout.window.height", this.Height.ToString());
            }
        }

        private void LoadLayout()
        {
            foreach (ColumnHeader ch in listViewActLog.Columns)
            {
                int w = Convert.ToInt32(ReadSetting("layout.listview.cols." + ch.Name + ".width"));
                if(w > 0)
                {
                    ch.Width = w;
                }
            }

            int iWidth = Convert.ToInt32(ReadSetting("layout.window.width"));
            int iHeight = Convert.ToInt32(ReadSetting("layout.window.height"));

            if(iWidth > 50 && iHeight > 50)
            {
                this.Width = iWidth;
                this.Height = iHeight;
            }
            
        }

        private void InitDefaultTags()
        {
            List<ActivityTag> tmpTags;
            tmpTags = new List<ActivityTag>();
            tmpTags.Add(new ActivityTag("blight") { BackColor = Color.LightGreen, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("delirium") { BackColor = Color.WhiteSmoke, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("einhar") { BackColor = Color.Red, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("incursion") { BackColor = Color.Turquoise, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("syndicate") { BackColor = Color.Gold, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("zana") { BackColor = Color.Blue, ForeColor = Color.White });
            tmpTags.Add(new ActivityTag("niko") { BackColor = Color.OrangeRed, ForeColor = Color.Black });
            tmpTags.Add(new ActivityTag("zana-map") { BackColor = Color.OrangeRed, ForeColor = Color.Black });

            foreach (ActivityTag tag in tmpTags)
            {
                try
                {
                    SqliteCommand cmd = dbconn.CreateCommand();
                    cmd.CommandText = "insert into tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type) values " +
                                  "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'default')";
                    cmd.ExecuteNonQuery();
                    log.Info("Default tag '" + tag.ID + "' added to database");
                }
                catch(SqliteException e)
                {
                    if(e.Message.Contains("SQLite Error 19"))
                    {
                        log.Info("Default tag '" + tag.ID + "' already in database, nothing todo");
                    }
                    else
                    {
                        log.Error(e.ToString());
                    }
                }
               
            }
        }

        private void LoadCustomTags()
        {
            SqliteDataReader sqlReader;
            SqliteCommand cmd;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tx_tags ORDER BY tag_id DESC";
            sqlReader = cmd.ExecuteReader();

            while (sqlReader.Read())
            {
                string sID = sqlReader.GetString(0);
                string sType = sqlReader.GetString(4);
                ActivityTag tag = new ActivityTag(sID, sType == "custom" ? false : true);
                tag.DisplayName = sqlReader.GetString(1);
                tag.BackColor = Color.FromArgb(Convert.ToInt32(sqlReader.GetString(2)));
                tag.ForeColor = Color.FromArgb(Convert.ToInt32(sqlReader.GetString(3)));
                tags.Add(tag);
            }
        }

        private void RenderTagsForConfig(bool b_reinit = false)
        {
            if (b_reinit)
            {
                groupBox3.Controls.Clear();
                tagLabelsConfig.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 23;

            int iX = iOffsetX;
            int iY = ioffsetY;
            int iLabelWidth = 100;
            int iMaxCols = 5;

            int iCols = (groupBox3.Width-20) / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < tags.Count; i++)
            {
                ActivityTag tag = tags[i];
                Label lbl = new Label();
                lbl.Width = iLabelWidth;

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if (!tagLabelsConfig.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = tag.BackColor;
                    lbl.ForeColor = tag.ForeColor;
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick1;
                    lbl.Location = new Point(iX, iY);

                    groupBox3.Controls.Add(lbl);
                    tagLabelsConfig.Add(tag.ID, lbl);
                }

                iX += lbl.Width + 5;
                iCurrCols++;
            }
        }

        private void Lbl_MouseClick1(object sender, MouseEventArgs e)
        {
            ActivityTag tag = GetTagByDisplayName(((Label)sender).Text);
            textBox4.Text = tag.ID;
            textBox5.Text = tag.DisplayName;
            label63.ForeColor = tag.ForeColor;
            label63.BackColor = tag.BackColor;
            label63.Text = tag.DisplayName;
        }

        private void tagLabel_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.None;
        }

        private void tagLabel_MouseOver(object sender, EventArgs e)
        {
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
        }

        private void RenderTagsForTracking(bool b_reinit = false)
        {
            if (b_reinit)
            {
                groupBox8.Controls.Clear();
                tagLabels.Clear();
            }

            int iOffsetX = 10;
            int ioffsetY = 20;
            int iLabelWidth = 100;
            int iMaxCols = 5;

            int iX = iOffsetX;
            int iY = ioffsetY;

            int iCols = groupBox8.Width / iLabelWidth;
            if (iCols > iMaxCols) iCols = iMaxCols;
            int iCurrCols = 0;

            for (int i = 0; i < tags.Count; i++)
            {
                ActivityTag tag = tags[i];
                Label lbl = new Label();
                lbl.Width = iLabelWidth;

                if (iCurrCols > (iCols - 1))
                {
                    iY += 28;
                    iX = iOffsetX;
                    iCurrCols = 0;
                }

                if(!tagLabels.ContainsKey(tag.ID))
                {
                    lbl.Text = tag.DisplayName;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = Color.Gray;
                    lbl.ForeColor = Color.LightGray;
                    lbl.Location = new Point(iX, iY);
                    lbl.MouseHover += tagLabel_MouseOver;
                    lbl.MouseLeave += tagLabel_MouseLeave;
                    lbl.MouseClick += Lbl_MouseClick;
                    
                    groupBox8.Controls.Add(lbl);
                    tagLabels.Add(tag.ID, lbl);
                }
                else
                {
                    if(currentActivity != null)
                    {
                        TrackedActivity mapToCheck = bIsMapZana ? currentActivity.ZanaMap : currentActivity;

                        if(mapToCheck.Tags.Contains(tag.ID))
                        {
                            tagLabels[tag.ID].BackColor = tag.BackColor;
                            tagLabels[tag.ID].ForeColor = tag.ForeColor;
                        }
                        else
                        {
                            tagLabels[tag.ID].BackColor = Color.Gray;
                            tagLabels[tag.ID].ForeColor = Color.LightGray;
                        }
                    }
                    else
                    {
                        tagLabels[tag.ID].BackColor = Color.Gray;
                        tagLabels[tag.ID].ForeColor = Color.LightGray;
                    }
                }

                iX += lbl.Width + 5;
                iCurrCols++;
            }
        }

        public ActivityTag GetTagByDisplayName(string s_display_name)
        {
            foreach(ActivityTag t in tags)
            {
                if (t.DisplayName == s_display_name)
                    return t;
            }

            return null;
        }

        private void Lbl_MouseClick(object sender, MouseEventArgs e)
        {
            ActivityTag tag = GetTagByDisplayName(((Label)sender).Text);
            if(!tag.IsDefault)
            {
                if(currentActivity != null)
                {
                    if(bIsMapZana && currentActivity.ZanaMap != null)
                    {
                        if (currentActivity.ZanaMap.HasTag(tag.ID))
                        {
                            currentActivity.ZanaMap.RemoveTag(tag.ID);
                        }
                        else
                        {
                            currentActivity.ZanaMap.AddTag(tag.ID);
                        }
                    }
                    else
                    {
                        if(currentActivity.HasTag(tag.ID))
                        {
                            currentActivity.RemoveTag(tag.ID);
                        }
                        else
                        {
                            currentActivity.AddTag(tag.ID);
                        }
                        
                    }
                }
            }
        }

        private void Lbl_MouseHover(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
                { "AreaChanges", 0 },
                { "BaranStarted", 0 },
                { "BaranKilled", 0 },
                { "CatarinaTried", 0 },
                { "CatarinaKilled", 0 },
                { "TotalKilledCount", 0 },
                { "DroxStarted", 0 },
                { "DroxKilled", 0 },
                { "EinharCaptures", 0 },
                { "ElderTried", 0 },
                { "ElderKilled", 0 },
                { "HighestLevel", 0 },
                { "HunterKilled", 0 },
                { "HunterStarted", 0 },
                { "LabsFinished", 0 },
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
                { "TrialMasterStarted", 0 },
                { "TrialMasterKilled", 0 },
                { "TrialMasterTookReward", 0 },
                { "TrialMasterVictory", 0 },
                { "TrialMasterSuccess", 0 },
                { "VeritaniaKilled", 0 },
                { "VeritaniaStarted", 0 },
            };

            statNamesLong = new Dictionary<string, string>
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
            };

            List<string> labs = new List<string>();
            labs.Add("Unknown");
            labs.Add("The Merciless Labyrinth");
            labs.Add("The Cruel Labyrinth");
            labs.Add("The Labyrinth");
            labs.Add("Uber-Lab");
            labs.Add("Advanced Uber-Lab");

            foreach (string s in areaMap.HEIST_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("HeistsFinished_" + sName))
                    numStats.Add("HeistsFinished_" + sName, 0);
                if (!statNamesLong.ContainsKey("HeistsFinished_" + sName))
                    statNamesLong.Add("HeistsFinished_" + sName, "Heists done: " + sName);
            }

            foreach (string s in labs)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("LabsCompleted_" + sName))
                    numStats.Add("LabsCompleted_" + sName, 0);
                if (!statNamesLong.ContainsKey("LabsCompleted_" + sName))
                    statNamesLong.Add("LabsCompleted_" + sName, "Labs completed: " + sName);
            }

            foreach (string s in areaMap.MAP_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("MapsFinished_" + sName))
                    numStats.Add("MapsFinished_" + sName, 0);
                if (!statNamesLong.ContainsKey("MapsFinished_" + sName))
                    statNamesLong.Add("MapsFinished_" + sName, "Maps done: " + sName);
            }
            foreach (string s in areaMap.SIMU_AREAS)
            {
                string sName = s.Replace("'", "");
                if (!numStats.ContainsKey("SimulacrumFinished_" + sName))
                    numStats.Add("SimulacrumFinished_" + sName, 0);
                if (!statNamesLong.ContainsKey("SimulacrumFinished_" + sName))
                    statNamesLong.Add("SimulacrumFinished_" + sName, "Simulacrum done: " + sName);
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

            log.Info("Stats cleared.");
        }

        private void ClearActivityLog()
        {
            SqliteCommand cmd;

            eventHistory.Clear();
            listViewActLog.Items.Clear();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "drop table tx_activity_log";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_activity_log " +
                 "(timestamp int, " +
                 "act_type text, " +
                 "act_area text, " +
                 "act_stopwatch int, " +
                 "act_deathcounter int," +
                 "act_ulti_rounds int," +
                 "act_is_zana int," +
                 "act_tags" + ")";
            cmd.ExecuteNonQuery();

            InitNumStats();
            SaveStatsCache();

            log.Info("Activity log cleared.");
        }

        private void ReadBackupList()
        {
            if(Directory.Exists("backups"))
            {
                foreach(string s in Directory.GetDirectories("backups"))
                {
                    foreach(string s2 in Directory.GetDirectories(s))
                    {
                        if(!backups.Contains(s2))
                            backups.Add(s2);
                    }
                }
            }
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
                "act_is_zana int," + 
                "act_tags" + ")";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create table if not exists tx_tags " +
                "(tag_id text, " +
                "tag_display text," +
                "tag_bgcolor text, " +
                "tag_forecolor text," +
                "tag_type text)";
            cmd.ExecuteNonQuery();

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "create unique index if not exists tx_tag_id on tx_tags(tag_id)";
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
            InitDefaultTags();

            PatchDatabase();
            log.Info("Database initialized.");
        }

        private void PatchDatabase()
        {
            SqliteCommand cmd;
            // Update 0.3.4
            try
            {
                cmd = dbconn.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_tags text";
                cmd.ExecuteNonQuery();
                log.Info("PatchDatabase 0.3.4 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.4.5
            try
            {
                cmd = dbconn.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_area_level int";
                cmd.ExecuteNonQuery();
                log.Info("PatchDatabase 0.4.5 -> " + cmd.CommandText);
            }
            catch
            {
            }
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
                log.Info("KnownPlayerAdded -> name: " + s_name);
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

        private void SaveToActivityLog(long i_ts, string s_type, string s_area, int i_area_level, int i_stopwatch, int i_death_counter, int i_ulti_rounds, bool b_zana, List<string> l_tags)
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

            SqliteCommand cmd;
            cmd = dbconn.CreateCommand();
            cmd.CommandText = "insert into tx_activity_log " +
               "(timestamp, " +
               "act_type, " +
               "act_area, " +
               "act_area_level, " +
               "act_stopwatch, " +
               "act_deathcounter, " +
               "act_ulti_rounds," +
               "act_is_zana," +
               "act_tags) VALUES (" +
               i_ts.ToString() 
                 + ", '" + s_type 
                 + "', '" + s_area
                 + "', '" + i_area_level.ToString()
                 + "', " + i_stopwatch 
                 + ", " + i_death_counter 
                 + ", " + i_ulti_rounds 
                 + ", " + (b_zana ? "1" : "0")
                 + ", '" + sTags + "')";

            cmd.ExecuteNonQuery();
        }

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
            }
            return ACTIVITY_TYPES.MAP;
        }

        private void ReadActivityLogFromSQLite()
        {
            SqliteDataReader sqlReader;
            SqliteCommand cmd;
            string[] arrTags;

            cmd = dbconn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tx_activity_log ORDER BY timestamp DESC";
            sqlReader = cmd.ExecuteReader();

            while(sqlReader.Read())
            {
                TimeSpan ts = TimeSpan.FromSeconds(sqlReader.GetInt32(3));
                string sType = sqlReader.GetString(1);
                ACTIVITY_TYPES aType = GetActTypeFromString(sType);
               

                TrackedActivity map = new TrackedActivity
                {
                    Started = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(sqlReader.GetInt32(0)).ToLocalTime(),
                    TimeStamp = sqlReader.GetInt32(0),
                    CustomStopWatchValue = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds),
                    Type = aType,
                    Area = sqlReader.GetString(2),
                    DeathCounter = sqlReader.GetInt32(4),
                    TrialMasterCount = sqlReader.GetInt32(5),
                    AreaLevel = sqlReader.GetInt32(8)
                };

                try
                {
                    string sTags = sqlReader.GetString(7);
                    arrTags = sTags.Split('|');
                }
                catch
                {
                    arrTags = new string[0];
                }

                for(int i = 0; i < arrTags.Length; i++)
                {
                    map.AddTag(arrTags[i]);
                }

                //mapHistory
                eventHistory.Add(map);
            }
            bHistoryInitialized = true;
            ResetMapHistory();
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
            log.Info("Started logfile parsing. Last hash was " + iLastHash.ToString());

            dLogLinesTotal = Convert.ToDouble(GetLogFileLineCount());

            var fs = new FileStream(sPoELogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bool bNewContent = iLastHash == 0;

            using (StreamReader reader = new StreamReader(fs))
            {
                string line;
                int lineHash = 0;
               

                // Keep file open
                while (!bExit)
                {
                    line = reader.ReadLine();
                    
                    if (line == null)
                    {
                        if (!bEventQInitialized)
                        {
                            currentActivity = null;
                            bIsMapZana = false;
                            dtInitEnd = DateTime.Now;
                            TimeSpan tsInitDuration = (dtInitEnd - dtInitStart);
                            eventQueue.Enqueue(new TrackedEvent(EVENT_TYPES.APP_READY) 
                            { 
                                EventTime = DateTime.Now, 
                                LogLine = "Application initialized in " 
                                  + Math.Round(tsInitDuration.TotalSeconds, 2) + " seconds." 
                            });
                            iLastHash = lineHash;
                        }
                        bEventQInitialized = true;

                        Thread.Sleep(100);
                        continue;
                    }

                    lineHash = line.GetHashCode();

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

                    foreach (KeyValuePair<string,EVENT_TYPES> kv in evMap.MAP)
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
                                    ev.EventTime = DateTime.Parse(line.Split(' ')[0] + " " + line.Split(' ')[1]);
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

        private void EventHandling()
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

            foreach (string s in areaMap.MAP_AREAS)
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

            foreach (string s in areaMap.HEIST_AREAS)
            {
                if (s.Trim().Equals(sArea.Trim()))
                    return true;
            }
            return false;
        }

        private void HandleChatCommand(string s_command)
        {
            log.Info("ChatCommand -> " + s_command);
            string[] spl = s_command.Split(' ');
            string sMain = "";
            string sArgs = "";

            sMain = spl[0];

            if(spl.Length > 1)
            {
                sArgs = spl[1];
            }

            TrackedActivity currentAct = null;
            if (currentActivity != null)
            {
                if (bIsMapZana && currentActivity.ZanaMap != null)
                {
                    currentAct = currentActivity.ZanaMap;
                }
                else
                {
                    currentAct = currentActivity;
                }
            }

            switch (sMain)
            {
                case "tag":
                    if(currentAct != null)
                    {
                        MethodInvoker mi = delegate
                        {
                            AddTagAutoCreate(sArgs, currentAct);
                        };
                        this.Invoke(mi);
                    }
                    break;
                case "untag":
                    if (currentAct != null)
                    {
                        MethodInvoker mi = delegate
                        {
                            RemoveTagFromActivity(sArgs, currentAct);
                        };
                        this.Invoke(mi);

                    }
                    break;
                case "pause":
                    if (currentActivity != null)
                    {
                        if (bIsMapZana && currentActivity.ZanaMap != null)
                        {
                            if (!currentActivity.ZanaMap.Paused)
                            {
                                currentActivity.ZanaMap.Pause();
                            }
                        }
                        else
                        {
                            if (!currentActivity.Paused)
                            {
                                currentActivity.Pause();
                            }
                        }
                    }
                    break;
                case "resume":
                    if (currentActivity != null)
                    {
                        if (bIsMapZana && currentActivity.ZanaMap != null)
                        {
                            if (currentActivity.ZanaMap.Paused)
                            {
                                currentActivity.ZanaMap.Resume();
                            }
                        }
                        else
                        {
                            if (currentActivity.Paused)
                            {
                                currentActivity.Resume();
                            }
                        }
                    }
                    break;
                case "finish":
                    if (currentAct != null && !bIsMapZana)
                    {
                        MethodInvoker mi = delegate
                        {
                            FinishMap(currentActivity, null, ACTIVITY_TYPES.MAP, DateTime.Now);
                        };
                        this.Invoke(mi);
                    }
                    break;
            }
        }

        private void HandleSingleEvent(TrackedEvent ev, bool bInit = false)
        {
            try
            {
                switch (ev.EventType)
                {
                    case EVENT_TYPES.CHAT_CMD_RECEIVED:
                        string sCmd = ev.LogLine.Split(new string[] { "::" }, StringSplitOptions.None)[1];

                        if (bEventQInitialized)
                        {
                            HandleChatCommand(sCmd);
                        }
                        break;

                    case EVENT_TYPES.ENTERED_AREA:

                        string sSourceArea = sCurrentArea;
                        string sTargetArea = GetAreaNameFromEvent(ev);
                        string sAreaName = GetAreaNameFromEvent(ev);
                        bool bSourceAreaIsMap = CheckIfAreaIsMap(sSourceArea);
                        bool bTargetAreaIsMap = CheckIfAreaIsMap(sTargetArea, sSourceArea);
                        bool bTargetAreaIsHeist = CheckIfAreaIsHeist(sTargetArea, sSourceArea);
                        bool bTargetAreaIsSimu = false;
                        bool bTargetAreaMine = sTargetArea == "Azurite Mine";
                        bool bTargetAreaIsLab = sTargetArea == "Estate Path" || sTargetArea == "Estate Walkways" || sTargetArea == "Estate Crossing";
                        long lTS = ((DateTimeOffset)ev.EventTime).ToUnixTimeSeconds();

                        dtInAreaSince = ev.EventTime;

                        IncrementStat("AreaChanges", ev.EventTime, 1);

                        //Simu?
                        if (areaMap.SIMU_AREAS.Contains(sAreaName))
                        {
                            bTargetAreaIsSimu = true;
                            if (sCurrentInstanceEndpoint != sLastSimuEndpoint)
                            {
                                IncrementStat("SimulacrumStarted", ev.EventTime, 1);
                                sLastSimuEndpoint = sCurrentInstanceEndpoint;

                                currentActivity = new TrackedActivity
                                {
                                    Area = sTargetArea,
                                    Type = ACTIVITY_TYPES.SIMULACRUM,
                                    AreaLevel = iNextAreaLevel,
                                    Started = ev.EventTime,
                                    TimeStamp = lTS,
                                    InstanceEndpoint = sCurrentInstanceEndpoint
                                };
                                iNextAreaLevel = 0;
                            }
                        }

                        // Special calculation for Elder fight - he has no start dialoge.
                        if (sAreaName == "Absence of Value and Meaning".Trim())
                        {
                            if (!bElderFightActive)
                            {
                                IncrementStat("ElderTried", ev.EventTime, 1);
                            }
                            bElderFightActive = true;
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

                        //Lab started?
                        if (actType == ACTIVITY_TYPES.LABYRINTH && sSourceArea == "Aspirants Plaza")
                        {
                            string sLabName = "Labyrinth";

                            switch (iNextAreaLevel)
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

                            currentActivity = new TrackedActivity
                            {
                                Area = sLabName,
                                AreaLevel = iNextAreaLevel,
                                Type = actType,
                                //Started = DateTime.Parse(ev.LogLine.Split(' ')[0] + " " + ev.LogLine.Split(' ')[1]),
                                Started = ev.EventTime,
                                TimeStamp = lTS,
                                InstanceEndpoint = sCurrentInstanceEndpoint
                            };
                           
                        }

                        //Lab cancelled?
                        if (currentActivity != null && currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
                        {
                            if (sTargetArea.Contains("Hideout") || areaMap.CAMP_AREAS.Contains(sTargetArea))
                            {
                                FinishMap(currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                            }
                        }

                        // Delving?
                        if ((currentActivity == null || currentActivity.Type != ACTIVITY_TYPES.DELVE) && actType == ACTIVITY_TYPES.DELVE)
                        {
                            currentActivity = new TrackedActivity
                            {
                                Area = "Azurite Mine",
                                Type = actType,
                                Started = ev.EventTime,
                                TimeStamp = lTS,
                                InstanceEndpoint = sCurrentInstanceEndpoint
                            };
                        }

                        // Update Delve level
                        if(currentActivity != null && currentActivity.Type == ACTIVITY_TYPES.DELVE && actType == ACTIVITY_TYPES.DELVE)
                        {
                            if(iNextAreaLevel > currentActivity.AreaLevel)
                            {
                                currentActivity.AreaLevel = iNextAreaLevel;
                            }
                        }

                        // End Delving?
                        if (currentActivity != null && currentActivity.Type == ACTIVITY_TYPES.DELVE && !bTargetAreaMine)
                        {
                            FinishMap(currentActivity, null, ACTIVITY_TYPES.DELVE, DateTime.Now);
                        }

                        if (bTargetAreaIsMap || bTargetAreaIsHeist || bTargetAreaIsSimu || bTargetAreaIsLab || bTargetAreaMine)
                        {
                            bElderFightActive = false;
                            iShaperKillsInFight = 0;

                            if (currentActivity == null)
                            {
                                currentActivity = new TrackedActivity
                                {
                                    Area = sTargetArea,
                                    Type = actType,
                                    AreaLevel = iNextAreaLevel,
                                    Started = ev.EventTime,
                                    TimeStamp = lTS,
                                    InstanceEndpoint = sCurrentInstanceEndpoint
                                };
                                iNextAreaLevel = 0;
                            }
                            if (!currentActivity.Paused)
                                currentActivity.StartStopWatch();

                            if (bSourceAreaIsMap && bTargetAreaIsMap)
                            {
                                if (!bIsMapZana)
                                {
                                    // entered Zana Map
                                    bIsMapZana = true;
                                    currentActivity.StopStopWatch();
                                    if (currentActivity.ZanaMap == null)
                                    {
                                        currentActivity.ZanaMap = new TrackedActivity
                                        {
                                            Type = ACTIVITY_TYPES.MAP,
                                            Area = sTargetArea,
                                            AreaLevel = iNextAreaLevel,
                                            Started = ev.EventTime,
                                            TimeStamp = lTS,
                                        };
                                        currentActivity.ZanaMap.AddTag("zana-map");
                                        iNextAreaLevel = 0;
                                    }
                                    if (!currentActivity.ZanaMap.Paused)
                                        currentActivity.ZanaMap.StartStopWatch();
                                }
                                else
                                {
                                    // leave Zana Map
                                    if (currentActivity.ZanaMap != null)
                                    {
                                        bIsMapZana = false;
                                        currentActivity.ZanaMap.StopStopWatch();
                                        if (!currentActivity.Paused)
                                            currentActivity.StartStopWatch();
                                    }
                                }
                            }
                            else
                            {
                                // Do not track Lab-Trials
                                if ((!sSourceArea.Contains("Trial of")) && (currentActivity.Type != ACTIVITY_TYPES.LABYRINTH) && (currentActivity.Type != ACTIVITY_TYPES.DELVE))
                                {
                                    if (sTargetArea != currentActivity.Area || sCurrentInstanceEndpoint != currentActivity.InstanceEndpoint)
                                    {
                                        FinishMap(currentActivity, sTargetArea, actType, DateTime.Parse(ev.LogLine.Split(' ')[0] + " " + ev.LogLine.Split(' ')[1]));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (currentActivity != null && currentActivity.Type != ACTIVITY_TYPES.LABYRINTH)
                                currentActivity.StopStopWatch();
                        }

                        sCurrentArea = sAreaName;
                        break;
                    case EVENT_TYPES.PLAYER_DIED:
                        string sPlayerName = ev.LogLine.Split(' ')[8];
                        if (!knownPlayers.Contains(sPlayerName))
                        {
                            IncrementStat("TotalKilledCount", ev.EventTime, 1);
                            dtLastDeath = DateTime.Now;
                            if (currentActivity != null)
                            {
                                if (bIsMapZana)
                                {
                                    if (currentActivity.ZanaMap != null)
                                    {
                                        currentActivity.ZanaMap.DeathCounter++;
                                    }

                                }
                                else
                                {
                                    currentActivity.DeathCounter++;
                                }
                            }

                            // Lab?
                            if (currentActivity != null && currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
                            {
                                FinishMap(currentActivity, null, ACTIVITY_TYPES.LABYRINTH, DateTime.Now);
                            }
                        }

                        break;
                    case EVENT_TYPES.ELDER_KILLED:
                        IncrementStat("ElderKilled", ev.EventTime, 1);
                        bElderFightActive = false;
                        break;
                    case EVENT_TYPES.SHAPER_KILLED:
                        // shaper has 3x the same kill dialogue
                        iShaperKillsInFight++;
                        if (iShaperKillsInFight == 3)
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
                        if (currentActivity != null)
                        {
                            currentActivity.TrialMasterSuccess = true;
                            currentActivity.TrialMasterFullFinished = true;
                        }
                        break;
                    case EVENT_TYPES.TRIALMASTER_TOOK_REWARD:
                        IncrementStat("TrialMasterTookReward", ev.EventTime, 1);
                        IncrementStat("TrialMasterSuccess", ev.EventTime, 1);
                        if (currentActivity != null)
                        {
                            currentActivity.TrialMasterSuccess = true;
                            currentActivity.TrialMasterFullFinished = false;
                        }
                        break;
                    case EVENT_TYPES.MAVEN_KILLED:
                        IncrementStat("MavenKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.TRIALMASTER_KILLED:
                        IncrementStat("TrialMasterKilled", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.VERITANIA_KILLED:
                        if (lastEvType != EVENT_TYPES.VERITANIA_KILLED)
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
                        if (currentActivity != null)
                        {
                            currentActivity.TrialMasterCount += 1;
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

                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("delirium");
                            }
                            else
                            {
                                currentActivity.AddTag("delirium");
                            }
                        }
                        break;
                    case EVENT_TYPES.BLIGHT_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("blight");
                            }
                            else
                            {
                                currentActivity.AddTag("blight");
                            }
                        }
                        break;
                    case EVENT_TYPES.EINHAR_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("einhar");
                            }
                            else
                            {
                                currentActivity.AddTag("einhar");
                            }
                        }
                        break;
                    case EVENT_TYPES.INCURSION_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("incursion");
                            }
                            else
                            {
                                currentActivity.AddTag("incursion");
                            }
                        }
                        break;
                    case EVENT_TYPES.NIKO_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("niko");
                            }
                            else
                            {
                                currentActivity.AddTag("niko");
                            }
                        }
                        break;
                    case EVENT_TYPES.ZANA_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            currentActivity.AddTag("zana");
                        }
                        break;
                    case EVENT_TYPES.SYNDICATE_ENCOUNTER:
                        if (CheckIfAreaIsMap(sCurrentArea) && currentActivity != null)
                        {
                            if (bIsMapZana && currentActivity.ZanaMap != null)
                            {
                                currentActivity.ZanaMap.AddTag("syndicate");
                            }
                            else
                            {
                                currentActivity.AddTag("syndicate");
                            }
                        }
                        break;
                    case EVENT_TYPES.LEVELUP:
                        bool bIsMySelf = true;
                        foreach (string s in knownPlayers)
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
                            if (iLevel > numStats["HighestLevel"])
                            {
                                SetStat("HighestLevel", ev.EventTime, iLevel);
                            }
                        }
                        break;
                    case EVENT_TYPES.SIMULACRUM_FULLCLEAR:
                        IncrementStat("SimulacrumCleared", ev.EventTime, 1);
                        break;
                    case EVENT_TYPES.LAB_FINISHED:
                        if (currentActivity != null && currentActivity.Type == ACTIVITY_TYPES.LABYRINTH)
                        {
                            IncrementStat("LabsFinished", ev.EventTime, 1);
                            IncrementStat("LabsCompleted_" + currentActivity.Area, ev.EventTime, 1);
                            FinishMap(currentActivity, null, ACTIVITY_TYPES.MAP, ev.EventTime);
                        }
                        break;
                    case EVENT_TYPES.LAB_START_INFO_RECEIVED:
                     
                        break;
                    case EVENT_TYPES.NEXT_AREA_LEVEL_RECEIVED:
                        string sLvl = ev.LogLine.Split(new string[] { "Generating level " }, StringSplitOptions.None)[1]
                            .Split(' ')[0];
                        iNextAreaLevel = Convert.ToInt32(sLvl);
                        iCurrentAreaLevel = iNextAreaLevel;
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
            catch(Exception ex)
            {
                log.Warn("ParseError -> Ex.Message: " + ex.Message + ", LogLine: " + ev.LogLine);
                log.Debug(ex.ToString());
            }
        }

        private void IncrementStat(string s_key, DateTime dt, int i_value = 1)
        {
            numStats[s_key] += i_value;

            SqliteCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = "INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + numStats[s_key] + ")";
            cmd.ExecuteNonQuery();
        }

        private void SetStat(string s_key, DateTime dt, int i_value)
        {
            numStats[s_key] = i_value;

            SqliteCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = "INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + numStats[s_key] + ")";
            cmd.ExecuteNonQuery();
        }

        private string GetEndpointFromInstanceEvent(TrackedEvent ev)
        {
            return ev.LogLine.Split(new String[] { "Connecting to instance server at "}, StringSplitOptions.None)[1];
        }

        private void FinishMap(TrackedActivity map, string sNextMap, ACTIVITY_TYPES sNextMapType, DateTime dtNextMapStarted)
        {
            currentActivity.StopStopWatch();

            if(bEventQInitialized)
            {
                eventHistory.Insert(0, currentActivity);
                AddMapLvItem(map);
                SaveToActivityLog(((DateTimeOffset)map.Started).ToUnixTimeSeconds(), GetStringFromActType(map.Type), map.Area, map.AreaLevel, Convert.ToInt32(map.StopWatchTimeSpan.TotalSeconds), map.DeathCounter, map.TrialMasterCount, false, map.Tags);
                if (map.ZanaMap != null)
                {
                    eventHistory.Insert(0, currentActivity.ZanaMap);
                    AddMapLvItem(map.ZanaMap, true);
                    SaveToActivityLog(((DateTimeOffset)map.ZanaMap.Started).ToUnixTimeSeconds(), GetStringFromActType(map.Type), map.ZanaMap.Area, map.ZanaMap.AreaLevel, Convert.ToInt32(map.ZanaMap.StopWatchTimeSpan.TotalSeconds), map.ZanaMap.DeathCounter, map.ZanaMap.TrialMasterCount, true, map.ZanaMap
                        .Tags);
                }
            }
           
            if(sNextMap != null)
            {
                currentActivity = new TrackedActivity
                {
                    Area = sNextMap,
                    Type = sNextMapType,
                    AreaLevel = iNextAreaLevel,
                    InstanceEndpoint = sCurrentInstanceEndpoint,
                    Started = dtNextMapStarted
                };
                iNextAreaLevel = 0;
                currentActivity.StartStopWatch();
            }
            else
            {
                currentActivity = null;
            }

            if(map.Type == ACTIVITY_TYPES.HEIST)
            {
                IncrementStat("TotalHeistsDone", map.Started, 1);
                IncrementStat("HeistsFinished_" + map.Area, map.Started, 1);
            }
            else if(map.Type == ACTIVITY_TYPES.MAP)
            {
                IncrementStat("TotalMapsDone", map.Started, 1);
                IncrementStat("MapsFinished_" + map.Area, map.Started, 1);

                if(map.ZanaMap != null)
                {
                    IncrementStat("TotalMapsDone", map.ZanaMap.Started, 1);
                    IncrementStat("MapsFinished_" + map.ZanaMap.Area, map.Started, 1);
                }
            }
            else if (map.Type == ACTIVITY_TYPES.SIMULACRUM)
            {
                IncrementStat("SimulacrumFinished_" + map.Area, map.Started, 1);
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

        private void SaveVersion()
        {
            StreamWriter wrt = new StreamWriter("version");
            wrt.WriteLine(APPINFO.VERSION);
            wrt.Close();
        }

        private void LogEvent(TrackedEvent ev)
        {
            log.Info(ev.ToString());
        }

        private void TextLogEvent(TrackedEvent ev)
        {
            this.Invoke((MethodInvoker)delegate
            {
                textBoxLogView.Text += ev.ToString() + Environment.NewLine;
            });
        }

        private void TextLog(string sTxt)
        {
            this.Invoke((MethodInvoker)delegate
            {
                textBoxLogView.Text += "[" + DateTime.Now.ToString() + "] " + sTxt + Environment.NewLine;
            });
            log.Info(sTxt);
        }

        public void ResetMapHistory()
        {
            lvmActLog.ClearLvItems();
            lvmActLog.Columns.Clear();

            ColumnHeader
                chTime = new ColumnHeader() { Name = "actlog_time", Text = "Time", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_time.width", "60")) },
                chType = new ColumnHeader() { Name = "actlog_type", Text = "Type", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_type.width", "60")) },
                chArea = new ColumnHeader() { Name = "actlog_area", Text = "Area", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_area.width", "60")) },
                chLvl = new ColumnHeader() { Name = "actlog_lvl", Text = "Level/Tier", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_lvl.width", "60")) },
                chStopwatch = new ColumnHeader() { Name = "actlog_stopwatch", Text = "Stopwatch", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_stopwatch.width", "60")) },
                chDeath = new ColumnHeader() { Name = "actlog_death", Text = "Deaths", Width = Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_death.width", "60")) };


            lvmActLog.Columns.Add(chTime);
            lvmActLog.Columns.Add(chType);
            lvmActLog.Columns.Add(chArea);
            lvmActLog.Columns.Add(chLvl);
            lvmActLog.Columns.Add(chStopwatch);
            lvmActLog.Columns.Add(chDeath);


            foreach (ActivityTag tag in tags)
            {
                ColumnHeader ch = new ColumnHeader() 
                { 
                    Name = "actlog_tag_" + tag.ID, 
                    Text = tag.DisplayName, 
                    Width =  Convert.ToInt32(ReadSetting("layout.listview.cols.actlog_tag_" + tag.ID + ".width", "60"))
                };
                lvmActLog.Columns.Add(ch);
            }

            foreach (TrackedActivity act in eventHistory)
            {
                AddMapLvItem(act, act.IsZana, -1);
            }

        }
                
        private void AddMapLvItem(TrackedActivity map, bool bZana = false, int iPos = 0)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ListViewItem lvi = new ListViewItem(map.Started.ToString());
                string sName = map.Area;
                string sTier = "";

                if(map.AreaLevel == 0)
                {
                    sTier = "-";
                }
                else if(map.Type == ACTIVITY_TYPES.MAP)
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

                foreach(ActivityTag t in tags)
                {
                    lvi.SubItems.Add(map.Tags.Contains(t.ID) ? "X" : "");
                }

                if(iPos == -1)
                {
                    //listViewActLog.Items.Add(lvi);
                    lvmActLog.AddLvItem(lvi, map.TimeStamp + "_" + map.Area);
                }
                else
                {
                    //listViewActLog.Items.Insert(iPos, lvi);
                    lvmActLog.InsertLvItem(lvi, map.TimeStamp + "_" + map.Area, iPos);
                }
                
            });
        }

        private TrackedActivity GetActivityFromListItemName(string s_name)
        {
            foreach(TrackedActivity ta in eventHistory)
            {
                if (ta.TimeStamp + "_" + ta.Area == s_name)
                    return ta;
            }
            return null;
        }

        private string GetStringFromActType(ACTIVITY_TYPES a_type)
        {
            return a_type.ToString().ToLower();
        }

        private string GetAreaNameFromEvent(TrackedEvent ev)
        {
            string sArea = ev.LogLine.Split(new string[] { "You have entered" }, StringSplitOptions.None)[1]
                .Replace(".", "").Trim();
            return sArea.Replace("'", "");
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
            checkBox1.Checked = bSettingActivityLogShowGrid;
            checkBox2.Checked = bSettingStatsShowGrid;
            ReadBackupList();
            listBox1.DataSource = backups;

            if (bEventQInitialized)
            {
                loadScreen.Close();
                this.Invoke((MethodInvoker)delegate
                {
                    this.Show();

                    if(bRestoreMOde)
                    {
                        bRestoreMOde = false;
                        if (bRestoreOk)
                        {
                            MessageBox.Show("Successfully restored from Backup!");
                        }
                        else
                        {
                            MessageBox.Show("Error restoring from backup: " +
                                Environment.NewLine +
                                Environment.NewLine + sFailedRestoreReason +
                                Environment.NewLine);

                        }
                    }

                    RenderTagsForTracking();
                    RenderTagsForConfig();

                    if(listViewStats.Items.Count > 0)
                    {
                        for (int i = 0; i < numStats.Count; i++)
                        {
                            KeyValuePair<string, int> kvp = numStats.ElementAt(i);
                            lvmStats.GetLvItem("stats_" + kvp.Key).SubItems[1].Text = kvp.Value.ToString();
                        }
                    }

                    if (!bHistoryInitialized)
                    {
                        ReadActivityLogFromSQLite();
                    }
                    
                    labelCurrArea.Text = sCurrentArea;
                    labelCurrentAreaLvl.Text = iCurrentAreaLevel > 0 ? iCurrentAreaLevel.ToString() : "-";
                    labelLastDeath.Text = dtLastDeath.Year > 2000 ? dtLastDeath.ToString() : "-";
                    
                    if(sCurrentArea.Contains("Hideout"))
                    {
                        labelCurrActivity.Text = "In Hideout";
                    }
                    else
                    {
                        if (currentActivity != null)
                        {
                            labelCurrActivity.Text = currentActivity.Type.ToString();
                        }
                        else
                        {
                            labelCurrActivity.Text = "Nothing";
                        }
                    }


                    if (currentActivity != null)
                    {
                        string sTier = "";

                       

                        if (currentActivity.Type == ACTIVITY_TYPES.SIMULACRUM)
                        {
                            currentActivity.AreaLevel = 75;
                        }

                        if (currentActivity.AreaLevel > 0)
                        {
                            if(currentActivity.Type == ACTIVITY_TYPES.MAP)
                            {
                                sTier = "T" + currentActivity.MapTier.ToString();
                            }
                            else
                            {
                                sTier = "Lvl. " + currentActivity.AreaLevel.ToString();
                            }
                        }
                       

                        if (bIsMapZana && currentActivity.ZanaMap != null)
                        {
                            labelStopWatch.Text = currentActivity.ZanaMap.StopWatchValue.ToString();
                            labelTrackingArea.Text = currentActivity.ZanaMap.Area + " (" + sTier + ", Zana)";
                            labelTrackingDied.Text = currentActivity.ZanaMap.DeathCounter.ToString();
                            pictureBox19.Hide();
                        }
                        else
                        {
                            labelStopWatch.Text = currentActivity.StopWatchValue.ToString();
                            labelTrackingArea.Text = currentActivity.Area + " (" + sTier + ")"; ;
                            labelTrackingType.Text = GetStringFromActType(currentActivity.Type).ToUpper();
                            labelTrackingDied.Text = currentActivity.DeathCounter.ToString();
                            pictureBox19.Show();
                        }
                    }
                    else
                    {
                        labelTrackingDied.Text = "0";
                        labelTrackingArea.Text = "-";
                        labelStopWatch.Text = "00:00:00";
                        labelTrackingType.Text = "Enter an ingame activity to auto. start tracking.";
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
            this.bSettingActivityLogShowGrid = Convert.ToBoolean(ReadSetting("ActivityLogShowGrid"));
            this.bSettingStatsShowGrid = Convert.ToBoolean(ReadSetting("StatsShowGrid"));

            listViewActLog.GridLines = bSettingActivityLogShowGrid;
            listViewStats.GridLines = bSettingStatsShowGrid;
        }

        public string ReadSetting(string key, string s_default = null)
        {
            try
            {
                return ConfigurationManager.AppSettings[key] ?? s_default;
            }
            catch (ConfigurationErrorsException)
            {
                return s_default;
            }
        }

        private void Exit()
        {
            bExit = true;
            if (currentActivity != null)
                FinishMap(currentActivity, null, currentActivity.Type, DateTime.Now);
            log.Info("Exitting.");
            Application.Exit();
        }

        public void AddUpdateAppSettings(string key, string value, bool b_log = false)
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
                if(b_log)
                    log.Info("Updated setting: " + key + "=" + value);
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
            if(lvmStats.listView.SelectedIndices.Count > 0)
            {
                chart1.Series[0].Points.Clear();
                DateTime dtStart = DateTime.Now.AddDays(i_days_back * -1);
                string sStatName = lvmStats.listView.SelectedItems[0].Name.Replace("stats_", "");

                DateTime dt1, dt2;
                SqliteDataReader sqlReader;
                SqliteCommand cmd;
                long lTS1, lTS2;

                label38.Text = listViewStats.SelectedItems[0].Text;

                for (int i = 0; i <= i_days_back; i++)
                {
                    dt1 = dtStart.AddDays(i);
                    dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
                    dt2 = new DateTime(dt1.Year, dt1.Month, dt1.Day, 23, 59, 59);
                    lTS1 = ((DateTimeOffset)dt1).ToUnixTimeSeconds();
                    lTS2 = ((DateTimeOffset)dt2).ToUnixTimeSeconds();
                    cmd = dbconn.CreateCommand();

                    if (sStatName == "HighestLevel")
                    {
                        cmd.CommandText = "SELECT stat_value FROM tx_stats WHERE stat_name = '" + sStatName + "' AND timestamp BETWEEN " + lTS1 + " AND " + lTS2;
                    }
                    else
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM tx_stats WHERE stat_name = '" + sStatName + "' AND timestamp BETWEEN " + lTS1 + " AND " + lTS2;
                    }

                    sqlReader = cmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        chart1.Series[0].Points.AddXY(dt1, sqlReader.GetInt32(0));
                    }
                }
            }
        }

        public ActivityTag GetTagByID(string s_id)
        {
            foreach(ActivityTag tag in tags)
            {
                if (tag.ID == s_id)
                    return tag;
            }
            return null;
        }

        private void CreateBackup(string s_name)
        {
            string sBackupDir = "backups/" + s_name + @"/" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            System.IO.Directory.CreateDirectory(sBackupDir);
            
            if(System.IO.File.Exists(sPoELogFilePath))
                System.IO.File.Copy(sPoELogFilePath, sBackupDir + @"/Client.txt");
            if(System.IO.File.Exists("stats.cache"))
                System.IO.File.Copy("stats.cache", sBackupDir + @"/stats.cache");
            if(System.IO.File.Exists("data.db"))
                System.IO.File.Copy("data.db", sBackupDir + @"/data.db");
            if(System.IO.File.Exists("TraXile.exe.config"))
                System.IO.File.Copy("TraXile.exe.config", sBackupDir + @"/TraXile.exe.config");
        }

        private void DoFullReset()
        {
            // Make logfile empty
            FileStream fs1 = new FileStream(sPoELogFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            ResetStats();
            ClearActivityLog();
            iLastHash = 0;
            File.Delete("stats.cache");
        }

        private void OpenActivityDetails(TrackedActivity ta)
        {
            ActivityDetails ad = new ActivityDetails(ta, this);
            ad.Show();
        }

        public void WriteActivitiesToCSV(string sPath)
        {
            StreamWriter wrt = new StreamWriter(sPath);
            TrackedActivity tm;

            //Write headline
            string sLine = "time;type;area;area_level;stopwatch;death_counter";
            wrt.WriteLine(sLine);

            for(int i = 0; i < eventHistory.Count; i++)
            {
                tm = eventHistory[i];
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
            if (currentActivity != null)
                currentActivity.Resume();
        }

        private void pictureBox13_Click_1(object sender, EventArgs e)
        {
            if (currentActivity != null)
                currentActivity.Pause();
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
            button2.Focus();
            if(comboBox1.SelectedIndex > 5)
            {
                if (MessageBox.Show("Selecting more than 3 month could lead to high loading times. Continue?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (listViewStats.SelectedItems.Count > 0)
                        RefreshChart();
                }
                else
                {
                    comboBox1.SelectedIndex = 0;
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
            if (currentActivity != null)
            {
                if (bIsMapZana && currentActivity.ZanaMap != null)
                {
                    if (currentActivity.ZanaMap.Paused)
                    {
                        currentActivity.ZanaMap.Resume();
                    }
                }
                else
                {
                    if (currentActivity.Paused)
                    {
                        currentActivity.Resume();
                    }
                }
            }
        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {
            if(currentActivity != null)
            {
                if(bIsMapZana && currentActivity.ZanaMap != null)
                {
                    if(!currentActivity.ZanaMap.Paused)
                    {
                        currentActivity.ZanaMap.Pause();
                    }
                }
                else
                {
                    if(!currentActivity.Paused)
                    {
                        currentActivity.Pause();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(listViewActLog.SelectedItems.Count == 1)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                long lTimestamp = eventHistory[iIndex].TimeStamp;
                string sType = listViewActLog.Items[iIndex].SubItems[1].Text;
                string sArea = listViewActLog.Items[iIndex].SubItems[2].Text;

                if (MessageBox.Show("Do you really want to delete this Activity? " + Environment.NewLine
                    + Environment.NewLine
                    + "Type: " + sType + Environment.NewLine
                    + "Area: " + sArea + Environment.NewLine
                    + "Time: " + listViewActLog.Items[iIndex].SubItems[0].Text, "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    listViewActLog.Items.Remove(listViewActLog.SelectedItems[0]);
                    eventHistory.RemoveAt(iIndex);
                    DeleteActLogEntry(lTimestamp);
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings stt = new Settings(this);
            stt.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExportActvityList exp = new ExportActvityList(this);
            exp.Show();
        }

       

        private void button5_Click(object sender, EventArgs e)
        {
            if (listViewActLog.SelectedIndices.Count > 0)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                TrackedActivity act = GetActivityFromListItemName(listViewActLog.Items[iIndex].Name);
                if(act != null)
                    OpenActivityDetails(act);
            }

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewActLog.SelectedIndices.Count > 0)
            {
                int iIndex = listViewActLog.SelectedIndices[0];
                TrackedActivity act = GetActivityFromListItemName(listViewActLog.Items[iIndex].Name);
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
            DialogResult dr = MessageBox.Show("For this action, the application needs to be restarted. Continue?", "Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
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
                    AddUpdateAppSettings("PoELogFilePath", ofd.FileName, false);
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
            bSettingActivityLogShowGrid = checkBox1.Checked;
            AddUpdateAppSettings("ActivityLogShowGrid", checkBox1.Checked.ToString());
            listViewActLog.GridLines = bSettingActivityLogShowGrid;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            bSettingStatsShowGrid = checkBox2.Checked;
            AddUpdateAppSettings("StatsShowGrid", checkBox2.Checked.ToString());
            listViewStats.GridLines = bSettingStatsShowGrid;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("With this action your Path of Exile log will be flushed and all data and statistics in TraXile will be deleted." + Environment.NewLine
                + Environment.NewLine +  "It is recommendet to create a backup first - using the 'Create Backup' function. Do you want to create a backup before reset?", "Warning", MessageBoxButtons.YesNoCancel);

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
                if(textBox6.Text == "")
                {
                    textBox6.Text = "Default";
                }

                if(textBox6.Text.Contains("/") || textBox6.Text.Contains("."))
                {
                    MessageBox.Show("Please do not define a path in the field name");
                }
                else
                {
                    CreateBackup(textBox6.Text);
                    MessageBox.Show("Backup successfully created!");
                }
               
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void PrepareBackupRestore(string sPath)
        {
            File.Copy(sPath + @"/stats.cache", "_stats.cache.restore");
            File.Copy(sPath + @"/data.db", "_data.db.restore");
            File.Copy(sPath + @"/Client.txt", Directory.GetParent(sPoELogFilePath) + @"/_Client.txt.restore");
            log.Info("Backup restore successfully prepared! Restarting Application");
            Application.Restart();
        }

        private void DoBackupRestoreIfPrepared()
        {
            if (File.Exists("_stats.cache.restore"))
            {
                File.Delete("stats.cache");
                File.Move("_stats.cache.restore", "stats.cache");
                log.Info("BackupRestored -> Source: _stats.cache.restore, Destination: stats.cache");
                bRestoreMOde = true;
            }

            if (File.Exists("_data.db.restore"))
            {
                File.Delete("data.db");
                File.Move("_data.db.restore", "data.db");
                log.Info("BackupRestored -> Source: _data.db.restore, Destination: data.db");
                bRestoreMOde = true;
            }

            try
            {
                if (File.Exists(Directory.GetParent(sPoELogFilePath) + @"/_Client.txt.restore"))
                {
                    File.Delete(sPoELogFilePath);
                    File.Move(Directory.GetParent(sPoELogFilePath) + @"/_Client.txt.restore", sPoELogFilePath);
                    log.Info("BackupRestored -> Source: " + Directory.GetParent(sPoELogFilePath) + @"/_Client.txt.restore" +
                        ", Destination: " + Directory.GetParent(sPoELogFilePath) + @"/_Client.txt");
                    bRestoreMOde = true;
                }

            }
            catch (Exception ex)
            {
                log.Error("Could not restore Client.txt, please make sure that Path of Exile is not running.");
                log.Debug(ex.ToString());
            }
        }

        private bool CheckTagExists(string s_id)
        {
            foreach(ActivityTag tag in tags)
            {
                if(tag.ID == s_id)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddTag(ActivityTag tag)
        {
            tags.Add(tag);

            SqliteCommand cmd = dbconn.CreateCommand();
            cmd.CommandText = "INSERT INTO tx_tags (tag_id, tag_display, tag_bgcolor, tag_forecolor, tag_type) VALUES "
                + "('" + tag.ID + "', '" + tag.DisplayName + "', '" + tag.BackColor.ToArgb() + "', '" + tag.ForeColor.ToArgb() + "', 'custom')";
            cmd.ExecuteNonQuery();

            listViewActLog.Columns.Add(tag.DisplayName);
            ResetMapHistory();
            RenderTagsForConfig(true);
            RenderTagsForTracking(true);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            DialogResult dr;

            if(Process.GetProcessesByName("PathOfExileSteam").Length > 0 ||
                Process.GetProcessesByName("PathOfExile").Length > 0)
            {
                MessageBox.Show("It seems that PathOfExile is running at the moment. Please close it first.");
            }
           else
            {
                dr = MessageBox.Show("Do you really want to restore the selected Backup? The Application will be restarted. Please make sure that your PathOfExile Client is not running.", "Warning", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    PrepareBackupRestore(listBox1.SelectedItem.ToString());
                }
            }
        }

        private void panelTags_SizeChanged(object sender, EventArgs e)
        {
            if(bEventQInitialized)
                RenderTagsForTracking(true);
        }

        private void panelEditTags_SizeChanged(object sender, EventArgs e)
        {
            if(bEventQInitialized)
                RenderTagsForConfig(true);
        }

        public bool ValidateTagName(string s_name, bool b_showmessage = false)
        {
            bool bValid = true;
            char[] invalid = new char[] { '=', ',', ';', ' ' };

            if (String.IsNullOrEmpty(s_name))
                bValid = false;

            foreach(char c in invalid)
            {
                if(s_name.Contains(c))
                {
                    bValid = false;
                }
            }

            if(bValid == false && b_showmessage )
            {
                MessageBox.Show("Sorry. this is not a valid tag ID!");
            }

            return bValid;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if(ValidateTagName(textBox2.Text, true))
            {
                if (!CheckTagExists(textBox2.Text))
                {
                    AddTag(new ActivityTag(textBox2.Text, false) { DisplayName = textBox3.Text });
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

        public void AddTagAutoCreate(string s_id, TrackedActivity act)
        {
            int iIndex = GetTagIndex(s_id);
            ActivityTag tag;

            if(ValidateTagName(s_id))
            {
                if (iIndex < 0)
                {
                    tag = new ActivityTag(s_id, false);
                    tag.BackColor = Color.White;
                    tag.ForeColor = Color.Black;
                    AddTag(tag);
                }
                else
                {
                    tag = tags[iIndex];
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
                    SqliteCommand cmd = dbconn.CreateCommand();
                    cmd.CommandText = "UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RemoveTagFromActivity(string s_id, TrackedActivity act)
        {
            ActivityTag tag = GetTagByID(s_id);
            if(tag != null && !tag.IsDefault)
            {
                act.RemoveTag(s_id);
                string sTags = "";

                // Update tags in DB // TODO
                for (int i = 0; i < act.Tags.Count; i++)
                {
                    sTags += act.Tags[i];
                    if (i < (act.Tags.Count - 1))
                        sTags += "|";
                    SqliteCommand cmd = dbconn.CreateCommand();
                    cmd.CommandText = "UPDATE tx_activity_log SET act_tags = '" + sTags + "' WHERE timestamp = " + act.TimeStamp.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateTag(string s_id, string s_display_name, string s_forecolor, string s_backcolor)
        {
            int iTagIndex = GetTagIndex(s_id);

            if(iTagIndex >= 0)
            {
                tags[iTagIndex].DisplayName = s_display_name;
                tags[iTagIndex].ForeColor = Color.FromArgb(Convert.ToInt32(s_forecolor));
                tags[iTagIndex].BackColor = Color.FromArgb(Convert.ToInt32(s_backcolor));

                SqliteCommand cmd = dbconn.CreateCommand();
                cmd.CommandText = "UPDATE tx_tags SET tag_display = '" + s_display_name + "', tag_forecolor = '" + s_forecolor + "', tag_bgcolor = '" + s_backcolor + "'" +
                    " WHERE tag_id = '" + s_id + "'";
                cmd.ExecuteNonQuery();
            }

            RenderTagsForConfig(true);
            RenderTagsForTracking(true);
            ResetMapHistory();
        }

        private int GetTagIndex(string s_id)
        {
            for(int i = 0; i < tags.Count; i++)
            {
                if(tags[i].ID == s_id)
                {
                    return i;
                }
            }
            return -1;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            UpdateTag(textBox4.Text, textBox5.Text, label63.ForeColor.ToArgb().ToString(), label63.BackColor.ToArgb().ToString());
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
                DeleteBackup(listBox1.SelectedItem.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            textBox5.Text = textBox4.Text;
        }

        private void MainW_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void chatCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChatCommandHelp cmh = new ChatCommandHelp();
            cmh.ShowDialog();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Exit();
        }

        private void contextMenuStrip1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void chatCommandsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ChatCommandHelp cmh = new ChatCommandHelp();
            cmh.ShowDialog();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if(textBox7.Text == String.Empty)
            {
                lvmStats.Reset();
            }
            lvmStats.ApplyFullTextFilter(textBox7.Text);
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            if (textBox8.Text == String.Empty)
            {
                lvmActLog.Reset();
            }
            else if (textBox8.Text.Contains("tags=="))
            {
                List<string> itemNames = new List<string>();
                try
                {
                    string[] sTagFilter = textBox8.Text.Split(new string[] { "==" }, StringSplitOptions.None)[1].Split(',');
                    int iMatched = 0;
                    foreach (TrackedActivity ta in eventHistory)
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
                            itemNames.Add(ta.TimeStamp + "_" + ta.Area);
                        }
                    }
                    lvmActLog.FilterByNameList(itemNames);
                }
                catch { }
            }
            else if(textBox8.Text.Contains("tags="))
            {
                List<string> itemNames = new List<string>();
                try
                {
                    string[] sTagFilter = textBox8.Text.Split('=')[1].Split(',');
                    int iMatched = 0;
                    foreach(TrackedActivity ta in eventHistory)
                    {
                        iMatched = 0;
                        foreach(string tag in sTagFilter)
                        {
                            if(ta.HasTag(tag))
                            {
                                iMatched++;
                            }
                        }
                        if(iMatched > 0)
                        {
                            itemNames.Add(ta.TimeStamp + "_" + ta.Area);    
                        }
                    }
                    lvmActLog.FilterByNameList(itemNames);
                }
                catch { }
            }
            else
            {
                lvmActLog.ApplyFullTextFilter(textBox8.Text);
            }
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SearchHelp sh = new SearchHelp();
            sh.Show();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox8.Text = "";
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox7.Text = "";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdate(true);
        }

        private void infoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutW ab = new AboutW();
            ab.ShowDialog();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutW ab = new AboutW();
            ab.ShowDialog();
        }

        private void DeleteTag(string s_id)
        {
            int iIndex = GetTagIndex(s_id);
            if(iIndex >= 0)
            {
                ActivityTag tag = tags[iIndex];

                if(tag.IsDefault)
                {
                    MessageBox.Show("Sorry. You cannot delete a default tag!");
                }
                else
                {
                    DialogResult dr = MessageBox.Show("Do you really want to delete the tag '" + s_id + "'?", "Warning", MessageBoxButtons.YesNo);
                    if(dr == DialogResult.Yes)
                    {
                        tags.RemoveAt(iIndex);
                        SqliteCommand cmd = dbconn.CreateCommand();
                        cmd.CommandText = "DELETE FROM tx_tags WHERE tag_id = '" + s_id + "' AND tag_type != 'default'";
                        cmd.ExecuteNonQuery();
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
            backups.Remove(listBox1.SelectedItem.ToString());
        }

        private void pictureBox19_Click(object sender, EventArgs e)
        {
            if (currentActivity != null)
            {
               FinishMap(currentActivity, null, currentActivity.Type, DateTime.Now);
            }
               
        }

        private void pictureBox15_Click_1(object sender, EventArgs e)
        {
            if (currentActivity != null)
            {
                FinishMap(currentActivity, null, currentActivity.Type, DateTime.Now);
            }
        }
       
    }
}