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
		Texture2D		texture;
		Ubershader		shader;
		StateFactory	factory;
		ViewLayerHdr	viewLayer;

		const int BlockSize				=	256;
		const int MaxInjectingParticles	=	1024;
		const int MaxSimulatedParticles =	384 * 1024;

		bool toMuchInjectedParticles = false;

		int					injectionCount = 0;
		Particle[]			injectionBufferCPU = new Particle[MaxInjectingParticles];
		StructuredBuffer	injectionBuffer;
		StructuredBuffer	simulationBuffer;
		StructuredBuffer	deadParticlesIndices;
		ConstantBuffer		paramsCB;

//       float2 Position;               // Offset:    0
//       float2 Velocity;               // Offset:    8
//       float2 Acceleration;           // Offset:   16
//       float4 Color0;                 // Offset:   24
//       float4 Color1;                 // Offset:   40
//       float Size0;                   // Offset:   56
//       float Size1;                   // Offset:   60
//       float Angle0;                  // Offset:   64
//       float Angle1;                  // Offset:   68
//       float TotalLifeTime;           // Offset:   72
//       float LifeTime;                // Offset:   76
//       float FadeIn;                  // Offset:   80
//       float FadeOut;                 // Offset:   84
		[StructLayout(LayoutKind.Explicit)]
		struct Particle {
			[FieldOffset( 0)] public Vector2	Position;
			[FieldOffset( 8)] public Vector2	Velocity;
			[FieldOffset(16)] public Vector2	Acceleration;
			[FieldOffset(24)] public Vector4	Color0;
			[FieldOffset(40)] public Vector4	Color1;
			[FieldOffset(56)] public float	Size0;
			[FieldOffset(60)] public float	Size1;
			[FieldOffset(64)] public float	Angle0;
			[FieldOffset(68)] public float	Angle1;
			[FieldOffset(72)] public float	TotalLifeTime;
			[FieldOffset(76)] public float	LifeTime;
			[FieldOffset(80)] public float	FadeIn;
			[FieldOffset(84)] public float	FadeOut;

			public override string ToString ()
			{
				return string.Format("life time = {0}/{1}", LifeTime, TotalLifeTime );
			}
		}

		enum Flags {
			INJECTION	=	0x1,
			SIMULATION	=	0x2,
			DRAW		=	0x4,
			INITIALIZE	=	0x8,
		}


//       row_major float4x4 View;       // Offset:    0
//       row_major float4x4 Projection; // Offset:   64
//       int MaxParticles;              // Offset:  128
//       float DeltaTime;               // Offset:  132

		[StructLayout(LayoutKind.Explicit, Size=144)]
		struct Params {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public int		MaxParticles;
			[FieldOffset(132)] public float		DeltaTime;
			[FieldOffset(136)] public uint		DeadListSize;

		} 

		Random rand = new Random();


		/// <summary>
		/// Gets and sets overall particle gravity.
		/// Default -9.8.
		/// </summary>
		public Vector3	Gravity { get; set; }
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		internal ParticleSystem ( RenderSystem rs, ViewLayerHdr viewLayer )
		{
			this.rs			=	rs;
			this.Game		=	rs.Game;
			this.viewLayer	=	viewLayer;

			Gravity	=	Vector3.Down * 9.80665f;

			paramsCB		=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			injectionBuffer			=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxInjectingParticles, StructuredBufferFlags.None );
			simulationBuffer		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle),	MaxSimulatedParticles, StructuredBufferFlags.None );
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

				paramsCB.Dispose();

				injectionBuffer.Dispose();
				simulationBuffer.Dispose();
				deadParticlesIndices.Dispose();
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
			ps.BlendState			=	BlendState.Additive;
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
		/// Returns random radial vector
		/// </summary>
		/// <returns></returns>
		Vector2 RadialRandomVector ()
		{
			Vector2 r;
			do {
				r	=	rand.NextVector2( -Vector2.One, Vector2.One );
			} while ( r.Length() > 1 );

			//r.Normalize();

			return r;
		}



		/// <summary>
		/// Injects hard particle.
		/// </summary>
		/// <param name="particle"></param>
		public void InjectParticle ( Vector2 pos, Vector2 vel, float lifeTime, float size0, float size1, float colorBoost = 1 )
		{
			if (injectionCount>=MaxInjectingParticles) {
				toMuchInjectedParticles = true;
				return;
			}

			toMuchInjectedParticles = false;

			//Log.LogMessage("...particle added");
			var v = vel + RadialRandomVector() * 5;
			var a = rand.NextFloat( -MathUtil.Pi, MathUtil.Pi );
			var s = (rand.NextFloat(0,1)>0.5f) ? -1 : 1;

			var p = new Particle () {
				Position		=	pos,
				Velocity		=	vel + RadialRandomVector() * 5,
				Acceleration	=	Vector2.UnitY * 10 - v * 0.2f,
				Color0			=	Vector4.Zero,
				//Color1			=	rand.NextVector4( Vector4.Zero, Vector4.One ) * colorBoost,
				Color1			=	Vector4.One * colorBoost,//rand.NextVector4( Vector4.Zero, Vector4.One ),
				Size0			=	size0,
				Size1			=	size1,
				Angle0			=	a,
				Angle1			=	a + 2 * s,
				TotalLifeTime	=	rand.NextFloat(lifeTime/2, lifeTime),
				LifeTime		=	0,
				FadeIn			=	0.01f,
				FadeOut			=	0.01f,
			};

			injectionBufferCPU[ injectionCount ] = p;
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
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void Render ( GameTime gameTime, Camera camera, StereoEye stereoEye, HdrFrame viewFrame )
		{
			var device	=	Game.GraphicsDevice;

			device.ResetStates();

			device.SetTargets( viewFrame.DepthBuffer, viewFrame.HdrBuffer );
			device.SetViewport( 0,0,viewFrame.HdrBuffer.Width, viewFrame.HdrBuffer.Height );

			int	w	=	device.DisplayBounds.Width;
			int h	=	device.DisplayBounds.Height;

			Params param = new Params();
			param.View			=	viewLayer.Camera.GetViewMatrix( stereoEye );
			param.Projection	=	viewLayer.Camera.GetProjectionMatrix( stereoEye );
			param.MaxParticles	=	0;
			param.DeltaTime		=	gameTime.ElapsedSec;


			device.ComputeShaderConstants[0]	= paramsCB ;
			device.VertexShaderConstants[0]		= paramsCB ;
			device.GeometryShaderConstants[0]	= paramsCB ;
			device.PixelShaderConstants[0]		= paramsCB ;
			
			device.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;


			//
			//	Inject :
			//
			injectionBuffer.SetData( injectionBufferCPU );

			device.ComputeShaderResources[1]	= injectionBuffer ;

			device.SetCSRWBuffer( 0, simulationBuffer,		0 );
			device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );


			param.MaxParticles	=	injectionCount;
			//param.DeadListSize	=	(uint)deadParticlesIndices.GetStructureCount();

			paramsCB.SetData( param );
			deadParticlesIndices.CopyStructureCount( paramsCB, 136 );
			//device.CSConstantBuffers[0] = paramsCB ;

			device.PipelineState	=	factory[ (int)Flags.INJECTION ];
			
			//	GPU time ???? -> 0.0046
			device.Dispatch( MathUtil.IntDivUp( MaxInjectingParticles, BlockSize ) );

			ClearParticleBuffer();

			//
			//	Simulate :
			//
			bool skipSim = Game.InputDevice.IsKeyDown(Fusion.Drivers.Input.Keys.P);

			if (!skipSim) {
	
				device.SetCSRWBuffer( 0, simulationBuffer,		0 );
				device.SetCSRWBuffer( 1, deadParticlesIndices, -1 );

				param.MaxParticles	=	MaxSimulatedParticles;
				paramsCB.SetData( param );
				device.ComputeShaderConstants[0] = paramsCB ;

				device.PipelineState	=	factory[ (int)Flags.SIMULATION ];
	
				/// GPU time : 1.665 ms	 --> 0.38 ms
				device.Dispatch( MathUtil.IntDivUp( MaxSimulatedParticles, BlockSize ) );//*/
			}


			//
			//	Render
			//
			device.PipelineState	=	factory[ (int)Flags.DRAW ];
			device.SetCSRWBuffer( 0, null );	
			device.PixelShaderResources[0]	=	texture ;
			device.GeometryShaderResources[1]	=	simulationBuffer ;
			device.GeometryShaderResources[2]	=	simulationBuffer ;

			//	GPU time : 0.81 ms	-> 0.91 ms
			device.Draw( MaxSimulatedParticles, 0 );
		}
	}
}
