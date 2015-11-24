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
using System.IO;
using Fusion.Engine.Common;


namespace Fusion.Engine.Server {

	public abstract partial class GameServer : GameModule {

		NetChan netChan;

		List<GameClientState>	clients;



		/// <summary>
		/// Starts network stuff.
		/// </summary>
		void NetStart ()
		{
			clients	=	new List<GameClientState>();
			netChan =	new NetChan(GameEngine, GameEngine.Network.ServerSocket, "SV");
		}



		/// <summary>
		/// Shuts down network stuff.
		/// </summary>
		void NetShutdown()
		{
			foreach ( var cl in clients ) {
				netChan.OutOfBand( cl.EndPoint, NetCommand.ServerDisconnected );
			}

			SafeDispose( ref netChan );
		}



		/// <summary>
		/// Dispatches incoming messages.
		/// </summary>
		void NetUpdate ( GameTime gameTime )
		{
			NetMessage message = null;
			
			//
			//	get clients' packets :
			//
			while ( netChan.Dispatch( out message ) ) {
				if (message==null) continue;
				NetDispatchIM(message);
			} 


			//
			//	update and feed clients back :
			//	
			var snapshot = Update( gameTime );

			foreach ( var cl in clients ) {
				cl.SendSnapshot( snapshot, snapshot.Length );
			}
		}



		/// <summary>
		/// Dispatches message.
		/// </summary>
		/// <param name="message"></param>
		void NetDispatchIM ( NetMessage message )
		{
			if (message.Header.IsOutOfBand) {

				switch ( message.Header.Command ) {
					case NetCommand.Connect		: NetRegisterClient( message );		break;
					case NetCommand.Disconnect	: NetUnregisterClient( message );	break;
				}

			} else {
				switch ( message.Header.Command ) {
					case NetCommand.UserCommand	: DispatchUserCmd( message );		break;
				}				
			}
		}



		/// <summary>
		/// Notifies all clients.
		/// </summary>
		/// <param name="message"></param>
		public void NotifyClientsInternal ( string message )
		{
			foreach ( var cl in clients ) {
				netChan.OutOfBand( cl.EndPoint, NetCommand.Notification, message );
			}
		}



		/// <summary>
		/// Gets client by EndPoint.
		/// </summary>
		/// <param name="ep"></param>
		/// <returns></returns>
		GameClientState GetClient( IPEndPoint ep )
		{
			return clients.FirstOrDefault( cl => cl.EndPoint.Equals( ep ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchUserCmd ( NetMessage msg )
		{
			var cl = GetClient(msg.SenderEP);
			
			if (cl==null) {
				Log.Warning("Command from unregistered client: {0}", msg.SenderEP);
				return;
			}	

			cl.NeedSnapshot = true;

			using ( var stream = new MemoryStream(msg.Data) ) {
				using ( var reader = new BinaryReader(stream) ) {
				
					int commandCounter	=	reader.ReadInt32();
					int length			=	reader.ReadInt32();

					var userCmd			=	reader.ReadBytes( length );

					FeedCommand( cl.ID, userCmd );
				}
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetRegisterClient ( NetMessage msg )
		{
			var cl = GetClient( msg.SenderEP );

			if ( cl!=null ) {
				
				//	probably accept commands has not been received.
				//	send it again.
				netChan.OutOfBand(msg.SenderEP, NetCommand.Accepted, ServerInfo() );

			} else {
				
				// add client :
				var client = new GameClientState( netChan, this, msg.SenderEP, msg.Text );

				netChan.Add( msg.SenderEP );
				clients.Add( client );

				//	send accept :
				netChan.OutOfBand(msg.SenderEP, NetCommand.Accepted, ServerInfo() );

				//	notify game about new client.
				ClientConnected( client.ID, client.UserInfo );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetUnregisterClient ( NetMessage msg )
		{
			var client = clients.FirstOrDefault( cl => cl.EndPoint.Equals( msg.SenderEP ) );

			if ( client == null ) {
				
				//Log.Warning("Duplicate disconnect from {0}. Ignored.", msg.SenderEP);

			} else {

				netChan.Remove( msg.SenderEP );
				clients.Remove( client );

				ClientDisconnected( client.ID, client.UserInfo );
			}
		}



		/// <summary>
		/// Prints server info.
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
		/// Drops client.
		/// </summary>
		/// <param name="clientName"></param>
		/// <param name="reason"></param>
		internal void Drop ( string clientName, string reason )
		{
			IPAddress ip;
			GameClientState client = null;

			if ( IPAddress.TryParse( clientName, out ip ) ) {
				client	=	clients.FirstOrDefault( cl => cl.EndPoint.Address.Equals( ip ) );
			} else {
				client = clients.FirstOrDefault( cl => cl.UserInfo.Contains( clientName ) );
			}

			if (client==null) {
				Log.Warning("No such client: {0}", client );
			} else {

				netChan.Remove( client.EndPoint );
				clients.Remove( client );

				netChan.OutOfBand( client.EndPoint, NetCommand.Dropped, reason );

				NotifyClients( string.Format("{0} was dropped.", client.UserInfo) );

			}
		}

	}
}
