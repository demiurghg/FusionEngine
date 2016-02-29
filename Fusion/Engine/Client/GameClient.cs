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
	public abstract partial class GameClient : GameModule {

		public readonly Guid Guid;

		
		/// <summary>
		/// Gets Client's content manager.
		/// </summary>
		public ContentManager Content { get { return content; }	}
		ContentManager content;


		/// <summary>
		/// Gets client state.
		/// </summary>
		public ClientState ClientState { get { return state.ClientState; } }


		/// <summary>
		/// 
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
		/// TimeNudge affect incoming snapshot de-jittering.
		/// Higher values lead to more latency and better smoothiness.
		/// Lower values lead to less latency and better responsiveness.
		/// </summary>
		public int TimeNudge {
			get { return timeNudge; }
			set { 
				if (value<-30 || value>30) throw new ArgumentOutOfRangeException("value", "TimeNudge must be within -30..30 range");
				timeNudge = value; 
			}
		}
		int timeNudge = 0;


		/// <summary>
		/// Called when connection request accepted by server.
		/// Method returns GameLoader
		/// </summary>
		/// <param name="serverInfo"></param>
		public abstract GameLoader LoadContent ( string serverInfo );

		/// <summary>
		/// Called when loader finished loading.
		/// This method lets client to complete loading process in main thread.
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
		/// </summary>
		/// <param name="snapshotStream">Snapshot data stream.</param>
		/// <param name="ackCommandID">Acknoledged (e.g. received and responsed) command ID. Zero value means first snapshot.</param>
		public abstract void FeedSnapshot ( byte[] snapshot, uint ackCommandID );

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
	}
}
