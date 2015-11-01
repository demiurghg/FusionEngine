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
			foreach ( var cl in clients ) {
				netChan.OutOfBand( cl.EndPoint, NetCommand.ServerDisconnected );
			}

			SafeDispose( ref netChan );
		}



		/// <summary>
		/// 
		/// </summary>
		void NetDispatchIM ( GameTime gameTime )
		{
			NetMessage message = null;

			while ( netChan.Dispatch( out message ) ) {

				NetDispatchIM(message);
				
			} 
		}



		/// <summary>
		/// Dispatches out-of-band (e.g. service) messages.
		/// </summary>
		/// <param name="message"></param>
		void NetDispatchIM ( NetMessage message )
		{
			switch ( message.Header.Command ) {
				case NetCommand.Connect		: NetRegClient( message );		break;
				case NetCommand.Disconnect	: NetUnregClient( message );	break;
			}
		}



		void NotifyClients ( string message )
		{
			foreach ( var cl in clients ) {
				netChan.OutOfBand( cl.EndPoint, NetCommand.Notification, message );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetRegClient ( NetMessage msg )
		{
			if ( clients.Any( cl => cl.EndPoint.Equals( msg.SenderEP ) ) ) {
				
				Log.Warning("Duplicate connect from {0}. Ignored.", msg.SenderEP);

			} else {
				
				// add client :
				var client = new ClientDesc( msg.SenderEP, msg.Text );

				clients.Add( client );

				//	send accept :
				netChan.OutOfBand(msg.SenderEP, NetCommand.Accepted, ServerInfo() );

				//Log.Message("Client connected: {0}", userInfo );

				//	notify game about new client.
				ClientConnected( msg.Address, msg.Text );

				NotifyClients( string.Format("{0} connected.", client.UserInfo) );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetUnregClient ( NetMessage msg )
		{
			var client = clients.FirstOrDefault( cl => cl.EndPoint.Equals( msg.SenderEP ) );

			if ( client == null ) {
				
				Log.Warning("Duplicate disconnect from {0}. Ignored.", msg.SenderEP);

			} else {

				clients.Remove( client );

				ClientDisconnected( msg.SenderEP.Address.ToString() + ":" + msg.SenderEP.Port.ToString() );

				NotifyClients( string.Format("{0} disconnected.", client.UserInfo) );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void PrintServerInfo ()
		{
			Log.Message("");
			Log.Message("-------- Server Info --------");
			Log.Message("{0}", ServerInfo() );

			foreach ( var cl in clients ) {
				Log.Message("  {1} : {0}", cl.UserInfo, cl.EndPoint.ToString() );
			}

			Log.Message("{0} clients are connected", clients.Count );
			Log.Message("-----------------------------");
			Log.Message("");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientName"></param>
		/// <param name="reason"></param>
		internal void Drop ( string clientName, string reason )
		{
			IPAddress ip;
			ClientDesc client = null;

			if ( IPAddress.TryParse( clientName, out ip ) ) {
				client	=	clients.FirstOrDefault( cl => cl.EndPoint.Address.Equals( ip ) );
			} else {
				client = clients.FirstOrDefault( cl => cl.UserInfo.Contains( clientName ) );
			}

			if (client==null) {
				Log.Warning("No such client: {0}", client );
			} else {

				clients.Remove( client );

				netChan.OutOfBand( client.EndPoint, NetCommand.Dropped, reason );

				NotifyClients( string.Format("{0} was dropped.", client.UserInfo) );

			}
		}

	}
}
