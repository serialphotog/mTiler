using mTiler.Core.Data;
using System;
using System.IO;

namespace mTiler.Core
{
    /// <summary>
    /// The tiling engine. Performs the actual tiling work.
    /// </summary>
    class TilingEngine
    {
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

            // Validate the input and output paths
            if (validateInputPath(inputPath))
            {
                if (validateOutputPath(outputPath))
                {
                    logger.log("TODO: Implement the hard stuff :-)");
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
    }
}
