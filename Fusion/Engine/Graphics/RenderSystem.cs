using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	public partial class RenderSystem : GameComponent {

		internal readonly GraphicsDevice Device;

		public SpriteEngine	SpriteEngine { get { return spriteEngine; } }
		SpriteEngine	spriteEngine;

		public Filter Filter { get{ return filter; } }
		Filter filter;

		public SsaoFilter SsaoFilter { get{ return ssaoFilter; } }
		SsaoFilter ssaoFilter;

		public BitonicSort BitonicSort { get{ return bitonicSort; } }
		BitonicSort bitonicSort;

		public HdrFilter HdrFilter { get{ return hdrFilter; } }
		HdrFilter hdrFilter;
		
		public DofFilter DofFilter { get{ return dofFilter; } }
		DofFilter dofFilter;
		
		public LightRenderer	LightRenderer { get { return lightRenderer; } }
		LightRenderer	lightRenderer;
		
		public SceneRenderer	SceneRenderer { get { return sceneRenderer; } }
		SceneRenderer	sceneRenderer;
		
		public Sky	Sky { get { return sky; } }
		public Sky	sky;

		/// <summary>
		/// Gets render counters.
		/// </summary>
		internal RenderCounters Counters { get; private set; }


		/// <summary>
		/// Fullscreen
		/// </summary>
		[Config]
		public bool Fullscreen { 
			get { 
				return isFullscreen;
			}
			set { 
				if (isFullscreen!=value) {
					isFullscreen = value;
					if (Device!=null) {
						Device.FullScreen = value;
					}
				}
			}
		}
		bool isFullscreen = false;



		RenderTarget2D	hdrTarget;

		public Texture	GrayTexture { get { return grayTexture; } }
		public Texture	WhiteTexture { get { return whiteTexture; } }
		public Texture	BlackTexture { get { return blackTexture; } }
		public Texture	FlatNormalMap { get { return flatNormalMap; } }

		DynamicTexture grayTexture;
		DynamicTexture whiteTexture;
		DynamicTexture blackTexture;
		DynamicTexture flatNormalMap;

		/// <summary>
		/// Gets default material.
		/// </summary>
		public MaterialInstance	DefaultMaterial { get { return defaultMaterial; } }
		MaterialInstance defaultMaterial;


		/// <summary>
		/// Gets render world.
		/// </summary>
		public RenderWorld RenderWorld {
			get {
				return renderWorld;
			}
		}

		RenderWorld renderWorld;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		public RenderSystem ( Game Game ) : base(Game)
		{
			Counters	=	new RenderCounters();

			Width			=	1024;
			Height			=	768;
			Fullscreen		=	false;
			StereoMode		=	StereoMode.Disabled;
			InterlacingMode	=	InterlacingMode.HorizontalLR;
			UseDebugDevice	=	false;
			VSyncInterval	=	1;
			MsaaEnabled		=	false;
			UseFXAA			=	true;

			this.Device	=	Game.GraphicsDevice;

			spriteEngine	=	new SpriteEngine( this );
			filter			=	new Filter( Game );
			ssaoFilter		=	new SsaoFilter( Game );
			hdrFilter		=	new HdrFilter( Game );
			dofFilter		=	new DofFilter( Game );
			lightRenderer	=	new LightRenderer( Game );
			sceneRenderer	=	new SceneRenderer( Game, this );
			sky				=	new Sky( Game );
			bitonicSort		=	new BitonicSort( Game );

			Device.DisplayBoundsChanged += (s,e) => {
				var handler = DisplayBoundsChanged;
				if (handler!=null) {
					handler(s,e);
				}
			};
		}


										  
		/// <summary>
		/// Applies graphics parameters.
		/// </summary>
		/// <param name="p"></param>
		internal void ApplyParameters ( ref GraphicsParameters p )
		{
			p.Width				=	Width;
			p.Height			=	Height;
			p.FullScreen		=	Fullscreen;
			p.StereoMode		=	(Fusion.Drivers.Graphics.StereoMode)StereoMode;
			p.InterlacingMode	=	(Fusion.Drivers.Graphics.InterlacingMode)InterlacingMode;
			p.UseDebugDevice	=	UseDebugDevice;
			p.MsaaLevel			=	MsaaEnabled ? 4 : 1;
		}



		/// <summary>
		/// Intializes graphics engine.
		/// </summary>
		public override void Initialize ()
		{
			//	init components :
			InitializeComponent( spriteEngine	);
			InitializeComponent( filter			);
			InitializeComponent( ssaoFilter		);
			InitializeComponent( hdrFilter		);
			InitializeComponent( dofFilter		);
			InitializeComponent( lightRenderer	);
			InitializeComponent( sceneRenderer	);
			InitializeComponent( sky			);	
			InitializeComponent( bitonicSort	);

			//	create default textures :
			whiteTexture	=	new DynamicTexture( this, 4,4, typeof(Color), false, false );
			whiteTexture.SetData( Enumerable.Range(0,16).Select( i => Color.White ).ToArray() );
			
			grayTexture		=	new DynamicTexture( this, 4,4, typeof(Color), false, false );
			grayTexture.SetData( Enumerable.Range(0,16).Select( i => Color.Gray ).ToArray() );

			blackTexture	=	new DynamicTexture( this, 4,4, typeof(Color), false, false );
			blackTexture.SetData( Enumerable.Range(0,16).Select( i => Color.Black ).ToArray() );

			flatNormalMap	=	new DynamicTexture( this, 4,4, typeof(Color), false, false );
			flatNormalMap.SetData( Enumerable.Range(0,16).Select( i => new Color(127,127,255,127) ).ToArray() );

			var baseIllum = new BaseIllum();
			defaultMaterial	=	baseIllum.CreateMaterialInstance(this, Game.Content);

			//	add default render world :
			renderWorld	=	new RenderWorld(Game, Width, Height);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				DisposeComponents();

				SafeDispose( ref renderWorld );

				SafeDispose( ref grayTexture );
				SafeDispose( ref whiteTexture );
				SafeDispose( ref blackTexture );
				SafeDispose( ref flatNormalMap );

				SafeDispose( ref defaultMaterial );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		internal void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			Counters.Reset();

			RenderWorld.Render( gameTime, stereoEye );

			if (ShowCounters) {
				Counters.PrintCounters();
			}
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Display stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void Screenshot ( string path = null )
		{
			Device.Screenshot(path);
		}
		

		/// <summary>
		/// Gets display bounds.
		/// </summary>
		public Rectangle DisplayBounds {
			get {
				return Device.DisplayBounds;
			}
		}


		/// <summary>
		/// Raises when display bound changes.
		/// DisplayBounds property is already has actual value when this event raised.
		/// </summary>
		public event EventHandler	DisplayBoundsChanged;
	}
}
