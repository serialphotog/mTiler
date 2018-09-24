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

using System;

namespace mTiler.Core.Profiling
{
    /// <summary>
    /// Acts as a basic interface between the interal tiling engine and the progress bar in the main form.
    /// </summary>
    class ProgressMonitor
    {
        /// <summary>
        /// The app controller
        /// </summary>
        ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// Tracks the total progress
        /// </summary>
        private int TotalProgress;

        /// <summary>
        /// Resets the progress monitor
        /// </summary>
        public void Reset()
        {
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
            if (!AppController.StopRequested)
            {
                // Don't overflow the progress bar
                if (TotalProgress > AppController.TotalWork)
                    TotalProgress = AppController.TotalWork;

                // Build the string label for the progress
                double progressPercent = ((double)TotalProgress / AppController.TotalWork) * 100;
                string progressText = decimal.Round((decimal)progressPercent, 0, MidpointRounding.AwayFromZero) + "%";

                AppController.MainFormRef.Invoke((Action)delegate
                {
                    AppController.MainFormRef.UpdateProgress(TotalProgress, progressText);
                });
            }
        }
    }
}
