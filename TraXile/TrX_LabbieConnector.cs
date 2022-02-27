using log4net;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TraXile
{
    /// <summary>
    /// History initizalized event handler
    /// </summary>
    public delegate void Trx_LabbieEventHandler(TrX_LabbieEventArgs e);

    /// <summary>
    /// Event args for Activity based events
    /// </summary>
    public class TrX_LabbieEventArgs : EventArgs
    {
        // Core logic
        private List<TrX_LabEnchant> _enchants;
        public List<TrX_LabEnchant> Enchants => _enchants;

        public TrX_LabbieEventArgs(List<TrX_LabEnchant> enchants)
        {
            _enchants = enchants;
        }
    }

    public class TrX_LabbieConnector
    {
        // log
        private ILog _log;

        // events
        public event Trx_LabbieEventHandler EnchantsReceived;

        // Path to labbie log file
        private string _labbieLogPath;
        public string LabbieLogPath
        {
            get { return _labbieLogPath; }
            set { _labbieLogPath = value; }
        }
        public string LabbieLogFilePath => _labbieLogPath + @"\enchants.jsonl";

        // Thread for file reading
        private Thread _readThread;

        // NLast modification date for labbie log
        private DateTime _lastFileTimestamp;

        // All known enchants
        private List<TrX_LabEnchant> _knownEnchants;
        public List<TrX_LabEnchant> KnownEnchants => _knownEnchants;

        // DB Handler
        private TrX_DataBackend _dataBackend;

        // Started?
        private bool _started;
        public bool IsStarted => _started;

        // Constructor
        public TrX_LabbieConnector(TrX_DataBackend backend, ref ILog log)
        {
            _log = log;
            _knownEnchants = new List<TrX_LabEnchant>();
            _dataBackend = backend;
            LoadKnownEnchantsFromSQLite();
        }

        public TrX_LabEnchant GetEnchantByID(int id)
        {
            for(int i = 0; i < _knownEnchants.Count; i++)
            {
                if(_knownEnchants[i].ID == id)
                {
                    return _knownEnchants[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Get Fileinfo object for Labbie Log
        /// </summary>
        /// <returns></returns>
        public FileInfo GetFileInfo()
        {
            return new FileInfo(LabbieLogFilePath);
        }

        /// <summary>
        /// Start receiving enchants
        /// </summary>
        public void Start()
        {
            try
            {
                _lastFileTimestamp = GetFileInfo().LastWriteTime;
                _readThread = new Thread(new ThreadStart(FileReading));
                _readThread.IsBackground = true;
                _readThread.Name = "LabbieThread";
                _readThread.Start();
                _started = true;
            }
            catch(Exception e)
            {
                _log.Error("Could not start Labbie connector: " + e.Message);
                _log.Debug(e.ToString());
            }
           
        }

        /// <summary>
        /// Read labbie log
        /// </summary>
        public void FileReading()
        {
            DateTime dt;
            string lastLine = "";

            while(true)
            {
                Thread.Sleep(5000);

                try
                {
                    dt = GetFileInfo().LastWriteTime;
                    if (dt > _lastFileTimestamp)
                    {
                        string line = TrX_Helpers.GetLastLineOfFile(LabbieLogFilePath);

                        if (line == lastLine)
                        {
                            _lastFileTimestamp = dt;
                            continue;
                        }

                        lastLine = line;

                        _log.Info("Labbie: New enchants looked up");
                        _log.Info("Enchants: " + lastLine);
                        List<string> enchantsText;
                        List<TrX_LabEnchant> enchants;
                        enchants = new List<TrX_LabEnchant>();
                        enchantsText = GetListFromJSONLine(lastLine);

                        TrX_LabEnchant enchant;
                        foreach(string s in enchantsText)
                        {
                            enchant = GetEnchantObjectForText(s);
                            if(enchant == null)
                            {
                                enchant = new TrX_LabEnchant();
                                enchant.Text = GetValidEnchantName(s);
                                enchant.ID = AddKnownEnchant(enchant.Text);
                                _knownEnchants.Add(enchant);
                            }

                            enchants.Add(enchant);
                        }

                        EnchantsReceived(new TrX_LabbieEventArgs(enchants));
                    }

                    _lastFileTimestamp = dt;
                }
                catch(Exception e)
                {
                    _log.Error("Could not read Labbie enchants: " + e.Message);
                    _log.Debug(e.ToString());
                }
            }

        }

        /// <summary>
        /// Check if a given enchant is already known
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        public TrX_LabEnchant GetEnchantObjectForText(string enchant)
        {
            foreach(TrX_LabEnchant en in _knownEnchants)
            {
                if(en.Text == enchant)
                {
                    return en;
                }
            }
            return null;
        }


        /// <summary>
        /// Remove not allowed chars
        /// </summary>
        /// <param name="enchant"></param>
        /// <returns></returns>
        private string GetValidEnchantName(string enchant)
        {
            string validName;
            validName = enchant.Replace("'", "");
            validName = validName.Replace("\"", "");
            return validName;
        }

        /// <summary>
        /// Add enchant to the list
        /// </summary>
        /// <param name="enchant"></param>
        public int AddKnownEnchant(string enchant)
        {
            int rowID;
            _dataBackend.DoNonQuery(string.Format("INSERT INTO tx_lab_enchants(enchant_text) VALUES ('{0}')", enchant));
            rowID = Convert.ToInt32(_dataBackend.GetSingleValue("SELECT last_insert_rowid()"));
            return rowID;
        }

        /// <summary>
        /// Read known enchants from sqlite
        /// </summary>
        private void LoadKnownEnchantsFromSQLite()
        {
            SqliteDataReader reader;
            reader = _dataBackend.GetSQLReader("SELECT * FROM tx_lab_enchants");

            TrX_LabEnchant enchant;
            while(reader.Read())
            {
                enchant = new TrX_LabEnchant() { ID = reader.GetInt32(0), Text = reader.GetString(1) };
                _knownEnchants.Add(enchant);
            }

            string s = "";
        }

        /// <summary>
        /// Get String list from labbie log line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public List<string> GetListFromJSONLine(string line)
        {
            List<string> results;
            results = new List<string>();

            string[] spl;
            spl = line.Split(new string[] { "\"," }, StringSplitOptions.None);
            string enchant;

            foreach(string s in spl)
            {
                enchant = s.Replace("[", "");
                enchant = enchant.Replace("]", "");
                enchant = enchant.Replace(Environment.NewLine, "");
                results.Add(GetValidEnchantName(enchant));
            }

            return results;
        }
       
    }
}
