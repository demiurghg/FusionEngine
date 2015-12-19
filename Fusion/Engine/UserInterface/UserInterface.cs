using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Drivers.Graphics;
using System.Diagnostics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;


namespace Fusion.Engine.UserInterface {


	public class UserInterface : GameModule {

		[Config]
		public Config	Config	{ get; set; }

		public	Frame			RootFrame		{ get; set; }
		public	Frame			TargetFrame		{ get; private set; }
		public	Frame			TopHoveredFrame	{ get; private set; }
		public	SpriteFont		DefaultFont		{ get; set; }
		string	defaultFontPath	;

		MouseProcessor	mouseProcessor;



		/// <summary>
		/// Creates view
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public UserInterface ( Game game, string defaultFont ) : base(game)
		{
			Config				=	new Fusion.Engine.UserInterface.Config();
			defaultFontPath		=	defaultFont;
			mouseProcessor		=	new MouseProcessor( Game, this );
		}



		/// <summary>
		/// Inits view
		/// </summary>
		public override void Initialize()
		{
			 DefaultFont	=	Game.Content.Load<SpriteFont>(defaultFontPath);

			 mouseProcessor.Initialize();
		}



		TimeSpan	uiUpdateProfiling;
		internal	bool	SuppressLayout { get; private set; }
		internal	bool	ForceLayout { get; private set; }


		/// <summary>
		/// Call this method after UI setup to make things right
		/// </summary>
		public void SettleControls () 
		{
			UpdateUI( new GameTime(), true, false );
			UpdateUI( new GameTime(), false, true );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="suppressLayout"></param>
		/// <param name="forceLayout"></param>
		protected void UpdateUI ( GameTime gameTime, bool suppressLayout = false, bool forceLayout = false )
		{
			SuppressLayout	=	suppressLayout;
			ForceLayout		=	forceLayout;

			if (RootFrame!=null) {
				RootFrame.UpdateInternal( gameTime, 0, 0 );
			}
		}



		/// <summary>
		/// Updates stuff
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update( GameTime gameTime )
		{
			var viewCtxt	=	new ViewContext();

			//
			//	Update and profile UI stuff :
			//
			Stopwatch sw = new Stopwatch();
			sw.Start();

				UpdateUI( gameTime );

				mouseProcessor.Update( RootFrame );

			sw.Stop();

			uiUpdateProfiling	=	sw.Elapsed;
		}



		/// <summary>
		/// Draws entire interface
		/// </summary>
		/// <param name="gameTime"></param>
		public void Draw ( GameTime gameTime, SpriteLayer spriteLayer )
		{
			if (Config.SkipUserInterface) {
				return;
			}

			var sb	=	spriteLayer;
			var vp  =	Game.RenderSystem.DisplayBounds;

			sb.Clear();
		
			if (RootFrame!=null) {
				RootFrame.DrawInternal( gameTime, sb, Color.White );
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Input processing :
		 * 
		-----------------------------------------------------------------------------------------*/

	}
}
