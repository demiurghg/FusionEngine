using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Engine.Network {

	public enum NetChanMsgType : uint {

		/// <summary>
		/// Non reliable in-band data.
		/// </summary>
		IB_NonReliable,

		/// <summary>
		/// Reliable in-band data.
		/// </summary>
		IB_Reliable,

		/// <summary>
		/// Out-of-band string.
		/// </summary>
		OOB_StringData,

		/// <summary>
		/// Out-of-band binary data.
		/// </summary>
		OOB_BinaryData,

		/// <summary>
		/// Out-of-band broadcast
		/// </summary>
		OOB_Broadcast,

		/// <summary>
		/// Out-of-band discovery request
		/// </summary>
		OOB_DiscoveryRequest,

		/// <summary>
		/// Out-of-band discovery response.
		/// </summary>
		OOB_DiscoveryResponse,

	}
}
