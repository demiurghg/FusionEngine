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

namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		NetChan netChan;

		List<SVClientState>	clients;



		/// <summary>
		/// Starts network stuff.
		/// </summary>
		void NetStart ()
		{
			clients	=	new List<SVClientState>();
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
			NetIMessage message = null;
			
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
			var buffer = new byte[1024];

			using ( var stream = new MemoryStream( buffer ) ) {
				
				Update( gameTime, stream );
			
				foreach ( var cl in clients ) {
					cl.SendSnapshot( stream );
				}
			}
		}



		/// <summary>
		/// Dispatches message.
		/// </summary>
		/// <param name="message"></param>
		void NetDispatchIM ( NetIMessage message )
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
		SVClientState GetClient( IPEndPoint ep )
		{
			return clients.FirstOrDefault( cl => cl.EndPoint.Equals( ep ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchUserCmd ( NetIMessage msg )
		{
			var cl = GetClient(msg.SenderEP);
			
			if (cl==null) {
				Log.Warning("Command from unregistered client: {0}", msg.SenderEP);
				return;
			}	

			cl.NeedSnapshot = true;

			/*using ( var reader = msg.OpenReader() ) {

				int count = reader.ReadInt32();

				var cmds = new UserCmd[count];

				for (int i=0; i<count; i++) {
					cmds[i] = UserCmd.Read( reader );
				}

				FeedCommand( cmds, cl.ID );
			} */
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="userInfo"></param>
		void NetRegisterClient ( NetIMessage msg )
		{
			var cl = GetClient( msg.SenderEP );

			if ( cl!=null ) {
				
				Log.Warning("Duplicate connect from {0}. Ignored.", msg.SenderEP);

			} else {
				
				// add client :
				var client = new SVClientState( this, msg.SenderEP, msg.Text );

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
		void NetUnregisterClient ( NetIMessage msg )
		{
			var client = clients.FirstOrDefault( cl => cl.EndPoint.Equals( msg.SenderEP ) );

			if ( client == null ) {
				
				Log.Warning("Duplicate disconnect from {0}. Ignored.", msg.SenderEP);

			} else {

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
			SVClientState client = null;

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
