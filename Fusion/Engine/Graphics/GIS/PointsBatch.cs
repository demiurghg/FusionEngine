using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS
{
	public class PointsBatch : GIS.Batch
	{
		Ubershader shader;

		[Flags]
		public enum PointFlags : int
		{
			Rotation = 1 << 0,
			Billboard = 1 << 1,
			UseColorArr = 1 << 2,
		}

		public PointFlags Flags;

		//public float	Height;
		//public float	AtlasRow;
		//public float	AtlasCol;
		//public float	Rotation;
		//public float	Size;


		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D TextureAtlas;
		public Vector2 ImageSizeInAtlas;
		public float SizeMultiplier;
		public int PointsCount { get { return PointsCpu.Length; } }

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public GIS.GeoPoint[] PointsCpu { get; protected set; }



		public PointsBatch(GameEngine engine) : base(engine)
		{
			
		}


		void SwapBuffers()
		{

		}
	}
}
