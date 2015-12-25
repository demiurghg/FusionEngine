using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Framework;
using Fusion.Build;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Frames;
using Fusion;
using Fusion.Core.Shell;
using System.IO;
using Fusion.Engine.Media;

namespace TestGame2 {

	
	[Command("refreshServers", CommandAffinity.Default)]
	public class RefreshServerList : NoRollbackCommand {
		
		public RefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StartDiscovery(4, new TimeSpan(0,0,10));
		}

	}
	
	[Command("stopRefresh", CommandAffinity.Default)]
	public class StopRefreshServerList : NoRollbackCommand {
		
		public StopRefreshServerList( Invoker invoker ) : base(invoker)
		{
		}

		public override void Execute ()
		{
			Invoker.Game.GameInterface.StopDiscovery();
		}

	}


	class CustomGameInterface : Fusion.Engine.Common.UserInterface {

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


		[GameModule("GUI", "gui", InitOrder.Before)]
		public FrameProcessor FrameProcessor { get { return userInterface; } }
		FrameProcessor userInterface;

		//VideoPlayer	videoPlayer;
		//Video		video;

		SpriteLayer		testLayer;
		SpriteLayer		uiLayer;
		DiscTexture		texture;
		ViewLayerHdr	masterView;
		ViewLayer		masterView2;
		TargetTexture	targetTexture;

		Scene		scene;

		Vector3		position = new Vector3(0,10,0);
		float		yaw = 0, pitch = 0;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( Game game ) : base(game)
		{
			console			=	new GameConsole( game, "conchars", "conback");
			userInterface	=	new FrameProcessor( game, @"Fonts\textFont" );
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			//videoPlayer	=	new VideoPlayer();
			//video		=	new Video(@"C:\infection_demo.wmv");//*/
			////video		=	GameEngine.Content.Load<Video>("infection_demo");
			//video		=	new Video(File.ReadAllBytes(@"C:\infection_demo.wmv"));//*/

			//var mtrl		=	GameEngine.Content.Load<Material>("testMtrl");

			var bounds		=	Game.RenderSystem.DisplayBounds;
			masterView		=	new ViewLayerHdr(Game, bounds.Width, bounds.Height);

			masterView2		=	new ViewLayer(Game);

			Game.RenderSystem.DisplayBoundsChanged += (s,e) => {
				masterView.Resize( Game.RenderSystem.DisplayBounds.Width, Game.RenderSystem.DisplayBounds.Height );
				//Log.Warning("{0} {1}", GameEngine.GraphicsEngine.DisplayBounds.Width, GameEngine.GraphicsEngine.DisplayBounds.Height);
			};

			targetTexture		=	new TargetTexture(Game.RenderSystem, bounds.Width, bounds.Height, TargetFormat.LowDynamicRange );
			//masterView.Target	=	targetTexture;

			Game.RenderSystem.AddLayer( masterView );
			Game.RenderSystem.AddLayer( masterView2 );

			testLayer	=	new SpriteLayer( Game.RenderSystem, 1024 );
			uiLayer		=	new SpriteLayer( Game.RenderSystem, 1024 );
			texture		=	Game.Content.Load<DiscTexture>( "lena" );

			masterView.SkySettings.SunPosition			=	new Vector3(10,20,30);

			masterView.LightSet.SpotAtlas				=	Game.Content.Load<TextureAtlas>("spots/spots");
			masterView.LightSet.DirectLight.Direction	=	masterView.SkySettings.SunLightDirection;
			masterView.LightSet.DirectLight.Intensity	=	masterView.SkySettings.SunLightColor;
			masterView.LightSet.DirectLight.Enabled		=	true;
			masterView.LightSet.AmbientLevel			=	Color4.Zero;//masterView.SkySettings.AmbientLevel;

			masterView.LightSet.EnvLights.Add( new EnvLight( new Vector3(0,40,0), 1, 50 ) );


			var rand = new Random();

			/*for (int i=0; i<64; i++) {
				var light = new OmniLight();
				light.Position		=	new Vector3( 8*(i/8-4), 4, 8*(i%8-4) );
				light.RadiusInner	=	1;
				light.RadiusOuter	=	8;
				light.Intensity		=	rand.NextColor4() * 100;
				masterView.LightSet.OmniLights.Add( light );
			} */


			masterView2.SpriteLayers.Add( console.ConsoleSpriteLayer );
			masterView2.SpriteLayers.Add( uiLayer );
			masterView2.SpriteLayers.Add( testLayer );


			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();
		}



		void LoadContent ()
		{
			masterView.Instances.Clear();

			scene		=	Game.Content.Load<Scene>( @"scenes\testScene" );

			var transforms = new Matrix[ scene.Nodes.Count ];
			scene.ComputeAbsoluteTransforms( transforms );

			var defMtrl		=	Game.RenderSystem.DefaultMaterial;
			var materials	=	scene.Materials.Select( m => Game.Content.Load<Material>( m.Name, defMtrl ) ).ToArray();
			
			for ( int i=0; i<scene.Nodes.Count; i++ ) {
			
				var meshIndex  = scene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}
				
				var inst   = new MeshInstance( Game.RenderSystem, scene, scene.Meshes[meshIndex], materials );
				inst.World = transforms[ i ];
			
				masterView.Instances.Add( inst );
			}


			masterView.HdrSettings.BloomAmount	=	0.1f;
			masterView.HdrSettings.DirtAmount	=	0.9f;
			masterView.HdrSettings.DirtMask1	=	Game.Content.Load<DiscTexture>("bloomMask|srgb");
			masterView.HdrSettings.DirtMask2	=	null;//GameEngine.Content.Load<DiscTexture>("bloomMask2");
		}


		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", null, false );

				Game.Reload();
			}

			//if (e.Key==Keys.P ) {
			//	videoPlayer.Play(video);
			//}
			//if (e.Key==Keys.O ) {
			//	videoPlayer.Stop();
			//} //*/
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				//SafeDispose( ref video );
				//SafeDispose( ref videoPlayer );//*/

				SafeDispose( ref testLayer );
				SafeDispose( ref uiLayer );
				SafeDispose( ref masterView );
				SafeDispose( ref masterView2 );
				SafeDispose( ref targetTexture );
				/*SafeDispose( ref sceneView );
				SafeDispose( ref sceneView1 );**/
			}
			base.Dispose( disposing );
		}


		Random rand = new Random();


		public override void RequestToExit ()
		{
			Game.Exit();
		}


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			console.Update( gameTime );

			testLayer.Color	=	Color.White;

			//sceneView.Camera.SetupCameraFov( new Vector3(20,10,20), Vector3.Zero, Vector3.Up, Vector3.Zero, MathUtil.DegreesToRadians(90), 0.1f, 1000, 1,0, 1 );

			/*if ( game.Keyboard.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );

			} */

			testLayer.Clear();
			testLayer.BlendMode = SpriteBlendMode.Opaque;
			//testLayer.Draw( masterView.HdrTexture,	 -200,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.DiffuseTexture,	    0,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.SpecularTexture, 200,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.NormalMapTexture, 400,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.Target, 200,200,300,200, Color.White);

			//if (videoPlayer.State==MediaState.Playing) {
			//	testLayer.Draw( videoPlayer.GetTexture(), 20,0,300,200, Color.White);
			//}//*/

			//Log.Message("{0}", videoPlayer.State);


			if ( Game.Keyboard.IsKeyDown(Keys.PageDown) ) {
				angle -= 0.01f;
			}
			if ( Game.Keyboard.IsKeyDown(Keys.PageUp) ) {
				angle += 0.01f;
			}

			testLayer.SetTransform( new Vector2(200,0), new Vector2(128+5,128+5), angle );

			var m = UpdateCam( gameTime );

			var vp = Game.RenderSystem.DisplayBounds;
			var ratio = vp.Width / (float)vp.Height;

			masterView.Camera.SetupCameraFov(m.TranslationVector, m.TranslationVector + m.Forward, m.Up, MathUtil.DegreesToRadians(90), 0.1f, 1000, 1, 0, ratio);
		}



		Matrix UpdateCam ( GameTime gameTime )
		{
			var kb = Game.Keyboard;

			if (!Console.Show) {
				if (kb.IsKeyDown( Keys.Left  )) yaw   += gameTime.ElapsedSec * 2f;
				if (kb.IsKeyDown( Keys.Right )) yaw   -= gameTime.ElapsedSec * 2f;
				if (kb.IsKeyDown( Keys.Up    )) pitch -= gameTime.ElapsedSec * 2f;
				if (kb.IsKeyDown( Keys.Down  )) pitch += gameTime.ElapsedSec * 2f;
			}

			var m = Matrix.RotationYawPitchRoll( yaw, pitch, 0 );

			if (!Console.Show) {
				if (kb.IsKeyDown( Keys.S )) position += m.Forward  * gameTime.ElapsedSec * 10;
				if (kb.IsKeyDown( Keys.Z )) position += m.Backward * gameTime.ElapsedSec * 10;
				if (kb.IsKeyDown( Keys.A )) position += m.Left     * gameTime.ElapsedSec * 10;
				if (kb.IsKeyDown( Keys.X )) position += m.Right    * gameTime.ElapsedSec * 10;
				if (kb.IsKeyDown( Keys.Space ))		position += Vector3.Up   * gameTime.ElapsedSec * 5;
				if (kb.IsKeyDown( Keys.C ))   position += Vector3.Down * gameTime.ElapsedSec * 5;
			}

			m.TranslationVector = position;

			return m;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse ( System.Net.IPEndPoint endPoint, string serverInfo )
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo );
		}
	}
}
