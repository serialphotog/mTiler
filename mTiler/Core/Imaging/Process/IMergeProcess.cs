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
