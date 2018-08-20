using mTiler.Core.Util;
using System;
using System.Drawing;
using System.IO;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents an individual map tile
    /// </summary>
    class MapTile
    {
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
            Boolean allWhite = true;
            for (int x=0; x < width; x++) // loop over width
            {
                for (int y=0; y < height; y++) // loop over height
                {
                    Color currentPixel = tileImage.GetPixel(x, y);
                    if (currentPixel.ToArgb() != Color.FromArgb(255, 253, 253, 253).ToArgb())
                    {
                        allWhite = false;
                        break;
                    }
                }

                if (!allWhite)
                    break;
            }

            return allWhite;
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
