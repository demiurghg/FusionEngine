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
	public class ViewLayerHdr : ViewLayer {

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
		/// Gets collection of mesh instances.
		/// </summary>
		public ICollection<MeshInstance> Instances {
			get; private set;
		}


		internal RenderTarget2D	HdrBuffer		;
		internal RenderTarget2D	LightAccumulator;
		internal DepthStencil2D	DepthBuffer		;
		internal RenderTarget2D	DiffuseBuffer	;
		internal RenderTarget2D	SpecularBuffer	;
		internal RenderTarget2D	NormalMapBuffer	;

		//	reuse diffuse buffer as temporal buffer for effects.
		internal RenderTarget2D TempFXBuffer { get { return DiffuseBuffer; } }

		internal RenderTarget2D	MeasuredOld;
		internal RenderTarget2D	MeasuredNew;
		internal RenderTarget2D	Bloom0;
		internal RenderTarget2D	Bloom1;


		//public TargetTexture	HdrTexture			{ get; set; }
		//public TargetTexture	DiffuseTexture		{ get; set; }
		//public TargetTexture	SpecularTexture		{ get; set; }
		//public TargetTexture	NormalMapTexture	{ get; set; }


		/// <summary>
		/// Creates ViewLayer instance
		/// </summary>
		/// <param name="Game">Game engine</param>
		/// <param name="width">Target width. Specify zero value for backbuffer.</param>
		/// <param name="height">Target height. Specify zero value for backbuffer.</param>
		/// <param name="enableHdr">Indicates that ViewLayer has HDR capabilities.</param>
		public ViewLayerHdr ( Game Game, int width, int height ) : base( Game )
		{
			var vp	=	Game.GraphicsDevice.DisplayBounds;

			if (width<=0) {
				width	=	vp.Width;
			}
			if (height<=0) {
				height	=	vp.Height;
			}

			HdrSettings		=	new HdrSettings();
			SkySettings		=	new SkySettings();

			Instances		=	new List<MeshInstance>();
			LightSet		=	new LightSet( Game.GraphicsEngine );

			MeasuredOld		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
			MeasuredNew		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );

			Resize( width, height );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref HdrBuffer );
				SafeDispose( ref LightAccumulator );
				SafeDispose( ref DepthBuffer );
				SafeDispose( ref DiffuseBuffer );
				SafeDispose( ref SpecularBuffer );
				SafeDispose( ref NormalMapBuffer );

				SafeDispose( ref Bloom0 );
				SafeDispose( ref Bloom1 );

				SafeDispose( ref MeasuredOld );
				SafeDispose( ref MeasuredNew );

			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Resize ( int newWidth, int newHeight )
		{
			SafeDispose( ref HdrBuffer );
			SafeDispose( ref LightAccumulator );
			SafeDispose( ref DepthBuffer );
			SafeDispose( ref DiffuseBuffer );
			SafeDispose( ref SpecularBuffer );
			SafeDispose( ref NormalMapBuffer );

			SafeDispose( ref Bloom0 );
			SafeDispose( ref Bloom1 );

			//	clamp values :
			newWidth	=	Math.Max(128, newWidth);
			newHeight	=	Math.Max(128, newHeight);

			int targetWidth		=	newWidth;
			int targetHeight	=	newHeight;

			int bloomWidth		=	( targetWidth/2  ) & 0xFFF0;
			int bloomHeight		=	( targetHeight/2 ) & 0xFFF0;

			HdrBuffer			=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, newWidth,	newHeight,	false, false );
			LightAccumulator	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, newWidth,	newHeight,	false, true );
			DepthBuffer			=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8,	newWidth,	newHeight,	1 );
			DiffuseBuffer		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8,	newWidth,	newHeight,	false, false );
			SpecularBuffer 		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8,	newWidth,	newHeight,	false, false );
			NormalMapBuffer		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgb10A2, newWidth,	newHeight,	false, false );
			
			Bloom0				=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, bloomWidth, bloomHeight, true, false );
			Bloom1				=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, bloomWidth, bloomHeight, true, false );

			//HdrTexture			=	new TargetTexture( HdrBuffer );
			//DiffuseTexture		=	new TargetTexture( DiffuseBuffer );
			//SpecularTexture		=	new TargetTexture( SpecularBuffer );
			//NormalMapTexture	=	new TargetTexture( NormalMapBuffer );
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Rendering :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Renders view
		/// </summary>
		internal override void RenderView ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetSurface = (Target == null) ? ge.Device.BackbufferColor.Surface : Target.RenderTarget.Surface;

			//	clear target buffer if necassary :
			if (Clear) {
				ge.Device.Clear( targetSurface, ClearColor );
			}

			var viewport	=	new Viewport( 0,0, targetSurface.Width, targetSurface.Height );

			//	Render HDR stuff: mesh instances, 
			//	special effects, sky, water, light etc. 
			RenderHdrScene( gameTime, stereoEye, viewport, targetSurface );

			//	Render GIS stuff :
			RenderGIS( gameTime, stereoEye, viewport, targetSurface );

			//	draw sprites :
			ge.SpriteEngine.DrawSprites( gameTime, stereoEye, targetSurface, SpriteLayers );
		}



		/// <summary>
		/// 
		/// </summary>
		public void ClearBuffers ()
		{
			Game.GraphicsDevice.Clear( DiffuseBuffer.Surface,		Color4.Black );
			Game.GraphicsDevice.Clear( SpecularBuffer.Surface,	Color4.Black );
			Game.GraphicsDevice.Clear( NormalMapBuffer.Surface,	Color4.Black );

			Game.GraphicsDevice.Clear( DepthBuffer.Surface,		1, 0 );
			Game.GraphicsDevice.Clear( HdrBuffer.Surface,			Color4.Black );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void RenderHdrScene ( GameTime gameTime, StereoEye stereoEye, Viewport viewport, RenderTargetSurface targetSurface )
		{
			//	clear g-buffer and hdr-buffers:
			ClearBuffers();

			//	render shadows :
			ge.LightRenderer.RenderShadows( this );
			
			//	render g-buffer :
			ge.SceneRenderer.RenderGBuffer( stereoEye, this );

			//	render sky :
			ge.Sky.Render( Camera, stereoEye, gameTime, DepthBuffer.Surface, HdrBuffer.Surface, viewport, SkySettings );

			//	render lights :
			ge.LightRenderer.RenderLighting( stereoEye, this, Game.GraphicsEngine.WhiteTexture );

			//	apply tonemapping and bloom :
			ge.HdrFilter.Render( gameTime, TempFXBuffer.Surface, HdrBuffer, this );

			//	apply FXAA
			ge.Filter.Fxaa( targetSurface, TempFXBuffer );
		}




		/// <summary>
		/// Renders lit mesh instances.
		/// </summary>
		void RenderLitMeshInstances ()
		{
		}



		void BuildVisibility ()
		{
		}
	}
}
