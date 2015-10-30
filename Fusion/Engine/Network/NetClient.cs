using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Fusion.Core;

namespace Fusion.Engine.Network {
	public class NetClient : DisposableBase {

		Socket		socket;
		EndPoint	endPoint;	
		readonly string host;
		readonly int	port;



		/// <summary>
		/// 
		/// </summary>
		public NetClient ( string host, int port )
		{
			this.host	=	host;
			this.port	=	port;

			var ipAddr	=	IPAddress.Parse( host );

			endPoint	=	new IPEndPoint( ipAddr, port );

			socket			=	new Socket( SocketType.Dgram, ProtocolType.Udp );
			//socket.Bind( localEP );
			socket.Blocking	=	false;
		}



		public void SendMessage ( string message )
		{
			var bytes = Encoding.ASCII.GetBytes(message);

			socket.SendTo( bytes, endPoint );

			Log.Message("...sent: {0}", message );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref socket );
			}

			base.Dispose( disposing );
		}	
	}
}
