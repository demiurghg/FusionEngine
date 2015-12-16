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
using System.Diagnostics;

namespace Fusion.Engine.Graphics {

	public class Sky : GameModule {
		[Flags]
		enum SkyFlags : int
		{
			SKY		= 1 << 0,
			FOG		= 1 << 1,
			SRGB	= 1 << 2,
			CIERGB	= 1 << 3,
		}

		//	row_major float4x4 MatrixWVP;      // Offset:    0 Size:    64 [unused]
		//	float3 SunPosition;                // Offset:   64 Size:    12
		//	float4 SunColor;                   // Offset:   80 Size:    16
		//	float Turbidity;                   // Offset:   96 Size:     4 [unused]
		//	float3 Temperature;                // Offset:  100 Size:    12
		//	float SkyIntensity;                // Offset:  112 Size:     4
		[StructLayout(LayoutKind.Explicit, Size=160)]
		struct SkyConsts {
			[FieldOffset(  0)] public Matrix 	MatrixWVP;
			[FieldOffset( 64)] public Vector3	SunPosition;
			[FieldOffset( 80)] public Color4	SunColor;
			[FieldOffset( 96)] public float		Turbidity;
			[FieldOffset(100)] public Vector3	Temperature; 
			[FieldOffset(112)] public float		SkyIntensity; 
			[FieldOffset(116)] public Vector3	Ambient;
			[FieldOffset(128)] public float		Time;
			[FieldOffset(132)] public Vector3	ViewPos;
		}


		struct SkyVertex {
			[Vertex("POSITION")]
			public Vector4 Vertex;
		}

		GraphicsDevice	rs;
		//Scene			skySphere;
		VertexBuffer	skyVB;
		Ubershader		sky;
		ConstantBuffer	skyConstsCB;
		SkyConsts		skyConstsData;
		StateFactory	factory;

		public Vector3	SkyAmbientLevel { get; protected set; }

		Vector3[]		randVectors;

		public RenderTargetCube	SkyCube { get { return skyCube; } }
		RenderTargetCube	skyCube;
		Texture2D			clouds;

		Random	rand = new Random();


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		public Sky ( GameEngine game ) : base( game )
		{
			rs	=	GameEngine.GraphicsDevice;
		}



		const int NumSegments	=	32;
		const int NumStacks		=	16;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize() 
		{
			skyCube		=	new RenderTargetCube( rs, ColorFormat.Rgba16F, 128, true );
			skyConstsCB	=	new ConstantBuffer( rs, typeof(SkyConsts) );

			LoadContent();

			GameEngine.Reloading += (s,e) => LoadContent();

			
			var skySphere	=	SkySphere.GetVertices(9);
			skyVB			=	new VertexBuffer( GameEngine.GraphicsDevice, typeof(SkyVertex), skySphere.Length );
			skyVB.SetData( skySphere );

			
			randVectors	=	new Vector3[64];

			for (int i=0; i<randVectors.Length; i++) {
				Vector3 randV;
				do {
					randV = rand.NextVector3( -Vector3.One, Vector3.One );
				} while ( randV.Length()>1 && randV.Y < 0 );

				randVectors[i] = randV.Normalized();
			}
		}






		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			//skySphere	=	GameEngine.Content.Load<Scene>("skySphere");
			sky			=	GameEngine.Content.Load<Ubershader>("sky");
			factory		=	new StateFactory( sky, typeof(SkyFlags), (ps,i) => EnumFunc(ps, (SkyFlags)i) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void EnumFunc ( PipelineState ps, SkyFlags flags )
		{
			ps.VertexInputElements	=	VertexInputElement.FromStructure<SkyVertex>();
			ps.RasterizerState		=	RasterizerState.CullCCW;
			ps.BlendState			=	BlendState.Opaque;
			ps.DepthStencilState	=	flags.HasFlag(SkyFlags.FOG) ? DepthStencilState.None : DepthStencilState.Readonly;
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing ) {
				SafeDispose( ref skyVB );
				SafeDispose( ref factory );
				SafeDispose( ref skyCube );
				SafeDispose( ref skyConstsCB );
			}
			base.Dispose( disposing );
		}



		void ApplyColorSpace ( ref SkyFlags flags, SkySettings settings )
		{	
			switch (settings.RgbSpace) {
				case RgbSpace.CIE_RGB	: flags |= SkyFlags.CIERGB;	break;
				case RgbSpace.sRGB		: flags |= SkyFlags.SRGB;	break;
			}
		}



		/// <summary>
		/// Renders fog look-up table
		/// </summary>
		internal void RenderFogTable( SkySettings settings )
		{
			var	sunPos		= settings.SunPosition;
			var sunColor	= settings.SunLightColor;

			var rotation	=	Matrix.Identity;
			var projection	=	MathUtil.ComputeCubemapProjectionMatrixLH( 0.125f, 10.0f );
			var cubeWVPS	=	MathUtil.ComputeCubemapViewMatriciesLH( Vector3.Zero, rotation, projection );

			var flags		=	SkyFlags.FOG;

			ApplyColorSpace( ref flags, settings );
				
			rs.PipelineState	=	factory[(int)flags];
//			rs.DepthStencilState = DepthStencilState.None ;

			skyConstsData.SunPosition	= sunPos;
			skyConstsData.SunColor		= sunColor;
			skyConstsData.Turbidity		= settings.SkyTurbidity;
			skyConstsData.Temperature	= Temperature.Get( settings.SunTemperature ); 
			skyConstsData.SkyIntensity	= settings.SkyIntensity;

			for( int i = 0; i < 6; ++i ) {
				rs.SetTargets( null, SkyCube.GetSurface(0, (CubeFace)i ) );

				SkyCube.SetViewport();

				skyConstsData.MatrixWVP = cubeWVPS[i];
	
				skyConstsCB.SetData( skyConstsData );
				rs.VertexShaderConstants[0] = skyConstsCB;
				rs.PixelShaderConstants[0] = skyConstsCB;


				rs.SetupVertexInput( skyVB, null );
				rs.Draw( skyVB.Capacity, 0 );
				/*for ( int j=0; j<skySphere.Meshes.Count; j++) {
					var mesh = skySphere.Meshes[j];

					rs.SetupVertexInput( mesh.VertexBuffer, mesh.IndexBuffer );
					rs.DrawIndexed( mesh.IndexCount, 0, 0 );
				} */
			}

			rs.ResetStates();

			SkyCube.BuildMipmaps();
		}



		/// <summary>
		/// Renders sky with specified technique
		/// </summary>
		/// <param name="rendCtxt"></param>
		/// <param name="techName"></param>
		internal void Render( Camera camera, StereoEye stereoEye, GameTime gameTime, DepthStencilSurface depthBuffer, RenderTargetSurface hdrTarget, Viewport viewport, SkySettings settings )
		{
			var scale		=	Matrix.Scaling( settings.SkySphereSize );
			var rotation	=	Matrix.Identity;

			var	sunPos		=	settings.SunPosition;
			var sunColor	=	settings.SunGlowColor;

			rs.ResetStates();

			//rs.DepthStencilState = depthBuffer==null? DepthStencilState.None : DepthStencilState.Default ;

			rs.SetTargets( depthBuffer, hdrTarget );

			rs.SetViewport( viewport );

			var viewMatrix = camera.GetViewMatrix( stereoEye );
			var projMatrix = camera.GetProjectionMatrix( stereoEye );

			skyConstsData.MatrixWVP		= scale * rotation * MathUtil.Transformation( viewMatrix.Right, viewMatrix.Up, viewMatrix.Backward ) * projMatrix;
			skyConstsData.SunPosition	= sunPos;
			skyConstsData.SunColor		= sunColor;
			skyConstsData.Turbidity		= settings.SkyTurbidity;
			skyConstsData.Temperature	= Temperature.Get( settings.SunTemperature ); 
			skyConstsData.SkyIntensity	= settings.SkyIntensity;
	
			skyConstsCB.SetData( skyConstsData );
			
			rs.VertexShaderConstants[0] = skyConstsCB;
			rs.PixelShaderConstants[0] = skyConstsCB;


			//
			//	Sky :
			//
			SkyFlags flags = SkyFlags.SKY;

			ApplyColorSpace( ref flags, settings );
				
			rs.PipelineState	=	factory[(int)flags];
						
			rs.SetupVertexInput( skyVB, null );
			rs.Draw( skyVB.Capacity, 0 );

			/*for ( int j=0; j<skySphere.Meshes.Count; j++) {
				var mesh = skySphere.Meshes[j];

				rs.SetupVertexInput( mesh.VertexBuffer, mesh.IndexBuffer );
				rs.DrawIndexed( mesh.IndexCount, 0, 0 );
			}*/

			rs.ResetStates();
		}


#if false
		/// <summary>
		/// Gets current Sun direction.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetSunDirection( SkySettings settings )
		{
			return settings.SunPosition.Normalized();
		}



		Color4 SunColor ( Vector3 dir, SkySettings settings )
		{
			Color4 dusk		=	new Color4(Temperature.Get(2000), 1);
			Color4 zenith	=	new Color4(Temperature.Get(settings.SunTemperature), 1);

			Vector3 ndir	=	dir.Normalized();

			return Color4.Lerp( dusk, zenith, (float)Math.Pow(ndir.Y, 0.5f) );
		}


		/// <summary>
		/// Gets Sun color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetSunLightColor(SkySettings settings)
		{
			var sunPos = GetSunDirection(settings);

			return SunColor( sunPos, settings ) * settings.SunLightIntensity;
		}



		/// <summary>
		/// Gets Sun color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetSunGlowColor(SkySettings settings)
		{
			var sunPos = GetSunDirection(settings);

			return SunColor( sunPos, settings ) * settings.SunGlowIntensity;

			/*var zenithColorYxy = perezZenith( Params.SkyTurbidity, sunPos.Y );
			var sunColorYxy = perezSun( Params.SkyTurbidity, sunPos.Y, 10 );
			
			return new Color4( YxyToRGB( sunColorYxy * new Vector3( Params.SunGlowIntensity, 1, 1 ) ) * Temperature.Get( Params.SunTemperature ), 1 );*/
		}



		/// <summary>
		/// Gets average sky color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetAmbientLevel(SkySettings settings)
		{
			var sunPos = GetSunDirection(settings);
			var ambientLight = Vector3.Zero;

			var norm = randVectors.Length;// * 2 * MathUtil.Pi;

			for (int i = 0; i < randVectors.Length; i++) {
				var yxy = SkyModel.perezSky( settings.SkyTurbidity, randVectors[i], sunPos );
				var rgb = SkyModel.YxyToRGB( yxy );// * Temperature.Get( settings.SunTemperature );
				ambientLight += rgb / norm;
			}

			return new Color4(ambientLight,1);
		}
#endif
	}
}
