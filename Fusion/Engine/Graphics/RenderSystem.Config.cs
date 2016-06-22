using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Core.Configuration;

namespace Fusion.Engine.Graphics {

	public partial class RenderSystem : GameComponent {

		public const int	EnvMapSize = 128;
		public const int	EnvMapSpecularMipCount = 7;
		public const int	EnvMapDiffuseMipLevel = 7;

		public const int	MaxOmniLights	=	1024;
		public const int	MaxEnvLights	=	256;
		public const int	MaxSpotLights	=	16;


		/// <summary>
		/// Fullscreen
		/// </summary>
		[Config]
		public bool Fullscreen { 
			get { 
				return isFullscreen;
			}
			set { 
				if (isFullscreen!=value) {
					isFullscreen = value;
					if (Device!=null && Device.IsInitialized) {
						Device.FullScreen = value;
					}
				}
			}
		}
		bool isFullscreen = false;

		/// <summary>
		/// Screen width
		/// </summary>
		[Config]
		public int	Width { get; set; }

		/// <summary>
		/// Screen height
		/// </summary>
		[Config]
		public int	Height { get; set; }
		
		/// <summary>
		/// Stereo mode.
		/// </summary>
		[Config]
		public StereoMode StereoMode { get; set; }

		/// <summary>
		/// Interlacing mode for stereo.
		/// </summary>
		[Config]
		public InterlacingMode InterlacingMode { get; set; }

		/// <summary>
		/// Vertical synchronization interval.
		/// </summary>
		[Config]
		public int VSyncInterval { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Config]
		public bool UseDebugDevice { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Config]
		public bool MsaaEnabled { get; set; }


		/// <summary>
		/// Forbids creation of default render world.
		/// </summary>
		[Config]
		public bool NoDefaultRenderWorld { get; set; }


		/// <summary>
		/// Shows G-buffer content.
		///		0 - show final image
		///		1 - show diffuse
		///		2 - show specular
		///		3 - show normal map
		/// </summary>
		[Config]
		public int ShowGBuffer { get; set; }


		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool ShowParticles { get; set; }


		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool SkipParticles { get; set; }

		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool SkipParticlesSimulation { get; set; }

		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool SkipSceneRendering { get; set; }

		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool SkipDebugRendering { get; set; }


		/// <summary>
		/// Shows particles statistics.
		/// </summary>
		[Config]
		public bool FreezeParticles { get; set; }


		/// <summary>
		/// Shows counters
		/// </summary>
		[Config]
		public bool ShowCounters { get; set; }

		/// <summary>
		/// Shows counters
		/// </summary>
		[Config]
		public bool ShowLightCounters { get; set; }


		/// <summary>
		/// 
		/// </summary>
		[Config]
		public bool UseFXAA { get; set; }
	}
}
