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
			state	=	new STStandBy(this);
		}

		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// The method can be invoked in parallel task.
		/// </summary>
		/// <param name="host"></param>
		public abstract void LoadLevel ( string serverInfo );

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		/// </summary>
		public abstract void UnloadLevel ();

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// </summary>
		/// <param name="gameTime">Game time.</param>
		/// <param name="commandStream">Stream to write user's command. 
		/// Should not exceed 1024 bytes. Good size for user commands is about 8-12 bytes.</param>
		public abstract void Update ( GameTime gameTime, Stream commandStream );

		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshotStream">Snapshot data stream.</param>
		public abstract void FeedSnapshot ( Stream snapshotStream );

		/// <summary>
		/// Gets user information.
		/// </summary>
		/// <returns>User information</returns>
		public abstract string UserInfo ();
	}
}
