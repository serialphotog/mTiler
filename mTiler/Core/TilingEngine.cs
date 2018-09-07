﻿using mTiler.Core.Data;
using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace mTiler.Core
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
        /// Reference to the form
        /// </summary>
        private MainForm Form;

        /// <summary>
        /// The atlases within the project
        /// </summary>
        private Atlas[] Atlases;

        /// <summary>
        /// The total number of loaded tiles
        /// </summary>
        private int NTiles = 0;
        
        /// <summary>
        /// Used to request that the tiling thread process be stopped
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Tracks the total progress by the tiling task
        /// </summary>
        private int TotalProgress = 0;

        /// <summary>
        /// Initializes the tiling engine
        /// </summary>
        /// <param name="inputPath">The path to the input directory</param>
        /// <param name="outputPath">The path for the output directory</param>
        /// <param name="logger">Reference to the logging component</param>
        /// <param name="form">Reference to the main form (for progress bar updating)</param>
        public TilingEngine(String inputPath, String outputPath, Logger logger, MainForm form)
        {
            this.Form = form;
            this.Logger = logger;
            this.InputPath = inputPath;
            this.OutputPath = outputPath;
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
                    await LoadAtlases();
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

        /// <summary>
        /// Loads all the atlas projects from the input directory
        /// </summary>
#pragma warning disable 1998
        private async Task LoadAtlases()
        {
            Logger.Log("Loading atlas projects from " + InputPath);

            // Find all of the atlas projects in the input path
            String[] potentialAtlases = FS.EnumerateDir(InputPath);

            if (potentialAtlases != null && potentialAtlases.Length > 0)
            {
                List<Atlas> atlases = new List<Atlas>();
                foreach (String dir in potentialAtlases)
                {
                    // Filter out all non atlas dirs.
                    // For now, the filtering is very basic and just looks for directories that end in "_atlas"
                    if (dir.EndsWith("_atlas"))
                    {
                        Logger.Log("Found atals project " + dir);
                        Atlas atlas = new Atlas(dir, Logger);
                        NTiles += atlas.NTiles;
                        atlases.Add(atlas);
                    }
                }

                // Check that we actually found some atlas projects
                if (atlases != null && !(atlases.Count > 0))
                {
                    Logger.Error("No atlas projects were found in " + InputPath);
                }
                else
                {
                    this.Atlases = atlases.ToArray();
                }
            } else
            {
                Logger.Error("No atlas projects were found in " + InputPath);
            }
        }
#pragma warning restore 1998

        /// <summary>
        /// Perform the tiling operations
        /// </summary>
        public void Tile()
        {
            Logger.Log("Performing the tiling operations...");
            TotalProgress = 0;

            // Tracks the tiles that have already been handled, based off the tile's name. There is no need
            // to handle anyone tile of a given ID more than once.
            List<String> visitedTiles = new List<string>();

            // Iterate through all of the atlas projects
            int nAtlases = Atlases.Length - 1;
            for (int currentAtlas=0; currentAtlas <= nAtlases; currentAtlas++)
            {
                Atlas atlas = Atlases[currentAtlas];
                String atlasID = atlas.GetName();

                // Iterate through the zoom levels
                ZoomLevel[] zoomLevels = Atlases[currentAtlas].GetZoomLevels();
                for (int currentZoom = 0; currentZoom < zoomLevels.Length; currentZoom++)
                {
                    ZoomLevel zoom = zoomLevels[currentZoom];
                    String zoomLevelID = zoom.GetName();

                    // Iterate through the map regions
                    MapRegion[] mapRegions = zoomLevels[currentZoom].GetMapRegions();
                    for (int currentRegion = 0; currentRegion < mapRegions.Length; currentRegion++)
                    {
                        MapRegion region = mapRegions[currentRegion];
                        String regionID = region.GetName();

                        // Iterate through the tiles
                        MapTile[] mapTiles = mapRegions[currentRegion].GetMapTiles();
                        for (int currentTile = 0; currentTile < mapTiles.Length; currentTile++)
                        {
                            if (StopRequested) // User requested thread be stopped
                                return;

                            // For each tile, perform a forward search in the other atlas projects for matching tiles.
                            // It should be noted that there should be no need to search backwards over previous atlas projects, since
                            // all the tiles within those should have already been handled.
                            MapTile tile = mapTiles[currentTile];
                            String tileID = tile.GetName();
                            String regionTileID = zoomLevelID + regionID + tileID;

                            // Tracks rather or not this tile has been fully handled. If it hasn't, we don't mark it as being ignored for
                            // future search. This is done for the case in which we have an all-white tile (a tile with no usuable data).
                            Boolean tileIsHandled = false;

                            // Don't handle the tile more than once
                            if (!visitedTiles.Contains(regionTileID))
                            {
                                Logger.Log("Analyzing tile " + tileID + " from atlas " + atlasID + " at zoom level " + zoomLevelID + " for map region " + regionID);

                                if (tile.IsDatalessTile())
                                {
                                    // This tile has no data, ignore it
                                    tileIsHandled = false;
                                    Logger.Log("\tTile " + tileID + " from atlas " + atlasID + " at zoom level " + zoomLevelID + " for map region " + regionID + " has no data. Ignoring it...");
                                    UpdateProgress(++TotalProgress);
                                }
                                else if (tile.IsComplete())
                                {
                                    // This tile is complete, ignore other versions of it and copy it to destination
                                    tileIsHandled = true;
                                    Logger.Log("\tTile " + tileID + " from atlas " + atlasID + " at zoom level " + zoomLevelID + " for map region " + regionID + " is already complete.");

                                    // Copy the complete tile to the output path
                                    HandleCompleteTile(atlasID, zoomLevelID, regionID, tileID);
                                    UpdateProgress(++TotalProgress);
                                } else
                                {
                                    // Copy the tiles to a temporary working directory for further processing.
                                    tileIsHandled = false;
                                    String tempDir = FS.BuildTempDir(OutputPath);
                                    String copyTo = FS.BuildTempPath(tempDir, zoomLevelID, regionID, tileID, atlasID);
                                    String copyFromPath = FS.GetTilePath(InputPath, atlasID, zoomLevelID, regionID, tileID);
                                    File.Copy(copyFromPath, copyTo, true);
                                }
                            }

                            if (tileIsHandled)
                            {
                                // Add this tile to the visited tiles list
                                visitedTiles.Add(regionTileID);
                            }
                        }
                    }
                }
            }

            // Handle all of the temporary tiles
            Logger.Log("Processing incomplete tiles...");
            ProcessTempTiles();

            // Delete the temp directory
            Logger.Log("Cleaning up the temporary directory");
            Directory.Delete(FS.BuildTempDir(OutputPath), true);
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
                            return;

                        // Get the tileID
                        String tileID = FS.GetTileID(tiles[i]);
                        Logger.Log("Handling incomplete tile with id: " + tileID);
                        List<String> currentTileCrop = new List<string>();
                        currentTileCrop.Add(tiles[i]);

                        // Get all tiles of this ID
                        while (!(i+1 > nTiles-1))
                        {
                            if (StopRequested)
                                return;

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

                            TotalProgress += 2;
                            UpdateProgress(TotalProgress);

                            if (currentTileCrop.Count > 2)
                            {
                                for (int j=2; j < currentTileCrop.Count; j++)
                                {
                                    tileA = resultingTile;
                                    tileB = new MapTile(tileCrop[j], Logger);
                                    mergeResult = MapTile.MergeTiles(tileA, tileB, resultPath);
                                    resultingTile = new MapTile(mergeResult, Logger);
                                    UpdateProgress(++TotalProgress);
                                }
                            }

                            // Copy the merged tile result to the final location
                            String copyToDir = FS.BuildOutputDir(OutputPath, zoomLevelName, mapRegionName);
                            String copyPath = Path.Combine(copyToDir, FS.GetTileID(mergeResult));
                            File.Copy(mergeResult, copyPath, true);
                        }
                        else
                        {
                            // There are not multiple of these tiles, just copy it to the output directory
                            Logger.Log("\tThere is only one incomplete tile with id " + tileID + " in zoom level " + zoomLevelName + " and map region " + mapRegionName + ". copying it to output directory");
                            String copyToDir = FS.BuildOutputDir(OutputPath, zoomLevelName, mapRegionName);
                            String copyPath = Path.Combine(copyToDir, tileID);
                            File.Copy(tiles[i], copyPath, true);

                            // Delete the temporary tile file
                            FS.DeleteFile(tiles[i]);

                            // Update the progress tracker
                            UpdateProgress(++TotalProgress);
                        }
                    }
                }
            }
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
            String copyFromPath = FS.GetTilePath(InputPath, atlasId, zoomLevelId, regionId, tileId);
            File.Copy(copyFromPath, copyPath, true);
        }

        /// <summary>
        /// updates the progress
        /// </summary>
        /// <param name="progress">The progress</param>
        private void UpdateProgress(int progress)
        {
            if (!StopRequested)
            {
                Form.Invoke((Action)delegate
                {
                    Form.UpdateProgress(progress);
                });
            }
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
