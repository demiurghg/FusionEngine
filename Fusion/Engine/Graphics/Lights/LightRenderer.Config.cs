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
	internal partial class LightRenderer {

		int csmCascadeSize;
		int csmCascadeCount;
		object csmLock = new object();


		/// <summary>
		/// Cascaded shadow map size.
		/// </summary>
		[Config]
		public int CSMCascadeSize {	
			get { return csmCascadeSize; }
			set {
				lock (csmLock) {
					if (csmCascadeCount>=3) {
						if (value<64 || value>2048) {
							throw new ArgumentOutOfRangeException("value must be within 64..2048 for three of four cascades");
						}
					}
					if (csmCascadeCount==2) {
						if (value<64 || value>4096) {
							throw new ArgumentOutOfRangeException("value must be within 64..4096 for two cascades");
						}
					}
					if (csmCascadeCount==1) {
						if (value<64 || value>8192) {
							throw new ArgumentOutOfRangeException("value must be within 64..8192 for one cascade");
						}
					}
					if (!MathUtil.IsPowerOfTwo(value)) {
						throw new ArgumentException("value must be power of 2");
					}
					csmCascadeSize	=	value;
				}
			}
		}

		/// <summary>
		/// Cascaded shadow map size.
		/// </summary>
		[Config]
		public int CSMCascadeCount {
			get { return csmCascadeCount; }
			set {
				lock (csmLock) {
					if (value<1 || value>4) {
						throw new ArgumentOutOfRangeException("value must be within 1..4");
					}
					if ((value==3 || value==4) && csmCascadeSize > 2048) {
						csmCascadeSize = 2048;
					}
					if (value==2 && csmCascadeSize > 4096) {
						csmCascadeSize = 4096;
					}
					if (value==1 && csmCascadeSize > 8192) {
						csmCascadeSize = 8192;
					}
					csmCascadeCount	=	value;
				}
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
		/// Split magnification factor.
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
			CSMCascadeSize		=	1024;
			CSMCascadeCount		=	4;
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

