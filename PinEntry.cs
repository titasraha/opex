using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPGPExplorer
{
    public partial class PinEntry : Form
    {
        public string Password { get; set; }
        public PinEntry()
        {
            InitializeComponent();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            Password = txtPassword.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
