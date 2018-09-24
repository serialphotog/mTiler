namespace mTiler
{
    partial class SettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkClearLogOnJobRun = new System.Windows.Forms.CheckBox();
            this.chkEnableVerboseLogging = new System.Windows.Forms.CheckBox();
            this.trackBarNThreads = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblNThreads = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarNThreads)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkClearLogOnJobRun);
            this.groupBox1.Controls.Add(this.chkEnableVerboseLogging);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(178, 95);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logging";
            // 
            // chkClearLogOnJobRun
            // 
            this.chkClearLogOnJobRun.AutoSize = true;
            this.chkClearLogOnJobRun.Location = new System.Drawing.Point(6, 54);
            this.chkClearLogOnJobRun.Name = "chkClearLogOnJobRun";
            this.chkClearLogOnJobRun.Size = new System.Drawing.Size(133, 17);
            this.chkClearLogOnJobRun.TabIndex = 2;
            this.chkClearLogOnJobRun.Text = "Clear Log On Job Start";
            this.chkClearLogOnJobRun.UseVisualStyleBackColor = true;
            // 
            // chkEnableVerboseLogging
            // 
            this.chkEnableVerboseLogging.AutoSize = true;
            this.chkEnableVerboseLogging.Location = new System.Drawing.Point(6, 31);
            this.chkEnableVerboseLogging.Name = "chkEnableVerboseLogging";
            this.chkEnableVerboseLogging.Size = new System.Drawing.Size(142, 17);
            this.chkEnableVerboseLogging.TabIndex = 1;
            this.chkEnableVerboseLogging.Text = "Enable Verbose Logging";
            this.chkEnableVerboseLogging.UseVisualStyleBackColor = true;
            // 
            // trackBarNThreads
            // 
            this.trackBarNThreads.LargeChange = 1;
            this.trackBarNThreads.Location = new System.Drawing.Point(9, 44);
            this.trackBarNThreads.Minimum = 1;
            this.trackBarNThreads.Name = "trackBarNThreads";
            this.trackBarNThreads.Size = new System.Drawing.Size(104, 45);
            this.trackBarNThreads.TabIndex = 1;
            this.trackBarNThreads.Value = 1;
            this.trackBarNThreads.ValueChanged += new System.EventHandler(this.trackBarNThreads_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblNThreads);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.trackBarNThreads);
            this.groupBox2.Location = new System.Drawing.Point(196, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(178, 95);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Performance";
            // 
            // lblNThreads
            // 
            this.lblNThreads.AutoSize = true;
            this.lblNThreads.Location = new System.Drawing.Point(120, 54);
            this.lblNThreads.Name = "lblNThreads";
            this.lblNThreads.Size = new System.Drawing.Size(13, 13);
            this.lblNThreads.TabIndex = 4;
            this.lblNThreads.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Max Number of  Tiling Threads:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(230, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(311, 113);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 146);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(414, 185);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(414, 185);
            this.Name = "SettingsDialog";
            this.Text = "mTiler Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarNThreads)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkClearLogOnJobRun;
        private System.Windows.Forms.CheckBox chkEnableVerboseLogging;
        private System.Windows.Forms.TrackBar trackBarNThreads;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblNThreads;
    }
}