using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TrX_EnchantNote
    {
        // ID for the enchant
        private int _enchantID;
        public int EnchantID => _enchantID;

        // note
        private string _note;
        public string Note => _note;

        // Lab ts
        private int _labTimeStamp;
        public int LabTimeStamp => _labTimeStamp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        public TrX_EnchantNote(int id, string note, int labtimestamp)
        {
            _enchantID = id;
            _note = note;
            _labTimeStamp = labtimestamp;
        }
    }
}
