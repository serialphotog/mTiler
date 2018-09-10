/*
Copyright 2018 Adam Thompson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using mTiler.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mTiler
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Reference to the logger instance
        /// </summary>
        private Logger Logger { get; set; }

        /// <summary>
        /// Reference to the tiling engine.
        /// </summary>
        private TilingEngine TilingEngine;

        /// <summary>
        /// The thread the tiling engine runs in
        /// </summary>
        private Thread TilingEngineThread;

        /// <summary>
        /// Tracks the total work for the progress bar
        /// </summary>
        private int TotalWork = 0;

        /// <summary>
        /// Delegates to update the progress bar
        /// </summary>
        /// <param name="progress"></param>
        public delegate void UpdateProgressDelegate(int progress);
        public UpdateProgressDelegate UpdateProgress;

        /// <summary>
        /// Initializes the main form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Initialize the update progress delegate handler
            UpdateProgress = new UpdateProgressDelegate(updateProgress);

            // Setup some output console properties
            outputConsole.ReadOnly = true;
            outputConsole.WordWrap = false;

            // Initialize the logger component 
            this.Logger = new Logger(this.outputConsole);
        }

        /// <summary>
        /// Allows the user to select a folder
        /// </summary>
        /// <returns>String - The path to the folder</returns>
        private String OpenFolderPath()
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
            if (TilingEngine != null)
                TilingEngine.StopRequested = true;
            Logger.StopRequested = true;
            Application.Exit();
        }

        /// <summary>
        /// Handles the click event for the inputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputPathBtn_Click(object sender, EventArgs e)
        {
            // Get the input path
            String path = OpenFolderPath();
            inputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the outputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void outputPathBtn_Click(object sender, EventArgs e)
        {
            String path = OpenFolderPath();
            outputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnStart_Click(object sender, EventArgs e)
        {
            // Initialize the tiling engine
            TilingEngine = new TilingEngine(inputPathTxt.Text, outputPathTxt.Text, Logger, this);

            // Load the data for the tiling engine
            await Task.Run(() => TilingEngine.Init());

            // Setup the progress bar
            TotalWork = TilingEngine.GetNTiles();
            progressBar.Maximum = TotalWork;
            progressBar.Step = 1;
            progressBar.Value = 0;
            lblProgress.Text = "0%";

            // Spawn the thread for the tiling engine
            if (TotalWork > 0)
            {
                TilingEngine.StopRequested = false;
                ThreadStart tilingThreadChildRef = new ThreadStart(TilingEngine.Tile);
                TilingEngineThread = new Thread(tilingThreadChildRef);
                TilingEngineThread.Start();
            }
        }


        /// <summary>
        /// Updates the progress bar
        /// </summary>
        /// <param name="progress"></param>
        private void updateProgress(int progress)
        {
            double progressPercent = ((double)progress / TotalWork) * 100;
            lblProgress.Text = Decimal.Round((decimal)progressPercent, 0, MidpointRounding.AwayFromZero) + "%";
            progressBar.Value = progress;
        }

    }
}
