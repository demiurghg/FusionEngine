using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Client;
using Fusion.Engine.Common;
using Fusion.Engine.Server;

namespace $safeprojectname$ {
	class $safeprojectname$Server : GameServer {

		
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public $safeprojectname$Server ( Game game ) : base(game)
		{
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// Method is invoked when server started.
		/// </summary>
		/// <param name="map"></param>
		public override void LoadContent ( string map )
		{
			//	Load content for given map/level here.
			//	...
		}


		/// <summary>
		/// Method is invoked when server shuts down.
		/// This method will be also called when server crashes.
		/// </summary>
		public override void UnloadContent ()
		{
			//	Unload content for given map/level here.
			//	...
		}



		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <returns>Snapshot bytes</returns>
		public override byte[] Update ( GameTime gameTime )
		{
			Thread.Sleep(10);

			return Encoding.UTF8.GetBytes( "World: [" + string.Join( " | ", state.Select(s1=>s1.Value) ) + "]");
		}


		Dictionary<Guid, string> state = new Dictionary<Guid,string>();


		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( Guid id, byte[] userCommand, uint commandID, float lag )
		{
			state[id] = Encoding.UTF8.GetString( userCommand );
		}



		/// <summary>
		/// Feed server notification from particular client.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="message"></param>
		public override void FeedNotification ( Guid id, string message )
		{
			Log.Message("NOTIFICATION {0}: {1}", id, message );
		}



		/// <summary>
		/// Gets server information that required for client to load the game.
		/// This information usually contains map name and game type.
		/// This information is also used for discovery response.
		/// </summary>
		/// <returns></returns>
		public override string ServerInfo ()
		{
			return "Alice";
		}



		/// <summary>
		/// Notifies server that client connected.
		/// </summary>
		public override void ClientConnected ( Guid id, string userInfo )
		{
			NotifyClients("CONNECTED: {0} - {1}", id, userInfo);
			Log.Message("CONNECTED: {0} - {1}", id, userInfo);
			state.Add( id, " --- " );
		}


		public override void ClientActivated ( Guid id )
		{
			NotifyClients("ACTIVATED: {0}", id);
			Log.Message("ACTIVATED: {0}", id);
		}



		public override void ClientDeactivated ( Guid id )
		{
			NotifyClients("DEACTIVATED: {0}", id);
			Log.Message("DEACTIVATED: {0}", id);
		}



		/// <summary>
		/// Notifies server that client disconnected.
		/// </summary>
		public override void ClientDisconnected ( Guid id )
		{
			NotifyClients("DISCONNECTED: {0}", id );
			Log.Message("DISCONNECTED: {0}", id );
			state.Remove( id );
		}



		/// <summary>
		/// Approves client by id and user info.
		/// </summary>
		public override bool ApproveClient( Guid id, string userInfo, out string reason )
		{
			Log.Message("APPROVE: {0} {1}", id, userInfo );
			reason = "";
			return true;
		}
	}
}
