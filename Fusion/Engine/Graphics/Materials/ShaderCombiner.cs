using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Defines the way how input textures and parameters will be combined.
	/// </summary>
	public enum ShaderCombiner {

		/// <summary>
		/// Base material with color texture, surface texture, normal map and emission texture.
		/// Material has dirt texture and optional detail map.
		/// Emission OR detail map could be modulated by color texture alpha.
		/// </summary>
		BaseIllum,

	}
}
