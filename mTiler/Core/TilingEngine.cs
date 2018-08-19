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
    }
}
