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

using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        public static String[] EnumerateDir(String path)
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
        public static String[] EnumerateFiles(String path)
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
        public static String GetPathName(String path)
        {
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// Gets the name of a file from a path
        /// </summary>
        /// <param name="path">The path to get filename from</param>
        /// <returns>The name of the file in the path</returns>
        public static String GetFilename(String path)
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
        public static String BuildOutputDir(String outputDir, String zoomLevel, String mapRegion)
        {
            String output = Path.Combine(outputDir, zoomLevel, mapRegion);
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }
            return output;
        }

        /// <summary>
        /// Builds a temporary directory in the output directory
        /// </summary>
        /// <param name="outputDir">The output directory path</param>
        /// <returns>The path to the temporary directory</returns>
        public static String BuildTempDir(String outputDir)
        {
            String output = Path.Combine(outputDir, "_temp");
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }
            return output;
        }

        /// <summary>
        /// Builds the full output path for a temporary file
        /// </summary>
        /// <param name="tempDir">The temp directory</param>
        /// <param name="zoomLevel">The zoom level id</param>
        /// <param name="regionID">The region ID</param>
        /// <param name="tileID">The tile ID</param>
        /// <param name="atlasID">The atlas ID</param>
        /// <returns>The output path</returns>
        public static String BuildTempPath(String tempDir, String zoomLevel, String regionID, String tileID, String atlasID)
        {
            String outPath = Path.Combine(tempDir, zoomLevel, regionID);
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            return (String)Path.Combine(outPath, tileID + "_" + atlasID + ".jpg");
        }

        /// <summary>
        /// Gets the path of a tile
        /// </summary>
        /// <param name="inputDir">The input directory</param>
        /// <param name="atlasID">The atlas id of the tile</param>
        /// <param name="zoomLevelID">The zoom level of the tile</param>
        /// <param name="regionID">The region id for the tile</param>
        /// <param name="tileID">The tile id</param>
        /// <returns>The path to the tile</returns>
        public static String GetTilePath(String inputDir, String atlasID, String zoomLevelID, String regionID, String tileID)
        {
            return (String)Path.Combine(inputDir, atlasID, zoomLevelID, regionID, tileID);
        }

        /// <summary>
        /// Gets the tileID from a tile path
        /// </summary>
        /// <param name="tilePath">The path to the tile</param>
        /// <returns>The tile ID</returns>
        public static String GetTileID(String tilePath)
        {
            if (!String.IsNullOrWhiteSpace(tilePath))
            {
                String tileFileName = FS.GetFilename(tilePath);
                int spacerLoc = tileFileName.IndexOf("_", StringComparison.Ordinal);
                if (spacerLoc > 0)
                {
                    return tileFileName.Substring(0, spacerLoc);
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Deletes a given file from the filesystem
        /// </summary>
        /// <param name="path">The path to the file to delete</param>
        public static void DeleteFile(String path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Writes a bitmap to disk as a PNG
        /// </summary>
        /// <param name="bmp">The bitmap to write to disk</param>
        /// <param name="outputDir">The directory to write to</param>
        /// <param name="name">The name to use</param>
        /// <returns>The path to the resulting PNG</returns>
        public static String WriteBitmapToJpeg(Bitmap bmp, String outputDir, String name)
        {
            String path = (String)Path.Combine(outputDir, name + "_mergeresult" + ".png");

            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    bmp.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            return path;
        }

    }
}
