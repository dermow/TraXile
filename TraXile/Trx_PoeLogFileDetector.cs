using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using log4net;
using System.Text.RegularExpressions;

namespace TraXile
{
    internal class Trx_PoeLogFileDetector
    {
        private string _steamRegistryPath32bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
        private string _steamRegistryPath64bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
        private string _poeStandaloneRegistryPath32bit = @"HKEY_CURRENT_USER\SOFTWARE\GrindingGearGames\Path of Exile";
        private string _poeStandaloneRegistryPath64bit = @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\GrindingGearGames\Path of Exile";
        private ILog _log;

        public Trx_PoeLogFileDetector()
        {
            _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private string GetSteamInstallationPath()
        {
            string installPath = null;
            installPath = TrX_Helpers.GetRegistryValue(_steamRegistryPath32bit, "InstallPath", null);
            
            if(String.IsNullOrEmpty(installPath))
            {
                installPath = TrX_Helpers.GetRegistryValue(_steamRegistryPath64bit, "InstallPath", null);
            }

            return installPath;
        }

        private string GetPoeStandaloneInstallPath()
        {
            string installPath = null;
            installPath = TrX_Helpers.GetRegistryValue(_poeStandaloneRegistryPath32bit, "InstallLocation", null);

            if (String.IsNullOrEmpty(installPath))
            {
                installPath = TrX_Helpers.GetRegistryValue(_poeStandaloneRegistryPath64bit, "InstallLocation", null);
            }

            return installPath;
        }

        private List<string> GetSteamLibraryPathes(string steamPath)
        {
            List<string> pathes = new List<string>();
            try
            {
                string[] data = File.ReadAllLines($@"{steamPath}\config\libraryfolders.vdf", Encoding.UTF8);

                foreach(string s in data)
                {
                    if(s.Contains("path"))
                    {
                        string tmp1 = Regex.Replace(s, @"[\r\n\t ]+", " ");
                        tmp1 = tmp1.Replace(@"\\", @"\");
                        tmp1 = tmp1.Split(new string[] { "path" }, StringSplitOptions.None)[1];
                        tmp1 = tmp1.Replace("\"", "").Trim();
                        _log.Debug($"found steam lib: {tmp1}");

                        pathes.Add($@"{tmp1}\steamapps\common");
                    }
                }
                  
              
            }
            catch(Exception ex)
            {
                _log.Warn($"Error discovering steam libraries: {ex.Message}");
                _log.Debug(ex.ToString());
            }

            return pathes;
        }

        public List<string> AutoDiscoverPoeLogs()
        {
            // Results
            List<string> results = new List<string>();

            // Searchlist
            List<string> searchList = new List<string>();

            // Get pathes for Steam Client
            string steamInstallPath = GetSteamInstallationPath();
            List<string> steamLibs = new List<string>();

            // Always look at default steam path
            steamLibs.Add($@"{steamInstallPath}\steamapps\common");


            // if steam path found, discover libraries
            if (!String.IsNullOrEmpty(steamInstallPath))
            {
                steamLibs = GetSteamLibraryPathes(steamInstallPath);
            }

            // Always look at default steam path
            searchList.Add($@"{steamInstallPath}\steamapps\common\Path of Exile\logs\Client.txt");

            // Add all stem libs
            foreach (string lib in steamLibs)
            {
                string logPath = $@"{lib}\Path of Exile\logs\Client.txt";
                if(!searchList.Contains(logPath))
                {
                    searchList.Add(logPath); 
                }
            }

            // Add standalone path
            string poeStandalonePath = GetPoeStandaloneInstallPath();
            if (!String.IsNullOrEmpty(poeStandalonePath))
            {
                searchList.Add($@"{poeStandalonePath}\logs\Client.txt");
            }

            // Do search
            foreach(string search in searchList)
            {
                if(File.Exists(search))
                {
                    results.Add(search);
                }
            }

            return results;
        }
    }
}
