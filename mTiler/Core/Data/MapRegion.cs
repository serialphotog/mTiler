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
    /// Represents a region of a map within an atlas project
    /// </summary>
    class MapRegion
    {
        /// <summary>
        /// The path to this region on disk
        /// </summary>
        private String Path;

        /// <summary>
        /// The zoom level this region is a part of
        /// </summary>
        private ZoomLevel Zoom;

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
        public MapRegion(String path, ZoomLevel zoomLevel, Logger logger)
        {
            Path = path;
            Zoom = zoomLevel;
            Logger = logger;
            Name = FS.GetPathName(path);

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
                    MapTile tile = new MapTile(dir, Zoom, this, Logger);
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

        /// <summary>
        /// Gets the zoom level this map region is a part of
        /// </summary>
        /// <returns></returns>
        public ZoomLevel GetZoomLevel()
        {
            return Zoom;
        }
    }
}
