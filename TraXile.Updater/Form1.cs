using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;

namespace TraXile.Updater
{
    public partial class Form1 : Form
    {
        private string sURL = "https://api.github.com/repos/dermow/TraXile/releases";
        string sTraXileVersion = "";
        bool bStarted, bExit;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 2000;
            timer1.Start();
        }

        private void Log(string text)
        {
            textBox1.Text += text + Environment.NewLine;
        }

        private string ReadCurrentVerison()
        {
            try
            {
                StreamReader rd = new StreamReader("version");
                string s = rd.ReadToEnd();
                return s;
            }
            catch
            {
                return "0.0.0";
            }
           
        }

        private string GetLatestRelease()
        {
            const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";
            WebClient webClient = new WebClient();
            // Added user agent
            webClient.Headers.Add("User-Agent", "Unity web player");
            Uri uri = new Uri(string.Format(GITHUB_API, "dermow", "TraXile"));
            string releases = webClient.DownloadString(uri);
            int iIndex = releases.IndexOf("tag_name");
            return releases.Substring(iIndex+11, 5);
        }

        private void Check()
        {
            Log("Checking for Updates...");
            sTraXileVersion = ReadCurrentVerison();
            string sNewVersion = GetLatestRelease();

            int iCurrent = Convert.ToInt32(sTraXileVersion.Replace(".", ""));
            int iNewest = Convert.ToInt32(sNewVersion.Replace(".", ""));

            if (iNewest > iCurrent)
            {
                StartUpdate(sNewVersion);
                Log("New release available: " + sNewVersion + ". Current = " + sTraXileVersion);
            }
            else
            {
                Log("TraXile is up to date :)");
                bExit = true;
            }

            bExit = true;
        }

        private void StartUpdate(string s_version)
        {
            Log("Downloading https://github.com/dermow/TraXile/releases/download/" + s_version + @"/" + s_version + ".zip");
            WebClient wc = new WebClient();
            Uri uri = new Uri("https://github.com/dermow/TraXile/releases/download/" + s_version + @"/" + s_version + ".zip");
            wc.DownloadFile(uri, s_version + ".zip");
            Process[] p = Process.GetProcessesByName("TraXile");
            Log(s_version + ".zip downloaded successfully");

            if (p.Length > 0)
            {
                Log("TraXile is running and will be closed now.");
                foreach(Process pr in p)
                {
                    pr.Kill();
                }
                Thread.Sleep(2000);
                Log("TraXile closed successfully");
            }

            // Preserve config
            if(File.Exists("TraXile.exe.config"))
            {
                File.Copy("TraXile.exe.config", "TraXile.exe.config.backup", true);
            }

            ZipArchive zip = ZipFile.OpenRead(s_version + ".zip");
            
            foreach(ZipArchiveEntry file in zip.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(Application.StartupPath, file.FullName));
                Log("Extract:" + completeFileName);

                if (!completeFileName.StartsWith(Application.StartupPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }

            // Preserve config
            if (File.Exists("TraXile.exe.config.backup"))
            {
                File.Copy("TraXile.exe.config.backup", "TraXile.exe.config", true);
            }

            DialogResult dr = MessageBox.Show("Update successful. Should TraXile be restarted now?", "Success", MessageBoxButtons.YesNo);
            if(dr == DialogResult.Yes)
            {
                Process.Start("TraXile.exe");
            }
         }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!bStarted)
            {
                bStarted = true;
                Check();
            }

            if(bExit)
            {
                Application.Exit();
            }
        }
    }
}
