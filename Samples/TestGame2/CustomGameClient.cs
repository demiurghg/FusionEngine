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
using System.Diagnostics;
using System.Threading;
using Fusion.Core.Shell;
using Fusion.Core.Configuration;


namespace TestGame2 {

	public class Config {
		public bool Show { get; set; }
	}


	class CustomGameClient : GameClient {

		[Config]
		public Config Config { get; set; }
		

		[Command("chat", CommandAffinity.Client)]
		public class Chat : NoRollbackCommand {

			[CommandLineParser.Required]
			public List<string> Messages { get; set; }

			public Chat ( Invoker invoker ) : base(invoker)
			{
				Messages = new List<string>();
			}

			public override void Execute ()
			{
				Invoker.Game.GameClient.NotifyServer("chat:" + string.Join(" ", Messages));
			}
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameClient ( Game game )	: base(game)
		{
			Config	= new Config();
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
			Log.Message("LOAD LEVEL: {0}", serverInfo);
			Thread.Sleep(100);
			Log.Message("LOAD LEVEL COMPLETED!");
			return null;
		}

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
		public override byte[] Update ( GameTime gameTime, uint ackCommandID )
		{
			var mouse = Game.Mouse;
			
			return Encoding.UTF8.GetBytes( string.Format("[{0} {1} {2}]", mouse.Position.X, mouse.Position.Y, UserInfo() ) );
		}


		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedSnapshot ( GameTime svTime, byte[] snapshot, uint ackCommandID ) 
		{
			var str = Encoding.UTF8.GetString( snapshot );
			if (Config.Show) {
				Log.Message("FOOD : {0}", str);
			}
		}


		public override void FeedNotification ( string message )
		{
			Log.Message("NOTIFICATION : {0}", message );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string UserInfo ()
		{
			return "Bob" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
		}
	}
}
