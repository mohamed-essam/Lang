namespace Lang
{
    partial class Form1
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
            this.outputRTB = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.InputRTB = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.LiveErrors = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ErrorsShower = new System.Windows.Forms.ToolStripStatusLabel();
            this.StopButton = new System.Windows.Forms.Button();
            this.CheckForEnded = new System.Windows.Forms.Timer(this.components);
            this.ProcessState = new System.Windows.Forms.Label();
            this.ResumeButton = new System.Windows.Forms.Button();
            this.VariableDetailsRTB = new System.Windows.Forms.RichTextBox();
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DebugLV = new System.Windows.Forms.ListView();
            this.LineNumberer = new LineNumbers.LineNumbers_For_RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(317, 31);
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
            this.codeRTB.Location = new System.Drawing.Point(33, 60);
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
            this.label1.Location = new System.Drawing.Point(9, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Code";
            // 
            // outputRTB
            // 
            this.outputRTB.Location = new System.Drawing.Point(398, 261);
            this.outputRTB.Name = "outputRTB";
            this.outputRTB.Size = new System.Drawing.Size(302, 179);
            this.outputRTB.TabIndex = 4;
            this.outputRTB.Text = "";
            this.outputRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(398, 245);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(398, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Input";
            // 
            // InputRTB
            // 
            this.InputRTB.Location = new System.Drawing.Point(398, 60);
            this.InputRTB.Name = "InputRTB";
            this.InputRTB.Size = new System.Drawing.Size(302, 182);
            this.InputRTB.TabIndex = 8;
            this.InputRTB.Text = "";
            this.InputRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1010, 24);
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
            this.statusStrip1.Size = new System.Drawing.Size(1010, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "Hello World";
            // 
            // ErrorsShower
            // 
            this.ErrorsShower.Name = "ErrorsShower";
            this.ErrorsShower.Size = new System.Drawing.Size(0, 17);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(236, 31);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 12;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            this.StopButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // CheckForEnded
            // 
            this.CheckForEnded.Enabled = true;
            this.CheckForEnded.Interval = 10;
            this.CheckForEnded.Tick += new System.EventHandler(this.CheckForEnded_Tick);
            // 
            // ProcessState
            // 
            this.ProcessState.AutoSize = true;
            this.ProcessState.Location = new System.Drawing.Point(653, 36);
            this.ProcessState.Name = "ProcessState";
            this.ProcessState.Size = new System.Drawing.Size(47, 13);
            this.ProcessState.TabIndex = 13;
            this.ProcessState.Text = "Stopped";
            // 
            // ResumeButton
            // 
            this.ResumeButton.Enabled = false;
            this.ResumeButton.Location = new System.Drawing.Point(155, 31);
            this.ResumeButton.Name = "ResumeButton";
            this.ResumeButton.Size = new System.Drawing.Size(75, 23);
            this.ResumeButton.TabIndex = 14;
            this.ResumeButton.Text = "Resume";
            this.ResumeButton.UseVisualStyleBackColor = true;
            this.ResumeButton.Click += new System.EventHandler(this.ResumeButton_Click);
            // 
            // VariableDetailsRTB
            // 
            this.VariableDetailsRTB.DetectUrls = false;
            this.VariableDetailsRTB.Location = new System.Drawing.Point(707, 261);
            this.VariableDetailsRTB.Name = "VariableDetailsRTB";
            this.VariableDetailsRTB.Size = new System.Drawing.Size(291, 179);
            this.VariableDetailsRTB.TabIndex = 16;
            this.VariableDetailsRTB.Text = "";
            // 
            // name
            // 
            this.name.Text = "Variable";
            this.name.Width = 130;
            // 
            // type
            // 
            this.type.Text = "Type";
            this.type.Width = 157;
            // 
            // DebugLV
            // 
            this.DebugLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.type});
            this.DebugLV.Location = new System.Drawing.Point(707, 60);
            this.DebugLV.Name = "DebugLV";
            this.DebugLV.Size = new System.Drawing.Size(291, 179);
            this.DebugLV.TabIndex = 15;
            this.DebugLV.UseCompatibleStateImageBehavior = false;
            this.DebugLV.View = System.Windows.Forms.View.Details;
            this.DebugLV.ItemMouseHover += new System.Windows.Forms.ListViewItemMouseHoverEventHandler(this.DebugLV_ItemMouseHover);
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
            this.LineNumberer.Location = new System.Drawing.Point(15, 60);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 469);
            this.Controls.Add(this.VariableDetailsRTB);
            this.Controls.Add(this.DebugLV);
            this.Controls.Add(this.ResumeButton);
            this.Controls.Add(this.ProcessState);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.LineNumberer);
            this.Controls.Add(this.codeRTB);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.InputRTB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.outputRTB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Lang";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
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
        private System.Windows.Forms.RichTextBox outputRTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox InputRTB;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveButton;
        private System.Windows.Forms.ToolStripMenuItem OpenButton;
        private System.Windows.Forms.ToolStripMenuItem NewButton;
        private System.Windows.Forms.ToolStripMenuItem SaveAsButton;
        private System.Windows.Forms.Timer LiveErrors;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel ErrorsShower;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Timer CheckForEnded;
        private System.Windows.Forms.Label ProcessState;
        private System.Windows.Forms.Button ResumeButton;
        internal LineNumbers.LineNumbers_For_RichTextBox LineNumberer;
        private System.Windows.Forms.RichTextBox VariableDetailsRTB;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.ListView DebugLV;
    }
}

