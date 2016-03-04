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
using Fusion.Engine.Audio;

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
		SoundWorld		soundWorld;
		TargetTexture	targetTexture;
		DiscTexture		debugFont;

		TilesGisLayer	tiles;
		TextGisLayer	text;

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

			var bounds		=	Game.RenderSystem.DisplayBounds;
			masterView		=	Game.RenderSystem.RenderWorld;

			Game.RenderSystem.DisplayBoundsChanged += (s,e) => {
				masterView.Resize( Game.RenderSystem.DisplayBounds.Width, Game.RenderSystem.DisplayBounds.Height );
			};

			targetTexture		=	new TargetTexture(Game.RenderSystem, bounds.Width, bounds.Height, TargetFormat.LowDynamicRange );

			testLayer	=	new SpriteLayer( Game.RenderSystem, 1024 );
			uiLayer		=	new SpriteLayer( Game.RenderSystem, 1024 );

			tiles = new TilesGisLayer(Game, masterView.GlobeCamera);
			masterView.GisLayers.Add(tiles);

			text = new TextGisLayer(Game, 100, masterView.GlobeCamera);
			masterView.GisLayers.Add(text);

			masterView.SpriteLayers.Add( testLayer );
			masterView.SpriteLayers.Add( text.TextSpriteLayer );
			masterView.SpriteLayers.Add(console.ConsoleSpriteLayer);

			
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
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				SafeDispose( ref testLayer );
				SafeDispose( ref uiLayer );
				SafeDispose( ref targetTexture );
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

			testLayer.Color	= Color.White;

			Hud.Clear(HudFps);
			Hud.Add(HudFps, Color.Orange, "FPS     : {0,6:0.00}", gameTime.Fps );


			testLayer.Clear();
			testLayer.BlendMode = SpriteBlendMode.AlphaBlend;

			int line = 0;
			foreach ( var debugString in Hud.GetLines() ) {
				testLayer.DrawDebugString( debugFont, 0+1, line*8+1, debugString.Text, Color.Black );
				testLayer.DrawDebugString( debugFont, 0+0, line*8+0, debugString.Text, debugString.Color );
				line++;
			}

			//text.Update(gameTime);
			tiles.Update(gameTime);
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
