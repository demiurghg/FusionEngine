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
	public class LightSet {

		/// <summary>
		/// Collection of omni lights.
		/// </summary>
		public ICollection<OmniLight> OmniLights {
			get {
				return omniLights;	
			}
		}


		/// <summary>
		/// Collection of spot lights.
		/// </summary>
		public ICollection<SpotLight> SpotLights {
			get {
				return spotLights;	
			}
		}


		/// <summary>
		/// Due to technical limitations only one 
		/// source of direct light foreach LightSet is avaiable.
		/// </summary>
		public DirectLight DirectLight {
			get {
				return directLight;
			}
		}
		



		DirectLight		directLight = new DirectLight();
		List<OmniLight> omniLights = new List<OmniLight>();
		List<SpotLight> spotLights = new List<SpotLight>();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ge"></param>
		public LightSet ( GraphicsEngine ge )
		{
		}
	}
}
