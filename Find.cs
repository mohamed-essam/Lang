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

        private void PrevButton_Click(object sender, EventArgs e)
        {
            string b4 = DockTo.Text.Substring(0, DockTo.SelectionStart);
            int pos = b4.LastIndexOf(Query.Text);
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

        private void NextButton_Click(object sender, EventArgs e)
        {
            int pos = DockTo.Text.Substring(DockTo.SelectionStart + DockTo.SelectionLength).IndexOf(Query.Text);
            if (pos == -1)
            {
                pos = DockTo.Text.IndexOf(Query.Text);
                if (pos == -1)
                {
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
    }
}
