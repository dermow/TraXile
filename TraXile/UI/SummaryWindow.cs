using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraXile.UI
{
    public partial class SummaryWindow : Form
    {
        public Label CountLabel => labelCount;
        public Label DurationLabel => labelDuaration;
        public Label AverageLabel => labelAvgDuration;

        public SummaryWindow()
        {
            InitializeComponent();
        }
    }
}
