using mTiler.Core.Data;
using mTiler.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

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
        private String inputPath;

        /// <summary>
        /// The output path, where we will write the final tiles
        /// </summary>
        private String outputPath;

        /// <summary>
        /// Reference to the logger component
        /// </summary>
        private Logger logger;

        /// <summary>
        /// The atlases within the project
        /// </summary>
        private Atlas[] atlases;

        /// <summary>
        /// Initializes the tiling engine
        /// </summary>
        /// <param name="inputPath">The path to the input directory</param>
        /// <param name="outputPath">The path for the output directory</param>
        /// <param name="logger">Reference to the logging component</param>
        public TilingEngine(String inputPath, String outputPath, Logger logger)
        {
            this.logger = logger;
            this.inputPath = inputPath;
            this.outputPath = outputPath;

            // Validate the input and output paths
            if (validateInputPath(inputPath))
            {
                if (validateOutputPath(outputPath))
                {
                    // Enumerate the atlases and kick off loading all of the data
                    loadAtlases();
                }
            }
        }

        /// <summary>
        /// Validates the output path. Will attempt to create it if it doesn't exist.
        /// </summary>
        /// <param name="path">The output path</param>
        /// <returns>True on success, else false</returns>
        private Boolean validateOutputPath(String path)
        {
            if (!(path.Length >= 3))
            {
                logger.error("Please enter a valid output path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                // The output path doesn't exist, attempt to create it
                logger.log("The output path " + path + " does not exist. Attempting to create it...");
                try
                {
                    Directory.CreateDirectory(path);
                    logger.log("Successfully created the output directory at " + path);
                    return true;
                } catch (Exception e)
                {
                    logger.error("Failed to create output directory at " + path + " . " + e.ToString());
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
        private Boolean validateInputPath(String path)
        {
            if (!(path.Length >= 3))
            {
                logger.error("Please enter a valid input path.");
                return false;
            }
            else if (!Directory.Exists(path))
            {
                logger.error("The input path " + path + " does not exist!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Loads all the atlas projects from the input directory
        /// </summary>
        private void loadAtlases()
        {
            logger.log("Loading atlas projects from " + inputPath);

            // Find all of the atlas projects in the input path
            String[] potentialAtlases = FS.enumerateDir(inputPath);

            if (potentialAtlases != null && potentialAtlases.Length > 0)
            {
                List<Atlas> atlases = new List<Atlas>();
                foreach (String dir in potentialAtlases)
                {
                    // Filter out all non atlas dirs.
                    // For now, the filtering is very basic and just looks for directories that end in "_atlas"
                    if (dir.EndsWith("_atlas"))
                    {
                        logger.log("Found atals project " + dir);
                        Atlas atlas = new Atlas(dir, logger);
                        atlases.Add(atlas);
                    }
                }

                // Check that we actually found some atlas projects
                if (atlases != null && !(atlases.Count > 0))
                {
                    logger.error("No atlas projects were found in " + inputPath);
                }
                else
                {
                    this.atlases = atlases.ToArray();
                }
            } else
            {
                logger.error("No atlas projects were found in " + inputPath);
            }
        }

        /// <summary>
        /// Perform the tiling operations
        /// </summary>
        public void tile()
        {
            logger.log("Performing the tiling operations...");

            // Tracks the tiles that have already been handled, based off the tile's name. There is no need
            // to handle anyone tile of a given ID more than once.
            List<String> visitedTiles = new List<string>();

            // Iterate through all of the atlas projects
            int nAtlases = atlases.Length - 1;
            for (int currentAtlas=0; currentAtlas <= nAtlases; currentAtlas++)
            {
                Atlas atlas = atlases[currentAtlas];
                String atlasID = atlas.getName();

                // Iterate through the zoom levels
                ZoomLevel[] zoomLevels = atlases[currentAtlas].getZoomLevels();
                for (int currentZoom = 0; currentZoom < zoomLevels.Length; currentZoom++)
                {
                    ZoomLevel zoom = zoomLevels[currentZoom];
                    String zoomLevelID = zoom.getName();

                    // Iterate through the map regions
                    MapRegion[] mapRegions = zoomLevels[currentZoom].getMapRegions();
                    for (int currentRegion = 0; currentRegion < mapRegions.Length; currentRegion++)
                    {
                        MapRegion region = mapRegions[currentRegion];
                        String regionID = region.getName();

                        // Iterate through the tiles
                        MapTile[] mapTiles = mapRegions[currentRegion].getMapTiles();
                        for (int currentTile = 0; currentTile < mapTiles.Length; currentTile++)
                        {
                            // For each tile, perform a forward search in the other atlas projects for matching tiles.
                            // It should be noted that there should be no need to search backwards over previous atlas projects, since
                            // all the tiles within those should have already been handled.
                            MapTile tile = mapTiles[currentTile];
                            String tileID = tile.getName();

                            // Tracks rather or not this tile has been fully handled. If it hasn't, we don't mark it as being ignored for
                            // future search. This is done for the case in which we have an all-white tile (a tile with no usuable data).
                            Boolean tileIsHandled = false;

                            // Don't handle the tile more than once
                            if (!visitedTiles.Contains(tileID))
                            {
                                logger.log("Analyzing tile " + tileID + " from atlas " + atlasID + " at zoom level " + zoomLevelID + " for map region " + regionID);
                                tileIsHandled = true;

                                if (tile.isDatalessTile())
                                {
                                    // This tile has no data, ignore it
                                    tileIsHandled = false;
                                    logger.log("\t\tTile " + tileID + " from atlas " + atlasID + " at zoom level " + zoomLevelID + " for map region " + regionID + " has no data. Ignoring it...");
                                }
                            }

                            if (tileIsHandled)
                            {
                                // Add this tile to the visited tiles list
                                visitedTiles.Add(tileID);
                            }
                        }
                    }
                }
            }
        }
    }
}
