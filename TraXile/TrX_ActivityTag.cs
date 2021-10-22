using System.Drawing;

namespace TraXile
{
    public class TrX_ActivityTag
    {
        private readonly string _tagID;
        private string _DisplayName;
        private readonly bool _isDefault;
        private bool _showInList;
        private Color _backColor;
        private Color _foreColor;

        public TrX_ActivityTag(string s_id, bool b_is_default = true)
        {
            _tagID = s_id;
            _backColor = Color.White;
            _foreColor = Color.Black;
            _isDefault = b_is_default;
            _DisplayName = _tagID;
            _showInList = false;
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

        public bool ShowInListView
        {
            get { return _showInList; }
            set { _showInList = value; }
        }


    }
}
