using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.GIS
{
	public class PolyBatch : GIS.Batch
	{
		Ubershader shader;

		[Flags]
		public enum PolyFlags : int
		{
			Palette = 1 << 0,
			Pattern = 1 << 1,
			Arrows = 1 << 2,
		}

		public PolyFlags Flags;

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;
		public Texture2D Palette;

		public Vector2	PatternSize;
		public float	ArrowsScale;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public GIS.GeoPoint[] PointsCpu { get; protected set; }


		public PolyBatch(GameEngine engine) : base(engine)
		{
			
		}


		public void GenerateRegularGrid() { }

		void SwapBuffers()
		{

		}
	}
}
