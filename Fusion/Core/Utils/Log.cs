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
using System.Threading;
using System.Collections.Concurrent;


namespace Fusion {
	public static class Log {

		/// <summary>
		/// Defines Log verbosity level.
		///	Value means lowest level, that will be printed.
		/// </summary>
		public static LogMessageType VerbosityLevel { get; set; }

		static object lockObj = new object();
		static List<LogListener> listeners = new List<LogListener>();


		///// <summary>
		///// Indicates that debug messages are allowed.
		///// </summary>
		//public static bool IsDebugMessageEnabled { get { return VerbosityLevel == LogMessageType.Debug; } }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		static void AddMessage ( LogMessageType type, string text )
		{
			if (type>=VerbosityLevel) {
				lock (lockObj) {
					var threadId	=	Thread.CurrentThread.ManagedThreadId;
					var message		=	new LogMessage( threadId, type, text );
					
					foreach (var listener in listeners) {
						listener.Log( message );
					}
				}
			}
		}



		/// <summary>
		/// Adds Log listener.
		/// </summary>
		/// <param name="listener"></param>
		public static void AddListener ( LogListener listener )
		{
			lock (lockObj) {
				listeners.Add( listener );
			}
		}



		/// <summary>
		/// Removes Log listener.
		/// </summary>
		/// <param name="listener"></param>
		public static void RemoveListener ( LogListener listener )
		{
			lock (lockObj) {
				listeners.Remove( listener );
				listener.Flush();
			}
		}



		/// <summary>
		/// Logs information message
		/// </summary>
		/// <param name="message"></param>
		public static void Message ( string message )
		{
			AddMessage( LogMessageType.Information, message );
		}



		/// <summary>
		/// Logs information message
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Message ( string format, params object[] args )
		{
			AddMessage( LogMessageType.Information, string.Format(format, args) );
		}



		/// <summary>
		/// Logs verbose message
		/// </summary>
		/// <param name="message"></param>
		public static void Verbose ( string message )
		{
			AddMessage( LogMessageType.Verbose, message );
		}



		/// <summary>
		/// Logs verbose message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Verbose ( string format, params object[] args )
		{
			AddMessage( LogMessageType.Verbose, string.Format(format, args) );
		}



		/// <summary>
		/// Logs warning message.
		/// </summary>
		/// <param name="message"></param>
		public static void Warning ( string message )
		{
			AddMessage( LogMessageType.Warning, message );
		}



		/// <summary>
		/// Logs warning message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Warning ( string format, params object[] args )
		{
			AddMessage( LogMessageType.Warning, string.Format(format, args) );
		}



		/// <summary>
		/// Logs error message.
		/// </summary>
		/// <param name="message"></param>
		public static void Error ( string message )
		{
			AddMessage( LogMessageType.Error, message );
		}



		/// <summary>
		/// Logs error message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Error ( string format, params object[] args )
		{
			AddMessage( LogMessageType.Error, string.Format(format, args) );
		}



		/// <summary>
		/// Logs fatal error message.
		/// </summary>
		/// <param name="message"></param>
		public static void Fatal ( string message )
		{
			AddMessage( LogMessageType.Fatal, message );
		}



		/// <summary>
		/// Logs fatal error message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Fatal ( string format, params object[] args )
		{
			AddMessage( LogMessageType.Fatal, string.Format(format, args) );
		}



		/// <summary>
		/// Logs debug message.
		/// </summary>
		/// <param name="message"></param>
		public static void Debug ( string message )
		{	
			if (VerbosityLevel>LogMessageType.Debug) {
				return;
			}
			AddMessage( LogMessageType.Debug, message );
		}



		/// <summary>
		/// Logs debug message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Debug ( string format, params object[] args )
		{	
			if (VerbosityLevel>LogMessageType.Debug) {
				return;
			}
			AddMessage( LogMessageType.Debug, string.Format(format, args) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void PrintException ( Exception e, string format, params object[] args )
		{
			Log.Error( format, args );

			var lines = e.ToString().Split('\n');

			foreach (var line in lines) {
				Log.Error(line);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		public static void Dump ( byte[] array )
		{
			Trace.WriteLine( "---------------------------------------------------------------------");
			Trace.WriteLine( string.Format("Dump: {0} bytes ({0:X8})", array.Length) );

			for (int i=0; i<MathUtil.IntDivRoundUp( array.Length, 16 ); i++) {

				int count	=	Math.Min(16, array.Length - i * 16);

				string hex	= "";
				string txt  = "";
				
				for (int j=0; j<count; j++) {
					
					var b  = array[i*16+j];
					var ch = (char)b;
					hex += b.ToString("x2");

					if (char.IsControl(ch)) {
						txt += ".";
					} else {
						txt += ch;
					}

					if (j==3||j==7||j==11) {
						hex += "  ";
					} else {
						hex += " ";
					}
				}

				Trace.WriteLine( string.Format("{0,-51}| {1}", hex, txt) );
			}

			Trace.WriteLine( "---------------------------------------------------------------------");
		}
	}
}
