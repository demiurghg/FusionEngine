using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Fusion.Engine.Network {

	[Serializable]
	public class NetChanException : System.Exception {

		public NetChanException ()
		{
		}
		
		public NetChanException ( string message ) : base( message )
		{
		}

		public NetChanException ( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
