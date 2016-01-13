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
	public class LightRendererConfig {

		int csmSize;

		/// <summary>
		/// Cascaded shadow map size.
		/// </summary>
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
		public float CSMDepth { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		public float SplitSize { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		public float SplitOffset { get; set; }

		/// <summary>
		/// Split size and offset magnification factor
		/// </summary>
		public float SplitFactor { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		public float CSMSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		public float CSMDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		public float CSMFilterSize { get; set; }



		int spotShadowSize;

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
		public float SpotSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		public float SpotDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		public float SpotFilterSize { get; set; }


		/// <summary>
		/// UseUE4LightingModel
		/// </summary>
		public bool UseUE4LightingModel { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		public bool SkipShadows { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		public bool SkipDirectLight { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		public bool SkipOmniLights { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		public bool SkipSpotLights { get; set; }

		/// <summary>
		/// Show CSM's splits
		/// </summary>
		public bool ShowCSMSplits { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowOmniLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowSpotLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowOmniLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowEnvLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowSpotLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowOmniLights { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		public bool ShowSpotLights { get; set; }


		public LightRendererConfig ()
		{
			CSMDepth		=	1024;
			CSMSize			=	1024;
			SplitSize		=	10;
			SplitFactor		=	2.5f;
			CSMSlopeBias	=	2;
			CSMDepthBias	=	0.0001f;
			CSMFilterSize	=	2;

			SpotShadowSize	=	512;
			SpotSlopeBias	=	2;
			SpotDepthBias	=	0.0001f;
			SpotFilterSize	=	2;
		}
	}
}

