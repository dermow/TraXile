using System.Drawing;

namespace TraXile
{
    public class TrX_ActivityTag
    {
        // Unique Tag ID
        private readonly string _tagID;
        public string ID => _tagID;

        // Default tag?
        private readonly bool _isDefault;
        public bool IsDefault => _isDefault;

        // Name to Display
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        // Text color
        private Color _foreColor;
        public Color ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        // Background color
        private Color _backColor;
        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        // Show in listview as column?
        private bool _showInList;
        public bool ShowInListView
        {
            get { return _showInList; }
            set { _showInList = value; }
        }

        private bool _soundEnabled;
        public bool SoundEnabled
        {
            get { return _soundEnabled; }
            set { _soundEnabled = value; }
        }

        private string _soundID;
        public string SoundID
        {
            get { return _soundID; }
            set { _soundID = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="s_id"></param>
        /// <param name="b_is_default"></param>
        public TrX_ActivityTag(string s_id, bool b_is_default = true)
        {
            _tagID = s_id;
            _backColor = Color.White;
            _foreColor = Color.Black;
            _isDefault = b_is_default;
            _displayName = _tagID;
            _showInList = false;
            _soundEnabled = false;
            _soundID = string.Empty;
        }
    }
}
