using System;

namespace TraXile
{
    class TrX_LeagueInfo
    {
        // League start date
        private DateTime _startDate;
        public DateTime Start
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        // League end date
        private DateTime _endDate;
        public DateTime End
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        // Name of the league
        private string _leagueName;
        public string Name
        {
            get { return _leagueName; }
            set { _leagueName = value; }
        }

        // Major version
        private int _major;
        public int Major
        {
            get { return _major; }
            set { _major = value; }
        }

        // Minor version
        private int _minor;
        public int Minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        // Full version
        public string Version => string.Format("{0}.{1}", _major, _minor);

        // Flag: is the current active league?
        private bool _isCurrent;
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public TrX_LeagueInfo(string name, int major, int minor, DateTime start, DateTime end)
        {
            _leagueName = name;
            _major = major;
            _minor = minor;
            _startDate = start;
            _endDate = end;
        }
    }
}
