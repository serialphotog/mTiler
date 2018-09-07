using mTiler.Core.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents an individual map tile
    /// </summary>
    class MapTile
    {
        /// <summary>
        /// The threshold to use when checking for "white" pixels
        /// </summary>
        private static readonly int WhiteThreshold = 50;

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        private static readonly int LikenessThreshold = 30;

        /// <summary>
        /// The amount of the back pixel color to keep when performing blends
        /// </summary>
        private static readonly double BlendAmount = 0.999;

        /// <summary>
        /// The path to the map tile on disk
        /// </summary>
        private String Path;

        /// <summary>
        /// The name of this tile
        /// </summary>
        private String Name;

        /// <summary>
        /// The logger instance
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// Initializes a map tile.
        /// </summary>
        /// <param name="path">The path to the tile on disk</param>
        public MapTile(String path, Logger logger)
        {
            this.Path = path;
            this.Logger = logger;
            this.Name = FS.GetFilename(path);
        }

        /// <summary>
        /// Gets this tile as a Bitmap image
        /// </summary>
        /// <returns>The bitmap representation of this tile</returns>
        public Bitmap GetBitmap()
        {
            Bitmap result;
            using (Stream bmpStream = File.Open(Path, FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);
                result = new Bitmap(image);
            }
            return result;
        }

        /// <summary>
        /// Determines if this tile is dataless (all white pixels).
        /// </summary>
        /// <returns>True if dataless, else false</returns>
        public Boolean IsDatalessTile()
        {
            Bitmap tileImage = GetBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;

            // Determine if every pixel is white
            for (int x=0; x < width; x++) // loop over width
            {
                for (int y=0; y < height; y++) // loop over height
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (ImageUtil.ColorWithinThresholdOfWhite(currentPixel, WhiteThreshold))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if this is a complete tile (no empty pixels)
        /// </summary>
        /// <returns>True if tile data is complete, else false</returns>
        public Boolean IsComplete()
        {
            Bitmap tileImage = GetBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;

            // Determine if the image lacks all white pixels
            for (int x=0; x < width; x++)
            {
                for (int y=0; y < height; y++)
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (ImageUtil.ColorWithinThresholdOfWhite(currentPixel, WhiteThreshold))
                    {
                        // Found a dataless pixel, tile is not complete
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Merges two map tiles together
        /// </summary>
        /// <param name="tileA">Tile "A" for the merge</param>
        /// <param name="tileB">Tile "B" for the merge</param>
        /// <param name="outputDir">The directory to write the resulting tile image to</param>
        /// <returns>The path to the resulting tile image</returns>
        public static String MergeTiles(MapTile tileA, MapTile tileB, String outputDir)
        {
            Bitmap tileAImage = tileA.GetBitmap();
            Bitmap tileBImage = tileB.GetBitmap();

            int width = tileAImage.Width;
            int height = tileBImage.Height;

            // Create the result bitmap
            PixelFormat pixelFormat = tileAImage.PixelFormat;
            Bitmap resultingTile = new Bitmap(width, height, pixelFormat);

            // Perform the merge
            for (int w=0; w < width; w++)
            {
                for (int h=0; h < height; h++)
                {
                    Color pixelA = tileAImage.GetPixel(w, h);
                    Color pixelB = tileBImage.GetPixel(w, h);

                    if (!ImageUtil.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold) && !ImageUtil.ColorWithinThresholdOfWhite(pixelB, WhiteThreshold))
                    {
                        // Set to the average of the two pixels
                        if (ImageUtil.ColorsAreClose(pixelA, pixelB, LikenessThreshold))
                        {
                            resultingTile.SetPixel(w, h, pixelA);
                        }
                        else
                        {
                            int brightnessA = ImageUtil.GetBrightness(pixelA);
                            int brightnessB = ImageUtil.GetBrightness(pixelB);
                            Color blendedPixel;

                            // Determine which order to mix the pixels in
                            if (brightnessA > brightnessB)
                            {
                                blendedPixel = ImageUtil.Blend(pixelB, pixelA, BlendAmount);
                            }
                            else
                            {
                                blendedPixel = ImageUtil.Blend(pixelA, pixelB, BlendAmount);
                            }
                            resultingTile.SetPixel(w, h, blendedPixel);
                        }
                    }
                    else if (!ImageUtil.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
                    {
                        // This pixel has data, copy it
                        resultingTile.SetPixel(w, h, pixelA);
                    }
                    else
                    {
                        // Pixel b has valid data to copy
                        resultingTile.SetPixel(w, h, pixelB);
                    }
                }
            }

            // Write the bitmap to disk and return the URI
            return FS.WriteBitmapToJpeg(resultingTile, outputDir, tileA.GetName());
        }

        /// <summary>
        /// Returns the name of this tile.
        /// </summary>
        /// <returns></returns>
        public String GetName()
        {
            return Name;
        }
    }
}
