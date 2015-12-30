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

	public class SceneRenderer : GameModule {

		ConstantBuffer	constBuffer;
		Ubershader		surfaceShader;
		StateFactory	factory;

		Texture2D		defaultDiffuse	;
		Texture2D		defaultSpecular	;
		Texture2D		defaultNormalMap;
		Texture2D		defaultEmission	;

		enum SurfaceFlags {
			GBUFFER					=	1 << 0,
			SHADOW					=	1 << 1,

			RIGID					=	1 << 2,
			SKINNED					=	1 << 3,
			
			LAYER0					=	1 << 4,
			LAYER1					=	1 << 5,
			LAYER2					=	1 << 6,
			LAYER3					=	1 << 7,

			TERRAIN					=	1 << 8,

			TRIPLANAR_SINGLE		=	1 << 9,
			TRIPLANAR_DOUBLE		=	1 << 10,
			TRIPLANAR_TRIPLE		=	1 << 11,
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
		/// <param name="Game"></param>
		public SceneRenderer ( Game Game ) : base( Game )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			LoadContent();

			constBuffer	=	new ConstantBuffer( Game.GraphicsDevice, typeof(CBSurfaceData) );


			defaultDiffuse	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultDiffuse.SetData( Enumerable.Range(0,16).Select( i => Color.Gray ).ToArray() );

			defaultSpecular	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultSpecular.SetData( Enumerable.Range(0,16).Select( i => new Color(0,128,0,255) ).ToArray() );

			defaultNormalMap	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultNormalMap.SetData( Enumerable.Range(0,16).Select( i => new Color(128,128,255,255) ).ToArray() );

			defaultEmission	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultEmission.SetData( Enumerable.Range(0,16).Select( i => Color.Black ).ToArray() );
			

			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			surfaceShader	=	Game.Content.Load<Ubershader>("surface");
			factory			=	new StateFactory( surfaceShader, typeof(SurfaceFlags), (ps,i) => Enum(ps, (SurfaceFlags)i ) );
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
		internal void RenderGBuffer ( StereoEye stereoEye, Camera camera, HdrFrame frame, ViewLayerHdr viewLayer )
		{		
			using ( new PixEvent("RenderGBuffer") ) {
				if (surfaceShader==null) {	
					return;
				}

				var device		=	Game.GraphicsDevice;

				var view			=	camera.GetViewMatrix( stereoEye );
				var projection		=	camera.GetProjectionMatrix( stereoEye );
				var viewPosition	=	camera.GetCameraPosition4( stereoEye );

				var cbData		=	new CBSurfaceData();

				var hdr			=	frame.HdrBuffer.Surface;
				var depth		=	frame.DepthBuffer.Surface;
				var diffuse		=	frame.DiffuseBuffer.Surface;
				var specular	=	frame.SpecularBuffer.Surface;
				var normals		=	frame.NormalMapBuffer.Surface;

				device.ResetStates();

				device.SetTargets( depth, hdr, diffuse, specular, normals );
				device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;


				var instances	=	viewLayer.Instances;

				//#warning INSTANSING!
				foreach ( var instance in instances ) {

					cbData.View			=	view;
					cbData.Projection	=	projection;
					cbData.World		=	instance.World;
					cbData.ViewPos		=	viewPosition;

					constBuffer.SetData( cbData );

					device.PixelShaderConstants[0]	= constBuffer ;
					device.VertexShaderConstants[0]	= constBuffer ;

					device.SetupVertexInput( instance.vb, instance.ib );

					try {

						foreach ( var sg in instance.ShadingGroups ) {

							device.PipelineState	=	factory[ (int)ApplyFlags(sg.Material, instance, SurfaceFlags.GBUFFER) ];

							device.PixelShaderConstants[1]	= sg.Material.LayerConstBuffer;
							device.VertexShaderConstants[1]	= sg.Material.LayerConstBuffer;

							sg.Material.SetTextures( device );

							device.DrawIndexed( sg.IndicesCount, sg.StartIndex, 0 );
						}
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
		SurfaceFlags ApplyFlags ( Material material, MeshInstance instance, SurfaceFlags flags )
		{
			if (material!=null) {
				switch ( material.Options ) {
					case MaterialOptions.SingleLayer : flags |= SurfaceFlags.LAYER0; break;	
					case MaterialOptions.DoubleLayer : flags |= SurfaceFlags.LAYER0|SurfaceFlags.LAYER1; break;
					case MaterialOptions.TripleLayer : flags |= SurfaceFlags.LAYER0|SurfaceFlags.LAYER1|SurfaceFlags.LAYER2; break;
					case MaterialOptions.QuadLayer	 : flags |= SurfaceFlags.LAYER0|SurfaceFlags.LAYER1|SurfaceFlags.LAYER2|SurfaceFlags.LAYER3; break;

					case MaterialOptions.Terrain : flags |= SurfaceFlags.TERRAIN; break;

					case MaterialOptions.TriplanarWorldSingle : flags |= SurfaceFlags.TRIPLANAR_SINGLE; break;
					case MaterialOptions.TriplanarWorldDouble : flags |= SurfaceFlags.TRIPLANAR_DOUBLE; break;
					case MaterialOptions.TriplanarWorldTriple : flags |= SurfaceFlags.TRIPLANAR_TRIPLE; break;
				}
			}

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
		internal void RenderShadowMapCascade ( ShadowContext shadowRenderCtxt, IEnumerable<MeshInstance> instances )
		{
			using ( new PixEvent("ShadowMap") ) {
				if (surfaceShader==null) {
					return;
				}

				var device			= Game.GraphicsDevice;

				var cbData			= new CBSurfaceData();

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

					device.PipelineState	=	factory[ (int)ApplyFlags( null, instance, SurfaceFlags.SHADOW ) ];
					cbData.World			=	instance.World;

					constBuffer.SetData( cbData );

					device.SetupVertexInput( instance.vb, instance.ib );
					device.DrawIndexed( instance.indexCount, 0, 0 );
				}
			}
		}
	}
}
