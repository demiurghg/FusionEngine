﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Fusion.Core.Utils {

	public class ColoredLogListener : LogListener {

		public override void Log ( LogMessage message )
		{
			Colorize( message.MessageType );
			
			GetWriter( message.MessageType ).WriteLine("[{0:HH:mm:ss}] {1}> : {2}", message.DateTime, message.ThreadId, message.MessageText );
			//GetWriter( message.MessageType ).WriteLine("{1,2}| {2}", message.DateTime, message.ThreadId, message.MessageText );

			Console.ResetColor();
		}



		TextWriter GetWriter ( LogMessageType eventType )
		{
			if ( eventType==LogMessageType.Error || eventType==LogMessageType.Warning || eventType==LogMessageType.Fatal ) {
				return Console.Error;
			} else {
				return Console.Out;
			}
		}


		void Colorize ( LogMessageType eventType )
		{
			switch (eventType) {
				case LogMessageType.Debug :
					Console.ForegroundColor	=	ConsoleColor.DarkGray;
					Console.BackgroundColor	=	ConsoleColor.Black;
				break;
				case LogMessageType.Verbose :
					Console.ForegroundColor	=	ConsoleColor.Gray;
					Console.BackgroundColor	=	ConsoleColor.Black;
				break;
				case LogMessageType.Information :
					Console.ForegroundColor	=	ConsoleColor.White;
					Console.BackgroundColor	=	ConsoleColor.Black;
				break;
				case LogMessageType.Warning :
					Console.ForegroundColor	=	ConsoleColor.Yellow;
					Console.BackgroundColor	=	ConsoleColor.Black;
				break;
				case LogMessageType.Error :
					Console.ForegroundColor	=	ConsoleColor.Red;
					Console.BackgroundColor	=	ConsoleColor.Black;
				break;
				case LogMessageType.Fatal :
					Console.ForegroundColor	=	ConsoleColor.Black;
					Console.BackgroundColor	=	ConsoleColor.Red;
				break;
			}
		}
	}
}
