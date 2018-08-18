using System;

namespace mTiler.Core.Data
{
    /// <summary>
    /// Represents an individual zoom level within an atlas project.
    /// </summary>
    class ZoomLevel
    {
        /// <summary>
        /// The map regions within this zoom level
        /// </summary>
        private MapRegion[] mapRegions;

        /// <summary>
        /// Initializes this zoom level
        /// </summary>
        /// <param name="path">The path to this zoom level on disk</param>
        public ZoomLevel(String path)
        {

        }
    }
}
