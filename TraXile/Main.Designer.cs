
using System;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace TraXile
{
    partial class Main
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TabControl tabControlMain;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title3 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title4 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend7 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title5 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend8 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title6 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.tabPageTracking = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.labelTrackingDied = new System.Windows.Forms.Label();
            this.pictureBoxSkull = new System.Windows.Forms.PictureBox();
            this.labelTrackingType = new System.Windows.Forms.Label();
            this.labelTrackingArea = new System.Windows.Forms.Label();
            this.pictureBoxStop = new System.Windows.Forms.PictureBox();
            this.pictureBoxPause = new System.Windows.Forms.PictureBox();
            this.pictureBoxPlay = new System.Windows.Forms.PictureBox();
            this.labelStopWatch = new System.Windows.Forms.Label();
            this.pictureBoxStopWatch = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelCurrArea = new System.Windows.Forms.Label();
            this.labelCurrActivity = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.labelLastDeath = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.labelCurrentAreaLvl = new System.Windows.Forms.Label();
            this.panelTags = new System.Windows.Forms.Panel();
            this.groupBoxTrackingTags = new System.Windows.Forms.GroupBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.labelAddTagsNote = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.listViewActLog = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonStartSearch = new System.Windows.Forms.Button();
            this.linkLabelClearSearch = new System.Windows.Forms.LinkLabel();
            this.linkLabelSearchSyntax = new System.Windows.Forms.LinkLabel();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label66 = new System.Windows.Forms.Label();
            this.buttonReloadActivities = new System.Windows.Forms.Button();
            this.buttonActivityDetails = new System.Windows.Forms.Button();
            this.buttonExportActivities = new System.Windows.Forms.Button();
            this.buttonDeleteActivity = new System.Windows.Forms.Button();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxShowMaxItems = new System.Windows.Forms.ComboBox();
            this.label73 = new System.Windows.Forms.Label();
            this.labelItemCount = new System.Windows.Forms.Label();
            this.tabPageStatistics = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chartStats = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonRefreshChart = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxTimeRangeStats = new System.Windows.Forms.ComboBox();
            this.label38 = new System.Windows.Forms.Label();
            this.lbl_info_2 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.linkLabelClearStatsSearch = new System.Windows.Forms.LinkLabel();
            this.textBoxSearchStats = new System.Windows.Forms.TextBox();
            this.label65 = new System.Windows.Forms.Label();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage11 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel24 = new System.Windows.Forms.TableLayoutPanel();
            this.chartGlobalDashboard = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label89 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader22 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel8 = new System.Windows.Forms.Panel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.chartMapTierCount = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label9 = new System.Windows.Forms.Label();
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.chartMapTierAvgTime = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label18 = new System.Windows.Forms.Label();
            this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
            this.listViewTop10Maps = new System.Windows.Forms.ListView();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label29 = new System.Windows.Forms.Label();
            this.tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
            this.listViewTaggingOverview = new System.Windows.Forms.ListView();
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label33 = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel17 = new System.Windows.Forms.TableLayoutPanel();
            this.chartLabsAvgTime = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label50 = new System.Windows.Forms.Label();
            this.tableLayoutPanel16 = new System.Windows.Forms.TableLayoutPanel();
            this.chartLabsDone = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label25 = new System.Windows.Forms.Label();
            this.tableLayoutPanel18 = new System.Windows.Forms.TableLayoutPanel();
            this.listViewBestLabs = new System.Windows.Forms.ListView();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label84 = new System.Windows.Forms.Label();
            this.panel17 = new System.Windows.Forms.Panel();
            this.label49 = new System.Windows.Forms.Label();
            this.checkBoxLabHideUnknown = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel19 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel20 = new System.Windows.Forms.TableLayoutPanel();
            this.chartHeistByLevel = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label85 = new System.Windows.Forms.Label();
            this.tableLayoutPanel21 = new System.Windows.Forms.TableLayoutPanel();
            this.listView4 = new System.Windows.Forms.ListView();
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label86 = new System.Windows.Forms.Label();
            this.tableLayoutPanel22 = new System.Windows.Forms.TableLayoutPanel();
            this.listView5 = new System.Windows.Forms.ListView();
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label87 = new System.Windows.Forms.Label();
            this.tableLayoutPanel23 = new System.Windows.Forms.TableLayoutPanel();
            this.chartHeistAvgTime = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label88 = new System.Windows.Forms.Label();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.panelBossElder = new System.Windows.Forms.Panel();
            this.labelElderTried = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label10 = new System.Windows.Forms.Label();
            this.labelElderKillCount = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelElderStatus = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelBossesShaper = new System.Windows.Forms.Panel();
            this.labelShaperTried = new System.Windows.Forms.Label();
            this.label93 = new System.Windows.Forms.Label();
            this.labelShaperKillCount = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.labelShaperStatus = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panelBossesMaven = new System.Windows.Forms.Panel();
            this.labelMavenTried = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.label43 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.labelMavenStatus = new System.Windows.Forms.Label();
            this.labelMavenKilled = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.panelBossesTrialmaster = new System.Windows.Forms.Panel();
            this.labelTrialMasterTried = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.labelTrialMasterKilled = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.labelTrialMasterStatus = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.panel9 = new System.Windows.Forms.Panel();
            this.label82 = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.label75 = new System.Windows.Forms.Label();
            this.label76 = new System.Windows.Forms.Label();
            this.label77 = new System.Windows.Forms.Label();
            this.label78 = new System.Windows.Forms.Label();
            this.label79 = new System.Windows.Forms.Label();
            this.label80 = new System.Windows.Forms.Label();
            this.label81 = new System.Windows.Forms.Label();
            this.pictureBox21 = new System.Windows.Forms.PictureBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.labelSirusTries = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.labelSirusKillCount = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.labelSirusStatus = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.panel11 = new System.Windows.Forms.Panel();
            this.labelHunterTries = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.labelHunterKillCount = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.labelHunterStatus = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.panel12 = new System.Windows.Forms.Panel();
            this.labelBaranTries = new System.Windows.Forms.Label();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.labelBaranKillCount = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.labelBaranStatus = new System.Windows.Forms.Label();
            this.panel13 = new System.Windows.Forms.Panel();
            this.labelDroxTries = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.labelDroxKillCount = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.labelDroxStatus = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.panel14 = new System.Windows.Forms.Panel();
            this.labelVeritaniaTries = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.labelVeritaniaKillCount = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.labelVeritaniaStatus = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label48 = new System.Windows.Forms.Label();
            this.panel15 = new System.Windows.Forms.Panel();
            this.label106 = new System.Windows.Forms.Label();
            this.label107 = new System.Windows.Forms.Label();
            this.panel16 = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.buttonSaveCaps = new System.Windows.Forms.Button();
            this.label99 = new System.Windows.Forms.Label();
            this.label98 = new System.Windows.Forms.Label();
            this.label97 = new System.Windows.Forms.Label();
            this.textBoxHeistCap = new System.Windows.Forms.TextBox();
            this.label96 = new System.Windows.Forms.Label();
            this.textBoxLabCap = new System.Windows.Forms.TextBox();
            this.label95 = new System.Windows.Forms.Label();
            this.textBoxMapCap = new System.Windows.Forms.TextBox();
            this.label94 = new System.Windows.Forms.Label();
            this.lbl_info_1 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.buttonDeleteBackup = new System.Windows.Forms.Button();
            this.buttonRestoreBackup = new System.Windows.Forms.Button();
            this.listBoxRestoreBackup = new System.Windows.Forms.ListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label62 = new System.Windows.Forms.Label();
            this.textBoxBackupName = new System.Windows.Forms.TextBox();
            this.buttonCreateBackup = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBoxShowGridInStats = new System.Windows.Forms.CheckBox();
            this.checkBoxShowGridInAct = new System.Windows.Forms.CheckBox();
            this.comboBoxTheme = new System.Windows.Forms.ComboBox();
            this.label55 = new System.Windows.Forms.Label();
            this.label61 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxLogFilePath = new System.Windows.Forms.TextBox();
            this.label56 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label46 = new System.Windows.Forms.Label();
            this.label72 = new System.Windows.Forms.Label();
            this.label71 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.label70 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.buttonRollLog = new System.Windows.Forms.Button();
            this.buttonFullReset = new System.Windows.Forms.Button();
            this.buttonChangeLogReload = new System.Windows.Forms.Button();
            this.buttonReloadLogfile = new System.Windows.Forms.Button();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.panelEditTags = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label58 = new System.Windows.Forms.Label();
            this.label57 = new System.Windows.Forms.Label();
            this.button10 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.label90 = new System.Windows.Forms.Label();
            this.button20 = new System.Windows.Forms.Button();
            this.label59 = new System.Windows.Forms.Label();
            this.button19 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.button14 = new System.Windows.Forms.Button();
            this.label60 = new System.Windows.Forms.Label();
            this.button13 = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label64 = new System.Windows.Forms.Label();
            this.button11 = new System.Windows.Forms.Button();
            this.label63 = new System.Windows.Forms.Label();
            this.button12 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBoxLogView = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chatCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wikiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.labelKilledBy = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelActivity = new System.Windows.Forms.Label();
            this.labelCurrentArea = new System.Windows.Forms.Label();
            this.labelLatDeath = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox16 = new System.Windows.Forms.PictureBox();
            this.labelCurrentMapName = new System.Windows.Forms.Label();
            this.pictureBox12 = new System.Windows.Forms.PictureBox();
            this.labelMapTime = new System.Windows.Forms.Label();
            this.labelDiedInMap = new System.Windows.Forms.Label();
            this.pictureBox14 = new System.Windows.Forms.PictureBox();
            this.pictureBox13 = new System.Windows.Forms.PictureBox();
            this.pictureBox15 = new System.Windows.Forms.PictureBox();
            this.label51 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.chatCommandsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.listViewStats = new TraXile.ListViewNF();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            tabControlMain = new System.Windows.Forms.TabControl();
            tabControlMain.SuspendLayout();
            this.tabPageTracking.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSkull)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStopWatch)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.panelTags.SuspendLayout();
            this.panel7.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tabPageStatistics.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartStats)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage11.SuspendLayout();
            this.tableLayoutPanel24.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartGlobalDashboard)).BeginInit();
            this.panel8.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartMapTierCount)).BeginInit();
            this.tableLayoutPanel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartMapTierAvgTime)).BeginInit();
            this.tableLayoutPanel13.SuspendLayout();
            this.tableLayoutPanel14.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tableLayoutPanel15.SuspendLayout();
            this.tableLayoutPanel17.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLabsAvgTime)).BeginInit();
            this.tableLayoutPanel16.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLabsDone)).BeginInit();
            this.tableLayoutPanel18.SuspendLayout();
            this.panel17.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tableLayoutPanel19.SuspendLayout();
            this.tableLayoutPanel20.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeistByLevel)).BeginInit();
            this.tableLayoutPanel21.SuspendLayout();
            this.tableLayoutPanel22.SuspendLayout();
            this.tableLayoutPanel23.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeistAvgTime)).BeginInit();
            this.tabPage9.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.panelBossElder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelBossesShaper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panelBossesMaven.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            this.panelBossesTrialmaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            this.panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox21)).BeginInit();
            this.panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.tabPage10.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.panel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            this.panel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.panel13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            this.panel14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.panel15.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.panelEditTags.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox16)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox14)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox15)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            tabControlMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            tabControlMain.Controls.Add(this.tabPageTracking);
            tabControlMain.Controls.Add(this.tabPageStatistics);
            tabControlMain.Controls.Add(this.tabPage8);
            tabControlMain.Controls.Add(this.tabPage3);
            tabControlMain.Controls.Add(this.tabPage2);
            tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlMain.HotTrack = true;
            tabControlMain.ImeMode = System.Windows.Forms.ImeMode.KatakanaHalf;
            tabControlMain.Location = new System.Drawing.Point(0, 24);
            tabControlMain.Multiline = true;
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new System.Drawing.Size(1123, 903);
            tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabControlMain.TabIndex = 1;
            // 
            // tabPageTracking
            // 
            this.tabPageTracking.BackColor = System.Drawing.Color.Transparent;
            this.tabPageTracking.Controls.Add(this.splitContainer1);
            this.tabPageTracking.Location = new System.Drawing.Point(4, 25);
            this.tabPageTracking.Name = "tabPageTracking";
            this.tabPageTracking.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTracking.Size = new System.Drawing.Size(1115, 874);
            this.tabPageTracking.TabIndex = 6;
            this.tabPageTracking.Text = "Tracking";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel5);
            this.splitContainer1.Size = new System.Drawing.Size(1109, 868);
            this.splitContainer1.SplitterDistance = 477;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.panelTags, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel7, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 341F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(477, 868);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.pictureBox10);
            this.panel2.Controls.Add(this.labelTrackingDied);
            this.panel2.Controls.Add(this.pictureBoxSkull);
            this.panel2.Controls.Add(this.labelTrackingType);
            this.panel2.Controls.Add(this.labelTrackingArea);
            this.panel2.Controls.Add(this.pictureBoxStop);
            this.panel2.Controls.Add(this.pictureBoxPause);
            this.panel2.Controls.Add(this.pictureBoxPlay);
            this.panel2.Controls.Add(this.labelStopWatch);
            this.panel2.Controls.Add(this.pictureBoxStopWatch);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(471, 224);
            this.panel2.TabIndex = 0;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Location = new System.Drawing.Point(5, 185);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(32, 32);
            this.pictureBox10.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox10.TabIndex = 9;
            this.pictureBox10.TabStop = false;
            // 
            // labelTrackingDied
            // 
            this.labelTrackingDied.AutoSize = true;
            this.labelTrackingDied.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrackingDied.ForeColor = System.Drawing.Color.White;
            this.labelTrackingDied.Location = new System.Drawing.Point(363, 106);
            this.labelTrackingDied.Name = "labelTrackingDied";
            this.labelTrackingDied.Size = new System.Drawing.Size(24, 25);
            this.labelTrackingDied.TabIndex = 8;
            this.labelTrackingDied.Text = "0";
            // 
            // pictureBoxSkull
            // 
            this.pictureBoxSkull.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxSkull.Image")));
            this.pictureBoxSkull.Location = new System.Drawing.Point(317, 105);
            this.pictureBoxSkull.Name = "pictureBoxSkull";
            this.pictureBoxSkull.Size = new System.Drawing.Size(55, 28);
            this.pictureBoxSkull.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSkull.TabIndex = 7;
            this.pictureBoxSkull.TabStop = false;
            // 
            // labelTrackingType
            // 
            this.labelTrackingType.AutoSize = true;
            this.labelTrackingType.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrackingType.ForeColor = System.Drawing.Color.White;
            this.labelTrackingType.Location = new System.Drawing.Point(4, 162);
            this.labelTrackingType.Name = "labelTrackingType";
            this.labelTrackingType.Size = new System.Drawing.Size(31, 15);
            this.labelTrackingType.TabIndex = 6;
            this.labelTrackingType.Text = "Map";
            // 
            // labelTrackingArea
            // 
            this.labelTrackingArea.AutoSize = true;
            this.labelTrackingArea.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrackingArea.ForeColor = System.Drawing.Color.White;
            this.labelTrackingArea.Location = new System.Drawing.Point(43, 194);
            this.labelTrackingArea.Name = "labelTrackingArea";
            this.labelTrackingArea.Size = new System.Drawing.Size(215, 15);
            this.labelTrackingArea.TabIndex = 5;
            this.labelTrackingArea.Text = "Rewritten Distant Memories";
            // 
            // pictureBoxStop
            // 
            this.pictureBoxStop.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStop.Image")));
            this.pictureBoxStop.Location = new System.Drawing.Point(263, 94);
            this.pictureBoxStop.Name = "pictureBoxStop";
            this.pictureBoxStop.Size = new System.Drawing.Size(48, 50);
            this.pictureBoxStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxStop.TabIndex = 4;
            this.pictureBoxStop.TabStop = false;
            this.pictureBoxStop.Click += new System.EventHandler(this.pictureBox19_Click);
            // 
            // pictureBoxPause
            // 
            this.pictureBoxPause.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxPause.Image")));
            this.pictureBoxPause.Location = new System.Drawing.Point(209, 94);
            this.pictureBoxPause.Name = "pictureBoxPause";
            this.pictureBoxPause.Size = new System.Drawing.Size(48, 50);
            this.pictureBoxPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPause.TabIndex = 3;
            this.pictureBoxPause.TabStop = false;
            this.pictureBoxPause.Click += new System.EventHandler(this.pictureBox18_Click);
            // 
            // pictureBoxPlay
            // 
            this.pictureBoxPlay.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxPlay.Image")));
            this.pictureBoxPlay.Location = new System.Drawing.Point(155, 94);
            this.pictureBoxPlay.Name = "pictureBoxPlay";
            this.pictureBoxPlay.Size = new System.Drawing.Size(48, 50);
            this.pictureBoxPlay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPlay.TabIndex = 2;
            this.pictureBoxPlay.TabStop = false;
            this.pictureBoxPlay.Click += new System.EventHandler(this.pictureBox17_Click);
            // 
            // labelStopWatch
            // 
            this.labelStopWatch.AutoSize = true;
            this.labelStopWatch.Font = new System.Drawing.Font("Noto Mono", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStopWatch.ForeColor = System.Drawing.Color.White;
            this.labelStopWatch.Location = new System.Drawing.Point(150, 30);
            this.labelStopWatch.Name = "labelStopWatch";
            this.labelStopWatch.Size = new System.Drawing.Size(257, 57);
            this.labelStopWatch.TabIndex = 1;
            this.labelStopWatch.Text = "00:00:00";
            // 
            // pictureBoxStopWatch
            // 
            this.pictureBoxStopWatch.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStopWatch.Image")));
            this.pictureBoxStopWatch.Location = new System.Drawing.Point(6, 27);
            this.pictureBoxStopWatch.Name = "pictureBoxStopWatch";
            this.pictureBoxStopWatch.Size = new System.Drawing.Size(139, 127);
            this.pictureBoxStopWatch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxStopWatch.TabIndex = 0;
            this.pictureBoxStopWatch.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.58448F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.41552F));
            this.tableLayoutPanel3.Controls.Add(this.labelCurrArea, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.labelCurrActivity, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label52, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label53, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label54, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.labelLastDeath, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.label67, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.labelCurrentAreaLvl, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 607);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(471, 179);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // labelCurrArea
            // 
            this.labelCurrArea.AutoSize = true;
            this.labelCurrArea.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrArea.ForeColor = System.Drawing.Color.White;
            this.labelCurrArea.Location = new System.Drawing.Point(184, 24);
            this.labelCurrArea.Name = "labelCurrArea";
            this.labelCurrArea.Size = new System.Drawing.Size(15, 15);
            this.labelCurrArea.TabIndex = 5;
            this.labelCurrArea.Text = "-";
            // 
            // labelCurrActivity
            // 
            this.labelCurrActivity.AutoSize = true;
            this.labelCurrActivity.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrActivity.ForeColor = System.Drawing.Color.White;
            this.labelCurrActivity.Location = new System.Drawing.Point(184, 0);
            this.labelCurrActivity.Name = "labelCurrActivity";
            this.labelCurrActivity.Size = new System.Drawing.Size(63, 15);
            this.labelCurrActivity.TabIndex = 4;
            this.labelCurrActivity.Text = "nothing";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.ForeColor = System.Drawing.Color.White;
            this.label52.Location = new System.Drawing.Point(3, 0);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(88, 15);
            this.label52.TabIndex = 0;
            this.label52.Text = "Activity:";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label53.ForeColor = System.Drawing.Color.White;
            this.label53.Location = new System.Drawing.Point(3, 24);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(52, 15);
            this.label53.TabIndex = 1;
            this.label53.Text = "Area:";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label54.ForeColor = System.Drawing.Color.White;
            this.label54.Location = new System.Drawing.Point(3, 72);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(106, 15);
            this.label54.TabIndex = 2;
            this.label54.Text = "Last death:";
            // 
            // labelLastDeath
            // 
            this.labelLastDeath.AutoSize = true;
            this.labelLastDeath.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastDeath.ForeColor = System.Drawing.Color.White;
            this.labelLastDeath.Location = new System.Drawing.Point(184, 72);
            this.labelLastDeath.Name = "labelLastDeath";
            this.labelLastDeath.Size = new System.Drawing.Size(15, 15);
            this.labelLastDeath.TabIndex = 6;
            this.labelLastDeath.Text = "-";
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label67.ForeColor = System.Drawing.Color.White;
            this.label67.Location = new System.Drawing.Point(3, 48);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(106, 15);
            this.label67.TabIndex = 7;
            this.label67.Text = "Area level:";
            // 
            // labelCurrentAreaLvl
            // 
            this.labelCurrentAreaLvl.AutoSize = true;
            this.labelCurrentAreaLvl.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrentAreaLvl.ForeColor = System.Drawing.Color.White;
            this.labelCurrentAreaLvl.Location = new System.Drawing.Point(184, 48);
            this.labelCurrentAreaLvl.Name = "labelCurrentAreaLvl";
            this.labelCurrentAreaLvl.Size = new System.Drawing.Size(15, 15);
            this.labelCurrentAreaLvl.TabIndex = 8;
            this.labelCurrentAreaLvl.Text = "-";
            // 
            // panelTags
            // 
            this.panelTags.Controls.Add(this.groupBoxTrackingTags);
            this.panelTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTags.Location = new System.Drawing.Point(3, 233);
            this.panelTags.Name = "panelTags";
            this.panelTags.Size = new System.Drawing.Size(471, 335);
            this.panelTags.TabIndex = 2;
            this.panelTags.SizeChanged += new System.EventHandler(this.panelTags_SizeChanged);
            // 
            // groupBoxTrackingTags
            // 
            this.groupBoxTrackingTags.BackColor = System.Drawing.Color.Black;
            this.groupBoxTrackingTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxTrackingTags.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxTrackingTags.ForeColor = System.Drawing.Color.Red;
            this.groupBoxTrackingTags.Location = new System.Drawing.Point(0, 0);
            this.groupBoxTrackingTags.Name = "groupBoxTrackingTags";
            this.groupBoxTrackingTags.Size = new System.Drawing.Size(471, 335);
            this.groupBoxTrackingTags.TabIndex = 0;
            this.groupBoxTrackingTags.TabStop = false;
            this.groupBoxTrackingTags.Text = "Tags";
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.labelAddTagsNote);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel7.Location = new System.Drawing.Point(3, 574);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(471, 27);
            this.panel7.TabIndex = 3;
            // 
            // labelAddTagsNote
            // 
            this.labelAddTagsNote.AutoSize = true;
            this.labelAddTagsNote.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAddTagsNote.ForeColor = System.Drawing.Color.Red;
            this.labelAddTagsNote.Location = new System.Drawing.Point(2, 0);
            this.labelAddTagsNote.Name = "labelAddTagsNote";
            this.labelAddTagsNote.Size = new System.Drawing.Size(455, 13);
            this.labelAddTagsNote.TabIndex = 0;
            this.labelAddTagsNote.Text = "* custom tags can be manually added to active map by clicking it";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.listViewActLog, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel7, 0, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(628, 868);
            this.tableLayoutPanel5.TabIndex = 5;
            // 
            // listViewActLog
            // 
            this.listViewActLog.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listViewActLog.BackColor = System.Drawing.Color.Black;
            this.listViewActLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader7,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewActLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewActLog.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewActLog.ForeColor = System.Drawing.Color.White;
            this.listViewActLog.FullRowSelect = true;
            this.listViewActLog.GridLines = true;
            this.listViewActLog.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewActLog.HideSelection = false;
            this.listViewActLog.Location = new System.Drawing.Point(3, 90);
            this.listViewActLog.MultiSelect = false;
            this.listViewActLog.Name = "listViewActLog";
            this.listViewActLog.Size = new System.Drawing.Size(622, 743);
            this.listViewActLog.SmallImageList = this.imageList1;
            this.listViewActLog.TabIndex = 4;
            this.listViewActLog.UseCompatibleStateImageBehavior = false;
            this.listViewActLog.View = System.Windows.Forms.View.Details;
            this.listViewActLog.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.listViewActLog_ColumnWidthChanged);
            this.listViewActLog.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Type";
            this.columnHeader7.Width = 62;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Area";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "StopWatch";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "DeathCounter";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "WhiteMap.png");
            this.imageList1.Images.SetKeyName(1, "YellowMap.png");
            this.imageList1.Images.SetKeyName(2, "RedMap.png");
            this.imageList1.Images.SetKeyName(3, "TempleMap.png");
            this.imageList1.Images.SetKeyName(4, "ContractItem.png");
            this.imageList1.Images.SetKeyName(5, "Abyss.png");
            this.imageList1.Images.SetKeyName(6, "Labyrinth.png");
            this.imageList1.Images.SetKeyName(7, "Campaign.png");
            this.imageList1.Images.SetKeyName(8, "ExpeditionChronicle3.png");
            this.imageList1.Images.SetKeyName(9, "Vaal.png");
            this.imageList1.Images.SetKeyName(10, "Catarina.png");
            this.imageList1.Images.SetKeyName(11, "Safehouse.png");
            this.imageList1.Images.SetKeyName(12, "Fossil.png");
            this.imageList1.Images.SetKeyName(13, "Invitation.png");
            this.imageList1.Images.SetKeyName(14, "Sirus.png");
            this.imageList1.Images.SetKeyName(15, "Vaal01.png");
            this.imageList1.Images.SetKeyName(16, "UberVaal04.png");
            this.imageList1.Images.SetKeyName(17, "Elder.png");
            this.imageList1.Images.SetKeyName(18, "Shaper.png");
            this.imageList1.Images.SetKeyName(19, "Simulacrum.png");
            this.imageList1.Images.SetKeyName(20, "MavenKey.png");
            this.imageList1.Images.SetKeyName(21, "BreachFragmentsChaosChayula.png");
            this.imageList1.Images.SetKeyName(22, "BreachFragmentsLightningEsh.png");
            this.imageList1.Images.SetKeyName(23, "BreachFragmentsFireXoph.png");
            this.imageList1.Images.SetKeyName(24, "BreachFragmentsPhysicalUul.png");
            this.imageList1.Images.SetKeyName(25, "BreachFragmentsColdTul.png");
            this.imageList1.Images.SetKeyName(26, "TulsFlawlessBreachstone.png");
            this.imageList1.Images.SetKeyName(27, "TulsPureBreachstone.png");
            this.imageList1.Images.SetKeyName(28, "TulsEnrichedBreachstone.png");
            this.imageList1.Images.SetKeyName(29, "TulsChargedBreachstone.png");
            this.imageList1.Images.SetKeyName(30, "UulNetolsFlawlessBreachstone.png");
            this.imageList1.Images.SetKeyName(31, "UulNetolsPureBreachstone.png");
            this.imageList1.Images.SetKeyName(32, "UulNetolsEnrichedBreachstone.png");
            this.imageList1.Images.SetKeyName(33, "UulNetolsChargedBreachstone.png");
            this.imageList1.Images.SetKeyName(34, "XophsFlawlessBreachstone.png");
            this.imageList1.Images.SetKeyName(35, "XophsPureBreachstone.png");
            this.imageList1.Images.SetKeyName(36, "XophsEnrichedBreachstone.png");
            this.imageList1.Images.SetKeyName(37, "XophsChargedBreachstone.png");
            this.imageList1.Images.SetKeyName(38, "ChayulasFlawlessBreachstone.png");
            this.imageList1.Images.SetKeyName(39, "ChayulasPureBreachstone.png");
            this.imageList1.Images.SetKeyName(40, "ChayulasEnrichedBreachstone.png");
            this.imageList1.Images.SetKeyName(41, "ChayulasChargedBreachstone.png");
            this.imageList1.Images.SetKeyName(42, "EshsFlawlessBreachstone.png");
            this.imageList1.Images.SetKeyName(43, "EshsPureBreachstone.png");
            this.imageList1.Images.SetKeyName(44, "EshsEnrichedBreachstone.png");
            this.imageList1.Images.SetKeyName(45, "EshsChargedBreachstone.png");
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel3.Controls.Add(this.buttonStartSearch);
            this.panel3.Controls.Add(this.linkLabelClearSearch);
            this.panel3.Controls.Add(this.linkLabelSearchSyntax);
            this.panel3.Controls.Add(this.textBox8);
            this.panel3.Controls.Add(this.label66);
            this.panel3.Controls.Add(this.buttonReloadActivities);
            this.panel3.Controls.Add(this.buttonActivityDetails);
            this.panel3.Controls.Add(this.buttonExportActivities);
            this.panel3.Controls.Add(this.buttonDeleteActivity);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(622, 81);
            this.panel3.TabIndex = 5;
            // 
            // buttonStartSearch
            // 
            this.buttonStartSearch.Location = new System.Drawing.Point(479, 15);
            this.buttonStartSearch.Name = "buttonStartSearch";
            this.buttonStartSearch.Size = new System.Drawing.Size(62, 23);
            this.buttonStartSearch.TabIndex = 10;
            this.buttonStartSearch.Text = "search";
            this.buttonStartSearch.UseVisualStyleBackColor = true;
            this.buttonStartSearch.Click += new System.EventHandler(this.button22_Click);
            // 
            // linkLabelClearSearch
            // 
            this.linkLabelClearSearch.AutoSize = true;
            this.linkLabelClearSearch.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelClearSearch.ForeColor = System.Drawing.Color.Red;
            this.linkLabelClearSearch.LinkColor = System.Drawing.Color.Red;
            this.linkLabelClearSearch.Location = new System.Drawing.Point(507, 50);
            this.linkLabelClearSearch.Name = "linkLabelClearSearch";
            this.linkLabelClearSearch.Size = new System.Drawing.Size(42, 13);
            this.linkLabelClearSearch.TabIndex = 7;
            this.linkLabelClearSearch.TabStop = true;
            this.linkLabelClearSearch.Text = "Clear";
            this.linkLabelClearSearch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabelSearchSyntax
            // 
            this.linkLabelSearchSyntax.AutoSize = true;
            this.linkLabelSearchSyntax.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelSearchSyntax.ForeColor = System.Drawing.Color.Red;
            this.linkLabelSearchSyntax.LinkColor = System.Drawing.Color.Red;
            this.linkLabelSearchSyntax.Location = new System.Drawing.Point(403, 50);
            this.linkLabelSearchSyntax.Name = "linkLabelSearchSyntax";
            this.linkLabelSearchSyntax.Size = new System.Drawing.Size(98, 13);
            this.linkLabelSearchSyntax.TabIndex = 6;
            this.linkLabelSearchSyntax.TabStop = true;
            this.linkLabelSearchSyntax.Text = "Search syntax";
            this.linkLabelSearchSyntax.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(74, 17);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(399, 20);
            this.textBox8.TabIndex = 5;
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label66.ForeColor = System.Drawing.Color.White;
            this.label66.Location = new System.Drawing.Point(5, 19);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(63, 15);
            this.label66.TabIndex = 4;
            this.label66.Text = "Search:";
            // 
            // buttonReloadActivities
            // 
            this.buttonReloadActivities.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonReloadActivities.FlatAppearance.BorderSize = 0;
            this.buttonReloadActivities.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReloadActivities.Location = new System.Drawing.Point(322, 50);
            this.buttonReloadActivities.Name = "buttonReloadActivities";
            this.buttonReloadActivities.Size = new System.Drawing.Size(75, 21);
            this.buttonReloadActivities.TabIndex = 3;
            this.buttonReloadActivities.Text = "reload";
            this.buttonReloadActivities.UseVisualStyleBackColor = false;
            this.buttonReloadActivities.Click += new System.EventHandler(this.button9_Click);
            // 
            // buttonActivityDetails
            // 
            this.buttonActivityDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.buttonActivityDetails.FlatAppearance.BorderSize = 0;
            this.buttonActivityDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonActivityDetails.Location = new System.Drawing.Point(241, 50);
            this.buttonActivityDetails.Name = "buttonActivityDetails";
            this.buttonActivityDetails.Size = new System.Drawing.Size(75, 21);
            this.buttonActivityDetails.TabIndex = 2;
            this.buttonActivityDetails.Text = "details";
            this.buttonActivityDetails.UseVisualStyleBackColor = false;
            this.buttonActivityDetails.Click += new System.EventHandler(this.button5_Click);
            // 
            // buttonExportActivities
            // 
            this.buttonExportActivities.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.buttonExportActivities.FlatAppearance.BorderSize = 0;
            this.buttonExportActivities.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExportActivities.Location = new System.Drawing.Point(160, 50);
            this.buttonExportActivities.Name = "buttonExportActivities";
            this.buttonExportActivities.Size = new System.Drawing.Size(75, 21);
            this.buttonExportActivities.TabIndex = 1;
            this.buttonExportActivities.Text = "export";
            this.buttonExportActivities.UseVisualStyleBackColor = false;
            this.buttonExportActivities.Click += new System.EventHandler(this.button4_Click);
            // 
            // buttonDeleteActivity
            // 
            this.buttonDeleteActivity.BackColor = System.Drawing.Color.Red;
            this.buttonDeleteActivity.FlatAppearance.BorderSize = 0;
            this.buttonDeleteActivity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDeleteActivity.Location = new System.Drawing.Point(79, 50);
            this.buttonDeleteActivity.Name = "buttonDeleteActivity";
            this.buttonDeleteActivity.Size = new System.Drawing.Size(75, 21);
            this.buttonDeleteActivity.TabIndex = 0;
            this.buttonDeleteActivity.Text = "delete";
            this.buttonDeleteActivity.UseVisualStyleBackColor = false;
            this.buttonDeleteActivity.Click += new System.EventHandler(this.button3_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel7.ColumnCount = 3;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 134F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 154F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 467F));
            this.tableLayoutPanel7.Controls.Add(this.comboBoxShowMaxItems, 2, 0);
            this.tableLayoutPanel7.Controls.Add(this.label73, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.labelItemCount, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 839);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(622, 26);
            this.tableLayoutPanel7.TabIndex = 6;
            // 
            // comboBoxShowMaxItems
            // 
            this.comboBoxShowMaxItems.Dock = System.Windows.Forms.DockStyle.Left;
            this.comboBoxShowMaxItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShowMaxItems.FormattingEnabled = true;
            this.comboBoxShowMaxItems.Items.AddRange(new object[] {
            "500",
            "1000",
            "1500",
            "2000",
            "3000",
            "5000"});
            this.comboBoxShowMaxItems.Location = new System.Drawing.Point(291, 3);
            this.comboBoxShowMaxItems.Name = "comboBoxShowMaxItems";
            this.comboBoxShowMaxItems.Size = new System.Drawing.Size(133, 21);
            this.comboBoxShowMaxItems.TabIndex = 12;
            this.comboBoxShowMaxItems.SelectionChangeCommitted += new System.EventHandler(this.comboBox2_SelectionChangeCommitted);
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label73.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label73.ForeColor = System.Drawing.Color.White;
            this.label73.Location = new System.Drawing.Point(137, 0);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(148, 26);
            this.label73.TabIndex = 13;
            this.label73.Text = "max items:";
            this.label73.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelItemCount
            // 
            this.labelItemCount.AutoSize = true;
            this.labelItemCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelItemCount.Font = new System.Drawing.Font("Noto Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelItemCount.ForeColor = System.Drawing.Color.White;
            this.labelItemCount.Location = new System.Drawing.Point(3, 0);
            this.labelItemCount.Name = "labelItemCount";
            this.labelItemCount.Size = new System.Drawing.Size(128, 26);
            this.labelItemCount.TabIndex = 9;
            this.labelItemCount.Text = "items: 0";
            this.labelItemCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabPageStatistics
            // 
            this.tabPageStatistics.Controls.Add(this.tableLayoutPanel1);
            this.tabPageStatistics.Location = new System.Drawing.Point(4, 25);
            this.tabPageStatistics.Name = "tabPageStatistics";
            this.tabPageStatistics.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStatistics.Size = new System.Drawing.Size(1115, 874);
            this.tabPageStatistics.TabIndex = 3;
            this.tabPageStatistics.Text = "Statistics";
            this.tabPageStatistics.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.listViewStats, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chartStats, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label38, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lbl_info_2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1109, 868);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chartStats
            // 
            this.chartStats.BackColor = System.Drawing.Color.Black;
            this.chartStats.BackSecondaryColor = System.Drawing.Color.Red;
            this.chartStats.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.BorderColor = System.Drawing.Color.Red;
            chartArea1.Name = "ChartArea1";
            this.chartStats.ChartAreas.Add(chartArea1);
            this.chartStats.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.BackColor = System.Drawing.Color.Black;
            legend1.ForeColor = System.Drawing.Color.Red;
            legend1.Name = "Legend1";
            this.chartStats.Legends.Add(legend1);
            this.chartStats.Location = new System.Drawing.Point(3, 643);
            this.chartStats.Name = "chartStats";
            series1.BackSecondaryColor = System.Drawing.Color.Red;
            series1.ChartArea = "ChartArea1";
            series1.Color = System.Drawing.Color.Red;
            series1.LabelBackColor = System.Drawing.Color.Red;
            series1.LabelBorderColor = System.Drawing.Color.Red;
            series1.LabelForeColor = System.Drawing.Color.Red;
            series1.Legend = "Legend1";
            series1.MarkerBorderColor = System.Drawing.Color.Red;
            series1.MarkerColor = System.Drawing.Color.Red;
            series1.Name = "Series1";
            this.chartStats.Series.Add(series1);
            this.chartStats.Size = new System.Drawing.Size(1103, 222);
            this.chartStats.TabIndex = 5;
            this.chartStats.Text = "chart1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonRefreshChart);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.comboBoxTimeRangeStats);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 593);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1103, 24);
            this.panel1.TabIndex = 7;
            // 
            // buttonRefreshChart
            // 
            this.buttonRefreshChart.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonRefreshChart.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRefreshChart.Location = new System.Drawing.Point(1028, 0);
            this.buttonRefreshChart.Name = "buttonRefreshChart";
            this.buttonRefreshChart.Size = new System.Drawing.Size(75, 24);
            this.buttonRefreshChart.TabIndex = 2;
            this.buttonRefreshChart.Text = "refresh";
            this.buttonRefreshChart.UseVisualStyleBackColor = true;
            this.buttonRefreshChart.Click += new System.EventHandler(this.button2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "time range:";
            // 
            // comboBoxTimeRangeStats
            // 
            this.comboBoxTimeRangeStats.BackColor = System.Drawing.Color.White;
            this.comboBoxTimeRangeStats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeRangeStats.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxTimeRangeStats.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxTimeRangeStats.FormattingEnabled = true;
            this.comboBoxTimeRangeStats.ImeMode = System.Windows.Forms.ImeMode.KatakanaHalf;
            this.comboBoxTimeRangeStats.Items.AddRange(new object[] {
            "Last week",
            "Last 2 weeks",
            "Last 3 weeks",
            "Last month",
            "Last 2 month",
            "Last 3 month",
            "Last year",
            "Last 2 years",
            "Last 3 years",
            "All time"});
            this.comboBoxTimeRangeStats.Location = new System.Drawing.Point(103, 3);
            this.comboBoxTimeRangeStats.Name = "comboBoxTimeRangeStats";
            this.comboBoxTimeRangeStats.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTimeRangeStats.TabIndex = 0;
            this.comboBoxTimeRangeStats.TextChanged += new System.EventHandler(this.comboBox1_TextChanged);
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label38.ForeColor = System.Drawing.Color.Red;
            this.label38.Location = new System.Drawing.Point(3, 620);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(1103, 20);
            this.label38.TabIndex = 8;
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_info_2
            // 
            this.lbl_info_2.AutoSize = true;
            this.lbl_info_2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_info_2.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_2.ForeColor = System.Drawing.Color.Red;
            this.lbl_info_2.Location = new System.Drawing.Point(3, 560);
            this.lbl_info_2.Name = "lbl_info_2";
            this.lbl_info_2.Size = new System.Drawing.Size(1103, 30);
            this.lbl_info_2.TabIndex = 9;
            this.lbl_info_2.Text = "* for some reason the final dialogue lines are missing in the logfile - this will" +
    " hopefully been fixed in a further version of TraXile";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.button3);
            this.panel6.Controls.Add(this.linkLabelClearStatsSearch);
            this.panel6.Controls.Add(this.textBoxSearchStats);
            this.panel6.Controls.Add(this.label65);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1103, 25);
            this.panel6.TabIndex = 10;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.White;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Location = new System.Drawing.Point(362, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 22);
            this.button3.TabIndex = 9;
            this.button3.Text = "search";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // linkLabelClearStatsSearch
            // 
            this.linkLabelClearStatsSearch.AutoSize = true;
            this.linkLabelClearStatsSearch.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelClearStatsSearch.ForeColor = System.Drawing.Color.Red;
            this.linkLabelClearStatsSearch.LinkColor = System.Drawing.Color.Red;
            this.linkLabelClearStatsSearch.Location = new System.Drawing.Point(444, 5);
            this.linkLabelClearStatsSearch.Name = "linkLabelClearStatsSearch";
            this.linkLabelClearStatsSearch.Size = new System.Drawing.Size(42, 13);
            this.linkLabelClearStatsSearch.TabIndex = 8;
            this.linkLabelClearStatsSearch.TabStop = true;
            this.linkLabelClearStatsSearch.Text = "Clear";
            this.linkLabelClearStatsSearch.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // textBoxSearchStats
            // 
            this.textBoxSearchStats.Location = new System.Drawing.Point(89, 1);
            this.textBoxSearchStats.Name = "textBoxSearchStats";
            this.textBoxSearchStats.Size = new System.Drawing.Size(267, 20);
            this.textBoxSearchStats.TabIndex = 1;
            this.textBoxSearchStats.TextChanged += new System.EventHandler(this.textBox7_TextChanged);
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label65.ForeColor = System.Drawing.Color.White;
            this.label65.Location = new System.Drawing.Point(14, 5);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(56, 13);
            this.label65.TabIndex = 0;
            this.label65.Text = "Search:";
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.tabControl3);
            this.tabPage8.Location = new System.Drawing.Point(4, 25);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(1115, 874);
            this.tabPage8.TabIndex = 8;
            this.tabPage8.Text = "Dashboards";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage11);
            this.tabControl3.Controls.Add(this.tabPage1);
            this.tabControl3.Controls.Add(this.tabPage5);
            this.tabControl3.Controls.Add(this.tabPage4);
            this.tabControl3.Controls.Add(this.tabPage9);
            this.tabControl3.Controls.Add(this.tabPage10);
            this.tabControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl3.Location = new System.Drawing.Point(3, 3);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(1109, 868);
            this.tabControl3.TabIndex = 0;
            // 
            // tabPage11
            // 
            this.tabPage11.BackColor = System.Drawing.Color.Black;
            this.tabPage11.Controls.Add(this.tableLayoutPanel24);
            this.tabPage11.Location = new System.Drawing.Point(4, 22);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage11.Size = new System.Drawing.Size(1101, 842);
            this.tabPage11.TabIndex = 5;
            this.tabPage11.Text = "Activity Overview";
            // 
            // tableLayoutPanel24
            // 
            this.tableLayoutPanel24.ColumnCount = 3;
            this.tableLayoutPanel24.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.9259259F));
            this.tableLayoutPanel24.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 99.07407F));
            this.tableLayoutPanel24.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.tableLayoutPanel24.Controls.Add(this.chartGlobalDashboard, 1, 1);
            this.tableLayoutPanel24.Controls.Add(this.label89, 1, 0);
            this.tableLayoutPanel24.Controls.Add(this.listView1, 1, 3);
            this.tableLayoutPanel24.Controls.Add(this.panel8, 1, 2);
            this.tableLayoutPanel24.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel24.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel24.Name = "tableLayoutPanel24";
            this.tableLayoutPanel24.RowCount = 4;
            this.tableLayoutPanel24.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel24.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel24.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel24.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 287F));
            this.tableLayoutPanel24.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel24.TabIndex = 0;
            // 
            // chartGlobalDashboard
            // 
            chartArea2.Name = "ChartArea1";
            this.chartGlobalDashboard.ChartAreas.Add(chartArea2);
            this.chartGlobalDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chartGlobalDashboard.Legends.Add(legend2);
            this.chartGlobalDashboard.Location = new System.Drawing.Point(12, 33);
            this.chartGlobalDashboard.Name = "chartGlobalDashboard";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartGlobalDashboard.Series.Add(series2);
            this.chartGlobalDashboard.Size = new System.Drawing.Size(965, 486);
            this.chartGlobalDashboard.TabIndex = 0;
            this.chartGlobalDashboard.Text = "chart8";
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label89.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label89.ForeColor = System.Drawing.Color.White;
            this.label89.Location = new System.Drawing.Point(12, 0);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(965, 30);
            this.label89.TabIndex = 1;
            this.label89.Text = "Activity Time (Hours)";
            this.label89.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader19,
            this.columnHeader22,
            this.columnHeader20,
            this.columnHeader21});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 552);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(965, 281);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "Type";
            this.columnHeader19.Width = 154;
            // 
            // columnHeader22
            // 
            this.columnHeader22.Text = "count";
            this.columnHeader22.Width = 86;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "time";
            this.columnHeader20.Width = 130;
            // 
            // columnHeader21
            // 
            this.columnHeader21.Text = "percent";
            this.columnHeader21.Width = 137;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.checkBox1);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(12, 525);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(965, 21);
            this.panel8.TabIndex = 3;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(0, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(95, 21);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "show \'Hideout\'";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            this.tabPage1.Controls.Add(this.tableLayoutPanel10);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1101, 842);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Mapping";
            this.tabPage1.Enter += new System.EventHandler(this.tabPage1_Enter);
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.AutoScroll = true;
            this.tableLayoutPanel10.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel10.ColumnCount = 2;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel11, 0, 0);
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel12, 1, 0);
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel13, 0, 2);
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel14, 1, 2);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 3;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 345F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel10.TabIndex = 0;
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 1;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.Controls.Add(this.chartMapTierCount, 0, 1);
            this.tableLayoutPanel11.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 2;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.19469F));
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.80531F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(540, 339);
            this.tableLayoutPanel11.TabIndex = 0;
            // 
            // chartMapTierCount
            // 
            this.chartMapTierCount.BackColor = System.Drawing.Color.Black;
            chartArea3.BackColor = System.Drawing.Color.Black;
            chartArea3.Name = "ChartArea1";
            this.chartMapTierCount.ChartAreas.Add(chartArea3);
            this.chartMapTierCount.Dock = System.Windows.Forms.DockStyle.Fill;
            legend3.Name = "Legend1";
            this.chartMapTierCount.Legends.Add(legend3);
            this.chartMapTierCount.Location = new System.Drawing.Point(3, 23);
            this.chartMapTierCount.Name = "chartMapTierCount";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chartMapTierCount.Series.Add(series3);
            this.chartMapTierCount.Size = new System.Drawing.Size(534, 313);
            this.chartMapTierCount.TabIndex = 1;
            this.chartMapTierCount.Text = "chart2";
            title1.Name = "Map Tiers";
            this.chartMapTierCount.Titles.Add(title1);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(3, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(534, 20);
            this.label9.TabIndex = 2;
            this.label9.Text = "Map Tiers";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 1;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.Controls.Add(this.chartMapTierAvgTime, 0, 1);
            this.tableLayoutPanel12.Controls.Add(this.label18, 0, 0);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(551, 4);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 2;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.19469F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.80531F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(540, 339);
            this.tableLayoutPanel12.TabIndex = 3;
            // 
            // chartMapTierAvgTime
            // 
            this.chartMapTierAvgTime.BackColor = System.Drawing.Color.Black;
            chartArea4.BackColor = System.Drawing.Color.Black;
            chartArea4.Name = "ChartArea1";
            this.chartMapTierAvgTime.ChartAreas.Add(chartArea4);
            this.chartMapTierAvgTime.Dock = System.Windows.Forms.DockStyle.Fill;
            legend4.Name = "Legend1";
            this.chartMapTierAvgTime.Legends.Add(legend4);
            this.chartMapTierAvgTime.Location = new System.Drawing.Point(3, 23);
            this.chartMapTierAvgTime.Name = "chartMapTierAvgTime";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chartMapTierAvgTime.Series.Add(series4);
            this.chartMapTierAvgTime.Size = new System.Drawing.Size(534, 313);
            this.chartMapTierAvgTime.TabIndex = 1;
            this.chartMapTierAvgTime.Text = "chart3";
            title2.Name = "Map Tiers";
            this.chartMapTierAvgTime.Titles.Add(title2);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.White;
            this.label18.Location = new System.Drawing.Point(3, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(534, 20);
            this.label18.TabIndex = 2;
            this.label18.Text = "Average time per map/tier (minutes)";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel13
            // 
            this.tableLayoutPanel13.ColumnCount = 1;
            this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.Controls.Add(this.listViewTop10Maps, 0, 1);
            this.tableLayoutPanel13.Controls.Add(this.label29, 0, 0);
            this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel13.Location = new System.Drawing.Point(4, 361);
            this.tableLayoutPanel13.Name = "tableLayoutPanel13";
            this.tableLayoutPanel13.RowCount = 2;
            this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.602151F));
            this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.39785F));
            this.tableLayoutPanel13.Size = new System.Drawing.Size(540, 471);
            this.tableLayoutPanel13.TabIndex = 4;
            // 
            // listViewTop10Maps
            // 
            this.listViewTop10Maps.BackColor = System.Drawing.Color.Black;
            this.listViewTop10Maps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader9});
            this.listViewTop10Maps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTop10Maps.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewTop10Maps.ForeColor = System.Drawing.Color.White;
            this.listViewTop10Maps.HideSelection = false;
            this.listViewTop10Maps.Location = new System.Drawing.Point(3, 43);
            this.listViewTop10Maps.Name = "listViewTop10Maps";
            this.listViewTop10Maps.Size = new System.Drawing.Size(534, 425);
            this.listViewTop10Maps.TabIndex = 0;
            this.listViewTop10Maps.UseCompatibleStateImageBehavior = false;
            this.listViewTop10Maps.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Map";
            this.columnHeader8.Width = 159;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Count";
            this.columnHeader9.Width = 174;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label29.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.ForeColor = System.Drawing.Color.White;
            this.label29.Location = new System.Drawing.Point(3, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(534, 40);
            this.label29.TabIndex = 1;
            this.label29.Text = "Top 10 Maps";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel14
            // 
            this.tableLayoutPanel14.ColumnCount = 1;
            this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel14.Controls.Add(this.listViewTaggingOverview, 0, 1);
            this.tableLayoutPanel14.Controls.Add(this.label33, 0, 0);
            this.tableLayoutPanel14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel14.Location = new System.Drawing.Point(551, 361);
            this.tableLayoutPanel14.Name = "tableLayoutPanel14";
            this.tableLayoutPanel14.RowCount = 2;
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.602151F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.39785F));
            this.tableLayoutPanel14.Size = new System.Drawing.Size(540, 471);
            this.tableLayoutPanel14.TabIndex = 5;
            // 
            // listViewTaggingOverview
            // 
            this.listViewTaggingOverview.BackColor = System.Drawing.Color.Black;
            this.listViewTaggingOverview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10,
            this.columnHeader11});
            this.listViewTaggingOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTaggingOverview.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewTaggingOverview.ForeColor = System.Drawing.Color.White;
            this.listViewTaggingOverview.HideSelection = false;
            this.listViewTaggingOverview.Location = new System.Drawing.Point(3, 43);
            this.listViewTaggingOverview.Name = "listViewTaggingOverview";
            this.listViewTaggingOverview.Size = new System.Drawing.Size(534, 425);
            this.listViewTaggingOverview.TabIndex = 0;
            this.listViewTaggingOverview.UseCompatibleStateImageBehavior = false;
            this.listViewTaggingOverview.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Tag";
            this.columnHeader10.Width = 159;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Count";
            this.columnHeader11.Width = 174;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label33.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.ForeColor = System.Drawing.Color.White;
            this.label33.Location = new System.Drawing.Point(3, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(534, 40);
            this.label33.TabIndex = 1;
            this.label33.Text = "Tagging Overview";
            this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.Black;
            this.tabPage5.Controls.Add(this.tableLayoutPanel15);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1101, 842);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Lab";
            // 
            // tableLayoutPanel15
            // 
            this.tableLayoutPanel15.ColumnCount = 2;
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel15.Controls.Add(this.tableLayoutPanel17, 1, 0);
            this.tableLayoutPanel15.Controls.Add(this.tableLayoutPanel16, 0, 0);
            this.tableLayoutPanel15.Controls.Add(this.tableLayoutPanel18, 0, 2);
            this.tableLayoutPanel15.Controls.Add(this.panel17, 0, 1);
            this.tableLayoutPanel15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel15.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel15.Name = "tableLayoutPanel15";
            this.tableLayoutPanel15.RowCount = 4;
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 332F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 270F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel15.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel15.TabIndex = 0;
            // 
            // tableLayoutPanel17
            // 
            this.tableLayoutPanel17.ColumnCount = 1;
            this.tableLayoutPanel17.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel17.Controls.Add(this.chartLabsAvgTime, 0, 1);
            this.tableLayoutPanel17.Controls.Add(this.label50, 0, 0);
            this.tableLayoutPanel17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel17.Location = new System.Drawing.Point(550, 3);
            this.tableLayoutPanel17.Name = "tableLayoutPanel17";
            this.tableLayoutPanel17.RowCount = 2;
            this.tableLayoutPanel17.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.895705F));
            this.tableLayoutPanel17.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.10429F));
            this.tableLayoutPanel17.Size = new System.Drawing.Size(542, 326);
            this.tableLayoutPanel17.TabIndex = 3;
            // 
            // chartLabsAvgTime
            // 
            this.chartLabsAvgTime.BackColor = System.Drawing.Color.Black;
            chartArea5.BackColor = System.Drawing.Color.Black;
            chartArea5.Name = "ChartArea1";
            this.chartLabsAvgTime.ChartAreas.Add(chartArea5);
            this.chartLabsAvgTime.Dock = System.Windows.Forms.DockStyle.Fill;
            legend5.Name = "Legend1";
            this.chartLabsAvgTime.Legends.Add(legend5);
            this.chartLabsAvgTime.Location = new System.Drawing.Point(3, 32);
            this.chartLabsAvgTime.Name = "chartLabsAvgTime";
            series5.ChartArea = "ChartArea1";
            series5.IsValueShownAsLabel = true;
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            series5.SmartLabelStyle.AllowOutsidePlotArea = System.Windows.Forms.DataVisualization.Charting.LabelOutsidePlotAreaStyle.Yes;
            this.chartLabsAvgTime.Series.Add(series5);
            this.chartLabsAvgTime.Size = new System.Drawing.Size(536, 291);
            this.chartLabsAvgTime.TabIndex = 1;
            this.chartLabsAvgTime.Text = "chart5";
            title3.Name = "Map Tiers";
            this.chartLabsAvgTime.Titles.Add(title3);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label50.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label50.ForeColor = System.Drawing.Color.White;
            this.label50.Location = new System.Drawing.Point(3, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(536, 29);
            this.label50.TabIndex = 2;
            this.label50.Text = "Avg. time per successful  lab (minutes)";
            this.label50.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel16
            // 
            this.tableLayoutPanel16.ColumnCount = 1;
            this.tableLayoutPanel16.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel16.Controls.Add(this.chartLabsDone, 0, 1);
            this.tableLayoutPanel16.Controls.Add(this.label25, 0, 0);
            this.tableLayoutPanel16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel16.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel16.Name = "tableLayoutPanel16";
            this.tableLayoutPanel16.RowCount = 2;
            this.tableLayoutPanel16.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.895705F));
            this.tableLayoutPanel16.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.10429F));
            this.tableLayoutPanel16.Size = new System.Drawing.Size(541, 326);
            this.tableLayoutPanel16.TabIndex = 1;
            // 
            // chartLabsDone
            // 
            this.chartLabsDone.BackColor = System.Drawing.Color.Black;
            chartArea6.BackColor = System.Drawing.Color.Black;
            chartArea6.Name = "ChartArea1";
            this.chartLabsDone.ChartAreas.Add(chartArea6);
            this.chartLabsDone.Dock = System.Windows.Forms.DockStyle.Fill;
            legend6.Name = "Legend1";
            this.chartLabsDone.Legends.Add(legend6);
            this.chartLabsDone.Location = new System.Drawing.Point(3, 32);
            this.chartLabsDone.Name = "chartLabsDone";
            series6.ChartArea = "ChartArea1";
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            this.chartLabsDone.Series.Add(series6);
            this.chartLabsDone.Size = new System.Drawing.Size(535, 291);
            this.chartLabsDone.TabIndex = 1;
            this.chartLabsDone.Text = "chart4";
            title4.Name = "Map Tiers";
            this.chartLabsDone.Titles.Add(title4);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label25.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.ForeColor = System.Drawing.Color.White;
            this.label25.Location = new System.Drawing.Point(3, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(535, 29);
            this.label25.TabIndex = 2;
            this.label25.Text = "Successful labs done by type";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel18
            // 
            this.tableLayoutPanel18.ColumnCount = 1;
            this.tableLayoutPanel18.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel18.Controls.Add(this.listViewBestLabs, 0, 1);
            this.tableLayoutPanel18.Controls.Add(this.label84, 0, 0);
            this.tableLayoutPanel18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel18.Location = new System.Drawing.Point(3, 360);
            this.tableLayoutPanel18.Name = "tableLayoutPanel18";
            this.tableLayoutPanel18.RowCount = 2;
            this.tableLayoutPanel18.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.91228F));
            this.tableLayoutPanel18.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.08772F));
            this.tableLayoutPanel18.Size = new System.Drawing.Size(541, 264);
            this.tableLayoutPanel18.TabIndex = 4;
            // 
            // listViewBestLabs
            // 
            this.listViewBestLabs.BackColor = System.Drawing.Color.Black;
            this.listViewBestLabs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.listViewBestLabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewBestLabs.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewBestLabs.ForeColor = System.Drawing.Color.White;
            this.listViewBestLabs.HideSelection = false;
            this.listViewBestLabs.Location = new System.Drawing.Point(3, 42);
            this.listViewBestLabs.Name = "listViewBestLabs";
            this.listViewBestLabs.Size = new System.Drawing.Size(535, 219);
            this.listViewBestLabs.TabIndex = 3;
            this.listViewBestLabs.UseCompatibleStateImageBehavior = false;
            this.listViewBestLabs.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Lab Type";
            this.columnHeader12.Width = 159;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Time";
            this.columnHeader13.Width = 174;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Timestamp";
            this.columnHeader14.Width = 213;
            // 
            // label84
            // 
            this.label84.AutoSize = true;
            this.label84.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label84.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label84.ForeColor = System.Drawing.Color.White;
            this.label84.Location = new System.Drawing.Point(3, 0);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(535, 39);
            this.label84.TabIndex = 2;
            this.label84.Text = "Best lab times";
            this.label84.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel17
            // 
            this.panel17.Controls.Add(this.label49);
            this.panel17.Controls.Add(this.checkBoxLabHideUnknown);
            this.panel17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel17.Location = new System.Drawing.Point(3, 335);
            this.panel17.Name = "panel17";
            this.panel17.Size = new System.Drawing.Size(541, 19);
            this.panel17.TabIndex = 5;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.ForeColor = System.Drawing.Color.White;
            this.label49.Location = new System.Drawing.Point(7, 1);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(268, 13);
            this.label49.TabIndex = 5;
            this.label49.Text = "* area levels are not trackable before 23.06.2021 (3.15)";
            // 
            // checkBoxLabHideUnknown
            // 
            this.checkBoxLabHideUnknown.AutoSize = true;
            this.checkBoxLabHideUnknown.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxLabHideUnknown.ForeColor = System.Drawing.Color.White;
            this.checkBoxLabHideUnknown.Location = new System.Drawing.Point(414, -1);
            this.checkBoxLabHideUnknown.Name = "checkBoxLabHideUnknown";
            this.checkBoxLabHideUnknown.Size = new System.Drawing.Size(124, 17);
            this.checkBoxLabHideUnknown.TabIndex = 4;
            this.checkBoxLabHideUnknown.Text = "hide \'Unknown\'";
            this.checkBoxLabHideUnknown.UseVisualStyleBackColor = true;
            this.checkBoxLabHideUnknown.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.Black;
            this.tabPage4.Controls.Add(this.tableLayoutPanel19);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1101, 842);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Heist";
            // 
            // tableLayoutPanel19
            // 
            this.tableLayoutPanel19.ColumnCount = 2;
            this.tableLayoutPanel19.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.10616F));
            this.tableLayoutPanel19.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.89384F));
            this.tableLayoutPanel19.Controls.Add(this.tableLayoutPanel20, 0, 0);
            this.tableLayoutPanel19.Controls.Add(this.tableLayoutPanel21, 0, 2);
            this.tableLayoutPanel19.Controls.Add(this.tableLayoutPanel22, 1, 2);
            this.tableLayoutPanel19.Controls.Add(this.tableLayoutPanel23, 1, 0);
            this.tableLayoutPanel19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel19.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel19.Name = "tableLayoutPanel19";
            this.tableLayoutPanel19.RowCount = 4;
            this.tableLayoutPanel19.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel19.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel19.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            this.tableLayoutPanel19.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel19.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel19.TabIndex = 0;
            // 
            // tableLayoutPanel20
            // 
            this.tableLayoutPanel20.ColumnCount = 1;
            this.tableLayoutPanel20.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel20.Controls.Add(this.chartHeistByLevel, 0, 1);
            this.tableLayoutPanel20.Controls.Add(this.label85, 0, 0);
            this.tableLayoutPanel20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel20.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel20.Name = "tableLayoutPanel20";
            this.tableLayoutPanel20.RowCount = 2;
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.91228F));
            this.tableLayoutPanel20.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.08772F));
            this.tableLayoutPanel20.Size = new System.Drawing.Size(542, 294);
            this.tableLayoutPanel20.TabIndex = 2;
            // 
            // chartHeistByLevel
            // 
            this.chartHeistByLevel.BackColor = System.Drawing.Color.Black;
            chartArea7.BackColor = System.Drawing.Color.Black;
            chartArea7.Name = "ChartArea1";
            this.chartHeistByLevel.ChartAreas.Add(chartArea7);
            this.chartHeistByLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            legend7.Name = "Legend1";
            this.chartHeistByLevel.Legends.Add(legend7);
            this.chartHeistByLevel.Location = new System.Drawing.Point(3, 46);
            this.chartHeistByLevel.Name = "chartHeistByLevel";
            series7.ChartArea = "ChartArea1";
            series7.Legend = "Legend1";
            series7.Name = "Series1";
            this.chartHeistByLevel.Series.Add(series7);
            this.chartHeistByLevel.Size = new System.Drawing.Size(536, 245);
            this.chartHeistByLevel.TabIndex = 1;
            this.chartHeistByLevel.Text = "chart6";
            title5.Name = "Map Tiers";
            this.chartHeistByLevel.Titles.Add(title5);
            // 
            // label85
            // 
            this.label85.AutoSize = true;
            this.label85.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label85.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label85.ForeColor = System.Drawing.Color.White;
            this.label85.Location = new System.Drawing.Point(3, 0);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(536, 43);
            this.label85.TabIndex = 2;
            this.label85.Text = "Heists done by level";
            this.label85.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel21
            // 
            this.tableLayoutPanel21.ColumnCount = 1;
            this.tableLayoutPanel21.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel21.Controls.Add(this.listView4, 0, 1);
            this.tableLayoutPanel21.Controls.Add(this.label86, 0, 0);
            this.tableLayoutPanel21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel21.Location = new System.Drawing.Point(3, 328);
            this.tableLayoutPanel21.Name = "tableLayoutPanel21";
            this.tableLayoutPanel21.RowCount = 2;
            this.tableLayoutPanel21.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.602151F));
            this.tableLayoutPanel21.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.39785F));
            this.tableLayoutPanel21.Size = new System.Drawing.Size(542, 354);
            this.tableLayoutPanel21.TabIndex = 5;
            // 
            // listView4
            // 
            this.listView4.BackColor = System.Drawing.Color.Black;
            this.listView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader15,
            this.columnHeader16});
            this.listView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView4.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView4.ForeColor = System.Drawing.Color.White;
            this.listView4.HideSelection = false;
            this.listView4.Location = new System.Drawing.Point(3, 33);
            this.listView4.Name = "listView4";
            this.listView4.Size = new System.Drawing.Size(536, 318);
            this.listView4.TabIndex = 0;
            this.listView4.UseCompatibleStateImageBehavior = false;
            this.listView4.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Area";
            this.columnHeader15.Width = 159;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Count";
            this.columnHeader16.Width = 174;
            // 
            // label86
            // 
            this.label86.AutoSize = true;
            this.label86.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label86.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label86.ForeColor = System.Drawing.Color.White;
            this.label86.Location = new System.Drawing.Point(3, 0);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(536, 30);
            this.label86.TabIndex = 1;
            this.label86.Text = "Heists done by area";
            this.label86.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel22
            // 
            this.tableLayoutPanel22.ColumnCount = 1;
            this.tableLayoutPanel22.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel22.Controls.Add(this.listView5, 0, 1);
            this.tableLayoutPanel22.Controls.Add(this.label87, 0, 0);
            this.tableLayoutPanel22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel22.Location = new System.Drawing.Point(551, 328);
            this.tableLayoutPanel22.Name = "tableLayoutPanel22";
            this.tableLayoutPanel22.RowCount = 2;
            this.tableLayoutPanel22.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.602151F));
            this.tableLayoutPanel22.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.39785F));
            this.tableLayoutPanel22.Size = new System.Drawing.Size(541, 354);
            this.tableLayoutPanel22.TabIndex = 6;
            // 
            // listView5
            // 
            this.listView5.BackColor = System.Drawing.Color.Black;
            this.listView5.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader17,
            this.columnHeader18});
            this.listView5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView5.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView5.ForeColor = System.Drawing.Color.White;
            this.listView5.HideSelection = false;
            this.listView5.Location = new System.Drawing.Point(3, 33);
            this.listView5.Name = "listView5";
            this.listView5.Size = new System.Drawing.Size(535, 318);
            this.listView5.TabIndex = 0;
            this.listView5.UseCompatibleStateImageBehavior = false;
            this.listView5.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "Tag";
            this.columnHeader17.Width = 159;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "Count";
            this.columnHeader18.Width = 174;
            // 
            // label87
            // 
            this.label87.AutoSize = true;
            this.label87.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label87.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label87.ForeColor = System.Drawing.Color.White;
            this.label87.Location = new System.Drawing.Point(3, 0);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(535, 30);
            this.label87.TabIndex = 1;
            this.label87.Text = "Tagging overview";
            this.label87.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel23
            // 
            this.tableLayoutPanel23.ColumnCount = 1;
            this.tableLayoutPanel23.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel23.Controls.Add(this.chartHeistAvgTime, 0, 1);
            this.tableLayoutPanel23.Controls.Add(this.label88, 0, 0);
            this.tableLayoutPanel23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel23.Location = new System.Drawing.Point(551, 3);
            this.tableLayoutPanel23.Name = "tableLayoutPanel23";
            this.tableLayoutPanel23.RowCount = 2;
            this.tableLayoutPanel23.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.91228F));
            this.tableLayoutPanel23.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.08772F));
            this.tableLayoutPanel23.Size = new System.Drawing.Size(541, 294);
            this.tableLayoutPanel23.TabIndex = 7;
            // 
            // chartHeistAvgTime
            // 
            this.chartHeistAvgTime.BackColor = System.Drawing.Color.Black;
            chartArea8.BackColor = System.Drawing.Color.Black;
            chartArea8.Name = "ChartArea1";
            this.chartHeistAvgTime.ChartAreas.Add(chartArea8);
            this.chartHeistAvgTime.Dock = System.Windows.Forms.DockStyle.Fill;
            legend8.Name = "Legend1";
            this.chartHeistAvgTime.Legends.Add(legend8);
            this.chartHeistAvgTime.Location = new System.Drawing.Point(3, 46);
            this.chartHeistAvgTime.Name = "chartHeistAvgTime";
            series8.ChartArea = "ChartArea1";
            series8.Legend = "Legend1";
            series8.Name = "Series1";
            this.chartHeistAvgTime.Series.Add(series8);
            this.chartHeistAvgTime.Size = new System.Drawing.Size(535, 245);
            this.chartHeistAvgTime.TabIndex = 1;
            this.chartHeistAvgTime.Text = "chart7";
            title6.Name = "Map Tiers";
            this.chartHeistAvgTime.Titles.Add(title6);
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label88.Font = new System.Drawing.Font("Noto Mono", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label88.ForeColor = System.Drawing.Color.White;
            this.label88.Location = new System.Drawing.Point(3, 0);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(535, 43);
            this.label88.TabIndex = 2;
            this.label88.Text = "Average time per heist/level (minutes)";
            this.label88.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage9
            // 
            this.tabPage9.BackColor = System.Drawing.Color.Black;
            this.tabPage9.Controls.Add(this.tableLayoutPanel8);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(1101, 842);
            this.tabPage9.TabIndex = 0;
            this.tabPage9.Text = "Bossing";
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.AutoScroll = true;
            this.tableLayoutPanel8.ColumnCount = 4;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.panelBossElder, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.panelBossesShaper, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.panelBossesMaven, 2, 0);
            this.tableLayoutPanel8.Controls.Add(this.panelBossesTrialmaster, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.panel9, 1, 1);
            this.tableLayoutPanel8.Controls.Add(this.panel10, 2, 1);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 3;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 342F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 336F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel8.TabIndex = 0;
            // 
            // panelBossElder
            // 
            this.panelBossElder.Controls.Add(this.labelElderTried);
            this.panelBossElder.Controls.Add(this.label92);
            this.panelBossElder.Controls.Add(this.pictureBox1);
            this.panelBossElder.Controls.Add(this.label10);
            this.panelBossElder.Controls.Add(this.labelElderKillCount);
            this.panelBossElder.Controls.Add(this.label8);
            this.panelBossElder.Controls.Add(this.labelElderStatus);
            this.panelBossElder.Controls.Add(this.label6);
            this.panelBossElder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBossElder.ForeColor = System.Drawing.Color.White;
            this.panelBossElder.Location = new System.Drawing.Point(3, 3);
            this.panelBossElder.Name = "panelBossElder";
            this.panelBossElder.Size = new System.Drawing.Size(294, 336);
            this.panelBossElder.TabIndex = 0;
            // 
            // labelElderTried
            // 
            this.labelElderTried.AutoSize = true;
            this.labelElderTried.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelElderTried.ForeColor = System.Drawing.Color.White;
            this.labelElderTried.Location = new System.Drawing.Point(146, 296);
            this.labelElderTried.Name = "labelElderTried";
            this.labelElderTried.Size = new System.Drawing.Size(21, 13);
            this.labelElderTried.TabIndex = 88;
            this.labelElderTried.Text = "8x";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label92.ForeColor = System.Drawing.Color.White;
            this.label92.Location = new System.Drawing.Point(78, 296);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(49, 13);
            this.label92.TabIndex = 87;
            this.label92.Text = "Tried:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 41);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(289, 220);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(91, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(138, 28);
            this.label10.TabIndex = 12;
            this.label10.Text = "The Elder";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelElderKillCount
            // 
            this.labelElderKillCount.AutoSize = true;
            this.labelElderKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelElderKillCount.ForeColor = System.Drawing.Color.White;
            this.labelElderKillCount.Location = new System.Drawing.Point(146, 280);
            this.labelElderKillCount.Name = "labelElderKillCount";
            this.labelElderKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelElderKillCount.TabIndex = 11;
            this.labelElderKillCount.Text = "8x";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(79, 280);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Killed:";
            // 
            // labelElderStatus
            // 
            this.labelElderStatus.AutoSize = true;
            this.labelElderStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelElderStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelElderStatus.Location = new System.Drawing.Point(146, 264);
            this.labelElderStatus.Name = "labelElderStatus";
            this.labelElderStatus.Size = new System.Drawing.Size(28, 13);
            this.labelElderStatus.TabIndex = 9;
            this.labelElderStatus.Text = "Yes";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(79, 264);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Done:";
            // 
            // panelBossesShaper
            // 
            this.panelBossesShaper.Controls.Add(this.labelShaperTried);
            this.panelBossesShaper.Controls.Add(this.label93);
            this.panelBossesShaper.Controls.Add(this.labelShaperKillCount);
            this.panelBossesShaper.Controls.Add(this.label13);
            this.panelBossesShaper.Controls.Add(this.labelShaperStatus);
            this.panelBossesShaper.Controls.Add(this.label11);
            this.panelBossesShaper.Controls.Add(this.label15);
            this.panelBossesShaper.Controls.Add(this.pictureBox2);
            this.panelBossesShaper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBossesShaper.Location = new System.Drawing.Point(303, 3);
            this.panelBossesShaper.Name = "panelBossesShaper";
            this.panelBossesShaper.Size = new System.Drawing.Size(294, 336);
            this.panelBossesShaper.TabIndex = 1;
            // 
            // labelShaperTried
            // 
            this.labelShaperTried.AutoSize = true;
            this.labelShaperTried.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelShaperTried.ForeColor = System.Drawing.Color.White;
            this.labelShaperTried.Location = new System.Drawing.Point(147, 296);
            this.labelShaperTried.Name = "labelShaperTried";
            this.labelShaperTried.Size = new System.Drawing.Size(21, 13);
            this.labelShaperTried.TabIndex = 90;
            this.labelShaperTried.Text = "8x";
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label93.ForeColor = System.Drawing.Color.White;
            this.label93.Location = new System.Drawing.Point(80, 296);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(49, 13);
            this.label93.TabIndex = 89;
            this.label93.Text = "Tried:";
            // 
            // labelShaperKillCount
            // 
            this.labelShaperKillCount.AutoSize = true;
            this.labelShaperKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelShaperKillCount.ForeColor = System.Drawing.Color.White;
            this.labelShaperKillCount.Location = new System.Drawing.Point(147, 280);
            this.labelShaperKillCount.Name = "labelShaperKillCount";
            this.labelShaperKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelShaperKillCount.TabIndex = 17;
            this.labelShaperKillCount.Text = "8x";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.White;
            this.label13.Location = new System.Drawing.Point(81, 280);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(56, 13);
            this.label13.TabIndex = 16;
            this.label13.Text = "Killed:";
            // 
            // labelShaperStatus
            // 
            this.labelShaperStatus.AutoSize = true;
            this.labelShaperStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelShaperStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelShaperStatus.Location = new System.Drawing.Point(147, 264);
            this.labelShaperStatus.Name = "labelShaperStatus";
            this.labelShaperStatus.Size = new System.Drawing.Size(28, 13);
            this.labelShaperStatus.TabIndex = 15;
            this.labelShaperStatus.Text = "Yes";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(79, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(152, 28);
            this.label11.TabIndex = 18;
            this.label11.Text = "The Shaper";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.White;
            this.label15.Location = new System.Drawing.Point(80, 264);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(42, 13);
            this.label15.TabIndex = 14;
            this.label15.Text = "Done:";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(8, 41);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(283, 220);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 13;
            this.pictureBox2.TabStop = false;
            // 
            // panelBossesMaven
            // 
            this.panelBossesMaven.Controls.Add(this.labelMavenTried);
            this.panelBossesMaven.Controls.Add(this.label35);
            this.panelBossesMaven.Controls.Add(this.pictureBox8);
            this.panelBossesMaven.Controls.Add(this.label43);
            this.panelBossesMaven.Controls.Add(this.label39);
            this.panelBossesMaven.Controls.Add(this.labelMavenStatus);
            this.panelBossesMaven.Controls.Add(this.labelMavenKilled);
            this.panelBossesMaven.Controls.Add(this.label37);
            this.panelBossesMaven.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBossesMaven.ForeColor = System.Drawing.Color.White;
            this.panelBossesMaven.Location = new System.Drawing.Point(603, 3);
            this.panelBossesMaven.Name = "panelBossesMaven";
            this.panelBossesMaven.Size = new System.Drawing.Size(294, 336);
            this.panelBossesMaven.TabIndex = 2;
            // 
            // labelMavenTried
            // 
            this.labelMavenTried.AutoSize = true;
            this.labelMavenTried.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMavenTried.ForeColor = System.Drawing.Color.White;
            this.labelMavenTried.Location = new System.Drawing.Point(151, 296);
            this.labelMavenTried.Name = "labelMavenTried";
            this.labelMavenTried.Size = new System.Drawing.Size(21, 13);
            this.labelMavenTried.TabIndex = 86;
            this.labelMavenTried.Text = "8x";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.ForeColor = System.Drawing.Color.White;
            this.label35.Location = new System.Drawing.Point(79, 9);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(138, 28);
            this.label35.TabIndex = 84;
            this.label35.Text = "The Maven";
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox8.Image")));
            this.pictureBox8.Location = new System.Drawing.Point(2, 41);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(289, 220);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox8.TabIndex = 79;
            this.pictureBox8.TabStop = false;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label43.ForeColor = System.Drawing.Color.White;
            this.label43.Location = new System.Drawing.Point(82, 296);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(49, 13);
            this.label43.TabIndex = 85;
            this.label43.Text = "Tried:";
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label39.ForeColor = System.Drawing.Color.White;
            this.label39.Location = new System.Drawing.Point(80, 264);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(42, 13);
            this.label39.TabIndex = 80;
            this.label39.Text = "Done:";
            // 
            // labelMavenStatus
            // 
            this.labelMavenStatus.AutoSize = true;
            this.labelMavenStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMavenStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelMavenStatus.Location = new System.Drawing.Point(151, 264);
            this.labelMavenStatus.Name = "labelMavenStatus";
            this.labelMavenStatus.Size = new System.Drawing.Size(28, 13);
            this.labelMavenStatus.TabIndex = 81;
            this.labelMavenStatus.Text = "Yes";
            // 
            // labelMavenKilled
            // 
            this.labelMavenKilled.AutoSize = true;
            this.labelMavenKilled.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMavenKilled.ForeColor = System.Drawing.Color.White;
            this.labelMavenKilled.Location = new System.Drawing.Point(151, 280);
            this.labelMavenKilled.Name = "labelMavenKilled";
            this.labelMavenKilled.Size = new System.Drawing.Size(21, 13);
            this.labelMavenKilled.TabIndex = 83;
            this.labelMavenKilled.Text = "8x";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label37.ForeColor = System.Drawing.Color.White;
            this.label37.Location = new System.Drawing.Point(81, 280);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(56, 13);
            this.label37.TabIndex = 82;
            this.label37.Text = "Killed:";
            // 
            // panelBossesTrialmaster
            // 
            this.panelBossesTrialmaster.Controls.Add(this.labelTrialMasterTried);
            this.panelBossesTrialmaster.Controls.Add(this.label41);
            this.panelBossesTrialmaster.Controls.Add(this.label40);
            this.panelBossesTrialmaster.Controls.Add(this.labelTrialMasterKilled);
            this.panelBossesTrialmaster.Controls.Add(this.label42);
            this.panelBossesTrialmaster.Controls.Add(this.labelTrialMasterStatus);
            this.panelBossesTrialmaster.Controls.Add(this.label44);
            this.panelBossesTrialmaster.Controls.Add(this.pictureBox9);
            this.panelBossesTrialmaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBossesTrialmaster.Location = new System.Drawing.Point(3, 345);
            this.panelBossesTrialmaster.Name = "panelBossesTrialmaster";
            this.panelBossesTrialmaster.Size = new System.Drawing.Size(294, 330);
            this.panelBossesTrialmaster.TabIndex = 3;
            // 
            // labelTrialMasterTried
            // 
            this.labelTrialMasterTried.AutoSize = true;
            this.labelTrialMasterTried.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrialMasterTried.ForeColor = System.Drawing.Color.White;
            this.labelTrialMasterTried.Location = new System.Drawing.Point(146, 289);
            this.labelTrialMasterTried.Name = "labelTrialMasterTried";
            this.labelTrialMasterTried.Size = new System.Drawing.Size(21, 13);
            this.labelTrialMasterTried.TabIndex = 84;
            this.labelTrialMasterTried.Text = "8x";
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label41.ForeColor = System.Drawing.Color.White;
            this.label41.Location = new System.Drawing.Point(75, 289);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(49, 13);
            this.label41.TabIndex = 83;
            this.label41.Text = "Tried:";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label40.ForeColor = System.Drawing.Color.White;
            this.label40.Location = new System.Drawing.Point(43, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(222, 28);
            this.label40.TabIndex = 82;
            this.label40.Text = "The Trialmaster";
            // 
            // labelTrialMasterKilled
            // 
            this.labelTrialMasterKilled.AutoSize = true;
            this.labelTrialMasterKilled.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrialMasterKilled.ForeColor = System.Drawing.Color.White;
            this.labelTrialMasterKilled.Location = new System.Drawing.Point(146, 273);
            this.labelTrialMasterKilled.Name = "labelTrialMasterKilled";
            this.labelTrialMasterKilled.Size = new System.Drawing.Size(21, 13);
            this.labelTrialMasterKilled.TabIndex = 81;
            this.labelTrialMasterKilled.Text = "8x";
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label42.ForeColor = System.Drawing.Color.White;
            this.label42.Location = new System.Drawing.Point(75, 273);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(56, 13);
            this.label42.TabIndex = 80;
            this.label42.Text = "Killed:";
            // 
            // labelTrialMasterStatus
            // 
            this.labelTrialMasterStatus.AutoSize = true;
            this.labelTrialMasterStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTrialMasterStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelTrialMasterStatus.Location = new System.Drawing.Point(146, 257);
            this.labelTrialMasterStatus.Name = "labelTrialMasterStatus";
            this.labelTrialMasterStatus.Size = new System.Drawing.Size(28, 13);
            this.labelTrialMasterStatus.TabIndex = 79;
            this.labelTrialMasterStatus.Text = "Yes";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label44.ForeColor = System.Drawing.Color.White;
            this.label44.Location = new System.Drawing.Point(75, 257);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(42, 13);
            this.label44.TabIndex = 78;
            this.label44.Text = "Done:";
            // 
            // pictureBox9
            // 
            this.pictureBox9.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox9.Image")));
            this.pictureBox9.Location = new System.Drawing.Point(0, 32);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(289, 220);
            this.pictureBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox9.TabIndex = 77;
            this.pictureBox9.TabStop = false;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.label82);
            this.panel9.Controls.Add(this.label83);
            this.panel9.Controls.Add(this.label75);
            this.panel9.Controls.Add(this.label76);
            this.panel9.Controls.Add(this.label77);
            this.panel9.Controls.Add(this.label78);
            this.panel9.Controls.Add(this.label79);
            this.panel9.Controls.Add(this.label80);
            this.panel9.Controls.Add(this.label81);
            this.panel9.Controls.Add(this.pictureBox21);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.Location = new System.Drawing.Point(303, 345);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(294, 330);
            this.panel9.TabIndex = 4;
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label82.ForeColor = System.Drawing.Color.White;
            this.label82.Location = new System.Drawing.Point(162, 287);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(21, 13);
            this.label82.TabIndex = 90;
            this.label82.Text = "8x";
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label83.ForeColor = System.Drawing.Color.White;
            this.label83.Location = new System.Drawing.Point(95, 287);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(49, 13);
            this.label83.TabIndex = 89;
            this.label83.Text = "Tried:";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label75.ForeColor = System.Drawing.Color.White;
            this.label75.Location = new System.Drawing.Point(105, 374);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(56, 13);
            this.label75.TabIndex = 88;
            this.label75.Text = "8 times";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label76.ForeColor = System.Drawing.Color.White;
            this.label76.Location = new System.Drawing.Point(23, 374);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(49, 13);
            this.label76.TabIndex = 87;
            this.label76.Text = "Tried:";
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label77.ForeColor = System.Drawing.Color.White;
            this.label77.Location = new System.Drawing.Point(94, 0);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(124, 28);
            this.label77.TabIndex = 86;
            this.label77.Text = "Catarina";
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label78.ForeColor = System.Drawing.Color.White;
            this.label78.Location = new System.Drawing.Point(162, 271);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(21, 13);
            this.label78.TabIndex = 85;
            this.label78.Text = "8x";
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label79.ForeColor = System.Drawing.Color.White;
            this.label79.Location = new System.Drawing.Point(95, 271);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(56, 13);
            this.label79.TabIndex = 84;
            this.label79.Text = "Killed:";
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label80.ForeColor = System.Drawing.Color.Red;
            this.label80.Location = new System.Drawing.Point(163, 255);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(21, 13);
            this.label80.TabIndex = 83;
            this.label80.Text = "No";
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label81.ForeColor = System.Drawing.Color.White;
            this.label81.Location = new System.Drawing.Point(95, 255);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(42, 13);
            this.label81.TabIndex = 82;
            this.label81.Text = "Done:";
            // 
            // pictureBox21
            // 
            this.pictureBox21.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox21.Image")));
            this.pictureBox21.Location = new System.Drawing.Point(8, 32);
            this.pictureBox21.Name = "pictureBox21";
            this.pictureBox21.Size = new System.Drawing.Size(289, 220);
            this.pictureBox21.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox21.TabIndex = 81;
            this.pictureBox21.TabStop = false;
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.labelSirusTries);
            this.panel10.Controls.Add(this.label14);
            this.panel10.Controls.Add(this.labelSirusKillCount);
            this.panel10.Controls.Add(this.label12);
            this.panel10.Controls.Add(this.labelSirusStatus);
            this.panel10.Controls.Add(this.label16);
            this.panel10.Controls.Add(this.label7);
            this.panel10.Controls.Add(this.pictureBox4);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel10.Location = new System.Drawing.Point(603, 345);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(294, 330);
            this.panel10.TabIndex = 5;
            // 
            // labelSirusTries
            // 
            this.labelSirusTries.AutoSize = true;
            this.labelSirusTries.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSirusTries.ForeColor = System.Drawing.Color.White;
            this.labelSirusTries.Location = new System.Drawing.Point(151, 289);
            this.labelSirusTries.Name = "labelSirusTries";
            this.labelSirusTries.Size = new System.Drawing.Size(21, 13);
            this.labelSirusTries.TabIndex = 26;
            this.labelSirusTries.Text = "8x";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.White;
            this.label14.Location = new System.Drawing.Point(76, 287);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(49, 13);
            this.label14.TabIndex = 25;
            this.label14.Text = "Tried:";
            // 
            // labelSirusKillCount
            // 
            this.labelSirusKillCount.AutoSize = true;
            this.labelSirusKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSirusKillCount.ForeColor = System.Drawing.Color.White;
            this.labelSirusKillCount.Location = new System.Drawing.Point(151, 271);
            this.labelSirusKillCount.Name = "labelSirusKillCount";
            this.labelSirusKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelSirusKillCount.TabIndex = 24;
            this.labelSirusKillCount.Text = "8x";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(76, 271);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Killed:";
            // 
            // labelSirusStatus
            // 
            this.labelSirusStatus.AutoSize = true;
            this.labelSirusStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSirusStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelSirusStatus.Location = new System.Drawing.Point(151, 255);
            this.labelSirusStatus.Name = "labelSirusStatus";
            this.labelSirusStatus.Size = new System.Drawing.Size(28, 13);
            this.labelSirusStatus.TabIndex = 22;
            this.labelSirusStatus.Text = "Yes";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.White;
            this.label16.Location = new System.Drawing.Point(75, 255);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(42, 13);
            this.label16.TabIndex = 21;
            this.label16.Text = "Done:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(111, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 29);
            this.label7.TabIndex = 19;
            this.label7.Text = "Sirus";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(0, 32);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(289, 220);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 14;
            this.pictureBox4.TabStop = false;
            // 
            // tabPage10
            // 
            this.tabPage10.Controls.Add(this.tableLayoutPanel9);
            this.tabPage10.Location = new System.Drawing.Point(4, 22);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage10.Size = new System.Drawing.Size(1101, 842);
            this.tabPage10.TabIndex = 1;
            this.tabPage10.Text = "Conqueror";
            this.tabPage10.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.AutoScroll = true;
            this.tableLayoutPanel9.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel9.ColumnCount = 4;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.panel11, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.panel12, 1, 0);
            this.tableLayoutPanel9.Controls.Add(this.panel13, 2, 0);
            this.tableLayoutPanel9.Controls.Add(this.panel14, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.panel15, 1, 1);
            this.tableLayoutPanel9.Controls.Add(this.panel16, 2, 1);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 3;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 342F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 336F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // panel11
            // 
            this.panel11.Controls.Add(this.labelHunterTries);
            this.panel11.Controls.Add(this.label27);
            this.panel11.Controls.Add(this.labelHunterKillCount);
            this.panel11.Controls.Add(this.label32);
            this.panel11.Controls.Add(this.labelHunterStatus);
            this.panel11.Controls.Add(this.label34);
            this.panel11.Controls.Add(this.label45);
            this.panel11.Controls.Add(this.pictureBox7);
            this.panel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel11.ForeColor = System.Drawing.Color.White;
            this.panel11.Location = new System.Drawing.Point(3, 3);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(294, 336);
            this.panel11.TabIndex = 0;
            // 
            // labelHunterTries
            // 
            this.labelHunterTries.AutoSize = true;
            this.labelHunterTries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHunterTries.ForeColor = System.Drawing.Color.White;
            this.labelHunterTries.Location = new System.Drawing.Point(147, 297);
            this.labelHunterTries.Name = "labelHunterTries";
            this.labelHunterTries.Size = new System.Drawing.Size(18, 13);
            this.labelHunterTries.TabIndex = 58;
            this.labelHunterTries.Text = "8x";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.ForeColor = System.Drawing.Color.White;
            this.label27.Location = new System.Drawing.Point(80, 297);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(34, 13);
            this.label27.TabIndex = 57;
            this.label27.Text = "Tried:";
            // 
            // labelHunterKillCount
            // 
            this.labelHunterKillCount.AutoSize = true;
            this.labelHunterKillCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHunterKillCount.ForeColor = System.Drawing.Color.White;
            this.labelHunterKillCount.Location = new System.Drawing.Point(147, 281);
            this.labelHunterKillCount.Name = "labelHunterKillCount";
            this.labelHunterKillCount.Size = new System.Drawing.Size(18, 13);
            this.labelHunterKillCount.TabIndex = 56;
            this.labelHunterKillCount.Text = "8x";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.ForeColor = System.Drawing.Color.White;
            this.label32.Location = new System.Drawing.Point(81, 281);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(35, 13);
            this.label32.TabIndex = 55;
            this.label32.Text = "Killed:";
            // 
            // labelHunterStatus
            // 
            this.labelHunterStatus.AutoSize = true;
            this.labelHunterStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHunterStatus.ForeColor = System.Drawing.Color.Lime;
            this.labelHunterStatus.Location = new System.Drawing.Point(147, 265);
            this.labelHunterStatus.Name = "labelHunterStatus";
            this.labelHunterStatus.Size = new System.Drawing.Size(25, 13);
            this.labelHunterStatus.TabIndex = 54;
            this.labelHunterStatus.Text = "Yes";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.ForeColor = System.Drawing.Color.White;
            this.label34.Location = new System.Drawing.Point(80, 265);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(36, 13);
            this.label34.TabIndex = 53;
            this.label34.Text = "Done:";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label45.ForeColor = System.Drawing.Color.White;
            this.label45.Location = new System.Drawing.Point(78, 7);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(138, 28);
            this.label45.TabIndex = 51;
            this.label45.Text = "Al-Hezmin";
            // 
            // pictureBox7
            // 
            this.pictureBox7.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox7.Image")));
            this.pictureBox7.Location = new System.Drawing.Point(2, 42);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(289, 220);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox7.TabIndex = 46;
            this.pictureBox7.TabStop = false;
            // 
            // panel12
            // 
            this.panel12.Controls.Add(this.labelBaranTries);
            this.panel12.Controls.Add(this.pictureBox5);
            this.panel12.Controls.Add(this.label19);
            this.panel12.Controls.Add(this.label21);
            this.panel12.Controls.Add(this.labelBaranKillCount);
            this.panel12.Controls.Add(this.label26);
            this.panel12.Controls.Add(this.label24);
            this.panel12.Controls.Add(this.labelBaranStatus);
            this.panel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel12.Location = new System.Drawing.Point(303, 3);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(294, 336);
            this.panel12.TabIndex = 1;
            // 
            // labelBaranTries
            // 
            this.labelBaranTries.AutoSize = true;
            this.labelBaranTries.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBaranTries.ForeColor = System.Drawing.Color.White;
            this.labelBaranTries.Location = new System.Drawing.Point(167, 298);
            this.labelBaranTries.Name = "labelBaranTries";
            this.labelBaranTries.Size = new System.Drawing.Size(21, 13);
            this.labelBaranTries.TabIndex = 43;
            this.labelBaranTries.Text = "8x";
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox5.Image")));
            this.pictureBox5.Location = new System.Drawing.Point(5, 42);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(289, 220);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox5.TabIndex = 30;
            this.pictureBox5.TabStop = false;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.White;
            this.label19.Location = new System.Drawing.Point(91, 298);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(49, 13);
            this.label19.TabIndex = 42;
            this.label19.Text = "Tried:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.White;
            this.label21.Location = new System.Drawing.Point(113, 7);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(82, 28);
            this.label21.TabIndex = 41;
            this.label21.Text = "Baran";
            // 
            // labelBaranKillCount
            // 
            this.labelBaranKillCount.AutoSize = true;
            this.labelBaranKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBaranKillCount.ForeColor = System.Drawing.Color.White;
            this.labelBaranKillCount.Location = new System.Drawing.Point(167, 281);
            this.labelBaranKillCount.Name = "labelBaranKillCount";
            this.labelBaranKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelBaranKillCount.TabIndex = 40;
            this.labelBaranKillCount.Text = "8x";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.ForeColor = System.Drawing.Color.White;
            this.label26.Location = new System.Drawing.Point(91, 265);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(42, 13);
            this.label26.TabIndex = 37;
            this.label26.Text = "Done:";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.ForeColor = System.Drawing.Color.White;
            this.label24.Location = new System.Drawing.Point(91, 281);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(56, 13);
            this.label24.TabIndex = 39;
            this.label24.Text = "Killed:";
            // 
            // labelBaranStatus
            // 
            this.labelBaranStatus.AutoSize = true;
            this.labelBaranStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBaranStatus.ForeColor = System.Drawing.Color.White;
            this.labelBaranStatus.Location = new System.Drawing.Point(167, 266);
            this.labelBaranStatus.Name = "labelBaranStatus";
            this.labelBaranStatus.Size = new System.Drawing.Size(28, 13);
            this.labelBaranStatus.TabIndex = 38;
            this.labelBaranStatus.Text = "Yes";
            // 
            // panel13
            // 
            this.panel13.Controls.Add(this.labelDroxTries);
            this.panel13.Controls.Add(this.label23);
            this.panel13.Controls.Add(this.labelDroxKillCount);
            this.panel13.Controls.Add(this.label28);
            this.panel13.Controls.Add(this.labelDroxStatus);
            this.panel13.Controls.Add(this.label30);
            this.panel13.Controls.Add(this.label47);
            this.panel13.Controls.Add(this.pictureBox6);
            this.panel13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel13.ForeColor = System.Drawing.Color.White;
            this.panel13.Location = new System.Drawing.Point(603, 3);
            this.panel13.Name = "panel13";
            this.panel13.Size = new System.Drawing.Size(294, 336);
            this.panel13.TabIndex = 2;
            // 
            // labelDroxTries
            // 
            this.labelDroxTries.AutoSize = true;
            this.labelDroxTries.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDroxTries.ForeColor = System.Drawing.Color.White;
            this.labelDroxTries.Location = new System.Drawing.Point(146, 298);
            this.labelDroxTries.Name = "labelDroxTries";
            this.labelDroxTries.Size = new System.Drawing.Size(21, 13);
            this.labelDroxTries.TabIndex = 50;
            this.labelDroxTries.Text = "8x";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.ForeColor = System.Drawing.Color.White;
            this.label23.Location = new System.Drawing.Point(78, 298);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(49, 13);
            this.label23.TabIndex = 49;
            this.label23.Text = "Tried:";
            // 
            // labelDroxKillCount
            // 
            this.labelDroxKillCount.AutoSize = true;
            this.labelDroxKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDroxKillCount.ForeColor = System.Drawing.Color.White;
            this.labelDroxKillCount.Location = new System.Drawing.Point(146, 282);
            this.labelDroxKillCount.Name = "labelDroxKillCount";
            this.labelDroxKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelDroxKillCount.TabIndex = 48;
            this.labelDroxKillCount.Text = "8x";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.ForeColor = System.Drawing.Color.White;
            this.label28.Location = new System.Drawing.Point(78, 282);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(56, 13);
            this.label28.TabIndex = 47;
            this.label28.Text = "Killed:";
            // 
            // labelDroxStatus
            // 
            this.labelDroxStatus.AutoSize = true;
            this.labelDroxStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDroxStatus.ForeColor = System.Drawing.Color.White;
            this.labelDroxStatus.Location = new System.Drawing.Point(146, 266);
            this.labelDroxStatus.Name = "labelDroxStatus";
            this.labelDroxStatus.Size = new System.Drawing.Size(28, 13);
            this.labelDroxStatus.TabIndex = 46;
            this.labelDroxStatus.Text = "Yes";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.ForeColor = System.Drawing.Color.White;
            this.label30.Location = new System.Drawing.Point(78, 266);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(42, 13);
            this.label30.TabIndex = 45;
            this.label30.Text = "Done:";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.ForeColor = System.Drawing.Color.White;
            this.label47.Location = new System.Drawing.Point(105, 7);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(68, 28);
            this.label47.TabIndex = 44;
            this.label47.Text = "Drox";
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox6.Image")));
            this.pictureBox6.Location = new System.Drawing.Point(3, 42);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(289, 220);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox6.TabIndex = 38;
            this.pictureBox6.TabStop = false;
            // 
            // panel14
            // 
            this.panel14.Controls.Add(this.labelVeritaniaTries);
            this.panel14.Controls.Add(this.label17);
            this.panel14.Controls.Add(this.labelVeritaniaKillCount);
            this.panel14.Controls.Add(this.label20);
            this.panel14.Controls.Add(this.labelVeritaniaStatus);
            this.panel14.Controls.Add(this.label22);
            this.panel14.Controls.Add(this.pictureBox3);
            this.panel14.Controls.Add(this.label48);
            this.panel14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel14.Location = new System.Drawing.Point(3, 345);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(294, 330);
            this.panel14.TabIndex = 3;
            // 
            // labelVeritaniaTries
            // 
            this.labelVeritaniaTries.AutoSize = true;
            this.labelVeritaniaTries.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVeritaniaTries.ForeColor = System.Drawing.Color.White;
            this.labelVeritaniaTries.Location = new System.Drawing.Point(160, 297);
            this.labelVeritaniaTries.Name = "labelVeritaniaTries";
            this.labelVeritaniaTries.Size = new System.Drawing.Size(21, 13);
            this.labelVeritaniaTries.TabIndex = 58;
            this.labelVeritaniaTries.Text = "8x";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.White;
            this.label17.Location = new System.Drawing.Point(80, 297);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(49, 13);
            this.label17.TabIndex = 57;
            this.label17.Text = "Tried:";
            // 
            // labelVeritaniaKillCount
            // 
            this.labelVeritaniaKillCount.AutoSize = true;
            this.labelVeritaniaKillCount.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVeritaniaKillCount.ForeColor = System.Drawing.Color.White;
            this.labelVeritaniaKillCount.Location = new System.Drawing.Point(161, 281);
            this.labelVeritaniaKillCount.Name = "labelVeritaniaKillCount";
            this.labelVeritaniaKillCount.Size = new System.Drawing.Size(21, 13);
            this.labelVeritaniaKillCount.TabIndex = 56;
            this.labelVeritaniaKillCount.Text = "8x";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.White;
            this.label20.Location = new System.Drawing.Point(81, 281);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(56, 13);
            this.label20.TabIndex = 55;
            this.label20.Text = "Killed:";
            // 
            // labelVeritaniaStatus
            // 
            this.labelVeritaniaStatus.AutoSize = true;
            this.labelVeritaniaStatus.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVeritaniaStatus.ForeColor = System.Drawing.Color.White;
            this.labelVeritaniaStatus.Location = new System.Drawing.Point(160, 265);
            this.labelVeritaniaStatus.Name = "labelVeritaniaStatus";
            this.labelVeritaniaStatus.Size = new System.Drawing.Size(28, 13);
            this.labelVeritaniaStatus.TabIndex = 54;
            this.labelVeritaniaStatus.Text = "Yes";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.ForeColor = System.Drawing.Color.White;
            this.label22.Location = new System.Drawing.Point(81, 265);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(42, 13);
            this.label22.TabIndex = 53;
            this.label22.Text = "Done:";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(5, 40);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(289, 220);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 52;
            this.pictureBox3.TabStop = false;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.ForeColor = System.Drawing.Color.White;
            this.label48.Location = new System.Drawing.Point(79, 9);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(138, 28);
            this.label48.TabIndex = 51;
            this.label48.Text = "Veritania";
            // 
            // panel15
            // 
            this.panel15.Controls.Add(this.label106);
            this.panel15.Controls.Add(this.label107);
            this.panel15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel15.Location = new System.Drawing.Point(303, 345);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(294, 330);
            this.panel15.TabIndex = 4;
            // 
            // label106
            // 
            this.label106.AutoSize = true;
            this.label106.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label106.ForeColor = System.Drawing.Color.White;
            this.label106.Location = new System.Drawing.Point(105, 374);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(56, 13);
            this.label106.TabIndex = 88;
            this.label106.Text = "8 times";
            // 
            // label107
            // 
            this.label107.AutoSize = true;
            this.label107.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label107.ForeColor = System.Drawing.Color.White;
            this.label107.Location = new System.Drawing.Point(23, 374);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(49, 13);
            this.label107.TabIndex = 87;
            this.label107.Text = "Tried:";
            // 
            // panel16
            // 
            this.panel16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel16.Location = new System.Drawing.Point(603, 345);
            this.panel16.Name = "panel16";
            this.panel16.Size = new System.Drawing.Size(294, 330);
            this.panel16.TabIndex = 5;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tabPage3.Controls.Add(this.tabControl2);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1115, 874);
            this.tabPage3.TabIndex = 7;
            this.tabPage3.Text = "Config";
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage6);
            this.tabControl2.Controls.Add(this.tabPage7);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(3, 3);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1109, 868);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage6
            // 
            this.tabPage6.AutoScroll = true;
            this.tabPage6.BackColor = System.Drawing.Color.Black;
            this.tabPage6.Controls.Add(this.button1);
            this.tabPage6.Controls.Add(this.groupBox9);
            this.tabPage6.Controls.Add(this.groupBox6);
            this.tabPage6.Controls.Add(this.groupBox5);
            this.tabPage6.Controls.Add(this.groupBox4);
            this.tabPage6.Controls.Add(this.groupBox2);
            this.tabPage6.Controls.Add(this.groupBox1);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(1101, 842);
            this.tabPage6.TabIndex = 0;
            this.tabPage6.Text = "Settings";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(931, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Wiki help";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.buttonSaveCaps);
            this.groupBox9.Controls.Add(this.label99);
            this.groupBox9.Controls.Add(this.label98);
            this.groupBox9.Controls.Add(this.label97);
            this.groupBox9.Controls.Add(this.textBoxHeistCap);
            this.groupBox9.Controls.Add(this.label96);
            this.groupBox9.Controls.Add(this.textBoxLabCap);
            this.groupBox9.Controls.Add(this.label95);
            this.groupBox9.Controls.Add(this.textBoxMapCap);
            this.groupBox9.Controls.Add(this.label94);
            this.groupBox9.Controls.Add(this.lbl_info_1);
            this.groupBox9.ForeColor = System.Drawing.Color.Red;
            this.groupBox9.Location = new System.Drawing.Point(6, 348);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(1056, 155);
            this.groupBox9.TabIndex = 8;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Calculation Options";
            // 
            // buttonSaveCaps
            // 
            this.buttonSaveCaps.BackColor = System.Drawing.Color.White;
            this.buttonSaveCaps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSaveCaps.Location = new System.Drawing.Point(6, 121);
            this.buttonSaveCaps.Name = "buttonSaveCaps";
            this.buttonSaveCaps.Size = new System.Drawing.Size(76, 23);
            this.buttonSaveCaps.TabIndex = 10;
            this.buttonSaveCaps.Text = "Save";
            this.buttonSaveCaps.UseVisualStyleBackColor = false;
            this.buttonSaveCaps.Click += new System.EventHandler(this.button23_Click);
            // 
            // label99
            // 
            this.label99.AutoSize = true;
            this.label99.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label99.ForeColor = System.Drawing.Color.White;
            this.label99.Location = new System.Drawing.Point(261, 94);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(483, 13);
            this.label99.TabIndex = 19;
            this.label99.Text = "<-- maximum time for heists in sec. (only affects stats / dashboard)";
            // 
            // label98
            // 
            this.label98.AutoSize = true;
            this.label98.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label98.ForeColor = System.Drawing.Color.White;
            this.label98.Location = new System.Drawing.Point(261, 68);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(469, 13);
            this.label98.TabIndex = 18;
            this.label98.Text = "<-- maximum time for labs in sec. (only affects stats / dashboard)";
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label97.ForeColor = System.Drawing.Color.White;
            this.label97.Location = new System.Drawing.Point(261, 42);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(469, 13);
            this.label97.TabIndex = 17;
            this.label97.Text = "<-- maximum time for maps in sec. (only affects stats / dashboard)";
            // 
            // textBoxHeistCap
            // 
            this.textBoxHeistCap.Location = new System.Drawing.Point(165, 90);
            this.textBoxHeistCap.Name = "textBoxHeistCap";
            this.textBoxHeistCap.Size = new System.Drawing.Size(90, 20);
            this.textBoxHeistCap.TabIndex = 16;
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label96.ForeColor = System.Drawing.Color.White;
            this.label96.Location = new System.Drawing.Point(6, 94);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(147, 13);
            this.label96.TabIndex = 15;
            this.label96.Text = "Time cap for Heists:";
            // 
            // textBoxLabCap
            // 
            this.textBoxLabCap.Location = new System.Drawing.Point(165, 64);
            this.textBoxLabCap.Name = "textBoxLabCap";
            this.textBoxLabCap.Size = new System.Drawing.Size(90, 20);
            this.textBoxLabCap.TabIndex = 14;
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label95.ForeColor = System.Drawing.Color.White;
            this.label95.Location = new System.Drawing.Point(6, 68);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(133, 13);
            this.label95.TabIndex = 13;
            this.label95.Text = "Time cap for Labs:";
            // 
            // textBoxMapCap
            // 
            this.textBoxMapCap.Location = new System.Drawing.Point(165, 38);
            this.textBoxMapCap.Name = "textBoxMapCap";
            this.textBoxMapCap.Size = new System.Drawing.Size(90, 20);
            this.textBoxMapCap.TabIndex = 12;
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label94.ForeColor = System.Drawing.Color.White;
            this.label94.Location = new System.Drawing.Point(6, 42);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(133, 13);
            this.label94.TabIndex = 11;
            this.label94.Text = "Time cap for Maps:";
            // 
            // lbl_info_1
            // 
            this.lbl_info_1.AutoSize = true;
            this.lbl_info_1.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_info_1.ForeColor = System.Drawing.Color.Red;
            this.lbl_info_1.Location = new System.Drawing.Point(6, 16);
            this.lbl_info_1.Name = "lbl_info_1";
            this.lbl_info_1.Size = new System.Drawing.Size(784, 13);
            this.lbl_info_1.TabIndex = 10;
            this.lbl_info_1.Text = "The options in this group are used to customize the behavior of stats calculation" +
    " for your personal preferences";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.buttonDeleteBackup);
            this.groupBox6.Controls.Add(this.buttonRestoreBackup);
            this.groupBox6.Controls.Add(this.listBoxRestoreBackup);
            this.groupBox6.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.ForeColor = System.Drawing.Color.Red;
            this.groupBox6.Location = new System.Drawing.Point(223, 504);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(432, 268);
            this.groupBox6.TabIndex = 7;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Restore Backup";
            // 
            // buttonDeleteBackup
            // 
            this.buttonDeleteBackup.BackColor = System.Drawing.Color.White;
            this.buttonDeleteBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDeleteBackup.Location = new System.Drawing.Point(342, 49);
            this.buttonDeleteBackup.Name = "buttonDeleteBackup";
            this.buttonDeleteBackup.Size = new System.Drawing.Size(75, 23);
            this.buttonDeleteBackup.TabIndex = 2;
            this.buttonDeleteBackup.Text = "Delete";
            this.buttonDeleteBackup.UseVisualStyleBackColor = false;
            this.buttonDeleteBackup.Click += new System.EventHandler(this.button18_Click);
            // 
            // buttonRestoreBackup
            // 
            this.buttonRestoreBackup.BackColor = System.Drawing.Color.White;
            this.buttonRestoreBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRestoreBackup.Location = new System.Drawing.Point(342, 20);
            this.buttonRestoreBackup.Name = "buttonRestoreBackup";
            this.buttonRestoreBackup.Size = new System.Drawing.Size(75, 23);
            this.buttonRestoreBackup.TabIndex = 1;
            this.buttonRestoreBackup.Text = "Restore";
            this.buttonRestoreBackup.UseVisualStyleBackColor = false;
            this.buttonRestoreBackup.Click += new System.EventHandler(this.button17_Click);
            // 
            // listBoxRestoreBackup
            // 
            this.listBoxRestoreBackup.FormattingEnabled = true;
            this.listBoxRestoreBackup.Location = new System.Drawing.Point(16, 19);
            this.listBoxRestoreBackup.Name = "listBoxRestoreBackup";
            this.listBoxRestoreBackup.Size = new System.Drawing.Size(319, 225);
            this.listBoxRestoreBackup.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label62);
            this.groupBox5.Controls.Add(this.textBoxBackupName);
            this.groupBox5.Controls.Add(this.buttonCreateBackup);
            this.groupBox5.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.ForeColor = System.Drawing.Color.Red;
            this.groupBox5.Location = new System.Drawing.Point(6, 504);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(214, 268);
            this.groupBox5.TabIndex = 4;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Create Backup";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Location = new System.Drawing.Point(14, 38);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(42, 13);
            this.label62.TabIndex = 6;
            this.label62.Text = "Name:";
            // 
            // textBoxBackupName
            // 
            this.textBoxBackupName.Location = new System.Drawing.Point(58, 35);
            this.textBoxBackupName.Name = "textBoxBackupName";
            this.textBoxBackupName.Size = new System.Drawing.Size(131, 20);
            this.textBoxBackupName.TabIndex = 5;
            this.textBoxBackupName.Text = "Default";
            // 
            // buttonCreateBackup
            // 
            this.buttonCreateBackup.BackColor = System.Drawing.Color.White;
            this.buttonCreateBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCreateBackup.Location = new System.Drawing.Point(17, 62);
            this.buttonCreateBackup.Name = "buttonCreateBackup";
            this.buttonCreateBackup.Size = new System.Drawing.Size(172, 23);
            this.buttonCreateBackup.TabIndex = 4;
            this.buttonCreateBackup.Text = "Create Backup";
            this.buttonCreateBackup.UseVisualStyleBackColor = false;
            this.buttonCreateBackup.Click += new System.EventHandler(this.button16_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.checkBoxShowGridInStats);
            this.groupBox4.Controls.Add(this.checkBoxShowGridInAct);
            this.groupBox4.Controls.Add(this.comboBoxTheme);
            this.groupBox4.Controls.Add(this.label55);
            this.groupBox4.Controls.Add(this.label61);
            this.groupBox4.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.ForeColor = System.Drawing.Color.Red;
            this.groupBox4.Location = new System.Drawing.Point(528, 29);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(534, 108);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "UI";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(216, 63);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "change";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // checkBoxShowGridInStats
            // 
            this.checkBoxShowGridInStats.AutoSize = true;
            this.checkBoxShowGridInStats.ForeColor = System.Drawing.Color.White;
            this.checkBoxShowGridInStats.Location = new System.Drawing.Point(12, 42);
            this.checkBoxShowGridInStats.Name = "checkBoxShowGridInStats";
            this.checkBoxShowGridInStats.Size = new System.Drawing.Size(264, 17);
            this.checkBoxShowGridInStats.TabIndex = 4;
            this.checkBoxShowGridInStats.Text = "Show grid lines in statistics list";
            this.checkBoxShowGridInStats.UseVisualStyleBackColor = true;
            this.checkBoxShowGridInStats.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBoxShowGridInAct
            // 
            this.checkBoxShowGridInAct.AutoSize = true;
            this.checkBoxShowGridInAct.ForeColor = System.Drawing.Color.White;
            this.checkBoxShowGridInAct.Location = new System.Drawing.Point(12, 19);
            this.checkBoxShowGridInAct.Name = "checkBoxShowGridInAct";
            this.checkBoxShowGridInAct.Size = new System.Drawing.Size(250, 17);
            this.checkBoxShowGridInAct.TabIndex = 3;
            this.checkBoxShowGridInAct.Text = "Show grid lines in activity list";
            this.checkBoxShowGridInAct.UseVisualStyleBackColor = true;
            this.checkBoxShowGridInAct.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // comboBoxTheme
            // 
            this.comboBoxTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTheme.FormattingEnabled = true;
            this.comboBoxTheme.Items.AddRange(new object[] {
            "Dark",
            "Light"});
            this.comboBoxTheme.Location = new System.Drawing.Point(65, 65);
            this.comboBoxTheme.Name = "comboBoxTheme";
            this.comboBoxTheme.Size = new System.Drawing.Size(145, 21);
            this.comboBoxTheme.TabIndex = 5;
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.ForeColor = System.Drawing.Color.White;
            this.label55.Location = new System.Drawing.Point(10, 68);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(56, 13);
            this.label55.TabIndex = 2;
            this.label55.Text = "Theme: ";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(6, 33);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(0, 13);
            this.label61.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxLogFilePath);
            this.groupBox2.Controls.Add(this.label56);
            this.groupBox2.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Red;
            this.groupBox2.Location = new System.Drawing.Point(3, 29);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(519, 108);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General";
            // 
            // textBoxLogFilePath
            // 
            this.textBoxLogFilePath.Location = new System.Drawing.Point(9, 50);
            this.textBoxLogFilePath.Name = "textBoxLogFilePath";
            this.textBoxLogFilePath.Size = new System.Drawing.Size(469, 20);
            this.textBoxLogFilePath.TabIndex = 1;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.ForeColor = System.Drawing.Color.White;
            this.label56.Location = new System.Drawing.Point(6, 33);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(91, 13);
            this.label56.TabIndex = 0;
            this.label56.Text = "PoE logfile:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label46);
            this.groupBox1.Controls.Add(this.label72);
            this.groupBox1.Controls.Add(this.label71);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.label70);
            this.groupBox1.Controls.Add(this.label69);
            this.groupBox1.Controls.Add(this.buttonRollLog);
            this.groupBox1.Controls.Add(this.buttonFullReset);
            this.groupBox1.Controls.Add(this.buttonChangeLogReload);
            this.groupBox1.Controls.Add(this.buttonReloadLogfile);
            this.groupBox1.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.Red;
            this.groupBox1.Location = new System.Drawing.Point(6, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1056, 190);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Advanced";
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.ForeColor = System.Drawing.Color.White;
            this.label46.Location = new System.Drawing.Point(240, 54);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(735, 13);
            this.label46.TabIndex = 11;
            this.label46.Text = "<-- Reload the logfile and re-calculate all statistics, keeping the events in his" +
    "tory (Restart required)";
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.ForeColor = System.Drawing.Color.White;
            this.label72.Location = new System.Drawing.Point(240, 140);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(763, 13);
            this.label72.TabIndex = 9;
            this.label72.Text = "<-- Safely clear your Client.txt (when it gets too big) without loosing your TraX" +
    "ile data (Restart required)";
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.ForeColor = System.Drawing.Color.White;
            this.label71.Location = new System.Drawing.Point(240, 111);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(574, 13);
            this.label71.TabIndex = 8;
            this.label71.Text = "<- Clear your Client.txt, drop all data and start new database (Restart required)" +
    "";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.White;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Location = new System.Drawing.Point(10, 49);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(217, 23);
            this.button4.TabIndex = 10;
            this.button4.Text = "Reload Logfile (stats only)";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.ForeColor = System.Drawing.Color.White;
            this.label70.Location = new System.Drawing.Point(240, 82);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(504, 13);
            this.label70.TabIndex = 7;
            this.label70.Text = "<-- change the path of the PoE Client.txt and reload (Restart required)";
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.ForeColor = System.Drawing.Color.White;
            this.label69.Location = new System.Drawing.Point(240, 25);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(714, 13);
            this.label69.TabIndex = 6;
            this.label69.Text = "<-- Reload the logfile and re-calculate all statistics, deleting events in histor" +
    "y (Restart required)";
            // 
            // buttonRollLog
            // 
            this.buttonRollLog.BackColor = System.Drawing.Color.White;
            this.buttonRollLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRollLog.Location = new System.Drawing.Point(10, 135);
            this.buttonRollLog.Name = "buttonRollLog";
            this.buttonRollLog.Size = new System.Drawing.Size(217, 23);
            this.buttonRollLog.TabIndex = 4;
            this.buttonRollLog.Text = "Roll client.txt";
            this.buttonRollLog.UseVisualStyleBackColor = false;
            this.buttonRollLog.Click += new System.EventHandler(this.button21_Click);
            // 
            // buttonFullReset
            // 
            this.buttonFullReset.BackColor = System.Drawing.Color.White;
            this.buttonFullReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFullReset.Location = new System.Drawing.Point(10, 106);
            this.buttonFullReset.Name = "buttonFullReset";
            this.buttonFullReset.Size = new System.Drawing.Size(217, 23);
            this.buttonFullReset.TabIndex = 3;
            this.buttonFullReset.Text = "Full reset - start fresh";
            this.buttonFullReset.UseVisualStyleBackColor = false;
            this.buttonFullReset.Click += new System.EventHandler(this.button15_Click);
            // 
            // buttonChangeLogReload
            // 
            this.buttonChangeLogReload.BackColor = System.Drawing.Color.White;
            this.buttonChangeLogReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChangeLogReload.Location = new System.Drawing.Point(10, 77);
            this.buttonChangeLogReload.Name = "buttonChangeLogReload";
            this.buttonChangeLogReload.Size = new System.Drawing.Size(217, 23);
            this.buttonChangeLogReload.TabIndex = 2;
            this.buttonChangeLogReload.Text = "Change logfile and reload";
            this.buttonChangeLogReload.UseVisualStyleBackColor = false;
            this.buttonChangeLogReload.Click += new System.EventHandler(this.button6_Click);
            // 
            // buttonReloadLogfile
            // 
            this.buttonReloadLogfile.BackColor = System.Drawing.Color.White;
            this.buttonReloadLogfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReloadLogfile.Location = new System.Drawing.Point(10, 20);
            this.buttonReloadLogfile.Name = "buttonReloadLogfile";
            this.buttonReloadLogfile.Size = new System.Drawing.Size(217, 23);
            this.buttonReloadLogfile.TabIndex = 1;
            this.buttonReloadLogfile.Text = "Reload Logfile (Full)";
            this.buttonReloadLogfile.UseVisualStyleBackColor = false;
            this.buttonReloadLogfile.Click += new System.EventHandler(this.button7_Click);
            // 
            // tabPage7
            // 
            this.tabPage7.BackColor = System.Drawing.Color.Black;
            this.tabPage7.Controls.Add(this.tableLayoutPanel6);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(1101, 842);
            this.tabPage7.TabIndex = 1;
            this.tabPage7.Text = "Manage Tags";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.01571F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.98429F));
            this.tableLayoutPanel6.Controls.Add(this.panelEditTags, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.panel5, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 4;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.84615F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1095, 836);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // panelEditTags
            // 
            this.panelEditTags.Controls.Add(this.groupBox3);
            this.panelEditTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEditTags.Location = new System.Drawing.Point(3, 40);
            this.panelEditTags.Name = "panelEditTags";
            this.panelEditTags.Size = new System.Drawing.Size(508, 752);
            this.panelEditTags.TabIndex = 0;
            this.panelEditTags.SizeChanged += new System.EventHandler(this.panelEditTags_SizeChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.Color.Red;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(508, 752);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tags (click to edit)";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.textBox3);
            this.panel4.Controls.Add(this.label58);
            this.panel4.Controls.Add(this.label57);
            this.panel4.Controls.Add(this.button10);
            this.panel4.Controls.Add(this.textBox2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(508, 31);
            this.panel4.TabIndex = 1;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(234, 5);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 4;
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label58.ForeColor = System.Drawing.Color.White;
            this.label58.Location = new System.Drawing.Point(137, 8);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(91, 13);
            this.label58.TabIndex = 3;
            this.label58.Tag = "";
            this.label58.Text = "Displayname:";
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label57.ForeColor = System.Drawing.Color.White;
            this.label57.Location = new System.Drawing.Point(4, 9);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(28, 13);
            this.label57.TabIndex = 2;
            this.label57.Tag = "";
            this.label57.Text = "ID:";
            // 
            // button10
            // 
            this.button10.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button10.Location = new System.Drawing.Point(354, 3);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(56, 23);
            this.button10.TabIndex = 1;
            this.button10.Text = "add";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(31, 5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 0;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.groupBox7);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(517, 40);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(575, 752);
            this.panel5.TabIndex = 2;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.checkBox4);
            this.groupBox7.Controls.Add(this.label90);
            this.groupBox7.Controls.Add(this.button20);
            this.groupBox7.Controls.Add(this.label59);
            this.groupBox7.Controls.Add(this.button19);
            this.groupBox7.Controls.Add(this.textBox4);
            this.groupBox7.Controls.Add(this.button14);
            this.groupBox7.Controls.Add(this.label60);
            this.groupBox7.Controls.Add(this.button13);
            this.groupBox7.Controls.Add(this.textBox5);
            this.groupBox7.Controls.Add(this.label64);
            this.groupBox7.Controls.Add(this.button11);
            this.groupBox7.Controls.Add(this.label63);
            this.groupBox7.Controls.Add(this.button12);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox7.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.ForeColor = System.Drawing.Color.Red;
            this.groupBox7.Location = new System.Drawing.Point(0, 0);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(575, 752);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Edit";
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(172, 78);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(15, 14);
            this.checkBox4.TabIndex = 13;
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label90.ForeColor = System.Drawing.Color.White;
            this.label90.Location = new System.Drawing.Point(15, 78);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(140, 13);
            this.label90.TabIndex = 12;
            this.label90.Text = "Display in history:";
            // 
            // button20
            // 
            this.button20.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button20.Location = new System.Drawing.Point(294, 51);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(88, 23);
            this.button20.TabIndex = 11;
            this.button20.Text = "Reset default";
            this.button20.UseVisualStyleBackColor = true;
            this.button20.Click += new System.EventHandler(this.button20_Click);
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label59.ForeColor = System.Drawing.Color.White;
            this.label59.Location = new System.Drawing.Point(15, 27);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(28, 13);
            this.label59.TabIndex = 0;
            this.label59.Text = "ID:";
            // 
            // button19
            // 
            this.button19.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button19.Location = new System.Drawing.Point(181, 235);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(75, 23);
            this.button19.TabIndex = 10;
            this.button19.Text = "Delete";
            this.button19.UseVisualStyleBackColor = true;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // textBox4
            // 
            this.textBox4.Enabled = false;
            this.textBox4.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.Location = new System.Drawing.Point(172, 27);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(116, 20);
            this.textBox4.TabIndex = 1;
            // 
            // button14
            // 
            this.button14.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button14.Location = new System.Drawing.Point(100, 235);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 23);
            this.button14.TabIndex = 9;
            this.button14.Text = "Cancel";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label60.ForeColor = System.Drawing.Color.White;
            this.label60.Location = new System.Drawing.Point(15, 53);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(98, 13);
            this.label60.TabIndex = 2;
            this.label60.Text = "Display Name:";
            // 
            // button13
            // 
            this.button13.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button13.Location = new System.Drawing.Point(16, 235);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 23);
            this.button13.TabIndex = 8;
            this.button13.Text = "Save";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // textBox5
            // 
            this.textBox5.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox5.Location = new System.Drawing.Point(172, 53);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(116, 20);
            this.textBox5.TabIndex = 3;
            this.textBox5.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label64.ForeColor = System.Drawing.Color.White;
            this.label64.Location = new System.Drawing.Point(19, 156);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(63, 13);
            this.label64.TabIndex = 7;
            this.label64.Text = "Preview:";
            // 
            // button11
            // 
            this.button11.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button11.Location = new System.Drawing.Point(19, 117);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(72, 23);
            this.button11.TabIndex = 4;
            this.button11.Text = "bg color";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // label63
            // 
            this.label63.BackColor = System.Drawing.Color.White;
            this.label63.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label63.Location = new System.Drawing.Point(16, 180);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(100, 23);
            this.label63.TabIndex = 6;
            this.label63.Text = "MyCustomTag";
            this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button12
            // 
            this.button12.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button12.Location = new System.Drawing.Point(97, 117);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 23);
            this.button12.TabIndex = 5;
            this.button12.Text = "text color";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBoxLogView);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1115, 874);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "EventLog";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBoxLogView
            // 
            this.textBoxLogView.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.textBoxLogView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLogView.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLogView.ForeColor = System.Drawing.Color.White;
            this.textBoxLogView.Location = new System.Drawing.Point(3, 3);
            this.textBoxLogView.Multiline = true;
            this.textBoxLogView.Name = "textBoxLogView";
            this.textBoxLogView.ReadOnly = true;
            this.textBoxLogView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLogView.Size = new System.Drawing.Size(1109, 868);
            this.textBoxLogView.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1123, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click_1);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chatCommandsToolStripMenuItem,
            this.infoToolStripMenuItem1,
            this.checkForUpdateToolStripMenuItem,
            this.wikiToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // chatCommandsToolStripMenuItem
            // 
            this.chatCommandsToolStripMenuItem.Name = "chatCommandsToolStripMenuItem";
            this.chatCommandsToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.chatCommandsToolStripMenuItem.Text = "Chat Commands";
            this.chatCommandsToolStripMenuItem.Click += new System.EventHandler(this.chatCommandsToolStripMenuItem_Click);
            // 
            // infoToolStripMenuItem1
            // 
            this.infoToolStripMenuItem1.Name = "infoToolStripMenuItem1";
            this.infoToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.infoToolStripMenuItem1.Text = "Info";
            this.infoToolStripMenuItem1.Click += new System.EventHandler(this.infoToolStripMenuItem1_Click);
            // 
            // checkForUpdateToolStripMenuItem
            // 
            this.checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
            this.checkForUpdateToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.checkForUpdateToolStripMenuItem.Text = "Check for Update";
            this.checkForUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdateToolStripMenuItem_Click);
            // 
            // wikiToolStripMenuItem
            // 
            this.wikiToolStripMenuItem.Name = "wikiToolStripMenuItem";
            this.wikiToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.wikiToolStripMenuItem.Text = "Wiki";
            this.wikiToolStripMenuItem.Click += new System.EventHandler(this.wikiToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 1000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.67939F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.32061F));
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(76, 245);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 5;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(744, 250);
            this.tableLayoutPanel4.TabIndex = 14;
            // 
            // labelKilledBy
            // 
            this.labelKilledBy.AutoSize = true;
            this.labelKilledBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelKilledBy.ForeColor = System.Drawing.Color.Gold;
            this.labelKilledBy.Location = new System.Drawing.Point(238, 128);
            this.labelKilledBy.Name = "labelKilledBy";
            this.labelKilledBy.Size = new System.Drawing.Size(19, 25);
            this.labelKilledBy.TabIndex = 14;
            this.labelKilledBy.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Gold;
            this.label4.Location = new System.Drawing.Point(3, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 25);
            this.label4.TabIndex = 13;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.ForeColor = System.Drawing.Color.Gold;
            this.label31.Location = new System.Drawing.Point(3, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(87, 25);
            this.label31.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Gold;
            this.label2.Location = new System.Drawing.Point(3, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 25);
            this.label2.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Gold;
            this.label1.Location = new System.Drawing.Point(3, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 25);
            this.label1.TabIndex = 1;
            // 
            // labelActivity
            // 
            this.labelActivity.AutoSize = true;
            this.labelActivity.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActivity.ForeColor = System.Drawing.Color.Gold;
            this.labelActivity.Location = new System.Drawing.Point(238, 0);
            this.labelActivity.Name = "labelActivity";
            this.labelActivity.Size = new System.Drawing.Size(19, 25);
            this.labelActivity.TabIndex = 12;
            // 
            // labelCurrentArea
            // 
            this.labelCurrentArea.AutoSize = true;
            this.labelCurrentArea.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrentArea.ForeColor = System.Drawing.Color.Gold;
            this.labelCurrentArea.Location = new System.Drawing.Point(238, 42);
            this.labelCurrentArea.Name = "labelCurrentArea";
            this.labelCurrentArea.Size = new System.Drawing.Size(19, 25);
            this.labelCurrentArea.TabIndex = 2;
            // 
            // labelLatDeath
            // 
            this.labelLatDeath.AutoSize = true;
            this.labelLatDeath.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLatDeath.ForeColor = System.Drawing.Color.Gold;
            this.labelLatDeath.Location = new System.Drawing.Point(238, 84);
            this.labelLatDeath.Name = "labelLatDeath";
            this.labelLatDeath.Size = new System.Drawing.Size(19, 25);
            this.labelLatDeath.TabIndex = 6;
            this.labelLatDeath.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Gold;
            this.label5.Location = new System.Drawing.Point(3, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 25);
            this.label5.TabIndex = 19;
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pictureBox16
            // 
            this.pictureBox16.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox16.Image")));
            this.pictureBox16.Location = new System.Drawing.Point(3, 19);
            this.pictureBox16.Name = "pictureBox16";
            this.pictureBox16.Size = new System.Drawing.Size(94, 142);
            this.pictureBox16.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox16.TabIndex = 16;
            this.pictureBox16.TabStop = false;
            // 
            // labelCurrentMapName
            // 
            this.labelCurrentMapName.Location = new System.Drawing.Point(0, 0);
            this.labelCurrentMapName.Name = "labelCurrentMapName";
            this.labelCurrentMapName.Size = new System.Drawing.Size(100, 23);
            this.labelCurrentMapName.TabIndex = 0;
            // 
            // pictureBox12
            // 
            this.pictureBox12.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox12.Image")));
            this.pictureBox12.Location = new System.Drawing.Point(8, 169);
            this.pictureBox12.Name = "pictureBox12";
            this.pictureBox12.Size = new System.Drawing.Size(19, 25);
            this.pictureBox12.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox12.TabIndex = 20;
            this.pictureBox12.TabStop = false;
            // 
            // labelMapTime
            // 
            this.labelMapTime.AutoSize = true;
            this.labelMapTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMapTime.ForeColor = System.Drawing.Color.Gold;
            this.labelMapTime.Location = new System.Drawing.Point(117, 50);
            this.labelMapTime.Name = "labelMapTime";
            this.labelMapTime.Size = new System.Drawing.Size(212, 55);
            this.labelMapTime.TabIndex = 17;
            this.labelMapTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDiedInMap
            // 
            this.labelDiedInMap.Location = new System.Drawing.Point(0, 0);
            this.labelDiedInMap.Name = "labelDiedInMap";
            this.labelDiedInMap.Size = new System.Drawing.Size(100, 23);
            this.labelDiedInMap.TabIndex = 0;
            // 
            // pictureBox14
            // 
            this.pictureBox14.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox14.Image")));
            this.pictureBox14.Location = new System.Drawing.Point(127, 108);
            this.pictureBox14.Name = "pictureBox14";
            this.pictureBox14.Size = new System.Drawing.Size(52, 37);
            this.pictureBox14.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox14.TabIndex = 22;
            this.pictureBox14.TabStop = false;
            this.pictureBox14.Click += new System.EventHandler(this.pictureBox14_Click_1);
            // 
            // pictureBox13
            // 
            this.pictureBox13.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox13.Image")));
            this.pictureBox13.Location = new System.Drawing.Point(185, 108);
            this.pictureBox13.Name = "pictureBox13";
            this.pictureBox13.Size = new System.Drawing.Size(44, 37);
            this.pictureBox13.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox13.TabIndex = 21;
            this.pictureBox13.TabStop = false;
            this.pictureBox13.Click += new System.EventHandler(this.pictureBox13_Click_1);
            // 
            // pictureBox15
            // 
            this.pictureBox15.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox15.Image")));
            this.pictureBox15.Location = new System.Drawing.Point(235, 107);
            this.pictureBox15.Name = "pictureBox15";
            this.pictureBox15.Size = new System.Drawing.Size(40, 37);
            this.pictureBox15.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox15.TabIndex = 23;
            this.pictureBox15.TabStop = false;
            this.pictureBox15.Click += new System.EventHandler(this.pictureBox15_Click_1);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.ForeColor = System.Drawing.Color.Gold;
            this.label51.Location = new System.Drawing.Point(124, 148);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(162, 18);
            this.label51.TabIndex = 24;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label36.ForeColor = System.Drawing.Color.Gold;
            this.label36.Location = new System.Drawing.Point(292, 148);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(16, 18);
            this.label36.TabIndex = 25;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1,
            this.helpToolStripMenuItem1,
            this.exitToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(108, 70);
            this.contextMenuStrip1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.contextMenuStrip1_MouseDoubleClick);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chatCommandsToolStripMenuItem1});
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.helpToolStripMenuItem1.Text = "Help";
            // 
            // chatCommandsToolStripMenuItem1
            // 
            this.chatCommandsToolStripMenuItem1.Name = "chatCommandsToolStripMenuItem1";
            this.chatCommandsToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
            this.chatCommandsToolStripMenuItem1.Text = "Chat Commands";
            this.chatCommandsToolStripMenuItem1.Click += new System.EventHandler(this.chatCommandsToolStripMenuItem1_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "WhiteMap.png");
            this.imageList2.Images.SetKeyName(1, "YellowMap.png");
            this.imageList2.Images.SetKeyName(2, "RedMap.png");
            this.imageList2.Images.SetKeyName(3, "TempleMap.png");
            this.imageList2.Images.SetKeyName(4, "ContractItem.png");
            this.imageList2.Images.SetKeyName(5, "Abyss.png");
            this.imageList2.Images.SetKeyName(6, "Labyrinth.png");
            this.imageList2.Images.SetKeyName(7, "Campaign.png");
            this.imageList2.Images.SetKeyName(8, "ExpeditionChronicle3.png");
            this.imageList2.Images.SetKeyName(9, "Vaal.png");
            this.imageList2.Images.SetKeyName(10, "Catarina.png");
            this.imageList2.Images.SetKeyName(11, "Safehouse.png");
            this.imageList2.Images.SetKeyName(12, "Fossil.png");
            this.imageList2.Images.SetKeyName(13, "Invitation.png");
            this.imageList2.Images.SetKeyName(14, "Sirus.png");
            this.imageList2.Images.SetKeyName(15, "Vaal01.png");
            this.imageList2.Images.SetKeyName(16, "UberVaal04.png");
            this.imageList2.Images.SetKeyName(17, "Elder.png");
            this.imageList2.Images.SetKeyName(18, "Shaper.png");
            this.imageList2.Images.SetKeyName(19, "Simulacrum.png");
            this.imageList2.Images.SetKeyName(20, "MavenKey.png");
            this.imageList2.Images.SetKeyName(21, "BreachFragmentsChaosChayula.png");
            this.imageList2.Images.SetKeyName(22, "BreachFragmentsLightningEsh.png");
            this.imageList2.Images.SetKeyName(23, "BreachFragmentsFireXoph.png");
            this.imageList2.Images.SetKeyName(24, "BreachFragmentsPhysicalUul.png");
            this.imageList2.Images.SetKeyName(25, "BreachFragmentsColdTul.png");
            this.imageList2.Images.SetKeyName(26, "TulsFlawlessBreachstone.png");
            this.imageList2.Images.SetKeyName(27, "TulsPureBreachstone.png");
            this.imageList2.Images.SetKeyName(28, "TulsEnrichedBreachstone.png");
            this.imageList2.Images.SetKeyName(29, "TulsChargedBreachstone.png");
            this.imageList2.Images.SetKeyName(30, "UulNetolsFlawlessBreachstone.png");
            this.imageList2.Images.SetKeyName(31, "UulNetolsPureBreachstone.png");
            this.imageList2.Images.SetKeyName(32, "UulNetolsEnrichedBreachstone.png");
            this.imageList2.Images.SetKeyName(33, "UulNetolsChargedBreachstone.png");
            this.imageList2.Images.SetKeyName(34, "XophsFlawlessBreachstone.png");
            this.imageList2.Images.SetKeyName(35, "XophsPureBreachstone.png");
            this.imageList2.Images.SetKeyName(36, "XophsEnrichedBreachstone.png");
            this.imageList2.Images.SetKeyName(37, "XophsChargedBreachstone.png");
            this.imageList2.Images.SetKeyName(38, "ChayulasFlawlessBreachstone.png");
            this.imageList2.Images.SetKeyName(39, "ChayulasPureBreachstone.png");
            this.imageList2.Images.SetKeyName(40, "ChayulasEnrichedBreachstone.png");
            this.imageList2.Images.SetKeyName(41, "ChayulasChargedBreachstone.png");
            this.imageList2.Images.SetKeyName(42, "EshsFlawlessBreachstone.png");
            this.imageList2.Images.SetKeyName(43, "EshsPureBreachstone.png");
            this.imageList2.Images.SetKeyName(44, "EshsEnrichedBreachstone.png");
            this.imageList2.Images.SetKeyName(45, "EshsChargedBreachstone.png");
            // 
            // listViewStats
            // 
            this.listViewStats.BackColor = System.Drawing.SystemColors.InfoText;
            this.listViewStats.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
            this.listViewStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStats.Font = new System.Drawing.Font("Noto Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewStats.ForeColor = System.Drawing.Color.White;
            this.listViewStats.FullRowSelect = true;
            this.listViewStats.HideSelection = false;
            this.listViewStats.Location = new System.Drawing.Point(3, 34);
            this.listViewStats.Name = "listViewStats";
            this.listViewStats.Size = new System.Drawing.Size(1103, 523);
            this.listViewStats.TabIndex = 0;
            this.listViewStats.UseCompatibleStateImageBehavior = false;
            this.listViewStats.View = System.Windows.Forms.View.Details;
            this.listViewStats.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.listViewStats.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Stat";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Value";
            // 
            // Main
            // 
            this.AcceptButton = this.buttonStartSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1123, 927);
            this.Controls.Add(tabControlMain);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainW_FormClosing);
            tabControlMain.ResumeLayout(false);
            this.tabPageTracking.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSkull)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPlay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStopWatch)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.panelTags.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.tabPageStatistics.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartStats)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.tabPage8.ResumeLayout(false);
            this.tabControl3.ResumeLayout(false);
            this.tabPage11.ResumeLayout(false);
            this.tableLayoutPanel24.ResumeLayout(false);
            this.tableLayoutPanel24.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartGlobalDashboard)).EndInit();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartMapTierCount)).EndInit();
            this.tableLayoutPanel12.ResumeLayout(false);
            this.tableLayoutPanel12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartMapTierAvgTime)).EndInit();
            this.tableLayoutPanel13.ResumeLayout(false);
            this.tableLayoutPanel13.PerformLayout();
            this.tableLayoutPanel14.ResumeLayout(false);
            this.tableLayoutPanel14.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tableLayoutPanel15.ResumeLayout(false);
            this.tableLayoutPanel17.ResumeLayout(false);
            this.tableLayoutPanel17.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLabsAvgTime)).EndInit();
            this.tableLayoutPanel16.ResumeLayout(false);
            this.tableLayoutPanel16.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLabsDone)).EndInit();
            this.tableLayoutPanel18.ResumeLayout(false);
            this.tableLayoutPanel18.PerformLayout();
            this.panel17.ResumeLayout(false);
            this.panel17.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tableLayoutPanel19.ResumeLayout(false);
            this.tableLayoutPanel20.ResumeLayout(false);
            this.tableLayoutPanel20.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeistByLevel)).EndInit();
            this.tableLayoutPanel21.ResumeLayout(false);
            this.tableLayoutPanel21.PerformLayout();
            this.tableLayoutPanel22.ResumeLayout(false);
            this.tableLayoutPanel22.PerformLayout();
            this.tableLayoutPanel23.ResumeLayout(false);
            this.tableLayoutPanel23.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeistAvgTime)).EndInit();
            this.tabPage9.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.panelBossElder.ResumeLayout(false);
            this.panelBossElder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelBossesShaper.ResumeLayout(false);
            this.panelBossesShaper.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panelBossesMaven.ResumeLayout(false);
            this.panelBossesMaven.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            this.panelBossesTrialmaster.ResumeLayout(false);
            this.panelBossesTrialmaster.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox21)).EndInit();
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.tabPage10.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.panel11.ResumeLayout(false);
            this.panel11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            this.panel12.ResumeLayout(false);
            this.panel12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.panel13.ResumeLayout(false);
            this.panel13.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            this.panel14.ResumeLayout(false);
            this.panel14.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.panel15.ResumeLayout(false);
            this.panel15.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage7.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.panelEditTags.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox16)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox15)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBoxLogView;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabPage tabPageStatistics;
        private ListViewNF listViewStats;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartStats;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonRefreshChart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxTimeRangeStats;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label labelKilledBy;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelActivity;
        private System.Windows.Forms.Label labelCurrentArea;
        private System.Windows.Forms.Label labelLatDeath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox16;
        private System.Windows.Forms.Label labelCurrentMapName;
        private System.Windows.Forms.PictureBox pictureBox12;
        private System.Windows.Forms.Label labelMapTime;
        private System.Windows.Forms.Label labelDiedInMap;
        private System.Windows.Forms.PictureBox pictureBox14;
        private System.Windows.Forms.PictureBox pictureBox13;
        private System.Windows.Forms.PictureBox pictureBox15;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TabPage tabPageTracking;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listViewActLog;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelTrackingType;
        private System.Windows.Forms.Label labelTrackingArea;
        private System.Windows.Forms.PictureBox pictureBoxStop;
        private System.Windows.Forms.PictureBox pictureBoxPause;
        private System.Windows.Forms.PictureBox pictureBoxPlay;
        private System.Windows.Forms.Label labelStopWatch;
        private System.Windows.Forms.PictureBox pictureBoxStopWatch;
        private System.Windows.Forms.Label labelTrackingDied;
        private System.Windows.Forms.PictureBox pictureBoxSkull;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.Label labelCurrArea;
        private System.Windows.Forms.Label labelCurrActivity;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.Label labelLastDeath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button buttonDeleteActivity;
        private System.Windows.Forms.Label lbl_info_2;
        private System.Windows.Forms.Button buttonExportActivities;
        private System.Windows.Forms.Button buttonActivityDetails;
        private System.Windows.Forms.Panel panelTags;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonChangeLogReload;
        private System.Windows.Forms.Button buttonReloadLogfile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxLogFilePath;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.Button buttonReloadActivities;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxShowGridInAct;
        private System.Windows.Forms.Label label61;
        private System.Windows.Forms.CheckBox checkBoxShowGridInStats;
        private System.Windows.Forms.Button buttonFullReset;
        private System.Windows.Forms.Button buttonCreateBackup;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label62;
        private System.Windows.Forms.TextBox textBoxBackupName;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ListBox listBoxRestoreBackup;
        private System.Windows.Forms.Button buttonDeleteBackup;
        private System.Windows.Forms.Button buttonRestoreBackup;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Panel panelEditTags;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label58;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label60;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label64;
        private System.Windows.Forms.Label label63;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label labelAddTagsNote;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBoxTrackingTags;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chatCommandsToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem chatCommandsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TextBox textBoxSearchStats;
        private System.Windows.Forms.Label label65;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label66;
        private System.Windows.Forms.LinkLabel linkLabelSearchSyntax;
        private System.Windows.Forms.LinkLabel linkLabelClearSearch;
        private System.Windows.Forms.LinkLabel linkLabelClearStatsSearch;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdateToolStripMenuItem;
        private System.Windows.Forms.Label label67;
        private System.Windows.Forms.Label labelCurrentAreaLvl;
        private System.Windows.Forms.Button buttonRollLog;
        private System.Windows.Forms.Label label72;
        private System.Windows.Forms.Label label71;
        private System.Windows.Forms.Label label70;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.TabPage tabPage10;
        private System.Windows.Forms.ToolStripMenuItem wikiToolStripMenuItem;
        private System.Windows.Forms.Label labelItemCount;
        private System.Windows.Forms.Button buttonStartSearch;
        private System.Windows.Forms.ComboBox comboBoxShowMaxItems;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Panel panelBossElder;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelElderKillCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelElderStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panelBossesShaper;
        private System.Windows.Forms.Label labelShaperKillCount;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label labelShaperStatus;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel panelBossesMaven;
        private System.Windows.Forms.Label labelMavenTried;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label labelMavenStatus;
        private System.Windows.Forms.Label labelMavenKilled;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Panel panelBossesTrialmaster;
        private System.Windows.Forms.Label labelTrialMasterTried;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Label labelTrialMasterKilled;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label labelTrialMasterStatus;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.PictureBox pictureBox9;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.Label label77;
        private System.Windows.Forms.Label label78;
        private System.Windows.Forms.Label label79;
        private System.Windows.Forms.Label label80;
        private System.Windows.Forms.Label label81;
        private System.Windows.Forms.PictureBox pictureBox21;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label labelSirusTries;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelSirusKillCount;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label labelSirusStatus;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.Panel panel11;
        private System.Windows.Forms.Panel panel12;
        private System.Windows.Forms.Panel panel13;
        private System.Windows.Forms.Panel panel14;
        private System.Windows.Forms.Panel panel15;
        private System.Windows.Forms.Label label106;
        private System.Windows.Forms.Label label107;
        private System.Windows.Forms.Panel panel16;
        private System.Windows.Forms.Label labelHunterTries;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label labelHunterKillCount;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label labelHunterStatus;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.Label labelBaranTries;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label labelBaranKillCount;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label labelBaranStatus;
        private System.Windows.Forms.Label labelDroxTries;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label labelDroxKillCount;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label labelDroxStatus;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.Label labelVeritaniaTries;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label labelVeritaniaKillCount;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label labelVeritaniaStatus;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel13;
        private System.Windows.Forms.ListView listViewTop10Maps;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel14;
        private System.Windows.Forms.ListView listViewTaggingOverview;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel15;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel16;
        private Chart chartLabsDone;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private Chart chartMapTierCount;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel12;
        private Chart chartMapTierAvgTime;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel17;
        private Chart chartLabsAvgTime;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel18;
        private System.Windows.Forms.Label label84;
        private System.Windows.Forms.ListView listViewBestLabs;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.Panel panel17;
        private System.Windows.Forms.CheckBox checkBoxLabHideUnknown;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel19;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel20;
        private Chart chartHeistByLevel;
        private System.Windows.Forms.Label label85;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel21;
        private System.Windows.Forms.ListView listView4;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.Label label86;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel22;
        private System.Windows.Forms.ListView listView5;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.Label label87;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel23;
        private Chart chartHeistAvgTime;
        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.TabPage tabPage11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel24;
        private Chart chartGlobalDashboard;
        private System.Windows.Forms.Label label89;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.Label labelElderTried;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.Label labelShaperTried;
        private System.Windows.Forms.Label label93;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Label label99;
        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.TextBox textBoxHeistCap;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.TextBox textBoxLabCap;
        private System.Windows.Forms.Label label95;
        private System.Windows.Forms.TextBox textBoxMapCap;
        private System.Windows.Forms.Label label94;
        private System.Windows.Forms.Label lbl_info_1;
        private System.Windows.Forms.Button buttonSaveCaps;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBoxTheme;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label49;
        public System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox10;
        public System.Windows.Forms.ImageList imageList2;
    }
}

