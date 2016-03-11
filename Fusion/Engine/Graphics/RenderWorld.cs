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
using System.Diagnostics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents entire visible world.
	/// </summary>
	public class RenderWorld : RenderLayer {

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
		/// Gets debug render
		/// </summary>
		public DebugRender Debug {
			get { return debug; }
		}
		DebugRender debug;

		/// <summary>
		/// Gets particle system instance.
		/// </summary>
		public ParticleSystem ParticleSystem {
			get { return particleSystem; }
		}
		ParticleSystem	particleSystem;

		/// <summary>
		/// Gets collection of mesh instances.
		/// </summary>
		public ICollection<MeshInstance> Instances {
			get; private set;
		}


		HdrFrame viewHdrFrame;
		HdrFrame radianceFrame;

		//	reuse diffuse buffer as temporal buffer for effects.
		internal RenderTarget2D TempFXBuffer { get { return viewHdrFrame.DiffuseBuffer; } }

		internal RenderTarget2D	MeasuredOld;
		internal RenderTarget2D	MeasuredNew;
		internal RenderTarget2D	Bloom0;
		internal RenderTarget2D	Bloom1;

		internal RenderTargetCube Radiance;

		internal TextureCubeArray RadianceCache;


		/// <summary>
		/// Creates ViewLayerHDR instance
		/// </summary>
		/// <param name="Game">Game engine</param>
		/// <param name="width">Target width.</param>
		/// <param name="height">Target height.</param>
		public RenderWorld ( Game game, int width, int height ) : base( game )
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
			LightSet		=	new LightSet( Game.RenderSystem );

			debug			=	new DebugRender( Game );
			
			particleSystem	=	new ParticleSystem( Game.RenderSystem, this );

			MeasuredOld		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
			MeasuredNew		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );

			radianceFrame	=	new HdrFrame( Game, 512,512 );

			Radiance		=	new RenderTargetCube( Game.GraphicsDevice, ColorFormat.Rgba16F, RenderSystemConfig.EnvMapSize, true );
			RadianceCache	=	new TextureCubeArray( Game.GraphicsDevice, 128, RenderSystemConfig.MaxEnvLights, ColorFormat.Rgba16F, true );

			Resize( width, height );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				
				SafeDispose( ref particleSystem );

				SafeDispose( ref debug );

				SafeDispose( ref Radiance );
				SafeDispose( ref RadianceCache );

				SafeDispose( ref viewHdrFrame );
				SafeDispose( ref radianceFrame );

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
		public void ClearWorld ()
		{
			LightSet.EnvLights.Clear();
			LightSet.OmniLights.Clear();
			LightSet.SpotLights.Clear();

			Instances.Clear();

			//	immediate?
			ParticleSystem.KillParticles();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Resize ( int newWidth, int newHeight )
		{
			SafeDispose( ref viewHdrFrame );

			SafeDispose( ref Bloom0 );
			SafeDispose( ref Bloom1 );

			//	clamp values :
			newWidth	=	Math.Max(128, newWidth);
			newHeight	=	Math.Max(128, newHeight);

			int targetWidth		=	newWidth;
			int targetHeight	=	newHeight;

			int bloomWidth		=	( targetWidth/2  ) & 0xFFF0;
			int bloomHeight		=	( targetHeight/2 ) & 0xFFF0;

			viewHdrFrame		=	new HdrFrame ( Game, targetWidth, targetHeight );
			
			Bloom0				=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, bloomWidth, bloomHeight, true, false );
			Bloom1				=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, bloomWidth, bloomHeight, true, false );

			//HdrTexture			=	new TargetTexture( HdrBuffer );
			//DiffuseTexture		=	new TargetTexture( DiffuseBuffer );
			//SpecularTexture		=	new TargetTexture( SpecularBuffer );
			//NormalMapTexture	=	new TargetTexture( NormalMapBuffer );
		}





		/// <summary>
		/// Indicates whether world is paused.
		/// </summary>
		public bool IsPaused {
			get {
				return isPaused;
			}
			set {	
				if (isPaused!=value) {
					isPaused = value;
				}
			}
		}


		bool isPaused;


		/// <summary>
		/// Pauses render world simulation and animation.
		/// </summary>
		public void Pause ()
		{
			IsPaused	=	true;
		}



		/// <summary>
		/// Resumes render world simulation and animation.
		/// </summary>
		public void Resume ()
		{
			IsPaused	=	false;
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Rendering :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Renders view
		/// </summary>
		internal override void Render ( GameTime gameTime, StereoEye stereoEye )
		{
			var targetSurface = (Target == null) ? rs.Device.BackbufferColor.Surface : Target.RenderTarget.Surface;

			//	clear target buffer if necassary :
			if (Clear) {
				rs.Device.Clear( targetSurface, ClearColor );
			}

			var viewport	=	new Viewport( 0,0, targetSurface.Width, targetSurface.Height );

			//	Render HDR stuff: mesh instances, 
			//	special effects, sky, water, light etc. 
			RenderHdrScene( gameTime, stereoEye, viewport, targetSurface );

			//	Render GIS stuff :
			RenderGIS( gameTime, stereoEye, viewport, targetSurface );

			//	draw sprites :
			rs.SpriteEngine.DrawSprites( gameTime, stereoEye, targetSurface, SpriteLayers );
		}



		/// <summary>
		/// 
		/// </summary>
		void ClearBuffers ( HdrFrame frame )
		{
			Game.GraphicsDevice.Clear( frame.DiffuseBuffer.Surface,		Color4.Black );
			Game.GraphicsDevice.Clear( frame.SpecularBuffer.Surface,	Color4.Black );
			Game.GraphicsDevice.Clear( frame.NormalMapBuffer.Surface,	Color4.Black );
			Game.GraphicsDevice.Clear( frame.ScatteringBuffer.Surface,	Color4.Black );

			Game.GraphicsDevice.Clear( frame.DepthBuffer.Surface,		1, 0 );
			Game.GraphicsDevice.Clear( frame.HdrBuffer.Surface,			Color4.Black );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void RenderHdrScene ( GameTime gameTime, StereoEye stereoEye, Viewport viewport, RenderTargetSurface targetSurface )
		{
			//	clear g-buffer and hdr-buffers:
			ClearBuffers( viewHdrFrame );

			//	render shadows :
			rs.LightRenderer.RenderShadows( this, this.LightSet );

			//	render g-buffer :
			rs.SceneRenderer.RenderGBuffer( stereoEye, Camera, viewHdrFrame, this );

			//	render ssao :
			rs.SsaoFilter.Render( stereoEye, Camera, viewHdrFrame.DepthBuffer, viewHdrFrame.NormalMapBuffer );

			switch (rs.Config.ShowGBuffer) {
				case 1 : rs.Filter.Copy( targetSurface, viewHdrFrame.DiffuseBuffer ); return;
				case 2 : rs.Filter.Copy( targetSurface, viewHdrFrame.SpecularBuffer ); return;
				case 3 : rs.Filter.Copy( targetSurface, viewHdrFrame.NormalMapBuffer ); return;
				case 4 : rs.Filter.Copy( targetSurface, viewHdrFrame.ScatteringBuffer ); return;
				case 5 : rs.Filter.Copy( targetSurface, rs.SsaoFilter.OcclusionMap ); return;
			}

			//	render sky :
			rs.Sky.Render( Camera, stereoEye, viewHdrFrame, SkySettings );
			rs.Sky.RenderFogTable( SkySettings );

			//	render lights :
			rs.LightRenderer.RenderLighting( stereoEye, Camera, viewHdrFrame, this, Radiance );

			//	render and simulate particles :
			ParticleSystem.Render( gameTime, Camera, stereoEye, viewHdrFrame );

			//	apply tonemapping and bloom :
			rs.HdrFilter.Render( gameTime, TempFXBuffer.Surface, viewHdrFrame.HdrBuffer, this );


			//	apply FXAA
			if (rs.Config.UseFXAA) {
				rs.Filter.Fxaa( targetSurface, TempFXBuffer );
			} else {
				rs.Filter.Copy( targetSurface, TempFXBuffer );
			} 

			//	draw debug lines :
			Debug.Render( targetSurface, Camera );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void RenderRadiance ()
		{
			var sw = new Stopwatch();

			/*if (!Game.Keyboard.IsKeyDown(Input.Keys.F3)) {
				return;
			} //*/

			Log.Message("Radiance capture...");

			sw.Start();
			using (new PixEvent("Capture Radiance")) {

				var sun	=	SkySettings.SunGlowIntensity;
				SkySettings.SunGlowIntensity = 0;

				int index = 0;

				foreach ( var envLight in LightSet.EnvLights ) {

					for (int i=0; i<6; i++) {
					
						ClearBuffers( radianceFrame );

						var camera = new Camera();
						camera.SetupCameraCubeFace( envLight.Position, (CubeFace)i, 0.125f, 5000 );

						//	render g-buffer :
						rs.SceneRenderer.RenderGBuffer( StereoEye.Mono, camera, radianceFrame, this );

						//	render sky :
						rs.Sky.Render( camera, StereoEye.Mono, radianceFrame, SkySettings );

						//	render lights :
						rs.LightRenderer.RenderLighting( StereoEye.Mono, camera, radianceFrame, this, rs.Sky.SkyCube );

						//	downsample captured frame to cube face.
						rs.Filter.StretchRect4x4( Radiance.GetSurface( 0, (CubeFace)i ), radianceFrame.HdrBuffer, SamplerState.LinearClamp, true );

						//	prefilter cubemap :
						rs.Filter.PrefilterEnvMap( Radiance );
					}
				
					RadianceCache.CopyFromRenderTargetCube( index, Radiance );
					index ++;
				}
				sw.Stop();
	
				SkySettings.SunGlowIntensity = sun;
			}

			Log.Message("{0} light probes - {1} ms", LightSet.EnvLights.Count, sw.ElapsedMilliseconds);
		}



		void BuildVisibility ()
		{
		}
	}
}
