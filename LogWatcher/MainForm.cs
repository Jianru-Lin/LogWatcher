using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogWatcher
{
    public partial class MainForm : Form
    {
        private bool userWannaClose = true;

        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            string name = textBox.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("please input pipe name");
                return;
            }

            userWannaClose = false;
            this.Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        public bool UserWannaClose
        {
            get
            {
                return userWannaClose;
            }
        }

        public string PipeName
        {
            get
            {
                return textBox.Text;
            }
        }
    }
}
