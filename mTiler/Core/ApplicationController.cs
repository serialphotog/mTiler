﻿using mTiler.Core.Profiling;
using mTiler.Core.Tiling;
using mTiler.Core.Util;
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
        
        public void Initialize(MainForm mainFormRef)
        {
            _initialized = true;
            MainFormRef = mainFormRef;
            Progress = new ProgressMonitor();
            Logger = new Logger(MainFormRef.GetOutputConsole());
            TilingEngine = new TilingEngine();
            MergeEngine = new MergeEngine();
        }

        /// <summary>
        /// Stop all current operations
        /// </summary>
        public void Stop()
        {
            if (!_initialized)
                throw new AppControllerNotInitializedException();
            StopRequested = true;
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

                // Perform the initial data load
                await Task.Run(() => TilingEngine.Init());
                TotalWork = TilingEngine.GetTotalTiles();
                MainFormRef.SetTotalWork(TotalWork);

                // Perform the job, if there is work to do
                if (TotalWork > 0)
                {
                    Thread tilingEngineThread = new Thread(new ThreadStart(TilingEngine.Tile));
                    tilingEngineThread.Start();
                }
                else
                {
                    // There is no work to do
                    JobRunning = false;
                    Logger.Error("There is no work to be performed.");
                }
            }
        }

    }
}