using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Data.Sqlite;

namespace TraXile
{
    class TrX_DBManager
    {
        private SqliteConnection _dbConnection;
        private readonly string _dbPath;
        private readonly ILog _log;

        public TrX_DBManager(string s_file, ref ILog log)
        {
            _dbPath = s_file;
            _log = log;
            Init();
            Patch();
        }

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
            // InitDefaultTags();

            Patch();
        }

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
        }

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

        public void DoNonQuery(string s_query)
        {
            SqliteCommand cmd;

            try
            {
                cmd = _dbConnection.CreateCommand();
                cmd.CommandText = s_query;
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                _log.Error("Query error: " + s_query);
                _log.Error(ex.ToString());
            }
           
        }

    }
}
