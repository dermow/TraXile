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
        private string _tagID;
        private string _DisplayName;
        private bool _isDefault;
        private Color _backColor;
        private Color _foreColor;

        public ActivityTag(string s_id, bool b_is_default = true)
        {
            _tagID = s_id;
            _backColor = Color.White;
            _foreColor = Color.Black;
            _isDefault = b_is_default;
            _DisplayName = _tagID;
        }

        public string ID
        {
            get { return _tagID; }
        }

        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }
     
        public Color ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
        }


    }
}
