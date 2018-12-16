using System.Drawing;
using System.Drawing.Imaging;

namespace mTiler.Core.Imaging.Process
{
    class VersionOneProcess : IMergeProcess
    {
        /// <summary>
        /// The threshold to use when checking for "white" pixels
        /// </summary>
        public int WhiteThreshold { get { return 30; } }

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        public int LikenessThreshold { get { return 10; } }

        /// <summary>
        /// The amount of the back pixel color to keep when performing blends
        /// </summary>
        public double BlendAmount { get { return 0.999; } }

        /// <summary>
        /// Provides the interface for merging two bitmaps together.
        /// </summary>
        /// <param name="bitmapA">The first bitmap for the merge</param>
        /// <param name="bitmapB">The second bitmap for the merge</param>
        /// <returns>The resulting bitmap</returns>
        public Bitmap Merge(Bitmap bitmapA, Bitmap bitmapB)
        {
            // Build the storage for the resulting bitmap
            PixelFormat pixelFormat = bitmapA.PixelFormat;
            Bitmap resultingBitmap = new Bitmap(bitmapA.Width, bitmapA.Height, pixelFormat);

            // Perform the merge
            for (int w = 0; w < bitmapA.Width; w++)
            {
                for (int h = 0; h < bitmapA.Height; h++)
                {
                    Color pixelA = bitmapA.GetPixel(w, h);
                    Color pixelB = bitmapB.GetPixel(w, h);

                    if (!PixelUtils.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
                    {
                        // Set to the average of the two pixels
                        if (PixelUtils.ColorsAreClose(pixelA, pixelB, LikenessThreshold))
                        {
                            resultingBitmap.SetPixel(w, h, pixelA);
                        }
                        else
                        {
                            int brightnessA = PixelUtils.GetBrightness(pixelA);
                            int brightnessB = PixelUtils.GetBrightness(pixelB);
                            Color blendedPixel;

                            // Determine which order to blend the pixels in
                            if (brightnessA > brightnessB)
                            {
                                blendedPixel = BlendPixels(pixelB, pixelA, BlendAmount);
                            }
                            else
                            {
                                blendedPixel = BlendPixels(pixelA, pixelB, BlendAmount);
                            }
                            resultingBitmap.SetPixel(w, h, blendedPixel);
                        }
                    }
                    else if (!PixelUtils.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
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
        /// Provides the interface for merging two pixels together.
        /// </summary>
        /// <param name="front">The front color for the merge</param>
        /// <param name="back">The back color for the merge</param>
        /// <param name="amt">The amount of front to blend into back</param>
        /// <returns>The resulting, blended pixel color</returns>
        public Color BlendPixels(Color front, Color back, double amt)
        {
            byte r = (byte)((front.R * amt) + back.R * (1 - amt));
            byte g = (byte)((front.G * amt) + back.G * (1 - amt));
            byte b = (byte)((front.B * amt) + back.B * (1 - amt));

            return Color.FromArgb(r, g, b);
        }
    }
}
