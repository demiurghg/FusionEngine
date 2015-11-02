using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameServer ( GameEngine gameEngine ) : base(gameEngine)
		{
		}

		/// <summary>
		/// Method is invoked when server started.
		/// </summary>
		/// <param name="map"></param>
		public abstract void LoadLevel ( string map );

		/// <summary>
		/// Method is invoked when server shuts down.
		/// This method will be also called when server crashes to clean-up.
		/// </summary>
		/// <param name="map"></param>
		public abstract void UnloadLevel ();

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public abstract byte[] GetSnapshot ();

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public abstract void FeedCommand ( UserCmd[] commands, string id );

		/// <summary>
		/// Gets server information that required for client to load the game.
		/// This information usually contains map name and game type.
		/// This information is also used for discovery response.
		/// </summary>
		/// <returns></returns>
		public abstract string ServerInfo ();

		/// <summary>
		/// Called when client connected.
		/// </summary>
		/// <param name="clientIP">Client IP in format 123.45.67.89:PORT. Could be used as client identifier.</param>
		/// <param name="userInfo">User information. Cann't be used as client identifier.</param>
		public abstract void ClientConnected ( string id, string userInfo );

		/// <summary>
		/// Called when client connected.
		/// </summary>
		/// <param name="clientIP">Client IP in format 123.45.67.89:PORT. Could be used as client identifier.</param>
		public abstract void ClientDisconnected ( string id, string userInfo );

		/// <summary>
		/// Sends text message to all clients.
		/// </summary>
		/// <param name="message"></param>
		public void NotifyClients ( string format, params object[] args )
		{
			NotifyClientsInternal( string.Format(format, args) );
		}
	}
}
