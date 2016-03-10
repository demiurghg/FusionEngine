using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Content;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class SsaoFilter : GameModule {

		[Config]
		public SsaoFilterConfig	Config { get; set; }


		public ShaderResource	OcclusionMap { 
			get {
				return occlusionMap0;
			}
		}


		Ubershader		shader;
		StateFactory	factory;
		ConstantBuffer	paramsCB;
		RenderTarget2D	downsampledDepth;
		RenderTarget2D	downsampledNormals;
		RenderTarget2D	occlusionMap0;
		RenderTarget2D	occlusionMap1;
		Texture2D		randomDir;
        Random rand = new Random();

		#pragma warning disable 649
		struct Params {
			public	Matrix	ProjMatrix;
			public	Matrix	View;
			public	Matrix	ViewProj;
			public	Matrix	InvViewProj;
			public	Matrix	InvProj;
			public	float	TraceStep;
			public	float	DecayRate;
			public float	MaxSampleRadius;
			public float	MaxDepthJump;
	//		public float	dummy0;
	//		public float	dummy1;
		}


		enum Flags {	
			HEMISPHERE	=	0x001,
			BLANK		=	0x001 << 1,
			S_4			=	0x001 << 2,
			S_8			=	0x001 << 3,
			S_16		=	0x001 << 4,
			S_32		=	0x001 << 5,
			S_64		=	0x001 << 6,
			HBAO		=	0x001 << 7,
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public SsaoFilter ( Game game ) : base(game)
		{
			Config	=	new SsaoFilterConfig();
		}


		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			CreateTargets();
			LoadContent();

			randomDir	=	new Texture2D( Game.GraphicsDevice, 64,64, ColorFormat.Rgba8, false );

	//		Color[] randVectors = Enumerable.Range(0, 4096).Select(i => rand.NextColor()).ToArray();

	//		randomDir.SetData( Enumerable.Range(0,4096).Select( i => rand.NextColor() ).ToArray() );
	//		randomDir.SetData(randVectors);
			Color [] randVectors = new Color[4096];
			for ( int i = 0; i < 4096; ++i )
			{
				Color c = rand.NextColor();
				while ((c.R * c.R + c.G * c.G + c.B * c.B) > 256*256)
				{
					c = rand.NextColor();
				}
				randVectors[i] = c;
			}
			randomDir.SetData(randVectors);
			Game.GraphicsDevice.DisplayBoundsChanged += (s,e) => CreateTargets();
			Game.Reloading += (s,e) => LoadContent();
		}


		/// <summary>
		/// 
		/// </summary>
		void CreateTargets ()
		{
			var disp	=	Game.GraphicsDevice.DisplayBounds;

			SafeDispose( ref downsampledDepth );
			SafeDispose( ref downsampledNormals );
			SafeDispose( ref occlusionMap0 );
			SafeDispose( ref occlusionMap1 );

			downsampledDepth	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  disp.Width/1, disp.Height/1, false, false );
			downsampledNormals	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/1, disp.Height/1, false, false );
			occlusionMap0		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/1, disp.Height/1, false, false );
			occlusionMap1		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/1, disp.Height/1, false, false );
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			shader	=	Game.Content.Load<Ubershader>("ssao");
			factory	=	shader.CreateFactory( typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None ); 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref downsampledNormals );
				SafeDispose( ref downsampledDepth );
				SafeDispose( ref occlusionMap0 );
				SafeDispose( ref occlusionMap1 );
				SafeDispose( ref paramsCB	 );
				SafeDispose( ref randomDir );
			}

			base.Dispose( disposing );
		}


		Flags getSampleNumFlag()
		{
			int sn = (int)Config.SampleNumber;
			Flags retFlag = Flags.S_4;
			switch (sn)
			{
				case 4:
					retFlag = Flags.S_4;
					break;
				case 8:
					retFlag = Flags.S_8;
					break;
				case 16:
					retFlag = Flags.S_16;
					break;
				case 32:
					retFlag = Flags.S_32;
					break;
				case 64:
					retFlag = Flags.S_64;
					break;
				default:
					retFlag = Flags.S_4;
					break;
			}
			return retFlag;
		}


		int getFlags()
		{
			int combin = 0;
			switch (Config.AOMethod)
			{
				case SsaoFilterConfig.Method.HEMISPHERE:
					combin = (int)Flags.HEMISPHERE;
					combin |= (int)getSampleNumFlag();
					break;
				case SsaoFilterConfig.Method.HBAO:
					combin = (int)Flags.HBAO;
                    combin |= (int)getSampleNumFlag();
					break;
                default:
                    combin = (int)Flags.HEMISPHERE;
					combin |= (int)getSampleNumFlag();
                    break;

			}
			return combin;
		}


		public void RenderBlank(Matrix view, Matrix projection, ShaderResource depthBuffer, ShaderResource wsNormals)
		{
			var device = Game.GraphicsDevice;

			var filter	=	Game.RenderSystem.Filter;

			filter.StretchRect(downsampledDepth.Surface, depthBuffer);
			filter.StretchRect(downsampledNormals.Surface, wsNormals);


			//
			//	Setup parameters :
			//
			var paramsData = new Params();
			paramsData.ProjMatrix = projection;
			paramsData.View = view;
			paramsData.ViewProj = view * projection;
			paramsData.InvViewProj = Matrix.Invert(view * projection);
			paramsData.InvProj = Matrix.Invert( projection );
			//paramsData.TraceStep = Config.TraceStep;
			//paramsData.DecayRate = Config.DecayRate;
			paramsData.MaxSampleRadius	= Config.MaxSamplingRadius;
			paramsData.MaxDepthJump		= Config.MaxDepthJump;

			paramsCB.SetData(paramsData);
			device.PixelShaderConstants[0] = paramsCB;

			//
			//	Measure and adapt :
			//
			device.SetTargets(null, occlusionMap0);

			device.PixelShaderResources[0] = downsampledDepth;
			device.PixelShaderResources[1] = downsampledNormals;
			device.PixelShaderResources[2] = randomDir;
			device.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			device.PipelineState = factory[(int)Flags.BLANK];


			device.Draw(3, 0);

			device.ResetStates();
		}


		/// <summary>
		/// Performs luminance measurement, tonemapping, applies bloom.
		/// </summary>
		/// <param name="target">LDR target.</param>
		/// <param name="hdrImage">HDR source image.</param>
		public void Render ( StereoEye stereoEye, Camera camera, ShaderResource depthBuffer, ShaderResource wsNormals )
		{
			var view		=	camera.GetViewMatrix( stereoEye );
			var projection	=	camera.GetProjectionMatrix( stereoEye );

			var device	=	Game.GraphicsDevice;
			var filter	=	Game.RenderSystem.Filter;

			using (new PixEvent("SSAO Render")) {
				filter.StretchRect(downsampledDepth.Surface, depthBuffer);
				filter.StretchRect(downsampledNormals.Surface, wsNormals);


				//
				//	Setup parameters :
				//
				using (new PixEvent("SSAO/HBAO Pass")) {
					var paramsData	=	new Params();
					paramsData.ProjMatrix	=	projection;
					paramsData.View			=	view;
					paramsData.ViewProj		=	view * projection;
					paramsData.InvViewProj	=	Matrix.Invert( view * projection );
					paramsData.InvProj		=	Matrix.Invert(projection);
					//paramsData.TraceStep	=	Config.TraceStep;
					//paramsData.DecayRate	=	Config.DecayRate;
					paramsData.MaxSampleRadius	= Config.MaxSamplingRadius;
					paramsData.MaxDepthJump		= Config.MaxDepthJump;

					paramsCB.SetData( paramsData );
					device.PixelShaderConstants[0]	=	paramsCB;

					//
					//	Measure and adapt :
					//
					device.SetTargets( null, occlusionMap0 );

					device.PixelShaderResources[0]	=	downsampledDepth;
					device.PixelShaderResources[1]	=	downsampledNormals;
					device.PixelShaderResources[2]	=	randomDir;
					device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;

					Flags sampleNumFlag = getSampleNumFlag();

					// Hemisphere ssao:
		//			device.PipelineState = factory[(int)Flags.SPHERE | (int)sampleNumFlag];

					// Horizon-based ao:
		//			device.PipelineState = factory[(int)Flags.HBAO];

					device.PipelineState = factory[getFlags()];

				
					device.Draw( 3, 0 );
			
					device.ResetStates();
				}

				using (new PixEvent("Bilateral Filter")) {
					if (Config.BlurSigma!=0) {
						filter.GaussBlurBilateral( occlusionMap0, occlusionMap1, downsampledDepth, downsampledNormals, Config.BlurSigma, Config.Sharpness, 0 );
					}
				}
			}
		}

	}
}
