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
            this.SuspendLayout();
            // 
            // Query
            // 
            this.Query.Location = new System.Drawing.Point(3, 3);
            this.Query.Name = "Query";
            this.Query.Size = new System.Drawing.Size(100, 20);
            this.Query.TabIndex = 0;
            this.Query.TextChanged += new System.EventHandler(this.Query_TextChanged);
            // 
            // Exit
            // 
            this.Exit.AutoSize = true;
            this.Exit.Location = new System.Drawing.Point(133, 8);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(14, 13);
            this.Exit.TabIndex = 1;
            this.Exit.Text = "X";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // PrevButton
            // 
            this.PrevButton.Location = new System.Drawing.Point(102, 2);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(15, 21);
            this.PrevButton.TabIndex = 2;
            this.PrevButton.Text = "<";
            this.PrevButton.UseVisualStyleBackColor = true;
            this.PrevButton.Click += new System.EventHandler(this.PrevButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(117, 2);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(15, 21);
            this.NextButton.TabIndex = 3;
            this.NextButton.Text = ">";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // Find
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.PrevButton);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.Query);
            this.Name = "Find";
            this.Size = new System.Drawing.Size(147, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Query;
        private System.Windows.Forms.Label Exit;
        private System.Windows.Forms.Button PrevButton;
        private System.Windows.Forms.Button NextButton;
    }
}
