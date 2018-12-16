using System.Drawing;

namespace mTiler.Core.Imaging.Process
{
    interface IMergeProcess
    {
        /// <summary>
        /// The threshold to use when checking for "white" pixels
        /// </summary>
        int WhiteThreshold { get; }

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        int LikenessThreshold { get; }

        /// <summary>
        /// The amount of the back pixel color to keep when performing blends
        /// </summary>
        double BlendAmount { get; }

        /// <summary>
        /// Provides the interface for merging two bitmaps together.
        /// </summary>
        /// <param name="bitmapA">The first bitmap for the merge</param>
        /// <param name="bitmapB">The second bitmap for the merge</param>
        /// <returns>The resulting bitmap</returns>
        Bitmap Merge(Bitmap bitmapA, Bitmap bitmapB);

        /// <summary>
        /// Provides the interface for merging two pixels together.
        /// </summary>
        /// <param name="front">The front color for the merge</param>
        /// <param name="back">The back color for the merge</param>
        /// <param name="amt">The amount of front to blend into back</param>
        /// <returns>The resulting, blended pixel color</returns>
        Color BlendPixels(Color front, Color back, double amt);
    }
}
