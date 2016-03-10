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

	public class SsaoFilter : GameModule {

		const int MaxSamples = 128;

		readonly GraphicsDevice device;
		readonly Random			rand	=	new Random();

		[Flags]
		enum ShaderFlags : int {
			SSAO								= 1 << 0,
		}

		[StructLayout( LayoutKind.Explicit )]
		struct SsaoParams {
			[FieldOffset(  0)]	public	Matrix	View;        
			[FieldOffset( 64)]	public	Matrix	Projection;        
			[FieldOffset(128)]	public	Matrix	InverseProjection;        
		}


		ConstantBuffer	paramsCB;
		ConstantBuffer	randomDirsCB;


		Ubershader		shaders;
		StateFactory	factory;


		/// <summary>
		/// Creates instance of HbaoFilter
		/// </summary>
		/// <param name="Game"></param>
		public SsaoFilter( Game Game ) : base( Game )
		{
			device = Game.GraphicsDevice;
		}



		/// <summary>
		/// Initializes Filter service
		/// </summary>
		public override void Initialize() 
		{
			paramsCB		=	new ConstantBuffer( device, typeof(SsaoParams) );
			randomDirsCB	=	new ConstantBuffer( device, typeof(Vector4), MaxSamples );

			randomDirsCB.SetData( Enumerable.Range(0,MaxSamples).Select( i => new Vector4(rand.UniformRadialDistribution(0,1), 0) ).ToArray() );


			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			shaders = Game.Content.Load<Ubershader>( "ssao" );
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

			if (flags==ShaderFlags.SSAO) {
				ps.BlendState = BlendState.Opaque;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Sets default render state
		/// </summary>
		void SetDefaultRenderStates()
		{
			device.ResetStates();
		}

		/*-----------------------------------------------------------------------------------------------
		 * 
		 *	SSAO filter :
		 * 
		-----------------------------------------------------------------------------------------------*/

		void SetViewport ( RenderTargetSurface dst )
		{
			device.SetViewport( 0,0, dst.Width, dst.Height );
		}



		/// <summary>
		/// Performs good-old StretchRect to destination buffer with blending.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		/// <param name="filter"></param>
		/// <param name="rect"></param>
		public void RenderSsao ( RenderTargetSurface ssaoDst, StereoEye stereoEye, Camera camera, DepthStencil2D depthSource, RenderTarget2D normalsSource )
		{
			SetDefaultRenderStates();

			using( new PixEvent("SSAO") ) {

				var ssaoParams = new SsaoParams();

				ssaoParams.View					=	camera.GetViewMatrix( stereoEye );	
				ssaoParams.Projection			=	camera.GetProjectionMatrix( stereoEye );	
				ssaoParams.InverseProjection	=	Matrix.Invert( ssaoParams.Projection );	
				paramsCB.SetData( ssaoParams );


				SetViewport( ssaoDst );
				device.SetTargets( null, ssaoDst );

				device.PipelineState			=	factory[ (int)(ShaderFlags.SSAO) ];

				device.VertexShaderResources[0] =	depthSource;
				device.PixelShaderResources[0]	=	depthSource;
				device.PixelShaderSamplers[0]	=	SamplerState.PointClamp;

				device.PixelShaderConstants[0]	=	paramsCB;
				device.PixelShaderConstants[1]	=	randomDirsCB;

				device.Draw( 3, 0 );
			}
			device.ResetStates();
		}

	}
}
