using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

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
        private String name;

        /// <summary>
        /// The path to this zoom level on disk
        /// </summary>
        private String path;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger logger;

        /// <summary>
        /// The map regions within this zoom level
        /// </summary>
        private MapRegion[] mapRegions;

        /// <summary>
        /// Initializes this zoom level
        /// </summary>
        /// <param name="path">The path to this zoom level on disk</param>
        public ZoomLevel(String path, Logger logger)
        {
            this.path = path;
            this.logger = logger;
            this.name = getZoomName();

            // Load the map regions within this zoom level
            loadRegions();
        }

        /// <summary>
        /// Loads the map regions contained within this zoom level
        /// </summary>
        private void loadRegions()
        {
            logger.log("\tLoading map regions for zoom level: " + name);

            // Find all of the map regions
            String[] regionPaths = Filesystem.enumerateDir(path);
            if (regionPaths != null && regionPaths.Length > 0)
            {
                List<MapRegion> regions = new List<MapRegion>();
                foreach (String dir in regionPaths)
                {
                    logger.log("\t\tFound map region: " + dir);
                    MapRegion region = new MapRegion(dir, logger);
                    regions.Add(region);
                }

                // Check that we actually found some regions
                if (regions != null && !(regions.Count > 0))
                {
                    logger.error("\tNo map regions found for zoom level: " + name);
                } else
                {
                    this.mapRegions = regions.ToArray();
                }
            } else
            {
                logger.error("\tNo map regions found for zoom level: " + name);
            }
        }

        /// <summary>
        /// Returns the name of this zoom level
        /// </summary>
        /// <returns></returns>
        private String getZoomName()
        {
            return new DirectoryInfo(path).Name;
        }
    }
}
