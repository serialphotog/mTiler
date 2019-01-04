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
