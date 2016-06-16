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
using Fusion.Core.Development;


namespace Fusion.Engine.Graphics {

	public class SceneRenderer : GameComponent {

		readonly RenderSystem	rs;
		internal const int MaxBones = 128;

		ConstantBuffer	constBuffer;
		ConstantBuffer	constBufferBones;
		Ubershader		surfaceShader;
		StateFactory	factory;

		Texture2D		defaultDiffuse	;
		Texture2D		defaultSpecular	;
		Texture2D		defaultNormalMap;
		Texture2D		defaultEmission	;

		RenderTarget2D	voxelBuffer;


		struct CBMeshInstanceData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos			;
			public Vector4	BiasSlopeFar	;
			public Color4	Color;
		}


		/// <summary>
		/// Gets pipeline state factory
		/// </summary>
		internal StateFactory Factory {
			get {
				return factory;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		public SceneRenderer ( Game Game, RenderSystem rs ) : base( Game )
		{
			this.rs	=	rs;
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			LoadContent();

			constBuffer			=	new ConstantBuffer( Game.GraphicsDevice, typeof(CBMeshInstanceData) );
			constBufferBones	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Matrix), MaxBones );


			defaultDiffuse	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultDiffuse.SetData( Enumerable.Range(0,16).Select( i => Color.Gray ).ToArray() );

			defaultSpecular	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultSpecular.SetData( Enumerable.Range(0,16).Select( i => new Color(0,128,0,255) ).ToArray() );

			defaultNormalMap	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultNormalMap.SetData( Enumerable.Range(0,16).Select( i => new Color(128,128,255,255) ).ToArray() );

			defaultEmission	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultEmission.SetData( Enumerable.Range(0,16).Select( i => Color.Black ).ToArray() );

			//Ubershader.AddEnumerator( "SceneRenderer", (t

			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			surfaceShader	=	Game.Content.Load<Ubershader>("surface");
			factory			=	surfaceShader.CreateFactory( typeof(SurfaceFlags), (ps,i) => Enum(ps, (SurfaceFlags)i ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void Enum ( PipelineState ps, SurfaceFlags flags )
		{
			ps.RasterizerState	=	RasterizerState.CullCW;

			if (flags.HasFlag( SurfaceFlags.SKINNED )) {
				ps.VertexInputElements	=	VertexColorTextureTBNSkinned.Elements;
			}
			
			if (flags.HasFlag( SurfaceFlags.RIGID )) {
				ps.VertexInputElements	=	VertexColorTextureTBNRigid.Elements;
			}
		}



		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
				SafeDispose( ref constBufferBones );

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
		internal void RenderGBuffer ( StereoEye stereoEye, Camera camera, HdrFrame frame, RenderWorld rw )
		{		
			using ( new PixEvent("RenderGBuffer") ) {
				if (surfaceShader==null) {	
					return;
				}

				if (rs.SkipSceneRendering) {
					return;
				}

				var device		=	Game.GraphicsDevice;

				var view			=	camera.GetViewMatrix( stereoEye );
				var projection		=	camera.GetProjectionMatrix( stereoEye );
				var viewPosition	=	camera.GetCameraPosition4( stereoEye );

				var cbData		=	new CBMeshInstanceData();

				var hdr			=	frame.HdrBuffer.Surface;
				var depth		=	frame.DepthBuffer.Surface;
				var diffuse		=	frame.DiffuseBuffer.Surface;
				var specular	=	frame.SpecularBuffer.Surface;
				var normals		=	frame.NormalMapBuffer.Surface;
				var scattering	=	frame.ScatteringBuffer.Surface;

				device.ResetStates();

				device.SetTargets( depth, hdr, diffuse, specular, normals, scattering );
				device.PixelShaderSamplers[0]	= SamplerState.LinearClamp ;

				var instances	=	rw.Instances;

				if (instances.Any()) {
					device.PixelShaderResources[0]	= rs.VirtualTexture.FallbackTexture.Srv;
				}

				//#warning INSTANSING!
				foreach ( var instance in instances ) {

					if (!instance.Visible) {
						continue;
					}

					cbData.View			=	view;
					cbData.Projection	=	projection;
					cbData.World		=	instance.World;
					cbData.ViewPos		=	viewPosition;

                    cbData.Color = instance.Color;

					constBuffer.SetData( cbData );

					device.PixelShaderConstants[0]	= constBuffer ;
					device.VertexShaderConstants[0]	= constBuffer ;

					device.SetupVertexInput( instance.vb, instance.ib );

					if (instance.IsSkinned) {
						constBufferBones.SetData( instance.BoneTransforms );
						device.VertexShaderConstants[3]	= constBufferBones ;
					}

					try {

						device.PipelineState	=	factory[ (int)( SurfaceFlags.GBUFFER | SurfaceFlags.RIGID ) ];

						device.DrawIndexed( instance.indexCount, 0, 0 );

						rs.Counters.SceneDIPs++;
						
					} catch ( UbershaderException e ) {
						Log.Warning( e.Message );					
						ExceptionDialog.Show( e );
					}
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="material"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		SurfaceFlags ApplyFlags ( MeshInstance instance, SurfaceFlags flags )
		{
			if (instance.IsSkinned) {
				flags |= SurfaceFlags.SKINNED;
			} else {
				flags |= SurfaceFlags.RIGID;
			}

			return flags;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		internal void RenderFeedbackBuffer ( StereoEye stereoEye, Camera camera, HdrFrame frame, IEnumerable<MeshInstance> instances )
		{
			if (rs.SkipFeedback) {
				return;
			}

			using ( new PixEvent("FeedbackBuffer") ) {
				if (surfaceShader==null) {
					return;
				}

				var device			=	Game.GraphicsDevice;

				var cbData			=	new CBMeshInstanceData();

				var view			=	camera.GetViewMatrix( stereoEye );
				var projection		=	camera.GetProjectionMatrix( stereoEye );
				var viewPosition	=	camera.GetCameraPosition4( stereoEye );

				device.SetTargets( frame.FeedbackBufferDepth.Surface, frame.FeedbackBufferColor.Surface );
				device.SetViewport( 0,0, frame.FeedbackBufferColor.Width, frame.FeedbackBufferColor.Height );

				device.PixelShaderConstants[0]	= constBuffer ;
				device.VertexShaderConstants[0]	= constBuffer ;
				device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

				cbData.Projection	=	projection;
				cbData.View			=	view;
				cbData.ViewPos		=	viewPosition;
				cbData.BiasSlopeFar	=	Vector4.Zero;

				//#warning INSTANSING!
				foreach ( var instance in instances ) {

					if (!instance.Visible) {
						continue;
					}

					device.PipelineState	=	factory[ (int)ApplyFlags( instance, SurfaceFlags.FEEDBACK ) ];
					cbData.World			=	instance.World;
					cbData.Color			=	instance.Color;

					constBuffer.SetData( cbData );

					if (instance.IsSkinned) {
						constBufferBones.SetData( instance.BoneTransforms );
						device.VertexShaderConstants[3]	= constBufferBones ;
					}

					device.SetupVertexInput( instance.vb, instance.ib );
					device.DrawIndexed( instance.indexCount, 0, 0 );
				}
			}

			var feedbackBuffer = new FeedbackData[ HdrFrame.FeedbackBufferWidth * HdrFrame.FeedbackBufferHeight ];
			frame.FeedbackBufferColor.GetData( feedbackBuffer );

			feedbackBuffer.Count();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		internal void RenderShadowMapCascade ( ShadowContext shadowRenderCtxt, IEnumerable<MeshInstance> instances )
		{
			using ( new PixEvent("ShadowMap") ) {
				if (surfaceShader==null) {
					return;
				}

				var device			= Game.GraphicsDevice;

				var cbData			= new CBMeshInstanceData();

				var viewPosition	= Matrix.Invert( shadowRenderCtxt.ShadowView ).TranslationVector;

				device.SetTargets( shadowRenderCtxt.DepthBuffer, shadowRenderCtxt.ColorBuffer );
				device.SetViewport( shadowRenderCtxt.ShadowViewport );

				device.PixelShaderConstants[0]	= constBuffer ;
				device.VertexShaderConstants[0]	= constBuffer ;
				device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

				cbData.Projection	=	shadowRenderCtxt.ShadowProjection;
				cbData.View			=	shadowRenderCtxt.ShadowView;
				cbData.ViewPos		=	new Vector4( viewPosition, 1 );
				cbData.BiasSlopeFar	=	new Vector4( shadowRenderCtxt.DepthBias, shadowRenderCtxt.SlopeBias, shadowRenderCtxt.FarDistance, 0 );

				//#warning INSTANSING!
				foreach ( var instance in instances ) {

					if (!instance.Visible) {
						continue;
					}

					device.PipelineState	=	factory[ (int)ApplyFlags( instance, SurfaceFlags.SHADOW ) ];
					cbData.World			=	instance.World;
					cbData.Color			=	instance.Color;

					constBuffer.SetData( cbData );

					if (instance.IsSkinned) {
						constBufferBones.SetData( instance.BoneTransforms );
						device.VertexShaderConstants[3]	= constBufferBones ;
					}

					device.SetupVertexInput( instance.vb, instance.ib );
					device.DrawIndexed( instance.indexCount, 0, 0 );

					rs.Counters.ShadowDIPs++;
				}
			}
		}
	}
}
