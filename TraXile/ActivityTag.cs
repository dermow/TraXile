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
        private string sDisplay;
        private bool bIsDefault;
        private Color backColor;
        private Color foreColor;

        public ActivityTag(string s_id, bool b_is_default = true)
        {
            sID = s_id;
            backColor = Color.White;
            foreColor = Color.Black;
            bIsDefault = b_is_default;
            sDisplay = sID;
        }

        public string ID
        {
            get { return sID; }
        }

        public string DisplayName
        {
            get { return sDisplay; }
            set { sDisplay = value; }
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

        public bool IsDefault
        {
            get { return bIsDefault; }
        }


    }
}
