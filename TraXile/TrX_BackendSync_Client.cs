using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    internal class TrX_BackendSync_Client
    {
        private string backendAPIAdress = "";
        public string BackendAPIAdress
        {
            get { return backendAPIAdress; }
            set { backendAPIAdress = value; }
        }

        public TrX_BackendSync_Client(string addr)
        {
            backendAPIAdress = addr;
        }
    }
}
