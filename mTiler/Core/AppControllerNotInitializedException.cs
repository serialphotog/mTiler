using System;

/// <summary>
/// Fired when someone attempts to use the application controller without first initializing it.
/// </summary>
namespace mTiler.Core
{
    class AppControllerNotInitializedException : Exception
    {

        public AppControllerNotInitializedException()
        {

        }

        public AppControllerNotInitializedException(string message) : base(message)
        {

        }

        public AppControllerNotInitializedException(string message, Exception inner) : base(message, inner)
        {

        }

    }
}
