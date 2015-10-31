using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Lidgren.Network;
using System.Threading;
using System.Net;
using Fusion.Engine.Network;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		NetChan netChan;


		class ClientDesc {
			
			public ClientDesc ( IPEndPoint ep, string userInfo )
			{
				EndPoint	= ep;
				UserInfo	= userInfo;
			}

			public IPEndPoint	EndPoint;
			public string		UserInfo;
		}


		List<ClientDesc>	clients;



		/// <summary>
		/// 
		/// </summary>
		void NetStart ()
		{
			clients	=	new List<ClientDesc>();
			netChan =	new NetChan(GameEngine, GameEngine.Network.ServerSocket, "SV");
		}



		/// <summary>
		/// 
		/// </summary>
		void NetShutdown()
		{
			SafeDispose( ref netChan );
		}



		/// <summary>
		/// 
		/// </summary>
		void NetDispatchIM ( GameTime gameTime )
		{
			NetMessage message = null;

			while ( netChan.Dispatch( out message ) ) {

				if (message.Header.IsOutOfBand) {

					NetDispatchOobIM(message);

				} else {
					//	...
				}
				
			} 
		}



		/// <summary>
		/// Dispatches out-of-band (e.g. service) messages.
		/// </summary>
		/// <param name="message"></param>
		void NetDispatchOobIM ( NetMessage message )
		{
			var args = CommandLineParser.SplitCommandLine( message.GetString() ).ToArray();

			if (args[0]==Protocol.ClientConnect) {
				NetRegClient( message, args[1] );
			} else
			if (args[0]==Protocol.ClientDisconnect) {
				NetUnregClient( message );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetRegClient ( NetMessage msg, string userInfo )
		{
			if ( clients.Any( cl => cl.EndPoint == msg.SenderEP ) ) {
				
				Log.Warning("Duplicate connect from {0}. Ignored.", msg.SenderEP);

			} else {
				
				// add client :
				clients.Add( new ClientDesc( msg.SenderEP, userInfo ) );

				//	send ack :
				netChan.OutOfBand(msg.SenderEP, Protocol.ServerConnectAck);
				Log.Message("Client connected: {0}", userInfo );

				//	notify game about new client.
				ClientConnected( msg.SenderEP.Address.ToString() + ":" + msg.SenderEP.Port.ToString(), userInfo );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetUnregClient ( NetMessage msg )
		{
			if ( !clients.Any( cl => cl.EndPoint == msg.SenderEP ) ) {
				
				Log.Warning("Duplicate disconnect from {0}. Ignored.", msg.SenderEP);

			} else {

				clients.RemoveAll( cl => cl.EndPoint == msg.SenderEP );

				ClientDisconnected( msg.SenderEP.Address.ToString() + ":" + msg.SenderEP.Port.ToString() );
			}
		}



	}
}
