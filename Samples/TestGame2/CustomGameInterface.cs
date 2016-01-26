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
		RenderWorld		masterView;
		ViewLayer		masterView2;
		TargetTexture	targetTexture;
		DiscTexture		debugFont;

		Scene		scene;
		Scene		animScene;
		Scene		skinScene;

		Vector3		position = new Vector3(0,10,0);
		float		yaw = 0, pitch = 0;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( Game game ) : base(game)
		{
			console			=	new GameConsole( game, "conchars");
			userInterface	=	new FrameProcessor( game, @"Fonts\textFont" );
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			debugFont		=	Game.Content.Load<DiscTexture>("conchars");
			//videoPlayer	=	new VideoPlayer();
			//video		=	new Video(@"C:\infection_demo.wmv");//*/
			////video		=	GameEngine.Content.Load<Video>("infection_demo");
			//video		=	new Video(File.ReadAllBytes(@"C:\infection_demo.wmv"));//*/

			//var mtrl		=	GameEngine.Content.Load<Material>("testMtrl");

			var bounds		=	Game.RenderSystem.DisplayBounds;
			masterView		=	new RenderWorld(Game, bounds.Width, bounds.Height);

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

			masterView.SkySettings.SunPosition			=	new Vector3(20,30,40);
			masterView.SkySettings.SunLightIntensity	=	50;
			masterView.SkySettings.SkyTurbidity			=	3;

			masterView.LightSet.SpotAtlas				=	Game.Content.Load<TextureAtlas>("spots/spots");
			masterView.LightSet.DirectLight.Direction	=	masterView.SkySettings.SunLightDirection;
			masterView.LightSet.DirectLight.Intensity	=	masterView.SkySettings.SunLightColor;
			masterView.LightSet.DirectLight.Enabled		=	true;
			masterView.LightSet.AmbientLevel			=	Color4.Zero;
			//masterView.LightSet.AmbientLevel			=	masterView.SkySettings.AmbientLevel;

			masterView.LightSet.EnvLights.Add( new EnvLight( new Vector3(0,4,-10), 1,  500 ) );

			//masterView.LightSet.EnvLights.Add( new EnvLight( new Vector3(0,4,-10), 1,  15 ) );
			//masterView.LightSet.EnvLights.Add( new EnvLight( new Vector3(0,4, 10), 1, 15 ) );


			var rand = new Random();

			/*for (int i=0; i<64; i++) {
				var light = new OmniLight();
				light.Position		=	new Vector3( 8*(i/8-4), 4, 8*(i%8-4) );
				light.RadiusInner	=	1;
				light.RadiusOuter	=	8;
				light.Intensity		=	rand.NextColor4() * 100;
				masterView.LightSet.OmniLights.Add( light );
			} //*/
														 
			/*for (int i=0; i<256; i++) {
				var light = new EnvLight();
				light.Position		=	new Vector3( 7*(i/16-8), 6, 7*(i%16-8) );
				light.RadiusInner	=	2;
				light.RadiusOuter	=	8;
				masterView.LightSet.EnvLights.Add( light );
			} //*/

			fireLight	=	new OmniLight();
			fireLight.Position	=	Vector3.UnitZ * (-10) + Vector3.UnitY * 5 + Vector3.UnitX * 20;
			fireLight.RadiusOuter	=	7;
			fireLight.Intensity		=	new Color4(249, 172, 61, 0)/2;

			masterView.LightSet.OmniLights.Add( fireLight );
			


			masterView2.SpriteLayers.Add( console.ConsoleSpriteLayer );
			masterView2.SpriteLayers.Add( uiLayer );
			masterView2.SpriteLayers.Add( testLayer );


			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();
		}


		List<Tuple<MeshInstance,int>> animInstances = new List<Tuple<MeshInstance,int>>();
		List<Tuple<MeshInstance,int>> skinInstances = new List<Tuple<MeshInstance,int>>();


		OmniLight	fireLight = null;

		void LoadContent ()
		{
			masterView.Instances.Clear();

			//-------------------------------------

			masterView.ParticleSystem.Images	=	Game.Content.Load<TextureAtlas>(@"sprites\particles|srgb");

			//-------------------------------------

			scene		=	Game.Content.Load<Scene>( @"scenes\testScene" );

			var transforms = new Matrix[ scene.Nodes.Count ];
			scene.ComputeAbsoluteTransforms( transforms );

			var defMtrl		=	Game.RenderSystem.DefaultMaterial;
			var materials	=	scene.Materials.Select( m => Game.Content.Load<MaterialInstance>( m.Name, defMtrl ) ).ToArray();
			
			for ( int i=0; i<scene.Nodes.Count; i++ ) {
			
				var meshIndex  = scene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}
				
				var inst   = new MeshInstance( Game.RenderSystem, scene, scene.Meshes[meshIndex], materials );
				inst.World = transforms[ i ];
			
				masterView.Instances.Add( inst );
			}


			//-------------------------------------

			animScene =	Game.Content.Load<Scene>(@"scenes\testAnim");
			animInstances.Clear();

			transforms = new Matrix[ animScene.Nodes.Count ];
			animScene.ComputeAbsoluteTransforms( transforms );

			materials	=	animScene.Materials.Select( m => Game.Content.Load<MaterialInstance>( m.Name, defMtrl ) ).ToArray();
			
			for ( int i=0; i<animScene.Nodes.Count; i++ ) {
			
				var meshIndex  = animScene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}
				
				var inst   = new MeshInstance( Game.RenderSystem, animScene, animScene.Meshes[meshIndex], materials );
				inst.World = transforms[ i ];

				animInstances.Add( new Tuple<MeshInstance,int>( inst, i ) );
			
				masterView.Instances.Add( inst );
			}

			//-------------------------------------

			skinScene =	Game.Content.Load<Scene>(@"scenes\testSkin");
			skinInstances.Clear();

			transforms = new Matrix[ skinScene.Nodes.Count ];
			skinScene.ComputeAbsoluteTransforms( transforms );

			materials	=	skinScene.Materials.Select( m => Game.Content.Load<MaterialInstance>( m.Name, defMtrl ) ).ToArray();
			
			for ( int i=0; i<skinScene.Nodes.Count; i++ ) {
			
				var meshIndex  = skinScene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}

				for (int j = 0; j<8; j++) {
				
					var inst   = new MeshInstance( Game.RenderSystem, skinScene, skinScene.Meshes[meshIndex], materials );
					inst.World = transforms[ i ] * Matrix.Translation(0,3,10) * Matrix.RotationY(MathUtil.Pi*2/8.0f * j);
				
					skinInstances.Add( new Tuple<MeshInstance,int>( inst, i ) );

					if (inst.IsSkinned) {
						var bones = new Matrix[skinScene.Nodes.Count];
						skinScene.GetAnimSnapshot( 8, bones );
						skinScene.ComputeBoneTransforms( bones, inst.BoneTransforms );
					}

					masterView.Instances.Add( inst );
				}
			}

			//-------------------------------------

			masterView.HdrSettings.BloomAmount	=	0.05f;
			masterView.HdrSettings.DirtAmount	=	0.95f;
			masterView.HdrSettings.DirtMask1	=	Game.Content.Load<DiscTexture>("bloomMask|srgb");
			masterView.HdrSettings.DirtMask2	=	null;//GameEngine.Content.Load<DiscTexture>("bloomMask2");
		}


		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild();
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

				SafeDispose( ref testLayer );
				SafeDispose( ref uiLayer );
				SafeDispose( ref masterView );
				SafeDispose( ref masterView2 );
				SafeDispose( ref targetTexture );
			}
			base.Dispose( disposing );
		}


		Random rand = new Random();


		public override void RequestToExit ()
		{
			Game.Exit();
		}

		static readonly Guid HudFps = Guid.NewGuid();

		float frame = 0;
		Random rand2 = new Random();

		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			console.Update( gameTime );

			testLayer.Color	=	Color.White;

			//sceneView.Camera.SetupCameraFov( new Vector3(20,10,20), Vector3.Zero, Vector3.Up, Vector3.Zero, MathUtil.DegreesToRadians(90), 0.1f, 1000, 1,0, 1 );
			Hud.Clear(HudFps);
			Hud.Add(HudFps, Color.Orange, "FPS     : {0,6:0.00}", gameTime.Fps );
			Hud.Add(HudFps, Color.Orange, "FPS avg : {0,6:0.00}", gameTime.AverageFrameRate );
			Hud.Add(HudFps, Color.Orange, "FPS max : {0,6:0.00}", gameTime.MaxFrameRate );
			Hud.Add(HudFps, Color.Orange, "FPS min : {0,6:0.00}", gameTime.MinFrameRate );


			testLayer.Clear();
			testLayer.BlendMode = SpriteBlendMode.AlphaBlend;

			int line = 0;
			foreach ( var debugString in Hud.GetLines() ) {
				testLayer.DrawDebugString( debugFont, 0+1, line*8+1, debugString.Text, Color.Black );
				testLayer.DrawDebugString( debugFont, 0+0, line*8+0, debugString.Text, debugString.Color );
				line++;
			}

			//-- animation --

			frame += gameTime.ElapsedSec * 24;

			var transformsLocal  = new Matrix[ animScene.Nodes.Count ];
			var transformsGlobal = new Matrix[ animScene.Nodes.Count ];

			animScene.GetAnimSnapshot( frame, animScene.FirstFrame, animScene.LastFrame, AnimationMode.Repeat, transformsLocal );
			animScene.ComputeAbsoluteTransforms( transformsLocal, transformsGlobal );

			for (int i=0; i<animInstances.Count; i++) {
				animInstances[i].Item1.World = transformsGlobal[ animInstances[i].Item2 ];
			}

			//-- skinning --

			var bonesLocal  = new Matrix[ skinScene.Nodes.Count ];

			var rand = new Random(4546);
			
			for (int i=0; i<skinInstances.Count; i++) {
				skinScene.GetAnimSnapshot( frame * rand.NextFloat(0.7f, 1.3f)/3.0f + rand.NextFloat(0,20), skinScene.FirstFrame, skinScene.LastFrame, AnimationMode.Repeat, bonesLocal );
				skinScene.ComputeBoneTransforms( bonesLocal, skinInstances[i].Item1.BoneTransforms );
				//skinInstances[i].Item1.World = transformsGlobal[ animInstances[i].Item2 ];
			}


			var vp = Game.RenderSystem.DisplayBounds;

			if (Game.Keyboard.IsKeyDown(Keys.K)) {
				masterView.ParticleSystem.KillParticles();
			}

			if (Game.Keyboard.IsKeyDown(Keys.P)) {

				var p = new Particle();
				p.FadeIn		=	0.2f;
				p.FadeOut		=	0.2f;
				p.Color0		=	new Color4(7000,7000,7000, 0);
				p.Color1		=	new Color4(7000,7000,7000, 0.2f);
				p.ImageIndex	=	0;
				p.TimeLag		=	0;

				for (int i=0; i<50; i++) {
					p.Velocity		=	(rand2.UniformRadialDistribution(0.0f,0.5f) + Vector3.Up * 4.0f) * Math.Abs(rand2.GaussDistribution(1, 0.25f));
					p.Position		=	Vector3.UnitZ * (-10) + Vector3.UnitY * 5 + Vector3.UnitX * 20 + rand2.UniformRadialDistribution(0.0f,0.5f);
					p.LifeTime		=	rand2.GaussDistribution(2.5f,0.25f);
					p.Size0			=	0.2f;
					p.Size1			=	0.0f;
					p.Rotation0		=	rand2.NextFloat(0,3.14f*2);
					p.Rotation1		=	rand2.NextFloat(0,3.14f*2);
					p.Gravity		=	0.3f;
					p.ImageIndex	=	0;
					//var 
					masterView.ParticleSystem.InjectParticle( p );
				}//*/

				for (int i=0; i<5; i++) {
					p.FadeIn		=	0.2f;
					p.FadeOut		=	0.2f;

					p.Color0		=	new Color4(700,700,700, 0);
					p.Color1		=	new Color4(700,700,700, 1.0f);

					p.Velocity		=	(rand2.UniformRadialDistribution(0.0f,0.5f) + Vector3.Up * 3.0f) * Math.Abs(rand2.GaussDistribution(1, 0.025f));
					p.Position		=	Vector3.UnitZ * (-10) + Vector3.UnitY * 5 + Vector3.UnitX * 20;// + rand.NextVector3( -Vector3.One * 2, Vector3.One * 2);
					p.LifeTime		=	rand2.GaussDistribution(1.0f,0.1f);
					p.Size0			=	2.4f;
					p.Size1			=	1.4f;
					p.Rotation0		=	rand2.NextFloat(0,3.14f*2);
					p.Rotation1		=	p.Rotation0 + rand2.NextFloat(-1,1);
					p.Gravity		=	0;
					p.ImageIndex	=	0;
					//var 
					masterView.ParticleSystem.InjectParticle( p );
				}//*/

				if (rand2.NextFloat(0,1)<0.3f || true) {
					p = new Particle();

					p.FadeIn		=	0.2f;
					p.FadeOut		=	0.2f;
					p.Color0		=	new Color4(4,4,4, 0);
					p.Color1		=	new Color4(4,4,4, 0.6f);
					p.ImageIndex	=	0;
					p.TimeLag		=	0;

					p.Velocity		=	rand2.GaussRadialDistribution(0.5f,0.1f) + Vector3.Up * rand2.GaussDistribution(3.0f,0.15f);
					p.Position		=	Vector3.UnitZ * (-10) + Vector3.UnitY * 5 + Vector3.UnitX * 20;// + rand.NextVector3( -Vector3.One * 2, Vector3.One * 2);
					p.LifeTime		=	rand2.GaussDistribution(4.4f,0.25f);
					p.Size0			=	1.2f;
					p.Size1			=	3.5f;
					p.Rotation0		=	rand2.NextFloat(0,3.14f*2);
					p.Rotation1		=	p.Rotation0 + rand2.NextFloat(-4,4);
					p.Gravity		=	0.05f;
					p.ImageIndex	=	1;
					//var 
					masterView.ParticleSystem.InjectParticle( p );
				}
			}


			/*if ( game.Keyboard.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );

			} */
			//testLayer.Draw( masterView.HdrTexture,	 -200,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.DiffuseTexture,	    0,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.SpecularTexture, 200,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.NormalMapTexture, 400,  0, 200,150, Color.White );
			//testLayer.Draw( masterView.Target, 200,200,300,200, Color.White);

			//if (videoPlayer.State==MediaState.Playing) {
			//	testLayer.Draw( videoPlayer.GetTexture(), 20,0,300,200, Color.White);
			//}//*/

			//Log.Message("{0}", videoPlayer.State);

			//masterView.IsPaused  = Game.Keyboard.IsKeyDown(Keys.O);


			if ( Game.Keyboard.IsKeyDown(Keys.PageDown) ) {
				angle -= 0.01f;
			}
			if ( Game.Keyboard.IsKeyDown(Keys.PageUp) ) {
				angle += 0.01f;
			}

			//testLayer.SetTransform( new Vector2(200,0), new Vector2(128+5,128+5), angle );

			var m = UpdateCam( gameTime );

			var ratio = vp.Width / (float)vp.Height;

			Game.Keyboard.ScanKeyboard = !Console.Show;

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
