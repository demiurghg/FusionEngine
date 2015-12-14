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
		}



		/// <summary>
		/// 
		/// </summary>
		void DisposeGpuResources ()
		{
			SafeDispose( ref constBuffer );
			SafeDispose( ref shaderResources );
		}
	}
}
																	    