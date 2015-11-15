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

namespace DeferredDemo {

	public enum TonemappingOperator {
		Linear,
		Reinhard,
		Filmic,
	}


	public class HdrFilterConfig {
		
		public TonemappingOperator TonemappingOperator { get; set; }
		
		public float AdaptationHalfLife { get; set; }

		public float KeyValue { get; set; } 

		public float LuminanceLowBound { get; set; }

		public float LuminanceHighBound { get; set; }
		
		public float GaussBlurSigma { get; set; }

		public float BloomAmount { get; set; }


		public HdrFilterConfig ()
		{
			TonemappingOperator	=	TonemappingOperator.Filmic;
			KeyValue			=	0.18f;
			AdaptationHalfLife	=	0.5f;
			LuminanceLowBound	=	0.0f;
			LuminanceHighBound	=	99999.0f;
			BloomAmount			=	0.1f;
			GaussBlurSigma		=	3.0f;

		}
	}
}
