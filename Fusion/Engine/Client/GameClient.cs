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


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameClient ( GameEngine gameEngine ) : base(gameEngine) 
		{
			InitInternal();
		}


		protected override void Dispose ( bool disposing )
		{
			base.Dispose( disposing );
		}


		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// The method can be invoked in parallel task.
		/// Thus this method should not setup scene.
		/// </summary>
		/// <param name="host"></param>
		public abstract void LoadContent ( string serverInfo );

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		/// </summary>
		public abstract void UnloadContent ();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public abstract void GameEnd ( string message );

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <returns>User command bytes</returns>
		public abstract byte[] Update ( GameTime gameTime );

		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshotStream">Snapshot data stream.</param>
		/// <param name="initial">Indicates that this is first snapshot.
		/// Client module may setup graphic scene here. 
		/// </param>
		public abstract void FeedSnapshot ( byte[] snapshot, bool initial );

		/// <summary>
		/// Feed notification from server.
		/// </summary>
		/// <param name="message"></param>
		public abstract void FeedNotification ( string message );

		/// <summary>
		/// Gets user information.
		/// </summary>
		/// <returns>User information</returns>
		public abstract string UserInfo ();


		/// <summary>
		/// Sends server string message.
		/// This method may be used for chat.
		/// </summary>
		/// <param name="message"></param>
		public void NotifyServer ( string message )
		{
			NotifyInternal(message);
		}
	}
}
