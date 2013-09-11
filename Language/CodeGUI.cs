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
            codeRTB.Text = handlingInterpreter.langManager.lexer.GetCode();
            CodeLineNumberer.BackgroundGradient_AlphaColor = SystemColors.Control;
            CodeLineNumberer.BackgroundGradient_BetaColor = SystemColors.Control;
            CodeLineNumberer.BreakPoints = _handler.breakpoints;
            CodeLineNumberer.Refresh();
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
            consoleRTB.SelectedText = "\n" + _line + "\n";
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
                node.Text = "\"" + ((LangString)obj).stringValue + "\"";
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

        private int IndexOfNth(string str, char c, int n)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    n--;
                    if (n < 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal void Debug(Hashtable vars, int Line)
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
            consoleRTB.Visible = false;
            codeRTB.Visible = true;
            CodeLineNumberer.Visible = true;
            int ind = 0, end = 0;
            if (Line == 1)
            {
                end = codeRTB.Text.IndexOf('\n');
                if (end == -1)
                    end = codeRTB.Text.Length;
            }
            else
            {
                ind = IndexOfNth(codeRTB.Text, '\n', Line - 2) + 1;
                end = IndexOfNth(codeRTB.Text, '\n', Line - 1);
                if (end == -1)
                {
                    end = codeRTB.Text.Length;
                }
            }
            codeRTB.Focus();
            codeRTB.SelectionStart = ind;
            codeRTB.SelectionLength = end - ind;
        }

        int getLineNumberAt(string code, int idx)
        {
            int line = 1;
            for (int i = 0; i < idx; i++)
            {
                if (code[i] == '\n')
                    line++;
            }
            return line;
        }

        private void ResumeButton_Click(object sender, EventArgs e)
        {
            lastCallingThread.Resume();
            DebugTV.Nodes.Clear();
            consoleRTB.Visible = true;
            codeRTB.Visible = false;
            CodeLineNumberer.Visible = false;
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

        private void codeRTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                int lnNum = getLineNumberAt(codeRTB.Text, codeRTB.SelectionStart);
                if (CodeLineNumberer.BreakPoints.Contains(lnNum))
                {
                    CodeLineNumberer.BreakPoints.Remove(lnNum);
                }
                else
                {
                    CodeLineNumberer.BreakPoints.Add(lnNum);
                }
                CodeLineNumberer.Refresh();
                handlingInterpreter.breakpoints = CodeLineNumberer.BreakPoints;
            }
        }

        private void SwitchButton_Click(object sender, EventArgs e)
        {
            if (consoleRTB.Visible)
            {
                consoleRTB.Visible = false;
                codeRTB.Visible = true;
                CodeLineNumberer.Visible = true;
            }
            else
            {
                consoleRTB.Visible = true;
                codeRTB.Visible = false;
                CodeLineNumberer.Visible = false;
            }
        }
    }
}
