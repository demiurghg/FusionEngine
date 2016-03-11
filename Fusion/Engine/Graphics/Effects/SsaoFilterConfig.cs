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
	public class SsaoFilterConfig {

		public enum sampleNum
		{
			s_4		= 4,
			s_8		= 8,
			s_16	= 16,
			s_32	= 32,
			s_64	= 64
		}

        public enum filterType
        {
            GAUSS,
            SQUARE,
        }

		public enum Method
		{
			HEMISPHERE,
			HBAO,
		}


		public bool Enabled { get; set; }
		//[Category("HBAO")]
		//public float TraceStep { get; set; }
		
		//[Category("HBAO")]
		//public float DecayRate { get; set; }
		[Category("AMBIENT OCCLUSION METHOD")]
		public Method AOMethod { get; set; }
		
		[Category("SETTINGS")]
		public float BlurSigma { get; set; }

		[Category("SETTINGS")]
		public float MaxSamplingRadius { get; set; }

		[Category("SETTINGS")]
		public float MaxDepthJump { get; set; }

		[Category("SETTINGS")]
		public float Sharpness { get; set; }

		[Category("SETTINGS")]
		public sampleNum SampleNumber { get; set; }

		[Category("SETTINGS")]
        public filterType FilterType { get; set; }
		/// <summary>
		///
		/// </summary>
		public SsaoFilterConfig()
		{
			AOMethod			= Method.HBAO;
			MaxSamplingRadius	= 1.6f;
			MaxDepthJump		= 1.0f;
			Sharpness			= 5.0f;
			SampleNumber		= sampleNum.s_16;
            FilterType          = filterType.GAUSS;
		}
	}
}
