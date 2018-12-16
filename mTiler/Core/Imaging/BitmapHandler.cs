using mTiler.Core.Imaging.Process;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace mTiler.Core.Imaging
{
    class BitmapHandler
    {
        /// <summary>
        /// The application controller instance
        /// </summary>
        private static ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// Merges two bitmaps together.
        /// </summary>
        /// <param name="imgA">The first bitmap for the merge</param>
        /// <param name="imgB">The second bitmap for the merge</param>
        /// <returns>The resulting bitmap</returns>
        public static Bitmap MergeBitmaps(Bitmap imgA, Bitmap imgB)
        {
            // Perform the merge using the enabled merge process
            IMergeProcess process;
            switch (AppController.EnabledMergeProcess)
            {
                case "Version 1 Process":
                    process = new VersionOneProcess();
                    break;
                default:
                    // An unknown merge process was encountered
                    throw new InvalidProcessException(AppController.EnabledMergeProcess);
            }
            
            return process.Merge(imgA, imgB);
        }

        /// <summary>
        /// Returns a list of available merge processes. This is used for the settings.
        /// </summary>
        /// <returns>string[] of available process names</returns>
        public static string[] GetAvailableProcesses()
        {
            return new string[] { "Version 1 Process" };
        }
    }
}
