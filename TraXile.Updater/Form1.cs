using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile.Updater
{
    public partial class Form1 : Form
    {
        // The one HttpClient of the updater process
        private static readonly HttpClient Http = new HttpClient();

        bool bStarted, bExit;
        readonly string _myAppData;
        readonly string _targetVersion;

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
                //_targetVersion = "0.5.2";
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

        private async Task StartUpdateAsync(string s_version)
        {
            Uri uri = new Uri("https://github.com/dermow/TraXile/releases/download/" + s_version + @"/Setup.msi");
            Log("Downloading " + uri);
            using (HttpResponseMessage response = await Http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                using (FileStream file = File.Create(_myAppData + @"\Setup_" + s_version + ".msi"))
                {
                    await response.Content.CopyToAsync(file);
                }
            }
            Log("Download successful. Saved installer to: " + _myAppData);

            // TraXile 3.0 (TraXile.Next) closes itself after starting the updater; kill what is still running
            Process[] p = Process.GetProcessesByName("TraXile");
            p = p.Concat(Process.GetProcessesByName("TraXile.Next")).ToArray();

            if (p.Length > 0)
            {
                Log("TraXile is running and will be closed now.");
                foreach (Process pr in p)
                {
                    pr.Kill();
                }
                await Task.Delay(2000);
                Log("TraXile closed successfully");
            }

            Log("Installing to: " + Application.StartupPath);

            try
            {

                Process p1 = new Process();
                p1.StartInfo.FileName = _myAppData + @"\Setup_" + s_version + ".msi";
                // An .msi is not an executable; it needs the shell (not the default on .NET)
                p1.StartInfo.UseShellExecute = true;
                p1.Start();
                bExit = true;

            }
            catch (Exception ex)
            {
                Log("Update failed: " + ex.Message);
            }

        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (!bStarted)
            {
                bStarted = true;

                try
                {
                    await StartUpdateAsync(_targetVersion);
                }
                catch (Exception ex)
                {
                    Log("ERROR: Could not update: " + ex.Message);
                    Log(Environment.NewLine);
                    Log(ex.ToString());
                }

            }

            if (bExit)
            {
                Application.Exit();
            }
        }
    }
}
