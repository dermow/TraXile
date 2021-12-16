using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    class TrX_LeagueInfo
    {
        private string _leagueName;
        private int _major;
        private int _minor;
        private DateTime _startDate;
        private DateTime _endDate;
        private bool _isCurrent;

        public TrX_LeagueInfo(string name, int major, int minor, DateTime start, DateTime end)
        {
            _leagueName = name;
            _major = major;
            _minor = minor;
            _startDate = start;
            _endDate = end;
        }

        public DateTime Start
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime End
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public string Name
        {
            get { return _leagueName; }
            set { _leagueName = value; }
        }

        public int Major
        {
            get { return _major; }
            set { _major = value; }
        }

        public int Minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        public string Version
        {
            get { return string.Format("{0}.{1}", _major, _minor); }
        }

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { _isCurrent = value; }
        }
    }
}
