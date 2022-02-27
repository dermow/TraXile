using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class TrX_TrackedLabrun : TrX_TrackedActivity
    {
        // List of found enchants
        private List<TrX_LabEnchant> _enchants;
        public List<TrX_LabEnchant> Enchants => _enchants;

        private List<TrX_LabEnchant> _enchantsTaken;
        public List<TrX_LabEnchant> EnchantsTaken => _enchantsTaken;

        // Count of Izaro-Fights
        private int _trialCount;
        public int TrialCount
        {
            get { return _trialCount; }
            set { _trialCount = value; }
        }

        public TrX_TrackedLabrun()
        {
            _enchants = new List<TrX_LabEnchant>();
            _enchantsTaken = new List<TrX_LabEnchant>();
        }
    }
}
