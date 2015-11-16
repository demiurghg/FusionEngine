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
	public enum SkyType {
		Procedural,
		Panoramic_Half,
		Panoramic_Full,
		CubeMap
	}



	public enum RgbSpace {
		sRGB,
		CIE_RGB,
	}


	public class SkyConfig {
		public float	SkyIntensity { get; set; }
		public Vector3	SunDirection { get; set; }
		public float	SunGlowIntensity { get; set; }
		public float	SunLightIntensity { get; set; }
		public int		SunTemperature { get; set; }
		public float	SkyTurbidity { get; set; }
		public float	SkySphereSize { get; set; }
		public float	AerialFogDensity { get; set; }
		public float	ScatteringLevel { get; set; }
		public RgbSpace	RgbSpace { get; set; }


		public SkyConfig()
		{
			RgbSpace	= RgbSpace.sRGB;
			AerialFogDensity = 0.001f;
			SkySphereSize = 5000.0f;
			SkyTurbidity = 4.0f;
			SunDirection = new Vector3( 1.0f, 0.5f, -1.0f );
			SunGlowIntensity = 1f;
			SunLightIntensity = 0.1f;
			SunTemperature = 5700;
			SkyIntensity = 1.0f;
			ScatteringLevel = 0.1f;
		}
	}
}
