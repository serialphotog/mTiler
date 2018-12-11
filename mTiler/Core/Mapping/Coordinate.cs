﻿namespace mTiler.Core.Mapping
{
    /// <summary>
    /// Represents a coordinate in 2D space
    /// </summary>
    struct Coordinate
    {
        /// <summary>
        /// The x component of the coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The y component of the coordinate
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Initializes a new coordinate
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}