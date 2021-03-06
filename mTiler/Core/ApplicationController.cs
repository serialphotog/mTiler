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

using mTiler.Core.Profiling;
using mTiler.Core.Tiling;
using mTiler.Core.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mTiler.Core
{
    class ApplicationController
    {

        /// <summary>
        /// The locing object
        /// </summary>
        private static object _syncLock = new object();

        /// <summary>
        /// Tracks if the app controller has been initialized
        /// </summary>
        private static bool _initialized = false;

        /// <summary>
        /// The app controller instance
        /// </summary>
        private static ApplicationController _instance;
        public static ApplicationController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApplicationController();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Tracks if user requested current job be stopped
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Tracks if there is currently a job running
        /// </summary>
        private bool JobRunning = false;

        /// <summary>
        /// Reference to the main form
        /// </summary>
        public MainForm MainFormRef;

        /// <summary>
        /// Reference to the logger
        /// </summary>
        public Logger Logger;

        /// <summary>
        /// The progress monitor
        /// </summary>
        public ProgressMonitor Progress;

        /// <summary>
        /// The tiling engine
        /// </summary>
        public TilingEngine TilingEngine;

        /// <summary>
        /// The merge engine
        /// </summary>
        public MergeEngine MergeEngine;

        /// <summary>
        /// The input path for tiling operations
        /// </summary>
        public string InputPath;

        /// <summary>
        /// The output path for tiling operations
        /// </summary>
        public string OutputPath;

        /// <summary>
        /// Tracks the total work for a given job
        /// </summary>
        public int TotalWork = 0;

        /// <summary>
        /// Timer for the initial data load
        /// </summary>
        private Core.Profiling.Timer DataLoadTimer = new Core.Profiling.Timer();

        /// <summary>
        /// Timer for the tiling operations.
        /// </summary>
        private Core.Profiling.Timer TilingTimer = new Core.Profiling.Timer();

        /// <summary>
        /// Rather or not to enable verbose logging
        /// </summary>
        public bool EnableVerboseLogging;

        /// <summary>
        /// Rather or not to clear the log on job start
        /// </summary>
        public bool ClearLogOnJobStart;

        /// <summary>
        /// The currently enabled merge process
        /// </summary>
        public string EnabledMergeProcess;

        /// <summary>
        /// The max number of threads to use for tiling
        /// </summary>
        public byte MaxTilingThreads;

        public void Initialize(MainForm mainFormRef)
        {
            _initialized = true;
            MainFormRef = mainFormRef;
            Progress = new ProgressMonitor();
            Logger = new Logger(MainFormRef.GetOutputConsole());
            TilingEngine = new TilingEngine();
            MergeEngine = new MergeEngine();

            // Update settings
            ReloadConfig();
        }

        /// <summary>
        /// Reloads the app configuration
        /// </summary>
        public void ReloadConfig()
        {
            EnableVerboseLogging = Properties.Settings.Default.EnableVerboseLogging;
            ClearLogOnJobStart = Properties.Settings.Default.ClearLogOnJobStart;
            MaxTilingThreads = Properties.Settings.Default.MaxNumberTilingThreads;
            EnabledMergeProcess = Properties.Settings.Default.EnabledMergeProcess;
        }

        /// <summary>
        /// Stop all current operations
        /// </summary>
        public void Stop()
        {
            if (!_initialized)
                throw new AppControllerNotInitializedException();
            StopRequested = true;
            JobRunning = false;
        }

        /// <summary>
        /// Starts a tiling job, if one isn't already running
        /// </summary>
        /// <param name="input">The input path</param>
        /// <param name="output">The output path</param>
        public async void Start(string input, string output)
        {
            if (!_initialized)
                throw new AppControllerNotInitializedException();

            InputPath = input;
            OutputPath = output;

            if (!JobRunning)
            {
                JobRunning = true;
                StopRequested = false;

                if (ClearLogOnJobStart)
                    Logger.Clear();

                // Perform the initial data load
                DataLoadTimer.Start();
                await Task.Run(() => TilingEngine.Init());
                DataLoadTimer.Stop();

                TotalWork = TilingEngine.GetTotalTiles();
                MainFormRef.SetTotalWork(TotalWork);

                // Perform the job, if there is work to do
                if (TotalWork > 0)
                {
                    TilingThreadWorker tilingWorker = new TilingThreadWorker();
                    tilingWorker.ThreadComplete += HandleTilingJobComplete;

                    Thread tilingThread = new Thread(() =>
                    {
                        tilingWorker.Run(TilingEngine);
                    });
                    TilingTimer.Start();
                    tilingThread.Start();
                }
                else
                {
                    // There is no work to do
                    JobRunning = false;
                    Logger.Error("There is no work to be performed.");
                }
            }
        }

        /// <summary>
        /// Gets called when the tiling thread completes its work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleTilingJobComplete(object sender, EventArgs e)
        {
            TilingTimer.Stop();
            JobRunning = false;

            // Log output
            Logger.Log("Tiling Complete!");
            Logger.Log("The initial data load took " + DataLoadTimer.GetMinutes() + " minutes");
            Logger.Log("The tiling took " + TilingTimer.GetMinutes() + " minutes");
        }

        class TilingThreadWorker
        {
            // Event that gets fired when the thread completes
            public event EventHandler ThreadComplete;

            /// <summary>
            ///  Runs the work
            /// </summary>
            public void Run(TilingEngine tilingEngine)
            {
                tilingEngine.Tile();

                if (ThreadComplete != null)
                    ThreadComplete(this, EventArgs.Empty);
            }
        }


    }
}
