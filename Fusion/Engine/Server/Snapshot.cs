using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Server {

	/// <summary>
	/// Represents byte array of server state.
	/// </summary>
	public class Snapshot {

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
		public Snapshot( uint frame, byte[] data )
		{
			if (frame==0) {
				throw new ArgumentOutOfRangeException("Frame must be greater than zero");
			}

			this.Frame	=	frame;
			this.Data	=	data.ToArray();
		}
	}
}
