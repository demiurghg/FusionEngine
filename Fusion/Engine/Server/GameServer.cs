using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Core.Shell;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Core.Content;


namespace Fusion.Engine.Server {

	/// <summary>
	/// Provides basic client-server interaction and server-side game logic.
	/// </summary>
	public abstract partial class GameServer : GameModule {

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="Game"></param>
		public GameServer ( Game game ) : base(game)
		{
			content = new ContentManager(game);
			atoms	= new AtomCollection();
		}


		/// <summary>
		/// Gets server's instance of content manager.
		/// </summary>
		public ContentManager Content {
			get {
				return content;
			}
		}

		ContentManager content;


		/// <summary>
		/// Gets atom collections.
		/// </summary>
		public AtomCollection Atoms {
			get {
				if (atoms==null) {
					throw new NullReferenceException("Atoms are ready to use at LoadContent");
				}
				return atoms;
			}
		}

		AtomCollection atoms;


		/// <summary>
		/// Gets and sets target server frame rate.
		/// Value must be within range 1..240.
		/// </summary>
		public float TargetFrameRate {
			get { return targetFrameRate; }
			set {
				if (value<1 || value>240) {
					throw new ArgumentOutOfRangeException("value", "Value must be within range 1..240.");
				}
				targetFrameRate	=	value;
			}
		}
		float targetFrameRate = 60;


		/// <summary>
		/// Releases all resources used by the GameServer class.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref content );
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// Method is invoked when server started.
		/// </summary>
		/// <param name="map"></param>
		public abstract void LoadContent ( string map );

		/// <summary>
		/// Method is invoked when server shuts down.
		/// This method will be also called when server crashes.
		/// </summary>
		/// <param name="map"></param>
		public abstract void UnloadContent ();

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// <remarks>Due to delta compression of snapshot keep data aligned. 
		/// Even small layout change will cause significiant increase of sending data</remarks>
		/// </summary>
		/// <param name="gameTime"></param>
		/// <returns>Snapshot bytes</returns>
		public abstract byte[] Update ( GameTime gameTime );

		/// <summary>
		/// Feed server with commands from particular client.
		/// </summary>
		/// <param name="clientGuid">Client's GUID</param>
		/// <param name="userCommand">Client's user command bytes</param>
		/// <param name="commandID">Client's user command index</param>
		/// <param name="lag">Lag in seconds</param>
		public abstract void FeedCommand ( Guid clientGuid, byte[] userCommand, uint commandID, float lag );

		/// <summary>
		/// Feed server with commands from particular client.
		/// </summary>
		/// <param name="clientGuid">Client's GUID</param>
		/// <param name="command">Client's user command stream</param>
		public abstract void FeedNotification ( Guid clientGuid, string message );

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
		/// <param name="clientGuid">Client GUID.</param>
		/// <param name="userInfo">User information. Cann't be used as client identifier.</param>
		public abstract void ClientConnected ( Guid clientGuid, string userInfo );

		/// <summary>
		/// Called when client received snapshot and ready to play.
		/// </summary>
		/// <param name="clientGuid">Client GUID.</param>
		/// <param name="userInfo">User information. Cann't be used as client identifier.</param>
		public abstract void ClientActivated ( Guid clientGuid );

		/// <summary>
		/// Called when before disconnect.
		/// </summary>
		/// <param name="clientGuid">Client GUID.</param>
		/// <param name="userInfo">User information. Cann't be used as client identifier.</param>
		public abstract void ClientDeactivated ( Guid clientGuid );

		/// <summary>
		/// Called when client disconnected.
		/// </summary>
		/// <param name="clientGuid">Client IP in format 123.45.67.89:PORT. Could be used as client identifier.</param>
		public abstract void ClientDisconnected ( Guid clientGuid );

		/// <summary>
		/// Approves client by id and user information.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userInfo"></param>
		/// <param name="reason">If method returns false this output parameters contains the reason of denial</param>
		/// <returns></returns>
		public abstract bool ApproveClient ( Guid guid, string userInfo, out string reason );

		/// <summary>
		/// Sends text message to all clients.
		/// </summary>
		/// <param name="message"></param>
		public void NotifyClients ( string format, params object[] args )
		{
			NotifyClientsInternal( string.Format(format, args) );
		}


		/// <summary>
		/// Gets ping time to particular client.
		/// </summary>
		/// <param name="clientGuid"></param>
		/// <returns></returns>
		public float GetPing ( Guid clientGuid )
		{
			return GetPingInternal( clientGuid );
		}


		/// <summary>
		/// Gets ping time to particular client.
		/// </summary>
		/// <param name="clientGuid"></param>
		/// <returns></returns>
		public float GetIP ( Guid clientGuid )
		{
			throw new NotImplementedException();
		}
	}
}
