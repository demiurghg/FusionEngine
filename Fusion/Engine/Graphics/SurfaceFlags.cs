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
using Fusion.Core.Development;


namespace Fusion.Engine.Graphics {

	public enum SurfaceFlags {
		GBUFFER					=	1 << 0,
		SHADOW					=	1 << 1,

		RIGID					=	1 << 4,
		SKINNED					=	1 << 5,
	}
}
