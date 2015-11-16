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

	/// <summary>
	/// Interface for shadow casters.
	/// </summary>
	internal interface IShadowCaster {
		void RenderShadowMapCascade ( ShadowRenderContext shadowRenderCtxt );
		void RenderShadowMapSpot	( ShadowRenderContext shadowRenderCtxt );
	}
}
