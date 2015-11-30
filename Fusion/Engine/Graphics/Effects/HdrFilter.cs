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
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Graphics {
	public class HdrFilter : GameModule {


		Ubershader	shader;
		ConstantBuffer	paramsCB;
		RenderTarget2D	averageLum;
		StateFactory	factory;


		Texture2D		bloomMask;

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
		public HdrFilter ( GameEngine game ) : base(game)
		{
		}



		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			averageLum	=	new RenderTarget2D( GameEngine.GraphicsDevice, ColorFormat.Rgba16F, 256,256, true, false );
			paramsCB	=	new ConstantBuffer( GameEngine.GraphicsDevice, typeof(Params) );

			LoadContent();

			GameEngine.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			shader	=	GameEngine.Content.Load<Ubershader>("hdr");
			factory	=	new StateFactory( shader, typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );

			bloomMask	=	GameEngine.Content.Load<Texture2D>("bloomMask");
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
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Performs luminance measurement, tonemapping, applies bloom.
		/// </summary>
		/// <param name="target">LDR target.</param>
		/// <param name="hdrImage">HDR source image.</param>
		public void Render ( GameTime gameTime, RenderTargetSurface target, ShaderResource hdrImage, ViewLayer viewLayer )
		{
			var device	=	GameEngine.GraphicsDevice;
			var filter	=	GameEngine.GraphicsEngine.Filter;

			var rect	=	new Rectangle( 0,0, viewLayer.Width, viewLayer.Height );

			var settings	=	viewLayer.HdrSettings;

			//
			//	Rough downsampling of source HDR-image :
			//
			filter.StretchRect( averageLum.Surface, hdrImage, SamplerState.PointClamp, rect );
			averageLum.BuildMipmaps();

			//
			//	Make bloom :
			//
			filter.StretchRect( viewLayer.bloom0.Surface, hdrImage, SamplerState.LinearClamp, rect );
			viewLayer.bloom0.BuildMipmaps();

			filter.GaussBlur( viewLayer.bloom0, viewLayer.bloom1, settings.GaussBlurSigma, 0 );
			filter.GaussBlur( viewLayer.bloom0, viewLayer.bloom1, settings.GaussBlurSigma, 1 );
			filter.GaussBlur( viewLayer.bloom0, viewLayer.bloom1, settings.GaussBlurSigma, 2 );
			filter.GaussBlur( viewLayer.bloom0, viewLayer.bloom1, settings.GaussBlurSigma, 3 );


			//
			//	Setup parameters :
			//
			var paramsData	=	new Params();
			paramsData.AdaptationRate		=	1 - (float)Math.Pow( 0.5f, gameTime.ElapsedSec / settings.AdaptationHalfLife );
			paramsData.LuminanceLowBound	=	settings.LuminanceLowBound;
			paramsData.LuminanceHighBound	=	settings.LuminanceHighBound;
			paramsData.KeyValue				=	settings.KeyValue;
			paramsData.BloomAmount			=	settings.BloomAmount;

			paramsCB.SetData( paramsData );
			device.PixelShaderConstants[0]	=	paramsCB;


			//
			//	Measure and adapt :
			//
			device.SetTargets( null, viewLayer.measuredNew );

			device.PixelShaderResources[0]	=	averageLum;
			device.PixelShaderResources[1]	=	viewLayer.measuredOld;

			device.PipelineState		=	factory[ (int)(Flags.MEASURE_ADAPT) ];
				
			device.Draw( 3, 0 );


			//
			//	Tonemap and compose :
			//
			device.SetTargets( null, target );

			device.PixelShaderResources[0]	=	hdrImage;// averageLum;
			device.PixelShaderResources[1]	=	viewLayer.measuredNew;// averageLum;
			device.PixelShaderResources[2]	=	viewLayer.bloom0;// averageLum;
			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;

			Flags op = Flags.LINEAR;
			if (settings.TonemappingOperator==TonemappingOperator.Filmic)   { op = Flags.FILMIC;   }
			if (settings.TonemappingOperator==TonemappingOperator.Linear)   { op = Flags.LINEAR;	 }
			if (settings.TonemappingOperator==TonemappingOperator.Reinhard) { op = Flags.REINHARD; }

			device.PipelineState		=	factory[ (int)(Flags.TONEMAPPING|op) ];
				
			device.Draw( 3, 0 );
			
			device.ResetStates();


			//	swap luminanice buffers :
			Misc.Swap( ref viewLayer.measuredNew, ref viewLayer.measuredOld );
		}



	}
}