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
using Fusion.Core.Content;
using Fusion.Engine.Graphics;

namespace ShooterDemo {
	partial class ShooterServer : GameServer {

		string mapName;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public ShooterServer ( Game game )
			: base( game )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Releases all resources used by the GameServer class.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				//	...
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Method is invoked when server started.
		/// </summary>
		/// <param name="map"></param>
		public override void LoadContent ( string map )
		{
			mapName	=	@"scenes\" + map;

			var scene = Content.Load<Scene>( mapName );

			InitializeStaticPhysWorld( scene );
		}



		/// <summary>
		/// Method is invoked when server shuts down.
		/// This method will be also called when server crashes.
		/// </summary>
		public override void UnloadContent ()
		{
			mapName		=	null;
			Content.Unload();
		}



		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <returns>Snapshot bytes</returns>
		public override byte[] Update ( GameTime gameTime )
		{
			Thread.Sleep( 10 );

			return Encoding.UTF8.GetBytes( "World: [" + string.Join( " | ", state.Select( s1 => s1.Value.ToString() ) ) + "]" );
		}


		Dictionary<string, object> state = new Dictionary<string, object>();


		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( string id, byte[] userCommand )
		{
			if (!userCommand.Any()) {
				return;
			}

			var userCmd = UserCommand.FromBytes( userCommand );
			state[id] = userCmd;
		}



		/// <summary>
		/// Feed server notification from particular client.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="message"></param>
		public override void FeedNotification ( string id, string message )
		{
			Log.Message( "NOTIFICATION {0}: {1}", id, message );
		}



		/// <summary>
		/// Gets server information that required for client to load the game.
		/// This information usually contains map name and game type.
		/// This information is also used for discovery response.
		/// </summary>
		/// <returns></returns>
		public override string ServerInfo ()
		{
			return mapName;
		}



		/// <summary>
		/// Notifies server that client connected.
		/// </summary>
		public override void ClientConnected ( string id, string userInfo )
		{
			NotifyClients( "CONNECTED: {0} - {1}", id, userInfo );
			Log.Message( "CONNECTED: {0} - {1}", id, userInfo );
			state.Add( id, " --- " );
		}



		/// <summary>
		/// Notifies server that client disconnected.
		/// </summary>
		public override void ClientDisconnected ( string id, string userInfo )
		{
			NotifyClients( "DISCONNECTED: {0} - {1}", id, userInfo );
			Log.Message( "DISCONNECTED: {0} - {1}", id, userInfo );
			state.Remove( id );
		}



		/// <summary>
		/// Approves client by id and user info.
		/// </summary>
		public override bool ApproveClient ( string id, string userInfo, out string reason )
		{
			Log.Message( "APPROVE: {0} {1}", id, userInfo );
			reason = "";
			return true;
		}
	}
}
