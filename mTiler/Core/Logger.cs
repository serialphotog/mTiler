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
        /// Logs a message to the output window
        /// </summary>
        /// <param name="msg">The message to log</param>
        public void log(String msg)
        {
            outputWindow.Select(outputWindow.TextLength, 0);
            outputWindow.SelectionColor = Color.Black;
            outputWindow.AppendText("[INFO]: " + msg + Environment.NewLine);
        }

        /// <summary>
        /// Logs an error to the output console
        /// </summary>
        /// <param name="msg">The error message to log</param>
        public void error(String msg)
        {
            outputWindow.Select(outputWindow.TextLength, 0);
            outputWindow.SelectionColor = Color.Red;
            outputWindow.AppendText("[ERROR]: " + msg + Environment.NewLine);
        }
    }
}
