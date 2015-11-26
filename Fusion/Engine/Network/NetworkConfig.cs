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

namespace Fusion.Engine.Network {

	public class NetworkConfig {

		public int Port { get; set; }
		public int MaxClients { get; set; }

		public bool ShowPackets { get; set; }

		public bool ShowCompression { get; set; }

		public float SimulatePacketsLoss { get; set; }


		/// <summary>
		/// Client connection resend timeout.
		/// </summary>
		public int ResendTimeout { get; set; }

		/// <summary>
		/// Client connection max resend count.
		/// </summary>
		public int ResendMaxCount { get; set; }


		public NetworkConfig ()
		{
			Port				=	28100;
			MaxClients			=	8;
			SimulatePacketsLoss	=	0;
			ShowPackets			=	false;
			ShowCompression		=	false;

			ResendTimeout		=	1000;
			ResendMaxCount		=	10;
		}

	}
}
