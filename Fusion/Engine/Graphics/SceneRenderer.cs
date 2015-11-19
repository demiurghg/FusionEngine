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


namespace Fusion.Engine.Graphics {

	public class SceneRenderer : GameModule {

		ConstantBuffer	constBuffer;
		Ubershader		surfaceShader;
		StateFactory	factory;

		Texture2D		defaultDiffuse	;
		Texture2D		defaultSpecular	;
		Texture2D		defaultNormalMap;
		Texture2D		defaultEmission	;

		enum SurfaceFlags {
			GBUFFER = 1,
			SHADOW = 2,
		}

		struct CBSurfaceData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos			;
			public Vector4	BiasSlopeFar	;
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public SceneRenderer ( GameEngine gameEngine ) : base( gameEngine )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			LoadContent();

			constBuffer	=	new ConstantBuffer( GameEngine.GraphicsDevice, typeof(CBSurfaceData) );


			defaultDiffuse	=	new Texture2D( GameEngine.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultDiffuse.SetData( Enumerable.Range(0,16).Select( i => Color.Gray ).ToArray() );

			defaultSpecular	=	new Texture2D( GameEngine.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultSpecular.SetData( Enumerable.Range(0,16).Select( i => new Color(0,128,0,255) ).ToArray() );

			defaultNormalMap	=	new Texture2D( GameEngine.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultNormalMap.SetData( Enumerable.Range(0,16).Select( i => new Color(128,128,255,255) ).ToArray() );

			defaultEmission	=	new Texture2D( GameEngine.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultEmission.SetData( Enumerable.Range(0,16).Select( i => Color.Black ).ToArray() );
			

			GameEngine.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			surfaceShader	=	GameEngine.Content.Load<Ubershader>("surface");
			factory			=	new StateFactory( surfaceShader, typeof(SurfaceFlags), (ps,i) => Enum(ps, (SurfaceFlags)i ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void Enum ( PipelineState ps, SurfaceFlags flags )
		{
			ps.VertexInputElements	=	VertexColorTextureTBNRigid.Elements;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
				SafeDispose( ref factory );

				SafeDispose( ref defaultDiffuse		);
				SafeDispose( ref defaultSpecular	);
				SafeDispose( ref defaultNormalMap	);
				SafeDispose( ref defaultEmission	);
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="depthBuffer"></param>
		/// <param name="hdrTarget"></param>
		/// <param name="diffuse"></param>
		/// <param name="specular"></param>
		/// <param name="normals"></param>
		internal void RenderGBuffer ( Camera camera, StereoEye stereoEye, IEnumerable<Instance> instances, Viewport viewport )
		{		
			if (surfaceShader==null) {	
				return;
			}

			var device		=	GameEngine.GraphicsDevice;

			var view			=	camera.GetViewMatrix( stereoEye );
			var projection		=	camera.GetProjectionMatrix( stereoEye );
			var viewPosition	=	camera.GetCameraPosition4( stereoEye );

			var cbData		=	new CBSurfaceData();

			var hdr			=	GameEngine.GraphicsEngine.LightRenderer.HdrBuffer.Surface;
			var depth		=	GameEngine.GraphicsEngine.LightRenderer.DepthBuffer.Surface;
			var diffuse		=	GameEngine.GraphicsEngine.LightRenderer.DiffuseBuffer.Surface;
			var specular	=	GameEngine.GraphicsEngine.LightRenderer.SpecularBuffer.Surface;
			var normals		=	GameEngine.GraphicsEngine.LightRenderer.NormalMapBuffer.Surface;

			device.ResetStates();

			device.SetTargets( depth, hdr, diffuse, specular, normals );

			device.SetViewport(viewport); 

			device.PipelineState	=	factory[ (int)SurfaceFlags.GBUFFER ];

			device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

			device.PixelShaderResources[0]	=	defaultDiffuse	;
			device.PixelShaderResources[1]	=	defaultSpecular	;
			device.PixelShaderResources[2]	=	defaultNormalMap;
			device.PixelShaderResources[3]	=	defaultEmission	;

			foreach ( var instance in instances ) {
				
				cbData.View			=	view;
				cbData.Projection	=	projection;
				cbData.World		=	instance.World;
				cbData.ViewPos		=	viewPosition;

				constBuffer.SetData( cbData );

				device.PixelShaderConstants[0]	= constBuffer ;
				device.VertexShaderConstants[0]	= constBuffer ;

				device.SetupVertexInput( instance.vb, instance.ib );
				device.DrawIndexed( instance.indexCount, 0, 0 );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		internal void RenderShadowMapCascade ( ShadowContext shadowRenderCtxt, IEnumerable<Instance> instances )
		{
			if (surfaceShader==null) {
				return;
			}

			var device			= GameEngine.GraphicsDevice;

			var cbData			= new CBSurfaceData();

			var viewPosition	= Matrix.Invert( shadowRenderCtxt.ShadowView ).TranslationVector;

			device.SetTargets( shadowRenderCtxt.DepthBuffer, shadowRenderCtxt.ColorBuffer );
			device.SetViewport( shadowRenderCtxt.ShadowViewport );

			device.PipelineState	=	factory[ (int)SurfaceFlags.SHADOW ];

			device.PixelShaderConstants[0]	= constBuffer ;
			device.VertexShaderConstants[0]	= constBuffer ;
			device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;


			cbData.Projection	=	shadowRenderCtxt.ShadowProjection;
			cbData.View			=	shadowRenderCtxt.ShadowView;
			cbData.ViewPos		=	new Vector4( viewPosition, 1 );
			cbData.BiasSlopeFar	=	new Vector4( shadowRenderCtxt.DepthBias, shadowRenderCtxt.SlopeBias, shadowRenderCtxt.FarDistance, 0 );


			foreach ( var instance in instances ) {
				
				cbData.World		=	instance.World;

				constBuffer.SetData( cbData );

				device.SetupVertexInput( instance.vb, instance.ib );
				device.DrawIndexed( instance.indexCount, 0, 0 );
			}
		}
	}
}
