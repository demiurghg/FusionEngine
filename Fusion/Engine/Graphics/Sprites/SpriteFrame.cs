using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	struct SpriteFrame {

		public RectangleF	ClipRectangle;
		public Color4		FrameColor;

		public SpriteFrame ( RectangleF clipRect, Color4 frameColor ) 
		{
			ClipRectangle	=	clipRect;
			FrameColor		=	frameColor;
		}
	}
}
