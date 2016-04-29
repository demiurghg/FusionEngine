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
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;
using System.IO.Compression;
using System.IO;

namespace Fusion.Engine.Common {

	public class Network : GameModule {

		[Config]
		public int Port { get; set; }

		[Config]
		public int MaxClients { get; set; }

		[Config]
		public bool ShowPackets { get; set; }

		[Config]
		public float SimulatePacketsLoss { get; set; }

		[Config]
		public float SimulateMinLatency { get; set; }
		
		[Config]
		public float SimulateRandomLatency { get; set; }

		[Config]
		public bool ShowSnapshots { get; set; }

		[Config]
		public bool ShowUserCommands { get; set; }

		[Config]
		public bool ShowLatency { get; set; }


		public Network(Game game) : base(game)
		{
		}


		public override void Initialize ()
		{
			//	do nothing
		}
	}

}
