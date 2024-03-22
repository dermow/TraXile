using log4net;
using Microsoft.Data.Sqlite;
using System;

namespace TraXile
{
    public class TrX_DataBackend
    {
        // SQLite connector
        private SqliteConnection _dbConnection;

        // Log
        private readonly ILog _log;

        // DB Path
        private readonly string _dbPath;
        public string DatabasePath => _dbPath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="s_file"></param>
        /// <param name="log"></param>
        public TrX_DataBackend(string s_file, ref ILog log)
        {
            _dbPath = s_file;
            _log = log;
            Init();
            Patch();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Init()
        {
            _dbConnection = new SqliteConnection("Data Source=" + _dbPath);
            _dbConnection.Open();

            //Create Tables
            SqliteCommand cmd;

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_activity_log " +
                "(timestamp int, " +
                "act_type text, " +
                "act_area text, " +
                "act_stopwatch int, " +
                "act_deathcounter int," +
                "act_ulti_rounds int," +
                "act_is_zana int," +
                "act_tags" + ")";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_tags " +
                "(tag_id text, " +
                "tag_display text," +
                "tag_bgcolor text, " +
                "tag_forecolor text," +
                "tag_type text," +
                "tag_show_in_lv int default 0)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create unique index if not exists tx_tag_id on tx_tags(tag_id)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_stats " +
                "(timestamp int, " +
                "stat_name text, " +
                "stat_value int)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_stats_cache " +
                "(" +
                "stat_name text, " +
                "stat_value int)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_known_players " +
                "(" +
                "player_name )";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_lab_enchants " +
                "(" +
                "enchant_id integer primary key autoincrement," +
                "enchant_text text)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_enchant_history " +
                "(" +
                "lab_timestamp integer, " +
                "enchant_id integer, " + 
                "action text)";
            cmd.ExecuteNonQuery();

            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "create table if not exists tx_enchant_notes " +
                "(" +
                "lab_timestamp integer, " +
                "enchant_id integer, " +
                "enchant_note text)";
            cmd.ExecuteNonQuery();
            
            Patch();
        }

        /// <summary>
        /// Apply DB patches
        /// </summary>
        public void Patch()
        {
            SqliteCommand cmd;
            // Update 0.3.4
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_tags text";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.3.4 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.4.5
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_area_level int default 0";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.4.5 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.5.2
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_success int default 0";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.5.2 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.5.2
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "alter table tx_tags add column tag_show_in_lv int default 0";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.5.2 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.7.0
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "alter table tx_activity_log add column act_pause_time int default 0";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.7.0 -> " + cmd.CommandText);
            }
            catch
            {
            }

            // Update 0.7.5
            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "create table if not exists tx_kvstore" +
                    "(" +
                    "key text, " +
                    "value text)";
                cmd.ExecuteNonQuery();
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = "create unique index if not exists kvstore on tx_kvstore(key)";
                cmd.ExecuteNonQuery();
                _log.Info("PatchDatabase 0.7.5 -> " + cmd.CommandText);
                DoNonQuery("INSERT INTO tx_kvstore (key, value) VALUES ('last_hash', '0')", false);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Set KV value, add if not existing
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetKVStoreValue(string key, string value)
        {
            string query;
            if(!CheckIfKVEntryExists(key))
            {
                query = string.Format("INSERT INTO tx_kvstore (key, value) VALUES ('{0}', '{1}')", key, value);
            }
            else
            {
                query = string.Format("UPDATE tx_kvstore SET value = '{0}' WHERE key = '{1}'", value, key);
            }
            DoNonQuery(query, true);
        }

        /// <summary>
        /// Check if KV store has entry with specific key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CheckIfKVEntryExists(string key)
        {
            SqliteDataReader reader;
            reader = GetSQLReader(string.Format("SELECT COUNT(*) FROM tx_kvstore WHERE key = '{0}'", key));
            while(reader.Read())
            {
                if(reader.GetInt32(0) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get Value from KV Store
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKVStoreValue(string key)
        {
            string val;

            try
            {
                val = GetSingleValue(string.Format("SELECT value FROM tx_kvstore WHERE key = '{0}'", key));
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message);
                _log.Debug(ex.ToString());
                val = null;
            }

            return val;
        }

        /// <summary>
        /// Get Single value returned by query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string GetSingleValue(string query)
        {
            SqliteDataReader reader;

            try
            {
                reader = GetSQLReader(query);
                while(reader.Read())
                {
                    return reader.GetString(0);
                }
                return null;
            }
            catch
            {
                return null; 
            }
        }

        /// <summary>
        /// Get SQL Reader for query
        /// </summary>
        /// <param name="s_query"></param>
        /// <returns></returns>
        public SqliteDataReader GetSQLReader(string s_query)
        {
            SqliteCommand cmd;

            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = s_query;
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                _log.Error("Query error: " + s_query);
                _log.Error(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Excecute non query
        /// </summary>
        /// <param name="s_query"></param>
        /// <param name="b_log_error"></param>
        public void DoNonQuery(string s_query, bool b_log_error = true)
        {
            SqliteCommand cmd;

            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = s_query;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (b_log_error)
                {
                    _log.Error("Query error: " + s_query);
                    _log.Error(ex.ToString());
                }
            }

        }

        /// <summary>
        /// Excecute non query
        /// </summary>
        /// <param name="s_query"></param>
        /// <param name="b_log_error"></param>
        public void DoNonQueryNoErrorHandling(string s_query, bool b_log_error = true)
        {
            SqliteCommand cmd;
            cmd = _dbConnection.CreateCommand();
            cmd.CommandText = s_query;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Close DB
        /// </summary>
        public void Close()
        {
            try
            {
                _dbConnection.Close();
            }
            catch (Exception ex)
            {
                _log.Error("Cannot close database connection: " + ex.Message);
                _log.Debug(ex.ToString());
            }
        }

    }
}
