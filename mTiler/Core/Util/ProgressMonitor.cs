using System;

namespace mTiler.Core.Util
{
    /// <summary>
    /// Acts as a basic interface between the interal tiling engine and the progress bar in the main form.
    /// </summary>
    class ProgressMonitor
    {

        /// <summary>
        /// Tracks the total progress
        /// </summary>
        private int TotalProgress;

        /// <summary>
        /// Reference to the form
        /// </summary>
        private MainForm FormRef;

        /// <summary>
        /// Used to request that the tiling thread process be stopped
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Initializes the progress montitor
        /// </summary>
        /// <param name="form">Reference to the main form</param>
        public ProgressMonitor(MainForm form)
        {
            FormRef = form;
        }

        /// <summary>
        /// Resets the progress monitor
        /// </summary>
        public void Reset()
        {
            StopRequested = false;
            TotalProgress = 0;
            UpdateProgress();
        }

        /// <summary>
        /// Updates the progress
        /// </summary>
        /// <param name="amt">The amount to update</param>
        public void Update(int amt)
        {
            TotalProgress += amt;
            UpdateProgress();
        }

        /// <summary>
        /// Updates the progress bar in the main GUI
        /// </summary>
        private void UpdateProgress()
        {
            if (!StopRequested)
            {
                FormRef.Invoke((Action)delegate
                {
                    FormRef.UpdateProgress(TotalProgress);
                });
            }
        }
    }
}
