using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace TraXile
{
    public partial class ExportActvityList : Form
    {
        private readonly Main _mainWindow;

        public ExportActvityList(Main main, string format)
        {
            InitializeComponent();
            this._mainWindow = main;
            this.comboBox1.SelectedItem = format;
            this.comboBox2.SelectedItem = "Filtered entries";
            this.Text = "Export Activity Log";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd;
            DialogResult rs;
            switch (comboBox1.SelectedItem.ToString())
            {
                case "csv":
                    sfd = new SaveFileDialog
                    {
                        Filter = "CSV-Files|*.csv",
                        FileName = $"TraXile_Activities_{DateTime.Now.ToString("yyyy-MM-dd-H-m-s")}.csv"
                    };
                    rs = sfd.ShowDialog();
                    if (rs == DialogResult.OK && sfd.FileName != null)
                    {
                        try
                        {
                            _mainWindow.WriteActivitiesToCSV(sfd.FileName, comboBox2.SelectedItem.ToString());
                            MessageBox.Show("Export successful!");
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                    break;

                case "json":
                    sfd = new SaveFileDialog
                    {
                        Filter = "JSON-Files|*.json",
                        FileName = $"TraXile_Activities_{DateTime.Now.ToString("yyyy-MM-dd-H-m-s")}.json"
                    };
                    rs = sfd.ShowDialog();
                    if (rs == DialogResult.OK && sfd.FileName != null)
                    {
                        try
                        {
                            _mainWindow.WriteActivitiesToJSON(sfd.FileName, comboBox2.SelectedItem.ToString());
                            MessageBox.Show("Export successful!");
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;
            }

        }
    }
}