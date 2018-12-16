using mTiler.Core.Imaging.Process;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace mTiler.Core.Imaging
{
    class BitmapHandler
    {
        /// <summary>
        /// Merges two bitmaps together.
        /// </summary>
        /// <param name="imgA">The first bitmap for the merge</param>
        /// <param name="imgB">The second bitmap for the merge</param>
        /// <returns>The resulting bitmap</returns>
        public static Bitmap MergeBitmaps(Bitmap imgA, Bitmap imgB)
        {
            // TODO: This should be configurable to allow for new processes!
            VersionOneProcess process = new VersionOneProcess();
            return process.Merge(imgA, imgB);
        }

    }
}
