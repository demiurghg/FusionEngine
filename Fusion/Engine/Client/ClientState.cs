using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Client {
	
	public enum ClientState {

		/// <summary>
		/// Stand-by mode, no connection.
		/// </summary>
		StandBy,

		/// <summary>
		/// Connecting to server.
		/// </summary>
		Connecting,

		/// <summary>
		/// Loading level.
		/// </summary>
		Loading,

		/// <summary>
		/// Awaiting first snapshot.
		/// </summary>
		Awaiting,

		/// <summary>
		/// Active state.
		/// </summary>
		Active,

		/// <summary>
		/// Client has been disconnected.
		/// </summary>
		Disconnected,
	}
}
