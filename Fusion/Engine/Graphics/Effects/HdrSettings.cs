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
using Fusion.Engine.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Defines tonemapping operators
	/// </summary>
	public enum TonemappingOperator {
		Linear,
		Reinhard,
		Filmic,
	}


	/// <summary>
	/// Represents HDR processing properties.
	/// </summary>
	public class HdrSettings {

		/// <summary>
		/// Tonemapping operator.
		/// </summary>
		public TonemappingOperator TonemappingOperator { get; set; }
		
		/// <summary>
		/// Time to adapt. Default value is 0.5 seconds.
		/// </summary>
		public float AdaptationHalfLife { get; set; }

		/// <summary>
		/// Luminance key value. Default value is 0.18.
		/// </summary>
		public float KeyValue { get; set; } 

		/// <summary>
		/// Minimum luminnance. Default is zero.
		/// </summary>
		public float LuminanceLowBound { get; set; }

		/// <summary>
		/// Maximum luminance. Default is 99999.
		/// </summary>
		public float LuminanceHighBound { get; set; }
		
		/// <summary>
		/// Bloom gaussian blur sigma. Default is 3.
		/// </summary>
		public float GaussBlurSigma { 
			get { return gaussBlurSigma; }
			set { gaussBlurSigma = MathUtil.Clamp( value, 1, 5 ); }
		}

		float gaussBlurSigma;

		/// <summary>
		/// Amount of bloom. Zero means no bloom.
		/// One means fully bloomed image.
		/// </summary>
		public float BloomAmount { get; set; }

		/// <summary>
		/// Amount of dirt. Zero means no bloom.
		/// One means fully bloomed image.
		/// </summary>
		public float DirtAmount { get; set; }

		/// <summary>
		/// Dirt mask lerp factor.
		/// Default value is zero.
		/// </summary>
		public float DirtMaskLerpFactor { get; set; }

		/// <summary>
		/// Dirt mask #1. This value could be null.
		/// </summary>
		public Texture DirtMask1 { get; set; }

		/// <summary>
		/// Dirt mask #2. This value could be null.
		/// </summary>
		public Texture DirtMask2 { get; set; }


		/// <summary>
		/// Gets and sets overall image saturation
		/// Default value is 1.
		/// </summary>
		public float Saturation { get; set; }

		/// <summary>
		/// Minimum output value.
		/// Default value is 1.
		/// </summary>
		public float MaximumOutputValue { get; set; }

		/// <summary>
		/// Minimum output value.
		/// Default value is 0.
		/// </summary>
		public float MinimumOutputValue { get; set; }


		/// <summary>
		/// Ctor.
		/// </summary>
		public HdrSettings ()
		{
			TonemappingOperator	=	TonemappingOperator.Filmic;
			KeyValue			=	0.18f;
			AdaptationHalfLife	=	0.5f;
			LuminanceLowBound	=	0.0f;
			LuminanceHighBound	=	99999.0f;
			BloomAmount			=	0.1f;
			GaussBlurSigma		=	3.0f;
			DirtAmount			=	0.9f;

			Saturation			=	1;
			MaximumOutputValue	=	1;
			MinimumOutputValue	=	0;

		}
	}
}
