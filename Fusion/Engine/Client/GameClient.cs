using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Engine.Network;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Core.Content;


namespace Fusion.Engine.Client {

	/// <summary>
	/// Provides basic client-server interaction and client-side game logic.
	/// </summary>
	public abstract partial class GameClient : GameModule {

		public readonly Guid Guid;

		
		/// <summary>
		/// Gets Client's instance of content manager.
		/// </summary>
		public ContentManager Content { get { return content; }	}
		ContentManager content;

		/// <summary>
		/// Gets atoms collection.
		/// </summary>
		public AtomCollection Atoms { 
			get { 
				if (atoms==null) {
					throw new NullReferenceException("Atoms are ready to use at LoadContent");
				}
				return atoms; 
			} 
			internal set { 
				atoms = value; 
			}	
		}
		AtomCollection atoms = null;

		/// <summary>
		/// Gets current client state.
		/// </summary>
		public ClientState ClientState { get { return state.ClientState; } }


		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <param name="Game"></param>
		public GameClient ( Game game ) : base(game) 
		{
			Guid	=	Guid.NewGuid();
			content	=	new ContentManager(game);
			InitInternal();
		}



		/// <summary>
		/// Releases all resources used by the GameClient class.
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
		/// Called when connection request accepted by server.
		/// Method returns GameLoader that could load all content according server info.
		/// </summary>
		/// <param name="serverInfo"></param>
		public abstract GameLoader LoadContent ( string serverInfo );

		/// <summary>
		/// Called when GameLoader finished loading.
		/// This method lets client to complete loading process in main thread.
		/// Add mesh instances, sounds, setup sky, hdr etc in this method.
		/// </summary>
		/// <param name="serverInfo"></param>
		public abstract void FinalizeLoad ( GameLoader loader );

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	In most cases you need just to call Content.Unload().
		/// </summary>
		public abstract void UnloadContent ();

		/// <summary>
		/// Called when the game has determined that client-side logic needs to be processed.
		/// </summary>
		/// <param name="gameTime">Cliemt-side game time.</param>
		/// <param name="sentCommandID">Command's ID that are going to be sent.</param>
		/// <returns>User command bytes</returns>
		public abstract byte[] Update ( GameTime gameTime, uint sentCommandID );

		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// <remarks>Not all snapshot could reach client.</remarks>
		/// </summary>
		/// <param name="serverTime">Server time includes number of server frames, total server time and elapsed time since last server frame. 
		/// <param name="snapshotStream">Snapshot data stream.</param>
		/// <param name="ackCommandID">Acknoledged (e.g. received and responsed) command ID. Zero value means first snapshot.</param>
		public abstract void FeedSnapshot ( GameTime serverTime, byte[] snapshot, uint ackCommandID );

		/// <summary>
		/// Feed notification from server.
		/// </summary>
		/// <param name="message">Message from server</param>
		public abstract void FeedNotification ( string message );

		/// <summary>
		/// Gets user information. 
		/// Called when client-server game logic has determined that server needs user information.
		/// </summary>
		/// <returns>User information</returns>
		public abstract string UserInfo ();

		/// <summary>
		/// Sends server string message.
		/// This method may be used for chat 
		/// or remote server control throw Shell.
		/// </summary>
		/// <param name="message"></param>
		public void NotifyServer ( string message )
		{
			NotifyInternal(message);
		}

		/// <summary>
		/// Gets ping between client and server in seconds.
		/// If not connected return is undefined.
		/// </summary>
		public float Ping {
			get {
				return ping;
			}
		}
	}
}
