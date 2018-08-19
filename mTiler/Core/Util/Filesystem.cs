using System;
using System.IO;

namespace mTiler.Core.Util
{
    class Filesystem
    {
        /// <summary>
        /// Enumerates all of the directories within a path.
        /// </summary>
        /// <param name="path">The path to enumerate directories for</param>
        /// <returns>String[] - All the directories within the path</returns>
        public static String[] enumerateDir(String path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            return Directory.GetDirectories(path);
        }
    }
}
