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
        bool bStarted, bExit;
        string _myAppData;
        string _targetVersion;

        public Form1()
        {
            InitializeComponent();
           
            //TEST: Create folder in userdata
            _myAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TraXile";

            if (!Directory.Exists(_myAppData))
            {
                Directory.CreateDirectory(_myAppData);
            }

            try
            {
                _targetVersion = Environment.GetCommandLineArgs()[1];
                timer1.Interval = 2000;
                timer1.Start();
            }
            catch
            {
                Log("No target release specified!");
            }
            

        }

        private void Log(string text)
        {
            textBox1.Text += text + Environment.NewLine;
        }

        private void Check()
        {
            StartUpdate(_targetVersion);
        }

        private void StartUpdate(string s_version)
        {
            Log("Downloading https://github.com/dermow/TraXile/releases/download/" + s_version + @"/Setup.msi");
            WebClient wc = new WebClient();
            Uri uri = new Uri("https://github.com/dermow/TraXile/releases/download/" + s_version + @"/Setup.msi");
            wc.DownloadFile(uri, _myAppData + @"\Setup_" + s_version + ".msi");
            Log("Download successful. Saved installer to: " + _myAppData);

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

            Log("Installing to: " + Application.StartupPath);
            
            try
            { 
                Process process = new Process();
                process.StartInfo.FileName = "msiexec";
                process.StartInfo.WorkingDirectory = _myAppData;
                process.StartInfo.Arguments = " /quiet /i Setup_" + s_version + ".msi TARGETDIR='" + Application.StartupPath + "'";
                process.Start();
                process.WaitForExit(60000);

                Log("Update successful. Starting TraXile again.");

                process = new Process();
                process.StartInfo.FileName = "TraXile.exe";
                process.StartInfo.WorkingDirectory = Application.StartupPath;
                process.Start();
                process.WaitForExit(60000);

            }
            catch(Exception ex)
            {
                Log("Update failed: " + ex.Message);
            }
           
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
