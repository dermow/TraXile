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

namespace TraXile.UI
{
    public partial class Form1 : MaterialForm
    {
        MaterialSkinManager msm;

        public Form1()
        {
            InitializeComponent();
            msm = MaterialSkinManager.Instance;
            msm.Theme = MaterialSkinManager.Themes.DARK;
        }

        private void materialCard1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
