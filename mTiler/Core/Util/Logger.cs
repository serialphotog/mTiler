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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mTiler.Core.Util
{
    class Logger
    {
        delegate void LoggingDelegate(string text);

        /// <summary>
        /// Reference to the output window
        /// </summary>
        private RichTextBox OutputWindow { get; set; }

        /// <summary>
        /// Used to determine if an application exit has been requested
        /// </summary>
        public volatile bool StopRequested = false;

        /// <summary>
        /// Initializes the logger component
        /// </summary>
        /// <param name="outputWindow">Reference to the output window textbox</param>
        public Logger(RichTextBox outputWindow)
        {
            this.OutputWindow = outputWindow;
        }

        /// <summary>
        /// Logs a message to the output console
        /// </summary>
        /// <param name="header">The header that gets printed before each message</param>
        /// <param name="msg">The message to log</param>
        /// <param name="color">The text color to log the message in</param>
        private void Log(string header, string msg, Color color)
        {
            if (!StopRequested)
            {
                OutputWindow.Invoke((Action)delegate // Make things thread-safe
                {
                    OutputWindow.Select(OutputWindow.TextLength, 0);
                    OutputWindow.SelectionColor = color;
                    StringBuilder builder = new StringBuilder(header);
                    builder.Append(msg).Append(Environment.NewLine);
                    OutputWindow.AppendText(builder.ToString());
                });
            }
        }

        /// <summary>
        /// Logs a message to the output window
        /// </summary>
        /// <param name="msg">The message to log</param>
        public void Log(string msg)
        {
            Log("[INFO]: ", msg, Color.Black);
        }

        /// <summary>
        /// Logs an error to the output console
        /// </summary>
        /// <param name="msg">The error message to log</param>
        public void Error(string msg)
        {
            Log("[ERROR]: ", msg, Color.Red);
        }

        /// <summary>
        /// Logs a warning message to the output console.
        /// </summary>
        /// <param name="msg">The warning message to log</param>
        public void Warn(string msg)
        {
            Log("[WARNING]: ", msg, Color.Orange);
        }
    }
}
