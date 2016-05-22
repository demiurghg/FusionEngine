using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Content;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using Fusion.Engine.Common;


namespace Fusion.Engine.Graphics {
	internal partial class SsaoFilter : GameModule {

		public enum SampleNum
		{
			s_4		= 4,
			s_8		= 8,
			s_16	= 16,
			s_32	= 32,
			s_64	= 64
		}

        public enum SsaoFilterType {
            GAUSS,
            SQUARE,
        }

		public enum SsaoMethod {
			HEMISPHERE,
			HBAO,
		}


		[Config] public bool Enabled { get; set; }

		[Config] public SsaoMethod Method { get; set; }
		
		[Config] public float BlurSigma { get; set; }

		[Config] public float MaxSamplingRadius { get; set; }

		[Config] public float MaxDepthJump { get; set; }

		[Config] public float Sharpness { get; set; }

		[Config] public SampleNum SampleNumber { get; set; }

        [Config] public SsaoFilterType FilterType { get; set; }

		/// <summary>
		///
		/// </summary>
		void SetDefaults()
		{
			Method				= SsaoMethod.HBAO;
			MaxSamplingRadius	= 1.6f;
			MaxDepthJump		= 1.0f;
			Sharpness			= 5.0f;
			SampleNumber		= SampleNum.s_16;
            FilterType          = SsaoFilterType.GAUSS;
		}
	}
}
