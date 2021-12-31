using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public delegate void FiringEventHandler(object source, FiringEventArgs e);

    /// <summary>
    /// Event args for event "FiringEvent"
    /// </summary>
    public class FiringEventArgs : EventArgs
    {
        private TrX_TrackingEvent trxEv;

        public FiringEventArgs(TrX_TrackingEvent trackingEvent)
        {
            trxEv = trackingEvent;
        }

        public TrX_TrackingEvent TrackingEvent
        {
            get { return trxEv; }
        }
    }

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
        /// Constructor
        /// </summary>
        /// <param name="path">Full path to Client.txt file</param>
        /// <param name="logref">log4net object </param>
        public TrX_ClientTxtParser(string path, ref ILog logref, int lasthash = 0)
        {
            _lastHash = lasthash;
            _clientTxtPath = path;
            _log = logref;
            _parsingThread = new Thread(new ThreadStart(ParseLogFile));
            _parsingThread.IsBackground = true;
            _parsingThread.Name = typeof(TrX_ClientTxtParser).ToString();
            _parsingThread.Start();
        }

        /// <summary>
        /// Log parsing main loop
        /// </summary>
        private void ParseLogFile()
        {
            _log.Info(string.Format("Start logfile parsing. Path: {0}, Last known hash: {1}", _clientTxtPath, _lastHash));

            bool newContent;
            newContent = (_lastHash == 0);


        }
        
    }
}
