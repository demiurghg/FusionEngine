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
		/// Minimal rougheness.
		/// </summary>
		public float MinRoughness;

		/// <summary>
		/// Maximal rougheness.
		/// </summary>
		public float MaxRoughness;

		/// <summary>
		/// Blend factor. 
		/// Zero value means soft blend. 
		/// One means harsh blending.
		/// </summary>
		public float BlendFactor;

		/// <summary>
		/// Tiling. Default value is (1, 1).
		/// </summary>
		public float TilingU;

		/// <summary>
		/// Tiling. Default value is (1, 1).
		/// </summary>
		public float TilingV;

		/// <summary>
		/// Tiling. Default value is (1, 1).
		/// </summary>
		public float TilingW;

		/// <summary>
		/// Offset. Default value is (0, 0).
		/// </summary>
		public float OffsetU;

		/// <summary>
		/// Offset. Default value is (0, 0).
		/// </summary>
		public float OffsetV;

		/// <summary>
		/// Offset. Default value is (0, 0).
		/// </summary>
		public float OffsetW;

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
			SpecularLevel	=	1;
			BumpLevel		=	1;
			EmissionLevel	=	1;
			Displacement	=	1;

			MinRoughness	=	0;
			MaxRoughness	=	1;

			TilingU	=	1;
			TilingV	=	1;
			TilingW	=	1;
			OffsetU	=	0;
			OffsetV	=	0;
			OffsetW	=	0;
		}	
	}
}
																	    