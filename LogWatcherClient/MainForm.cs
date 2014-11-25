using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;

namespace LogWatcherClient
{
    public partial class MainForm : Form
    {
        NamedPipeClientStream client;

        public MainForm()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                MessageBox.Show("you have a pipe connection already.");
                return;
            }

            string serverName = serverNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("provide server name please.");
                return;
            }

            string pipeName = pipeNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(pipeName)) 
            {
                MessageBox.Show("provide pipe name please.");
                return;
            }

            try
            {
                client = new NamedPipeClientStream(serverName, pipeName);
                client.Connect(100);
                MessageBox.Show("connect successfully.");
                connectButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                client = null;
                connectButton.Enabled = true;
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("please connect first.");
                return;
            }

            string text = richTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("you can not send empty message.");
                return;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(client, Encoding.Default, 4 * 1024, true))
                {
                    writer.Write(text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                client = null;
                connectButton.Enabled = true;
            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
