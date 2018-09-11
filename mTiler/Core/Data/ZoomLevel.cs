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
        private string Name;

        /// <summary>
        /// The atlas this zoom level originall belongs to
        /// </summary>
        private Atlas Atlas;

        /// <summary>
        /// The path to this zoom level on disk
        /// </summary>
        private string Path;

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
        public ZoomLevel(string path, Atlas atlas, Logger logger)
        {
            Path = path;
            Logger = logger;
            Name = FS.GetPathName(path);
            Atlas = atlas;

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
            string[] regionPaths = FS.EnumerateDir(Path);
            if (regionPaths != null && regionPaths.Length > 0)
            {
                List<MapRegion> regions = new List<MapRegion>();
                foreach (string dir in regionPaths)
                {
                    Logger.Log("\t\tFound map region: " + dir);
                    MapRegion region = new MapRegion(dir, Atlas, this, Logger);
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
        public string GetName()
        {
            return Name;
        }
    }
}
