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

namespace mTiler.Core.Mapping
{
    /// <summary>
    /// Represents a region of a map within an atlas project
    /// </summary>
    class MapRegion
    {
        /// <summary>
        /// The path to this region on disk
        /// </summary>
        private string Path;

        /// <summary>
        /// The atlas this region was originally part of
        /// </summary>
        private Atlas Atlas;

        /// <summary>
        /// The zoom level this region is a part of
        /// </summary>
        private ZoomLevel Zoom;

        /// <summary>
        /// The name of this map region
        /// </summary>
        private string Name;

        /// <summary>
        /// The app controller
        /// </summary>
        private ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// The map tiles within this map region.
        /// </summary>
        private Tile[] MapTiles;

        /// <summary>
        /// Tracks the total number of tiles in this region
        /// </summary>
        public int NTiles = 0;

        /// <summary>
        /// Initializes a map region
        /// </summary>
        /// <param name="path">The path to this map region on disk</param>
        public MapRegion(string path, Atlas atlas, ZoomLevel zoomLevel)
        {
            Path = path;
            Zoom = zoomLevel;
            Atlas = atlas;
            Name = FilesystemHelper.GetPathName(path);

            // Load the tiles in this region
            LoadTiles();
        }

        /// <summary>
        /// Loads the tiles within this map region
        /// </summary>
        private void LoadTiles()
        {
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\t\tLoading tiles for map region: " + Name);

            // Find all of the tiles
            List<string> tilePaths = FilesystemHelper.EnumerateFiles(Path);
            if (tilePaths != null && tilePaths.Count > 0)
            {
                List<Tile> tiles = new List<Tile>();
                foreach (string dir in tilePaths)
                {
                    if (AppController.EnableVerboseLogging)
                        AppController.Logger.Log("\t\t\tFound tile: " + dir);
                    NTiles++;
                    Tile tile = new Tile(dir, Atlas, Zoom, this);
                    tiles.Add(tile);
                }

                if (tiles == null || !(tiles.Count > 0))
                {
                    AppController.Logger.Error("\t\tNo tiles found for region " + Name);
                } else
                {
                    this.MapTiles = tiles.ToArray();
                }
            } else
            {
                AppController.Logger.Error("\t\tNo tiles found for region " + Name);
            }
        }

        /// <summary>
        /// Returns the map tiles within this region
        /// </summary>
        /// <returns></returns>
        public Tile[] GetMapTiles()
        {
            return MapTiles;
        }

        /// <summary>
        /// Returns the name of this map region
        /// </summary>
        /// <returns></returns>
        public string GetName()
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
