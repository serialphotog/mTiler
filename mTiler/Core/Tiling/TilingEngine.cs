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

using mTiler.Core.IO;
using mTiler.Core.Mapping;
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
        /// The buffer the tiles are initially loaded into before they are processed.
        /// </summary>
        private TileLoadBuffer TileLoadBuffer;

        /// <summary>
        /// List of complete tiles. This is used to separate I/O operations from processing threads.
        /// </summary>
        private ConcurrentDictionary<string, Tile> CompleteTiles;

        /// <summary>
        /// List of incomplete tiles. This is used to separate I/O operations from processing threads.
        /// </summary>
        private ConcurrentBag<Tile> IncompleteTiles;

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
            TileDataLoader loader = new TileDataLoader(AppController.InputPath);
            TileLoadBuffer = loader.LoadTileData();
        }
#pragma warning restore 1998

        /// <summary>
        /// Performs the actual tiling operations.
        /// </summary>
        public void Tile()
        {
            // Start from a clean state
            Reset();
            AppController.Logger.Log("Performing the tiling operations...");

            // Handle the tiles in the tile load buffer
            Parallel.ForEach(TileLoadBuffer.Buffer, new ParallelOptions { MaxDegreeOfParallelism = AppController.MaxTilingThreads }, (values, state) =>
            {
                if (AppController.StopRequested) // User requested tiling thread be stopped
                    state.Break();

                List<Tile> tileStore = values.Value;
                if (tileStore.Count > 0)
                {
                    foreach (Tile currentTile in tileStore)
                    {
                        ProcessTile(currentTile);
                    }
                }
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
            Directory.Delete(FS.BuildTempDir(AppController.OutputPath), true);
        }

        /// <summary>
        /// Performs the processing on a single tile.
        /// </summary>
        /// <param name="currentTile">The tile to process</param>
        private void ProcessTile(Tile currentTile)
        {
            // Don't mess with a tile when we've already found a complete version of it
            if (!CompleteTiles.ContainsKey(currentTile.RegionID))
            {
                if (AppController.EnableVerboseLogging)
                    AppController.Logger.Log("Analyzing tile " + currentTile.GetName() + " for zoom level " + currentTile.ZoomLevel.ToString() + " and region " + currentTile.Coords.Y.ToString());

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
        private void ProcessDatalessTile(Tile tile)
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
        private void ProcessIncompleteTile(Tile tile)
        {
            // Copy the tile to the temporary working directory for merging
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\tTile " + tile.GetName() + " is incomplete. Adding it to the merge queue.");
            if (AppController.MergeEngine.HasJob(tile.RegionID))
            {
                List<Tile> jobTiles = AppController.MergeEngine.GetJob(tile.RegionID);
                jobTiles.Add(tile);
                AppController.MergeEngine.Update(tile.RegionID, jobTiles);
            }
            else
            {
                List<Tile> jobTiles = new List<Tile>
                {
                    tile
                };
                AppController.MergeEngine.Update(tile.RegionID, jobTiles);
            }
            IncompleteTiles.Add(tile);
        }

        /// <summary>
        /// Performs the processing on a complete tile
        /// </summary>
        /// <param name="tile">The complete tile to process</param>
        private void ProcessCompleteTile(Tile tile)
        {
            // This tile is complete. Ignore other non-complete versions and copy to final destination
            if (AppController.EnableVerboseLogging)
                AppController.Logger.Log("\tTile " + tile.GetName() + " is already complete. Copying it to final destination.");

            // Remove any incomplete version from the process queue, if present
            if (AppController.MergeEngine.HasJob(tile.RegionID))
            {
                AppController.Progress.Update(AppController.MergeEngine.GetCountForJob(tile.RegionID));
                AppController.MergeEngine.Remove(tile.RegionID);
            }

            CompleteTiles.TryAdd(tile.RegionID, tile);

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
                foreach (Tile tile in CompleteTiles.Values)
                {
                    if (AppController.StopRequested)
                        break;

                    HandleCompleteTile(tile);
                }
            }));

            Thread incompleteTileThread = new Thread(new ThreadStart(() =>
            {
                foreach (Tile tile in IncompleteTiles)
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
            CompleteTiles = new ConcurrentDictionary<string, Tile>();
            IncompleteTiles = new ConcurrentBag<Tile>();
        }

        /// <summary>
        /// Hanldes a complete tile by copying it to the final destination.
        /// </summary>
        /// <param name="tile">The complete tile to copy</param>
        private void HandleCompleteTile(Tile tile)
        {
            string copyToDir = FS.BuildOutputDir(AppController.OutputPath, tile.ZoomLevel.ToString(), tile.Coords.Y.ToString());
            string copyPath = Path.Combine(copyToDir, tile.GetName());
            File.Copy(tile.Path, copyPath, true);
        }

        /// <summary>
        /// Handles incomplete tiles by copying them to the temporary working directory
        /// </summary>
        /// <param name="tile">The incomplete tile to copy</param>
        private void HandleIncompleteTile(Tile tile)
        {
            string tmpDir = FS.BuildTempDir(AppController.OutputPath);
            string copyTo = FS.BuildTempPath(tmpDir, tile.ZoomLevel.ToString(), tile.Coords.Y.ToString(), tile.GetName(), tile.Atlas);
            File.Copy(tile.Path, copyTo, true);
        }

        /// <summary>
        /// Returns the total number of tiles
        /// </summary>
        /// <returns></returns>
        public int GetTotalTiles()
        {
            return TileLoadBuffer.GetCount();
        }

    }
}
