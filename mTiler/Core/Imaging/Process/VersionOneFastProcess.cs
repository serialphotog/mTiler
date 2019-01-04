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
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace mTiler.Core.Imaging.Process
{
    class VersionOneFastProcess : IMergeProcess
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
            //ApplicationController.Instance.Logger.Log("Using fast");
            // Build the storage for the resulting bitmap
            PixelFormat pixelFormat = bitmapA.PixelFormat;
            Bitmap resultingBitmap = new Bitmap(bitmapA.Width, bitmapA.Height, pixelFormat);

            int width = bitmapA.Width;
            int height = bitmapA.Height;

            unsafe
            {
                BitmapData bitmapAData = bitmapA.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
                BitmapData bitmapBData = bitmapB.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
                BitmapData resultingBitmapData = resultingBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                int heightInPixels = bitmapAData.Height;
                int widthInBytes = bitmapAData.Width * bytesPerPixel;
                byte* firstPixelA = (byte*)bitmapAData.Scan0;
                byte* firstPixelB = (byte*)bitmapBData.Scan0;
                byte* firstPixelRes = (byte*)resultingBitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLineA = firstPixelA + (y * bitmapAData.Stride);
                    byte* currentLineB = firstPixelB + (y * bitmapBData.Stride);
                    byte* currentLineRes = firstPixelRes + (y * resultingBitmapData.Stride);

                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        // Load the RGB values for the bitmaps
                        int rA = currentLineA[x];
                        int rB = currentLineB[x];
                        int gA = currentLineA[x + 1];
                        int gB = currentLineB[x + 1];
                        int bA = currentLineA[x + 2];
                        int bB = currentLineB[x + 2];

                        // Translate to Microsoft classes
                        // TODO: It would probably be faster to cut out the middle-man here...
                        Color currentPixelA = Color.FromArgb(rA, gA, bA);
                        Color currentPixelB = Color.FromArgb(rB, gB, bB);

                        if (!PixelUtils.ColorWithinThresholdOfWhite(currentPixelA, WhiteThreshold))
                        {
                            if (PixelUtils.ColorsAreClose(currentPixelA, currentPixelB, LikenessThreshold))
                            {
                                // These colors are quite similar, just copy out the pixel from the first bitmap
                                currentLineRes[x] = (byte)rA;
                                currentLineRes[x + 1] = (byte)gA;
                                currentLineRes[x + 2] = (byte)bA;
                            }
                            else
                            {
                                // Blend the pixels together
                                int brightnessA = PixelUtils.GetBrightness(currentPixelA);
                                int brightnessB = PixelUtils.GetBrightness(currentPixelB);
                                Color blendedPixel;

                                // Determine which order to blend the pixels in
                                if (brightnessA > brightnessB)
                                {
                                    blendedPixel = BlendPixels(currentPixelB, currentPixelA, BlendAmount);
                                }
                                else
                                {
                                    blendedPixel = BlendPixels(currentPixelA, currentPixelB, BlendAmount);
                                }

                                currentLineRes[x] = blendedPixel.R;
                                currentLineRes[x + 1] = blendedPixel.G;
                                currentLineRes[x + 2] = blendedPixel.B;
                            }
                        }
                        else if (!PixelUtils.ColorWithinThresholdOfWhite(currentPixelA, WhiteThreshold))
                        {
                            // Just default to the pixel form bitmap a
                            currentLineRes[x] = (byte)rA;
                            currentLineRes[x + 1] = (byte)gA;
                            currentLineRes[x + 2] = (byte)bA;
                        }
                        else
                        {
                            // Fall back to the pixel from bitmap b
                            currentLineRes[x] = (byte)rB;
                            currentLineRes[x + 1] = (byte)gB;
                            currentLineRes[x + 2] = (byte)bB;
                        }
                    }
                });
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
