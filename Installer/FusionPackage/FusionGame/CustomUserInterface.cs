using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Framework;
using Fusion;
using Fusion.Engine.Client;
using Fusion.Engine.Common;
using Fusion.Engine.Server;

namespace $safeprojectname$ {


	class $safeprojectname$Interface : UserInterface {

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


		/// <summary>
		/// Creates instance of $safeprojectname$Interface
		/// </summary>
		/// <param name="engine"></param>
		public $safeprojectname$Interface ( Game game ) : base(game)
		{
			console		=	new GameConsole( game, "conchars");
		}



		/// <summary>
		/// Called after the $safeprojectname$Interface is created,
		/// </summary>
		public override void Initialize ()
		{
			//	add console sprite layer to master view layer :
			Game.RenderSystem.RenderWorld.SpriteLayers.Add( console.ConsoleSpriteLayer );
		}



		/// <summary>
		/// Overloaded. Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Called when the game has determined that UI logic needs to be processed.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			//	update console :
			console.Update( gameTime );
		}



		/// <summary>
		/// Called when user closes game window using Close button or Alt+F4.
		/// </summary>
		public override void RequestToExit ()
		{
			Game.Exit();
		}



		/// <summary>
		/// Called when discovery respone arrives.
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}
	}
}
