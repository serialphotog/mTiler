using System;
using System.Drawing;

namespace mTiler.Core.Util
{
    /// <summary>
    /// Various image handling utils.
    /// </summary>
    class ImageUtil
    {

        /// <summary>
        /// Determines if a color is within a given threshold of white.
        /// </summary>
        /// <param name="color">The color to check</param>
        /// <param name="threshold">The threshold within white</param>
        /// <returns>True if within white threshold, else false</returns>
        public static Boolean colorWithinThresholdOfWhite(Color color, int threshold)
        {
            int r = (int)(255 - color.R);
            int g = (int)(255 - color.G);
            int b = (int)(255 - color.B);

            return (r * r + g * g + b * b) <= threshold * threshold;
        }

    }
}
