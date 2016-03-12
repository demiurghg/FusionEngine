using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Audio {
	public struct CurvePoint {
		
		public CurvePoint ( float distance, float dspSetting )
		{
			Distance	=	distance;
			DspSetting	=	dspSetting;
		}

		public float	Distance;
		public float	DspSetting;
	}
}
