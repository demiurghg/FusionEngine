using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fusion.Engine.Server {

	/// <summary>
	/// Represents byte array of server state.
	/// </summary>
	//[DebuggerDisplay("Frame={Frame} Data={Data.Length} bytes")]
	public class Snapshot {

		public TimeSpan Timestamp;

		/// <summary>
		/// Frame index when snapshot was captured.
		/// </summary>
		public readonly uint Frame;

		/// <summary>
		/// Contains copy of snapshot data.
		/// </summary>
		public readonly byte[] Data;


		/// <summary>
		///
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="data"></param>
		public Snapshot( TimeSpan timestamp, uint frame, byte[] data )
		{
			this.Timestamp	=	timestamp;

			if (frame==0) {
				throw new ArgumentOutOfRangeException("Frame must be greater than zero");
			}

			this.Frame	=	frame;
			this.Data	=	data.ToArray();
		}


		public override string ToString ()
		{
			return string.Format("#{0}: [{1}]", Frame, string.Join(" ", Data.Select(b=>b.ToString("X2"))));
		}
	}
}
