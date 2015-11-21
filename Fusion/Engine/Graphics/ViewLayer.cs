using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents entire visible world.
	/// </summary>
	public class ViewLayer : DisposableBase {
		
		readonly GameEngine		GameEngine;
		readonly GraphicsEngine	ge;

		/// <summary>
		/// Indicates whether view should be drawn.
		/// Default value is True.
		/// </summary>
		public bool Visible {
			get; set;
		}

		/// <summary>
		/// Indicates in which order view should be drawn.
		/// </summary>
		public int Order {
			get; set;
		}

		/// <summary>
		/// Gets and sets view's camera.
		/// This value is already initialized when View object is created.
		/// </summary>
		public Camera Camera {
			get; set;
		}

		/// <summary>
		/// Gets view target.
		/// Null value indicates that view will be rendered to backbuffer.
		/// Default value is null.
		/// </summary>
		public TargetTexture Target {
			get {
				return new TargetTexture( ldrTarget );
			}
		}

		/// <summary>
		/// Indicated whether target buffer should be cleared before rendering.
		/// </summary>
		public bool Clear {	
			get; set;
		}

		/// <summary>
		/// Gets and sets clear color
		/// </summary>
		public Color4 ClearColor {
			get; set;
		}


		/// <summary>
		/// Gets HDR settings.
		/// </summary>
		public HdrSettings HdrSettings {
			get; private set;
		}

		/// <summary>
		/// Gets sky settings.
		/// </summary>
		public SkySettings SkySettings {
			get; private set;
		}

		/// <summary>
		/// Gets view light set.
		/// This value is already initialized when View object is created.
		/// </summary>
		public LightSet LightSet {
			get; private set;
		}

		/// <summary>
		/// Gets collection of sprite layers.
		/// </summary>
		public ICollection<SpriteLayer>	SpriteLayers {
			get; private set;
		}


		/// <summary>
		/// Gets collection of mesh instances.
		/// </summary>
		public ICollection<Instance> Instances {
			get; private set;
		}


		/// <summary>
		/// Gets collection of GIS layers.
		/// </summary>
		public ICollection<Gis.GisLayer> GisLayers;


		/// <summary>
		/// Gets view layer width.
		/// </summary>
		public int Width { 
			get {
				return ldrTarget == null ? ge.DisplayBounds.Width : ldrTarget.Width;
			}
		}


		/// <summary>
		/// Gets view layer height
		/// </summary>
		public int Height {
			get {
				return ldrTarget == null ? ge.DisplayBounds.Height : ldrTarget.Height;
			}
		}

		


		readonly bool useBackbuffer;
		readonly bool useHDR;

		internal RenderTarget2D ldrTarget;
		internal RenderTarget2D	measuredOld;
		internal RenderTarget2D	measuredNew;
		internal RenderTarget2D	bloom0;
		internal RenderTarget2D	bloom1;


		/// <summary>
		/// Creates ViewLayer instance
		/// </summary>
		/// <param name="gameEngine">Game engine</param>
		/// <param name="width">Target width. Specify zero value for backbuffer.</param>
		/// <param name="height">Target height. Specify zero value for backbuffer.</param>
		/// <param name="enableHdr">Indicates that ViewLayer has HDR capabilities.</param>
		public ViewLayer ( GameEngine gameEngine, int width, int height, bool enableHdr )
		{
			GameEngine		=	gameEngine;
			this.ge			=	gameEngine.GraphicsEngine;
			useBackbuffer	=	width == 0 || height == 0;
			useHDR			=	enableHdr;

			Visible			=	true;
			Order			=	0;

			Camera			=	new Camera();
			HdrSettings		=	new HdrSettings();
			SkySettings		=	new SkySettings();

			SpriteLayers	=	new List<SpriteLayer>();
			Instances		=	new List<Instance>();
			LightSet		=	new LightSet( gameEngine.GraphicsEngine );
			GisLayers		=	new List<Gis.GisLayer>();

			if (useHDR) {
				measuredOld	=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
				measuredNew	=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
			}

			if (useBackbuffer) {
				Resize( 1, 1 );
			} else {
				Resize( width, height );
			}

			ge.DisplayBoundsChanged += ge_DisplayBoundsChanged;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				ge.DisplayBoundsChanged -= ge_DisplayBoundsChanged;

				SafeDispose( ref measuredNew );
				SafeDispose( ref measuredOld );
				SafeDispose( ref bloom0 );
				SafeDispose( ref bloom1 );
				SafeDispose( ref ldrTarget );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ge_DisplayBoundsChanged ( object sender, EventArgs e )
		{
			if (useBackbuffer) {
				Resize( ge.DisplayBounds.Width, ge.DisplayBounds.Height );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Resize ( int newWidth, int newHeight )
		{
			var bounds = ge.DisplayBounds;
			
			if ( newWidth <= 0 || newWidth > bounds.Width ) {
				throw new ArgumentOutOfRangeException("newWidth", "argument must be within range [1..DisplayWidth]");
			}
			if ( newHeight <= 0 || newHeight > bounds.Height ) {
				throw new ArgumentOutOfRangeException("newHeight", "argument must be within range [1..DisplayHeight]");
			}


			if (useHDR) {

				int targetWidth		=	newWidth;
				int targetHeight	=	newHeight;

				if (useBackbuffer) {
					targetWidth		=	bounds.Width;
					targetHeight	=	bounds.Height;
				}

				int width	=	( targetWidth/2  ) & 0xFFF0;
				int height	=	( targetHeight/2 ) & 0xFFF0;
			

				if (bloom0==null || bloom1==null || width!=bloom0.Width || height!=bloom1.Height) {

					SafeDispose( ref bloom0 );
					SafeDispose( ref bloom1 );

					bloom0		=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba16F, width, height, true, false );
					bloom1		=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba16F, width, height, true, false );
				}
			}

			if (!useBackbuffer) {

				SafeDispose( ref ldrTarget );
				ldrTarget	=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba8, newWidth, newHeight, false, false );

			} else {
				ldrTarget	=	null;
			}
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Rendering :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Renders view
		/// </summary>
		internal void RenderView ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetSurface = useBackbuffer ? ge.Device.BackbufferColor.Surface : Target.RenderTarget.Surface;

			//	clear target buffer if necassary :
			if (Clear) {
				ge.Device.Clear( targetSurface, ClearColor );
			}


			var viewport	=	new Viewport( 0,0, targetSurface.Width, targetSurface.Height );


			//	Render HDR stuff: mesh instances, 
			//	special effects, sky, water, light etc. 
			RenderHdrScene( gameTime, stereoEye, viewport, targetSurface );


			/*GameEngine.GraphicsDevice.RestoreBackbuffer();
			if (GisLayers.Any()) {
				var tiles = GisLayers.First() as TilesGisLayer;
				if(tiles != null)
					tiles.Update(gameTime);
			}
			ge.Gis.Draw(gameTime, stereoEye, GisLayers);*/

			//	draw sprites :

			ge.SpriteEngine.DrawSprites( gameTime, stereoEye, targetSurface, SpriteLayers );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void RenderHdrScene ( GameTime gameTime, StereoEye stereoEye, Viewport viewport, RenderTargetSurface targetSurface )
		{
			if (!useHDR) {
				if (Instances.Any()) {
					Log.Warning("HDR must be enabled on ViewLayer to render mesh instances");
				}
				return;
			}


			ge.LightRenderer.ClearHdrBuffer();

			if (Instances.Any()) {
				//	clear g-buffer :
				ge.LightRenderer.ClearGBuffer();

				//	render shadows :
				ge.LightRenderer.RenderShadows( Camera, Instances );
			
				//	render g-buffer :
				ge.SceneRenderer.RenderGBuffer( Camera, stereoEye, Instances, viewport );

				//	render sky :
				ge.Sky.Render( Camera, stereoEye, gameTime, ge.LightRenderer.DepthBuffer.Surface, ge.LightRenderer.HdrBuffer.Surface, viewport, SkySettings );

				//	render lights :
				ge.LightRenderer.RenderLighting( Camera, stereoEye, LightSet, GameEngine.GraphicsEngine.WhiteTexture, viewport );
			}

			//	apply tonemapping and bloom :
			ge.HdrFilter.Render( gameTime, targetSurface, ge.LightRenderer.HdrBuffer, this );


		}
	}
}
