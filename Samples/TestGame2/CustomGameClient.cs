using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;


namespace TestGame2 {
	class CustomGameClient : GameClient {

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameClient ( GameEngine gameEngine )	: base(gameEngine)
		{
		}


		/// <summary>
		/// Initializes game
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// </summary>
		/// <param name="map"></param>
		public override void LoadLevel ( string serverInfo )
		{
		}

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		public override void UnloadLevel ()
		{
		}

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// Do not close the stream.
		/// </summary>
		/// <param name="gameTime"></param>
		public override byte[] Update ( GameTime gameTime )
		{
			var mouse = GameEngine.Mouse;
			
			return Encoding.UTF8.GetBytes( string.Format("[{0} {1}]", mouse.Position.X, mouse.Position.Y ) );
		}


		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedSnapshot ( byte[] snapshot ) 
		{
			var str = Encoding.UTF8.GetString( snapshot );
			Log.Message("CL : {0}", str);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string UserInfo ()
		{
			return "Bob";
		}
	}
}
