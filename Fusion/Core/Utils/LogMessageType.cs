using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using CC = System.ConsoleColor;
using Fusion.Core.Mathematics;


namespace Fusion {

	/// <summary>
	/// Defines log message types
	/// </summary>
	public enum LogMessageType {
        /// <summary>
        /// A debug message (level 0).
        /// </summary>
        Debug = 0,

        /// <summary>
        /// A verbose message (level 1).
        /// </summary>
        Verbose = 1,

        /// <summary>
        /// An regular info message (level 2).
        /// </summary>
        Information = 2,

        /// <summary>
        /// A warning message (level 3).
        /// </summary>
        Warning = 3,

        /// <summary>
        /// An error message (level 4).
        /// </summary>
        Error = 4,

        /// <summary>
        /// A Fatal error message (level 5).
        /// </summary>
        Fatal = 5,
	}
}
