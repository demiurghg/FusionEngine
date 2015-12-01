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
	/// Provides the abstract base class for the listeners who monitor log output.
	/// </summary>
	public class LogMessage {

		public readonly DateTime DateTime;

		/// <summary>
		/// Thread id from whon log event was captured.
		/// </summary>
		public readonly int ThreadId;

		/// <summary>
		/// Log event type. 
		/// </summary>
		public readonly LogMessageType MessageType;

		/// <summary>
		/// 
		/// </summary>
		public readonly string MessageText;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="threadId"></param>
		/// <param name="eventType"></param>
		/// <param name="message"></param>
		internal LogMessage ( int threadId, LogMessageType messageType, string messageText )
		{
			this.DateTime		=	DateTime.Now;
			this.ThreadId		=	threadId;
			this.MessageType	=	messageType;
			this.MessageText	=	messageText;
		}
	}
}
