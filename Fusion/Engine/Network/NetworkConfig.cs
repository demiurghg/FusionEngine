using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Lidgren.Network;


namespace Fusion.Engine.Network {

	public class NetworkConfig {

		public int Port { get; set; }
		public int MaxClients { get; set; }

		public bool ShowPackets { get; set; }

		public float SimulatePacketsLoss { get; set; }

		public float SimulateMinLatency { get; set; }
		public float SimulateRandomLatency { get; set; }

		public bool ShowSnapshots { get; set; }

		public bool ShowJitter { get; set; }


		public NetworkConfig ()
		{
			/*var cfg = new NetPeerConfiguration("");
			cfg.*/

			Port				=	28100;
			MaxClients			=	8;
			SimulatePacketsLoss	=	0;
			ShowPackets			=	false;
			ShowJitter			=	false;
		}

	}
}
