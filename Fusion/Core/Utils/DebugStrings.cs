using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using System.Threading;

namespace Fusion {

	public class DebugString {
		public readonly Color Color;
		public readonly int	ThreadID;
		public readonly string Text;

		internal DebugString ( int threadId, Color color, string text )
		{
			this.Color		=	color;
			this.ThreadID	=	threadId;
			this.Text		=	text;
		}
	}


	/// <summary>
	/// Represents class to monitor everything.
	/// </summary>
	public static class DebugStrings {

		static object lockObj = new object();

		static List<DebugString>	lines = new List<DebugString>();


		/// <summary>
		/// Clears all lines added from current thread.
		/// </summary>
		static public void Clear ()
		{
			lock (lockObj) {
				var id = Thread.CurrentThread.ManagedThreadId;
				lines.RemoveAll( line => line.ThreadID == id );
			}
		}

		
		/// <summary>
		/// Adds line from current thread.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="text"></param>
		static public void Add ( Color color, string text )
		{
			lock (lockObj) {
				lines.Add( new DebugString( Thread.CurrentThread.ManagedThreadId, color, text ) );
			}
		}



		/// <summary>
		/// Adds line from current thread.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="text"></param>
		static public void Add ( Color color, string format, params object[] args )
		{
			Add( color, string.Format(format, args) );
		}



		/// <summary>
		/// Gets lines.
		/// </summary>
		/// <returns></returns>
		static IEnumerable<DebugString> GetLines ()
		{
			lock (lockObj) {
				return lines.OrderBy( line => line.ThreadID ).ToArray();
			}
		}
	}
}
