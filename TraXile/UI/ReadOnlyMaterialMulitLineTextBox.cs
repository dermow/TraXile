using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MaterialSkin.Controls;
using System.Windows.Forms;

namespace TraXile.UI
{
    public class ReadOnlyMaterialMultilineTextBox : MaterialMultiLineTextBox
    {
        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        public ReadOnlyMaterialMultilineTextBox()
        {
            ReadOnly = true;
            TabStop = false;
            Cursor = Cursors.Default;
            BorderStyle = BorderStyle.None;

            // Optional: Make it blend in more like a label
            BackColor = SystemColors.Control;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            HideCaret(this.Handle);
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            HideCaret(this.Handle);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            HideCaret(this.Handle);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            HideCaret(this.Handle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            HideCaret(this.Handle);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Ensure caret stays hidden even after control is redrawn
            this.BeginInvoke((MethodInvoker)delegate
            {
                HideCaret(this.Handle);
            });
        }
    }
}
