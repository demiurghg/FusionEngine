using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;



namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents particle rendering and simulation system.
	/// 1. http://www.gdcvault.com/play/1014347/HALO-REACH-Effects
	/// 2. Gareth Thomas Compute-based GPU Particle
	/// </summary>
	public class ParticleSystem : DisposableBase {

		readonly Game Game;
		readonly RenderSystem rs;
		Ubershader		shader;
		StateFactory	factory;
		RenderWorld	renderWorld;

		const int BlockSize				=	256;
		const int MaxInjectingParticles	=	1024;
		const int MaxSimulatedParticles =	256 * 256;
		const int MaxImages				=	512;

		bool toMuchInjectedParticles = false;

		int					injectionCount = 0;
		Particle[]			injectionBufferCPU = new Particle[MaxInjectingParticles];
		StructuredBuffer	injectionBuffer;
		StructuredBuffer	simulationBuffer;
		StructuredBuffer	deadParticlesIndices;
		StructuredBuffer	sortParticlesBuffer;
		ConstantBuffer		paramsCB;
		ConstantBuffer		imagesCB;

		enum Flags {
			INJECTION		=	0x01,
			SIMULATION		=	0x02,
			DRAW			=	0x04,
			INITIALIZE		=	0x08,
		}


		//	row_major float4x4 View;       // Offset:    0
		//	row_major float4x4 Projection; // Offset:   64
		//	int MaxParticles;              // Offset:  128
		//	float DeltaTime;               // Offset:  132
		[StructLayout(LayoutKind.Explicit, Size=256)]
		struct PrtParams {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public Vector4	CameraForward;
			[FieldOffset(144)] public Vector4	CameraRight;
			[FieldOffset(160)] public Vector4	CameraUp;
			[FieldOffset(176)] public Vector4	Gravity;
			[FieldOffset(192)] public int		MaxParticles;
			[FieldOffset(196)] public float		DeltaTime;
			[FieldOffset(200)] public uint		DeadListSize;
		} 

		Random rand = new Random();


		/// <summary>
		/// Gets and sets overall particle gravity.
		/// Default -9.8.
		/// </summary>
		public Vector3	Gravity { get; set; }
		


		/// <summary>
		/// Sets and gets images for particles.
		/// This property must be set before particle injection.
		/// To prevent interference between textures in atlas all images must be padded with 16 pixels.
		/// </summary>
		public TextureAtlas Images { 
			get {
				return images;
			}
			set {
				if (value!=null && value.Count>MaxImages) {
					throw new ArgumentOutOfRangeException("Number of subimages in texture atlas is greater than " + MaxImages.ToString() );
				}
				images = value;
			}
		}

		TextureAtlas images = null;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		internal ParticleSystem ( RenderSystem rs, RenderWorld viewLayer )
		{
			this.rs				=	rs;
			this.Game			=	rs.Game;
			this.renderWorld	=	viewLayer;

			Gravity	=	Vector3.Down * 9.80665f;

			paramsCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(PrtParams) );
			imagesCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(Vector4), MaxImages );

			injectionBuffer			=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxInjectingParticles, StructuredBufferFlags.None );
			simulationBuffer		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxSimulatedParticles, StructuredBufferFlags.None );
			sortParticlesBuffer		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Vector2),		MaxSimulatedParticles, StructuredBufferFlags.None );
			deadParticlesIndices	=	new StructuredBuffer( Game.GraphicsDevice, typeof(uint),		MaxSimulatedParticles, StructuredBufferFlags.Append );

			rs.Game.Reloading += LoadContent;
			LoadContent(this, EventArgs.Empty);

			//	initialize dead list :
			var device = Game.GraphicsDevice;

			device.SetCSRWBuffer( 1, deadParticlesIndices, 0 );
			device.PipelineState	=	factory[ (int)Flags.INITIALIZE ];
			device.Dispatch( MathUtil.IntDivUp( MaxSimulatedParticles, BlockSize ) );
		}



		/// <summary>
		/// Loads content
		/// </summary>
		void LoadContent ( object sender, EventArgs args )
		{
			shader	=	rs.Game.Content.Load<Ubershader>("particles.hlsl");
			factory	=	shader.CreateFactory( typeof(Flags), (ps,i) => EnumAction( ps, (Flags)i ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {	

				rs.Game.Reloading -= LoadContent;

				SafeDispose( ref paramsCB );
				SafeDispose( ref imagesCB );

				SafeDispose( ref injectionBuffer );
				SafeDispose( ref simulationBuffer );
				SafeDispose( ref sortParticlesBuffer );
				SafeDispose( ref deadParticlesIndices );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flag"></param>
		void EnumAction ( PipelineState ps, Flags flag )
		{
			ps.BlendState			=	BlendState.AlphaBlend;
			ps.DepthStencilState	=	DepthStencilState.Readonly;
			ps.Primitive			=	Primitive.PointList;
		}



		/// <summary>
		/// 
		/// </summary>
		public TextureAtlas TextureAtlas {
			get; set;
		}


		/// <summary>
		/// Injects hard particle.
		/// </summary>
		/// <param name="particle"></param>
		public void InjectParticle ( Particle particle )
		{
			if (renderWorld.IsPaused) {
				return;
			}

			if (Images==null) {
				throw new InvalidOperationException("Images must be set");
			}

			if (injectionCount>=MaxInjectingParticles) {
				toMuchInjectedParticles = true;
				return;
			}

			if (particle.LifeTime<=0) {
				return;
			}

			toMuchInjectedParticles = false;

			injectionBufferCPU[ injectionCount ] = particle;
			injectionCount ++;
		}



		/// <summary>
		/// Makes all particles wittingly dead
		/// </summary>
		void ClearParticleBuffer ()
		{
			//for (int i=0; i<MaxInjectingParticles; i++) {
			//	injectionBufferCPU[i].Timing.X = -999999;
			//}
			injectionCount = 0;
		}



		/// <summary>
		/// Updates particle properties.
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Simulate ( GameTime gameTime )
		{
		}



		/// <summary>
		/// Immediatly kills all living particles.
		/// </summary>
		/// <returns></returns>
		public void KillParticles ()
		{
			requestKill = true;
			ClearParticleBuffer();
		}

		bool requestKill = false;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Render ( GameTime gameTime, Camera camera, StereoEye stereoEye, HdrFrame viewFrame )
		{
			var device	=	Game.GraphicsDevice;

			if (rs.Config.SkipParticles) {
				return;
			}

			using ( new PixEvent("Particle Rendering") ) {

				device.ResetStates();

				int	w	=	device.DisplayBounds.Width;
				int h	=	device.DisplayBounds.Height;

				//
				//	Setup images :
				//
				if (Images!=null && !Images.IsDisposed) {
					imagesCB.SetData( Images.GetNormalizedRectangles( MaxImages ) );
				}
			

				var deltaTime	=	gameTime.ElapsedSec;

				//	kill particles by applying very large delta.
				if (requestKill) {
					deltaTime	=	float.MaxValue / 2;
					requestKill	=	false;
				}
				if (rs.Config.FreezeParticles) {
					deltaTime = 0;
				}

				//
				//	Setup parameters :
				//	
				PrtParams param = new PrtParams();
				param.View			=	renderWorld.Camera.GetViewMatrix( stereoEye );
				param.Projection	=	renderWorld.Camera.GetProjectionMatrix( stereoEye );
				param.MaxParticles	=	0;
				param.DeltaTime		=	deltaTime;
				param.CameraForward	=	new Vector4( renderWorld.Camera.GetCameraMatrix( StereoEye.Mono ).Forward	, 0 );
				param.CameraRight	=	new Vector4( renderWorld.Camera.GetCameraMatrix( StereoEye.Mono ).Right	, 0 );
				param.CameraUp		=	new Vector4( renderWorld.Camera.GetCameraMatrix( StereoEye.Mono ).Up		, 0 );
				param.Gravity		=	new Vector4( this.Gravity, 0 );
				param.MaxParticles	=	injectionCount;

				paramsCB.SetData( param );

				//	set DeadListSize to prevent underflow:
				deadParticlesIndices.CopyStructureCount( paramsCB, Marshal.OffsetOf( typeof(PrtParams), "DeadListSize").ToInt32() );

				device.ComputeShaderConstants[0]	= paramsCB ;


				//
				//	Inject :
				//
				using (new PixEvent("Injection")) {
					injectionBuffer.SetData( injectionBufferCPU );

					device.ComputeShaderResources[1]	= injectionBuffer ;
					device.SetCSRWBuffer( 0, simulationBuffer,		0 );
					device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );

					device.PipelineState	=	factory[ (int)Flags.INJECTION ];
			
					//	GPU time ???? -> 0.0046
					device.Dispatch( MathUtil.IntDivUp( MaxInjectingParticles, BlockSize ) );

					ClearParticleBuffer();
				}

				//
				//	Simulate :
				//
				using (new PixEvent("Simulation")) {

					if (!renderWorld.IsPaused && !rs.Config.SkipParticlesSimulation) {
	
						device.SetCSRWBuffer( 0, simulationBuffer,		0 );
						device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );
						device.SetCSRWBuffer( 2, sortParticlesBuffer, 0 );

						param.MaxParticles	=	MaxSimulatedParticles;
						paramsCB.SetData( param );
						device.ComputeShaderConstants[0] = paramsCB ;

						device.PipelineState	=	factory[ (int)Flags.SIMULATION ];
	
						/// GPU time : 1.665 ms	 --> 0.38 ms
						device.Dispatch( MathUtil.IntDivUp( MaxSimulatedParticles, BlockSize ) );//*/
					}
				}


				using (new PixEvent("Sort")) {
					rs.BitonicSort.Sort( sortParticlesBuffer );
				}



				//
				//	Render
				//
				using (new PixEvent("Drawing")) {

					device.ResetStates();
	
					//	target and viewport :
					device.SetTargets( viewFrame.DepthBuffer, viewFrame.HdrBuffer );
					device.SetViewport( 0,0,viewFrame.HdrBuffer.Width, viewFrame.HdrBuffer.Height );

					//	params CB :			
					device.ComputeShaderConstants[0]	= paramsCB ;
					device.VertexShaderConstants[0]		= paramsCB ;
					device.GeometryShaderConstants[0]	= paramsCB ;

					//	atlas CB :
					device.VertexShaderConstants[1]		= imagesCB ;
					device.GeometryShaderConstants[1]	= imagesCB ;
					device.PixelShaderConstants[1]		= imagesCB ;

					//	sampler & textures :
					device.PixelShaderSamplers[0]	= SamplerState.LinearClamp4Mips ;

					device.PixelShaderResources[0]		=	Images==null? null : Images.Texture.Srv;
					device.GeometryShaderResources[1]	=	simulationBuffer ;
					device.GeometryShaderResources[2]	=	simulationBuffer ;
					device.GeometryShaderResources[3]	=	sortParticlesBuffer;

					//	setup PS :
					device.PipelineState	=	factory[ (int)Flags.DRAW ];

					//	GPU time : 0.81 ms	-> 0.91 ms
					device.Draw( MaxSimulatedParticles, 0 );


					if (rs.Config.ShowParticles) {
						rs.Counters.DeadParticles	=	deadParticlesIndices.GetStructureCount();
					}
				}
			}
		}
	}
}
