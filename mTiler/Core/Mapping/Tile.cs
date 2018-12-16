using mTiler.Core.Imaging;
using mTiler.Core.Util;
using System;
using System.Drawing;
using System.IO;

namespace mTiler.Core.Mapping
{
    /// <summary>
    /// Represents a map tile
    /// </summary>
    class Tile
    {
        /// <summary>
        /// The white point for determining if tiles are dataless
        /// </summary>
        private static readonly Color WhitePoint = Color.FromArgb(255, 253, 253, 253);

        /// <summary>
        /// The application controller instance
        /// </summary>
        private ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// The atlas this tile orginally belonged to
        /// </summary>
        public string Atlas { get; set; }

        /// <summary>
        /// The zoom level of this tile
        /// </summary>
        public int ZoomLevel { get; set; }

        /// <summary>
        /// The x,y coords of this tile
        /// </summary>
        public Coordinate Coords { get; set; }

        /// <summary>
        /// The region id for the tile
        /// </summary>
        public string RegionID { get; set; }

        /// <summary>
        /// The path to the tile on disk
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The name of this tile
        /// </summary>
        private string Name;

        /// <summary>
        /// Cached tile image bitmap
        /// </summary>
        private Bitmap TileImage;

        /// <summary>
        /// Initializes a tile
        /// </summary>
        /// <param name="atlas">The atlas the tile belongs to</param>
        /// <param name="zoomLevel">The zoom level of the tile</param>
        /// <param name="coords">The x,y coords of the tile</param>
        /// <param name="path">The path to the tile on disk</param>
        public Tile(string atlas, int zoomLevel, Coordinate coords, string path)
        {
            Atlas = atlas;
            ZoomLevel = zoomLevel;
            Coords = coords;
            Path = path;

            GenerateRegionId();
        }

        /// <summary>
        /// Generates the region ID of this tile
        /// </summary>
        private void GenerateRegionId()
        {
            RegionID = string.Format("{0}{1}{2}", ZoomLevel, Coords.Y, Coords.X);
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
        /// Cleans up the memory used by this tile
        /// </summary>
        public void Clean()
        {
            if (TileImage != null)
            {
                TileImage.Dispose();
                TileImage = null;
                GC.Collect();
            }
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
            for (int x = 0; x < width; x++) // loop over width
            {
                for (int y = 0; y < height; y++) // loop over height
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
        /// Determines if a tile is complete.
        /// </summary>
        /// <returns>True if complete, else false</returns>
        public bool IsComplete()
        {
            Bitmap tileImage = GetBitmap();
            int width = tileImage.Width;
            int height = tileImage.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (PixelUtils.ColorWithinThresholdOfWhite(currentPixel, 5))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the name of this tile.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            if (String.IsNullOrEmpty(Name))
            {
                // Fix issues with some tiles having a .bcnav.png extension
                if (System.IO.Path.GetExtension(Path) == ".bcnav")
                {
                    AppController.Logger.Warn("Fixing incorrect BCNAV file extension issue");

                    string originalPath = Path;
                    Path = System.IO.Path.ChangeExtension(Path, "");
                    if (System.IO.Path.GetExtension(Path) == ".png")
                    {
                        Path = System.IO.Path.ChangeExtension(Path, ".jpg");
                    }

                    File.Move(originalPath, Path);
                }

                Name = FS.GetFilename(Path);
            }

            return Name;
        }
    }
}
