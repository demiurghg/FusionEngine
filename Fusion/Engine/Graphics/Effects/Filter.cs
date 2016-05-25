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

namespace Fusion.Engine.Graphics
{
	/// <summary>
	/// Class for base image processing such as copying, blurring, enhancement, anti-aliasing etc.
	/// </summary>
	public class Filter : GameComponent {
		readonly GraphicsDevice rs;

		const int MaxBlurTaps	=	33;

		[Flags]
		enum ShaderFlags : int
		{
			PASS1								= 1 << 0,
			PASS2								= 1 << 1,
			FXAA								= 1 << 2,
			COPY								= 1 << 3,
			STRETCH_RECT						= 1 << 4,
			DOWNSAMPLE_2_4x4					= 1 << 5,
			DOWNSAMPLE_4						= 1 << 6,
			GAUSS_BLUR_3x3						= 1 << 7,
			GAUSS_BLUR							= 1 << 8,
			TO_CUBE_FACE						= 1 << 9,
			LINEARIZE_DEPTH						= 1 << 10,
			RESOLVE_AND_LINEARIZE_DEPTH_MSAA	= 1 << 11,
			OVERLAY_ADDITIVE					= 1 << 12,
			PREFILTER_ENVMAP					= 1 << 13,
			POSX								= 1 << 14,
			POSY								= 1 << 15,
			POSZ								= 1 << 16,
			NEGX								= 1 << 17,
			NEGY								= 1 << 18,
			NEGZ								= 1 << 19,
			FILL_ALPHA_ONE						= 1 << 20,
			BILATERAL							= 1 << 21,

		}

		[StructLayout( LayoutKind.Explicit )]
		struct LinearDepth
		{
			[FieldOffset(0)]	public	float	linearizeDepthA;        
			[FieldOffset(4)]	public	float	linearizeDepthB;        
		}


		Ubershader		shaders;
		StateFactory	factory;
		ConstantBuffer	gaussWeightsCB;
		ConstantBuffer	sourceRectCB;
		ConstantBuffer	matrixCB;
		ConstantBuffer	vectorCB;
		ConstantBuffer	bufLinearizeDepth;
		
		public Filter( Game Game ) : base( Game )
		{
			rs = Game.GraphicsDevice;
		}



		/// <summary>
		/// Initializes Filter service
		/// </summary>
		public override void Initialize() 
		{
			bufLinearizeDepth	= new ConstantBuffer( rs, 128 );
			gaussWeightsCB		= new ConstantBuffer( rs, typeof(Vector4), MaxBlurTaps );
			sourceRectCB		= new ConstantBuffer( rs, typeof(Vector4) );
			matrixCB			= new ConstantBuffer( rs, typeof(Matrix), 1 );
			vectorCB			= new ConstantBuffer( rs, typeof(Vector4), 1 );

			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			shaders = Game.Content.Load<Ubershader>( "filter" );
			factory	= shaders.CreateFactory( typeof(ShaderFlags), (ps,i) => Enum(ps, (ShaderFlags)i) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void Enum ( PipelineState ps, ShaderFlags flags )
		{
			ps.Primitive			=	Primitive.TriangleList;
			ps.VertexInputElements	=	VertexInputElement.Empty;
			ps.BlendState			=	BlendState.Opaque;
			ps.RasterizerState		=	RasterizerState.CullNone;
			ps.DepthStencilState	=	DepthStencilState.None;

			if (flags==ShaderFlags.OVERLAY_ADDITIVE) {
				ps.BlendState = BlendState.Additive;
			}

			if (flags==ShaderFlags.FILL_ALPHA_ONE) {
				ps.BlendState = BlendState.AlphaMaskWrite;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				SafeDispose( ref gaussWeightsCB );
				SafeDispose( ref sourceRectCB );
				SafeDispose( ref bufLinearizeDepth );
				SafeDispose( ref matrixCB );
				SafeDispose( ref vectorCB );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Sets default render state
		/// </summary>
		void SetDefaultRenderStates()
		{
			rs.ResetStates();
		}

		/*-----------------------------------------------------------------------------------------------
		 * 
		 *	Base filters
		 * 
		-----------------------------------------------------------------------------------------------*/

		/// <summary>
		/// Performs good-old StretchRect to destination buffer with blending.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		/// <param name="filter"></param>
		/// <param name="rect"></param>
		public void StretchRect( RenderTargetSurface dst, ShaderResource src, SamplerState filter = null, bool flipToCubeFace = false )
		{
			SetDefaultRenderStates();

			using( new PixEvent("StretchRect") ) {

				SetViewport(dst);
				rs.SetTargets( null, dst );

				if (flipToCubeFace) {
					rs.PipelineState		=	factory[ (int)(ShaderFlags.STRETCH_RECT|ShaderFlags.TO_CUBE_FACE) ];
				} else {
					rs.PipelineState		=	factory[ (int)ShaderFlags.STRETCH_RECT ];
				}
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	filter ?? SamplerState.LinearPointClamp;
				rs.VertexShaderConstants[0]	=	sourceRectCB;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void StretchRect4x4( RenderTargetSurface dst, RenderTarget2D src, SamplerState filter = null, bool flipToCubeFace = false )
		{
			SetDefaultRenderStates();

			using( new PixEvent("StretchRect4x4") ) {

				rs.SetTargets( null, dst );
				SetViewport(dst);
				
				if (flipToCubeFace) {
					rs.PipelineState		=	factory[ (int)(ShaderFlags.DOWNSAMPLE_2_4x4|ShaderFlags.TO_CUBE_FACE) ];
				} else {
					rs.PipelineState		=	factory[ (int)ShaderFlags.DOWNSAMPLE_2_4x4 ];
				}
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	filter ?? SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void DownSample4( RenderTarget2D dst, RenderTarget2D src )
		{
			SetDefaultRenderStates();

			using( new PixEvent("DownSample4") ) {

				dst.SetViewport();
				rs.SetTargets( null, dst );

				rs.PipelineState			=	factory[ (int)ShaderFlags.DOWNSAMPLE_4 ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void LinearizeDepth( RenderTargetSurface dst, ShaderResource src )
		{
			throw new NotImplementedException();
		#if false
			Debug.Assert( Game.IsServiceExist<Camera>() );

			var camera = Game.GetService<Camera>();

			bufLinearizeDepth.Data.linearizeDepthA = 1.0f / camera.FrustumZFar - 1.0f / camera.FrustumZNear;
			bufLinearizeDepth.Data.linearizeDepthB = 1.0f / camera.FrustumZNear;
			bufLinearizeDepth.UpdateCBuffer();


			var isDepthMSAA = ( src.SampleCount > 1 );
			var depthShader = (int)( isDepthMSAA ? ShaderFlags.RESOLVE_AND_LINEARIZE_DEPTH_MSAA : ShaderFlags.LINEARIZE_DEPTH );

			string signature;

			SetDefaultRenderStates();

			using( new PixEvent() ) {
				bufLinearizeDepth.SetCBufferPS( 0 );

				shaders.SetPixelShader( depthShader );
				shaders.SetVertexShader( depthShader );

				dst.SetViewport();
				rs.SetRenderTargets( dst );
				src.SetPS( 0 );

				rs.Draw( Primitive.TriangleList, 3, 0 );
			}
			rs.ResetStates();
		#endif
		}



		void SetViewport ( RenderTargetSurface dst )
		{
			rs.SetViewport( 0,0, dst.Width, dst.Height );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst">target to copy to</param>
		/// <param name="src">target to copy from</param>
		public void Copy( RenderTargetSurface dst, ShaderResource src )
		{
			SetDefaultRenderStates();

			using( new PixEvent("Copy") ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					SetViewport(dst);
					rs.SetTargets( null, dst );
				}

				rs.PipelineState			=	factory[ (int)ShaderFlags.COPY ];
				rs.PixelShaderResources[0]	= src;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst">target to copy to</param>
		/// <param name="src">target to copy from</param>
		public void FillAlphaOne( RenderTargetSurface dst )
		{
			SetDefaultRenderStates();

			using( new PixEvent("FillAlphaOne") ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					SetViewport(dst);
					rs.SetTargets( null, dst );
				}

				rs.PipelineState = factory[ (int)ShaderFlags.FILL_ALPHA_ONE ];

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst">target to copy to</param>
		/// <param name="src">target to copy from</param>
		public void OverlayAdditive( RenderTargetSurface dst, ShaderResource src )
		{
			SetDefaultRenderStates();

			using( new PixEvent("OverlayAdditive") ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					rs.SetTargets( null, dst );
				}

				rs.PipelineState			=	factory[ (int)ShaderFlags.OVERLAY_ADDITIVE ];
				rs.PixelShaderResources[0]	=	src;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// Performs FXAA antialiasing.
		/// </summary>
		/// <param name="dst">Target buffer to render FXAA to</param>
		/// <param name="src">Source image with luminance in alpha</param>
		public void Fxaa( RenderTargetSurface dst, ShaderResource src )
		{
			SetDefaultRenderStates();

			using( new PixEvent("Fxaa") ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					SetViewport( dst );
					rs.SetTargets( null, dst );
				}

				rs.PipelineState			=	factory[ (int)ShaderFlags.FXAA ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="cubeSrc"></param>
		/// <param name="sampleCount"></param>
		public void PrefilterEnvMap ( RenderTargetCube envMap )
		{
			SetDefaultRenderStates();

			int width  = envMap.Width / 2;
			int height = envMap.Height / 2;

			using( new PixEvent("PrefilterEnvMap") ) {

				var sides =	new[]{ ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.POSX, ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.NEGX, 
								   ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.POSY, ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.NEGY, 
								   ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.POSZ, ShaderFlags.PREFILTER_ENVMAP | ShaderFlags.NEGZ };

				//	loop through mip levels from second to last specular mip level :
				for (int mip=1; mip<RenderSystem.EnvMapSpecularMipCount; mip++) {

					float roughness = (float)mip / (float)(RenderSystem.EnvMapSpecularMipCount-1);
					float step		= 1.0f / width;

					vectorCB.SetData( new Vector4( roughness, step,0,0 ) );
					
								
					for (int face=0; face<6; face++) {

						rs.SetTargets( null, envMap.GetSurface( mip, (CubeFace)face ) );
					
						rs.SetViewport( 0,0, width, height );

						rs.PixelShaderConstants[0]	=	vectorCB;
						rs.PipelineState			=	factory[ (int)sides[face] ];
						rs.VertexShaderResources[0] =	envMap.GetCubeShaderResource( mip-1 );
						rs.PixelShaderResources[0]	=	envMap.GetCubeShaderResource( mip-1 );
						rs.PixelShaderSamplers[0]	=	SamplerState.LinearWrap;
						rs.VertexShaderConstants[0]	=	matrixCB;

						rs.Draw( 3, 0 );
					}

					width /= 2;
					height /= 2;
				}
			}


			rs.ResetStates();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst">source and destination target</param>
		/// <param name="temporary">temporaru target for two pass filter</param>
		/*public void GaussBlur3x3( RenderTarget2D srcDst, RenderTarget2D temporary )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {
				srcDst.SetViewport();
				rs.PixelShaderSamplers[0] = SamplerState.LinearPointClamp;

				rs.PipelineState			=	factory[ (int)ShaderFlags.GAUSS_BLUR_3x3 ];
                shaders.SetPixelShader( (int)(ShaderFlags.GAUSS_BLUR_3x3) );
                shaders.SetVertexShader( (int)(ShaderFlags.GAUSS_BLUR_3x3) );

                rs.SetTargets( null, temporary );
				rs.VertexShaderResources[0] = srcDst;
				rs.PixelShaderResources[0] = srcDst;
                
				rs.Draw( Primitive.TriangleList, 3, 0 );

                shaders.SetPixelShader( (int)(ShaderFlags.GAUSS_BLUR_3x3 | ShaderFlags.PASS2) );
                shaders.SetVertexShader( (int)(ShaderFlags.GAUSS_BLUR_3x3 | ShaderFlags.PASS2) );

                rs.SetTargets( null, srcDst );
				rs.VertexShaderResources[0] = temporary;
				rs.PixelShaderResources[0] = temporary;

                rs.Draw( Primitive.TriangleList, 3, 0 );
			}
			rs.ResetStates();
		}	*/

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst"></param>
		/// <param name="temporary"></param>
		/// <param name="sigma"></param>
		/// <param name="mipLevel"></param>
		public void GaussBlur ( RenderTarget2D srcDst, RenderTarget2D temporary, float sigma, int mipLevel )
		{
			GaussBlurInternal( srcDst, temporary, sigma, 0f, mipLevel, null, null );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst"></param>
		/// <param name="temporary"></param>
		/// <param name="?"></param>
		/// <param name="depthData"></param>
		/// <param name="normalData"></param>
		/// <param name="sigma"></param>
		/// <param name="mipLevel"></param>
		public void GaussBlurBilateral ( RenderTarget2D srcDst, RenderTarget2D temporary, ShaderResource depthData, ShaderResource normalData, float sigma, float sharpness, int mipLevel )
		{
			GaussBlurInternal( srcDst, temporary, sigma, sharpness, mipLevel, depthData, normalData );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst"></param>
		/// <param name="temporary"></param>
		/// <param name="sigma"></param>
		/// <param name="kernelSize"></param>
		void GaussBlurInternal ( RenderTarget2D srcDst, RenderTarget2D temporary, float sigma, float sharpness, int mipLevel, ShaderResource depthData, ShaderResource normalData )
		{
			var taps = GetGaussWeightsBuffer( sigma, mipLevel );

			SetDefaultRenderStates();

			gaussWeightsCB.SetData( taps );


			int combination	=	(int)ShaderFlags.GAUSS_BLUR;

			if (depthData!=null && normalData!=null) {
				combination |=	(int)ShaderFlags.BILATERAL;
			}



			using( new PixEvent("GaussBlur") ) {

				SetViewport(temporary.GetSurface(mipLevel));
				rs.SetTargets( null, temporary.GetSurface(mipLevel) );

				rs.PipelineState			=	factory[ combination|(int)ShaderFlags.PASS1 ];
				rs.VertexShaderResources[0]	=	srcDst;
				rs.PixelShaderResources[0]	=	srcDst;
				rs.PixelShaderResources[1]	=	depthData;
				rs.PixelShaderResources[2]	=	normalData;

				rs.PixelShaderConstants[0]	=	gaussWeightsCB;
				
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;
				rs.PixelShaderSamplers[1]	=	SamplerState.PointClamp;

				rs.Draw( 3, 0 );



				rs.VertexShaderResources[0] =	null;
				rs.PixelShaderResources[0]	=	null;

				SetViewport(srcDst.GetSurface(mipLevel));
				rs.SetTargets( null, srcDst.GetSurface(mipLevel) );

				rs.PipelineState			=	factory[ combination|(int)ShaderFlags.PASS2 ];
				rs.VertexShaderResources[0] =	temporary;
				rs.PixelShaderResources[0]	=	temporary;
				rs.PixelShaderResources[1]	=	depthData;
				rs.PixelShaderResources[2]	=	normalData;

				rs.PixelShaderConstants[0]	=	gaussWeightsCB;

				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;
				rs.PixelShaderSamplers[1]	=	SamplerState.PointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		float GaussDistribution ( float x, float sigma )
		{
			var k1 =  1.0 / (sigma * Math.Sqrt(2.0*Math.PI));
			var k2 = -1.0 / (2.0 * sigma * sigma);
			return (float)( k1 * Math.Exp( k2 * x * x ) );
		}


		Vector4[] GetGaussWeightsBuffer( float sigma, int mipLevel ) 
		{
			var taps = new Vector4[MaxBlurTaps];

			for ( int i=0; i<MaxBlurTaps; i++) {

				float x = i - (MaxBlurTaps/2);

				taps[i].X = GaussDistribution( x, sigma );
				taps[i].Y = mipLevel;
				taps[i].W = x * (1 << mipLevel);
			}

			return taps;
			//bufGaussWeights.UpdateCBuffer();
			//#endif
		}
	}
}
