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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace mTiler.Core.Tiling
{
    /// <summary>
    /// The tiling engine. Performs the actual tiling work.
    /// </summary>
    class TilingEngine
    {
        /// <summary>
        /// The input path that stores our atlas directories
        /// </summary>
        private string InputPath;

        /// <summary>
        /// The output path, where we will write the final tiles
        /// </summary>
        private string OutputPath;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger Logger;

        /// <summary>
        /// Reference to the progress monitor instance
        /// </summary>
        private ProgressMonitor Progress;

        /// <summary>
        /// The atlases within the project
        /// </summary>
        private List<Atlas> Atlases;
        
        /// <summary>
        /// Used to request that the tiling thread process be stopped
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Used to time execution.
        /// </summary>
        private Stopwatch Stopwatch;

        /// <summary>
        /// The buffer the tiles are initially loaded into before they are processed.
        /// </summary>
        private List<MapTile> TileLoadBuffer;

        /// <summary>
        /// The merge queue
        /// </summary>
        private Dictionary<string, List<MapTile>> MergeQueue;

        /// <summary>
        /// Initializes the tiling engine
        /// </summary>
        /// <param name="inputPath">The path to the input directory</param>
        /// <param name="outputPath">The path for the output directory</param>
        /// <param name="logger">Reference to the logging component</param>
        /// <param name="form">Reference to the main form (for progress bar updating)</param>
        public TilingEngine(string inputPath, string outputPath, Logger logger, ProgressMonitor progressMonitor)
        {
            Logger = logger;
            InputPath = inputPath;
            OutputPath = outputPath;
            Progress = progressMonitor;
        }

        /// <summary>
        /// Performs the initialization task
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            // Validate the input and output paths
            if (ValidateInputPath(InputPath))
            {
                if (ValidateOutputPath(OutputPath))
                {
                    // Enumerate the atlases and kick off loading all of the data
                    await PerformInitialLoad();
                }
            }
        }

        /// <summary>
        /// Validates the output path. Will attempt to create it if it doesn't exist.
        /// </summary>
        /// <param name="path">The output path</param>
        /// <returns>True on success, else false</returns>
        private bool ValidateOutputPath(string path)
        {
            if (!(path.Length >= 3))
            {
                Logger.Error("Please enter a valid output path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                // The output path doesn't exist, attempt to create it
                Logger.Log("The output path " + path + " does not exist. Attempting to create it...");
                try
                {
                    Directory.CreateDirectory(path);
                    Logger.Log("Successfully created the output directory at " + path);
                    return true;
                } catch (Exception e)
                {
                    Logger.Error("Failed to create output directory at " + path + " . " + e.ToString());
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validates that the input path is valid
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <returns>True if valid, else false</returns>
        private bool ValidateInputPath(string path)
        {
            if (!(path.Length >= 3))
            {
                Logger.Error("Please enter a valid input path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                Logger.Error("The input path " + path + " does not exist!");
                return false;
            }
            return true;
        }

#pragma warning disable 1998
        /// <summary>
        /// Performs the initial data load
        /// </summary>
        /// <returns></returns>
        private async Task PerformInitialLoad()
        {
            Logger.Log("Performing initial data load from " + InputPath);
            LoadAtlases();
            LoadTiles();
        }
#pragma warning restore 1998

        /// <summary>
        /// Loads all of the atlases in the project directory
        /// </summary>
        private void LoadAtlases()
        {
            Logger.Log("Loading atlases...");

            string[] potentialAtlases = FS.EnumerateDir(InputPath);
            Atlases = new List<Atlas>();
            if (potentialAtlases != null && potentialAtlases.Length > 0)
            {
                foreach (string dir in potentialAtlases)
                {
                    // Filter out anything that is not an atlas directory
                    if (dir.EndsWith("_atlas"))
                    {
                        // This is an atlas
                        Logger.Log("\tFound atlas " + dir);
                        Atlas atlas = new Atlas(dir, Logger);
                        Atlases.Add(atlas);
                    }
                }
            }
            else
            {
                Logger.Error("No atlases found in input path " + InputPath);
            }
        }

        /// <summary>
        /// Loads all of the tiles into the tile load buffer
        /// </summary>
        private void LoadTiles()
        {
            TileLoadBuffer = new List<MapTile>();

            // Iterate through each atlas
            foreach (Atlas atlas in Atlases)
            {
                // Iterate through the zoom levels
                foreach (ZoomLevel zoom in atlas.GetZoomLevels())
                {
                    // Iterate through each map region
                    foreach (MapRegion region in zoom.GetMapRegions())
                    {
                        // Iterate over each tile
                        foreach (MapTile tile in region.GetMapTiles())
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
            Logger.Log("Performing the tiling operations...");

            // Tracks the visited tiles
            Dictionary<string, MapTile> visitedTiles = new Dictionary<string, MapTile>();

            // Handle the tiles
            foreach (MapTile currentTile in TileLoadBuffer)
            {
                if (StopRequested)
                {
                    // User requested the tiling operations be canceled
                    return;
                }

                // Load the tile properties
                string currentTileName = currentTile.GetName();
                ZoomLevel currentTileZoom = currentTile.GetZoomLevel();
                MapRegion currentTileRegion = currentTile.GetMapRegion();
                string currentTileRegionId = currentTileZoom.GetName() + currentTileRegion.GetName() + currentTileName;
                int currentTileCompleteness = currentTile.IsComplete();

                // Check if the current tile is more complete than any previous complete versions
                if (currentTileCompleteness > 0 && visitedTiles.ContainsKey(currentTileRegionId))
                {
                    // Load the previous complete tile
                    MapTile previousTile = visitedTiles[currentTileRegionId];
                    int previousTileCompleteness = previousTile.IsComplete();

                    if (currentTileCompleteness > previousTileCompleteness)
                    {
                        // This tile is more complete than the previous one, overwrite it
                        HandleCompleteTile(currentTile);
                    }
                }

                // Don't mess with a tile when we've already found a complete version of it
                if (!visitedTiles.ContainsKey(currentTileRegionId))
                {
                    Logger.Log("Analyzing tile " + currentTileName + " for zoom level " + currentTileZoom.GetName() + " and region " + currentTileRegion.GetName());

                    if (currentTile.IsDatalessTile())
                    {
                        // This tile has no data, ignore it
                        Logger.Log("\tTile " + currentTileName + " has no usable data. Ignoring it.");
                        Progress.Update(1);
                    }
                    else if (currentTileCompleteness > 0)
                    {
                        // This tile is complete. Ignore other non-complete versions and copy to final destination
                        Logger.Log("\tTile " + currentTileName + " is already complte. Copying it to final destination.");

                        // Remove any incomplete version from the process queue, if present
                        if (MergeQueue.ContainsKey(currentTileRegionId))
                        {
                            MergeQueue.Remove(currentTileRegionId);
                        }

                        visitedTiles.Add(currentTileRegionId, currentTile);

                        HandleCompleteTile(currentTile);
                        Progress.Update(1);
                    }
                    else
                    {
                        // Copy the tile to the temporary working directory for merging
                        Logger.Log("\tTile " + currentTileName + " is incomplete. Adding it to the merge queue.");
                        if (MergeQueue.ContainsKey(currentTileRegionId))
                        {
                            List<MapTile> jobTiles = MergeQueue[currentTileRegionId];
                            jobTiles.Add(currentTile);
                            MergeQueue[currentTileRegionId] = jobTiles;
                        }
                        else
                        {
                            List<MapTile> jobTiles = new List<MapTile>
                            {
                                currentTile
                            };
                            MergeQueue.Add(currentTileRegionId, jobTiles);
                        }
                        HandleIncompleteTile(currentTile);
                    }
                }
                else
                {
                    Progress.Update(1);
                }
            }

            // Perform the merge jobs
            Logger.Log("Handling the merge queue...");
            RunMergeQueue();

            // Cleanup
            Logger.Log("Cleaning up the temporary directory");
            Directory.Delete(FS.BuildTempDir(OutputPath), true);
            Stopwatch.Stop();
            var elapsedTime = Stopwatch.ElapsedMilliseconds;
            Logger.Log("Complete! Tiling took " + elapsedTime + "ms");
        }

        /// <summary>
        /// Processes the merge queue
        /// </summary>
        private void RunMergeQueue()
        {
            // Run the queue
            foreach (List<MapTile> mergeJob in MergeQueue.Values)
            {
                if (StopRequested)
                {
                    // User requested the tiling operations be canceled
                    return;
                }

                HandleMergeJob(mergeJob);
            }
        }

        /// <summary>
        /// Handles a single merge job
        /// </summary>
        /// <param name="mergeJob">The merge job to handle</param>
        private void HandleMergeJob(List<MapTile> mergeJob)
        {
            int jobSize = mergeJob.Count;
            if (jobSize > 0)
            {
                string tmpDir = FS.BuildTempDir(OutputPath);
                MapTile currentTile = mergeJob[0];
                    
                if (jobSize > 1)
                {
                    // There are multiple tiles to merge
                    Logger.Log("Handling " + jobSize + " tiles with ID " + currentTile.GetName() + " in zoom level " + currentTile.GetZoomLevel().GetName() + " and region " + currentTile.GetMapRegion().GetName());
                    MapTile nextTile = mergeJob[1];
                    string resultPath = Path.Combine(tmpDir, currentTile.GetZoomLevel().GetName(), currentTile.GetMapRegion().GetName());

                    // Merge the first two tiles
                    string mergeResult = MapTile.MergeTiles(currentTile, nextTile, resultPath);
                    MapTile resultingTile = new MapTile(mergeResult, null, currentTile.GetZoomLevel(), currentTile.GetMapRegion(), Logger);
                    Progress.Update(2);

                    if (jobSize > 2)
                    {
                        // Merge all the remaining tiles together
                        for (int i = 2; i < jobSize; i++)
                        {
                            currentTile = resultingTile;
                            nextTile = mergeJob[i];
                            mergeResult = MapTile.MergeTiles(currentTile, nextTile, resultPath);
                            resultingTile = new MapTile(mergeResult, null, currentTile.GetZoomLevel(), currentTile.GetMapRegion(), Logger);
                            Progress.Update(1);
                        }
                    }

                    // Copy the merge tile to the final location
                    HandleMergedTile(resultingTile);
                }
                else
                {
                    // There are not multiple copies of this tile. Just copy it to final destination
                    Logger.Log("\tThere is only one instance of this tile. Copying it to final destination");
                    HandleIncompleteNonMergedTile(currentTile);
                    Progress.Update(1);
                }
            }
        }

        /// <summary>
        /// Resets the tiling engine to a clean state.
        /// </summary>
        private void Reset()
        {
            Progress.Reset();
            MergeQueue = new Dictionary<string, List<MapTile>>(); // Reset the merge queue
            this.Stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Hanldes a complete tile by copying it to the final destination.
        /// </summary>
        /// <param name="tile">The complete tile to copy</param>
        private void HandleCompleteTile(MapTile tile)
        {
            string copyToDir = FS.BuildOutputDir(OutputPath, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName());
            string copyPath = Path.Combine(copyToDir, tile.GetName());
            File.Copy(tile.GetPath(), copyPath, true);
        }

        /// <summary>
        /// Handles incomplete tiles by copying them to the temporary working directory
        /// </summary>
        /// <param name="tile">The incomplete tile to copy</param>
        private void HandleIncompleteTile(MapTile tile)
        {
            string tmpDir = FS.BuildTempDir(OutputPath);
            string copyTo = FS.BuildTempPath(tmpDir, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName(), tile.GetName(), tile.GetAtlas().GetName());
            File.Copy(tile.GetPath(), copyTo, true);
        }

        /// <summary>
        /// Handles a merged tile by copying it to the final destination.
        /// </summary>
        /// <param name="tile">The merged tile to move</param>
        private void HandleMergedTile(MapTile tile)
        {
            string copyTo = FS.BuildOutputDir(OutputPath, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName());
            string copyPath = Path.Combine(copyTo, tile.GetName());
            File.Copy(tile.GetPath(), copyPath, true);
        }

        /// <summary>
        /// Handles an incomplete, but non-merged, tile by copying it to the final destination
        /// </summary>
        /// <param name="tile">The tile to copy</param>
        private void HandleIncompleteNonMergedTile(MapTile tile)
        {
            string copyTo = FS.BuildOutputDir(OutputPath, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName());
            string copyPath = Path.Combine(copyTo, tile.GetName());
            File.Copy(tile.GetPath(), copyPath, true);
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
