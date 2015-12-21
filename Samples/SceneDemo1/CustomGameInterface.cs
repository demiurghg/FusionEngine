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


	class SceneDemo1GameInterface : Fusion.Engine.Common.UserInterface {

		[GameModule( "Console", "con", InitOrder.Before )]
		public GameConsole Console { get { return console; } }
		public GameConsole console;

		ViewLayerHdr	master;
		Scene			scene;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public SceneDemo1GameInterface ( Game game )
			: base( game )
		{
			console = new GameConsole( game, "conchars", "conback" );
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			//	create view layer :
			master = new ViewLayerHdr( Game, 0, 0 );

			//	add view to layer to scene :
			Game.RenderSystem.AddLayer( master );

			//	add console sprite layer to master view layer :
			master.SpriteLayers.Add( console.ConsoleSpriteLayer );

			//	load content and scubscribe on content reload.
			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}


		
		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			scene	=	Game.Content.Load<Scene>(@"Scenes\testScene");

			//master.Instances.Add( new InstancedMesh(
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
			master.Camera.SetupCameraFov( Vector3.One * 10, Vector3.Up, Vector3.Zero, MathUtil.Rad(120), 0.1f, 1000.0f, 1, 0, 1 );

			//	update console :
			console.Update( gameTime );
		}



		public override void RequestToExit ()
		{
			Game.Exit();
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
	}
}
