using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using static System.Windows.Forms.ListViewItem;
using System;

namespace TraXile
{
    class TrX_ListViewManager
    {
        private readonly ListView _listView;
        private bool _isFiltered;
        private readonly List<ListViewItem> _masterList;
        private readonly List<ListViewItem> _filteredList;
        private readonly Dictionary<string, ListViewItem> _itemMap;

        public List<ListViewItem> MasterList => _masterList;

        public TrX_ListViewManager(ListView lv_to_manage)
        {
            _listView = lv_to_manage;
            _masterList = new List<ListViewItem>();
            _filteredList = new List<ListViewItem>();
            _itemMap = new Dictionary<string, ListViewItem>();

            foreach (ListViewItem lvi in _listView.Items)
            {
                _masterList.Add(lvi);
                _filteredList.Add(lvi);
                _itemMap.Add(lvi.Name, lvi);
            }
        }

        public void FilterByRange(int i_min, int i_max)
        {
            _listView.SuspendLayout();

            _listView.BeginUpdate();
            int iMax = _masterList.Count > i_max ? i_max : _masterList.Count;
            _filteredList.Clear();

            foreach (ListViewItem lvi in _masterList.GetRange(i_min, iMax))
            {
                if (!_filteredList.Contains(lvi))
                {
                    _filteredList.Add(lvi);
                }

            }

            _listView.Items.Clear();

            foreach (ListViewItem lvi in _filteredList)
            {
                _listView.Items.Add(lvi);
            }
            _isFiltered = true;
            _listView.EndUpdate();

            _listView.ResumeLayout();
        }

        public void ApplyFullTextFilter(string s_filter)
        {
            List<string> names = new List<string>();

            try
            {
                foreach (ListViewItem lvi in _masterList)
                {
                    if (Regex.IsMatch(lvi.Text, s_filter))
                    {
                        if (!names.Contains(lvi.Name))
                        {
                            names.Add(lvi.Name);
                        }
                    }
                    else
                    {
                        foreach (ListViewSubItem si in lvi.SubItems)
                        {
                            if (Regex.IsMatch(si.Text, s_filter))
                            {
                                if (!names.Contains(lvi.Name))
                                {
                                    names.Add(lvi.Name);
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                
            }
           
            FilterByNameList(names);
        }

        public void FilterByNameList(List<string> item_names)
        {
            _listView.SuspendLayout();

            _filteredList.Clear();
            foreach (string s in item_names)
            {
                if (_itemMap.ContainsKey(s))
                {
                    _filteredList.Add(_itemMap[s]);
                }
            }

            _listView.Items.Clear();

            foreach (ListViewItem lvi in _filteredList)
            {
                _listView.Items.Add(lvi);
            }
            _isFiltered = true;

            _listView.ResumeLayout();
        }

        public void Clear()
        {
            _listView.Items.Clear();
        }

        public void Reset()
        {
            _listView.Items.Clear();

            foreach (ListViewItem lvi in _masterList)
            {
                _listView.Items.Add(lvi);
            }
            _isFiltered = false;
        }

        public void ClearLvItems()
        {
            _masterList.Clear();
            _itemMap.Clear();
            _listView.Items.Clear();
        }

        public void AddLvItem(ListViewItem lvi, string s_name, bool b_display = true)
        {
            if (!_itemMap.ContainsKey(s_name))
            {
                lvi.Name = s_name;
                _masterList.Add(lvi);
                _itemMap.Add(s_name, lvi);
                if (!_isFiltered)
                {
                    if (b_display)
                        _listView.Items.Add(lvi);
                }
            }
        }

        public void InsertLvItem(ListViewItem lvi, string s_name, int i_pos, bool b_display = true)
        {
            if (!_itemMap.ContainsKey(s_name))
            {
                lvi.Name = s_name;
                _masterList.Insert(i_pos, lvi);
                _itemMap.Add(s_name, lvi);
                if (!_isFiltered)
                {
                    if (b_display)
                        _listView.Items.Insert(i_pos, lvi);
                }
            }
        }

        public void RemoveItemByName(string s_name)
        {
            if (_itemMap.ContainsKey(s_name))
            {
                ListViewItem toDelete = _itemMap[s_name];
                if (_masterList.Contains(toDelete))
                {
                    _masterList.Remove(toDelete);
                }
                if (_filteredList.Contains(toDelete))
                {
                    _filteredList.Remove(toDelete);
                }
                if (_listView.Items.Contains(toDelete))
                {
                    _listView.Items.Remove(toDelete);
                }
            }
        }

        public ListView listView
        {
            get { return _listView; }
        }

        public ListViewItem GetLvItem(string s_name)
        {
            return _itemMap[s_name];
        }

        public ListView.ColumnHeaderCollection Columns
        {
            get { return _listView.Columns; }
        }

        public int CurrentItemCount
        {
            get { return _listView.Items.Count; }
        }
    }
}
