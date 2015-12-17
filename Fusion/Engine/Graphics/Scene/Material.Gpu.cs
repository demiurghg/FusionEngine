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

		struct ConstData {
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
			constBuffer		=	new ConstantBuffer( content.GameEngine.GraphicsDevice, typeof(ConstData) );

			shaderResources	=	new ShaderResource[16];

			if (Layer0!=null) {
				shaderResources[ 0]	=	LoadColorTexture( content, Layer0.ColorTexture		).Srv;
				shaderResources[ 1]	=	LoadColorTexture( content, Layer0.SurfaceTexture	).Srv;
				shaderResources[ 2]	=	LoadColorTexture( content, Layer0.NormalMapTexture	).Srv;
				shaderResources[ 3]	=	LoadColorTexture( content, Layer0.EmissionTexture	).Srv;
			}
			
			if (Layer1!=null) {				 
				shaderResources[ 4]	=	LoadColorTexture( content, Layer1.ColorTexture		).Srv;
				shaderResources[ 5]	=	LoadColorTexture( content, Layer1.SurfaceTexture	).Srv;
				shaderResources[ 6]	=	LoadColorTexture( content, Layer1.NormalMapTexture	).Srv;
				shaderResources[ 7]	=	LoadColorTexture( content, Layer1.EmissionTexture	).Srv;
			}
							 
			if (Layer1!=null) {				 
				shaderResources[ 8]	=	LoadColorTexture( content, Layer2.ColorTexture		).Srv;
				shaderResources[ 9]	=	LoadColorTexture( content, Layer2.SurfaceTexture	).Srv;
				shaderResources[10]	=	LoadColorTexture( content, Layer2.NormalMapTexture	).Srv;
				shaderResources[11]	=	LoadColorTexture( content, Layer2.EmissionTexture	).Srv;
			}
							 
			if (Layer1!=null) {				 
				shaderResources[12]	=	LoadColorTexture( content, Layer3.ColorTexture		).Srv;
				shaderResources[13]	=	LoadColorTexture( content, Layer3.SurfaceTexture	).Srv;
				shaderResources[14]	=	LoadColorTexture( content, Layer3.NormalMapTexture	).Srv;
				shaderResources[15]	=	LoadColorTexture( content, Layer3.EmissionTexture	).Srv;
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
																	    