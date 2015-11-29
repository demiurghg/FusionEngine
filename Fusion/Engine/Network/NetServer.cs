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


		/// <summary>
		/// Create and starts new server instance.
		/// </summary>
		/// <param name="port">Port to listen.</param>
		/// <param name="serverInfo">Server information string.</param>
		public NetServer ( int port, string serverInfo, int timeout )
		{
			//socket			=	new Socket( SocketType.Dgram, ProtocolType.Udp );
			//socket.Blocking	=	false;

			//var	localEP		=	new IPEndPoint( IPAddress.Any, port );

			//Log.Message("NetServer: Local EP: {0}", localEP.ToString() );

			//socket.Bind( localEP );
		}



		/// <summary>
		/// Disposes server.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if (disposing) {
			}

 			base.Dispose(disposing);
		}


		/// <summary>
		/// Adds new snapshot.
		/// </summary>
		/// <param name="snapshot"></param>
		public void PushSnapshot ( byte[] snapshot )
		{
		}


		/// <summary>
		/// Pops user commands.
		/// </summary>
		/// <param name="clientId">Client's ID.</param>
		/// <param name="userCommand">User command data.</param>
		/// <returns>True if new user commands is available. False otherwise.</returns>
		public bool GetUserCommand ( out string clientId, byte[] userCommand )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool PopMessage ( out string clientId, out string message )
		{
		}



		/// <summary>
		/// Sends relieble message to client.
		/// </summary>
		/// <param name="clientId"></param>
		public void NotifyClient ( string clientId, string message )
		{
		}


		/// <summary>
		/// Drops connection to client.
		/// </summary>
		/// <param name="clientId"></param>
		public void Drop ( string clientId )
		{
		}

	}
}
