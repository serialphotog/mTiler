using System;

namespace mTiler.Core.Imaging.Process
{
    /// <summary>
    /// Exception that gets thrown when an invalid merge process is encountered
    /// </summary>
    class InvalidProcessException : Exception
    {
        public InvalidProcessException()
        {

        }

        public InvalidProcessException(string process) : base(String.Format("Invalid process: {0}", process))
        {

        }
    }
}
