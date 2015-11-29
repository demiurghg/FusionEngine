using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fusion.Core.Utils {		 
	
	/// <summary>
	/// Trace recorder
	/// </summary>
	public class TraceRecorder : TraceListener {

		public class Line {
			public readonly TraceEventType EventType;
			public readonly string Message;

			public Line ( TraceEventType eventType, string message ) 
			{
				EventType	=	eventType;
				Message		=	message;
			}
		}


		public static event EventHandler	TraceRecorded;

		static object lockObj = new object();


		/// <summary>
		/// Max recorded line count
		/// </summary>
		public static int MaxLineCount {
			get;
			set;
		}
		
		static List<Line> lines = new List<Line>();



		void NotifyTraceRecord ()
		{
		}
			   

		static TraceRecorder ()
		{
			MaxLineCount	=	1024;
		}



		void AddMessage ( Line line )
		{
			lock (lockObj) {
				lines.Add( line );

				while (lines.Count>MaxLineCount) {
					lines.RemoveAt(0);
				}
				if (TraceRecorded!=null) {
					TraceRecorded(null, EventArgs.Empty);
				}
			}
		}



		/// <summary>
		/// Gets lines.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Line> GetLines ()
		{
			lock (lockObj) {
				return lines.ToArray();
			}
		}



		public static void Clear ()
		{
			lock (lockObj) {
				lines.Clear();

				if (TraceRecorded!=null) {
					TraceRecorded(null, EventArgs.Empty);
				}
			}
		}



		public override void Fail ( string message )
		{
			base.Fail( message );
			NotifyTraceRecord();
		}


		public override void Fail ( string message, string detailMessage )
		{
			base.Fail( message, detailMessage );
			NotifyTraceRecord();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id )
		{
			AddMessage( new Line( eventType, "" ) );
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args )
		{
			AddMessage( new Line( eventType, string.Format( format, args ) ) );
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message )
		{
			AddMessage( new Line( eventType, message ) );
		}



		public override void Write ( string message )
		{
			AddMessage( new Line( TraceEventType.Information, message ) );
		}

		public override void WriteLine ( string message )
		{
			AddMessage( new Line( TraceEventType.Information, message ) );
		}
	}
}
