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
	/// Shadow render context
	/// </summary>
	internal class ShadowRenderContext {
		public Matrix				ShadowView;
		public Matrix				ShadowProjection;
		public DepthStencilSurface	DepthBuffer;
		public RenderTargetSurface	ColorBuffer;
		public Viewport				ShadowViewport;
		public float				DepthBias;
		public float				SlopeBias;
		public float				FarDistance;
	}
}
