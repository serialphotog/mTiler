using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace mTiler.Core.Imaging
{
    class BitmapHandler
    {
        /// <summary>
        /// The threshold to use when checking for "white" pixels
        /// </summary>
        private static readonly int WhiteThreshold = 30;

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        private static readonly int LikenessThreshold = 10;

        /// <summary>
        /// The amount of the back pixel color to keep when performing blends
        /// </summary>
        private static readonly double BlendAmount = 0.999;

        /// <summary>
        /// Merges two bitmaps together.
        /// </summary>
        /// <param name="imgA">The first bitmap for the merge</param>
        /// <param name="imgB">The second bitmap for the merge</param>
        /// <returns>The resulting bitmap</returns>
        public static Bitmap MergeBitmaps(Bitmap imgA, Bitmap imgB)
        {
            // Build the storage for the resulting bitmap
            PixelFormat pixelFormat = imgA.PixelFormat;
            Bitmap resultingBitmap = new Bitmap(imgA.Width, imgA.Height, pixelFormat);

            // Perform the merge
            for (int w = 0; w < imgA.Width; w++)
            {
                for (int h = 0; h < imgA.Height; h++)
                {
                    Color pixelA = imgA.GetPixel(w, h);
                    Color pixelB = imgB.GetPixel(w, h);

                    if (!ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
                    {
                        // Set to the average of the two pixels
                        if (ColorsAreClose(pixelA, pixelB, LikenessThreshold))
                        {
                            resultingBitmap.SetPixel(w, h, pixelA);
                        } 
                        else
                        {
                            int brightnessA = GetBrightness(pixelA);
                            int brightnessB = GetBrightness(pixelB);
                            Color blendedPixel;

                            // Determine which order to blend the pixels in
                            if (brightnessA > brightnessB)
                            {
                                blendedPixel = Blend(pixelB, pixelA, BlendAmount);
                            }
                            else
                            {
                                blendedPixel = Blend(pixelA, pixelB, BlendAmount);
                            }
                            resultingBitmap.SetPixel(w, h, blendedPixel);
                        }
                    }
                    else if (!ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
                    {
                        // PixelA has valid data, copy it
                        resultingBitmap.SetPixel(w, h, pixelA);
                    }
                    else
                    {
                        // Just set this pixel to the value of pixelB
                        resultingBitmap.SetPixel(w, h, pixelB);
                    }
                }
            }

            return resultingBitmap;
        }

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
