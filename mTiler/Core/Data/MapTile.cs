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
        /// Initializes a map tile.
        /// </summary>
        /// <param name="path">The path to the tile on disk</param>
        public MapTile(String path)
        {
            this.path = path;
        }
    }
}
