using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents a region of a map within an atlas project
    /// </summary>
    class MapRegion
    {
        /// <summary>
        /// The path to this region on disk
        /// </summary>
        private String path;

        /// <summary>
        /// The name of this map region
        /// </summary>
        private String name;

        /// <summary>
        /// Reference to the logging instance
        /// </summary>
        private Logger logger;

        /// <summary>
        /// The map tiles within this map region.
        /// </summary>
        private MapTile[] mapTiles;

        /// <summary>
        /// Initializes a map region
        /// </summary>
        /// <param name="path">The path to this map region on disk</param>
        public MapRegion(String path, Logger logger)
        {
            this.path = path;
            this.logger = logger;
            this.name = FS.getPathName(path);

            // Load the tiles in this region
            loadTiles();
        }

        /// <summary>
        /// Loads the tiles within this map region
        /// </summary>
        private void loadTiles()
        {
            logger.log("\t\tLoading tiles for map region: " + name);

            // Find all of the tiles
            String[] tilePaths = FS.enumerateFiles(path);
            if (tilePaths != null && tilePaths.Length > 0)
            {
                List<MapTile> tiles = new List<MapTile>();
                foreach (String dir in tilePaths)
                {
                    logger.log("\t\t\tFound tile: " + dir);
                    MapTile tile = new MapTile(dir);
                    tiles.Add(tile);
                }

                if (tiles != null || !(tiles.Count > 0))
                {
                    logger.error("\t\tNo tiles found for region " + name);
                } else
                {
                    this.mapTiles = tiles.ToArray();
                }
            } else
            {
                logger.error("\t\tNo tiles found for region " + name);
            }
        }
    }
}
