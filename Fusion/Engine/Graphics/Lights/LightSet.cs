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
		/// Due to technical limitations only one source of direct 
		/// light is avaiable foreach LightSet.
		/// </summary>
		public DirectLight DirectLight {
			get {
				return directLight;
			}
		}
		


		/// <summary>
		/// Spot-light mask atlas.
		/// </summary>
		public TextureAtlas SpotAtlas {
			get; set;
		}


		/// <summary>
		/// Average ambient level.
		/// </summary>
		public Color4 AmbientLevel {
			get; set; 
		}


		DirectLight		directLight = new DirectLight();
		List<OmniLight> omniLights = new List<OmniLight>();
		List<SpotLight> spotLights = new List<SpotLight>();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		public LightSet ( RenderSystem rs )
		{
			AmbientLevel	=	Color4.Zero;
		}
	}
}
