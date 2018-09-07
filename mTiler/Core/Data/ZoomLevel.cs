using mTiler.Core.Util;
using System;
using System.Collections.Generic;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents an individual zoom level within an atlas project.
    /// </summary>
    class ZoomLevel
    {
        /// <summary>
        /// The name of this zoom level
        /// </summary>
        private String Name;

        /// <summary>
        /// The path to this zoom level on disk
        /// </summary>
        private String Path;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// The map regions within this zoom level
        /// </summary>
        private MapRegion[] MapRegions;

        /// <summary>
        /// The total number of tiles in this zoom level
        /// </summary>
        public int NTiles = 0;

        /// <summary>
        /// Initializes this zoom level
        /// </summary>
        /// <param name="path">The path to this zoom level on disk</param>
        public ZoomLevel(String path, Logger logger)
        {
            this.Path = path;
            this.Logger = logger;
            this.Name = FS.GetPathName(path);

            // Load the map regions within this zoom level
            LoadRegions();
        }

        /// <summary>
        /// Loads the map regions contained within this zoom level
        /// </summary>
        private void LoadRegions()
        {
            Logger.Log("\tLoading map regions for zoom level: " + Name);

            // Find all of the map regions
            String[] regionPaths = FS.EnumerateDir(Path);
            if (regionPaths != null && regionPaths.Length > 0)
            {
                List<MapRegion> regions = new List<MapRegion>();
                foreach (String dir in regionPaths)
                {
                    Logger.Log("\t\tFound map region: " + dir);
                    MapRegion region = new MapRegion(dir, Logger);
                    NTiles += region.NTiles;
                    regions.Add(region);
                }

                // Check that we actually found some regions
                if (regions == null || !(regions.Count > 0))
                {
                    Logger.Error("\tNo map regions found for zoom level: " + Name);
                } else
                {
                    this.MapRegions = regions.ToArray();
                }
            } else
            {
                Logger.Error("\tNo map regions found for zoom level: " + Name);
            }
        }

        /// <summary>
        /// Gets the map regions within this zoom level
        /// </summary>
        /// <returns></returns>
        public MapRegion[] GetMapRegions()
        {
            return MapRegions;
        }

        /// <summary>
        /// Returns the name of this zoom level.
        /// </summary>
        /// <returns></returns>
        public String GetName()
        {
            return Name;
        }
    }
}
