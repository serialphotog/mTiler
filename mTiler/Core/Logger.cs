using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mTiler.Core
{
    class Logger
    {
        delegate void LoggingDelegate(String text);

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
        private void Log(String header, String msg, Color color)
        {
            if (!StopRequested)
            {
                OutputWindow.Invoke((Action)delegate // Make things thread-safe
                {
                    OutputWindow.Select(OutputWindow.TextLength, 0);
                    OutputWindow.SelectionColor = color;
                    OutputWindow.AppendText(header + msg + Environment.NewLine);
                });
            }
        }

        /// <summary>
        /// Logs a message to the output window
        /// </summary>
        /// <param name="msg">The message to log</param>
        public void Log(String msg)
        {
            Log("[INFO]: ", msg, Color.Black);
        }

        /// <summary>
        /// Logs an error to the output console
        /// </summary>
        /// <param name="msg">The error message to log</param>
        public void Error(String msg)
        {
            Log("[ERROR]: ", msg, Color.Red);
        }
    }
}
