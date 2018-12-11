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

using mTiler.Core.Mapping;
using mTiler.Core.Profiling;
using mTiler.Core.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace mTiler.Core.Tiling
{
    /// <summary>
    /// The core engine that merges the incomplete map tiles together. 
    /// </summary>
    class MergeEngine
    {
        /// <summary>
        /// The threshold to use when checking for "white" pixels
        /// </summary>
        private static readonly int WhiteThreshold = 30;

        /// <summary>
        /// Threshold that is used when determining how alike colors are
        /// </summary>
        private static readonly int LikenessThreshold = 10;

        /// <summary>
        /// The amount of the back pixel color to keep when performing blends
        /// </summary>
        private static readonly double BlendAmount = 0.999;

        /// <summary>
        /// The app controller instance
        /// </summary>
        private ApplicationController AppController = ApplicationController.Instance;

        /// <summary>
        /// The merge queue
        /// </summary>
        private ConcurrentDictionary<string, List<Tile>> MergeQueue;

        /// <summary>
        /// Runs the job queue
        /// </summary>
        public void Run()
        {
            RunMergeQueue();
        }

        /// <summary>
        /// Checks if the merge queue contains a given job
        /// </summary>
        /// <param name="jobId">The job id</param>
        /// <returns>True if queue has job, else false</returns>
        public Boolean HasJob(string jobId)
        {
            return MergeQueue.ContainsKey(jobId);
        }

        /// <summary>
        /// Removes a job from the queue
        /// </summary>
        /// <param name="jobId">The job id to remove</param>
        public void Remove(string jobId)
        {
            if (HasJob(jobId))
            {
                List<Tile> tmp;
                MergeQueue.TryRemove(jobId, out tmp);
            }
        }

        /// <summary>
        /// Gets the size of a job
        /// </summary>
        /// <param name="jobId">The id of job to get size for</param>
        /// <returns>The count of the job, or -1 if job doesn't exist</returns>
        public int GetCountForJob(string jobId)
        {
            if (HasJob(jobId))
            {
                return MergeQueue[jobId].Count;
            }
            return -1;
        }

        /// <summary>
        /// Gets a job from the queue
        /// </summary>
        /// <param name="jobId">The id of the job</param>
        /// <returns>The job, or null if not present in queue</returns>
        public List<Tile> GetJob(string jobId)
        {
            if (HasJob(jobId))
            {
                return MergeQueue[jobId];
            }
            return null;
        }

        /// <summary>
        /// Updates (or adds) a job to the queue. 
        /// </summary>
        /// <param name="jobId">The job id</param>
        /// <param name="job">The job</param>
        public void Update(string jobId, List<Tile> job)
        {
            if (HasJob(jobId))
            {
                MergeQueue[jobId] = job;
            }
            else
            {
                MergeQueue.TryAdd(jobId, job);
            }
        }

        /// <summary>
        /// Reset the merge engine to a clean state
        /// </summary>
        public void Reset()
        {
            MergeQueue = new ConcurrentDictionary<string, List<Tile>>();
        }

        /// <summary>
        /// Processes the merge queue
        /// </summary>
        private void RunMergeQueue()
        {
            // Run the merge queue
            Parallel.ForEach(MergeQueue.Values, new ParallelOptions { MaxDegreeOfParallelism = AppController.MaxTilingThreads }, (mergeJob, state) =>
            {
                if (AppController.StopRequested)
                    state.Break();

                HandleMergeJob(mergeJob);
                // Free up some memory
                mergeJob.Clear();
            });
            // Free up memory
            MergeQueue.Clear();
            MergeQueue = null; 
        }

        /// <summary>
        /// Handles a single merge job
        /// </summary>
        /// <param name="mergeJob">The merge job to handle</param>
        private void HandleMergeJob(List<Tile> mergeJob)
        {
            int jobSize = mergeJob.Count;
            if (jobSize > 0)
            {
                string tmpDir = FS.BuildTempDir(AppController.OutputPath);
                Tile currentTile = mergeJob[0];

                if (jobSize > 1)
                {
                    // There are multiple tiles to merge
                    if (ApplicationController.Instance.EnableVerboseLogging)
                        AppController.Logger.Log("Handling " + jobSize + " tiles with ID " + currentTile.GetName() + " in zoom level " + currentTile.ZoomLevel.ToString() + " and region " + currentTile.Coords.Y.ToString());
                    Tile nextTile = mergeJob[1];
                    string resultPath = Path.Combine(tmpDir, currentTile.ZoomLevel.ToString(), currentTile.Coords.Y.ToString());

                    // Merge the first two tiles
                    string mergeResult = MergeTiles(currentTile, nextTile, resultPath);
                    Tile resultingTile = new Tile(null, currentTile.ZoomLevel, currentTile.Coords, mergeResult);
                    AppController.Progress.Update(2);

                    if (jobSize > 2)
                    {
                        // Merge all the remaining tiles together
                        for (int i = 2; i < jobSize; i++)
                        {
                            // Clean some memory
                            currentTile.Clean();
                            currentTile = null;
                            nextTile.Clean();
                            nextTile = null;

                            currentTile = resultingTile;
                            nextTile = mergeJob[i];
                            mergeResult = MergeTiles(currentTile, nextTile, resultPath);
                            resultingTile = new Tile(null, currentTile.ZoomLevel, currentTile.Coords, mergeResult);
                            AppController.Progress.Update(1);
                        }
                    }

                    // Copy the merge tile to the final location
                    HandleMergedTile(resultingTile);

                    // Clean up tile memory
                    currentTile.Clean();
                    nextTile.Clean();
                }
                else
                {
                    // There are not multiple copies of this tile. Just copy it to final destination
                    if (ApplicationController.Instance.EnableVerboseLogging)
                        AppController.Logger.Warn("There is only one instance of tile " + currentTile.GetName() + " in zoom level " + currentTile.ZoomLevel.ToString() + " and region " + currentTile.Coords.Y.ToString() + ". Copying it to final destination");
                    HandleIncompleteNonMergedTile(currentTile);
                    AppController.Progress.Update(1);
                }
            }
        }

        private string MergeTiles(Tile tileA, Tile tileB, string outputDir)
        {
            Bitmap tileAImage = tileA.GetBitmap();
            Bitmap tileBImage = tileB.GetBitmap();

            int width = tileAImage.Width;
            int height = tileBImage.Height;

            // Create the result bitmap
            PixelFormat pixelFormat = tileAImage.PixelFormat;
            Bitmap resultingTile = new Bitmap(width, height, pixelFormat);

            // Perform the merge
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    Color pixelA = tileAImage.GetPixel(w, h);
                    Color pixelB = tileBImage.GetPixel(w, h);

                    if (!ImageUtil.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold) && !ImageUtil.ColorWithinThresholdOfWhite(pixelB, WhiteThreshold))
                    {
                        // Set to the average of the two pixels
                        if (ImageUtil.ColorsAreClose(pixelA, pixelB, LikenessThreshold))
                        {
                            resultingTile.SetPixel(w, h, pixelA);
                        }
                        else
                        {
                            int brightnessA = ImageUtil.GetBrightness(pixelA);
                            int brightnessB = ImageUtil.GetBrightness(pixelB);
                            Color blendedPixel;

                            // Determine which order to mix the pixels in
                            if (brightnessA > brightnessB)
                            {
                                blendedPixel = ImageUtil.Blend(pixelB, pixelA, BlendAmount);
                            }
                            else
                            {
                                blendedPixel = ImageUtil.Blend(pixelA, pixelB, BlendAmount);
                            }
                            resultingTile.SetPixel(w, h, blendedPixel);
                        }
                    }
                    else if (!ImageUtil.ColorWithinThresholdOfWhite(pixelA, WhiteThreshold))
                    {
                        // This pixel has data, copy it
                        resultingTile.SetPixel(w, h, pixelA);
                    }
                    else
                    {
                        // Pixel b has valid data to copy
                        resultingTile.SetPixel(w, h, pixelB);
                    }
                }
            }

            // Write the bitmap to disk and return the URI
            return FS.WriteBitmapToJpeg(resultingTile, outputDir, tileA.GetName());
        }

        /// <summary>
        /// Handles a merged tile by copying it to the final destination.
        /// </summary>
        /// <param name="tile">The merged tile to move</param>
        private void HandleMergedTile(Tile tile)
        {
            string copyTo = FS.BuildOutputDir(AppController.OutputPath, tile.ZoomLevel.ToString(), tile.Coords.Y.ToString());
            string copyPath = Path.Combine(copyTo, tile.GetName());
            File.Copy(tile.Path, copyPath, true);
        }

        /// <summary>
        /// Handles an incomplete, but non-merged, tile by copying it to the final destination
        /// </summary>
        /// <param name="tile">The tile to copy</param>
        private void HandleIncompleteNonMergedTile(Tile tile)
        {
            string copyTo = FS.BuildOutputDir(AppController.OutputPath, tile.ZoomLevel.ToString(), tile.Coords.Y.ToString());
            string copyPath = Path.Combine(copyTo, tile.GetName());
            File.Copy(tile.Path, copyPath, true);
        }

    }
}
