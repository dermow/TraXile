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
        string sTraXileVersion = "";
        bool bStarted, bExit;
        string _myAppData;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 2000;
            timer1.Start();

            //TEST: Create folder in userdata
            _myAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TraXile";

            if (!Directory.Exists(_myAppData))
            {
                Directory.CreateDirectory(_myAppData);
            }

        }

        private void Log(string text)
        {
            textBox1.Text += text + Environment.NewLine;
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
            StartUpdate(GetLatestRelease());
        }

        private void StartUpdate(string s_version)
        {
            Log("Downloading https://github.com/dermow/TraXile/releases/download/" + s_version + @"/Setup.msi");
            WebClient wc = new WebClient();
            Uri uri = new Uri("https://github.com/dermow/TraXile/releases/download/" + s_version + @"/Setup.msi");
            wc.DownloadFile(uri, _myAppData + @"\Setup_" + s_version + ".msi");
            Process[] p = Process.GetProcessesByName("TraXile");

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

            // Run setup
            Process.Start(_myAppData + @"\Setup_" + s_version + ".msi");
            bExit = true;
         }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!bStarted)
            {
                bStarted = true;

                try
                {
                    Check();
                }
                catch(Exception ex)
                {
                    Log("ERROR: Could not update: " + ex.Message);
                    Log(Environment.NewLine);
                    Log(ex.ToString());
                }
                
            }

            if(bExit)
            {
                Application.Exit();
            }
        }
    }
}
