using System;
using System.IO;

namespace mTiler.Core.Util
{
    class FS
    {
        /// <summary>
        /// Enumerates all of the directories within a path.
        /// </summary>
        /// <param name="path">The path to enumerate directories for</param>
        /// <returns>String[] - All the directories within the path, or null if none</returns>
        public static String[] enumerateDir(String path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            return Directory.GetDirectories(path);
        }

        /// <summary>
        /// Enumerates all of the files within a directory
        /// </summary>
        /// <param name="path">The path to enumerate files for</param>
        /// <returns>String[] of files, else null</returns>
        public static String[] enumerateFiles(String path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            return Directory.GetFiles(path);
        }

        /// <summary>
        /// Gets the name of a path
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The name of the end item of the path</returns>
        public static String getPathName(String path)
        {
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// Gets the name of a file from a path
        /// </summary>
        /// <param name="path">The path to get filename from</param>
        /// <returns>The name of the file in the path</returns>
        public static String getFilename(String path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Builds the output directory, if it doesn't exist, for completed tiles
        /// </summary>
        /// <param name="outputDir">The output directory path</param>
        /// <param name="zoomLevel">The zoom level</param>
        /// <param name="mapRegion">The map Region</param>
        /// <returns>The output path</returns>
        public static String buildOutputDir(String outputDir, String zoomLevel, String mapRegion)
        {
            String output = Path.Combine(outputDir, zoomLevel, mapRegion);
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }
            return output;
        }

    }
}
