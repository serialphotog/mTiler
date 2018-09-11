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
        /// The white point for determining if tiles are dataless
        /// </summary>
        private static readonly Color WhitePoint = Color.FromArgb(255, 253, 253, 253);

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
        /// The threshold for determining rather a tile is dataless or not
        /// </summary>
        private static readonly int DatalessThreshold = (int)((256 * 256) * 0.01);

        /// <summary>
        /// The path to the map tile on disk
        /// </summary>
        private string Path;

        /// <summary>
        /// The atlas this tile orginally belongs to
        /// </summary>
        private Atlas Atlas;

        /// <summary>
        /// The zoom level of this tile
        /// </summary>
        private ZoomLevel Zoom;

        /// <summary>
        /// The map region this tile is in
        /// </summary>
        private MapRegion Region;

        /// <summary>
        /// The name of this tile
        /// </summary>
        private string Name;

        /// <summary>
        /// The logger instance
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// Cached tile image bitmap
        /// </summary>
        private Bitmap TileImage;

        /// <summary>
        /// Initializes a map tile.
        /// </summary>
        /// <param name="path">The path to the tile on disk</param>
        /// <param name="atlas">The atlas this tile is orginally in</param>
        /// <param name="zoomLevel">The zoom level of this tile</param>
        /// <param name="mapRegion">The map region this tile belongs to</param>
        /// <param name="logger">Reference to the logger</param>
        public MapTile(string path, Atlas atlas, ZoomLevel zoomLevel, MapRegion mapRegion, Logger logger)
        {
            Path = path;
            Zoom = zoomLevel;
            Region = mapRegion;
            Atlas = atlas;
            Logger = logger;
            Name = FS.GetFilename(path);
        }

        /// <summary>
        /// Gets this tile as a Bitmap image
        /// </summary>
        /// <returns>The bitmap representation of this tile</returns>
        public Bitmap GetBitmap()
        {
            if (TileImage == null)
            {
                using (Stream bmpStream = File.Open(Path, FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);
                    TileImage = new Bitmap(image);
                }
            }
            return TileImage;
        }

        /// <summary>
        /// Determines if this tile is dataless (all white pixels).
        /// </summary>
        /// <returns>True if dataless, else false</returns>
        public bool IsDatalessTile()
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
                    if (currentPixel != WhitePoint)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a tile is complete and, if so, how complete it is.
        /// </summary>
        /// <returns>-1 if not complete, else returns a value representing how complete a pixel is</returns>
        public int IsComplete()
        {
            Bitmap tileImage = GetBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;
            int dataPixels = 0;
            int datalessPixels = 0;

            // Count the data-containing pixels
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (datalessPixels >= DatalessThreshold)
                        return -1;

                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (currentPixel != WhitePoint)
                    {
                        dataPixels++;
                    }
                    else
                    {
                        datalessPixels++;
                    }
                }
            }

            return dataPixels;
        }

        /// <summary>
        /// Merges two map tiles together
        /// </summary>
        /// <param name="tileA">Tile "A" for the merge</param>
        /// <param name="tileB">Tile "B" for the merge</param>
        /// <param name="outputDir">The directory to write the resulting tile image to</param>
        /// <returns>The path to the resulting tile image</returns>
        public static string MergeTiles(MapTile tileA, MapTile tileB, string outputDir)
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
        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Gets the zoom level of this tile
        /// </summary>
        /// <returns></returns>
        public ZoomLevel GetZoomLevel()
        {
            return Zoom;
        }

        /// <summary>
        /// Gets the map region this tile is in
        /// </summary>
        /// <returns></returns>
        public MapRegion GetMapRegion()
        {
            return Region;
        }

        /// <summary>
        /// The atlas this tile belongs to
        /// </summary>
        /// <returns></returns>
        public Atlas GetAtlas()
        {
            return Atlas;
        }

        /// <summary>
        /// Gets the path to the tile on disk
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return Path;
        }
    }
}
