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

		public const int BlockSize				=	256;
		public const int MaxInjectingParticles	=	4096;
		public const int MaxSimulatedParticles =	256 * 256;
		public const int MaxImages				=	512;

		bool toMuchInjectedParticles = false;

		int					injectionCount = 0;
		Particle[]			injectionBufferCPU = new Particle[MaxInjectingParticles];
		StructuredBuffer	injectionBuffer;
		StructuredBuffer	simulationBuffer;
		StructuredBuffer	deadParticlesIndices;
		StructuredBuffer	sortParticlesBuffer;
		StructuredBuffer	particleLighting;
		ConstantBuffer		paramsCB;
		ConstantBuffer		imagesCB;


		/// <summary>
		/// Gets structured buffer of simulated particles.
		/// </summary>
		internal StructuredBuffer SimulatedParticles {
			get {
				return simulationBuffer;
			}
		}


		/// <summary>
		/// Gets structured buffer of simulated particles.
		/// </summary>
		internal StructuredBuffer ParticleLighting {
			get {
				return particleLighting;
			}
		}

		enum Flags {
			INJECTION		=	0x01,
			SIMULATION		=	0x02,
			DRAW			=	0x04,
			INITIALIZE		=	0x08,
			DRAW_SHADOW		=	0x10,
		}


//       row_major float4x4 View;       // Offset:    0
//       row_major float4x4 Projection; // Offset:   64
//       float4 CameraForward;          // Offset:  128
//       float4 CameraRight;            // Offset:  144
//       float4 CameraUp;               // Offset:  160
//       float4 CameraPosition;         // Offset:  176
//       float4 Gravity;                // Offset:  192
//       int MaxParticles;              // Offset:  208
//       float DeltaTime;               // Offset:  212
//       uint DeadListSize;             // Offset:  216		
		[StructLayout(LayoutKind.Explicit, Size=256)]
		struct PrtParams {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public Vector4	CameraForward;
			[FieldOffset(144)] public Vector4	CameraRight;
			[FieldOffset(160)] public Vector4	CameraUp;
			[FieldOffset(176)] public Vector4	CameraPosition;
			[FieldOffset(192)] public Vector4	Gravity;
			[FieldOffset(208)] public int		MaxParticles;
			[FieldOffset(212)] public float		DeltaTime;
			[FieldOffset(216)] public uint		DeadListSize;
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
		internal ParticleSystem ( RenderSystem rs, RenderWorld renderWorld )
		{
			this.rs				=	rs;
			this.Game			=	rs.Game;
			this.renderWorld	=	renderWorld;

			Gravity	=	Vector3.Down * 9.80665f;

			paramsCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(PrtParams) );
			imagesCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(Vector4), MaxImages );

			injectionBuffer			=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxInjectingParticles, StructuredBufferFlags.None );
			simulationBuffer		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxSimulatedParticles, StructuredBufferFlags.None );
			particleLighting		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Vector4),		MaxSimulatedParticles, StructuredBufferFlags.None );
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
				SafeDispose( ref particleLighting );
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
			ps.BlendState			=	BlendState.AlphaBlendPremul;
			ps.DepthStencilState	=	DepthStencilState.Readonly;
			ps.Primitive			=	Primitive.PointList;

			if (flag==Flags.DRAW_SHADOW) {

				var bs = new BlendState();
				bs.DstAlpha	=	Blend.One;
				bs.SrcAlpha	=	Blend.One;
				bs.SrcColor	=	Blend.DstColor;
				bs.DstColor	=	Blend.Zero;
				bs.AlphaOp	=	BlendOp.Add;

				ps.BlendState			=	bs;
				ps.DepthStencilState	=	DepthStencilState.Readonly;
			}
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
			injectionCount = 0;
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
		void SetupGPUParameters ( GameTime gameTime, Matrix view, Matrix projection, Flags flags )
		{
			var deltaTime		=	gameTime.ElapsedSec;
			var camera			=	Matrix.Invert( view );

			//	kill particles by applying very large delta.
			if (requestKill) {
				deltaTime	=	float.MaxValue / 2;
				requestKill	=	false;
			}
			if (rs.FreezeParticles) {
				deltaTime = 0;
			}

			//	fill constant data :
			PrtParams param		=	new PrtParams();

			param.View			=	view;
			param.Projection	=	projection;
			param.MaxParticles	=	0;
			param.DeltaTime		=	deltaTime;
			param.CameraForward	=	new Vector4( camera.Forward	, 0 );
			param.CameraRight	=	new Vector4( camera.Right	, 0 );
			param.CameraUp		=	new Vector4( camera.Up		, 0 );
			param.CameraPosition=	new Vector4( camera.TranslationVector	, 1 );
			param.Gravity		=	new Vector4( this.Gravity, 0 );
			param.MaxParticles	=	MaxSimulatedParticles;

			if (flags==Flags.INJECTION) {
				param.MaxParticles	=	injectionCount;
			}

			//	copy to gpu :
			paramsCB.SetData( param );

			//	set DeadListSize to prevent underflow:
			if (flags==Flags.INJECTION) {
				deadParticlesIndices.CopyStructureCount( paramsCB, Marshal.OffsetOf( typeof(PrtParams), "DeadListSize").ToInt32() );
			}
		}




		/// <summary>
		/// Updates particle properties.
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Simulate ( GameTime gameTime, Camera camera )
		{
			var device	=	Game.GraphicsDevice;

			var view		=	camera.GetViewMatrix( StereoEye.Mono );
			var projection	=	camera.GetProjectionMatrix( StereoEye.Mono );

			using ( new PixEvent("Particle Simulation") ) {

				device.ResetStates();

				//
				//	Inject :
				//
				using (new PixEvent("Injection")) {

					injectionBuffer.SetData( injectionBufferCPU );

					device.ComputeShaderResources[1]	= injectionBuffer ;
					device.SetCSRWBuffer( 0, simulationBuffer,		0 );
					device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );

					SetupGPUParameters( gameTime, view, projection, Flags.INJECTION );
					device.ComputeShaderConstants[0]	= paramsCB ;

					device.PipelineState	=	factory[ (int)Flags.INJECTION ];
			
					//	GPU time ???? -> 0.0046
					device.Dispatch( MathUtil.IntDivUp( MaxInjectingParticles, BlockSize ) );

					ClearParticleBuffer();
				}

				//
				//	Simulate :
				//
				using (new PixEvent("Simulation")) {

					if (!renderWorld.IsPaused && !rs.SkipParticlesSimulation) {
	
						device.SetCSRWBuffer( 0, simulationBuffer,		0 );
						device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );
						device.SetCSRWBuffer( 2, sortParticlesBuffer, 0 );

						SetupGPUParameters( gameTime, view, projection, Flags.SIMULATION);
						device.ComputeShaderConstants[0] = paramsCB ;

						device.PipelineState	=	factory[ (int)Flags.SIMULATION ];
	
						/// GPU time : 1.665 ms	 --> 0.38 ms
						device.Dispatch( MathUtil.IntDivUp( MaxSimulatedParticles, BlockSize ) );//*/
					}
				}

				//
				//	Sort :
				//
				using (new PixEvent("Sort")) {
					rs.BitonicSort.Sort( sortParticlesBuffer );
				}


				if (rs.ShowParticles) {
					rs.Counters.DeadParticles	=	deadParticlesIndices.GetStructureCount();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void RenderGeneric ( string passName, GameTime gameTime, Viewport viewport, Matrix view, Matrix projection, RenderTargetSurface colorTarget, DepthStencilSurface depthTarget, Flags flags )
		{
			var device	=	Game.GraphicsDevice;

			if (rs.SkipParticles) {
				return;
			}


			using ( new PixEvent(passName) ) {

				device.ResetStates();

				//
				//	Setup images :
				//
				if (Images!=null && !Images.IsDisposed) {
					imagesCB.SetData( Images.GetNormalizedRectangles( MaxImages ) );
				}

				SetupGPUParameters( gameTime, view, projection, flags );
				device.ComputeShaderConstants[0] = paramsCB ;

				//
				//	Render
				//
				using (new PixEvent("Drawing")) {

					device.ResetStates();
	
					//	target and viewport :
					device.SetTargets( depthTarget, colorTarget );
					device.SetViewport( viewport );

					//	params CB :			
					device.ComputeShaderConstants[0]	= paramsCB ;
					device.VertexShaderConstants[0]		= paramsCB ;
					device.GeometryShaderConstants[0]	= paramsCB ;

					//	atlas CB :
					device.VertexShaderConstants[1]		= imagesCB ;
					device.GeometryShaderConstants[1]	= imagesCB ;
					device.PixelShaderConstants[1]		= imagesCB ;

					//	sampler & textures :
					device.PixelShaderSamplers[0]		=	SamplerState.LinearClamp4Mips ;

					device.PixelShaderResources[0]		=	Images==null? rs.WhiteTexture.Srv : Images.Texture.Srv;
					device.GeometryShaderResources[1]	=	simulationBuffer ;
					device.GeometryShaderResources[2]	=	simulationBuffer ;
					device.GeometryShaderResources[3]	=	sortParticlesBuffer;
					device.GeometryShaderResources[4]	=	particleLighting;

					//	setup PS :
					device.PipelineState	=	factory[ (int)flags ];

					//	GPU time : 0.81 ms	-> 0.91 ms
					device.Draw( MaxSimulatedParticles, 0 );
				}
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Render ( GameTime gameTime, Camera camera, StereoEye stereoEye, HdrFrame viewFrame )
		{
			var view		=	camera.GetViewMatrix( stereoEye );
			var projection	=	camera.GetProjectionMatrix( stereoEye );

			var colorTarget	=	viewFrame.HdrBuffer.Surface;
			var depthTarget	=	viewFrame.DepthBuffer.Surface;

			var viewport	=	new Viewport( 0, 0, colorTarget.Width, colorTarget.Height );

			RenderGeneric( "Particles", gameTime, viewport, view, projection, colorTarget, depthTarget, Flags.DRAW );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void RenderShadow ( GameTime gameTime, Viewport viewport, Matrix view, Matrix projection, RenderTargetSurface particleShadow, DepthStencilSurface depthBuffer )
		{
			var colorTarget	=	particleShadow;
			var depthTarget	=	depthBuffer;

			RenderGeneric( "Particles Shadow", gameTime, viewport, view, projection, colorTarget, depthTarget, Flags.DRAW_SHADOW );
		}
	}
}
