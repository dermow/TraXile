using MaterialSkin.Controls;
using MaterialSkin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class SearchableMaterialComboBox : UserControl
{
    private MaterialTextBox2 searchBox;
    private ListBox dropdownList;
    private List<string> items = new List<string>();

    public event EventHandler SelectedValueChanged;

    public SearchableMaterialComboBox()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Height = 48;
        this.Width = 250;

        searchBox = new MaterialTextBox2
        {
            Dock = DockStyle.Top,
            Hint = "Search...",
            //BorderStyle = BorderStyle.None,
            Font = new Font("Roboto", 10),
            Height = 48
        };
        searchBox.TextChanged += SearchBox_TextChanged;
        searchBox.Enter += (s, e) => ShowDropdown();

        dropdownList = new ListBox
        {
            Visible = false,
            Height = 100,
            Font = new Font("Roboto", 10),
            IntegralHeight = false
        };
        dropdownList.Click += DropdownList_Click;

        this.Controls.Add(dropdownList);
        this.Controls.Add(searchBox);
    }

    public void SetItems(IEnumerable<string> sourceItems)
    {
        items = sourceItems.ToList();
        UpdateList(items);
    }

    public string SelectedItem => dropdownList.SelectedItem?.ToString();

    private void SearchBox_TextChanged(object sender, EventArgs e)
    {
        var filtered = items
            .Where(i => i.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        UpdateList(filtered);
        ShowDropdown();
    }

    private void UpdateList(List<string> source)
    {
        dropdownList.BeginUpdate();
        dropdownList.Items.Clear();
        foreach (var item in source)
            dropdownList.Items.Add(item);
        dropdownList.EndUpdate();
    }

    private void ShowDropdown()
    {
        if (dropdownList.Items.Count > 0)
        {
            dropdownList.Visible = true;
            dropdownList.BringToFront();
            dropdownList.Width = this.Width;
            dropdownList.Location = new Point(0, searchBox.Bottom);
        }
        else
        {
            dropdownList.Visible = false;
        }
    }

    private void DropdownList_Click(object sender, EventArgs e)
    {
        if (dropdownList.SelectedItem != null)
        {
            searchBox.Text = dropdownList.SelectedItem.ToString();
            dropdownList.Visible = false;
            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        dropdownList.Visible = false;
    }
}