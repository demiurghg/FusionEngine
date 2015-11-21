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

namespace TestGame2 {


	class CustomGameInterface : Fusion.Engine.Common.GameInterface {

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;

		/*[GameModule("BarBefore", "bar", InitOrder.Before)]
		public SomeModule2 Bar { get; set; }

		[GameModule("BarAfter", "bar", InitOrder.After)]
		public SomeModule2 Bar2 { get; set; }*/

		SpriteLayer testLayer;
		SpriteLayer	uiLayer;
		DiscTexture	texture;
		ViewLayer	masterView;
		ViewLayer	sceneView;
		ViewLayer	sceneView1;

		Scene		scene;

		Vector3		position = new Vector3(0,5,0);
		float		yaw = 0, pitch = 0;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( GameEngine gameEngine ) : base(gameEngine)
		{
			console		=	new GameConsole( gameEngine, "conchars", "conback");

			/*Bar = new SomeModule2(gameEngine);
			Bar2 = new SomeModule2(gameEngine);*/
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			masterView		=	new ViewLayer(GameEngine, 0,0, true);
			sceneView		=	new ViewLayer(GameEngine, 256,256, true);
			sceneView1		=	new ViewLayer(GameEngine, 1024,768, true);

			GameEngine.GraphicsEngine.ViewLayers.Add( sceneView );
			GameEngine.GraphicsEngine.ViewLayers.Add( sceneView1 );
			GameEngine.GraphicsEngine.ViewLayers.Add( masterView );

			testLayer	=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			uiLayer		=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			texture		=	GameEngine.Content.Load<DiscTexture>( "lena" );
			scene		=	GameEngine.Content.Load<Scene>( "testScene" );


			masterView.LightSet.SpotAtlas	=	GameEngine.Content.Load<TextureAtlas>("spots/spots");
			masterView.LightSet.DirectLight.Position	=	new Vector3(1,2,3);
			masterView.LightSet.DirectLight.Intensity	=	Color4.White;
			masterView.LightSet.DirectLight.Enabled		=	true;

			sceneView1.LightSet.SpotAtlas	=	GameEngine.Content.Load<TextureAtlas>("spots/spots");
			sceneView1.LightSet.DirectLight.Position	=	new Vector3(1,2,3);
			sceneView1.LightSet.DirectLight.Intensity	=	Color4.White;
			sceneView1.LightSet.DirectLight.Enabled		=	true;

			var transforms = new Matrix[ scene.Nodes.Count ];
			scene.ComputeAbsoluteTransforms( transforms );
			
			for ( int i=0; i<scene.Nodes.Count; i++ ) {
			
				var meshIndex  = scene.Nodes[i].MeshIndex;
			
				if (meshIndex<0) {
					continue;
				}
				
				var inst = new Instance( GameEngine.GraphicsEngine, scene.Meshes[meshIndex] );
				inst.World = transforms[ i ];
			
				masterView.Instances.Add( inst );
				sceneView1.Instances.Add( inst );
			}

			testLayer.Clear();
			//testLayer.Draw( target, 10,10 + 384,256,256, Color.White );

			testLayer.Draw( GameEngine.GraphicsEngine.LightRenderer.DiffuseTexture,     0,  0, 200,150, Color.White );
			testLayer.Draw( GameEngine.GraphicsEngine.LightRenderer.SpecularTexturer, 200,  0, 200,150, Color.White );
			testLayer.Draw( GameEngine.GraphicsEngine.LightRenderer.NormalMapTexture, 400,  0, 200,150, Color.White );
			testLayer.Draw( sceneView.Target,		  600,  0, 256,256, Color.White );//*/
			testLayer.Draw( sceneView1.Target,		  600,384, 256,256, Color.White );//*/

			//testLayer.DrawDebugString( debugFont, 10,276, "Lenna Soderberg", Color.White );
			sceneView1.SpriteLayers.Add( console.ConsoleSpriteLayer );

			masterView.SpriteLayers.Add( console.ConsoleSpriteLayer );
			masterView.SpriteLayers.Add( uiLayer );
			masterView.SpriteLayers.Add( testLayer );

			//master.GisLayers.Add(new TilesGisLayer(GameEngine));
			//master.GisLayers.Add(PolyGisLayer.CreateFromUtmFbxModel(GameEngine, "water_342631_6664090_36N"));

			//master.GisLayers.Add(new ModelLayer(GameEngine, new DVector2(30.30913, 59.95684), "itmo_building")
			//{
			//	ScaleFactor = 0.000142f,
			//	Yaw = MathUtil.Rad(57)
			//});


			GameEngine.Keyboard.KeyDown += Keyboard_KeyDown;
		}



		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );

				GameEngine.Reload();
			}
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref testLayer );
				SafeDispose( ref uiLayer );
				SafeDispose( ref masterView );
				SafeDispose( ref sceneView );
				SafeDispose( ref sceneView1 );
			}
			base.Dispose( disposing );
		}


		Random rand = new Random();


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			console.Update( gameTime );

			testLayer.Color	=	Color.White;

			sceneView.Camera.SetupCameraFov( new Vector3(20,10,20), Vector3.Zero, Vector3.Up, Vector3.Zero, MathUtil.DegreesToRadians(90), 0.1f, 1000, 0,0, 1 );

			/*if ( gameEngine.Keyboard.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );
			} */

			if ( GameEngine.Keyboard.IsKeyDown(Keys.PageDown) ) {
				angle -= 0.01f;
			}
			if ( GameEngine.Keyboard.IsKeyDown(Keys.PageUp) ) {
				angle += 0.01f;
			}

			testLayer.SetTransform( new Vector2(100,0), new Vector2(128+5,128+5), angle );

			var m = UpdateCam( gameTime );

			masterView.Camera.SetupCameraFov(m.TranslationVector, m.TranslationVector + m.Forward, m.Up, Vector3.Zero, MathUtil.DegreesToRadians(90), 0.1f, 1000, 0, 0, 1);

			var origin = Vector3.TransformCoordinate(Vector3.One * 25, Matrix.RotationY( (float)gameTime.Total.TotalSeconds ));
			sceneView1.Camera.SetupCameraFov( origin, Vector3.Zero, Vector3.Up, Vector3.Zero, MathUtil.DegreesToRadians(90), 0.1f, 1000, 0,0, 1);
		}



		Matrix UpdateCam ( GameTime gameTime )
		{
			var kb = GameEngine.Keyboard;

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
