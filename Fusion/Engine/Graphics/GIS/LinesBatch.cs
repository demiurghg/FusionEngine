﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS
{
	public class LinesGisBatch : GIS.GisBatch
	{
		Ubershader shader;

		[Flags]
		public enum LineFlags : int
		{
			Poly = 1 << 0,
			Arc = 1 << 1,
			Adjacency = 1 << 2,
			TexCoordByDistance = 1 << 3,
		}

		public LineFlags Flags;


		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public GIS.GeoPoint[] PointsCpu { get; protected set; }


		public LinesGisBatch(GameEngine engine) : base(engine)
		{
			
		}


		void SwapBuffers()
		{

		}
	}
}
