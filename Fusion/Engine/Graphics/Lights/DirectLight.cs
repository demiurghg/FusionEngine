using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class DirectLight {
		
		Vector3 direction;


		/// <summary>
		/// Indicates that given direct light is enabled.
		/// </summary>
		public bool Enabled {
			get;
			set;
		}


		/// <summary>
		/// Directional light intensity.
		/// Note: directional light normally does not decay.
		/// </summary>
		public Color4 Intensity {
			get;
			set;
		}


		/// <summary>
		/// The direction FROM distant light source (Sun, Moon, thunder etc).
		/// </summary>
		public Vector3 Direction {
			get {
				return direction;
			}
			set {
				direction = value;
			}
		}
	}
}
