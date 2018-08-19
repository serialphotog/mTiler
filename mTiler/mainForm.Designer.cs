namespace mTiler
{
    partial class mainForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.outputPathBtn = new System.Windows.Forms.Button();
            this.inputPathBtn = new System.Windows.Forms.Button();
            this.outputPathTxt = new System.Windows.Forms.TextBox();
            this.inputPathTxt = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.outputConsole = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel1.Controls.Add(this.btnStart);
            this.splitContainer1.Panel1.Controls.Add(this.outputPathBtn);
            this.splitContainer1.Panel1.Controls.Add(this.inputPathBtn);
            this.splitContainer1.Panel1.Controls.Add(this.outputPathTxt);
            this.splitContainer1.Panel1.Controls.Add(this.inputPathTxt);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.outputConsole);
            this.splitContainer1.Size = new System.Drawing.Size(584, 411);
            this.splitContainer1.SplitterDistance = 149;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(416, 114);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(497, 114);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // outputPathBtn
            // 
            this.outputPathBtn.Location = new System.Drawing.Point(478, 56);
            this.outputPathBtn.Name = "outputPathBtn";
            this.outputPathBtn.Size = new System.Drawing.Size(75, 23);
            this.outputPathBtn.TabIndex = 5;
            this.outputPathBtn.Text = "Browse";
            this.outputPathBtn.UseVisualStyleBackColor = true;
            this.outputPathBtn.Click += new System.EventHandler(this.outputPathBtn_Click);
            // 
            // inputPathBtn
            // 
            this.inputPathBtn.Location = new System.Drawing.Point(478, 21);
            this.inputPathBtn.Name = "inputPathBtn";
            this.inputPathBtn.Size = new System.Drawing.Size(75, 23);
            this.inputPathBtn.TabIndex = 4;
            this.inputPathBtn.Text = "Browse";
            this.inputPathBtn.UseVisualStyleBackColor = true;
            this.inputPathBtn.Click += new System.EventHandler(this.inputPathBtn_Click);
            // 
            // outputPathTxt
            // 
            this.outputPathTxt.Location = new System.Drawing.Point(99, 58);
            this.outputPathTxt.Name = "outputPathTxt";
            this.outputPathTxt.Size = new System.Drawing.Size(373, 20);
            this.outputPathTxt.TabIndex = 3;
            // 
            // inputPathTxt
            // 
            this.inputPathTxt.Location = new System.Drawing.Point(99, 23);
            this.inputPathTxt.Name = "inputPathTxt";
            this.inputPathTxt.Size = new System.Drawing.Size(373, 20);
            this.inputPathTxt.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output Path:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input Path:";
            // 
            // outputConsole
            // 
            this.outputConsole.Enabled = false;
            this.outputConsole.Location = new System.Drawing.Point(12, 3);
            this.outputConsole.Name = "outputConsole";
            this.outputConsole.Size = new System.Drawing.Size(560, 255);
            this.outputConsole.TabIndex = 0;
            this.outputConsole.Text = "";
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.splitContainer1);
            this.MaximumSize = new System.Drawing.Size(600, 450);
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "mainForm";
            this.Text = "mTiler";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox outputConsole;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox outputPathTxt;
        private System.Windows.Forms.TextBox inputPathTxt;
        private System.Windows.Forms.Button outputPathBtn;
        private System.Windows.Forms.Button inputPathBtn;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnStart;
    }
}

