using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Pipes;
using System.IO;

namespace LogWatcher
{
    public partial class LogForm : Form
    {
        MapItem currentMapItem = null;
        Dictionary<string, MapItem> map = new Dictionary<string, MapItem>();

        public LogForm()
        {
            InitializeComponent();
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            MainForm form = new MainForm();
            form.ShowDialog();
            if (form.UserWannaClose)
            {
                Application.Exit();
            }
            else
            {
                backgroundWorker.RunWorkerAsync(form.PipeName);
            }
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

        public void Append(string text)
        {
            const string globalName = "Global";

            // 如果文本符合模块输出语法，则要输出到指定的模块窗口中
            // 否则输出到全局窗口

            string module, content;
            regexMatch(text, out module, out content);

            module = module == "" ? globalName : module;

            MapItem mapItem = null;
            if (!map.TryGetValue(module, out mapItem))
            {
                mapItem = new MapItem(module);
                map[module] = mapItem;
                treeView.Nodes.Add(mapItem.GetTreeNode());
            }

            string line = content;
            mapItem.AppendLine(line);
            if (mapItem == currentMapItem)
            {
                richTextBox.AppendText(line);
            }
        }

        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentMapItem = MapItem.Of(e.Node);
            richTextBox.Clear();
            if (currentMapItem != null)
            {
                richTextBox.AppendText(currentMapItem.GetAllLines());
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
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState as State != null)
            {
                toolStripStatusLabel.Text = (e.UserState as State).ToString();
                return;
            }

            string text = e.UserState as string;
            Append(text);
        }


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

        class MapItem
        {
            public MapItem(string text)
            {
                lines = new List<string>();
                node = new TreeNode(text);
                node.Tag = this;
            }

            public void AppendLine(string text)
            {
                lines.Add(text);
            }

            public string GetAllLines()
            {
                return String.Join<string>("", lines);
            }

            public TreeNode GetTreeNode()
            {
                return node;
            }

            static public MapItem Of(TreeNode node)
            {
                if (node == null) return null;
                return node.Tag as MapItem;
            }

            List<string> lines;
            TreeNode node;
        }
    }
}
