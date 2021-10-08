using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace TraXile
{
    class ListViewManager
    {
        private ListView lv;
        private bool bIsFiltered;
        private List<ListViewItem> masterList, filteredList;
        private Dictionary<string, ListViewItem> itemMap;

        public ListViewManager(ListView lv_to_manage)
        {
            lv = lv_to_manage;
            masterList = new List<ListViewItem>();
            filteredList = new List<ListViewItem>();
            itemMap = new Dictionary<string, ListViewItem>();

            foreach(ListViewItem lvi in lv.Items)
            {
                masterList.Add(lvi);
                filteredList.Add(lvi);
                itemMap.Add(lvi.Name, lvi);
            }
        }

        public void ApplyFullTextFilter(string s_filter)
        {
            List<string> names = new List<string>();

            foreach(ListViewItem lvi in masterList)
            {
                if(lvi.Text.ToLower().Contains(s_filter.ToLower()))
                {
                    if(!names.Contains(lvi.Name))
                    {
                        names.Add(lvi.Name);
                    }
                }
                else
                {
                    foreach(ListViewSubItem si in lvi.SubItems)
                    {
                        if(si.Text.ToLower().Contains(s_filter.ToLower()))
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
            FilterByNameList(names);
        }

        public void FilterByNameList(List<string> item_names)
        {
            filteredList.Clear();
            foreach(string s in item_names)
            {
                if(itemMap.ContainsKey(s))
                {
                    filteredList.Add(itemMap[s]);
                }
            }

            lv.Items.Clear();

            foreach(ListViewItem lvi in filteredList)
            {
                lv.Items.Add(lvi);
            }
        }

        public void Reset()
        {
            lv.Items.Clear();

            foreach (ListViewItem lvi in masterList)
            {
                lv.Items.Add(lvi);
            }
        }

        public void ClearLvItems()
        {
            masterList.Clear();
            itemMap.Clear();
            lv.Items.Clear();
        }

        public void AddLvItem(ListViewItem lvi, string s_name)
        {
            if(!itemMap.ContainsKey(s_name))
            {
                lvi.Name = s_name;
                masterList.Add(lvi);
                itemMap.Add(s_name, lvi);
                if (!bIsFiltered)
                {
                    lv.Items.Add(lvi);
                }
            }
        }

        public void InsertLvItem(ListViewItem lvi, string s_name, int i_pos)
        {
            if(!itemMap.ContainsKey(s_name))
            {
                lvi.Name = s_name;
                masterList.Insert(i_pos, lvi);
                itemMap.Add(s_name, lvi);
                if (!bIsFiltered)
                {
                    lv.Items.Insert(i_pos, lvi);
                }
            }
        }

        public ListViewItem GetLvItem(string s_name)
        {
            return itemMap[s_name];
        }

        public ListView.ColumnHeaderCollection Columns
        {
            get { return lv.Columns; }
        }
    }
}
