using Lang.language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Lang.Language
{
    public partial class CodeGUI : Form
    {
        private int idx;
        private int curLine;
        private ArrayList lines;
        internal string lastRequestedString;
        internal bool TokenRequsted, LineRequested;
        Thread lastCallingThread;
        Interpreter handlingInterpreter;

        public CodeGUI(Thread thread, Interpreter _handler)
        {
            InitializeComponent();
            idx = curLine = 0;
            TokenRequsted = LineRequested = false;
            lines = new ArrayList();
            lastCallingThread = thread;
            handlingInterpreter = _handler;
        }

        internal void getNextToken()
        {
            if (lines.Count <= curLine)
            {
                lastCallingThread.Suspend();
                TokenRequsted = true;
                return;
            }
            char cur;
            string ret = "";
            while ((cur = ((string)lines[curLine])[idx]) != ' ')
            {
                if(cur != '\n')
                    ret += cur;
                idx++;
                if (((string)lines[curLine]).Length <= idx)
                {
                    idx = 0;
                    curLine++;
                    break;
                }
            }
            lastRequestedString = ret;
            if (lastCallingThread.ThreadState == ThreadState.Suspended)
                lastCallingThread.Resume();
        }

        internal void getLine()
        {
            if (lines.Count <= curLine)
            {
                lastCallingThread.Suspend();
                LineRequested = true;
                return;
            }
            char cur;
            string ret = "";
            while ((cur = ((string)lines[curLine])[idx]) != '\n')
            {
                ret += cur;
                idx++;
            }
            curLine++;
            idx = 0;
            lastRequestedString = ret;
            if (lastCallingThread.ThreadState == ThreadState.Suspended)
            {
                lastCallingThread.Resume();
            }
        }

        internal void printLine(string _line)
        {
            int selPos = consoleRTB.SelectionStart;
            int lastbn = consoleRTB.Text.LastIndexOf('\n');
            if(lastbn < 0)
                lastbn = 0;
            consoleRTB.SelectionStart = lastbn;
            consoleRTB.SelectionLength = 0;
            consoleRTB.SelectedText = "\n" + _line;
        }

        private void consoleRTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int LineStart = consoleRTB.Text.LastIndexOf('\n');
                if (LineStart < 0)
                    LineStart = 0;
                lines.Add(consoleRTB.Text.Substring(LineStart) + "\n");
                if (TokenRequsted)
                {
                    getNextToken();
                }
                else if(LineRequested)
                {
                    getLine();
                }
                else if (lastCallingThread.ThreadState == ThreadState.Suspended)
                {
                    lastCallingThread.Resume();
                }
            }
        }

        private void ViewVariables(TreeNode node, LangObject obj)
        {
            if (obj.objectType == ObjectType.NUMBER)
            {
                node.Text = ((LangNumber)obj).numberValue + "";
            }
            else if (obj.objectType == ObjectType.STRING)
            {
                node.Text = ((LangString)obj).stringValue;
            }
            else if (obj.objectType == ObjectType.MAP)
            {
                LangMap mp = ((LangMap)obj);
                node.Text = "map";
                SortedDictionary<Object, LangObject> sd = new SortedDictionary<object, LangObject>();
                foreach (DictionaryEntry dic in mp.arrayValue)
                {
                    sd.Add(dic.Key, (LangObject)dic.Value);
                }
                foreach (KeyValuePair<Object, LangObject> dic in sd)
                {
                    TreeNode child = new TreeNode();
                    ViewVariables(child, dic.Value);
                    child.Text = "[" + dic.Key.ToString() + "]" + " : " + child.Text;
                    node.Nodes.Add(child);
                }
            }
            else if (obj.objectType == ObjectType.CLASS)
            {
                LangClass objcls = (LangClass)obj;
                node.Text = objcls.name;
                foreach (DictionaryEntry it in objcls.vars)
                {
                    LangObject obj2 = ((LangObject)it.Value);
                    TreeNode child = new TreeNode((string)it.Key);
                    TreeNode child2 = new TreeNode();
                    ViewVariables(child2, obj2);
                    child.Nodes.Add(child2);
                    node.Nodes.Add(child);
                }
            }
        }

        internal void Debug(Hashtable vars)
        {
            lastCallingThread.Suspend();
            foreach (DictionaryEntry dic in vars)
            {
                TreeNode root = new TreeNode((string)dic.Key);
                TreeNode etsarraf = new TreeNode();
                ViewVariables(etsarraf, ((LangObject)dic.Value));
                root.Nodes.Add(etsarraf);
                DebugTV.Nodes.Add(root);
            }
            ResumeButton.Enabled = true;
        }

        private void ResumeButton_Click(object sender, EventArgs e)
        {
            lastCallingThread.Resume();
            DebugTV.Nodes.Clear();
        }

        internal void HandleExceptions(Exception e, Token ErrorToken, ArrayList StackTrace)
        {
            string trace = "";
            foreach (StackTraceEntry entry in StackTrace)
            {
                trace = "\nIn file " + entry.FileName + ": Line " + entry.LineNumber + ": Function '" + entry.FunctionName + "'" + trace;
            }
            string exception = "In file: " + ErrorToken.file + e.Message;
            consoleRTB.Text += "\n" + exception + trace;
        }

        private void CodeGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            handlingInterpreter.keepWorking = false;
            if (lastCallingThread.ThreadState == ThreadState.Suspended)
            {
                lastCallingThread.Resume();
            }
        }
    }
}
