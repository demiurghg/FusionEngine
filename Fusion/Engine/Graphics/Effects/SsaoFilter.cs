using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Core.Content;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics {
	/// <summary>
	/// 
	/// TODO:
	///		1. [DONE] Variable depth-dependent sampling radius.
	///		2. [DONE] Bleeding edge due to upsampling. Solved via half-pixel offset.
	///		3. Sharpenss as parameter (now its hardcoded in shader).
	///		4. Performance measurement.
	///		5. Normals reconstruction from depth (optional?).
	///		6. Sample count/quality configuration.
	///		8. Far-plane flickering.
	/// 
	/// </summary>
	[RequireShader("ssao")]
	internal partial class SsaoFilter : RenderComponent {

		public ShaderResource	OcclusionMap { 
			get {
				return occlusionMapFull;
			}
		}

		Ubershader		shader;
		StateFactory	factory;

		ConstantBuffer	paramsCB;
        ConstantBuffer  sampleDirectionsCB;

		RenderTarget2D	downsampledDepth;
		RenderTarget2D	downsampledNormals;
		RenderTarget2D	occlusionMapHalf;
		RenderTarget2D	occlusionMapFull;
		RenderTarget2D	temporary;

		Texture2D		randomDir;

        Random rand = new Random();
        Vector2 []      sampleDirectionData;

        const int maxNumberOfSamples = 64;
//        const int minNumberOfSamples = 4;

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
		public SsaoFilter ( RenderSystem rs ) : base(rs)
		{
			SetDefaults();
		}


		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );
            sampleDirectionsCB = new ConstantBuffer( Game.GraphicsDevice, typeof(Vector2), maxNumberOfSamples );
            sampleDirectionData = getSampleDirections();

			CreateTargets();
			LoadContent();

			randomDir	=	new Texture2D( Game.GraphicsDevice, 64,64, ColorFormat.Rgba8, false );


			Color [] randVectors = new Color[4096];

			for ( int i = 0; i < 4096; ++i ) {

				#if false
				var dir = rand.UniformRadialDistribution(1,1);
				var color = new Color();
				color.R = (byte)(dir.X * 127 + 128);
				color.G = (byte)(dir.Y * 127 + 128);
				color.B = (byte)(dir.Z * 127 + 128);
				color.A = 128;
				
				randVectors[i] = color;
				#else
				Color c = rand.NextColor();
				while ((c.R * c.R + c.G * c.G + c.B * c.B) > 256*256) {
					c = rand.NextColor();
				}
				randVectors[i] = c;
				#endif
			}

			randomDir.SetData(randVectors);
			Game.RenderSystem.DisplayBoundsChanged += (s,e) => CreateTargets();
			Game.Reloading += (s,e) => LoadContent();
		}


		/// <summary>
		/// 
		/// </summary>
		void CreateTargets ()
		{
			var disp	=	Game.GraphicsDevice.DisplayBounds;

			var newWidth	=	Math.Max(64, disp.Width/2);
			var newHeight	=	Math.Max(64, disp.Height/2);
			var newWidthF	=	Math.Max(64, disp.Width);
			var newHeightF	=	Math.Max(64, disp.Height);

			SafeDispose( ref downsampledDepth );
			SafeDispose( ref downsampledNormals );
			SafeDispose( ref occlusionMapFull );
			SafeDispose( ref occlusionMapHalf );
			SafeDispose( ref temporary );

			downsampledDepth	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  newWidth,	newHeight,	 false, false );
			downsampledNormals	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, newWidth,	newHeight,	 false, false );
			occlusionMapHalf	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, newWidth,	newHeight,	 false, false );
			occlusionMapFull	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, newWidthF, newHeightF, false, false );
			temporary			=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, newWidthF, newHeightF, false, false );
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			shader	=	rs.Shaders.Load("ssao");
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
				SafeDispose( ref downsampledDepth );
				SafeDispose( ref downsampledNormals );
				SafeDispose( ref occlusionMapFull );
				SafeDispose( ref occlusionMapHalf );
				SafeDispose( ref temporary );
				SafeDispose( ref paramsCB	 );
				SafeDispose( ref sampleDirectionsCB );
				SafeDispose( ref randomDir );
			}

			base.Dispose( disposing );
		}


		Flags getSampleNumFlag()
		{
			int sn = (int)SampleNumber;
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
			switch (Method)
			{
				case SsaoFilter.SsaoMethod.HEMISPHERE:
					combin = (int)Flags.HEMISPHERE;
					combin |= (int)getSampleNumFlag();
					break;
				case SsaoFilter.SsaoMethod.HBAO:
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


        Vector2[] getSampleDirections()
        {
            float twoPi = 6.283185307f;
            Vector2[] directions = new Vector2[maxNumberOfSamples];

			for (int i = 0; i < 2; ++i)
			{
				float angle = i * twoPi / 2.0f;
				directions[i].X = (float)Math.Sin(angle);
				directions[i].Y = (float)Math.Cos(angle);
			}

            for (int step = 2; step < maxNumberOfSamples; step *= 2)
            {
                for (int i = 0; i < step; ++i)
                {
                    float angle = twoPi / (float)(step * 2) + i * twoPi / (float)(step);
                    directions[step + i].X = (float)Math.Sin(angle);
                    directions[step + i].Y = (float)Math.Cos(angle);
                }
            }
            return directions;
        }



        public void RenderBlank(Matrix view, Matrix projection, ShaderResource depthBuffer, ShaderResource wsNormals)
		{
			var device = Game.GraphicsDevice;
			var filter = Game.RenderSystem.Filter;

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
			paramsData.MaxSampleRadius	= MaxSamplingRadius;
			paramsData.MaxDepthJump		= MaxDepthJump;

			paramsCB.SetData(paramsData);
            sampleDirectionsCB.SetData(sampleDirectionData);

			device.PixelShaderConstants[0] = paramsCB;
            device.PixelShaderConstants[1] = sampleDirectionsCB;
			//
			//	Measure and adapt :
			//
			device.SetTargets(null, occlusionMapFull);

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

			if (!Enabled) {
				device.Clear( occlusionMapHalf.Surface, Color4.White );
				device.Clear( occlusionMapFull.Surface, Color4.White );
				return;
			}

			using (new PixEvent("SSAO Render")) {
				filter.StretchRect( downsampledDepth.Surface, depthBuffer );
				filter.StretchRect( downsampledNormals.Surface, wsNormals );

				using (new PixEvent("SSAO Pass")) {

					//
					//	Setup parameters :
					//
					var paramsData	=	new Params();
					paramsData.ProjMatrix	=	projection;
					paramsData.View			=	view;
					paramsData.ViewProj		=	view * projection;
					paramsData.InvViewProj	=	Matrix.Invert( view * projection );
					paramsData.InvProj = Matrix.Invert(projection);
					//paramsData.TraceStep	=	Config.TraceStep;
					//paramsData.DecayRate	=	Config.DecayRate;
					paramsData.MaxSampleRadius	= MaxSamplingRadius;
					paramsData.MaxDepthJump		= MaxDepthJump;

					paramsCB.SetData( paramsData );
					sampleDirectionsCB.SetData(sampleDirectionData);

					device.PixelShaderConstants[0]	=	paramsCB;
					device.PixelShaderConstants[1]  =   sampleDirectionsCB;

					//
					//	SSAO :
					//
					device.SetTargets( null, occlusionMapHalf );

					device.PixelShaderResources[0]	=	downsampledDepth;
					device.PixelShaderResources[1]	=	downsampledNormals;
					device.PixelShaderResources[2]	=	randomDir;
					device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;

					Flags sampleNumFlag = getSampleNumFlag();

					device.PipelineState = factory[getFlags()];
			
					device.Draw( 3, 0 );
			
					device.ResetStates();
				}

				using (new PixEvent("Bilateral Filter")) {
					if (BlurSigma!=0) {
						filter.GaussBlurBilateral( occlusionMapHalf, occlusionMapFull, temporary, depthBuffer, wsNormals, BlurSigma, Sharpness, 0 );
					} else {
						filter.StretchRect( occlusionMapFull.Surface, occlusionMapHalf );
					}
				}
			}
		}

	}
}
