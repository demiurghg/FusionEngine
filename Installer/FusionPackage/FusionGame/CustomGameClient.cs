using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;


namespace $safeprojectname$ {
	class $safeprojectname$Client : GameClient {

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public $safeprojectname$Client ( Game game )	: base(game)
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
		public override GameLoader LoadContent ( string serverInfo )
		{
			return new $safeprojectname$Loader( this, serverInfo );
		}

		

		/// <summary>
		/// Called when loader completes loading.
		/// </summary>
		/// <param name="map"></param>
		public override void FinalizeLoad ( GameLoader loader )
		{
		}


		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		public override void UnloadContent ()
		{
		}



		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// Do not close the stream.
		/// </summary>
		/// <param name="gameTime"></param>
		public override byte[] Update ( GameTime gameTime )
		{
			var mouse = Game.Mouse;
			
			return Encoding.UTF8.GetBytes( string.Format("[{0} {1} {2}]", mouse.Position.X, mouse.Position.Y, UserInfo() ) );
		}



		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedSnapshot ( byte[] snapshot, bool initial ) 
		{
			var str = Encoding.UTF8.GetString( snapshot );
			Log.Message("SNAPSHOT : {0}", str);
		}



		/// <summary>
		/// Feed server notification to client.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedNotification ( string message )
		{
			Log.Message("NOTIFICATION : {0}", message );
		}



		/// <summary>
		/// Returns user informations.
		/// </summary>
		/// <returns></returns>
		public override string UserInfo ()
		{
			return "Bob" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
		}
	}
}
