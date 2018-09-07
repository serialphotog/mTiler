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
        private static readonly int WHITE_THRESHOLD = 50;

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        private static readonly int LIKENESS_THRESHOLD = 30;

        /// <summary>
        /// The path to the map tile on disk
        /// </summary>
        private String path;

        /// <summary>
        /// The name of this tile
        /// </summary>
        private String name;

        /// <summary>
        /// The logger instance
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Initializes a map tile.
        /// </summary>
        /// <param name="path">The path to the tile on disk</param>
        public MapTile(String path, Logger logger)
        {
            this.path = path;
            this.logger = logger;
            this.name = FS.getFilename(path);
        }

        /// <summary>
        /// Gets this tile as a Bitmap image
        /// </summary>
        /// <returns>The bitmap representation of this tile</returns>
        public Bitmap getBitmap()
        {
            Bitmap result;
            using (Stream bmpStream = File.Open(path, FileMode.Open))
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
        public Boolean isDatalessTile()
        {
            Bitmap tileImage = getBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;

            // Determine if every pixel is white
            for (int x=0; x < width; x++) // loop over width
            {
                for (int y=0; y < height; y++) // loop over height
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (ImageUtil.colorWithinThresholdOfWhite(currentPixel, WHITE_THRESHOLD))
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
        public Boolean isComplete()
        {
            Bitmap tileImage = getBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;

            // Determine if the image lacks all white pixels
            for (int x=0; x < width; x++)
            {
                for (int y=0; y < height; y++)
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (ImageUtil.colorWithinThresholdOfWhite(currentPixel, WHITE_THRESHOLD))
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
        public static String mergeTiles(MapTile tileA, MapTile tileB, String outputDir)
        {
            Bitmap tileAImage = tileA.getBitmap();
            Bitmap tileBImage = tileB.getBitmap();

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

                    if (!ImageUtil.colorWithinThresholdOfWhite(pixelA, WHITE_THRESHOLD) && !ImageUtil.colorWithinThresholdOfWhite(pixelB, WHITE_THRESHOLD))
                    {
                        // Set to the average of the two pixels
                        if (ImageUtil.colorsAreClose(pixelA, pixelB, LIKENESS_THRESHOLD))
                        {
                            resultingTile.SetPixel(w, h, pixelA);
                        }
                        else
                        {
                            //resultingTile.SetPixel(w, h, Color.FromArgb(255, 255, 0, 0));
                            resultingTile.SetPixel(w, h, ImageUtil.averageColor(pixelA, pixelB));
                        }
                    }
                    else if (!ImageUtil.colorWithinThresholdOfWhite(pixelA, WHITE_THRESHOLD))
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
            return FS.writeBitmapToJpeg(resultingTile, outputDir, tileA.getName());
        }

        /// <summary>
        /// Returns the name of this tile.
        /// </summary>
        /// <returns></returns>
        public String getName()
        {
            return name;
        }
    }
}
