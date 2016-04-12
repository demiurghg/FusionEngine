using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics {
	public partial class LightRenderer : GameModule {

		const int	BlockSizeX		=	16;
		const int	BlockSizeY		=	16;

		RenderSystem rs { get { return Game.RenderSystem; } }

		[Config]
		public LightRendererConfig	Config { get; set; }

		DepthStencil2D		csmDepth		;
		RenderTarget2D		csmColor		;
		DepthStencil2D		spotDepth		;
		RenderTarget2D		spotColor		;
		DepthStencil2D		skyMapDepth		;
		RenderTarget2D		skyMapColor		;


		OmniLightGPU[]		omniLightData;
		SpotLightGPU[]		spotLightData;
		EnvLightGPU[]		envLightData;
		StructuredBuffer	omniLightBuffer	;
		StructuredBuffer	spotLightBuffer	;
		StructuredBuffer	envLightBuffer	;

		public RenderTarget2D	CSMColor { get { return csmColor; } }
		public DepthStencil2D	CSMDepth { get { return csmDepth; } }

		public RenderTarget2D	SkyMapColor { get { return skyMapColor; } }
		public DepthStencil2D	SkyMapDepth { get { return skyMapDepth; } }

		public RenderTarget2D	SpotColor { get { return spotColor; } }
		public DepthStencil2D	SpotDepth { get { return spotDepth; } }



		enum LightingFlags {
			SOLIDLIGHTING	=	0x0001,
			PARTICLES		=	0x0002,
		}

		[StructLayout(LayoutKind.Explicit, Size=640)]
		struct LightingParams {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public Matrix	InverseViewProjection;
			[FieldOffset(192)] public Vector4	FrustumVectorTR;
			[FieldOffset(208)] public Vector4	FrustumVectorBR;
			[FieldOffset(224)] public Vector4	FrustumVectorBL;
			[FieldOffset(240)] public Vector4	FrustumVectorTL;
			[FieldOffset(256)] public Matrix	CSMViewProjection0;
			[FieldOffset(320)] public Matrix	CSMViewProjection1;
			[FieldOffset(384)] public Matrix	CSMViewProjection2;
			[FieldOffset(448)] public Matrix	CSMViewProjection3;
			[FieldOffset(512)] public Vector4	ViewPosition;
			[FieldOffset(528)] public Vector4	DirectLightDirection;
			[FieldOffset(544)] public Vector4	DirectLightIntensity;
			[FieldOffset(560)] public Vector4	ViewportSize;
			[FieldOffset(576)] public Vector4	CSMFilterRadius;
			[FieldOffset(592)] public Color4	AmbientColor;
			[FieldOffset(608)] public Vector4	Viewport;
			[FieldOffset(624)] public float		ShowCSLoadOmni;
			[FieldOffset(628)] public float		ShowCSLoadEnv;
			[FieldOffset(632)] public float		ShowCSLoadSpot;
		}


		struct OmniLightGPU {
			public Vector4	PositionRadius;
			public Vector4	Intensity;
			public Vector4	ExtentMin;	// x,y, depth
			public Vector4	ExtentMax;	// x,y, depth
		}


		struct EnvLightGPU {
			public Vector4	Position;
			public Vector4	Intensity;
			public Vector4	ExtentMin;	// x,y, depth
			public Vector4	ExtentMax;	// x,y, depth
			public Vector4	InnerOuterRadius;
		}


		struct SpotLightGPU {
			public Matrix	ViewProjection;
			public Vector4	PositionRadius;
			public Vector4	IntensityFar;
			public Vector4	ExtentMin;	// x,y, depth
			public Vector4	ExtentMax;	// x,y, depth
			public Vector4	MaskScaleOffset;
			public Vector4	ShadowScaleOffset;
		}


		Ubershader		lightingShader;
		StateFactory	factory;
		ConstantBuffer	lightingCB;
		ConstantBuffer	skyOcclusionCB;

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public LightRenderer( Game game ) : base(game)
		{
			Config	=	new LightRendererConfig();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			lightingCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(LightingParams) );
			skyOcclusionCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Matrix), 64 );
			omniLightBuffer	=	new StructuredBuffer( Game.GraphicsDevice, typeof(OmniLightGPU), RenderSystemConfig.MaxOmniLights, StructuredBufferFlags.None );
			spotLightBuffer	=	new StructuredBuffer( Game.GraphicsDevice, typeof(SpotLightGPU), RenderSystemConfig.MaxSpotLights, StructuredBufferFlags.None );
			envLightBuffer	=	new StructuredBuffer( Game.GraphicsDevice, typeof(EnvLightGPU),  RenderSystemConfig.MaxEnvLights, StructuredBufferFlags.None );

			CreateShadowMaps();

			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			lightingShader	=	Game.Content.Load<Ubershader>("lighting");
			factory			=	lightingShader.CreateFactory( typeof(LightingFlags), Primitive.TriangleList, VertexInputElement.Empty );
		}



		/// <summary>
		/// 
		/// </summary>
		void CreateShadowMaps ()
		{
			SafeDispose( ref csmColor );
			SafeDispose( ref csmDepth );

			SafeDispose( ref spotColor );
			SafeDispose( ref spotDepth );

			SafeDispose( ref skyMapColor );
			SafeDispose( ref skyMapDepth );

			csmColor	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  Config.CSMSize * 4, Config.CSMSize );
			csmDepth	=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8, Config.CSMSize * 4, Config.CSMSize );

			spotColor	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  Config.SpotShadowSize * 4, Config.SpotShadowSize * 4 );
			spotDepth	=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8, Config.SpotShadowSize * 4, Config.SpotShadowSize * 4 );

			skyMapColor	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  4096, 4096 );
			skyMapDepth	=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8, 4096, 4096 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref csmDepth );
				SafeDispose( ref csmColor );
				SafeDispose( ref spotDepth );
				SafeDispose( ref spotColor );
				SafeDispose( ref skyMapColor );
				SafeDispose( ref skyMapDepth );

				SafeDispose( ref lightingCB );
				SafeDispose( ref skyOcclusionCB );
				SafeDispose( ref omniLightBuffer );
				SafeDispose( ref spotLightBuffer );
				SafeDispose( ref envLightBuffer );
			}

			base.Dispose( disposing );
		}

		Matrix[] csmViewProjections = new Matrix[4];
		Matrix[] skyOcclusionViewProjection = new Matrix[64];


		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="projection"></param>
		internal void RenderLighting ( StereoEye stereoEye, Camera camera, HdrFrame hdrFrame, RenderWorld viewLayer, RenderTargetCube envLight )
		{
			using ( new PixEvent("TiledLighting") ) {
				var view		=	camera.GetViewMatrix( stereoEye );
				var projection	=	camera.GetProjectionMatrix( stereoEye );

				var device = Game.GraphicsDevice;
				device.ResetStates();

				var width	=	hdrFrame.HdrBuffer.Width;
				var height	=	hdrFrame.HdrBuffer.Height;


				//
				//	Setup compute shader parameters and states :
				//
				try {

					var cbData	=	new LightingParams();
					var invView	=	Matrix.Invert( view );
					var invVP	=	Matrix.Invert( view * projection );
					var viewPos	=	invView.TranslationVector;

					skyOcclusionCB.SetData( skyOcclusionViewProjection );

					cbData.DirectLightDirection		=	new Vector4( viewLayer.LightSet.DirectLight.Direction, 0 );
					cbData.DirectLightIntensity		=	viewLayer.LightSet.DirectLight.Intensity.ToVector4();
					cbData.Projection				=	projection;

					cbData.CSMViewProjection0		=	csmViewProjections[0];
					cbData.CSMViewProjection1		=	csmViewProjections[1];
					cbData.CSMViewProjection2		=	csmViewProjections[2];
					cbData.CSMViewProjection3		=	csmViewProjections[3];

					cbData.View						=	view;
					cbData.ViewPosition				=	new Vector4(viewPos,1);
					cbData.InverseViewProjection	=	invVP;
					cbData.CSMFilterRadius			=	new Vector4( Config.CSMFilterSize );

					cbData.AmbientColor				=	viewLayer.LightSet.AmbientLevel;
					cbData.Viewport					=	new Vector4( 0, 0, width, height );
					cbData.ShowCSLoadOmni			=	Config.ShowOmniLightTileLoad ? 1 : 0;
					cbData.ShowCSLoadEnv			=	Config.ShowEnvLightTileLoad  ? 1 : 0;
					cbData.ShowCSLoadSpot			=	Config.ShowSpotLightTileLoad ? 1 : 0;

					ComputeOmniLightsTiles( view, projection, viewLayer.LightSet );
					ComputeSpotLightsTiles( view, projection, viewLayer.LightSet );
					ComputeEnvLightsTiles(  view, projection, viewLayer.LightSet );

					//
					//	set states :
					//
					device.SetTargets( null, hdrFrame.HdrBuffer.Surface );

					lightingCB.SetData( cbData );

					device.ComputeShaderSamplers[0]	=	SamplerState.PointClamp;
					device.ComputeShaderSamplers[1]	=	SamplerState.LinearClamp;
					device.ComputeShaderSamplers[2]	=	SamplerState.ShadowSampler;

					device.ComputeShaderResources[0]	=	hdrFrame.DiffuseBuffer;
					device.ComputeShaderResources[1]	=	hdrFrame.SpecularBuffer;
					device.ComputeShaderResources[2]	=	hdrFrame.NormalMapBuffer;
					device.ComputeShaderResources[3]	=	hdrFrame.ScatteringBuffer;
					device.ComputeShaderResources[4]	=	hdrFrame.DepthBuffer;
					device.ComputeShaderResources[5]	=	csmColor;
					device.ComputeShaderResources[6]	=	spotColor;
					device.ComputeShaderResources[7]	=	viewLayer.LightSet.SpotAtlas==null ? rs.WhiteTexture.Srv : viewLayer.LightSet.SpotAtlas.Texture.Srv;
					device.ComputeShaderResources[8]	=	omniLightBuffer;
					device.ComputeShaderResources[9]	=	spotLightBuffer;
					device.ComputeShaderResources[10]	=	envLightBuffer;
					device.ComputeShaderResources[11]	=	rs.SsaoFilter.OcclusionMap;
					device.ComputeShaderResources[12]	=	viewLayer.RadianceCache;
					device.ComputeShaderResources[13]	=	viewLayer.ParticleSystem.SimulatedParticles;
					device.ComputeShaderResources[14]	=	skyMapColor;

					device.ComputeShaderConstants[0]	=	lightingCB;
					device.ComputeShaderConstants[1]	=	skyOcclusionCB;

					device.SetCSRWTexture( 0, hdrFrame.LightAccumulator.Surface );
					device.SetCSRWTexture( 1, hdrFrame.SSSAccumulator.Surface );
					device.SetCSRWBuffer(  2, viewLayer.ParticleSystem.ParticleLighting );

					//
					//	Dispatch solids :
					//
					device.PipelineState	=	factory[ (int)LightingFlags.SOLIDLIGHTING ];
					device.Dispatch( MathUtil.IntDivUp( width, BlockSizeX ), MathUtil.IntDivUp( height, BlockSizeY ), 1 );

					//
					//	Dispatch particles :
					//
					if (stereoEye!=StereoEye.Right) {
						int threadGroupCount	=	MathUtil.IntDivUp( ParticleSystem.MaxSimulatedParticles, ParticleSystem.BlockSize );
						device.PipelineState	=	factory[ (int)LightingFlags.PARTICLES ];
						device.Dispatch( threadGroupCount, 1, 1 );
					}

				} catch ( UbershaderException e ) {
					Log.Warning("{0}", e.Message );
				}


				//
				//	Add accumulated light  :
				//
				rs.Filter.OverlayAdditive( hdrFrame.HdrBuffer.Surface, hdrFrame.LightAccumulator );

				//	Uncomment to enable SSS :
				#if false
				rs.Filter.GaussBlur( hdrFrame.SSSAccumulator, hdrFrame.LightAccumulator, 5, 0 ); 
				rs.Filter.OverlayAdditive( hdrFrame.HdrBuffer.Surface, hdrFrame.SSSAccumulator );
				#endif

				device.ResetStates();


				if (rs.Config.ShowLightCounters) {
					var ls = viewLayer.LightSet;
					Log.Message("lights: {0,5} omni {1,5} spot {2,5} env", ls.OmniLights.Count, ls.SpotLights.Count, ls.EnvLights.Count );
				}
			}
		}
	}
}
