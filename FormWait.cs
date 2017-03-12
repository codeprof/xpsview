using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace xpsview
{
    public partial class FormWait : Form
    {
        public FormWait()
        {
            InitializeComponent();
        }

        private void FormWait_Load(object sender, EventArgs e)
        {
            this.labelWait.Text = Lang.translate("Please wait ...", "Bitte warten ...", "s'il vous plaît patienter ...");
        }
    }
}
