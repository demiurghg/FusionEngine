using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Common {

	/// <summary>
	/// 
	/// </summary>
	public enum ConnState {
		Standby,
		Disconnected,
		Connecting,
		Connected,
		Active,
	}



	public enum GameServerState {
		Standby,
		Initializing,
		Active,
	}
}
