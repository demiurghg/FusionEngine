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

namespace SceneDemo1 {


	class SceneDemo1GameInterface : Fusion.Engine.Common.GameInterface {

		[GameModule( "Console", "con", InitOrder.Before )]
		public GameConsole Console { get { return console; } }
		public GameConsole console;

		ViewLayer	master;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public SceneDemo1GameInterface ( GameEngine gameEngine )
			: base( gameEngine )
		{
			console = new GameConsole( gameEngine, "conchars", "conback" );
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			//	create view layer :
			master = new ViewLayer( GameEngine );

			var data = GameEngine.Content.Load<byte[]>("workspace");

			//Log.Dump(data);

			//	add view to layer to scene :
			GameEngine.GraphicsEngine.AddLayer( master );

			//	add console sprite layer to master view layer :
			master.SpriteLayers.Add( console.ConsoleSpriteLayer );
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref master );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			//	update console :
			console.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message( "DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}



		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowMessage ( string message )
		{
		}



		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowWarning ( string message )
		{
		}



		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowError ( string message )
		{
		}



		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ChatMessage ( string message )
		{
		}
	}
}
