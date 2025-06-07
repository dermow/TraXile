using System;
using MaterialSkin.Controls;

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
