using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace TraXile
{
    public class TrX_StatsManager
    {
        // Database manager
        private readonly TrX_DataBackend _myDB;

        // Numeric stats dictionary
        private Dictionary<string, int> _numericStats;
        public Dictionary<string, int> NumericStats
        {
            get { return _numericStats; }
            set { _numericStats = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db"></param>
        public TrX_StatsManager(TrX_DataBackend db)
        {
            _myDB = db;
            _numericStats = new Dictionary<string, int>();
        }

        /// <summary>
        /// Read the oldes available timestamp from DB
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get values on day bases
        /// </summary>
        /// <param name="stat_name"></param>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public List<KeyValuePair<long, int>> GetByDayValues(string stat_name, long ts1, long ts2, int interval = 1)
        {
            List<KeyValuePair<long, int>> results = new List<KeyValuePair<long, int>>();
            DateTime start = DateTimeOffset.FromUnixTimeSeconds(ts1).DateTime;

            DateTime curr = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            DateTime to = curr.AddHours(24);

            long t1 = ((DateTimeOffset)curr).ToUnixTimeSeconds();
            long t2 = ((DateTimeOffset)to).ToUnixTimeSeconds();
            double val = GetCount(stat_name, t1, t2);

            results.Add(new KeyValuePair<long, int>(t1, Convert.ToInt32(val)));

            if (ts2 > ts1)
            {
                bool fin = false;
                while (!fin)
                {
                    curr = curr.AddDays(interval);
                    to = curr.AddHours(24);
                    t1 = ((DateTimeOffset)curr).ToUnixTimeSeconds();
                    t2 = ((DateTimeOffset)to).ToUnixTimeSeconds();
                    val = GetCount(stat_name, t1, t2);
                    results.Add(new KeyValuePair<long, int>(t1, Convert.ToInt32(val)));

                    if (t1 > ts2)
                    {
                        fin = true;
                    }

                }
            }
            return results;
        }

        /// <summary>
        /// Get timeseries
        /// </summary>
        /// <param name="stat_name"></param>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
        public List<KeyValuePair<long, int>> GetTimeSeries(string stat_name, long ts1, long ts2)
        {
            List<KeyValuePair<long, int>> results = new List<KeyValuePair<long, int>>();
            string q1 = string.Format("SELECT timestamp, stat_value FROM tx_stats WHERE stat_name = '{0}' AND timestamp between {1} and {2} ORDER BY timestamp ASC", stat_name, ts1, ts2);
            SqliteDataReader dr1;
            dr1 = _myDB.GetSQLReader(q1);

            while (dr1.Read())
            {
                KeyValuePair<long, int> kvp = new KeyValuePair<long, int>(dr1.GetInt64(0), dr1.GetInt32(1));
                results.Add(kvp);
            }

            return results;
        }

        /// <summary>
        /// Get most recent value of statistic
        /// </summary>
        /// <param name="stat_name"></param>
        /// <returns></returns>
        public int GetLastValue(string stat_name)
        {
            return _numericStats[stat_name];
        }

        /// <summary>
        /// Get coung of entries for statistic
        /// </summary>
        /// <param name="stat_name"></param>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the increment value between 2 timestamps
        /// </summary>
        /// <param name="stat_name"></param>
        /// <param name="ts1"></param>
        /// <param name="ts2"></param>
        /// <returns></returns>
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
                if (dr1.GetValue(0) != DBNull.Value)
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

        /// <summary>
        /// Increment given stat by value
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="dt"></param>
        /// <param name="i_value"></param>
        public void IncrementStat(string s_key, DateTime dt, int i_value = 1)
        {
            _numericStats[s_key] += i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
        }

        /// <summary>
        /// Add svalue to statistic 
        /// </summary>
        /// <param name="s_key"></param>
        /// <param name="dt"></param>
        /// <param name="i_value"></param>
        public void SetStat(string s_key, DateTime dt, int i_value)
        {
            _numericStats[s_key] = i_value;
            _myDB.DoNonQuery("INSERT INTO tx_stats (timestamp, stat_name, stat_value) VALUES (" + ((DateTimeOffset)dt).ToUnixTimeSeconds() + ", '" + s_key + "', " + _numericStats[s_key] + ")");
        }

       
    }
}
