using System;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents a region of a map within an atlas project
    /// </summary>
    class MapRegion
    {
        /// <summary>
        /// The map tiles within this map region.
        /// </summary>
        private MapTile[] mapTiles;

        /// <summary>
        /// Initializes a map region
        /// </summary>
        /// <param name="path">The path to this map region on disk</param>
        public MapRegion(String path)
        {

        }
    }
}
