namespace Lang
{
    partial class Find
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Query = new System.Windows.Forms.TextBox();
            this.Exit = new System.Windows.Forms.Label();
            this.PrevButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Replace = new System.Windows.Forms.TextBox();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.ShowHideReplace = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Query
            // 
            this.Query.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Query.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Query.Location = new System.Drawing.Point(45, 3);
            this.Query.Name = "Query";
            this.Query.Size = new System.Drawing.Size(96, 20);
            this.Query.TabIndex = 0;
            this.Query.TextChanged += new System.EventHandler(this.Query_TextChanged);
            this.Query.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Query_KeyDown);
            // 
            // Exit
            // 
            this.Exit.AutoSize = true;
            this.Exit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Exit.Location = new System.Drawing.Point(177, 6);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(16, 15);
            this.Exit.TabIndex = 1;
            this.Exit.Text = "X";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // PrevButton
            // 
            this.PrevButton.BackColor = System.Drawing.Color.Gray;
            this.PrevButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PrevButton.Location = new System.Drawing.Point(144, 2);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(16, 22);
            this.PrevButton.TabIndex = 2;
            this.PrevButton.Text = "◀";
            this.PrevButton.UseVisualStyleBackColor = false;
            this.PrevButton.Click += new System.EventHandler(this.PrevButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.BackColor = System.Drawing.Color.Gray;
            this.NextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NextButton.Location = new System.Drawing.Point(160, 2);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(16, 22);
            this.NextButton.TabIndex = 3;
            this.NextButton.Text = "▶";
            this.NextButton.UseVisualStyleBackColor = false;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Find:";
            // 
            // Replace
            // 
            this.Replace.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Replace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Replace.Location = new System.Drawing.Point(16, 28);
            this.Replace.Name = "Replace";
            this.Replace.Size = new System.Drawing.Size(100, 20);
            this.Replace.TabIndex = 5;
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.BackColor = System.Drawing.Color.Gray;
            this.ReplaceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReplaceButton.Location = new System.Drawing.Point(122, 27);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(67, 23);
            this.ReplaceButton.TabIndex = 6;
            this.ReplaceButton.Text = "Replace";
            this.ReplaceButton.UseVisualStyleBackColor = false;
            this.ReplaceButton.Click += new System.EventHandler(this.ReplaceButton_Click);
            // 
            // ShowHideReplace
            // 
            this.ShowHideReplace.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ShowHideReplace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShowHideReplace.Location = new System.Drawing.Point(0, 2);
            this.ShowHideReplace.Name = "ShowHideReplace";
            this.ShowHideReplace.Size = new System.Drawing.Size(16, 22);
            this.ShowHideReplace.TabIndex = 7;
            this.ShowHideReplace.Text = "▼";
            this.ShowHideReplace.UseVisualStyleBackColor = false;
            this.ShowHideReplace.Click += new System.EventHandler(this.ShowHideReplace_Click);
            // 
            // Find
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.ShowHideReplace);
            this.Controls.Add(this.ReplaceButton);
            this.Controls.Add(this.Replace);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.PrevButton);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.Query);
            this.Name = "Find";
            this.Size = new System.Drawing.Size(197, 27);
            this.Load += new System.EventHandler(this.Find_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Query;
        private System.Windows.Forms.Label Exit;
        private System.Windows.Forms.Button PrevButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Replace;
        private System.Windows.Forms.Button ReplaceButton;
        private System.Windows.Forms.Button ShowHideReplace;
    }
}
