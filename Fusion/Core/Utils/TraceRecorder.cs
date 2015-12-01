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
	public class LogRecorder : LogListener {

		public static event EventHandler	TraceRecorded;

		static object lockObj = new object();


		/// <summary>
		/// Max recorded line count
		/// </summary>
		public static int MaxLineCount {
			get;
			set;
		}
		
		static List<LogMessage> lines = new List<LogMessage>();



		/// <summary>
		/// 
		/// </summary>
		static LogRecorder ()
		{
			MaxLineCount	=	1024;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		void RecordMessage ( LogMessage line )
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
		public static IEnumerable<LogMessage> GetLines ()
		{
			lock (lockObj) {
				return lines.ToArray();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public static void Clear ()
		{
			lock (lockObj) {
				lines.Clear();

				if (TraceRecorded!=null) {
					TraceRecorded(null, EventArgs.Empty);
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public override void Log ( LogMessage message )
		{
			RecordMessage( message );
		}
	}
}
