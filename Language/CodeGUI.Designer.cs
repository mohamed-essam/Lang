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
            this.SuspendLayout();
            // 
            // consoleRTB
            // 
            this.consoleRTB.BackColor = System.Drawing.SystemColors.MenuText;
            this.consoleRTB.ForeColor = System.Drawing.SystemColors.Info;
            this.consoleRTB.Location = new System.Drawing.Point(13, 38);
            this.consoleRTB.Name = "consoleRTB";
            this.consoleRTB.Size = new System.Drawing.Size(359, 442);
            this.consoleRTB.TabIndex = 0;
            this.consoleRTB.Text = "";
            this.consoleRTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.consoleRTB_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Console:";
            // 
            // DebugTV
            // 
            this.DebugTV.Location = new System.Drawing.Point(378, 38);
            this.DebugTV.Name = "DebugTV";
            this.DebugTV.Size = new System.Drawing.Size(313, 442);
            this.DebugTV.TabIndex = 2;
            // 
            // ResumeButton
            // 
            this.ResumeButton.Enabled = false;
            this.ResumeButton.Location = new System.Drawing.Point(378, 13);
            this.ResumeButton.Name = "ResumeButton";
            this.ResumeButton.Size = new System.Drawing.Size(75, 23);
            this.ResumeButton.TabIndex = 3;
            this.ResumeButton.Text = "Resume";
            this.ResumeButton.UseVisualStyleBackColor = true;
            this.ResumeButton.Click += new System.EventHandler(this.ResumeButton_Click);
            // 
            // CodeGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 492);
            this.Controls.Add(this.ResumeButton);
            this.Controls.Add(this.DebugTV);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.consoleRTB);
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
    }
}