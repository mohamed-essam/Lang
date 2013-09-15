namespace Lang
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.RunButton = new System.Windows.Forms.Button();
            this.codeRTB = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.LiveErrors = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ErrorsShower = new System.Windows.Forms.ToolStripStatusLabel();
            this.Finder = new Lang.Find();
            this.LineNumberer = new LineNumbers.LineNumbers_For_RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(311, 31);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 0;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            this.RunButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // codeRTB
            // 
            this.codeRTB.AcceptsTab = true;
            this.codeRTB.DetectUrls = false;
            this.codeRTB.Location = new System.Drawing.Point(27, 60);
            this.codeRTB.Name = "codeRTB";
            this.codeRTB.Size = new System.Drawing.Size(359, 380);
            this.codeRTB.TabIndex = 1;
            this.codeRTB.Text = "";
            this.codeRTB.WordWrap = false;
            this.codeRTB.TextChanged += new System.EventHandler(this.CodeRTB_TextChanged);
            this.codeRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.codeRTB_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Code";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(394, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.OpenButton,
            this.SaveButton,
            this.SaveAsButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // NewButton
            // 
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(112, 22);
            this.NewButton.Text = "New";
            this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // OpenButton
            // 
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(112, 22);
            this.OpenButton.Text = "Open";
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(112, 22);
            this.SaveButton.Text = "Save";
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(112, 22);
            this.SaveAsButton.Text = "Save as";
            this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // LiveErrors
            // 
            this.LiveErrors.Enabled = true;
            this.LiveErrors.Interval = 500;
            this.LiveErrors.Tick += new System.EventHandler(this.LiveErrors_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ErrorsShower});
            this.statusStrip1.Location = new System.Drawing.Point(0, 447);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(394, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "Hello World";
            // 
            // ErrorsShower
            // 
            this.ErrorsShower.Name = "ErrorsShower";
            this.ErrorsShower.Size = new System.Drawing.Size(0, 17);
            // 
            // Finder
            // 
            this.Finder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Finder.DockTo = this.codeRTB;
            this.Finder.Location = new System.Drawing.Point(234, 64);
            this.Finder.Name = "Finder";
            this.Finder.Size = new System.Drawing.Size(147, 26);
            this.Finder.TabIndex = 12;
            this.Finder.Visible = false;
            // 
            // LineNumberer
            // 
            this.LineNumberer._SeeThroughMode_ = false;
            this.LineNumberer.AutoSizing = true;
            this.LineNumberer.BackgroundGradient_AlphaColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.LineNumberer.BackgroundGradient_BetaColor = System.Drawing.Color.LightSteelBlue;
            this.LineNumberer.BackgroundGradient_Direction = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            this.LineNumberer.BorderLines_Color = System.Drawing.Color.SlateGray;
            this.LineNumberer.BorderLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
            this.LineNumberer.BorderLines_Thickness = 1F;
            this.LineNumberer.DockSide = LineNumbers.LineNumbers_For_RichTextBox.LineNumberDockSide.Left;
            this.LineNumberer.GridLines_Color = System.Drawing.Color.SlateGray;
            this.LineNumberer.GridLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
            this.LineNumberer.GridLines_Thickness = 1F;
            this.LineNumberer.LineNrs_Alignment = System.Drawing.ContentAlignment.TopRight;
            this.LineNumberer.LineNrs_AntiAlias = true;
            this.LineNumberer.LineNrs_AsHexadecimal = false;
            this.LineNumberer.LineNrs_ClippedByItemRectangle = true;
            this.LineNumberer.LineNrs_LeadingZeroes = true;
            this.LineNumberer.LineNrs_Offset = new System.Drawing.Size(0, 0);
            this.LineNumberer.Location = new System.Drawing.Point(9, 60);
            this.LineNumberer.Margin = new System.Windows.Forms.Padding(0);
            this.LineNumberer.MarginLines_Color = System.Drawing.Color.SlateGray;
            this.LineNumberer.MarginLines_Side = LineNumbers.LineNumbers_For_RichTextBox.LineNumberDockSide.Right;
            this.LineNumberer.MarginLines_Style = System.Drawing.Drawing2D.DashStyle.Solid;
            this.LineNumberer.MarginLines_Thickness = 1F;
            this.LineNumberer.Name = "LineNumberer";
            this.LineNumberer.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.LineNumberer.ParentRichTextBox = this.codeRTB;
            this.LineNumberer.Show_BackgroundGradient = true;
            this.LineNumberer.Show_BorderLines = true;
            this.LineNumberer.Show_GridLines = true;
            this.LineNumberer.Show_LineNrs = true;
            this.LineNumberer.Show_MarginLines = true;
            this.LineNumberer.Size = new System.Drawing.Size(17, 380);
            this.LineNumberer.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 469);
            this.Controls.Add(this.Finder);
            this.Controls.Add(this.LineNumberer);
            this.Controls.Add(this.codeRTB);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Lang";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.RichTextBox codeRTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveButton;
        private System.Windows.Forms.ToolStripMenuItem OpenButton;
        private System.Windows.Forms.ToolStripMenuItem NewButton;
        private System.Windows.Forms.ToolStripMenuItem SaveAsButton;
        private System.Windows.Forms.Timer LiveErrors;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel ErrorsShower;
        internal LineNumbers.LineNumbers_For_RichTextBox LineNumberer;
        private Find Finder;
    }
}

