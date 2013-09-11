namespace Lang.Language
{
    partial class CodeGUI
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
            this.consoleRTB = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DebugTV = new System.Windows.Forms.TreeView();
            this.ResumeButton = new System.Windows.Forms.Button();
            this.codeRTB = new System.Windows.Forms.RichTextBox();
            this.CodeLineNumberer = new LineNumbers.LineNumbers_For_RichTextBox();
            this.SwitchButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // consoleRTB
            // 
            this.consoleRTB.BackColor = System.Drawing.SystemColors.MenuText;
            this.consoleRTB.ForeColor = System.Drawing.SystemColors.Info;
            this.consoleRTB.Location = new System.Drawing.Point(27, 38);
            this.consoleRTB.Name = "consoleRTB";
            this.consoleRTB.Size = new System.Drawing.Size(359, 442);
            this.consoleRTB.TabIndex = 0;
            this.consoleRTB.Text = "";
            this.consoleRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.consoleRTB_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Console:";
            // 
            // DebugTV
            // 
            this.DebugTV.Location = new System.Drawing.Point(392, 38);
            this.DebugTV.Name = "DebugTV";
            this.DebugTV.Size = new System.Drawing.Size(313, 442);
            this.DebugTV.TabIndex = 2;
            // 
            // ResumeButton
            // 
            this.ResumeButton.Enabled = false;
            this.ResumeButton.Location = new System.Drawing.Point(392, 13);
            this.ResumeButton.Name = "ResumeButton";
            this.ResumeButton.Size = new System.Drawing.Size(75, 23);
            this.ResumeButton.TabIndex = 3;
            this.ResumeButton.Text = "Resume";
            this.ResumeButton.UseVisualStyleBackColor = true;
            this.ResumeButton.Click += new System.EventHandler(this.ResumeButton_Click);
            // 
            // codeRTB
            // 
            this.codeRTB.Location = new System.Drawing.Point(26, 38);
            this.codeRTB.Name = "codeRTB";
            this.codeRTB.ReadOnly = true;
            this.codeRTB.Size = new System.Drawing.Size(360, 442);
            this.codeRTB.TabIndex = 4;
            this.codeRTB.Text = "";
            this.codeRTB.Visible = false;
            this.codeRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.codeRTB_KeyDown);
            // 
            // CodeLineNumberer
            // 
            this.CodeLineNumberer._SeeThroughMode_ = false;
            this.CodeLineNumberer.AutoSizing = true;
            this.CodeLineNumberer.BackgroundGradient_AlphaColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CodeLineNumberer.BackgroundGradient_BetaColor = System.Drawing.Color.LightSteelBlue;
            this.CodeLineNumberer.BackgroundGradient_Direction = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            this.CodeLineNumberer.BorderLines_Color = System.Drawing.Color.SlateGray;
            this.CodeLineNumberer.BorderLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
            this.CodeLineNumberer.BorderLines_Thickness = 1F;
            this.CodeLineNumberer.DockSide = LineNumbers.LineNumbers_For_RichTextBox.LineNumberDockSide.Left;
            this.CodeLineNumberer.GridLines_Color = System.Drawing.Color.SlateGray;
            this.CodeLineNumberer.GridLines_Style = System.Drawing.Drawing2D.DashStyle.Dot;
            this.CodeLineNumberer.GridLines_Thickness = 1F;
            this.CodeLineNumberer.LineNrs_Alignment = System.Drawing.ContentAlignment.TopRight;
            this.CodeLineNumberer.LineNrs_AntiAlias = true;
            this.CodeLineNumberer.LineNrs_AsHexadecimal = false;
            this.CodeLineNumberer.LineNrs_ClippedByItemRectangle = true;
            this.CodeLineNumberer.LineNrs_LeadingZeroes = true;
            this.CodeLineNumberer.LineNrs_Offset = new System.Drawing.Size(0, 0);
            this.CodeLineNumberer.Location = new System.Drawing.Point(8, 38);
            this.CodeLineNumberer.Margin = new System.Windows.Forms.Padding(0);
            this.CodeLineNumberer.MarginLines_Color = System.Drawing.Color.SlateGray;
            this.CodeLineNumberer.MarginLines_Side = LineNumbers.LineNumbers_For_RichTextBox.LineNumberDockSide.Right;
            this.CodeLineNumberer.MarginLines_Style = System.Drawing.Drawing2D.DashStyle.Solid;
            this.CodeLineNumberer.MarginLines_Thickness = 1F;
            this.CodeLineNumberer.Name = "CodeLineNumberer";
            this.CodeLineNumberer.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.CodeLineNumberer.ParentRichTextBox = this.codeRTB;
            this.CodeLineNumberer.Show_BackgroundGradient = true;
            this.CodeLineNumberer.Show_BorderLines = true;
            this.CodeLineNumberer.Show_GridLines = true;
            this.CodeLineNumberer.Show_LineNrs = true;
            this.CodeLineNumberer.Show_MarginLines = true;
            this.CodeLineNumberer.Size = new System.Drawing.Size(17, 442);
            this.CodeLineNumberer.TabIndex = 5;
            this.CodeLineNumberer.Visible = false;
            // 
            // SwitchButton
            // 
            this.SwitchButton.Location = new System.Drawing.Point(310, 12);
            this.SwitchButton.Name = "SwitchButton";
            this.SwitchButton.Size = new System.Drawing.Size(75, 23);
            this.SwitchButton.TabIndex = 6;
            this.SwitchButton.Text = "Switch View";
            this.SwitchButton.UseVisualStyleBackColor = true;
            this.SwitchButton.Click += new System.EventHandler(this.SwitchButton_Click);
            // 
            // CodeGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 492);
            this.Controls.Add(this.SwitchButton);
            this.Controls.Add(this.CodeLineNumberer);
            this.Controls.Add(this.ResumeButton);
            this.Controls.Add(this.DebugTV);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.consoleRTB);
            this.Controls.Add(this.codeRTB);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CodeGUI";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "CodeGUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CodeGUI_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox consoleRTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView DebugTV;
        private System.Windows.Forms.Button ResumeButton;
        private System.Windows.Forms.RichTextBox codeRTB;
        private LineNumbers.LineNumbers_For_RichTextBox CodeLineNumberer;
        private System.Windows.Forms.Button SwitchButton;
    }
}