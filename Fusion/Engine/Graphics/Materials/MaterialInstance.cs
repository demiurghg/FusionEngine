using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.IniParser;
using System.IO;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.IniParser.Model;
using Fusion.Core.IniParser.Model.Formatting;
using Fusion.Core.Content;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Reprsents material.
	/// MaterialInstance???
	/// </summary>
	public partial class MaterialInstance : DisposableBase {

		const int MaxTextures = 16;

		ShaderResource[]	shaderResources;
		ConstantBuffer		constBufferParams;

		/// <summary>
		/// Gets material's constant buffer :
		/// </summary>
		internal ConstantBuffer ConstantBufferParameters {
			get {
				return constBufferParams;
			}
		}


		internal readonly PipelineState GBufferRigid;
		internal readonly PipelineState GBufferSkinned;
		internal readonly PipelineState ShadowRigid;
		internal readonly PipelineState ShadowSkinned;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="maxTextures"></param>
		internal MaterialInstance ( RenderSystem rs, ContentManager content, MaterialData parameters, IEnumerable<TextureMapBind> textureBinds, SurfaceFlags surfaceFlags )
		{
			if (rs==null) {
				throw new ArgumentNullException("rs");
			}

			if (textureBinds.Count()<0 || textureBinds.Count()>MaxTextures) {
				throw new ArgumentException("textureCount", "Must be less or equal to " + MaxTextures.ToString() );
			}


			//
			//	Pipeline states :
			//
			var factory	=	rs.SceneRenderer.Factory;

			var gbufferRigid	=	SurfaceFlags.GBUFFER | SurfaceFlags.RIGID	| surfaceFlags ;
			var gbufferSkinned	=	SurfaceFlags.GBUFFER | SurfaceFlags.SKINNED | surfaceFlags ;
			var shadowRigid		=	SurfaceFlags.SHADOW  | SurfaceFlags.RIGID	| surfaceFlags ;
			var shadowSkinned	=	SurfaceFlags.SHADOW  | SurfaceFlags.SKINNED | surfaceFlags ;

			GBufferRigid	=	factory[ (int)gbufferRigid ];
			GBufferSkinned	=	factory[ (int)gbufferSkinned ];

			ShadowRigid		=	factory[ (int)shadowRigid ];
			ShadowSkinned	=	factory[ (int)shadowSkinned ];


			//
			//	Textures :
			//
			var textures = textureBinds
				.Select( texBind => texBind.TextureMap.LoadTexture( content, texBind.FallbackPath ) );

			var uvMods = new Vector4[MaxTextures];

			textureBinds
				.Select( tb => tb.TextureMap )
				.Select( tm => new Vector4( tm.ScaleU, tm.ScaleV, tm.OffsetU, tm.OffsetV ) )
				.ToArray()
				.CopyTo( uvMods, 0 );

			shaderResources		=	textures.Select( tex => tex.Srv ).ToArray();

			//
			//	Constants :
			//
			constBufferParams	=	new ConstantBuffer( rs.Device, typeof(MaterialData) );
			constBufferParams.SetData( parameters );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBufferParams );
			}
			base.Dispose( disposing );
		}

		

		/// <summary>
		/// Sets materials textures
		/// </summary>
		/// <param name="device"></param>
		internal void SetTextures ( GraphicsDevice device )
		{
			for (int i=0; i<shaderResources.Length; i++) {
				device.PixelShaderResources[i] = shaderResources[i];
			}
		}
	}
}
																	    