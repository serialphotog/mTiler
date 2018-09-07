using mTiler.Core.Util;
using System;
using System.Collections.Generic;

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
        private String Path;

        /// <summary>
        /// The name of this map region
        /// </summary>
        private String Name;

        /// <summary>
        /// Reference to the logging instance
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// The map tiles within this map region.
        /// </summary>
        private MapTile[] MapTiles;

        /// <summary>
        /// Tracks the total number of tiles in this region
        /// </summary>
        public int NTiles = 0;

        /// <summary>
        /// Initializes a map region
        /// </summary>
        /// <param name="path">The path to this map region on disk</param>
        public MapRegion(String path, Logger logger)
        {
            this.Path = path;
            this.Logger = logger;
            this.Name = FS.GetPathName(path);

            // Load the tiles in this region
            LoadTiles();
        }

        /// <summary>
        /// Loads the tiles within this map region
        /// </summary>
        private void LoadTiles()
        {
            Logger.Log("\t\tLoading tiles for map region: " + Name);

            // Find all of the tiles
            String[] tilePaths = FS.EnumerateFiles(Path);
            if (tilePaths != null && tilePaths.Length > 0)
            {
                List<MapTile> tiles = new List<MapTile>();
                foreach (String dir in tilePaths)
                {
                    Logger.Log("\t\t\tFound tile: " + dir);
                    NTiles++;
                    MapTile tile = new MapTile(dir, Logger);
                    tiles.Add(tile);
                }

                if (tiles == null || !(tiles.Count > 0))
                {
                    Logger.Error("\t\tNo tiles found for region " + Name);
                } else
                {
                    this.MapTiles = tiles.ToArray();
                }
            } else
            {
                Logger.Error("\t\tNo tiles found for region " + Name);
            }
        }

        /// <summary>
        /// Returns the map tiles within this region
        /// </summary>
        /// <returns></returns>
        public MapTile[] GetMapTiles()
        {
            return MapTiles;
        }

        /// <summary>
        /// Returns the name of this map region
        /// </summary>
        /// <returns></returns>
        public String GetName()
        {
            return Name;
        }
    }
}
