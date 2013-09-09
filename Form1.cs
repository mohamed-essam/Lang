using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lang.language;
using System.Collections;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Lang
{
    public partial class Form1 : Form
    {
        #region Members
        static internal LangManager langManager;
        LangConsole console;
        string path = null;
        bool FileChanged = false;
        string lastSavedFile = "";
        Thread runnerThread;
        bool wasRun = false;
        string curCode, curIn;
        System.Diagnostics.Stopwatch watch;
        bool shownData = false;
        ArrayList mustSpaceAfterTokens;
        #endregion

        public Form1()
        {
            InitializeComponent();
            LineNumberer.BackgroundGradient_AlphaColor = SystemColors.Control;
            LineNumberer.BackgroundGradient_BetaColor = SystemColors.Control;
            LineNumberer.BorderLines_Color = SystemColors.Control;
            LineNumberer.GridLines_Color = SystemColors.Control;
            LineNumberer.MarginLines_Color = SystemColors.Control;
            LineNumberer.LineNrs_LeadingZeroes = false;
            Text = Text + " | Untitled.lan";
            runnerThread = new Thread(new ThreadStart(Run));
            watch = new Stopwatch();
            mustSpaceAfterTokens = new ArrayList();
            populateSpaceTokens();
        }

        private void populateSpaceTokens()
        {
            mustSpaceAfterTokens.Add(TokenType.CATCH);
            mustSpaceAfterTokens.Add(TokenType.DOT);
            mustSpaceAfterTokens.Add(TokenType.ELSE);
            mustSpaceAfterTokens.Add(TokenType.ENDCATCH);
            mustSpaceAfterTokens.Add(TokenType.ENDCLASS);
            mustSpaceAfterTokens.Add(TokenType.ENDFUNCTION);
            mustSpaceAfterTokens.Add(TokenType.ENDIF);
            mustSpaceAfterTokens.Add(TokenType.ENDLOOP);
            mustSpaceAfterTokens.Add(TokenType.ENDTRY);
            mustSpaceAfterTokens.Add(TokenType.EOF);
            mustSpaceAfterTokens.Add(TokenType.L_BRACK);
            mustSpaceAfterTokens.Add(TokenType.L_PARA);
            mustSpaceAfterTokens.Add(TokenType.R_BRACK);
            mustSpaceAfterTokens.Add(TokenType.R_PARA);
            //mustSpaceAfterTokens.Add(TokenType.SEMICOLON);
            mustSpaceAfterTokens.Add(TokenType.STOP);
        }

        private void Run()
        {
            RunButton.Enabled = false;
            StopButton.Enabled = true;
            curCode = codeRTB.Text;
            curIn = InputRTB.Text;
            outputRTB.Text = "";
            try
            {
                langManager.updateCode(curCode);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            console.processInput(curIn);
            runnerThread = new Thread(new ThreadStart(langManager.run), 33554432);
            runnerThread.Start();
            wasRun = true;
            ProcessState.Text = "Running...";
            watch.Start();
            LiveErrors.Enabled = false;
        }

        #region Flow Control Button Presses
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (!runnerThread.IsAlive)
            {
                MessageBox.Show("The process isn't running!");
                return;
            }
            RunButton.Enabled = true;
            StopButton.Enabled = ResumeButton.Enabled = false;
            langManager.interpreter.keepWorking = false;
            if (runnerThread.ThreadState == System.Threading.ThreadState.Suspended)
            {
                runnerThread.Resume();
            }
            outputRTB.Text = console.output_ + "\r\n\r\n<Forcedely closed>";
            TimeSpan ts = watch.Elapsed;
            watch.Stop();
            watch.Reset();
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            wasRun = false;
            outputRTB.Text += "\r\nElapsed Time: " + elapsedTime;
            console.output_ = "";
            ProcessState.Text = "Stopped";
            DebugLV.Enabled = false;
            shownData = false;
            LiveErrors.Enabled = true;
            langManager.Dispose();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void ResumeButton_Click(object sender, EventArgs e)
        {
            if (runnerThread.ThreadState == System.Threading.ThreadState.Suspended)
                runnerThread.Resume();
            ProcessState.Text = "Running...";
            shownData = false;
            DebugLV.Enabled = false;
            ResumeButton.Enabled = false;
            VariableDetailsRTB.Text = "";
        }
        #endregion

        #region Form1 Events
        private void Form1_Load(object sender, EventArgs e)
        {
            console = new LangConsole();
            langManager = new LangManager(console, "Untitled.lan");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (codeRTB.Text != lastSavedFile)
                AskToSave();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            ButtonPressed(e);
        }
        #endregion

        #region File Managment Functions
        private void TextChangedChanged()
        {
            if (FileChanged)
            {
                if (!Text.EndsWith(" *"))
                {
                    Text += " *";
                }
            }
            else
            {
                if (Text.EndsWith(" *"))
                {
                    Text = Text.Substring(0, Text.Length - 2);
                }
            }
        }

        private void SaveFile()
        {
            if (path == null)
            {
                SaveFileDialog sa = new SaveFileDialog();
                sa.Filter = "Lang Files | *.lan";
                sa.InitialDirectory = Directory.GetCurrentDirectory();
                sa.DefaultExt = "lan";
                if (sa.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter sw = File.CreateText(sa.FileName);
                    path = sa.FileName;
                    sw.Write(codeRTB.Text);
                    sw.Close();
                    FileChanged = false;
                    TextChangedChanged();
                    Text = "Lang | " + path.Substring(path.LastIndexOf('\\') + 1);
                    lastSavedFile = codeRTB.Text;
                    langManager.lexer.FileName = path.Substring(path.LastIndexOf('\\') + 1);
                    langManager.interpreter.FileName = path.Substring(path.LastIndexOf('\\') + 1);
                    sw.Dispose();
                }
            }
            else
            {
                StreamWriter sw = File.CreateText(path);
                sw.Write(codeRTB.Text);
                sw.Close();
                FileChanged = false;
                TextChangedChanged();
                lastSavedFile = codeRTB.Text;
                sw.Dispose();
            }
        }

        private void AskToSave()
        {
            string fileName = "";
            if (path != null)
            {
                fileName = path.Substring(path.LastIndexOf('\\') + 1);
            }
            else
            {
                fileName = "Untitled.lan";
            }
            DialogResult dr = MessageBox.Show("The file " + fileName + " has been modified, Do you want to Save it?", "Save", MessageBoxButtons.YesNo);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                SaveFile();
            }
        }

        private void OpenFile()
        {
            if (codeRTB.Text != lastSavedFile)
            {
                AskToSave();
            }
            OpenFileDialog op = new OpenFileDialog();
            op.DefaultExt = "lan";
            op.Filter = "Lang Files | *.lan";
            op.InitialDirectory = Directory.GetCurrentDirectory();
            if (op.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = File.OpenText(op.FileName);
                path = op.FileName;
                codeRTB.Text = sr.ReadToEnd();
                sr.Close();
                Text = "Lang | " + path.Substring(path.LastIndexOf('\\') + 1);
                lastSavedFile = codeRTB.Text;
                langManager.lexer.FileName = path.Substring(path.LastIndexOf('\\') + 1);
                langManager.interpreter.FileName = path.Substring(path.LastIndexOf('\\') + 1);
                sr.Dispose();
            }
            op.Dispose();
        }

        private void NewFile()
        {
            if (codeRTB.Text != lastSavedFile)
                AskToSave();
            path = null;
            codeRTB.Text = "";
            Text = "Lang | Untitled.lan";
            lastSavedFile = "";
            FileChanged = false;
            TextChangedChanged();
            langManager.lexer.FileName = "Untitled.lan";
            langManager.interpreter.FileName = "Untitled.lan";
        }
        #endregion

        #region File Managment Events
        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sa = new SaveFileDialog();
            sa.Filter = "Lang Files | *.lan";
            sa.InitialDirectory = Directory.GetCurrentDirectory();
            sa.DefaultExt = "lan";
            if (sa.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = File.CreateText(sa.FileName);
                path = sa.FileName;
                sw.Write(codeRTB.Text);
                sw.Close();
                FileChanged = false;
                Text = "Lang | " + path.Substring(path.LastIndexOf('\\') + 1);
                lastSavedFile = Text;
                langManager.lexer.FileName = path.Substring(path.LastIndexOf('\\') + 1);
                sw.Dispose();
            }
        }

        private void CodeRTB_TextChanged(object sender, EventArgs e)
        {
            LiveErrors.Stop();
            LiveErrors.Start();
            if (codeRTB.Text == lastSavedFile)
            {
                FileChanged = false;
            }
            else
            {
                FileChanged = true;
            }
            TextChangedChanged();
        }
        #endregion

        private void ButtonPressed(KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
            {
                SaveFile();
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.O)
            {
                OpenFile();
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.N)
            {
                NewFile();
            }
            if (e.KeyCode == Keys.F5)
            {
                Run();
            }
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

        private void codeRTB_KeyDown(object sender, KeyEventArgs e)
        {
            ButtonPressed(e);
            if (e.KeyCode == Keys.Tab)
            {
                codeRTB.SelectedText = "    ";
                e.SuppressKeyPress = true;
            }
            #region hidden

            //if ((e.KeyCode == Keys.OemSemicolon && e.Modifiers == Keys.None) || e.KeyCode == Keys.Enter)
            //{
            //    int selpos = codeRTB.SelectionStart;
            //    int lastbn = 0;
            //    for (int i = 0; i < selpos; i++)
            //    {
            //        if (codeRTB.Text[i] == '\n')
            //            lastbn = i;
            //    }
            //    int lastbn2 = lastbn;
            //    string curLine = codeRTB.Text.Substring(lastbn, selpos - lastbn);
            //    Lexer lx = new Lexer(new LangManager(null, null), curLine);
            //    ArrayList tokens = lx.lex();
            //    string spaces = "";
            //    if (lastbn + 1 < codeRTB.Text.Length)
            //    {
            //        while (codeRTB.Text[++lastbn] == ' ')
            //        {
            //            spaces += ' ';
            //            if (lastbn + 1 >= codeRTB.Text.Length)
            //            {
            //                spaces += ' ';
            //                break;
            //            }
            //        }
            //    }
            //    lastbn = lastbn2;
            //    string nxtLine = "";
            //    Token lastToken = null;
            //    foreach (Token tok in tokens)
            //    {
            //        if (tok.type == TokenType.R_PARA || tok.type == TokenType.R_BRACK)
            //        {
            //            if (nxtLine.EndsWith(" "))
            //                nxtLine = nxtLine.TrimEnd(new char[] { ' ' });
            //        }
            //        if (!mustSpaceAfterTokens.Contains(tok.type))
            //        {
            //            if (tok.type == TokenType.MINUS)
            //            {
            //                if (lastToken == null || (lastToken.type != TokenType.ID && lastToken.type != TokenType.NUMBER))
            //                {
            //                    nxtLine += tok.lexeme;
            //                }
            //                else
            //                {
            //                    nxtLine += tok.lexeme + " ";
            //                }
            //            }
            //            else if (tok.type == TokenType.SEMICOLON)
            //            {
            //                nxtLine = nxtLine.TrimEnd(new char[] { ' ' });
            //                nxtLine += tok.lexeme + " ";
            //            }
            //            else
            //            {
            //                nxtLine += tok.lexeme + " ";
            //            }
            //        }
            //        else
            //        {
            //            nxtLine += tok.lexeme;
            //        }
            //        lastToken = tok;
            //    }
            //    nxtLine = nxtLine.TrimEnd(new char[] { ' ' });
            //    codeRTB.SelectionStart = ((lastbn == 0)?0:lastbn+1);
            //    codeRTB.SelectionLength = ((lastbn == 0) ? curLine.Length : curLine.Length - 1);
            //    codeRTB.SelectedText = spaces + nxtLine;
            //    //codeRTB.SelectionStart = ((lastbn == 0) ? 0 : lastbn + 1) + spaces.Length + nxtLine.Length;
            //}
            #endregion
            if (e.KeyCode == Keys.Enter)
            {
                int selpos = codeRTB.SelectionStart;
                int lastbn = 0;
                for (int i = 0; i < selpos; i++)
                {
                    if (codeRTB.Text[i] == '\n')
                        lastbn = i;
                }
                string spaces = "";
                if (lastbn + 1 < codeRTB.Text.Length)
                {
                    while (codeRTB.Text[++lastbn] == ' ')
                    {
                        spaces += ' ';
                        if (lastbn + 1 >= codeRTB.Text.Length)
                        {
                            spaces += ' ';
                            break;
                        }
                    }
                }

                codeRTB.SelectedText = "\n" + spaces;
                //codeRTB.SelectionStart = selpos + spaces.Length + 1;
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.F9)
            {
                int lnNum = getLineNumberAt(codeRTB.Text, codeRTB.SelectionStart);
                if (LineNumberer.BreakPoints.Contains(lnNum))
                {
                    LineNumberer.BreakPoints.Remove(lnNum);
                    langManager.interpreter.breakpoints.Remove(lnNum);
                }
                else
                {
                    LineNumberer.BreakPoints.Add(lnNum);
                    langManager.interpreter.breakpoints.Add(lnNum);
                }
                LineNumberer.Refresh();
            }
        }

        #region Timer Ticks
        private void CheckForEnded_Tick(object sender, EventArgs e)
        {
            if (!runnerThread.IsAlive && wasRun)
            {
                wasRun = false;
                RunButton.Enabled = true;
                StopButton.Enabled = ResumeButton.Enabled = false;
                outputRTB.Text = console.output_;
                console.output_ = "";
                ProcessState.Text = "Stopped";
                TimeSpan ts = watch.Elapsed;
                watch.Stop();
                watch.Reset();
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                if (langManager.lastException.Length > 0)
                {
                    MessageBox.Show(langManager.lastException, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    if (langManager.lastErrorToken != null)
                    {
                        codeRTB.SelectionStart = langManager.lastErrorToken.startIndex;
                        if (langManager.lastErrorToken.lexeme != null)
                        {
                            codeRTB.SelectionLength = langManager.lastErrorToken.lexeme.Length;
                        }
                        else
                        {
                            codeRTB.SelectionLength = langManager.lastErrorToken.endIndex - langManager.lastErrorToken.startIndex;
                        }
                    }
                }
                outputRTB.Text += "\r\nElapsed Time: " + elapsedTime;
                LiveErrors.Enabled = true;
                langManager.Dispose();
            }
            else if (wasRun)
            {
                if(!outputRTB.Text.Equals(console.output_))
                    outputRTB.Text = console.output_;
            }
            if (langManager.interpreter.isStopped && runnerThread.ThreadState == System.Threading.ThreadState.Suspended && !shownData)
            {
                codeRTB.SelectionStart = langManager.interpreter.stoppedLine;
                codeRTB.SelectionLength = 1;
                ProcessState.Text = "Paused";
                DebugLV.Items.Clear();
                DebugLV.Enabled = true;
                ResumeButton.Enabled = true;
                foreach (DictionaryEntry dic in ((Hashtable)langManager.interpreter.table[langManager.interpreter.level]))
                {
                    string ot = "";
                    LangObject obj = (LangObject)dic.Value;
                    ot = Convert.ToString(obj.objectType).ToLower();
                    ListViewItem lvi = new ListViewItem(new string[] { (string)dic.Key, ot });
                    DebugLV.Items.Add(lvi);
                }
                shownData = true;
            }
        }

        private void LiveErrors_Tick(object sender, EventArgs e)
        {
            langManager.INTERPRET = false;
            langManager.updateCode(codeRTB.Text);
            langManager.lastLiveException = "";
            Thread thread = new Thread(new ThreadStart(langManager.run));
            thread.Start();
            thread.Join(1000);
            ErrorsShower.Text = langManager.lastLiveException;
            if (langManager.lastLiveException.Length > 0)
            {
                string lineNumber = langManager.lastLiveException;
                int number = -1;
                try
                {
                    lineNumber = lineNumber.Substring(5);
                    lineNumber = lineNumber.Substring(0, lineNumber.IndexOf(':'));
                    number = Convert.ToInt32(lineNumber);
                }
                catch (Exception)
                {
                    // Nothing to do here
                }
                LineNumberer.ErrorLineNumber = number;
                LineNumberer.Refresh();
            }
            else
            {
                LineNumberer.ErrorLineNumber = -1;
                LineNumberer.Refresh();
            }
            langManager.INTERPRET = true;
        }
        #endregion

        #region Debug List View
        string getStringValue(LangObject obj)
        {
            string ot = "";
            switch (obj.objectType)
            {
                case ObjectType.STRING:
                    ot = ((LangString)obj).stringValue;
                    break;
                case ObjectType.NUMBER:
                    ot = Convert.ToString(((LangNumber)obj).numberValue);
                    break;
                case ObjectType.ARRAY:
                    break;
                case ObjectType.MAP:
                    LangMap mp = ((LangMap)obj);
                    ot = "{\n";
                    SortedDictionary<Object, LangObject> sd = new SortedDictionary<object,LangObject>();
                    foreach (DictionaryEntry dic in mp.arrayValue)
                    {
                        sd.Add(dic.Key, (LangObject)dic.Value);
                    }
                    foreach (KeyValuePair<Object, LangObject> dic in sd)
                    {
                        LangObject val;
                        val = ((LangObject)dic.Value);
                        ot += "[" + dic.Key.ToString() + "]" + " : ";
                        string ret_r = getStringValue(val) + ",";
                        ret_r += "\n";
                        ot += ret_r;
                    }
                    if (ot.EndsWith(","))
                        ot = ot.Substring(0, ot.Length - 1);
                    else if (ot.EndsWith(",\n"))
                        ot = ot.Substring(0, ot.Length - 2) + "\n";
                    ot += "}";
                    break;
                case ObjectType.STATE:
                    ot = "state";
                    break;
                case ObjectType.CLASS:
                    LangClass objcls = (LangClass)obj;
                    ot = objcls.name + ": {\n";
                    foreach (DictionaryEntry it in objcls.vars)
                    {
                        ot += "\"";
                        ot += ((string)it.Key);
                        ot += "\"";
                        ot += ":";
                        LangObject obj2 = ((LangObject)it.Value);
                        ot += getStringValue(obj2);
                        ot += ",";
                        ot += "\n";
                    }
                    if (ot.EndsWith(","))
                        ot = ot.Substring(0, ot.Length - 1);
                    else if (ot.EndsWith(",\n"))
                        ot = ot.Substring(0, ot.Length - 2) + "\n";
                    ot += "}";
                    break;
                default:
                    break;
            }
            int lastbackn = -1;
            for (int i = 0; i < ot.Length; i++)
            {
                if (ot[i] == '\n')
                {
                    lastbackn = i;
                    ot = ot.Substring(0, i) + "\n    " + ot.Substring(i + 1);
                    i++;
                }
            }
            if (lastbackn != -1)
            {
                ot = ot.Substring(0, lastbackn+1) + ot.Substring(lastbackn + 5);
            }
            return ot;
        }

        private void DebugLV_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item.SubItems.Count == 2)
            {
                VariableDetailsRTB.Text = getStringValue(((LangObject)((Hashtable)langManager.interpreter.table[langManager.interpreter.level])[(string)e.Item.Text]));
            }
        }
        #endregion
    }
}
