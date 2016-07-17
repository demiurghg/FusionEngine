using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	public static class VTConfig {

		public const int PageSize			=	128;
		public const int VirtualPageCount	=	128;
		public const int TextureSize		=	PageSize * VirtualPageCount;
		public const int MipCount			=	6;
		public const int MaxMipLevel		=	MipCount - 1;

		public const int FallbackSize		=	TextureSize >> MaxMipLevel;

		public const int PhysicalPageCount		=	8;
		public const int TotalPhysicalPageCount	=	PhysicalPageCount * PhysicalPageCount;

	}
}
