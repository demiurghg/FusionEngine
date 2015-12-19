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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		internal void LoadGpuResources ( ContentManager content )
		{
			constBuffer		=	new ConstantBuffer( content.GameEngine.GraphicsDevice, typeof(LayerData), 4 );

			shaderResources	=	new ShaderResource[16];

			for (int i=0; i<layers.Length; i++) {
				if (layers[i]!=null) {
					shaderResources[ i * 4 + 0]	=	LoadColorTexture( content, layers[i].ColorTexture		).Srv;
					shaderResources[ i * 4 + 1]	=	LoadColorTexture( content, layers[i].SurfaceTexture		).Srv;
					shaderResources[ i * 4 + 2]	=	LoadColorTexture( content, layers[i].NormalMapTexture	).Srv;
					shaderResources[ i * 4 + 3]	=	LoadColorTexture( content, layers[i].EmissionTexture	).Srv;
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
		/// <param name="content"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		Texture LoadColorTexture ( ContentManager content, string path )
		{
			var defaultTexture	=	content.GameEngine.GraphicsEngine.GrayTexture;

			if (string.IsNullOrWhiteSpace(path)) {
				return defaultTexture;
			}

			return content.Load<Texture>( path + "|srgb", defaultTexture );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		Texture LoadSpecularTexture ( ContentManager content, string path )
		{
			var defaultTexture	=	content.GameEngine.GraphicsEngine.BlackTexture;

			if (string.IsNullOrWhiteSpace(path)) {
				return defaultTexture;
			}

			return content.Load<Texture>( path, defaultTexture );
		}



		/// <summary>
		/// Loads normal map texture
		/// </summary>
		/// <param name="content"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		Texture LoadNormalMapTexture ( ContentManager content, string path )
		{
			var defaultTexture	=	content.GameEngine.GraphicsEngine.FlatNormalMap;

			if (string.IsNullOrWhiteSpace(path)) {
				return defaultTexture;
			}

			return content.Load<Texture>( path, defaultTexture );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		Texture LoadEmissionTexture ( ContentManager content, string path )
		{
			var defaultTexture	=	content.GameEngine.GraphicsEngine.BlackTexture;

			if (string.IsNullOrWhiteSpace(path)) {
				return defaultTexture;
			}

			return content.Load<Texture>( path + "|srgb" );
		}


	}
}
																	    