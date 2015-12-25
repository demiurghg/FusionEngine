﻿using System;
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
	public class LightRenderer : GameModule {

		const int	BlockSizeX		=	16;
		const int	BlockSizeY		=	16;
		const int	MaxOmniLights	=	1024;
		const int	MaxSpotLights	=	16;

		RenderSystem rs { get { return Game.RenderSystem; } }


		[Config]
		public LightRendererConfig	Config { get; set; }
		
		//public Vector3	DirectLightDirection	=	new Vector3(1,1,1);
		//public Color4	DirectLightIntensity	=	new Color4(10,10,10,1);
		//public Color4	AmbientLevel			=	new Color4(0,0,0,0);

		//List<OmniLight>	omniLights = new List<OmniLight>();
		//List<SpotLight>	spotLights = new List<SpotLight>();


		DepthStencil2D		csmDepth		;
		RenderTarget2D		csmColor		;
		DepthStencil2D		spotDepth		;
		RenderTarget2D		spotColor		;


		OmniLightGPU[]		omniLightData;
		SpotLightGPU[]		spotLightData;
		StructuredBuffer	omniLightBuffer	;
		StructuredBuffer	spotLightBuffer	;

		public RenderTarget2D	CSMColor { get { return csmColor; } }
		public DepthStencil2D	CSMDepth { get { return csmDepth; } }

		public RenderTarget2D	SpotColor { get { return spotColor; } }
		public DepthStencil2D	SpotDepth { get { return spotDepth; } }



		enum LightingFlags {
			NONE			=	0x0000,
			DIRECT			=	0x0001,
			OMNI			=	0x0002,
			SPOT			=	0x0004,
			SHOW_SPLITS		=	0x0008,
			SHOW_OMNI_LOAD	=	0x0010,
			SHOW_SPOT_LOAD	=	0x0020,
			USE_UE4			=	0x0040,
		}


		[StructLayout(LayoutKind.Explicit, Size=560)]
		struct LightingParams {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public Matrix	InverseViewProjection;
			[FieldOffset(192)] public Matrix	CSMViewProjection0;
			[FieldOffset(256)] public Matrix	CSMViewProjection1;
			[FieldOffset(320)] public Matrix	CSMViewProjection2;
			[FieldOffset(384)] public Matrix	CSMViewProjection3;
			[FieldOffset(448)] public Vector4	ViewPosition;
			[FieldOffset(464)] public Vector4	DirectLightDirection;
			[FieldOffset(480)] public Vector4	DirectLightIntensity;
			[FieldOffset(496)] public Vector4	ViewportSize;
			[FieldOffset(512)] public Vector4	CSMFilterRadius;
			[FieldOffset(528)] public Color4	AmbientColor;
			[FieldOffset(544)] public Vector4	Viewport;


		}


		struct OmniLightGPU {
			public Vector4	PositionRadius;
			public Vector4	Intensity;
			public Vector4	ExtentMin;	// x,y, depth
			public Vector4	ExtentMax;	// x,y, depth
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
			omniLightBuffer	=	new StructuredBuffer( Game.GraphicsDevice, typeof(OmniLightGPU), MaxOmniLights, StructuredBufferFlags.None );
			spotLightBuffer	=	new StructuredBuffer( Game.GraphicsDevice, typeof(SpotLightGPU), MaxSpotLights, StructuredBufferFlags.None );

			CreateShadowMaps();

			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			lightingShader	=	Game.Content.Load<Ubershader>("lighting");
			factory			=	new StateFactory( lightingShader, typeof(LightingFlags), Primitive.TriangleList, VertexInputElement.Empty );
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

			csmColor	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  Config.CSMSize * 4, Config.CSMSize );
			csmDepth	=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8, Config.CSMSize * 4, Config.CSMSize );

			spotColor	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  Config.SpotShadowSize * 4, Config.SpotShadowSize * 4 );
			spotDepth	=	new DepthStencil2D( Game.GraphicsDevice, DepthFormat.D24S8, Config.SpotShadowSize * 4, Config.SpotShadowSize * 4 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref csmDepth );
				SafeDispose( ref csmColor );
				SafeDispose( ref spotDepth );
				SafeDispose( ref spotColor );

				SafeDispose( ref lightingCB );
				SafeDispose( ref omniLightBuffer );
				SafeDispose( ref spotLightBuffer );
			}

			base.Dispose( disposing );
		}

		Matrix[] csmViewProjections = new Matrix[4];


		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="projection"></param>
		internal void RenderLighting ( StereoEye stereoEye, Camera camera, HdrFrame frame, ViewLayerHdr viewLayer, Texture occlusionMap, RenderTargetCube envLight )
		{
			using ( new PixEvent("TiledLighting") ) {
				var view		=	camera.GetViewMatrix( stereoEye );
				var projection	=	camera.GetProjectionMatrix( stereoEye );

				var device = Game.GraphicsDevice;
				device.ResetStates();

				LightingFlags	flags = LightingFlags.NONE;

				if (!Config.SkipDirectLight) {
					flags	|=	LightingFlags.DIRECT;

					if (Config.ShowCSMSplits) {
						flags |= LightingFlags.SHOW_SPLITS;
					}
				}
				if (!Config.SkipOmniLights) {
					flags	|=	LightingFlags.OMNI;

					if (Config.ShowOmniLightTileLoad) {
						flags |= LightingFlags.SHOW_OMNI_LOAD;
					}
				}
				if (!Config.SkipSpotLights) {
					flags	|=	LightingFlags.SPOT;

					if (Config.ShowSpotLightTileLoad) {
						flags |= LightingFlags.SHOW_SPOT_LOAD;
					}
				}
				if (Config.UseUE4LightingModel) {
					flags |= LightingFlags.USE_UE4;
				}


				var width	=	frame.HdrBuffer.Width;
				var height	=	frame.HdrBuffer.Height;


				//
				//	Setup compute shader parameters and states :
				//
				try {
					device.PipelineState	=	factory[ (int)flags ];

					//lightingShader.SetComputeShader((int)flags);

					var cbData	=	new LightingParams();
					var invView	=	Matrix.Invert( view );
					var invVP	=	Matrix.Invert( view * projection );
					var viewPos	=	invView.TranslationVector;


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

					PrepareOmniLights( view, projection, viewLayer.LightSet );
					PrepareSpotLights( view, projection, viewLayer.LightSet );

					//
					//	set states :
					//
					device.SetTargets( null, frame.HdrBuffer.Surface );

					lightingCB.SetData( cbData );

					device.ComputeShaderSamplers[0]	=	SamplerState.PointClamp;
					device.ComputeShaderSamplers[1]	=	SamplerState.LinearClamp;
					device.ComputeShaderSamplers[2]	=	SamplerState.ShadowSampler;

					device.ComputeShaderResources[0]	=	frame.DiffuseBuffer;
					device.ComputeShaderResources[1]	=	frame.SpecularBuffer;
					device.ComputeShaderResources[2]	=	frame.NormalMapBuffer;
					device.ComputeShaderResources[3]	=	frame.DepthBuffer;
					device.ComputeShaderResources[4]	=	csmColor;
					device.ComputeShaderResources[5]	=	spotColor;
					device.ComputeShaderResources[6]	=	viewLayer.LightSet.SpotAtlas==null ? rs.WhiteTexture.Srv : viewLayer.LightSet.SpotAtlas.Texture.Srv;
					device.ComputeShaderResources[7]	=	omniLightBuffer;
					device.ComputeShaderResources[8]	=	spotLightBuffer;
					device.ComputeShaderResources[9]	=	occlusionMap.Srv;
					device.ComputeShaderResources[10]	=	envLight;

					device.ComputeShaderConstants[0]	=	lightingCB;

					device.SetCSRWTexture( 0, frame.LightAccumulator.Surface );

					//
					//	Dispatch :
					//
					device.Dispatch( MathUtil.IntDivUp( width, BlockSizeX ), MathUtil.IntDivUp( height, BlockSizeY ), 1 );

				} catch ( UbershaderException e ) {
					Log.Warning("{0}", e.Message );
				}


				//
				//	Add accumulated light  :
				//
				rs.Filter.OverlayAdditive( frame.HdrBuffer.Surface, frame.LightAccumulator );

				device.ResetStates();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		internal void RenderShadows ( ViewLayerHdr viewLayer, LightSet lightSet )
		{
			var camera		=	viewLayer.Camera;
			var instances	=	viewLayer.Instances;

			if (Config.SkipShadows) {
				return;
			}

			Game.GraphicsDevice.ResetStates();
			
			if ( csmDepth.Height!=Config.CSMSize || spotDepth.Height!=Config.SpotShadowSize * 4) {
				CreateShadowMaps();
			}

			var device = Game.GraphicsDevice;

			device.Clear( csmDepth.Surface, 1, 0 );
			device.Clear( csmColor.Surface, Color4.White );

			Matrix[] shadowViews, shadowProjections;

			//	shadow is computed for both eyes :
			var view = camera.GetViewMatrix( StereoEye.Mono );

			ComputeCSMMatricies( view, viewLayer.LightSet.DirectLight.Direction, out shadowViews, out shadowProjections, out csmViewProjections );

			for (int i=0; i<4; i++) {

				var smSize = Config.CSMSize;
				var context = new ShadowContext();
				context.ShadowView			=	shadowViews[i];
				context.ShadowProjection	=	shadowProjections[i];
				context.ShadowViewport		=	new Viewport( smSize * i, 0, smSize, smSize );
				context.FarDistance			=	1;
				context.SlopeBias			=	Config.CSMSlopeBias;
				context.DepthBias			=	Config.CSMDepthBias;
				context.ColorBuffer			=	csmColor.Surface;
				context.DepthBuffer			=	csmDepth.Surface;

				Game.RenderSystem.SceneRenderer.RenderShadowMapCascade( context, instances );
			}


			//
			//	Spot-Lights :
			//
			device.Clear( spotDepth.Surface, 1, 0 );
			device.Clear( spotColor.Surface, Color4.White );
			int index = 0;

			foreach ( var spot in lightSet.SpotLights ) {

				var smSize	= Config.SpotShadowSize;
				var context = new ShadowContext();
				var dx      = index % 4;
				var dy		= index / 4;
				var far		= spot.Projection.GetFarPlaneDistance();

				index++;

				context.ShadowView			=	spot.SpotView;
				context.ShadowProjection	=	spot.Projection;
				context.ShadowViewport		=	new Viewport( smSize * dx, smSize * dy, smSize, smSize );
				context.FarDistance			=	far;
				context.SlopeBias			=	Config.SpotSlopeBias;
				context.DepthBias			=	Config.SpotDepthBias;
				context.ColorBuffer			=	spotColor.Surface;
				context.DepthBuffer			=	spotDepth.Surface;

				Game.RenderSystem.SceneRenderer.RenderShadowMapCascade( context, instances );
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		void ComputeCSMMatricies ( Matrix view, Vector3 lightDir2, out Matrix[] shadowViews, out Matrix[] shadowProjections, out Matrix[] shadowViewProjections )
		{
			shadowViews				=	new Matrix[4];
			shadowProjections		=	new Matrix[4];
			shadowViewProjections	=	new Matrix[4];

			var	smSize		=	Config.CSMSize;
			var camMatrix	=	Matrix.Invert( view );
			var viewPos		=	camMatrix.TranslationVector;


			for ( int i = 0; i<4; i++ ) {

				float	offset		=	Config.SplitOffset * (float)Math.Pow( Config.SplitFactor, i );
				float	radius		=	Config.SplitSize   * (float)Math.Pow( Config.SplitFactor, i );

				Vector3 viewDir		=	camMatrix.Forward.Normalized();
				Vector3	lightDir	=	lightDir2.Normalized();
				Vector3	origin		=	viewPos + viewDir * offset;

				Matrix	lightRot	=	Matrix.LookAtRH( Vector3.Zero, Vector3.Zero + lightDir, Vector3.UnitY );
				Matrix	lightRotI	=	Matrix.Invert( lightRot );
				Vector3	lsOrigin	=	Vector3.TransformCoordinate( origin, lightRot );
				float	snapValue	=	4.0f * radius / smSize;
				lsOrigin.X			=	(float)Math.Round(lsOrigin.X / snapValue) * snapValue;
				lsOrigin.Y			=	(float)Math.Round(lsOrigin.Y / snapValue) * snapValue;
				lsOrigin.Z			=	(float)Math.Round(lsOrigin.Z / snapValue) * snapValue;
				origin				=	Vector3.TransformCoordinate( lsOrigin, lightRotI );//*/

				shadowViews[i]				=	Matrix.LookAtRH( origin, origin + lightDir, Vector3.UnitY );
				shadowProjections[i]		=	Matrix.OrthoRH( radius*2, radius*2, -Config.CSMDepth/2, Config.CSMDepth/2);

				shadowViewProjections[i]	=	shadowViews[i] * shadowProjections[i];
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void PrepareOmniLights ( Matrix view, Matrix proj, LightSet lightSet )
		{
			//t totalTileTo//

			var vp = Game.GraphicsDevice.DisplayBounds;

			omniLightData = Enumerable
					.Range(0,MaxOmniLights)
					.Select( i => new OmniLightGPU(){ PositionRadius = Vector4.Zero, Intensity = Vector4.Zero })
					.ToArray();

			int index = 0;

			foreach ( var light in lightSet.OmniLights ) {

				Vector4 min, max;

				var visible = GetSphereExtent( view, proj, light.Position, vp, light.RadiusOuter, out min, out max );

				if (!visible) {
					continue;
				}

				omniLightData[index].PositionRadius	=	new Vector4( light.Position, light.RadiusOuter );
				omniLightData[index].Intensity		=	new Vector4( light.Intensity.ToVector3(), 1.0f / light.RadiusOuter / light.RadiusOuter );
				omniLightData[index].ExtentMax		=	max;
				omniLightData[index].ExtentMin		=	min;

				index++;
			}

			//#warning Debug omni-lights.
			#if false
			if (Config.ShowOmniLights) {
				var dr	=	Game.GetService<DebugRender>();

				foreach ( var light in omniLights ) {
					dr.DrawPoint( light.Position, 1, Color.LightYellow );
					dr.DrawSphere( light.Position, light.RadiusOuter, Color.LightYellow, 16 );
				}
			}
			#endif

			omniLightBuffer.SetData( omniLightData );
		}



		/// <summary>
		/// 
		/// </summary>
		void PrepareSpotLights ( Matrix view, Matrix projection, LightSet lightSet )
		{
			var znear	=	projection.M34 * projection.M43 / projection.M33;
			var vp		=	Game.GraphicsDevice.DisplayBounds;
			//var dr		=	Game.GetService<DebugRender>();
			//var sb		=	Game.GetService<SpriteBatch>();

			spotLightData	=	Enumerable
							.Range(0, MaxSpotLights)
							.Select( i => new SpotLightGPU() )
							.ToArray();

			int index	=	0;
			int spotId	=	0;

			
			foreach ( var spot in lightSet.SpotLights ) {

				var shadowSO	=	new Vector4( 0.125f, -0.125f, 0.25f*(spotId % 4)+0.125f, 0.25f*(spotId / 4)+0.125f );
				spotId ++;
				
				var maskRect	=	lightSet.SpotAtlas.GetSubImageRectangle( spot.MaskName );
				var maskX		=	maskRect.Left   / (float)lightSet.SpotAtlas.Texture.Width;
				var maskY		=	maskRect.Top    / (float)lightSet.SpotAtlas.Texture.Height;
				var maskW		=	maskRect.Width  / (float)lightSet.SpotAtlas.Texture.Width;
				var maskH		=	maskRect.Height / (float)lightSet.SpotAtlas.Texture.Height;
				var maskSO		=	new Vector4( maskW*0.5f, -maskH*0.5f, maskX + maskW/2f, maskY + maskH/2f );

				var bf = new BoundingFrustum( spot.SpotView * spot.Projection );
				var pos = Matrix.Invert(spot.SpotView).TranslationVector;

				//#warning Debug spot-lights.
				#if false
				if (Config.ShowSpotLights) {
					dr.DrawPoint( pos, 0.5f, Color.LightYellow );
					dr.DrawFrustum( bf, Color.LightYellow );
				}
				#endif

				Vector4 min, max;

				bool r = GetFrustumExtent( view, projection, vp, bf, out min, out max );

				if (r) {
					spotLightData[index].ViewProjection		=	spot.SpotView * spot.Projection;
					spotLightData[index].PositionRadius		=	new Vector4( pos, spot.RadiusOuter );
					spotLightData[index].IntensityFar		=	spot.Intensity.ToVector4();
					spotLightData[index].IntensityFar.W		=	spot.Projection.GetFarPlaneDistance();
					spotLightData[index].ExtentMin			=	min;
					spotLightData[index].ExtentMax			=	max;
					spotLightData[index].MaskScaleOffset	=	maskSO;
					spotLightData[index].ShadowScaleOffset	=	shadowSO;
					index ++;
				}

			}

			spotLightBuffer.SetData( spotLightData );
		}



		class Line {
			public Line ( Vector3 a, Vector3 b ) { A = a; B = b; }
			public Vector3 A;
			public Vector3 B;
			
			/// <summary>
			/// Returns true if line is visible
			/// </summary>
			/// <param name="znear"></param>
			/// <returns></returns>
			public bool Clip ( float znear ) 
			{
				if ( A.Z <= znear && B.Z <= znear ) {
					return true;
				}
				if ( A.Z >= znear && B.Z >= znear ) {
					return false;
				}

				var factor	=	( znear - A.Z ) / ( B.Z - A.Z );
				var point	=	Vector3.Lerp( A, B, factor );
				
				if ( A.Z > znear ) A = point;
				if ( B.Z > znear ) B = point;

				return true;
			}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="projection"></param>
		/// <param name="frustum"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		bool GetFrustumExtent ( Matrix view, Matrix projection, Rectangle viewport, BoundingFrustum frustum, out Vector4 min, out Vector4 max )
		{
			min = max	=	Vector4.Zero;

			var znear	=	projection.M34 * projection.M43 / projection.M33;
			
			var viewPoints = frustum.GetCorners()
					.Select( p0 => Vector3.TransformCoordinate( p0, view ) )
					.ToArray();

			//var dr		=	Game.GetService<DebugRender>();

			var lines = new[]{
				new Line( viewPoints[0], viewPoints[1] ),
				new Line( viewPoints[1], viewPoints[2] ),
				new Line( viewPoints[2], viewPoints[3] ),
				new Line( viewPoints[3], viewPoints[0] ),
														
				new Line( viewPoints[4], viewPoints[5] ),
				new Line( viewPoints[5], viewPoints[6] ),
				new Line( viewPoints[6], viewPoints[7] ),
				new Line( viewPoints[7], viewPoints[4] ),
													
				new Line( viewPoints[0], viewPoints[4] ),
				new Line( viewPoints[1], viewPoints[5] ),
				new Line( viewPoints[2], viewPoints[6] ),
				new Line( viewPoints[3], viewPoints[7] ),
			};

			lines = lines.Where( line => line.Clip(znear) ).ToArray();

			if (!lines.Any()) {
				return false;
			}

			var projPoints = new List<Vector3>();
			
			foreach ( var line in lines ) {
				projPoints.Add( Vector3.TransformCoordinate( line.A, projection ) );
				projPoints.Add( Vector3.TransformCoordinate( line.B, projection ) );
			}

			min.X	=	projPoints.Min( p => p.X );
			min.Y	=	projPoints.Max( p => p.Y );
			min.Z	=	projPoints.Min( p => p.Z );

			max.X	=	projPoints.Max( p => p.X );
			max.Y	=	projPoints.Min( p => p.Y );
			max.Z	=	projPoints.Max( p => p.Z );

			min.X	=	( min.X *  0.5f + 0.5f ) * viewport.Width;
			min.Y	=	( min.Y * -0.5f + 0.5f ) * viewport.Height;

			max.X	=	( max.X *  0.5f + 0.5f ) * viewport.Width;
			max.Y	=	( max.Y * -0.5f + 0.5f ) * viewport.Height;

			return true;
		} 



		/// <summary>
		/// 
		/// </summary>
		/// <param name="projection"></param>
		/// <param name="viewPos"></param>
		/// <param name="radius"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		bool GetSphereExtent ( Matrix view, Matrix projection, Vector3 position, Rectangle vp, float radius, out Vector4 min, out Vector4 max )
		{
			min = max	=	Vector4.Zero;

			var znear	=	projection.M34 * projection.M43 / projection.M33;
			var nearW	=	projection.M11;
			var nearH	=	projection.M22;
			var viewPos	=	Vector3.TransformCoordinate( position, view );

			Vector3 min3, max3;
			

			var r0		=	GetSphereExtentAxis( znear, viewPos.X, viewPos.Z, radius, out min3.X, out max3.X );
			var r1		=	GetSphereExtentAxis( znear, viewPos.Y, viewPos.Z, radius, out min3.Y, out max3.Y );

			max3.Z		=	min3.Z	=	znear;
			var maxP	=	Vector3.TransformCoordinate( max3, projection );
			var minP	=	Vector3.TransformCoordinate( min3, projection );

			min.X		=	( minP.X * 0.5f + 0.5f ) * vp.Width;
			max.X		=	( maxP.X * 0.5f + 0.5f ) * vp.Width;

			max.Y		=	( minP.Y * -0.5f + 0.5f ) * vp.Height;
			min.Y		=	( maxP.Y * -0.5f + 0.5f ) * vp.Height;

			min.Z		=	Vector3.TransformCoordinate( new Vector3(0,0, Math.Min( viewPos.Z + radius, znear )), projection ).Z;
			max.Z		=	Vector3.TransformCoordinate( new Vector3(0,0, Math.Min( viewPos.Z - radius, znear )), projection ).Z;

			//Game.GetService<DebugStrings>().Add("Z-min = {0} | Z-max = {1}", min.Z, max.Z );

			if (!r0) {
				return false;
			}

			return true;
		}


		float sqrt( float x ) { return (float)Math.Sqrt(x); }
		float square( float x ) { return x*x; }
		float exp( float x ) { return (float)Math.Exp(x); }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="znear"></param>
		/// <param name="a"></param>
		/// <param name="z"></param>
		/// <param name="r"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		bool GetSphereExtentAxis ( float znear, float a, float z, float r, out float min, out float max )
		{
			min = max = 0;

			if (z>r-znear) {
				return false;
			}

			var c		=	new Vector2( a, z );
			var t		=	sqrt( c.LengthSquared() - r * r );
			var cLen	=	c.Length();
	 		var cosT	=	t / cLen;
			var sinT	=	r / cLen;

			c.X /= cLen;
			c.Y /= cLen;

			var T		=	new Vector2( cosT * c.X - sinT * c.Y, +sinT * c.X + cosT * c.Y ) * t; 
			var B		=	new Vector2( cosT * c.X + sinT * c.Y, -sinT * c.X + cosT * c.Y ) * t; 

			var tau		=	new Vector2( a + sqrt( r*r - square(znear-z) ), znear );
			var beta	=	new Vector2( a - sqrt( r*r - square(znear-z) ), znear );

			var U		=	T.Y < znear ? T : tau;
			var L		=	B.Y < znear ? B : beta;

			max			=	U.X / U.Y * znear;
			min			=	L.X / L.Y * znear;

			return true;
		}
	}
}
