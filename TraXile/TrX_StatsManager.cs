using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    class TrX_StatsManager
    {
        private TrX_DBManager _myDB;
        private Dictionary<string, int> _numericStats;

        public TrX_StatsManager(TrX_DBManager db)
        {
            _myDB = db;
            _numericStats = new Dictionary<string, int>();
        }

        public long GetOldestTimeStamp()
        {
            string q1 = string.Format("SELECT timestamp FROM tx_stats  ORDER BY timestamp ASC limit 1");
            SqliteDataReader dr1;
            dr1 = _myDB.GetSQLReader(q1);
            long result = 0;

            while (dr1.Read())
            {
                result = dr1.GetInt64(0);
            }

            return result;
        }

        public List<KeyValuePair<long, int>> GetByDayValues(string stat_name, long ts1, long ts2, int interval = 1)
        {
            List<KeyValuePair<long, int>> results = new List<KeyValuePair<long, int>>();
            DateTime start = DateTimeOffset.FromUnixTimeSeconds(ts1).DateTime;
            DateTime end = DateTimeOffset.FromUnixTimeSeconds(ts2).DateTime;

            DateTime curr = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            DateTime to = curr.AddHours(24);

            //Add first

            long t1 = ((DateTimeOffset)curr).ToUnixTimeSeconds();
            long t2 = ((DateTimeOffset)to).ToUnixTimeSeconds();

            double val = GetCount(stat_name, t1, t2);

            results.Add(new KeyValuePair<long, int>(t1, Convert.ToInt32(val)));
            

            if(ts2 > ts1)
            {
                bool fin = false;
                while(!fin)
                {
                    curr = curr.AddDays(interval);
                    to = curr.AddHours(24);
                    t1 = ((DateTimeOffset)curr).ToUnixTimeSeconds();
                    t2 = ((DateTimeOffset)to).ToUnixTimeSeconds();
                    val = GetCount(stat_name, t1, t2);
                    results.Add(new KeyValuePair<long, int>(t1, Convert.ToInt32(val)));

                    if(t1 > ts2)
                    {
                        fin = true;
                    }

                }
            }


            return results;
        }

        public List<KeyValuePair<long,int>> GetTimeSeries(string stat_name, long ts1, long ts2)
        {
            List<KeyValuePair<long, int>> results = new List<KeyValuePair<long, int>>();
            string q1 = string.Format("SELECT timestamp, stat_value FROM tx_stats WHERE stat_name = '{0}' AND timestamp between {1} and {2} ORDER BY timestamp ASC", stat_name, ts1, ts2);
            SqliteDataReader dr1;
            dr1 = _myDB.GetSQLReader(q1);

            while(dr1.Read())
            {
                KeyValuePair<long, int> kvp = new KeyValuePair<long, int>(dr1.GetInt64(0), dr1.GetInt32(1));
                results.Add(kvp);
            }

            return results;
        }


        public int GetLastValue(string stat_name)
        {
            return _numericStats[stat_name];
        }

        public double GetCount(string stat_name, long ts1, long ts2)
        {
            //Request HO time first
            string q1 = string.Format("SELECT COUNT(*) FROM tx_stats WHERE stat_name = '{0}' AND timestamp between {1} and {2}", stat_name, ts1, ts2);
            SqliteDataReader dr1;
            dr1 = _myDB.GetSQLReader(q1);

            int val1 = 0;

            while (dr1.Read())
            {
                val1 = dr1.GetInt32(0);
            }

            return val1;
        }

        public double GetIncrementValue(string stat_name, long ts1, long ts2)
        {
            //Request HO time first
            string q1 = string.Format("SELECT min(stat_value) FROM tx_stats WHERE stat_name = '{0}' AND timestamp between {1} and {2}", stat_name, ts1, ts2);
            string q2 = string.Format("SELECT max(stat_value) FROM tx_stats WHERE stat_name = '{0}' AND timestamp between {1} and {2}", stat_name, ts1, ts2);
            SqliteDataReader dr1, dr2;
            dr1 = _myDB.GetSQLReader(q1);
            dr2 = _myDB.GetSQLReader(q2);

            int val1 = 0;
            int val2 = 0;

            while (dr1.Read())
            {
                if(dr1.GetValue(0) != DBNull.Value)
                {
                    val1 = dr1.GetInt32(0);
                }
            }

            while (dr2.Read())
            {
                if (dr2.GetValue(0) != DBNull.Value)
                {
                    val2 = dr2.GetInt32(0);
                }
            }

            if (val1 == 1)
                val1 = 0;

            return (val2 - val1);
        }

        public void IncrementStat(string s_key, DateTime dt, int i_value = 1)
        {
            _numericStats[s_key] += i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
        }

        public void SetStat(string s_key, DateTime dt, int i_value)
        {
            _numericStats[s_key] = i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
        }

        public Dictionary<string, int> NumericStats
        {
            get { return _numericStats; }
            set { _numericStats = value; }
        }

       

    }
}
