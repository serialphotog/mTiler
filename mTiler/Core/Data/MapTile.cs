using mTiler.Core.Util;
using System;

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
    }
}
