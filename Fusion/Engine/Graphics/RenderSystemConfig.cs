using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {

	public class RenderSystemConfig {

		internal static int EnvMapSize = 256;
		internal static int EnvMapSpecularMipCount = 8;
		internal static int EnvMapDiffuseMipLevel = 8;
		internal static int EnvMapFilterSampleCount = 64;

		/// <summary>
		/// Screen width
		/// </summary>
		public int	Width { get; set; }

		/// <summary>
		/// Screen height
		/// </summary>
		public int	Height { get; set; }

		/// <summary>
		/// Fullscreen.
		/// </summary>
		public bool Fullscreen { get; set; }
		
		/// <summary>
		/// Stereo mode.
		/// </summary>
		public StereoMode StereoMode { get; set; }

		/// <summary>
		/// Interlacing mode for stereo.
		/// </summary>
		public InterlacingMode InterlacingMode { get; set; }

		/// <summary>
		/// Vertical synchronization interval.
		/// </summary>
		public int VSyncInterval { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool UseDebugDevice { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool MsaaEnabled { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public bool TrackObjects { get; set; }


		/// <summary>
		/// Ctor
		/// </summary>
		public RenderSystemConfig ()
		{
			Width			=	1024;
			Height			=	768;
			Fullscreen		=	false;
			StereoMode		=	StereoMode.Disabled;
			InterlacingMode	=	InterlacingMode.HorizontalLR;
			UseDebugDevice	=	false;
			TrackObjects	=	true;
			VSyncInterval	=	1;
			MsaaEnabled		=	false;
		}

	}
}
