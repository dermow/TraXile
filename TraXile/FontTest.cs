using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace TraXile
{
    public partial class FontTest : Form
    {
        public FontTest()
        {
            InitializeComponent();
            PrivateFontCollection fonts = new PrivateFontCollection();
            
            foreach(string f in Directory.GetFiles("fonts"))
            {
                fonts.AddFontFile(f);
            }

            string s = "";

            FontFamily fam = new FontFamily("Roboto Light", fonts);
            
            Font font = new Font(fam, 20);

            label1.Text = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut l";
            label1.Font = font;

        }
    }
}
