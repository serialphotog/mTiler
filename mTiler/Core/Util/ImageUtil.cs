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
        public static Boolean ColorWithinThresholdOfWhite(Color color, int threshold)
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
        public static Color AverageColor(Color c1, Color c2)
        {
            byte r = (byte)Math.Sqrt((c1.R * c1.R + c2.R * c2.R) / 2);
            byte g = (byte)Math.Sqrt((c1.G * c1.G + c2.G * c2.G) / 2);
            byte b = (byte)Math.Sqrt((c1.B * c1.B + c2.B * c2.B) / 2);
            return Color.FromArgb(r, g, b);
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
        /// Blends two colors together.
        /// </summary>
        /// <param name="front">The front color for the blend</param>
        /// <param name="back">The back color</param>
        /// <param name="amt">The amount of the color to keep</param>
        /// <returns>The blended color</returns>
        public static Color Blend(Color front, Color back, double amt)
        {
            byte r = (byte)((front.R * amt) + back.R * (1 - amt));
            byte g = (byte)((front.G * amt) + back.G * (1 - amt));
            byte b = (byte)((front.B * amt) + back.B * (1 - amt));

            return Color.FromArgb(r, g, b);
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
