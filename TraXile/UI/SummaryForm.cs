using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TraXile.UI
{
    public partial class SummaryForm : MaterialForm
    {
        public MaterialLabel CountLabel => materialLabel4;
        public MaterialLabel DurationLabel => materialLabel5;
        public MaterialLabel AverageLabel => materialLabel6;
        public MaterialListView ListViewTypes => materialListView1;
        public MaterialCard TagCard => materialCard2;

        public SummaryForm()
        {
            InitializeComponent();
        }

        private void materialLabel9_Click(object sender, EventArgs e)
        {

        }
    }
}
