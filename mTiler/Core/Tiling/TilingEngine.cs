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

using mTiler.Core.Data;
using mTiler.Core.Util;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace mTiler.Core.Tiling
{
    /// <summary>
    /// The tiling engine. Performs the actual tiling work.
    /// </summary>
    class TilingEngine
    {
        /// <summary>
        /// The application controller
        /// </summary>
        ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// The atlases within the project
        /// </summary>
        private List<Atlas> Atlases;

        /// <summary>
        /// The buffer the tiles are initially loaded into before they are processed.
        /// </summary>
        private List<MapTile> TileLoadBuffer;

        /// <summary>
        /// List of complete tiles. This is used to separate I/O operations from processing threads.
        /// </summary>
        private ConcurrentDictionary<string, MapTile> CompleteTiles;

        /// <summary>
        /// List of incomplete tiles. This is used to separate I/O operations from processing threads.
        /// </summary>
        private ConcurrentBag<MapTile> IncompleteTiles;

        /// <summary>
        /// Performs the initialization task
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            // Enumerate the atlases and kick off loading all of the data
            await PerformInitialLoad();
        }

#pragma warning disable 1998
        /// <summary>
        /// Performs the initial data load
        /// </summary>
        /// <returns></returns>
        private async Task PerformInitialLoad()
        {
            AppController.Logger.Log("Performing initial data load from " + AppController.InputPath);
            LoadAtlases();
            LoadTiles();
        }
#pragma warning restore 1998

        /// <summary>
        /// Loads all of the atlases in the project directory
        /// </summary>
        private void LoadAtlases()
        {
            AppController.Logger.Log("Loading atlases...");

            List<string> potentialAtlases = FilesystemHelper.EnumerateDir(AppController.InputPath);
            Atlases = new List<Atlas>();
            if (potentialAtlases != null && potentialAtlases.Count > 0)
            {
                foreach (string dir in potentialAtlases)
                {
                    // Filter out anything that is not an atlas directory
                    if (dir.EndsWith("_atlas"))
                    {
                        // This is an atlas
                        if (AppController.EnableVerboseLogging)
                            AppController.Logger.Log("\tFound atlas " + dir);
                        Atlas atlas = new Atlas(dir);
                        Atlases.Add(atlas);
                    }
                }
            }
            else
            {
                AppController.Logger.Error("No atlases found in input path " + AppController.InputPath);
            }
        }

        /// <summary>
        /// Loads all of the tiles into the tile load buffer
        /// </summary>
        private void LoadTiles()
        {
            TileLoadBuffer = new List<MapTile>();

            // Iterate through each atlas
            if (Atlases == null || Atlases.Count <= 0)
            {
                AppController.Logger.Error("There are no atlases in " + AppController.InputPath);
                return;
            }
            foreach (Atlas atlas in Atlases)
            {
                // Iterate through the zoom levels
                ZoomLevel[] zoomLevels = atlas.GetZoomLevels();
                if (zoomLevels == null || zoomLevels.Length <= 0) continue;
                foreach (ZoomLevel zoom in zoomLevels)
                {
                    // Iterate through each map region
                    MapRegion[] mapRegions = zoom.GetMapRegions();
                    if (mapRegions == null || mapRegions.Length <= 0) continue;
                    foreach (MapRegion region in mapRegions)
                    {
                        // Iterate over each tile
                        MapTile[] mapTiles = region.GetMapTiles();
                        if (mapTiles == null || mapTiles.Length <= 0) continue;
                        foreach (MapTile tile in mapTiles)
                        {
                            // Add the tile to the tile load buffer
                            TileLoadBuffer.Add(tile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs the actual tiling operations.
        /// </summary>
        public void Tile()
        {
            // Start from a clean state
            Reset();
            AppController.Logger.Log("Performing the tiling operations...");

            // Handle the tiles in the tile load buffer
            Parallel.ForEach(TileLoadBuffer, new ParallelOptions { MaxDegreeOfParallelism = AppController.MaxTilingThreads }, (currentTile, state) =>
            {
                if (AppController.StopRequested) // User requested tiling thread be stopped
                    state.Break();

                ProcessTile(currentTile);
            });

            TileLoadBuffer.Clear();
            TileLoadBuffer = null;

            if (AppController.StopRequested)
                return;

            HandleTileIO();

            // Clear some memory
            CompleteTiles.Clear();
            CompleteTiles = null;
            IncompleteTiles = null;

            // Perform the merge jobs
            AppController.Logger.Log("Handling the merge queue...");
            AppController.MergeEngine.Run();

            // Cleanup
            Cleanup();
        }

        /// <summary>
        /// Performs the cleanup at the end of the tiling.
        /// </summary>
        private void Cleanup()
        {
            AppController.Logger.Log("Cleaning up the temporary directory");
            Directory.Delete(FilesystemHelper.BuildTempDir(AppController.OutputPath), true);
        }

        /// <summary>
        /// Performs the processing on a single tile.
        /// </summary>
        /// <param name="currentTile">The tile to process</param>
        private void ProcessTile(MapTile currentTile)
        {
            // Don't mess with a tile when we've already found a complete version of it
            if (!CompleteTiles.ContainsKey(currentTile.GetRegionId()))
            {
                if (AppController.EnableVerboseLogging)
                    AppController.Logger.Log("Analyzing tile " + currentTile.GetName() + " for zoom level " + currentTile.GetZoomLevel().GetName() + " and region " + currentTile.GetMapRegion().GetName());

                if (currentTile.IsDatalessTile())
                {
                    ProcessDatalessTile(currentTile);
                }
                else if (currentTile.IsComplete())
                {
                    ProcessCompleteTile(currentTile);
                }
                else
                {
                    ProcessIncompleteTile(currentTile);
                }
            }
            else
            {
                AppController.Progress.Update(1);
            }

            // Memory cleanup
            currentTile.Clean();
            currentTile = null;
        }

        /// <summary>
        /// Performs the processing on a dataless tile
        /// </summary>
        /// <param name="tile">The dataless tile to process</param>
        private void ProcessDatalessTile(MapTile tile)
        {
            // This tile has no data, ignore it
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\tTile " + tile.GetName() + " has no usable data. Ignoring it.");
            AppController.Progress.Update(1);
        }

        /// <summary>
        /// Performs the processing on an incomplete tile
        /// </summary>
        /// <param name="tile">The incomplete tile to process</param>
        private void ProcessIncompleteTile(MapTile tile)
        {
            // Copy the tile to the temporary working directory for merging
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\tTile " + tile.GetName() + " is incomplete. Adding it to the merge queue.");
            if (AppController.MergeEngine.HasJob(tile.GetRegionId()))
            {
                List<MapTile> jobTiles = AppController.MergeEngine.GetJob(tile.GetRegionId());
                jobTiles.Add(tile);
                AppController.MergeEngine.Update(tile.GetRegionId(), jobTiles);
            }
            else
            {
                List<MapTile> jobTiles = new List<MapTile>
                {
                    tile
                };
                AppController.MergeEngine.Update(tile.GetRegionId(), jobTiles);
            }
            IncompleteTiles.Add(tile);
        }

        /// <summary>
        /// Performs the processing on a complete tile
        /// </summary>
        /// <param name="tile">The complete tile to process</param>
        private void ProcessCompleteTile(MapTile tile)
        {
            // This tile is complete. Ignore other non-complete versions and copy to final destination
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\tTile " + tile.GetName() + " is already complete. Copying it to final destination.");

            // Remove any incomplete version from the process queue, if present
            if (AppController.MergeEngine.HasJob(tile.GetRegionId()))
            {
                AppController.Progress.Update(AppController.MergeEngine.GetCountForJob(tile.GetRegionId()));
                AppController.MergeEngine.Remove(tile.GetRegionId());
            }

            CompleteTiles.TryAdd(tile.GetRegionId(), tile);

            AppController.Progress.Update(1);
        }

        /// <summary>
        /// Handles the tile I/O operations
        /// </summary>
        private void HandleTileIO()
        {
            // Perform the I/O operations.
            AppController.Logger.Log("Performing file I/O operations... This may take several minutes.");

            Thread completeTileThread = new Thread(new ThreadStart(() =>
            {
                foreach (MapTile tile in CompleteTiles.Values)
                {
                    if (AppController.StopRequested)
                        break;

                    HandleCompleteTile(tile);
                }
            }));

            Thread incompleteTileThread = new Thread(new ThreadStart(() =>
            {
                foreach (MapTile tile in IncompleteTiles)
                {
                    if (AppController.StopRequested)
                        break;

                    HandleIncompleteTile(tile);
                }
            }));

            completeTileThread.Start();
            incompleteTileThread.Start();
            completeTileThread.Join();
            incompleteTileThread.Join();
        }

        /// <summary>
        /// Resets the tiling engine to a clean state.
        /// </summary>
        private void Reset()
        {
            AppController.Progress.Reset();
            AppController.MergeEngine.Reset();
            CompleteTiles = new ConcurrentDictionary<string, MapTile>();
            IncompleteTiles = new ConcurrentBag<MapTile>();
        }

        /// <summary>
        /// Hanldes a complete tile by copying it to the final destination.
        /// </summary>
        /// <param name="tile">The complete tile to copy</param>
        private void HandleCompleteTile(MapTile tile)
        {
            string copyToDir = FilesystemHelper.BuildOutputDir(AppController.OutputPath, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName());
            string copyPath = Path.Combine(copyToDir, tile.GetName());
            File.Copy(tile.GetPath(), copyPath, true);
        }

        /// <summary>
        /// Handles incomplete tiles by copying them to the temporary working directory
        /// </summary>
        /// <param name="tile">The incomplete tile to copy</param>
        private void HandleIncompleteTile(MapTile tile)
        {
            string tmpDir = FilesystemHelper.BuildTempDir(AppController.OutputPath);
            string copyTo = FilesystemHelper.BuildTempPath(tmpDir, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName(), tile.GetName(), tile.GetAtlas().GetName());
            File.Copy(tile.GetPath(), copyTo, true);
        }

        /// <summary>
        /// Returns the total number of tiles
        /// </summary>
        /// <returns></returns>
        public int GetTotalTiles()
        {
            return TileLoadBuffer.Count;
        }

    }
}
