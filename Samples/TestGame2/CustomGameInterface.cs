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

namespace TestGame2 {

	class SomeModule  : GameModule {
		public SomeModule(GameEngine e): base(e) {}
		public override void Initialize ()	{}
		protected override void Dispose ( bool disposing )	{}
	}

	class SomeModule2  : GameModule {

		[GameModule("BooBefore", "boo", InitOrder.Before)]
		public SomeModule Boo { get; set; }

		[GameModule("BooAfter", "boo", InitOrder.After)]
		public SomeModule Boo2 { get; set; }

		public SomeModule2(GameEngine e): base(e) { Boo = new SomeModule(e); Boo2 = new SomeModule(e); }
		public override void Initialize ()	{}
		protected override void Dispose ( bool disposing )	{}
	}


	class CustomGameInterface : Fusion.Engine.Common.GameInterface {

		[GameModule("Console", "con", InitOrder.Before)]
		public GameConsole Console { get { return console; } }
		public GameConsole console;


		[GameModule("BarBefore", "bar", InitOrder.Before)]
		public SomeModule2 Bar { get; set; }

		[GameModule("BarAfter", "bar", InitOrder.After)]
		public SomeModule2 Bar2 { get; set; }

		
		SpriteLayer testLayer;
		DiscTexture	texture;
		DiscTexture	debugFont;


		Composition	master;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( GameEngine gameEngine ) : base(gameEngine)
		{
			console		=	new GameConsole( gameEngine, "conchars", "conback");

			Bar = new SomeModule2(gameEngine);
			Bar2 = new SomeModule2(gameEngine);
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			master		=	new Composition(GameEngine);

			GameEngine.GraphicsEngine.Compositions.Add( master );

			testLayer	=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			texture		=	GameEngine.Content.Load<DiscTexture>( "lena" );
			//debugFont	=	GameEngine.Content.Load<DiscTexture>( "debugFont" );

			testLayer.Clear();
			testLayer.Draw( texture, 10,10 + 384,256,256, Color.White );

			//testLayer.DrawDebugString( debugFont, 10,276, "Lenna Soderberg", Color.White );

			master.SpriteLayers.Add( testLayer );
			master.SpriteLayers.Add( console.ConsoleSpriteLayer );
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref testLayer );
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

			/*if ( gameEngine.Keyboard.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );
			} */

			if ( GameEngine.Keyboard.IsKeyDown(Keys.Left) ) {
				angle -= 0.01f;
			}
			if ( GameEngine.Keyboard.IsKeyDown(Keys.Right) ) {
				angle += 0.01f;
			}

			testLayer.SetTransform( new Vector2(100,0), new Vector2(128+5,128+5), angle );
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
