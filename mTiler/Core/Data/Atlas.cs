using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents an atlas
    /// </summary>
    class Atlas
    {
        /// <summary>
        /// The name of the atlas
        /// </summary>
        private String name;

        /// <summary>
        /// The path to this atlas
        /// </summary>
        private String path;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger logger;

        /// <summary>
        /// The zoom levels within this atlas
        /// </summary>
        private ZoomLevel[] zoomLevels;

        /// <summary>
        /// Initializes the atlas
        /// </summary>
        /// <param name="path">The path to the atlas on Disk</param>
        /// <param name="logger">Reference to the logger component</param>
        public Atlas(String path, Logger logger)
        {
            this.logger = logger;
            this.path = path;
            this.name = FS.getPathName(path);

            // Load the zoom levels
            loadZoomLevels();
        }

        /// <summary>
        /// Loads all of the zoom levels within this atlas project
        /// </summary>
        private void loadZoomLevels()
        {
            logger.log("Loading zoom levels for atlas: " + name);

            // Find all of the zoom levels
            String[] zoomPaths = FS.enumerateDir(path);
            if (zoomPaths != null && zoomPaths.Length > 0)
            {
                List<ZoomLevel> zooms = new List<ZoomLevel>();
                foreach (String dir in zoomPaths)
                {
                    logger.log("\tFound zoom level: " + dir);
                    ZoomLevel zoom = new ZoomLevel(dir, logger);
                    zooms.Add(zoom);
                }

                // Check that we actually found some zoom levels
                if (zooms != null || !(zooms.Count > 0))
                {
                    logger.error("No zoom levels found for atlas project: " + name);
                } else
                {
                    this.zoomLevels = zooms.ToArray();
                }
            } else
            {
                logger.error("No zoom levels found for atlas project: " + name);
            }
        }
    }
}
