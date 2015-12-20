using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	/// </summary>
	public partial class Material : DisposableBase {

		struct LayerData {
			public Vector4	Tiling;
			public Vector4	Offset;
			public Vector2	RoughnessRange;
			public Vector2	GlowNarrowness;

			public float	ColorLevel;
			public float	AlphaLevel;
			public float	SpecularLevel;
			public float	EmissionLevel;
			public float	BumpLevel;
			public float	Displacement;
			public float	BlendHardness;
			
			public float	Dummy;
		}
		
		ConstantBuffer		constBuffer;
		ShaderResource[]	shaderResources;

		internal ConstantBuffer LayerConstBuffer { 
			get {
				return constBuffer;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		internal void LoadGpuResources ( ContentManager content )
		{
			var defaultColorTexture		=	content.Game.RenderSystem.GrayTexture;
			var defaultSurfaceTexture	=	content.Game.RenderSystem.BlackTexture;
			var defaultNormalMapTexture	=	content.Game.RenderSystem.FlatNormalMap;
			var defaultEmissionTexture	=	content.Game.RenderSystem.BlackTexture;

			constBuffer		=	new ConstantBuffer( content.Game.GraphicsDevice, typeof(LayerData), 4 );

			shaderResources	=	new ShaderResource[16];

			for (int i=0; i<layers.Length; i++) {
				if (layers[i]!=null) {
					shaderResources[ i * 4 + 0]	=	LoadTexture( content, layers[i].ColorTexture	, defaultColorTexture	  , true  ).Srv;
					shaderResources[ i * 4 + 1]	=	LoadTexture( content, layers[i].SurfaceTexture	, defaultSurfaceTexture	  , false ).Srv;
					shaderResources[ i * 4 + 2]	=	LoadTexture( content, layers[i].NormalMapTexture, defaultNormalMapTexture , false ).Srv;
					shaderResources[ i * 4 + 3]	=	LoadTexture( content, layers[i].EmissionTexture , defaultEmissionTexture  , true  ).Srv;
				} else {
					shaderResources[ i * 4 + 0]	=	null;
					shaderResources[ i * 4 + 1]	=	null;
					shaderResources[ i * 4 + 2]	=	null;
					shaderResources[ i * 4 + 3]	=	null;
				}
			}

			var constData = new LayerData[4];

			for (int i=0; i<layers.Length; i++) {
				if (layers[i]!=null) {
					constData[i].Tiling				=	new Vector4(layers[i].Tiling, 0);
					constData[i].Offset				=	new Vector4(layers[i].Offset, 0);
					constData[i].RoughnessRange		=	layers[i].RoughnessRange ;
					constData[i].GlowNarrowness		=	layers[i].GlowNarrowness ;
					constData[i].ColorLevel			=	layers[i].ColorLevel	 ;
					constData[i].AlphaLevel			=	layers[i].AlphaLevel	 ;
					constData[i].SpecularLevel		=	layers[i].SpecularLevel	 ;
					constData[i].EmissionLevel		=	layers[i].EmissionLevel	 ;
					constData[i].Displacement		=	layers[i].Displacement	 ;
					constData[i].BlendHardness		=	layers[i].BlendHardness	 ;
					constData[i].Dummy				=	0;
				}
			}

			constBuffer.SetData( constData );
		}



		/// <summary>
		/// 
		/// </summary>
		void DisposeGpuResources ()
		{
			SafeDispose( ref constBuffer );
			SafeDispose( ref shaderResources );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="LoadColorTexture"></param>
		internal void SetTextures ( GraphicsDevice device )
		{
			for (int i=0; i<16; i++) {
				device.PixelShaderResources[i]	=	shaderResources[i];
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		Texture LoadTexture ( ContentManager content, string path, Texture defaultTexture, bool srgb )
		{
			if (string.IsNullOrWhiteSpace(path)) {
				return defaultTexture;
			}

			if ( !content.Exists( path ) ) {
				return defaultTexture;
			}
			
			return content.Load<DiscTexture>( srgb ? path+"|srgb" : path );
		}
	}
}
																	    