using System;
using System.Drawing;

namespace mTiler.Core.Imaging
{
    class PixelUtils
    {
        /// <summary>
        /// Determines if a color is within a given threshold of white.
        /// </summary>
        /// <param name="color">The color to check</param>
        /// <param name="threshold">The threshold within white</param>
        /// <returns>True if within white threshold, else false</returns>
        public static bool ColorWithinThresholdOfWhite(Color color, int threshold)
        {
            int r = (int)(255 - color.R);
            int g = (int)(255 - color.G);
            int b = (int)(255 - color.B);

            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        /// <summary>
        /// Determines rather two colors are within a certain threshold of 'likeness'
        /// </summary>
        /// <param name="c1">Color 1 for comparison</param>
        /// <param name="c2">Color 2 for comparison</param>
        /// <param name="threshold">The threshold for likeness</param>
        /// <returns>True if colors are within closness threshold, else false</returns>
        public static bool ColorsAreClose(Color c1, Color c2, int threshold)
        {
            int r = c1.R - c2.R;
            int g = c1.G - c2.G;
            int b = c1.B - c2.B;
            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        /// <summary>
        /// Calculates the brightness value of a color
        /// </summary>
        /// <param name="c">The color to calculate brightness value of</param>
        /// <returns>The brightness of the color</returns>
        public static int GetBrightness(Color c)
        {
            return (int)Math.Sqrt(c.R * c.R * 0.241 + c.G * c.G * 0.691 + c.B * c.B * 0.068);
        }
    }
}
