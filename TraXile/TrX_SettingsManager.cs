using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TraXile
{
    class TrX_SettingsManager
    {
        // Patah to XML file
        readonly string _xmlPath;

        // Key value store
        public readonly Dictionary<string, string> kvStore;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file_path"></param>
        public TrX_SettingsManager(string file_path)
        {
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
            if (kvStore.ContainsKey(s_key))
            {
                kvStore[s_key] = s_value;
            }
            else
            {
                kvStore.Add(s_key, s_value);
            }
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public void LoadFromXml()
        {
            if (File.Exists(_xmlPath))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(_xmlPath);

                foreach (XmlNode n in xml.SelectNodes(@"root/setting"))
                {
                    kvStore.Add(n.Attributes["key"].Value, n.Attributes["value"].Value);
                }
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
        }
    }
}
