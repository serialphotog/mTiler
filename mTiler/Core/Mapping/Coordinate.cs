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

namespace mTiler.Core.Mapping
{
    /// <summary>
    /// Represents an X, Y coordinate
    /// </summary>
    struct Coordinate
    {
        /// <summary>
        /// The x component of the coordinate 
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
