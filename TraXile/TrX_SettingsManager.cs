using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using log4net;

namespace TraXile
{
    class TrX_SettingsManager
    {
        // Patah to XML file
        readonly string _xmlPath;

        private readonly ILog _log;

        // Key value store
        public readonly Dictionary<string, string> kvStore;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file_path"></param>
        public TrX_SettingsManager(string file_path)
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _log.Info("TrX_SettingsManager initialized.");
            kvStore = new Dictionary<string, string>();
            _xmlPath = file_path;
        }

        /// <summary>
        /// Read setting of return default value
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="s_default"></param>
        /// <returns></returns>
        public string ReadSetting(string s_key, string s_default = null)
        {
            if (kvStore.ContainsKey(s_key))
            {
                return kvStore[s_key];
            }
            else
            {
                return s_default;
            }
        }

        /// <summary>
        /// Add or update a setting
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="s_value"></param>
        public void AddOrUpdateSetting(string s_key, string s_value)
        {
            bool hasChanged = false;

            if (kvStore.ContainsKey(s_key))
            {
                // Logging
                if(kvStore[s_key] != s_value)
                {
                    string sOldValue = kvStore[s_key];
                    kvStore[s_key] = s_value;
                    hasChanged = true;

                    _log.Debug($"Changed setting '{s_key}': '{sOldValue}' -> '{s_value}'");
                }
            }
            else
            {
                kvStore.Add(s_key, s_value);
                hasChanged = true;
            }

            if(hasChanged)
            {
                WriteToXml();
            }
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public void LoadFromXml()
        {
            _log.Debug("Loading settings from config.xml");

            if (File.Exists(_xmlPath))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(_xmlPath);

                foreach (XmlNode n in xml.SelectNodes(@"root/setting"))
                {
                    kvStore.Add(n.Attributes["key"].Value, n.Attributes["value"].Value);
                }

                _log.Debug("Loaded settings from config.xml");
            }
        }

        /// <summary>
        /// Write settings to file
        /// </summary>
        public void WriteToXml()
        {
            XmlTextWriter wrt = new XmlTextWriter(_xmlPath, Encoding.UTF8);
            wrt.WriteStartDocument();
            wrt.WriteStartElement("root");
            wrt.Formatting = Formatting.Indented;
            wrt.Indentation = 4;

            // Write Settings
            foreach (KeyValuePair<string, string> kvp in kvStore)
            {
                wrt.WriteStartElement("setting");
                wrt.WriteAttributeString("key", kvp.Key);
                wrt.WriteAttributeString("value", kvp.Value);
                wrt.WriteEndElement();
            }

            wrt.WriteEndElement();
            wrt.WriteEndDocument();
            wrt.Close();

            // Debug empty config.xml problem
            long configXmlSize = new FileInfo(_xmlPath).Length;
            _log.Debug($"Written settings to config.xml, file size is {configXmlSize} bytes.");
        }
    }
}
