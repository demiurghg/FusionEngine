using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Represents 
	/// </summary>
	internal class HdrFrame {
		public RenderTarget2D	HdrBuffer		;	
		public RenderTarget2D	LightAccumulator;
		public DepthStencil2D	DepthBuffer		;	
		public RenderTarget2D	DiffuseBuffer	;
		public RenderTarget2D	SpecularBuffer	;
		public RenderTarget2D	NormalMapBuffer	;
		public RenderTarget2D	ScatteringBuffer;
	}
}
