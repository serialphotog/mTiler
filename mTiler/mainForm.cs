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

using Microsoft.WindowsAPICodePack.Taskbar;
using mTiler.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace mTiler
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// The application controller
        /// </summary>
        private ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// Tracks the total work to be done
        /// </summary>
        private int TotalWork;

        /// <summary>
        /// Delegates to update the progress bar
        /// </summary>
        /// <param name="progress"></param>
        public delegate void UpdateProgressDelegate(int progress, string lblText);
        public UpdateProgressDelegate UpdateProgress;

        /// <summary>
        /// Initializes the main form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Initialize the app controller
            AppController.Initialize(this);

            // Initialize the update progress delegate handler
            UpdateProgress = new UpdateProgressDelegate(updateProgress);

            // Setup some output console properties
            outputConsole.ReadOnly = true;
            outputConsole.WordWrap = false;

            // Add context menu to the log
            ContextMenu logContextMenu = new ContextMenu();
            MenuItem clearMenuItem = new MenuItem("Clear");
            clearMenuItem.Click += ClearLog;
            logContextMenu.MenuItems.Add(clearMenuItem);
            outputConsole.ContextMenu = logContextMenu;

            // Enable double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Clears the log when context menu item is selected   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLog(object sender, EventArgs e)
        {
            AppController.Logger.Clear();
        }

        /// <summary>
        /// Validates the input path
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <returns>True if input path is valid, else false</returns>
        private bool ValidateInputPath(string path)
        {
            if (!(path.Length >= 3))
            {
                AppController.Logger.Error("Please enter a valid input path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                AppController.Logger.Error("The input path " + path + " does not exist!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the output path
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <returns>True if valid, else false</returns>
        private bool ValidateOutputPath(string path)
        {
            if (!(path.Length >= 3))
            {
                AppController.Logger.Error("Please enter a valid output path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                // The output path doesn't exist, attempt to create it
                AppController.Logger.Log("The output path " + path + " does not exist. Attempting to create it...");
                try
                {
                    Directory.CreateDirectory(path);
                    AppController.Logger.Log("Successfully created the output directory at " + path);
                    return true;
                }
                catch (Exception e)
                {
                    AppController.Logger.Error("Failed to create output directory at " + path + " . " + e.ToString());
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Allows the user to select a folder
        /// </summary>
        /// <returns>String - The path to the folder</returns>
        private string OpenFolderPath()
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
            AppController.Stop();
        }

        /// <summary>
        /// Handles the click event for the inputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputPathBtn_Click(object sender, EventArgs e)
        {
            // Get the input path
            string path = OpenFolderPath();
            inputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the outputPath button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void outputPathBtn_Click(object sender, EventArgs e)
        {
            string path = OpenFolderPath();
            outputPathTxt.Text = path;
        }

        /// <summary>
        /// Handles the click event for the start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (ValidateInputPath(inputPathTxt.Text) && ValidateOutputPath(outputPathTxt.Text))
            {
                AppController.Start(inputPathTxt.Text, outputPathTxt.Text);
            }
        }

        /// <summary>
        /// Updates the progress bar
        /// </summary>
        /// <param name="progress"></param>
        private void updateProgress(int progress, string lblTxt)
        {
            // Update the in-app progress bar
            lblProgress.Text = lblTxt;
            progressBar.Value = progress;

            // Update the taskbar progress
            // Updat the progress in the taskbar
            TaskbarManager.Instance.SetProgressValue(progress, TotalWork);

            // Check if we are done
            if (progress >= TotalWork)
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }

        /// <summary>
        /// Returns the output console
        /// </summary>
        /// <returns></returns>
        public RichTextBox GetOutputConsole()
        {
            return this.outputConsole;
        }

        /// <summary>
        /// Sets the maximum bounds for the progress bar
        /// </summary>
        /// <param name="totalWork"></param>
        public void SetTotalWork(int totalWork)
        {
            TotalWork = totalWork;
            progressBar.Maximum = totalWork;
            progressBar.Step = 1;
            progressBar.Value = 0;
            lblProgress.Text = "0%";
        }

        /// <summary>
        /// Handles the click event for the exit menu item (File->Exit)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileMenuItemExit_Click(object sender, EventArgs e)
        {
            AppController.Stop();
            Application.Exit();
        }

        /// <summary>
        /// Handles the click event for the Tiling->Cancel menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tilingMenuItemCancel_Click(object sender, EventArgs e)
        {
            AppController.Stop();
        }

        /// <summary>
        /// Shows the settings dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileMenuItemSettings_Click(object sender, EventArgs e)
        {
            SettingsDialog settingsDialog = new SettingsDialog();
            settingsDialog.ShowDialog(this);
        }
    }
}
