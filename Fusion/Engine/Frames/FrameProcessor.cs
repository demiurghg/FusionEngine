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


namespace Fusion.Engine.Frames {


	public class FrameProcessor : GameComponent {

		[Config]	public bool		ShowFrames			{ get; set; }
		[Config]	public bool		SkipUserInterface	{ get; set; }
		[Config]	public bool		ShowProfilingInfo	{ get; set; }


		/// <summary>
		/// Sets and gets current root frame.
		/// </summary>
		public	Frame RootFrame { get; set; }


		/// <summary>
		/// Gets ans sets default font.
		/// If this value not set, 
		/// the creation of Frames without explicitly specified font will fail.
		/// </summary>
		public	SpriteFont DefaultFont { get; set; }


		/// <summary>
		/// Gets and sets current target frame.
		/// </summary>
		public	Frame TargetFrame { 
			get { 
				return targetFrame;
			}
			internal set {
				if (targetFrame!=value) {
					if (targetFrame!=null) {
						targetFrame.OnDeactivate();
					}
					if (value!=null) {
						value.OnActivate();
					}
					targetFrame = value;
				}
			}
		}
		Frame targetFrame = null;

		MouseProcessor	mouseProcessor;


		/// <summary>
		/// Gets FrameProcessor's sprite layer, that could be attached to RenderWorld and RenderView.
		/// </summary>
		public SpriteLayer FramesSpriteLayer {
			get {
				return spriteLayer;
			}
		}

		SpriteLayer spriteLayer;


		/// <summary>
		/// Creates view
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public FrameProcessor ( Game game ) : base(game)
		{
			mouseProcessor		=	new MouseProcessor( Game, this );
		}



		/// <summary>
		/// Inits view
		/// </summary>
		public override void Initialize()
		{
			 spriteLayer	=	new SpriteLayer( Game.RenderSystem, 1024 );

			 mouseProcessor.Initialize();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref spriteLayer );
			}
			base.Dispose( disposing );
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
				RootFrame.UpdateInternal( gameTime );
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


			//
			//	Draw UI :
			//
			Draw ( gameTime, spriteLayer );
		}



		/// <summary>
		/// Draws entire interface
		/// </summary>
		/// <param name="gameTime"></param>
		void Draw ( GameTime gameTime, SpriteLayer spriteLayer )
		{
			if (SkipUserInterface) {
				return;
			}

			spriteLayer.Clear();

			Frame.DrawNonRecursive( RootFrame, gameTime, spriteLayer );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Input processing :
		 * 
		-----------------------------------------------------------------------------------------*/

	}
}
