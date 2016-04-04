using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	struct SpriteVertex {

		[Vertex("POSITION")]	public Vector3	Position;
		[Vertex("TEXCOORD")]	public Vector2	TexCoord;
		[Vertex("COLOR")]		public Color	Color;
		[Vertex("FRAME")]		public int		FrameIndex;

		public SpriteVertex ( Vector3 p, Color c, Vector2 tc, int frameIndex = 0 ) {
			Position	=	p;
			Color		=	c;
			TexCoord	=	tc;
			FrameIndex	=	frameIndex;
		}

		public SpriteVertex ( float x, float y, float z, Color c, float u, float v, int frameIndex = 0  )
		{
			Position	=	new Vector3( x, y, z );
			Color		=	c;
			TexCoord	=	new Vector2( u, v );
			FrameIndex	=	frameIndex;
		}
	}
}
