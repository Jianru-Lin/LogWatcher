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

namespace LogWatcher
{
    public partial class LogForm : Form
    {
        Dictionary<string, TreeNode> map = new Dictionary<string, TreeNode>();

        public LogForm()
        {
            InitializeComponent();
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
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

            TreeNode node = null;
            if (!map.TryGetValue(module, out node))
            {
                node = new TreeNode(module);
                map[module] = node;
                treeView.Nodes.Add(node);
            }
            
        }

        private void LogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MessageBox.Show("after select");
        }
    }
}
