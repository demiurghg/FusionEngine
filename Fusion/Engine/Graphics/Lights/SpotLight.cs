using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Scene;

namespace Fusion.Engine.Graphics {

	public class SpotLight {
		/// <summary>
		/// Spot-light view matrix.
		/// </summary>
		public Matrix	SpotView;

		/// <summary>
		/// Spot-light projection matrix.
		/// </summary>
		public Matrix	Projection;

		/// <summary>
		/// Spot-light intensity.
		/// </summary>
		public Color4	Intensity;

		/// <summary>
		/// Spot-light mask atlas name.
		/// </summary>
		public string	MaskName;

		/// <summary>
		/// Spot-light inner radius.
		/// </summary>
		public float	RadiusInner;

		/// <summary>
		/// Spot-light outer radius.
		/// </summary>
		public float	RadiusOuter;
	}
}
