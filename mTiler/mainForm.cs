using mTiler.Core;
using System;
using System.Windows.Forms;

namespace mTiler
{
    public partial class mainForm : Form
    {
        /// <summary>
        /// Reference to the logger instance
        /// </summary>
        private Logger logger { get; set; }

        /// <summary>
        /// Reference to the tiling engine.
        /// </summary>
        private TilingEngine tilingEngine;

        /// <summary>
        /// Initializes the main form
        /// </summary>
        public mainForm()
        {
            InitializeComponent();

            // Setup some output console properties
            outputConsole.ReadOnly = true;
            outputConsole.WordWrap = false;

            // Initialize the logger component 
            this.logger = new Logger(this.outputConsole);
        }

        /// <summary>
        /// Allows the user to select a folder
        /// </summary>
        /// <returns>String - The path to the folder</returns>
        private String openFolderPath()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "";
        }

        /// <summary>
        /// Handles the click event for the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the click event for the inputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputPathBtn_Click(object sender, EventArgs e)
        {
            // Get the input path
            String path = openFolderPath();
            inputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the outputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void outputPathBtn_Click(object sender, EventArgs e)
        {
            String path = openFolderPath();
            outputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Pass everything over to the stacking engine
            this.tilingEngine = new TilingEngine(this.inputPathTxt.Text, this.outputPathTxt.Text, this.logger);
            tilingEngine.tile();
        }
    }
}
