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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace mTiler.Core.IO
{
    /// <summary>
    /// A load buffer for reading tiles from disk into.
    /// </summary>
    class TileLoadBuffer
    {
        /// <summary>
        ///  The backing storage for the buffer.
        /// </summary>
        public ConcurrentDictionary<string, List<Tile>> Buffer { get; set; }

        /// <summary>
        /// Initialize the buffer
        /// </summary>
        public TileLoadBuffer()
        {
            Buffer = new ConcurrentDictionary<string, List<Tile>>();
        }

        /// <summary>
        /// Cleans up the memory used by the buffer.
        /// </summary>
        ~TileLoadBuffer()
        {
            Clear();
        }

        /// <summary>
        /// Clears the memory used by the buffer
        /// </summary>
        public void Clear()
        {
            if (Buffer != null)
            {
                Buffer.Clear();
                Buffer = null;
            }
        }

        /// <summary>
        /// Checks if the buffer contains an instance of a given tile
        /// </summary>
        /// <param name="tile">The tile to check for an instance of</param>
        /// <returns>True if the buffer has an instance of the tile, else false</returns>
        public bool ContainsInstanceOfTile(Tile tile)
        {
            string tileRegionId = tile.RegionID;
            return Buffer.ContainsKey(tileRegionId);
        }

        /// <summary>
        /// Returns the number of instances of a given tile in the buffer
        /// </summary>
        /// <param name="tile">The tile to get instance count for</param>
        /// <returns>The number of instances of the given tile</returns>
        public int GetInstanceCount(Tile tile)
        {
            string tileRegionId = tile.RegionID;
            if (ContainsInstanceOfTile(tile))
            {
                return Buffer[tileRegionId].Count;
            }
            return 0;
        }

        /// <summary>
        /// Adds a tile to the load buffer
        /// </summary>
        /// <param name="tile">The tile to add to the buffer</param>
        public void Add(Tile tile)
        {
            string tileRegionId = tile.RegionID;
            List<Tile> tileStore;

            if (Buffer.ContainsKey(tileRegionId))
            {
                // There is already an instance of this tile, add it to that part of the buffer
                tileStore = Buffer[tileRegionId];
                tileStore.Add(tile);
                Buffer[tileRegionId] = tileStore;
            }
            else
            {
                tileStore = new List<Tile>
                {
                    tile
                };
                Buffer.TryAdd(tileRegionId, tileStore);
            }
        }

        /// <summary>
        /// Removes an item from the buffer
        /// </summary>
        /// <param name="tileRegionId">The region id for the tile to remove</param>
        public void Remove(string tileRegionId)
        {
            List<Tile> output;
            Buffer.TryRemove(tileRegionId, out output);
        }

        /// <summary>
        /// Returns the number of items in the tile load buffer.
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            int count = 0;
            foreach (List<Tile> tiles in Buffer.Values)
            {
                count += tiles.Count;
            }
            return count;
        }
    }
}
