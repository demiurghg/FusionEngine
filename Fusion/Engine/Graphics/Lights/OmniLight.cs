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
	public class OmniLight {
		/// <summary>
		/// Omni-light position
		/// </summary>
		public Vector3	Position;

		/// <summary>
		/// Omni-light intensity
		/// </summary>
		public Color4	Intensity;

		/// <summary>
		/// Omni-light inner radius.
		/// </summary>
		public float	RadiusInner;

		/// <summary>
		/// Omni-light outer radius.
		/// </summary>
		public float	RadiusOuter;
	}
}
