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
        delegate void loggingDelegate(String text);

        /// <summary>
        /// Reference to the output window
        /// </summary>
        private RichTextBox outputWindow { get; set; }

        /// <summary>
        /// Initializes the logger component
        /// </summary>
        /// <param name="outputWindow">Reference to the output window textbox</param>
        public Logger(RichTextBox outputWindow)
        {
            this.outputWindow = outputWindow;
        }

        /// <summary>
        /// Logs a message to the output console
        /// </summary>
        /// <param name="header">The header that gets printed before each message</param>
        /// <param name="msg">The message to log</param>
        /// <param name="color">The text color to log the message in</param>
        private void log(String header, String msg, Color color)
        {
           outputWindow.Invoke((Action)delegate // Make things thread-safe
           {
               try
               {
                   outputWindow.Select(outputWindow.TextLength, 0);
                   outputWindow.SelectionColor = color;
                   outputWindow.AppendText(header + msg + Environment.NewLine);
               } catch (Exception e)
               {
                    // Ignore. This is the result of canceling in the middle of an operation.
               }
           });
        }

        /// <summary>
        /// Logs a message to the output window
        /// </summary>
        /// <param name="msg">The message to log</param>
        public void log(String msg)
        {
            log("[INFO]: ", msg, Color.Black);
        }

        /// <summary>
        /// Logs an error to the output console
        /// </summary>
        /// <param name="msg">The error message to log</param>
        public void error(String msg)
        {
            log("[ERROR]: ", msg, Color.Red);
        }
    }
}
