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
	/// A base class to implement a log listener
	/// </summary>
	public abstract class LogListener : IDisposable {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public virtual void Log ( LogMessage message )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public virtual void Flush ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public virtual void Dispose ()
		{
		}

	}
}
