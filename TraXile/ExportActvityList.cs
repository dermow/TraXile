using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile
{
    public partial class ExportActvityList : Form
    {
        MainW main;

        public ExportActvityList(MainW main)
        {
            InitializeComponent();
            this.main = main;
            this.comboBox1.SelectedIndex = 0;
            this.Text = "Export Activity Log";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedItem.ToString())
            {
                case "csv":
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "CSV-Files|*.csv";
                    sfd.FileName = "TraXile_Activities_" + DateTime.Now.ToString("yyyy-MM-dd-H-m-s") + ".csv";
                    DialogResult rs = sfd.ShowDialog();
                    if (rs == DialogResult.OK && sfd.FileName != null)
                    {
                        try
                        {
                            main.WriteActivitiesToCSV(sfd.FileName);
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
