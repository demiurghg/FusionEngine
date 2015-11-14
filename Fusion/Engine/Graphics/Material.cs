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
	}
}
																	    