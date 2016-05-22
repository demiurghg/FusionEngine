using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	internal class SpriteEngine : GameModule {

		enum Flags {
			OPAQUE				=	0x0001, 
			ALPHA_BLEND			=	0x0002, 
			ALPHA_BLEND_PREMUL	=	0x0004, 
			ADDITIVE			=	0x0008, 
			SCREEN				=	0x0010, 
			MULTIPLY			=	0x0020, 
			NEG_MULTIPLY		=	0x0040,
			ALPHA_ONLY			=	0x0080,
		}


		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset( 0)]	public Matrix	Transform;
			[FieldOffset(64)]	public Vector4	ClipRectangle;
			[FieldOffset(80)]	public Color4	MasterColor;
		}

		StateFactory	factory;
		Ubershader		shader;
		GraphicsDevice	device;
		ConstData		constData;
		ConstantBuffer	constBuffer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		public SpriteEngine( RenderSystem rs ) : base(rs.Game)
		{
			this.device	=	rs.Device;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize()
		{
			LoadContent();

			Game.Reloading	+= (s,e) => LoadContent();

			constBuffer	=	new ConstantBuffer( device, typeof(ConstData) );
			constData	=	new ConstData();
		}


		void LoadContent ()
		{
			shader		=	device.Game.Content.Load<Ubershader>("sprite");
			factory		=	shader.CreateFactory( typeof(Flags), (ps,i) => StateEnum( ps, (Flags)i) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void StateEnum ( PipelineState ps, Flags flags )
		{
			switch ( flags ) {																			   
				case Flags.OPAQUE				: ps.BlendState		=	BlendState.Opaque			; break;
				case Flags.ALPHA_BLEND			: ps.BlendState		=	BlendState.AlphaBlend		; break;
				case Flags.ALPHA_BLEND_PREMUL	: ps.BlendState		=	BlendState.AlphaBlendPremul	; break;
				case Flags.ADDITIVE				: ps.BlendState		=	BlendState.Additive			; break;
				case Flags.SCREEN				: ps.BlendState		=	BlendState.Screen			; break;
				case Flags.MULTIPLY				: ps.BlendState		=	BlendState.Multiply			; break;
				case Flags.NEG_MULTIPLY			: ps.BlendState		=	BlendState.NegMultiply		; break;
				case Flags.ALPHA_ONLY			: ps.BlendState		=	BlendState.AlphaMaskWrite	; break;
			}

			ps.RasterizerState		=	RasterizerState.CullNone;
			ps.DepthStencilState	=	DepthStencilState.None;
			ps.Primitive			=	Primitive.TriangleList;
			ps.VertexInputElements	=	VertexInputElement.FromStructure( typeof(SpriteVertex) );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
			}

			base.Dispose( disposing );
		}


		/// <summary>
		/// Draws sprite laters and all sublayers.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawSprites ( GameTime gameTime, StereoEye stereoEye, RenderTargetSurface surface, IEnumerable<SpriteLayer> layers )
		{
			device.ResetStates();
			//device.RestoreBackbuffer();
			device.SetTargets( null, surface );

			DrawSpritesRecursive( gameTime, stereoEye, surface, layers, Matrix.Identity, new Color4(1f,1f,1f,1f) );
		}


		/// <summary>
		/// Draw sprite layers
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="layers"></param>
		void DrawSpritesRecursive ( GameTime gameTime, StereoEye stereoEye, RenderTargetSurface surface, IEnumerable<SpriteLayer> layers, Matrix parentTransform, Color4 parentColor )
		{
			int	w	=	surface.Width;
			int h	=	surface.Height;
			var ofs	=	0f;

			var projection = Matrix.OrthoOffCenterRH(ofs, w + ofs, h + ofs, ofs, -9999, 9999);

			var orderedLayers	=	layers.Where( layer0 => layer0!=null ).OrderBy( layer1 => layer1.Order );

			foreach ( var layer in orderedLayers ) {
				
				if (!layer.Visible) {
					continue;
				}

				using ( new PixEvent("SpriteLayer") ) {

					Matrix absTransform	=	parentTransform * layer.Transform;
					Color4 absColor		=	parentColor * layer.Color.ToColor4();

					constData.Transform		=	absTransform * projection;
					constData.ClipRectangle	=	new Vector4(0,0,0,0);
					constData.MasterColor	=	absColor;

					constBuffer.SetData( constData );

					device.VertexShaderConstants[0]	=	constBuffer;
					device.PixelShaderConstants[0]	=	constBuffer;
				
					PipelineState ps = null;
					SamplerState ss = null;

					switch ( layer.FilterMode ) {
						case SpriteFilterMode.PointClamp		: ss = SamplerState.PointClamp;			break;
						case SpriteFilterMode.PointWrap			: ss = SamplerState.PointWrap;			break;
						case SpriteFilterMode.LinearClamp		: ss = SamplerState.LinearClamp;		break;
						case SpriteFilterMode.LinearWrap		: ss = SamplerState.LinearWrap;			break;
						case SpriteFilterMode.AnisotropicClamp	: ss = SamplerState.AnisotropicClamp;	break;
						case SpriteFilterMode.AnisotropicWrap	: ss = SamplerState.AnisotropicWrap;	break;
					}

					switch ( layer.BlendMode ) {
						case SpriteBlendMode.Opaque				: ps = factory[(int)Flags.OPAQUE			]; break;
						case SpriteBlendMode.AlphaBlend			: ps = factory[(int)Flags.ALPHA_BLEND		]; break;
						case SpriteBlendMode.AlphaBlendPremul	: ps = factory[(int)Flags.ALPHA_BLEND_PREMUL]; break;
						case SpriteBlendMode.Additive			: ps = factory[(int)Flags.ADDITIVE			]; break;
						case SpriteBlendMode.Screen				: ps = factory[(int)Flags.SCREEN			]; break;
						case SpriteBlendMode.Multiply			: ps = factory[(int)Flags.MULTIPLY			]; break;
						case SpriteBlendMode.NegMultiply		: ps = factory[(int)Flags.NEG_MULTIPLY		]; break;
						case SpriteBlendMode.AlphaOnly			: ps = factory[(int)Flags.ALPHA_ONLY		]; break;
					}

					device.PipelineState			=	ps;
					device.PixelShaderSamplers[0]	=	ss;

					layer.Draw( gameTime, stereoEye );

					DrawSpritesRecursive( gameTime, stereoEye, surface, layer.Layers, absTransform, absColor );
				}
			}
		}
	}
}
