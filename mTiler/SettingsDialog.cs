using mTiler.Core;
using mTiler.Core.Imaging;
using System;
using System.Windows.Forms;

namespace mTiler
{
    public partial class SettingsDialog : Form
    {
        /// <summary>
        /// The app controller
        /// </summary>
        ApplicationController AppController = ApplicationController.Instance;

        private bool EnableVerboseLogging = Properties.Settings.Default.EnableVerboseLogging;
        private bool ClearLogOnJobStart = Properties.Settings.Default.ClearLogOnJobStart;
        private byte MaxNumberThreads = Properties.Settings.Default.MaxNumberTilingThreads;
        private string EnabledMergeProcess = Properties.Settings.Default.EnabledMergeProcess;

        public SettingsDialog()
        {
            InitializeComponent();

            // Initialize the merge process combo box
            comboMergeProcess.DataSource = BitmapHandler.GetAvailableProcesses();

            // Load data
            chkEnableVerboseLogging.Checked = EnableVerboseLogging;
            chkClearLogOnJobRun.Checked = ClearLogOnJobStart;
            trackBarNThreads.Value = MaxNumberThreads;
            comboMergeProcess.SelectedItem = EnabledMergeProcess;
        }

        /// <summary>
        /// Handles the click event for the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the click event for the save button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableVerboseLogging = chkEnableVerboseLogging.Checked;
            Properties.Settings.Default.ClearLogOnJobStart = chkClearLogOnJobRun.Checked;
            Properties.Settings.Default.MaxNumberTilingThreads = (byte)trackBarNThreads.Value;
            Properties.Settings.Default.EnabledMergeProcess = (string)comboMergeProcess.SelectedItem;

            Properties.Settings.Default.Save();
            AppController.ReloadConfig();
            Close();
        }

        /// <summary>
        /// Number of threads track bar value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarNThreads_ValueChanged(object sender, EventArgs e)
        {
            lblNThreads.Text = trackBarNThreads.Value.ToString();
        }
    }
}
