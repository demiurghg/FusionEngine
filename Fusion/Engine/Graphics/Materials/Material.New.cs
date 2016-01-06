using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.IniParser;
using System.IO;
using Fusion.Core;
using Fusion.Core.IniParser.Model;
using Fusion.Core.IniParser.Model.Formatting;
using Fusion.Core.Content;
using Fusion.Drivers.Graphics;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {
	
	public enum ShadingFunction {

		/// <summary>
		/// Base material with color, surface properties, normal map and emission.
		/// </summary>
		BaseIllum,

		/// <summary>
		/// Base transparent material with color, surface properties, normal map and emission.
		/// </summary>
		BaseGlass,

		/// <summary>
		/// Advanced version of BaseIllum with four emission textures.
		/// </summary>
		AdvancedIllum,

		/// <summary>
		/// Same as BaseIllum, but with applied monitor effects to emission
		/// </summary>
		Monitor,

		/// <summary>
		/// Four layer terrain. Layers are blended using vertex color mask.
		/// </summary>
		Terrain,

		/// <summary>
		/// Two layer material with 
		/// </summary>
		TwoLayerIllum,

		/// <summary>
		/// Triplanar mapping.
		/// </summary>
		Triplanar,

		/// <summary>
		/// Triplanar mapping with cap. Use another set of textures.
		/// </summary>
		TriplanarCapped,

		/// <summary>
		/// Skin material
		/// </summary>
		Skin,

		/// <summary>
		/// Eye material
		/// </summary>
		Eye
	}


	/// <summary>
	/// Reprsents material.
	/// </summary>
	public partial class MaterialNew : DisposableBase {

		public bool NoShadow { get; set; }

		public bool TwoSided { get; set; }

		public bool PhongTesselation { get; set; }

		public bool DisplacementMapping { get; set; }

		public bool UseAlphaTest { get; set; }

		//	always enabled?
		public bool UseDirtMap { get; set; }
		
		public bool UseDetailMap { get; set; }

		public bool DetailMaskInAlpha { get; set; }

		public bool EmissionMaskInAlpha { get; set; }

		public bool ModulateColorByVertexColor { get; set; }

		public bool ObjectSpaceTriplanarMapping { get; set; }


		//	Master parameters :
		public float RoughnessMinimum;
		public float RoughnessMaximum;
		public float GlossLevel;
		public float ColorLevel;
		public float EmissionLevel;
		public float DisplacementLevel;

		public Color4	SSSColor;
		public float	SSSLevel;

		public float	RefractionLevel;

		// Monitor parameters
		public Color4	MonitorTint;
		public float	MonitorChromaShift;
		public float	MonitorContrast;
		public float	MonitorSaturation;
		public float	MonitorNoiseLevel;
		public float	MonitorInterference;
		public float	MonitorGhosting;
		public float	MonitorFrequency;



	}
}
																	    