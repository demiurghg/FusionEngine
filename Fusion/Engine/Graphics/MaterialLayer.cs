using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Reprsents single material layer.
	/// </summary>
	public class MaterialLayer {

		/// <summary>
		/// Color texture multiplier.
		/// </summary>
		public float ColorLevel;

		/// <summary>
		/// Alpha multiplier.
		/// </summary>
		public float AlphaLevel;

		/// <summary>
		/// Emission multiplier.
		/// </summary>
		public float EmissionLevel;

		/// <summary>
		/// Specular multiplier.
		/// </summary>
		public float SpecularLevel;

		/// <summary>
		/// Bumpiness level.
		/// </summary>
		public float BumpLevel;

		/// <summary>
		/// Displacement level.
		/// </summary>
		public float Displacement;

		/// <summary>
		/// Rougheness mapping range (min, max).
		/// </summary>
		public Vector2 RoughnessRange;

		/// <summary>
		/// Emission glow narrowness range (min, max, bias(?)).
		/// </summary>
		public Vector2 GlowNarrowness;

		/// <summary>
		/// Blend factor. 
		/// Zero value means soft blend. 
		/// One means harsh blending.
		/// </summary>
		public float BlendFactor;

		/// <summary>
		/// Tiling. Default value is (1, 1, 1).
		/// Z-coordinate is used for triplanar mapping.
		/// </summary>
		public Vector3 Tiling;

		/// <summary>
		/// Offset. Default value is (0, 0, 0).
		/// Z-coordinate is used for triplanar mapping.
		/// </summary>
		public Vector3 Offset;

		/// <summary>
		/// Path to color texture.
		/// </summary>
		public string ColorTexture;
		
		/// <summary>
		/// Path to surface texture.
		/// </summary>
		public string SurfaceTexture;		

		/// <summary>
		/// Path to normal map texture.
		/// </summary>
		public string NormalMapTexture;

		/// <summary>
		/// Path to emission texture.
		/// </summary>
		public string EmissionTexture;	
		
		
		/// <summary>
		/// 
		/// </summary>
		public MaterialLayer ()
		{
			ColorLevel		=	1;
			AlphaLevel		=	1;
			SpecularLevel	=	1;
			BumpLevel		=	1;
			EmissionLevel	=	1;
			Displacement	=	1;

			GlowNarrowness	=	new Vector2(0,0);

			RoughnessRange	=	new Vector2(0,1);

			Tiling	=	Vector3.One;
			Offset	=	Vector3.Zero;

			ColorTexture		=	"*grey";
			SurfaceTexture		=	"*metal";
			NormalMapTexture	=	"*flat";
			EmissionTexture		=	"*black";
		}	
	}
}
																	    