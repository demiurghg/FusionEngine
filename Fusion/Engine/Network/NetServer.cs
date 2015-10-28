using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Fusion.Core;

namespace Fusion.Engine.Network {
	public class NetServer : DisposableBase {

		public const int MTU	=	1408;

		Socket	socket;


		byte[]	recvBuffer	=	new byte[MTU];



		/// <summary>
		/// 
		/// </summary>
		public NetServer ( int port )
		{
			socket			=	new Socket( SocketType.Dgram, ProtocolType.Udp );
			socket.Blocking	=	false;

			var	localEP		=	new IPEndPoint( IPAddress.Any, port );

			Log.Message("NetServer: Local EP: {0}", localEP.ToString() );

			socket.Bind( localEP );
		}



		/// <summary>
		/// 
		/// </summary>
		public void GetMessages ()
		{
			try {
				while (true) {

					EndPoint clientEP	=	new IPEndPoint( IPAddress.Any, 0 );

					int size		=	socket.ReceiveFrom(	recvBuffer, SocketFlags.None, ref clientEP );

					string msg		=	Encoding.ASCII.GetString( recvBuffer, 0, size );

					Log.Message("...recv: [{0}] {1} {2}", size, msg, clientEP.ToString() );
				}

			} catch ( SocketException se ) {
				if (se.SocketErrorCode==SocketError.WouldBlock) {
					//	that is ok, there are no messages.
					return;
				}
				Log.Error("Socket error: {0}", se.SocketErrorCode );
			}
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
