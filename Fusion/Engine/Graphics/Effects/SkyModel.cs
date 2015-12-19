using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Graphics {

	static public class SkyModel {

		static float dot ( Vector3 a, Vector3 b ) { return Vector3.Dot(a, b); }
		static float dot ( Vector4 a, Vector4 b ) { return Vector4.Dot(a, b); }
		static float acos(float x) { return (float)Math.Acos(x);}
		static float tan (float x) { return (float)Math.Tan(x);}
		static float exp (float x) { return (float)Math.Exp(x);}

		static Vector3 perezZenith ( float t, float thetaSun )
		{
			float	pi = 3.1415926f;
			Vector4	cx1 = new Vector4 ( 0.00000f,  0.00209f, -0.00375f,  0.00165f );
			Vector4	cx2 = new Vector4 ( 0.00394f, -0.03202f,  0.06377f, -0.02903f );
			Vector4	cx3 = new Vector4 ( 0.25886f,  0.06052f, -0.21196f,  0.11693f );
			Vector4	cy1 = new Vector4 ( 0.00000f,  0.00317f, -0.00610f,  0.00275f );
			Vector4	cy2 = new Vector4 ( 0.00516f, -0.04153f,  0.08970f, -0.04214f );
			Vector4	cy3 = new Vector4 ( 0.26688f,  0.06670f, -0.26756f,  0.15346f );

			float	t2    = t*t;
			float	chi   = (4.0f / 9.0f - t / 120.0f ) * (pi - 2.0f * thetaSun );
			Vector4	theta = new Vector4 ( 1, thetaSun, thetaSun*thetaSun, thetaSun*thetaSun*thetaSun );

			float	Y = (4.0453f * t - 4.9710f) * tan ( chi ) - 0.2155f * t + 2.4192f;
			float	x = t2 * dot ( cx1, theta ) + t * dot ( cx2, theta ) + dot ( cx3, theta );
			float	y = t2 * dot ( cy1, theta ) + t * dot ( cy2, theta ) + dot ( cy3, theta );

			return new Vector3 ( Y, x, y );//*/
		}


		public static Vector3  perezFunc ( float t, float cosTheta, float cosGamma )
		{
			//return 1;
			float  gamma      = acos ( cosGamma );
			float  cosGammaSq = cosGamma * cosGamma;
			float  aY =  0.17872f * t - 1.46303f;	      float  bY = -0.35540f * t + 0.42749f;
			float  cY = -0.02266f * t + 5.32505f;	      float  dY =  0.12064f * t - 2.57705f;
			float  eY = -0.06696f * t + 0.37027f;	      float  ax = -0.01925f * t - 0.25922f;
			float  bx = -0.06651f * t + 0.00081f;	      float  cx = -0.00041f * t + 0.21247f;
			float  dx = -0.06409f * t - 0.89887f;	      float  ex = -0.00325f * t + 0.04517f;
			float  ay = -0.01669f * t - 0.26078f;	      float  by = -0.09495f * t + 0.00921f;
			float  cy = -0.00792f * t + 0.21023f;	      float  dy = -0.04405f * t - 1.65369f;
			float  ey = -0.01092f * t + 0.05291f;	  
			return new Vector3 ( (1.0f + aY * exp(bY/cosTheta)) * (1.0f + cY * exp(dY * gamma) + eY*cosGammaSq),
							(1.0f + ax * exp(bx/cosTheta)) * (1.0f + cx * exp(dx * gamma) + ex*cosGammaSq),
							(1.0f + ay * exp(by/cosTheta)) * (1.0f + cy * exp(dy * gamma) + ey*cosGammaSq) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="t">Turbidity</param>
		/// <param name="cosTheta">cosine of view angle</param>
		/// <param name="cosGamma">cosine of angle between view dir and sun dir</param>
		/// <param name="cosThetaSun">cosine of sun angle</param>
		/// <returns></returns>
		public static Vector3  perezSky ( float t, float cosTheta, float cosGamma, float cosThetaSun )
		{
			float	thetaSun	=	acos ( cosThetaSun );
			Vector3 zenith		=	perezZenith ( t, thetaSun );
			Vector3 clrYxy		=	zenith * perezFunc ( t, cosTheta, cosGamma );
			Vector3	perez1		=	perezFunc ( t, 1.0f, cosThetaSun );

			clrYxy.X /= perez1.X;
			clrYxy.Y /= perez1.Y;
			clrYxy.Z /= perez1.Z;
			//clrYxy.X = clrYxy.X * smoothstep ( 0.0f, 0.1f, cosThetaSun );			// make sure when thetaSun > PI/2 we have black color
	
			return clrYxy;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="t">Turbidity</param>
		/// <param name="viewDir">Normalized (!) view direction</param>
		/// <param name="sunDir">Normalized (!) sun direction</param>
		/// <returns></returns>
		public static Vector3  perezSky ( float t, Vector3 viewDir, Vector3 sunDir )
		{
			return perezSky( t, Math.Max( 0, viewDir.Y ), Vector3.Dot( viewDir, sunDir ), Math.Max( 0, sunDir.Y ) );
		}


		public static Vector3 perezSun ( float t, float cosThetaSun, float boost=100 )
		{
			return perezSky( t, cosThetaSun, 1, cosThetaSun ) * new Vector3(boost,1,1);
		}


		public static Vector3 YxyToRGB ( Vector3 Yxy )
		{
			//var uc = Game.GetService<Settings>().UserConfig;

			Vector3  clrYxy = Yxy;

			clrYxy.X = MathUtil.Clamp( clrYxy.X, 0, 999999 );

			Vector3  XYZ;
			//float tm = 0.7f * 1/(1 + clrYxy.X); 
			float tm = 1;

			//clrYxy.X = 1.0f - exp ( -clrYxy.X / uc.SkyExposure );

			XYZ.X = clrYxy.X * clrYxy.Y / clrYxy.Z;     
			XYZ.Y = clrYxy.X;              
			XYZ.Z = (1 - clrYxy.Y - clrYxy.Z) * clrYxy.X / clrYxy.Z; 

			Vector3 rCoeffs = new Vector3 ( 2.0413690f, -0.5649464f, -0.3446944f);
			Vector3 gCoeffs = new Vector3 (-0.9692660f,  1.8760108f,  0.0415560f);
			Vector3 bCoeffs = new Vector3 ( 0.0134474f, -0.1183897f,  1.0154096f);

			return new Vector3 ( dot ( rCoeffs, XYZ ), dot ( gCoeffs, XYZ ), dot ( bCoeffs, XYZ ) ) * tm;
		}
	}
}
