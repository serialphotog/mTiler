﻿using System;
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

        /// <summary>
        /// Averages two colors
        /// </summary>
        /// <param name="c1">Color 1 for the average</param>
        /// <param name="c2">Color 2 for the average</param>
        /// <returns>The resulting, averaged color</returns>
        public static Color averageColor(Color c1, Color c2)
        {
            byte r = (byte)Math.Sqrt((c1.R * c1.R + c2.R * c2.R) / 2);
            byte g = (byte)Math.Sqrt((c1.G * c1.G + c2.G * c2.G) / 2);
            byte b = (byte)Math.Sqrt((c1.B * c1.B + c2.B * c2.B) / 2);
            return Color.FromArgb(r, g, b);
        }

    }
}
