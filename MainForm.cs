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
    public partial class MainForm : Form
    {
        #region Members
        string path = null;
        bool FileChanged = false;
        string lastSavedFile = "";
        string curCode;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            LineNumberer.BackgroundGradient_AlphaColor = SystemColors.Control;
            LineNumberer.BackgroundGradient_BetaColor = SystemColors.Control;
            LineNumberer.BorderLines_Color = SystemColors.Control;
            LineNumberer.GridLines_Color = SystemColors.Control;
            LineNumberer.MarginLines_Color = SystemColors.Control;
            LineNumberer.LineNrs_LeadingZeroes = false;
            Text = Text + " | Untitled.lan";
        }

        private void Run()
        {
            curCode = codeRTB.Text;
            string fileName = "Untitled.lan";
            if (path != null)
            {
                fileName = path.Substring(path.LastIndexOf('\\') + 1);
            }
            LangManager langManager = new LangManager(fileName);
            langManager.interpreter.breakpoints = (ArrayList)LineNumberer.BreakPoints.Clone();
            try
            {
                langManager.updateCode(curCode);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Thread runnerThread = new Thread(new ThreadStart(langManager.run), 33554432);
            Thread.Sleep(500);
            runnerThread.Start();
            LiveErrors.Enabled = false;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Run();
        }

        #region MainForm Events
        private void Form1_Load(object sender, EventArgs e)
        {
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

        private void MainForm_Resize(object sender, EventArgs e)
        {
            RunButton.Location = new Point(this.Size.Width - 99, RunButton.Location.Y);
            codeRTB.Size = new Size(this.Size.Width - 51, this.Size.Height - 128);
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
                }
                else
                {
                    LineNumberer.BreakPoints.Add(lnNum);
                }
                LineNumberer.Refresh();
            }
        }

        private void LiveErrors_Tick(object sender, EventArgs e)
        {
            string fileName = "Untitled.lan";
            if (path != null)
            {
                fileName = path.Substring(path.LastIndexOf('\\') + 1);
            }
            LangManager langManager = new LangManager(fileName);
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
            }
            else
            {
                LineNumberer.ErrorLineNumber = -1;
            }
            LineNumberer.Refresh();
            langManager.INTERPRET = true;
        }
    }
}
