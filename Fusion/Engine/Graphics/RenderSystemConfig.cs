using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {

	public class RenderSystemConfig {

		public const int	EnvMapSize = 128;
		public const int	EnvMapSpecularMipCount = 7;
		public const int	EnvMapDiffuseMipLevel = 7;

		public const int	MaxOmniLights	=	1024;
		public const int	MaxEnvLights	=	256;
		public const int	MaxSpotLights	=	16;


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
		/// Shows G-buffer content.
		///		0 - show final image
		///		1 - show diffuse
		///		2 - show specular
		///		3 - show normal map
		/// </summary>
		public int ShowGBuffer { get; set; }


		/// <summary>
		/// Shows counters
		/// </summary>
		public bool ShowCounters { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public bool UseFXAA { get; set; }


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
			VSyncInterval	=	1;
			MsaaEnabled		=	false;
			UseFXAA			=	true;
		}

	}
}
