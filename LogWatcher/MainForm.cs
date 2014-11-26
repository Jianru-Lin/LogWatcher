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

namespace LogWatcher
{
    public partial class MainForm : Form
    {
        LogForm logForm = new LogForm();

        class State
        {
            string text;

            public State(string text)
            {
                this.text = text;
            }

            public override string ToString()
            {
                return text != null ? text : "";
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                MessageBox.Show("running already");
                return;
            }

            string name = textBox.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("please input pipe name");
                return;
            }

            backgroundWorker.RunWorkerAsync(name);
            startButton.Enabled = false;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker self = sender as BackgroundWorker;
            string name = e.Argument as string;
            NamedPipeServerStream server;

            try
            {
                while (true)
                {
                    using (server = new NamedPipeServerStream(name))
                    {
                        self.ReportProgress(0, new State("waiting for connection."));
                        server.WaitForConnection();
                        self.ReportProgress(0, new State("new connection established."));
                        using (StreamReader reader = new StreamReader(server, Encoding.Default, false, 4 * 1024, true))
                        {
                            while (true)
                            {
                                string text = reader.ReadLine();
                                if (text != null)
                                {
                                    self.ReportProgress(0, text + "\n");
                                }
                                else
                                {
                                    // reach the end
                                    break;
                                }
                            }
                        }
                        self.ReportProgress(0, new State("disconnected."));

                    }
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result as Exception != null)
            {
                MessageBox.Show((e.Result as Exception).Message);
            }

            startButton.Enabled = true;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState as State != null)
            {
                this.richTextBox.AppendText((e.UserState as State).ToString() + "\n");
                return;
            }

            string text = e.UserState as string;
            logForm.Append(text);
            logForm.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

    }
}
