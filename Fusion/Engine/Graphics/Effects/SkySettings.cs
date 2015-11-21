using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics {

	public enum RgbSpace {
		sRGB,
		CIE_RGB,
	}


	public class SkySettings {

		/// <summary>
		/// Sky intensity scale.
		/// </summary>
		public float SkyIntensity { get; set; }

		/// <summary>
		/// Sky turbidity level. Must be within range 2..8.
		/// </summary>
		public float SkyTurbidity { get; set; }

		/// <summary>
		/// Sky sphere size.
		/// </summary>
		public float SkySphereSize { get; set; }


		public float AerialFogDensity { get; set; }


		/// <summary>
		/// Sun direction
		/// </summary>
		public Vector3 SunDirection { get; set; }

		/// <summary>
		/// Sun glow intensity
		/// </summary>
		public float SunGlowIntensity { get; set; }

		/// <summary>
		/// Sun light intensity
		/// </summary>
		public float SunLightIntensity { get; set; }

		/// <summary>
		/// Sun temperature.
		/// </summary>
		public int SunTemperature { get; set; }


		/// <summary>
		/// RGB space.
		/// </summary>
		public RgbSpace	RgbSpace { get; set; }


		public SkySettings()
		{
			RgbSpace			= RgbSpace.sRGB;
			AerialFogDensity	= 0.001f;
			SkySphereSize		= 1000.0f;
			SkyTurbidity		= 4.0f;
			SunDirection		= new Vector3( 1.0f, 1.0f, 1.0f );
			SunGlowIntensity	= Half.MaxValue;
			SunLightIntensity	= 1f;
			SunTemperature		= 5700;
			SkyIntensity		= 1.0f;
		}
	}
}
