/*
Copyright 2018 Adam Thompson

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using mTiler.Core.Util;
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
        private string Name;

        /// <summary>
        /// The path to this atlas
        /// </summary>
        private string Path;

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
        public Atlas(string path, Logger logger)
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
            string[] zoomPaths = FS.EnumerateDir(Path);
            if (zoomPaths != null && zoomPaths.Length > 0)
            {
                List<ZoomLevel> zooms = new List<ZoomLevel>();
                foreach (string dir in zoomPaths)
                {
                    Logger.Log("\tFound zoom level: " + dir);
                    ZoomLevel zoom = new ZoomLevel(dir, this, Logger);
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
        public string GetName()
        {
            return Name;
        }
    }
}
