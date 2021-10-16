using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraXile
{
    class TxSettingsManager
    {
        string _xmlPath;
        Dictionary<string, string> kvStore;

        public TxSettingsManager(string file_path)
        {
            kvStore = new Dictionary<string, string>();
            _xmlPath = file_path;
        }

        public string ReadSetting(string s_key, string s_default = null)
        {
            if(kvStore.ContainsKey(s_key))
            {
                return kvStore[s_key];
            }
            else
            {
                return s_default;
            }
        }

        public void AddOrUpdateSetting(string s_key, string s_value)
        {
            if(kvStore.ContainsKey(s_key))
            {
                kvStore[s_key] = s_value;
            }
            else
            {
                kvStore.Add(s_key, s_value);
            }
        }

        public void LoadFromXml()
        {
            if(File.Exists(_xmlPath))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(_xmlPath);

                foreach (XmlNode n in xml.SelectNodes(@"root/setting"))
                {
                    kvStore.Add(n.Attributes["key"].Value, n.Attributes["value"].Value);
                }
            }
        }

        public void WriteToXml()
        {
            XmlTextWriter wrt = new XmlTextWriter(_xmlPath, Encoding.UTF8);
            wrt.WriteStartDocument();
            wrt.WriteStartElement("root");

            // Write Settings
            foreach(KeyValuePair<string,string> kvp in kvStore)
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
