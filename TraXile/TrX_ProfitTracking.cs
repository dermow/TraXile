using System;
using System.Data;
using System.IO;

namespace TraXile
{
    class TrX_ProfitTracking
    {
        private DataTable _data;
        public DataTable Data => _data;
        private string _xmlSavePath;
        public string DataFilePath
        {
            get { return _xmlSavePath; }
            set { _xmlSavePath = value; }
        }

        private int _bases;
        public int BaseCount => _bases;

        private int _basesSold;
        public int BasesSold => _basesSold;

        private double _income;
        public double Income => _income;

        private double _baseCosts;
        public double BaseCosts => _baseCosts;

        public double Profit => (_income - _baseCosts);

        public TrX_ProfitTracking(string dataPath)
        {
            _xmlSavePath = dataPath;
            InitDataColumns();
        }

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            _data.WriteXml(_xmlSavePath);
        }

        /// <summary>
        /// Initialize Table structure
        /// </summary>
        private void InitDataColumns()
        {
            _data = new DataTable("LabWare");
            _data.Columns.Add("Time", System.Type.GetType("System.DateTime"));
            _data.Columns.Add("Enchant", System.Type.GetType("System.String"));
            _data.Columns.Add("Base", System.Type.GetType("System.String"));
            _data.Columns.Add("Base Cost (Divines)", System.Type.GetType("System.Double"));
            _data.Columns.Add("Sold For (Divines)", System.Type.GetType("System.Double"));
            _data.Columns.Add("Profit (Divines)", System.Type.GetType("System.Double"));
            _data.Columns.Add("State", System.Type.GetType("System.String"));
            _data.Columns.Add("Note", System.Type.GetType("System.String"));

            if (File.Exists(_xmlSavePath))
            {
                // Patch Exalts -> Divines
                StreamReader r = new StreamReader(_xmlSavePath);
                string xmlstr = r.ReadToEnd();

                if (xmlstr.Contains("Exalts"))
                {
                    xmlstr = xmlstr.Replace("Exalts", "Divines");
                    r.Close();
                    StreamWriter w = new StreamWriter(_xmlSavePath);
                    w.Write(xmlstr);
                    w.Close();
                }

                _data.ReadXml(_xmlSavePath);
                Calculate();
            }
        }

        /// <summary>
        /// Calculate values per row
        /// </summary>
        public void Calculate()
        {
            double baseCost = 0;
            double soldFor = 0;
            double profit = 0;

            int basesSold = 0;
            int bases = 0;

            double basesCostTotal = 0;
            double incomeTotal = 0;

            for (int i = 0; i < _data.Rows.Count; i++)
            {
                soldFor = Convert.ToDouble(_data.Rows[i]["Sold for (Divines)"].ToString());
                baseCost = Convert.ToDouble(_data.Rows[i]["Base Cost (Divines)"].ToString());
                basesCostTotal += baseCost;

                if (soldFor > 0)
                {
                    incomeTotal += soldFor;
                    profit = soldFor - baseCost;
                    _data.Rows[i]["Profit (Divines)"] = profit.ToString();
                    _data.Rows[i]["State"] = "sold";
                    basesSold++;
                }
                else
                {
                    _data.Rows[i]["State"] = "open";
                }

                bases++;
            }

            _basesSold = basesSold;
            _bases = bases;
            _income = incomeTotal;
            _baseCosts = basesCostTotal;
        }


    }
}
