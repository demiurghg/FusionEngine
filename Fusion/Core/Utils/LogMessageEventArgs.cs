using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using CC = System.ConsoleColor;
using Fusion.Core.Mathematics;


namespace Fusion {

	/// <summary>
	/// 
	/// </summary>
	public class LogMessageEventArgs : EventArgs {

		/// <summary>
		/// Message.
		/// </summary>
		public readonly LogMessage Message;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="threadId"></param>
		/// <param name="eventType"></param>
		/// <param name="message"></param>
		internal LogMessageEventArgs ( LogMessage message )
		{
			this.Message = message;
		}
	}
}
