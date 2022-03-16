using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TrX_EnchantInfo
    {
        // Enchant ID
        private int _enchantID;
        public int EnchantID
        {
            get { return _enchantID; }
            set { _enchantID = value; }
        }

        // Notes
        private List<TrX_EnchantNote> _enchantsNotes;
        public List<TrX_EnchantNote> EnchantNotes => _enchantsNotes;

        // How often this enchant was found
        private int _found;
        public int Found
        {
            get { return _found; }
            set { _found = value; }
        }

        // Taken
        private int _taken;
        public int Taken
        {
            get { return _taken; }
            set { _taken = value; }
        }

        // History
        private List<string> _history;
        public List<string> History => _history;

        // Last found
        private DateTime _lastFound;
        public DateTime LastFound
        {
            get { return _lastFound; }
            set { _lastFound = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public TrX_EnchantInfo(int id)
        {
            _enchantID = id;
            _enchantsNotes = new List<TrX_EnchantNote>();
            _history = new List<string>();
            _found = 0;
            _taken = 0;
        }
    }
}
