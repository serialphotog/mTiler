using mTiler.Core.Util;
using System;
using System.Collections.Generic;

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
        private String Name;

        /// <summary>
        /// The path to this atlas
        /// </summary>
        private String Path;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// The zoom levels within this atlas
        /// </summary>
        private ZoomLevel[] ZoomLevels;

        /// <summary>
        /// The total number of tiles in this atlas
        /// </summary>
        public int NTiles = 0;

        /// <summary>
        /// Initializes the atlas
        /// </summary>
        /// <param name="path">The path to the atlas on Disk</param>
        /// <param name="logger">Reference to the logger component</param>
        public Atlas(String path, Logger logger)
        {
            this.Logger = logger;
            this.Path = path;
            this.Name = FS.GetPathName(path);

            // Load the zoom levels
            LoadZoomLevels();
        }

        /// <summary>
        /// Loads all of the zoom levels within this atlas project
        /// </summary>
        private void LoadZoomLevels()
        {
            Logger.Log("Loading zoom levels for atlas: " + Name);

            // Find all of the zoom levels
            String[] zoomPaths = FS.EnumerateDir(Path);
            if (zoomPaths != null && zoomPaths.Length > 0)
            {
                List<ZoomLevel> zooms = new List<ZoomLevel>();
                foreach (String dir in zoomPaths)
                {
                    Logger.Log("\tFound zoom level: " + dir);
                    ZoomLevel zoom = new ZoomLevel(dir, Logger);
                    NTiles += zoom.NTiles;
                    zooms.Add(zoom);
                }

                // Check that we actually found some zoom levels
                if (zooms == null || !(zooms.Count > 0))
                {
                    Logger.Error("No zoom levels found for atlas project: " + Name);
                } else
                {
                    this.ZoomLevels = zooms.ToArray();
                }
            } else
            {
                Logger.Error("No zoom levels found for atlas project: " + Name);
            }
        }

        /// <summary>
        /// Returns the zoom levels within this atlas project
        /// </summary>
        /// <returns>ZoomLevel[] - The zoom levels</returns>
        public ZoomLevel[] GetZoomLevels()
        {
            return ZoomLevels;
        }

        /// <summary>
        /// Returns the name of this atlas.
        /// </summary>
        /// <returns></returns>
        public String GetName()
        {
            return Name;
        }
    }
}
