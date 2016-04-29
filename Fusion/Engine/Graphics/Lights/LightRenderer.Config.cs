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

namespace Fusion.Engine.Graphics {
	public partial class LightRenderer : GameModule {

		int csmSize;

		/// <summary>
		/// Cascaded shadow map size.
		/// </summary>
		[Config]
		public int CSMSize {
			get {
				return csmSize;
			}
			set {
				csmSize =	value;
				csmSize =	MathUtil.Clamp( 1 << (MathUtil.LogBase2( csmSize )-1), 64, 2048 );
			}
		}

		/// <summary>
		/// First split size
		/// </summary>
		[Config]
		public float CSMProjectionDepth { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		[Config]
		public float SplitSize { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		[Config]
		public float SplitOffset { get; set; }

		/// <summary>
		/// Split size and offset magnification factor
		/// </summary>
		[Config]
		public float SplitFactor { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float CSMSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float CSMDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float CSMFilterSize { get; set; }



		int spotShadowSize;

		[Config]
		public int SpotShadowSize {
			get {
				return spotShadowSize;
			}
			set {
				spotShadowSize =	value;
				spotShadowSize =	MathUtil.Clamp( 1 << (MathUtil.LogBase2( spotShadowSize )-1), 64, 1024 );
			}
		}


		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float SpotSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float SpotDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Config]
		public float SpotFilterSize { get; set; }


		/// <summary>
		/// UseUE4LightingModel
		/// </summary>
		[Config]
		public bool UseUE4LightingModel { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Config]
		public bool SkipShadows { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Config]
		public bool SkipDirectLight { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Config]
		public bool SkipOmniLights { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Config]
		public bool SkipSpotLights { get; set; }

		/// <summary>
		/// Show CSM's splits
		/// </summary>
		[Config]
		public bool ShowCSMSplits { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowOmniLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowSpotLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowOmniLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowEnvLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowSpotLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowOmniLights { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Config]
		public bool ShowSpotLights { get; set; }


		void SetDefaults ()
		{
			CSMProjectionDepth	=	1024;
			CSMSize				=	1024;
			SplitSize			=	10;
			SplitFactor			=	2.5f;
			CSMSlopeBias		=	2;
			CSMDepthBias		=	0.0001f;
			CSMFilterSize		=	2;

			SpotShadowSize		=	512;
			SpotSlopeBias		=	2;
			SpotDepthBias		=	0.0001f;
			SpotFilterSize		=	2;
		}
	}
}

