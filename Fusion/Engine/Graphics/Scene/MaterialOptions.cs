using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Defines material mapping and layer blending options
	/// </summary>
	public enum MaterialOptions {
		
		/// <summary>
		/// Single layer material.
		/// </summary>
		SingleLayer,
		
		/// <summary>
		/// Double layer material
		/// </summary>
		DoubleLayer,
		
		/// <summary>
		/// Triple layer material
		/// </summary>
		TripleLayer,
		
		/// <summary>
		/// Quadruple layer material
		/// </summary>
		QuadLayer,
		
		/// <summary>
		/// Use vertex color as blend weights 
		/// for material's layers.
		/// </summary>
		Terrain,

		/// <summary>
		/// Triplanar mapping in orld space with single layer 
		/// aplied for each side.
		/// </summary>
		TriplanarWorldSingle,

		/// <summary>
		/// Triplanar mapping in world space with two layers.
		/// First layer is applied on top and bottom size.
		/// Second layer is applied on left, right, front and back side of the object.
		/// </summary>
		TriplanarWorldDouble,

		/// <summary>
		/// Triplanar mapping in world space with three layers.
		/// Each layers is applied to YZ, XZ and XY planes.
		/// </summary>
		TriplanarWorldTriple,
	}
}