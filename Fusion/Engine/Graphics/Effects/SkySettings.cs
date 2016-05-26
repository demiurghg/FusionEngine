using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
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

		/// <summary>
		/// 
		/// </summary>
		public float AerialFogDensity { get; set; }

		/// <summary>
		/// Vector that points toward the sun.
		/// </summary>
		public Vector3 SunPosition { get; set; }

		/// <summary>
		/// Sun glow intensity
		/// </summary>
		public float SunGlowIntensity { get; set; }

		/// <summary>
		/// Sun light intensity
		/// </summary>
		public float SunLightIntensity { get; set; }

		/// <summary>
		/// Maximal sun temperature.
		/// </summary>
		public int SunTemperature { get; set; }

		/// <summary>
		/// RGB space.
		/// </summary>
		public RgbSpace	RgbSpace { get; set; }

		/// <summary>
		/// Gets normalized sun-light direction.
		/// </summary>
		public Vector3 SunLightDirection {
			get {
				return -SunPosition.Normalized();
			}
		}

		/// <summary>
		/// Lerps sun color temperature from 2000K to 5500K and multiplies result by SunLightIntensity.
		/// </summary>
		public Color4 SunLightColor {
			get {
				Color4 dusk		=	new Color4(Temperature.Get(2000), 1);
				Color4 zenith	=	new Color4(Temperature.Get(SunTemperature), 1);

				Vector3 ndir	=	SunPosition.Normalized();

				return Color4.Lerp( dusk, zenith, (float)Math.Pow(ndir.Y, 0.5f) ) * SunLightIntensity;
			}
		}

		/// <summary>
		/// Lerps sun color temperature from 2000K to 5500K and multiplies result by SunGlowIntensity.
		/// </summary>
		public Color4 SunGlowColor {
			get {
				Color4 dusk		=	new Color4(Temperature.Get(2000), 1);
				Color4 zenith	=	new Color4(Temperature.Get(SunTemperature), 1);

				Vector3 ndir	=	SunPosition.Normalized();

				return Color4.Lerp( dusk, zenith, (float)Math.Pow(ndir.Y, 0.5f) ) * SunGlowIntensity;
			}
		}


		/// <summary>
		/// Gets average ambient level.
		/// </summary>
		public Color4 AmbientLevel {
			get {
				var sunPos = SunPosition.Normalized();
				var ambientLight = Vector3.Zero;

				var norm = randVectors.Length;// * 2 * MathUtil.Pi;

				for (int i = 0; i < randVectors.Length; i++) {
					var yxy = SkyModel.perezSky( SkyTurbidity, randVectors[i], sunPos );
					var rgb = SkyModel.YxyToRGB( yxy );// * Temperature.Get( settings.SunTemperature );
					ambientLight += rgb / norm;
				}

				return new Color4(ambientLight,1) * SkyIntensity;
			}
		}


		Vector3[] randVectors;



		public SkySettings()
		{
			RgbSpace			= RgbSpace.sRGB;
			AerialFogDensity	= 0.001f;
			SkySphereSize		= 1000.0f;
			SkyTurbidity		= 4.0f;
			SunPosition		= new Vector3( 1.0f, 0.1f, 1.0f );
			SunGlowIntensity	= Half.MaxValue/2;
			SunLightIntensity	= 300.0f;
			SunTemperature		= 5500;
			SkyIntensity		= 1.0f;


			randVectors	=	new Vector3[64];
			var rand	=	new Random(465464);

			for (int i=0; i<randVectors.Length; i++) {
				Vector3 randV;
				do {
					randV = rand.NextVector3( -Vector3.One, Vector3.One );
				} while ( randV.Length()>1 && randV.Y < 0 );

				randVectors[i] = randV.Normalized();
			}
		}
	}
}
