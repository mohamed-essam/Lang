using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Lang
{
    public partial class Find : UserControl
    {
        private int lastFindPos;
        private string lastSearchQuery;

        public Find()
        {
            InitializeComponent();
            lastFindPos = -1;
            lastSearchQuery = "";
            this.Width = 182;
            this.Height = 49;
        }

        private void Query_TextChanged(object sender, EventArgs e)
        {
            if (DockTo != null)
            {
                if (lastFindPos != -1)
                {
                    DockTo.SelectionStart = lastFindPos;
                    DockTo.SelectionLength = lastSearchQuery.Length;
                    DockTo.SelectionColor = SystemColors.WindowText;
                }
                int pos = DockTo.Text.IndexOf(Query.Text);
                if (pos >= 0)
                {
                    Query.ForeColor = SystemColors.WindowText;
                    DockTo.SelectionStart = pos;
                    DockTo.SelectionLength = Query.Text.Length;
                    DockTo.SelectionColor = Color.Red;
                }
                else
                {
                    Query.ForeColor = Color.Red;
                }
                if (Query.Text == "")
                {
                    Query.ForeColor = Color.Red;
                }
                lastFindPos = pos;
                lastSearchQuery = Query.Text;
            }
        }

        private void Prev()
        {
            int pos = DockTo.Text.LastIndexOf(Query.Text, DockTo.SelectionStart);
            if (pos == -1)
            {
                pos = DockTo.Text.LastIndexOf(Query.Text);
                if (pos == -1)
                {
                    return;
                }
            }
            if (lastFindPos != -1)
            {
                DockTo.SelectionStart = lastFindPos;
                DockTo.SelectionLength = lastSearchQuery.Length;
                DockTo.SelectionColor = SystemColors.WindowText;
            }
            DockTo.SelectionStart = pos;
            DockTo.SelectionLength = Query.Text.Length;
            DockTo.SelectionColor = Color.Red;
            lastFindPos = pos;
            lastSearchQuery = Query.Text;
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            Prev();
        }

        private void Next()
        {
            int pos = DockTo.Text.IndexOf(Query.Text, DockTo.SelectionStart + DockTo.SelectionLength);
            if (pos == -1)
            {
                pos = DockTo.Text.IndexOf(Query.Text);
                if (pos == -1)
                {
                    Query.ForeColor = Color.Red;
                    DockTo.SelectionColor = SystemColors.WindowText;
                    return;
                }
            }
            else
            {
                pos += DockTo.SelectionLength + DockTo.SelectionStart;
            }
            if (lastFindPos != -1)
            {
                DockTo.SelectionStart = lastFindPos;
                DockTo.SelectionLength = lastSearchQuery.Length;
                DockTo.SelectionColor = SystemColors.WindowText;
            }
            DockTo.SelectionStart = pos;
            DockTo.SelectionLength = Query.Text.Length;
            DockTo.SelectionColor = Color.Red;
            lastFindPos = pos;
            lastSearchQuery = Query.Text;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            if (lastFindPos != -1)
            {
                DockTo.SelectionStart = lastFindPos;
                DockTo.SelectionLength = lastSearchQuery.Length;
                DockTo.SelectionColor = SystemColors.WindowText;
            }
            this.Visible = false;
            DockTo.Focus();
            DockTo.SelectionLength = 0;
            lastFindPos = -1;
            lastSearchQuery = "";
            Query.Text = "";
        }

        public RichTextBox DockTo { get; set; }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (lastFindPos != -1 && lastFindPos == DockTo.SelectionStart && lastSearchQuery.Length == DockTo.SelectionLength && DockTo.SelectedText == lastSearchQuery)
            {
                DockTo.SelectionColor = SystemColors.WindowText;
                DockTo.SelectedText = Replace.Text;
                Next();
            }
            else
            {
                Next();
                DockTo.SelectedText = Replace.Text;
            }
        }

        private void Find_Load(object sender, EventArgs e)
        {

        }

        private void Query_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
