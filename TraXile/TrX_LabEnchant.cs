using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TrX_LabEnchant
    {
        // Enchant Text
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        // Enchant ID
        private int _id;
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        // Enchant Info
        private TrX_EnchantInfo _enchantInfo;
        public TrX_EnchantInfo EnchantInfo
        {
            get { return _enchantInfo; }
            set { _enchantInfo = value; }
        }

        public TrX_LabEnchant()
        {
            _enchantInfo = new TrX_EnchantInfo(0);
        }
    }
}
