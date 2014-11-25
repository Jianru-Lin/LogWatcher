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
using System.Text.RegularExpressions;

namespace LogWatcher
{
    public partial class MainForm : Form
    {
        Dictionary<string, LogForm> map = new Dictionary<string,LogForm>();

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

        private void regexMatch(string text, out string module, out string content)
        {
            module = "";
            content = "";
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Regex pattern = new Regex(@"\[([^\]]+)\]\s([^$]*)");
            MatchCollection matches = pattern.Matches(text);
            if (matches.Count == 0)
            {
                content = text;
            }
            else
            {
                module = matches[0].Groups[1].Value;
                content = matches[0].Groups[2].Value;
            }
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
            
            // 如果文本符合模块输出语法，则要输出到指定的模块窗口中
            // 否则输出到全局窗口

            string module, content;
            regexMatch(text, out module, out content);
            LogForm target = null;
            if (!map.TryGetValue(module, out target))
            {
                target = new LogForm();
                target.Text = string.IsNullOrEmpty(module) ? "Global" : module;
                map[module] = target;
                target.Show();
            }

            target.Append(text);
        }

    }
}
