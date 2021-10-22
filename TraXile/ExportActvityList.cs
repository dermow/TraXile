using System;
using System.Windows.Forms;

namespace TraXile
{
    public partial class ExportActvityList : Form
    {
        private readonly Main _mainWindow;

        public ExportActvityList(Main main)
        {
            InitializeComponent();
            this._mainWindow = main;
            this.comboBox1.SelectedIndex = 0;
            this.Text = "Export Activity Log";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedItem.ToString())
            {
                case "csv":
                    SaveFileDialog sfd = new SaveFileDialog
                    {
                        Filter = "CSV-Files|*.csv",
                        FileName = "TraXile_Activities_" + DateTime.Now.ToString("yyyy-MM-dd-H-m-s") + ".csv"
                    };
                    DialogResult rs = sfd.ShowDialog();
                    if (rs == DialogResult.OK && sfd.FileName != null)
                    {
                        try
                        {
                            _mainWindow.WriteActivitiesToCSV(sfd.FileName);
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
