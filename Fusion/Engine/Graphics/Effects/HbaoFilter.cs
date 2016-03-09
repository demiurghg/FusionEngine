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

	public class HbaoFilter : GameModule {

		readonly GraphicsDevice rs;

		[Flags]
		enum ShaderFlags : int {
			HBAO								= 1 << 0,
		}

		[StructLayout( LayoutKind.Explicit )]
		struct LinearDepth
		{
			[FieldOffset(0)]	public	float	linearizeDepthA;        
			[FieldOffset(4)]	public	float	linearizeDepthB;        
		}


		Ubershader		shaders;
		StateFactory	factory;


		/// <summary>
		/// Creates instance of HbaoFilter
		/// </summary>
		/// <param name="Game"></param>
		public HbaoFilter( Game Game ) : base( Game )
		{
			rs = Game.GraphicsDevice;
		}



		/// <summary>
		/// Initializes Filter service
		/// </summary>
		public override void Initialize() 
		{
			LoadContent();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			shaders = Game.Content.Load<Ubershader>( "hbao" );
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

			if (flags==ShaderFlags.HBAO) {
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
			rs.ResetStates();
		}

		/*-----------------------------------------------------------------------------------------------
		 * 
		 *	HBAO filter :
		 * 
		-----------------------------------------------------------------------------------------------*/

		void SetViewport ( RenderTargetSurface dst )
		{
			rs.SetViewport( 0,0, dst.Width, dst.Height );
		}



		/// <summary>
		/// Performs good-old StretchRect to destination buffer with blending.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		/// <param name="filter"></param>
		/// <param name="rect"></param>
		public void RenderHbao ( RenderTargetSurface hbaoDst, DepthStencil2D depthSource, RenderTarget2D normalsSource )
		{
			SetDefaultRenderStates();

			using( new PixEvent("HBAO") ) {

				SetViewport( hbaoDst );
				rs.SetTargets( null, hbaoDst );

				rs.PipelineState			=	factory[ (int)(ShaderFlags.HBAO) ];

				rs.VertexShaderResources[0] =	depthSource;
				rs.PixelShaderResources[0]	=	depthSource;
				rs.PixelShaderSamplers[0]	=	SamplerState.PointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}

	}
}
