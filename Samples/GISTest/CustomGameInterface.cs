using System;
using System.CodeDom;
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
using Fusion.Engine.Audio;
using Fusion.Engine.Graphics.Graph;

namespace GISTest {

	
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

		SpriteLayer		testLayer;
		SpriteLayer		uiLayer;
		DiscTexture		texture;
		RenderWorld		masterView;
		RenderLayer		viewLayer;
		SoundWorld		soundWorld;
		DiscTexture		debugFont;

		TilesGisLayer	tiles;
		TextGisLayer	text;

		Scene		scene;
		Scene		animScene;
		Scene		skinScene;

		Vector3		position = new Vector3(0,10,0);
		float		yaw = 0, pitch = 0;

		PointsGisLayerCPU	pointsCPU;
		PointsGisLayer		pointsGPU;


		private GraphLayer graph;

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
			debugFont		=	Game.Content.Load<DiscTexture>( "conchars" );

			var bounds		=	Game.RenderSystem.DisplayBounds;
			masterView		=	Game.RenderSystem.RenderWorld;


			Game.RenderSystem.RemoveLayer(masterView);

			viewLayer = new RenderLayer(Game);
			Game.RenderSystem.AddLayer(viewLayer);

			//Game.RenderSystem.DisplayBoundsChanged += (s,e) => {
			//	masterView.Resize( Game.RenderSystem.DisplayBounds.Width, Game.RenderSystem.DisplayBounds.Height );
			//};


			testLayer	=	new SpriteLayer( Game.RenderSystem, 1024 );
			uiLayer		=	new SpriteLayer( Game.RenderSystem, 1024 );


			// Setup tiles
			tiles = new TilesGisLayer(Game, viewLayer.GlobeCamera);
			//viewLayer.GisLayers.Add(tiles);
			tiles.SetMapSource(TilesGisLayer.MapSource.OpenStreetMap);

			
			
			text = new TextGisLayer(Game, 100, viewLayer.GlobeCamera);
			//masterView.GisLayers.Add(text);

			//masterView.SpriteLayers.Add( testLayer );
			//masterView.SpriteLayers.Add( text.TextSpriteLayer );
			viewLayer.SpriteLayers.Add(console.ConsoleSpriteLayer);

			
			text.GeoTextArray[0] = new TextGisLayer.GeoText {
				Color	= Color.Red,
				Text	= "Arrow",
				LonLat	= DMathUtil.DegreesToRadians(new DVector2(30.306473, 59.944082))
			};

			text.GeoTextArray[1] = new TextGisLayer.GeoText {
				Color	= Color.Teal,
				Text	= "Park",
				LonLat	= DMathUtil.DegreesToRadians(new DVector2(30.313897, 59.954623))
			};

			Game.Keyboard.KeyDown += Keyboard_KeyDown;

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();


			var r = new Random();

			//pointsCPU = new PointsGisLayerCPU(Game, 8000000);
			//pointsCPU.TextureAtlas = Game.Content.Load<Texture2D>("Zombie");
			//
			//for (int i = 0; i < pointsCPU.PointsCountToDraw; i++) {
			//	pointsCPU.AddPoint(i, DMathUtil.DegreesToRadians(new DVector2(30.240383 + r.NextDouble(-1, 1) * 0.1, 59.944007 - r.NextDouble(-1, 1) * 0.05)), 0, 0.01f);
			//}
			//pointsCPU.UpdatePointsBuffer();
			//viewLayer.GisLayers.Add(pointsCPU);
			

			pointsGPU = new PointsGisLayer(Game, 1000, true);
			pointsGPU.TextureAtlas = Game.Content.Load<Texture2D>("Zombie");
			pointsGPU.ImageSizeInAtlas = new Vector2(36, 36);
			
			for (int i = 0; i < pointsGPU.PointsCountToDraw; i++) {
				var pos = DMathUtil.DegreesToRadians(new DVector2(30.240383 + r.NextDouble(-1, 1)*0.2, 59.944007 - r.NextDouble(-1, 1)*0.1));
				pointsGPU.PointsCpu[i] = new Gis.GeoPoint {
					Lon = pos.X,
					Lat = pos.Y,
					Color = Color.White,
					Tex0 = new Vector4(0, 0, 1.02f, 0)
				};
			}

			pointsGPU.Flags = (int)(PointsGisLayer.PointFlags.DOTS_SCREENSPACE);

			pointsGPU.UpdatePointsBuffer();
			//viewLayer.GisLayers.Add(pointsGPU);


			//viewLayer.GlobeCamera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);
			//viewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 5;

			Game.Touch.Tap			+= args => System.Console.WriteLine("You just perform tap gesture at point: " + args.Position);
			Game.Touch.DoubleTap	+= args => System.Console.WriteLine("You just perform double tap gesture at point: " + args.Position);
			Game.Touch.SecondaryTap += args => System.Console.WriteLine("You just perform secondary tap gesture at point: " + args.Position);
			Game.Touch.Manipulate	+= args => System.Console.WriteLine("You just perform touch manipulation: " + args.Position + "	" + args.ScaleDelta + "	" + args.RotationDelta + " " + args.IsEventBegin + " " + args.IsEventEnd);


			graph = new GraphLayer(Game);
			graph.Camera = new GreatCircleCamera();
			
			graph.Initialize();
			
			viewLayer.GraphLayers.Add(graph);
			viewLayer.Camera = graph.Camera;
		}




		void LoadContent ()
		{
			
		}


		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.F5) {

				Builder.SafeBuild();
				Game.Reload();
			}


			if (e.Key == Keys.LeftShift) {
				viewLayer.GlobeCamera.ToggleViewToPointCamera();
			}
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				SafeDispose( ref testLayer );
				SafeDispose( ref uiLayer );
				tiles.Dispose();
				masterView.Dispose();
				pointsGPU.Dispose();
				text.Dispose();
				viewLayer.Dispose();
			}
			base.Dispose( disposing );
		}


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



			///////////////////////////////////////////////////////////////////////////////
			///////////////////////////////////////////////////////////////////////////////
			//var r = new Random();

			//for (int i = 0; i < pointsCPU.PointsCountToDraw; i++)
			//{
			//	pointsCPU.AddPoint(i, DMathUtil.DegreesToRadians(new DVector2(30.240383 + r.NextDouble(-1, 1) * 0.1, 59.944007 - r.NextDouble(-1, 1) * 0.05)), 0, 0.01f);
			//}
			//pointsCPU.UpdatePointsBuffer();


			//for (int i = 0; i < pointsGPU.PointsCountToDraw; i++)
			//{
			//	//var pos = DMathUtil.DegreesToRadians(new DVector2(30.240383 + r.NextDouble(-1, 1) * 0.1, 59.944007 - r.NextDouble(-1, 1) * 0.05));
			//	//pointsGPU.PointsCpu[i] = new Gis.GeoPoint
			//	//{
			//	//	Lon = pos.X,
			//	//	Lat = pos.Y,
			//	//	Color = Color4.White,
			//	//	Tex0 = new Vector4(0, 0, 0.02f, 0)
			//	//};
			//
			//	pointsGPU.PointsCpu[i] = pointsGPU.PointsCpu[pointsGPU.PointsCountToDraw - 1 - i];
			//}
			//pointsGPU.UpdatePointsBuffer();

			///////////////////////////////////////////////////////////////////////////////
			///////////////////////////////////////////////////////////////////////////////
			//testLayer.Color	= Color.White;
			//
			//Hud.Clear(HudFps);
			//Hud.Add(HudFps, Color.Orange, "FPS     : {0,6:0.00}", gameTime.Fps );
			//
			//
			//testLayer.Clear();
			//testLayer.BlendMode = SpriteBlendMode.AlphaBlend;
			//
			//int line = 0;
			//foreach ( var debugString in Hud.GetLines() ) {
			//	testLayer.DrawDebugString( debugFont, 0+1, line*8+1, debugString.Text, Color.Black );
			//	testLayer.DrawDebugString( debugFont, 0+0, line*8+0, debugString.Text, debugString.Color );
			//	line++;
			//}
			//
			////text.Update(gameTime);
			tiles.Update(gameTime);
			tt += gameTime.ElapsedSec;
			if (tt > 10)
			{
				tt = tt - 10;
				counter++;
				if (graph.state == State.RUN && counter < 1000)
				{
					graph.createLinksFromFile(counter);
				
				}
			}
			
		}

		private int counter = 0;
		private float tt = 10;

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
