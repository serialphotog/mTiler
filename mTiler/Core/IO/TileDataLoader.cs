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

using mTiler.Core.Mapping;
using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace mTiler.Core.IO
{
    /// <summary>
    /// A fast, efficient loader for tile data on disk
    /// </summary>
    class TileDataLoader
    {
        /// <summary>
        /// The application controller instance
        /// </summary>
        private ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// The path to load the tile data from
        /// </summary>
        private string Path;

        /// <summary>
        /// Flag that tracks if the user requested a stop
        /// </summary>
        private volatile bool StopRequested = false;

        /// <summary>
        /// Tracks if the data loader is currently running
        /// </summary>
        private bool Running = false;

        /// <summary>
        /// Initializes the tile data loader
        /// </summary>
        /// <param name="path">The input path to load the tiles from</param>
        public TileDataLoader(string path)
        {
            if (!Directory.Exists(path))
            {
                // The input directory doesn't exist, we can't continue
                string msg = String.Format("Invalid input path: {0}", path);
                AppController.Logger.Error(msg);
                throw new InvalidPathException(path);
            } 
            else
            {
                Path = path;
            }
        }

        /// <summary>
        /// Sends the stop signal to the tile loader
        /// </summary>
        public void Stop()
        {
            if (Running)
            {
                AppController.Logger.Log("Stopping the data importer...");
                StopRequested = true;
            }
        }

        /// <summary>
        /// Loads the tiles from disk into a buffer
        /// </summary>
        /// <returns>The tile load buffer</returns>
        public TileLoadBuffer LoadTileData()
        {
            // The storage for the loaded tiles
            TileLoadBuffer tiles = new TileLoadBuffer();
            Running = true;

            List<string> atlases = LoadAtlases();
            if (atlases != null && atlases.Count > 0)
            {
                foreach (string atlas in atlases)
                {
                    if (StopRequested)
                        break;

                    List<string> zoomLevels = LoadZoomLevels(atlas);
                    if (zoomLevels != null && zoomLevels.Count > 0)
                    {
                        foreach (string zoomLevel in zoomLevels)
                        {
                            if (StopRequested)
                                break;

                            List<string> yRegions = LoadYRegions(zoomLevel);
                            if (yRegions != null && yRegions.Count > 0)
                            {
                                foreach (string yRegion in yRegions)
                                {
                                    if (StopRequested)
                                        break;

                                    List<string> tilePaths = LoadTilePaths(yRegion);
                                    if (tilePaths != null && tilePaths.Count > 0)
                                    {
                                        foreach (string tilePath in tilePaths)
                                        {
                                            if (StopRequested)
                                                break;

                                            // Build the tile and add it to the tiles list
                                            string tileAtlas = FS.GetPathName(atlas);
                                            int tileZoomLevel = Int32.Parse(FS.GetPathName(zoomLevel));
                                            int y = Int32.Parse(FS.GetPathName(yRegion));
                                            int x = Int32.Parse(FS.GetFileBasename(tilePath));
                                            Coordinate tileCoord = new Coordinate(x, y);

                                            Tile tile = new Tile(tileAtlas, tileZoomLevel, tileCoord, tilePath);
                                            tiles.Add(tile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Running = false;
            return tiles;
        }

        /// <summary>
        /// Loads the tiles in a y region
        /// </summary>
        /// <param name="yRegion">The y region to load tiles for</param>
        /// <returns>The tiles loaded</returns>
        private List<string> LoadTilePaths(string yRegion)
        {
            return FS.EnumerateFiles(yRegion);
        }

        /// <summary>
        /// Loads the YRegions for a zoom level
        /// </summary>
        /// <param name="zoomLevel">The zoom level to load y regions for</param>
        /// <returns>The list of y regions</returns>
        private List<string> LoadYRegions(string zoomLevel)
        {
            return FS.EnumerateDir(zoomLevel);
        }

        /// <summary>
        /// Loads the zoom levels
        /// </summary>
        /// <param name="atlas">The atlas to load zoom levels for</param>
        /// <returns>The list of zoom levels for the given atlas</returns>
        private List<string> LoadZoomLevels(string atlas)
        {
            return FS.EnumerateDir(atlas);
        }

        /// <summary>
        /// Loads the atlases
        /// </summary>
        /// <returns>The list of laoded atlases</returns>
        private List<string> LoadAtlases()
        {
            List<string> potentialAtlases = FS.EnumerateDir(Path);
            List<string> atlases = new List<string>();

            if (potentialAtlases != null && potentialAtlases.Count > 0)
            {
                foreach (string atlas in potentialAtlases)
                {
                    if (atlas.EndsWith("_atlas"))
                    {
                        atlases.Add(atlas);
                    }
                }
            }

            return atlases;
        }
    }
}
