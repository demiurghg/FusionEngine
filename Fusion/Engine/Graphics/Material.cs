using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Reprsents material.
	/// </summary>
	public class Material {
		
		/// <summary>
		/// Indicates that material is tranparent.
		/// </summary>
		public bool Transparent;

		/// <summary>
		/// Indicates that object with this material should cast shadow.
		/// </summary>
		public bool CastShadow;
		
		/// <summary>
		/// Indicates that material uses displacement mapping.
		/// </summary>
		public bool UseDisplacement;

		/// <summary>
		/// Material options for mapping and layer blending.
		/// Default value is SingleLayer.
		/// </summary>
		public MaterialOptions Options;

		/// <summary>
		/// Material layer #0.
		/// Null value means no layer.
		/// </summary>
		public MaterialLayer	Layer0;

		/// <summary>
		/// Material layer #1
		/// Null value means no layer.
		/// </summary>
		public MaterialLayer	Layer1;

		/// <summary>
		/// Material layer #2
		/// Null value means no layer.
		/// </summary>
		public MaterialLayer	Layer2;

		/// <summary>
		/// Material layer #2
		/// Null value means no layer.
		/// </summary>
		public MaterialLayer	Layer3;


		/// <summary>
		/// Creates non-transparent material that casts shadow from color texture.
		/// Method search for existing textures with postfixes like "_s", "_n", "_e" 
		/// and substitutes them into material.
		/// </summary>
		/// <param name="path"></param>
		public static Material CreateFromTexture ( string path )
		{
			var mtrl = new Material();
			mtrl.Layer0	=	new MaterialLayer();
			mtrl.Layer0.ColorTexture		=	path + "|srgb";
			mtrl.Layer0.SurfaceTexture		=	path + "_s";
			mtrl.Layer0.NormalMapTexture	=	path + "_n";
			mtrl.Layer0.EmissionTexture		=	path + "_e|srgb";

			mtrl.Layer1	=	null;
			mtrl.Layer2	=	null;
			mtrl.Layer3	=	null;

			mtrl.UseDisplacement	=	false;
			mtrl.Transparent		=	false;
			mtrl.CastShadow			=	true;

			return mtrl;
		}
	}
}
																	    