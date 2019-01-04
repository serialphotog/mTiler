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

using mTiler.Core.Imaging.Process;
using System.Drawing;

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
                case "Version One Fast Process":
                    process = new VersionOneFastProcess();
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
            return new string[] { "Version 1 Process", "Version One Fast Process" };
        }
    }
}
