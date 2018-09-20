using System;
using System.Diagnostics;

namespace mTiler.Core.Profiling
{
    class Timer
    {

        /// <summary>
        /// The actual timer for the timer
        /// </summary>
        private Stopwatch Stopwatch;

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Start()
        {
            Stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            Stopwatch.Stop();
        }

        /// <summary>
        /// Gets the number of minutes elapsed by the timer
        /// </summary>
        /// <returns></returns>
        public String GetMinutes()
        {
            return String.Format("{0:0.00}", TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds).TotalMinutes);
        }

    }
}
