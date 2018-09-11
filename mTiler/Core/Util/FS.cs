﻿/*
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
using System.Linq;

namespace mTiler.Core.Util
{
    class FS
    {
        /// <summary>
        /// Used to help generate random strings for merge result file names
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Enumerates all of the directories within a path.
        /// </summary>
        /// <param name="path">The path to enumerate directories for</param>
        /// <returns>String[] - All the directories within the path, or null if none</returns>
        public static string[] EnumerateDir(string path)
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
        public static string[] EnumerateFiles(string path)
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
        public static string GetPathName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// Gets the name of a file from a path
        /// </summary>
        /// <param name="path">The path to get filename from</param>
        /// <returns>The name of the file in the path</returns>
        public static string GetFilename(string path)
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
        public static string BuildOutputDir(string outputDir, string zoomLevel, string mapRegion)
        {
            string output = Path.Combine(outputDir, zoomLevel, mapRegion);
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
        public static string BuildTempDir(string outputDir)
        {
            string output = Path.Combine(outputDir, "_temp");
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
        public static string BuildTempPath(string tempDir, string zoomLevel, string regionID, string tileID, string atlasID)
        {
            string outPath = Path.Combine(tempDir, zoomLevel, regionID);
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            return (string)Path.Combine(outPath, tileID + "_" + atlasID + ".jpg");
        }

        public static string BuildTempPath(string tempDir, string zoomLevel, string regionID, string tileID)
        {
            string outPath = Path.Combine(tempDir, zoomLevel, regionID);
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            return (string)Path.Combine(outPath, tileID + ".jpg");
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
        public static string GetTilePath(string inputDir, string atlasID, string zoomLevelID, string regionID, string tileID)
        {
            return (string)Path.Combine(inputDir, atlasID, zoomLevelID, regionID, tileID);
        }

        /// <summary>
        /// Gets the tileID from a tile path
        /// </summary>
        /// <param name="tilePath">The path to the tile</param>
        /// <returns>The tile ID</returns>
        public static string GetTileID(string tilePath)
        {
            if (!string.IsNullOrWhiteSpace(tilePath))
            {
                string tileFileName = FS.GetFilename(tilePath);
                int spacerLoc = tileFileName.IndexOf("_", StringComparison.Ordinal);
                if (spacerLoc > 0)
                {
                    return tileFileName.Substring(0, spacerLoc);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Deletes a given file from the filesystem
        /// </summary>
        /// <param name="path">The path to the file to delete</param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Generates a random string to be used on the end of merge result file names
        /// </summary>
        /// <param name="length">The length of the random string</param>
        /// <returns>The random string</returns>
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        private static string CleanupMergeName(string name)
        {
            if (name.Contains("_mergeresult"))
            {
                name = name.Substring(name.Length - 20);
                string uuid = RandomString(4);
                name += "_mergeresult" + uuid + ".jpg";
            }

            return name;
        }

        /// <summary>
        /// Writes a bitmap to disk as a PNG
        /// </summary>
        /// <param name="bmp">The bitmap to write to disk</param>
        /// <param name="outputDir">The directory to write to</param>
        /// <param name="name">The name to use</param>
        /// <returns>The path to the resulting PNG</returns>
        public static string WriteBitmapToJpeg(Bitmap bmp, string outputDir, string name)
        {
            // Cleanup the name


            string path = (string)Path.Combine(outputDir, CleanupMergeName(name));

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

        /// <summary>
        /// Gets the path to a tile in the output directory
        /// </summary>
        /// <param name="output">The output path</param>
        /// <param name="zoomLevel">The zoom level of the tile</param>
        /// <param name="mapRegion">The region the tile is in</param>
        /// <param name="tileId">The tile's id</param>
        /// <returns>The path to the tile on disk</returns>
        public static string GetTileFromOutput(string output, string zoomLevel, string mapRegion, string tileId)
        {
            string outputDir = FS.BuildOutputDir(output, zoomLevel, mapRegion);
            return Path.Combine(outputDir, tileId);
        }

    }
}
