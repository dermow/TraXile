using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraXile
{
    public class ActivityTag
    {
        private string sID;
        private string sDescription;
        private Color backColor;
        private Color foreColor;

        public ActivityTag(string s_id)
        {
            sID = s_id;
            backColor = Color.White;
            foreColor = Color.Red;
        }

        public string ID
        {
            get { return sID; }
        }

        public string Description
        {
            get { return sDescription; }
            set { sDescription = value; }
        }
     
        public Color ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }

        public Color BackColor
        {
            get { return backColor; }
            set { backColor = value; }
        }


    }
}
