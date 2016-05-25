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
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Graphics {
	public class DofFilter : GameComponent {

		Ubershader		shader;
		ConstantBuffer	paramsCB;
		StateFactory	factory;


		[StructLayout(LayoutKind.Explicit, Size=16)]
		struct Params {
			[FieldOffset( 0)]	public	float	LinearDepthScale;
			[FieldOffset( 4)]	public	float 	LinearDepthBias;
			[FieldOffset( 8)]	public	float	CocScale;
			[FieldOffset(12)]	public	float	CocBias;
		}


		enum Flags {	
			COC_TO_ALPHA	=	0x001,
			DEPTH_OF_FIELD	=	0x002,
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public DofFilter ( Game game ) : base(game)
		{
		}



		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			shader	=	Game.Content.Load<Ubershader>("dof");
			factory	=	shader.CreateFactory( typeof(Flags), (ps,i) => EnumAction(ps, (Flags)i ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flag"></param>
		void EnumAction ( PipelineState ps, Flags flag )
		{
			ps.BlendState			=	BlendState.Opaque;
			ps.DepthStencilState	=	DepthStencilState.None;
			ps.Primitive			=	Primitive.TriangleList;
			ps.VertexInputElements	=	VertexInputElement.Empty;

			if (flag==Flags.COC_TO_ALPHA) {
				ps.BlendState			=	BlendState.AlphaOnly;
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref paramsCB	 );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Applies DOF effect
		/// </summary>
		public void Render ( GameTime gameTime, RenderTarget2D temp, RenderTarget2D hdrImage, ShaderResource depthBuffer, RenderWorld renderWorld )
		{
			if (!renderWorld.DofSettings.Enabled) {
				return;
			}

			var device	=	Game.GraphicsDevice;
			var filter	=	Game.RenderSystem.Filter;

			device.ResetStates();


			//
			//	Setup parameters :
			//
			var paramsData	=	new Params();
			paramsData.LinearDepthBias	=	renderWorld.Camera.LinearizeDepthBias;
			paramsData.LinearDepthScale	=	renderWorld.Camera.LinearizeDepthScale;	
			paramsData.CocBias			=	renderWorld.DofSettings.CocBias;
			paramsData.CocScale			=	renderWorld.DofSettings.CocScale;

			paramsCB.SetData( paramsData );
			device.PixelShaderConstants[0]	=	paramsCB;

			//
			//	Compute COC and write it in alpha channel :
			//
			device.SetTargets( (DepthStencilSurface)null, hdrImage.Surface );

			device.PixelShaderResources[0]	=	depthBuffer;
			device.PipelineState			=	factory[ (int)(Flags.COC_TO_ALPHA) ];
				
			device.Draw( 3, 0 );


			//
			//	Perform DOF :
			//
			device.SetTargets( null, temp );

			device.PixelShaderResources[0]	=	hdrImage;
			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;
			device.VertexShaderResources[0]	=	hdrImage;
			device.VertexShaderSamplers[0]	=	SamplerState.LinearClamp;

			device.PipelineState			=	factory[ (int)(Flags.DEPTH_OF_FIELD) ];
				
			device.Draw( 3, 0 );
			
			device.ResetStates();


			//
			//	Copy DOFed image back to source :
			//
			filter.Copy( hdrImage.Surface, temp );
		}



	}
}
