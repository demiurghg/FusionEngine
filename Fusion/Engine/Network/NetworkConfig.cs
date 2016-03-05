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

		public bool ShowLatency { get; set; }

		public bool ShowJitter { get; set; }

		/// <summary>
		/// PlayoutDelay affect incoming snapshot de-jittering.
		/// Higher values lead to more latency and better smoothiness.
		/// Lower values lead to less latency and better responsiveness.
		/// </summary>
		public int PlayoutDelay { get; set; }


		public NetworkConfig ()
		{
			Port				=	28100;
			MaxClients			=	8;
			SimulatePacketsLoss	=	0;
			ShowPackets			=	false;
			ShowJitter			=	false;
			PlayoutDelay		=	30;
			ShowLatency			=	false;
		}

	}
}
