using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Network {
	public enum NetCommand : uint {
		
		/// <summary>
		/// Client sends this command 
		/// when tries to connect to server.
		/// Rest of the packet contains user information.
		/// </summary>
		Connect,

		/// <summary>
		/// Notifies server that client left the game 
		/// by its own reason.
		/// No additional data.
		/// </summary>
		Disconnect,

		/// <summary>
		/// Server sends this command when client connection was accepted.
		/// No additional data.
		/// </summary>
		Accepted,

		/// <summary>
		/// Server sends this command when client was refused.
		/// Data contains the reason of refuse.
		/// </summary>
		Refused,

		/// <summary>
		/// Servers sends this command when client was dropped for some reason.
		/// Data contains the reason of the drop.
		/// </summary>
		Dropped,


		GameNotification,
		SystemNotification,
		ChallengeRequest,
		ChallengeResponse,
	}
}
