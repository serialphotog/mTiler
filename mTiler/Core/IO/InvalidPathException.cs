using System;

namespace mTiler.Core.IO
{
    /// <summary>
    /// Exception that gets thrown when an invalid FS path is detected.
    /// </summary>
    class InvalidPathException : Exception
    {
        public InvalidPathException()
        {

        }

        public InvalidPathException(string path) : base(String.Format("Invalid path: {0}", path))
        {

        }
    }
}
