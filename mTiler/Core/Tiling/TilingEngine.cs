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
        private String InputPath;

        /// <summary>
        /// The output path, where we will write the final tiles
        /// </summary>
        private String OutputPath;

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
        /// The total number of loaded tiles
        /// </summary>
        private int NTiles = 0;
        
        /// <summary>
        /// Used to request that the tiling thread process be stopped
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Used to store tiles that need to be processed
        /// </summary>
        private List<String> ProcessQueue = new List<string>();

        /// <summary>
        /// Used to time execution.
        /// </summary>
        private Stopwatch Stopwatch;

        /// <summary>
        /// The buffer the tiles are initially loaded into before they are processed.
        /// </summary>
        private List<MapTile> TileLoadBuffer;

        /// <summary>
        /// Initializes the tiling engine
        /// </summary>
        /// <param name="inputPath">The path to the input directory</param>
        /// <param name="outputPath">The path for the output directory</param>
        /// <param name="logger">Reference to the logging component</param>
        /// <param name="form">Reference to the main form (for progress bar updating)</param>
        public TilingEngine(String inputPath, String outputPath, Logger logger, ProgressMonitor progressMonitor)
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
        private Boolean ValidateOutputPath(String path)
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
        private Boolean ValidateInputPath(String path)
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

            String[] potentialAtlases = FS.EnumerateDir(InputPath);
            Atlases = new List<Atlas>();
            if (potentialAtlases != null && potentialAtlases.Length > 0)
            {
                foreach (String dir in potentialAtlases)
                {
                    // Filter out anything that is not an atlas directory
                    if (dir.EndsWith("_atlas"))
                    {
                        // This is an atlas
                        Logger.Log("\tFound atlas " + dir);
                        Atlas atlas = new Atlas(dir, Logger);
                        NTiles += atlas.NTiles; // TODO: This will no longer be needed with the new architecture
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
            Dictionary<String, MapTile> visitedTiles = new Dictionary<string, MapTile>();

            // Handle the tiles
            foreach (MapTile currentTile in TileLoadBuffer)
            {
                if (StopRequested)
                {
                    // User requested the tiling operations be canceled
                    return;
                }

                // Load the tile properties
                String currentTileName = currentTile.GetName();
                ZoomLevel currentTileZoom = currentTile.GetZoomLevel();
                MapRegion currentTileRegion = currentTile.GetMapRegion();
                String currentTileRegionId = currentTileZoom.GetName() + currentTileRegion.GetName() + currentTileName;
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
                        if (ProcessQueue.Contains(currentTileRegionId))
                        {
                            ProcessQueue.RemoveAll(s => s == currentTileRegionId);
                        }

                        visitedTiles.Add(currentTileRegionId, currentTile);

                        HandleCompleteTile(currentTile);
                        Progress.Update(1);
                    }
                    else
                    {
                        // Copy the tile to the temporary working directory for merging
                        Logger.Log("\tTile " + currentTileName + " is incomplete. Adding it to the merge queue.");
                        ProcessQueue.Add(currentTileRegionId);
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
            ProcessTempTiles();

            // Cleanup
            Logger.Log("Cleaning up the temporary directory");
            Directory.Delete(FS.BuildTempDir(OutputPath), true);
            Stopwatch.Stop();
            var elapsedTime = Stopwatch.ElapsedMilliseconds;
            Logger.Log("Complete! Tiling took " + elapsedTime + "ms");
        }

        /// <summary>
        /// Handles the tiles in the temp directory
        /// </summary>
        private void ProcessTempTiles()
        {
            String tempDir = FS.BuildTempDir(OutputPath);
            String[] zoomLevels = FS.EnumerateDir(tempDir);
            foreach (String zoomLevel in zoomLevels)
            {
                String zoomLevelName = FS.GetPathName(zoomLevel);
                String[] mapRegions = FS.EnumerateDir(Path.Combine(tempDir, zoomLevel));
                foreach (String mapRegion in mapRegions)
                {
                    String mapRegionName = FS.GetPathName(mapRegion);
                    String[] tiles = FS.EnumerateFiles(mapRegion);
                    int nTiles = tiles.Length;
                    for (int i=0; i < nTiles; i++)
                    {
                        if (StopRequested)
                        {
                            return;
                        }

                        // Get the tileID
                        String tileID = FS.GetTileID(tiles[i]);

                        // Ensure that this tile still needs to be processed
                        String regionTileID = zoomLevelName + mapRegionName + tileID;
                        if (!ProcessQueue.Contains(regionTileID))
                        {
                            Progress.Update(1);
                            continue;
                        }

                        Logger.Log("Handling incomplete tile with id: " + tileID);
                        List<String> currentTileCrop = new List<string>();
                        currentTileCrop.Add(tiles[i]);

                        // Get all tiles of this ID
                        while (!(i+1 > nTiles-1))
                        {
                            if (StopRequested)
                            {
                                return;
                            }

                            if (FS.GetTileID(tiles[i + 1]) == tileID)
                            {
                                currentTileCrop.Add(tiles[++i]);
                            } else
                            {
                                break;
                            }
                        }

                        // Process the tile crop
                        if (currentTileCrop.Count > 1)
                        {
                            // We have multiple incomplete tiles with this id, handle them
                            Logger.Log("\tHandling " + currentTileCrop.Count + " tiles with ID " + tileID + " in zoom level " + zoomLevelName + " and map region " + mapRegionName);

                            // Handle the tiles in the current tile corp
                            String[] tileCrop = currentTileCrop.ToArray();
                            MapTile tileA = new MapTile(tileCrop[0], Logger);
                            MapTile tileB = new MapTile(tileCrop[1], Logger);
                            String resultPath = Path.Combine(tempDir, zoomLevelName, mapRegionName);

                            // Merge the first two tiles
                            String mergeResult = MapTile.MergeTiles(tileA, tileB, resultPath);
                            MapTile resultingTile = new MapTile(mergeResult, Logger);

                            Progress.Update(2);

                            if (currentTileCrop.Count > 2)
                            {
                                for (int j=2; j < currentTileCrop.Count; j++)
                                {
                                    tileA = resultingTile;
                                    tileB = new MapTile(tileCrop[j], Logger);
                                    mergeResult = MapTile.MergeTiles(tileA, tileB, resultPath);
                                    resultingTile = new MapTile(mergeResult, Logger);
                                    Progress.Update(1);
                                }
                            }

                            // Copy the merged tile result to the final location
                            HandleMergedTile(zoomLevelName, mapRegionName, mergeResult);
                        }
                        else
                        {
                            // There are not multiple of these tiles, just copy it to the output directory
                            Logger.Log("\tThere is only one incomplete tile with id " + tileID + " in zoom level " + zoomLevelName + " and map region " + mapRegionName + ". copying it to output directory");
                            HandleIncompleteNonMergedTile(zoomLevelName, mapRegionName, tileID, tiles[i]);

                            // Delete the temporary tile file
                            FS.DeleteFile(tiles[i]);

                            // Update the progress tracker
                            Progress.Update(1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets the tiling engine to a clean state.
        /// </summary>
        private void Reset()
        {
            Progress.Reset();
            this.Stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Handles a tile which is already complete by copying it to its final destination.
        /// </summary>
        /// <param name="atlasId">The ID for the atlas</param>
        /// <param name="zoomLevelId">The zoom level for the tile</param>
        /// <param name="regionId">The region the tile is in</param>
        /// <param name="tileId">The tile ID</param>
        private void HandleCompleteTile(String atlasId, String zoomLevelId, String regionId, String tileId)
        {
            String copyToDir = FS.BuildOutputDir(OutputPath, zoomLevelId, regionId);
            String copyPath = Path.Combine(copyToDir, tileId);
            String copyFrom = FS.GetTilePath(InputPath, atlasId, zoomLevelId, regionId, tileId);
            File.Copy(copyFrom, copyPath, true);
        }

        private void HandleCompleteTile(MapTile tile)
        {
            String copyToDir = FS.BuildOutputDir(OutputPath, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName());
            String copyPath = Path.Combine(copyToDir, tile.GetName());
            File.Copy(tile.GetPath(), copyPath, true);
        }

        /// <summary>
        /// Handles an incomplete tile by copying it to the temporary working directory
        /// </summary>
        /// <param name="atlasId">The atlas the tile is in</param>
        /// <param name="zoomLevelId">The zoom level of the tile</param>
        /// <param name="regionId">The region the tile is in</param>
        /// <param name="tileId">The tile ID</param>
        private void HandleIncompleteTile(String atlasId, String zoomLevelId, String regionId, String tileId)
        {
            String tmpDir = FS.BuildTempDir(OutputPath);
            String copyTo = FS.BuildTempPath(tmpDir, zoomLevelId, regionId, tileId, atlasId);
            String copyFrom = FS.GetTilePath(InputPath, atlasId, zoomLevelId, regionId, tileId);
            File.Copy(copyFrom, copyTo, true);
        }

        private void HandleIncompleteTile(MapTile tile)
        {
            String tmpDir = FS.BuildTempDir(OutputPath);
            String copyTo = FS.BuildTempPath(tmpDir, tile.GetZoomLevel().GetName(), tile.GetMapRegion().GetName(), tile.GetName(), tile.GetAtlas().GetName());
            File.Copy(tile.GetPath(), copyTo, true);
        }

        /// <summary>
        /// Handles a merged tile by copying it to the final destination.
        /// </summary>
        /// <param name="zoomLevelId">The zoom level of this tile</param>
        /// <param name="regionId">The region this tile is in</param>
        /// <param name="mergedTile">The path to the merged tile file</param>
        private void HandleMergedTile(String zoomLevelId, String regionId, String mergedTile)
        {
            String copyTo = FS.BuildOutputDir(OutputPath, zoomLevelId, regionId);
            String copyFrom = Path.Combine(copyTo, FS.GetTileID(mergedTile));
            File.Copy(mergedTile, copyFrom, true);
        }

        /// <summary>
        /// Handles incomplete tiles that could not be merged by copying them to the final destination
        /// </summary>
        /// <param name="zoomLevelId">The zoom level of the tile</param>
        /// <param name="regionId">The region the tile is in</param>
        /// <param name="tileId">The tile id</param>
        /// <param name="tile">The path to the tile on disk</param>
        private void HandleIncompleteNonMergedTile(String zoomLevelId, String regionId, String tileId, String tile)
        {
            String copyTo = FS.BuildOutputDir(OutputPath, zoomLevelId, regionId);
            String copyPath = Path.Combine(copyTo, tileId);
            File.Copy(tile, copyPath, true);
        }

        /// <summary>
        /// Returns the total number of tiles
        /// </summary>
        /// <returns></returns>
        public int GetNTiles()
        {
            return this.NTiles;
        }

    }
}
