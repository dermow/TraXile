using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace TraXile
{
    /// <summary>
    /// Event handler definition
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    public delegate void InitializedEventHandler();

    /// <summary>
    /// Parsing handler for Client.txt
    /// </summary>
    class TrX_ClientTxtParser
    {
        /// <summary>
        /// Queue for buffering evetns while live tracking
        /// </summary>
        private ConcurrentQueue<TrX_TrackingEvent> _eventQueue;

        /// <summary>
        /// Last known line hash from Client.txt
        /// </summary>
        private int _lastHash;

        /// <summary>
        /// Path to Client.txt
        /// </summary>
        private string _clientTxtPath;

        /// <summary>
        /// Logger
        /// </summary>
        private ILog _log;

        /// <summary>
        /// Thread for log parsing
        /// </summary>
        private Thread _parsingThread;

        /// <summary>
        /// Thread for reading Q
        /// </summary>
        private Thread _dequeueThread;

        /// <summary>
        /// Is on stream end
        /// </summary>
        private bool _isLive;

        /// <summary>
        /// List of known hashes
        /// </summary>
        private List<int> _hashList;

        /// <summary>
        /// Number of read lines
        /// </summary>
        private int _linesRead;

        /// <summary>
        /// Total number of lines to read
        /// </summary>
        private int _totalLines;

        /// <summary>
        /// Event Mapping
        /// </summary>
        private TrX_EventMapping _eventMapping;

        /// <summary>
        /// Event: Initzialized (reached live status)
        /// </summary>
        public event InitializedEventHandler OnInizialized;

        /// <summary>
        /// DateTimeFormat to use for all time calculations
        /// </summary>
        private DateTimeFormatInfo _dtfi;

        /// <summary>
        /// Main class
        /// </summary>
        private Main _main; //TODO: move event handling in own class instead

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Full path to Client.txt file</param>
        /// <param name="logref">log4net object </param>
        public TrX_ClientTxtParser(string path, ref ILog logref, Main m,  int lasthash = 0)
        {
            _lastHash = lasthash;
            _clientTxtPath = path;
            _log = logref;
            _isLive = false;
            _hashList = new List<int>();
            _linesRead = 0;
            _totalLines = 0;
            _main = m;
            _dtfi = DateTimeFormatInfo.GetInstance(new CultureInfo("en-CA"));
            _dtfi.Calendar = new GregorianCalendar();
            _eventMapping = new TrX_EventMapping();
            _eventQueue = new ConcurrentQueue<TrX_TrackingEvent>();
            _parsingThread = new Thread(new ThreadStart(ParseLogFile));
            _parsingThread.IsBackground = true;
            _parsingThread.Start();
            _dequeueThread = new Thread(new ThreadStart(EventHandling));
            _dequeueThread.IsBackground = true;
            _dequeueThread.Start();
        }

        /// <summary>
        /// Log parsing main loop
        /// </summary>
        private void ParseLogFile()
        {
            _log.Info(string.Format("Start logfile parsing. Path: {0}, Last known hash: {1}", _clientTxtPath, _lastHash));

            bool newContent;
            newContent = (_lastHash == 0);

            // Count lines
            _totalLines = GetLogFileLineCount();

            FileStream fs;
            fs = new FileStream(_clientTxtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using(StreamReader sr = new StreamReader(fs))
            {
                string line;
                int hash;
                DateTime lastEvTime;

                line = "";
                hash = 0;
                lastEvTime = new DateTime();

                while(true)
                {
                    line = sr.ReadLine();

                    // End of stream reached?
                    if(line == null)
                    {
                        if(!_isLive)
                        {
                            // fire event "OnInitialized"
                            OnInizialized();
                        }
                        _isLive = true;

                        Thread.Sleep(100);
                        newContent = true;
                        continue;
                    }

                    hash = line.GetHashCode();

                    // Hash already processed?
                    if(_hashList.Contains(hash))
                    {
                        continue;
                    }

                    if(hash == _lastHash || _lastHash == 0)
                    {
                        newContent = true;
                    }

                    if(!newContent)
                    {
                        _linesRead++;
                        continue;
                    }

                    _lastHash = hash;
                    _linesRead++;
                    // Check if line matches any mapping
                    foreach (KeyValuePair<string, EVENT_TYPES> kvp in _eventMapping.MAP)
                    {
                        if(line.Contains(kvp.Key))
                        {
                            if (!_hashList.Contains(hash))
                            {
                                // Match...
                                TrX_TrackingEvent ev;
                                ev = new TrX_TrackingEvent(kvp.Value);
                                ev.LogLine = line;

                                try
                                {
                                    ev.EventTime = DateTime.Parse(line.Split(' ')[0] + " " + line.Split(' ')[1], _dtfi);
                                    lastEvTime = ev.EventTime;
                                }
                                catch
                                {
                                    ev.EventTime = lastEvTime;
                                }
                                _hashList.Add(hash);

                                if (!_isLive)
                                {
                                    _main.HandleSingleEvent(ev, true);
                                }
                                else
                                {
                                    _eventQueue.Enqueue(ev);
                                }
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Handle events - Read Queue
        /// </summary>
        private void EventHandling()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (true)
                {
                    while (_eventQueue.TryDequeue(out TrX_TrackingEvent deqEvent))
                    {
                        _main.HandleSingleEvent(deqEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Get line count from Client.txt. Used for progress calculation
        /// </summary>
        /// <returns></returns>
        private int GetLogFileLineCount()
        {
            int iCount = 0;
            FileStream fs1 = new FileStream(_clientTxtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TextReader reader1 = new StreamReader(fs1);
            while ((reader1.ReadLine()) != null)
            {
                iCount++;
            }
            reader1.Close();
            return iCount;
        }

        /// <summary>
        /// Hash of last parsed line
        /// </summary>
        public int LastHash
        {
            get { return _lastHash; }
        }

        /// <summary>
        /// Number of lines read
        /// </summary>
        public int LogLinesRead
        {
            get { return _linesRead; }
        }

        /// <summary>
        /// Total number of lines
        /// </summary>
        public int LogLinesTotal
        {
            get { return _totalLines; }
        }
        
    }
}
