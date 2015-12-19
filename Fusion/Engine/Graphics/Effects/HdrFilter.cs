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
	public class HdrFilter : GameModule {


		Ubershader	shader;
		ConstantBuffer	paramsCB;
		RenderTarget2D	averageLum;
		StateFactory	factory;
		DynamicTexture	whiteTex;


		//	float AdaptationRate;          // Offset:    0
		//	float LuminanceLowBound;       // Offset:    4
		//	float LuminanceHighBound;      // Offset:    8
		//	float KeyValue;                // Offset:   12
		//	float BloomAmount;             // Offset:   16
		[StructLayout(LayoutKind.Explicit, Size=32)]
		struct Params {
			[FieldOffset( 0)]	public	float	AdaptationRate;
			[FieldOffset( 4)]	public	float 	LuminanceLowBound;
			[FieldOffset( 8)]	public	float	LuminanceHighBound;
			[FieldOffset(12)]	public	float	KeyValue;
			[FieldOffset(16)]	public	float	BloomAmount;
			[FieldOffset(20)]	public	float	DirtMaskLerpFactor;
			[FieldOffset(24)]	public	float	DirtAmount;
		}


		enum Flags {	
			TONEMAPPING		=	0x001,
			MEASURE_ADAPT	=	0x002,
			LINEAR			=	0x004, 
			REINHARD		=	0x008,
			FILMIC			=	0x010,
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public HdrFilter ( Game game ) : base(game)
		{
		}



		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			averageLum	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, 256,256, true, false );
			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );
			whiteTex	=	new DynamicTexture( Game.RenderSystem, 4,4, typeof(Color), false, false);
			whiteTex.SetData( Enumerable.Range(0,16).Select( i=> Color.White ).ToArray() );

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			shader	=	Game.Content.Load<Ubershader>("hdr");
			factory	=	new StateFactory( shader, typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref averageLum	 );
				SafeDispose( ref paramsCB	 );
				SafeDispose( ref whiteTex );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Performs luminance measurement, tonemapping, applies bloom.
		/// </summary>
		/// <param name="target">LDR target.</param>
		/// <param name="hdrImage">HDR source image.</param>
		public void Render ( GameTime gameTime, RenderTargetSurface target, ShaderResource hdrImage, ViewLayerHdr viewLayer )
		{
			var device	=	Game.GraphicsDevice;
			var filter	=	Game.RenderSystem.Filter;

			var settings	=	viewLayer.HdrSettings;

			//
			//	Rough downsampling of source HDR-image :
			//
			filter.StretchRect( averageLum.Surface, hdrImage, SamplerState.PointClamp, null );
			averageLum.BuildMipmaps();

			//
			//	Make bloom :
			//
			filter.StretchRect( viewLayer.Bloom0.Surface, hdrImage, SamplerState.LinearClamp, null );
			viewLayer.Bloom0.BuildMipmaps();

			filter.GaussBlur( viewLayer.Bloom0, viewLayer.Bloom1, settings.GaussBlurSigma, 0 );
			filter.GaussBlur( viewLayer.Bloom0, viewLayer.Bloom1, settings.GaussBlurSigma, 1 );
			filter.GaussBlur( viewLayer.Bloom0, viewLayer.Bloom1, settings.GaussBlurSigma, 2 );
			filter.GaussBlur( viewLayer.Bloom0, viewLayer.Bloom1, settings.GaussBlurSigma, 3 );

			//
			//	Setup parameters :
			//
			var paramsData	=	new Params();
			paramsData.AdaptationRate		=	1 - (float)Math.Pow( 0.5f, gameTime.ElapsedSec / settings.AdaptationHalfLife );
			paramsData.LuminanceLowBound	=	settings.LuminanceLowBound;
			paramsData.LuminanceHighBound	=	settings.LuminanceHighBound;
			paramsData.KeyValue				=	settings.KeyValue;
			paramsData.BloomAmount			=	settings.BloomAmount;
			paramsData.DirtMaskLerpFactor	=	settings.DirtMaskLerpFactor;
			paramsData.DirtAmount			=	settings.DirtAmount;

			paramsCB.SetData( paramsData );
			device.PixelShaderConstants[0]	=	paramsCB;

			//
			//	Measure and adapt :
			//
			device.SetTargets( null, viewLayer.MeasuredNew );

			device.PixelShaderResources[0]	=	averageLum;
			device.PixelShaderResources[1]	=	viewLayer.MeasuredOld;

			device.PipelineState		=	factory[ (int)(Flags.MEASURE_ADAPT) ];
				
			device.Draw( 3, 0 );


			//
			//	Tonemap and compose :
			//
			device.SetTargets( null, target );

			device.PixelShaderResources[0]	=	hdrImage;// averageLum;
			device.PixelShaderResources[1]	=	viewLayer.MeasuredNew;// averageLum;
			device.PixelShaderResources[2]	=	viewLayer.Bloom0;// averageLum;
			device.PixelShaderResources[3]	=	settings.DirtMask1==null ? whiteTex.Srv : settings.DirtMask1.Srv;
			device.PixelShaderResources[4]	=	settings.DirtMask2==null ? whiteTex.Srv : settings.DirtMask2.Srv;
			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;

			Flags op = Flags.LINEAR;
			if (settings.TonemappingOperator==TonemappingOperator.Filmic)   { op = Flags.FILMIC;   }
			if (settings.TonemappingOperator==TonemappingOperator.Linear)   { op = Flags.LINEAR;	 }
			if (settings.TonemappingOperator==TonemappingOperator.Reinhard) { op = Flags.REINHARD; }

			device.PipelineState		=	factory[ (int)(Flags.TONEMAPPING|op) ];
				
			device.Draw( 3, 0 );
			
			device.ResetStates();


			//	swap luminanice buffers :
			Misc.Swap( ref viewLayer.MeasuredNew, ref viewLayer.MeasuredOld );
		}



	}
}
