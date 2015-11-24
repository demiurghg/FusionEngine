using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class PolyGisLayer : Gis.GisLayer
	{
		protected Ubershader	shader;
		protected StateFactory	factory;
		protected StateFactory	factoryXray;

		[Flags]
		public enum PolyFlags : int
		{
			VERTEX_SHADER	= 1 << 0,
			PIXEL_SHADER	= 1 << 1,
			DRAW_HEAT		= 1 << 2,
			DRAW_TEXTURED	= 1 << 3,
			SHOW_FRAMES		= 1 << 4,
			COMPUTE_SHADER	= 1 << 5,
			BLUR_HORIZONTAL = 1 << 6,
			BLUR_VERTICAL	= 1 << 7,
			DRAW_COLORED	= 1 << 8,
			XRAY			= 1 << 9,
		}

		public int Flags;

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;
		public Texture2D Palette;

		public Vector2	PatternSize;
		public float	ArrowsScale;

		protected VertexBuffer	firstBuffer;
		protected VertexBuffer	secondBuffer;
		protected VertexBuffer	currentBuffer;
		protected IndexBuffer	indexBuffer;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }

		protected PolyGisLayer(GameEngine engine) : base(engine) { }


		PolyGisLayer(GameEngine engine, Gis.GeoPoint[] points, int[] indeces, bool isDynamic) : base(engine)
		{
			Console.WriteLine(points.Length + " _ " + indeces.Length);
			Initialize(points, indeces, isDynamic);

			Flags = (int)(PolyFlags.VERTEX_SHADER | PolyFlags.PIXEL_SHADER | PolyFlags.DRAW_COLORED);
		}


		protected void Initialize(Gis.GeoPoint[] points, int[] indeces, bool isDynamic)
		{
			shader		= GameEngine.Content.Load<Ubershader>("globe.Poly.hlsl");
			factory		= new StateFactory(shader, typeof(PolyFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
			factoryXray = new StateFactory(shader, typeof(PolyFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.Additive, RasterizerState.CullCW, DepthStencilState.None);

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer = new VertexBuffer(GameEngine.GraphicsDevice, typeof(Gis.GeoPoint), points.Length, vbOptions);
			firstBuffer.SetData(points);
			currentBuffer = firstBuffer;

			indexBuffer = new IndexBuffer(GameEngine.Instance.GraphicsDevice, indeces.Length);
			indexBuffer.SetData(indeces);

			PointsCpu = points;
		}
		

		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			if (((PolyFlags) Flags).HasFlag(PolyFlags.XRAY)) {
				GameEngine.GraphicsDevice.PipelineState = factoryXray[Flags];
			}
			else {
				GameEngine.GraphicsDevice.PipelineState = factory[Flags];
			}

			GameEngine.GraphicsDevice.VertexShaderConstants[0] = constBuffer;

			GameEngine.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			GameEngine.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;

			GameEngine.GraphicsDevice.SetupVertexInput(currentBuffer, indexBuffer);
			GameEngine.GraphicsDevice.DrawIndexed(indexBuffer.Capacity, 0, 0);

			//game.GraphicsDevice.ResetStates();
		}


		public static PolyGisLayer GenerateRegularGrid(GameEngine engine, double left, double right, double top, double bottom, int density, int dimX, int dimY, MapProjection projection)
		{
			int[] indexes;
			Gis.GeoPoint[] vertices;

			CalculateVertices(out vertices, out indexes, density, left, right, top, bottom, projection);

			//var vb = new VertexBuffer(GameEngine.Instance.GraphicsDevice, typeof(Gis.GeoPoint), vertices.Length);
			//var ib = new IndexBuffer(GameEngine.Instance.GraphicsDevice, indexes.Length);
			//ib.SetData(indexes);
			//vb.SetData(vertices, 0, vertices.Length);

			return new PolyGisLayer(engine, vertices, indexes, false);
		}


		public static PolyGisLayer CreateFromContour()
		{
			return null;
		}



		public static PolyGisLayer CreateFromUtmFbxModel(GameEngine engine, string fileName)
		{
			var scene = engine.Content.Load<Scene>(fileName);

			var s = fileName.Split('_');
			double easting	= double.Parse(s[1]);
			double northing = double.Parse(s[2]);
			string region	= s[3];
			
			var transforms = new Matrix[scene.Nodes.Count];
			scene.ComputeAbsoluteTransforms(transforms);

			List<Gis.GeoPoint>	points = new List<Gis.GeoPoint>();
			List<int>			indeces = new List<int>();

			for (int i = 0; i < scene.Nodes.Count; i++) {

				var meshIndex = scene.Nodes[i].MeshIndex;

				if (meshIndex < 0) {
					continue;
				}

				int vertexOffset = points.Count;

				var world = transforms[i];
				
				foreach (var vert in scene.Meshes[meshIndex].Vertices) {
					var pos = vert.Position;

					var worldPos	= Vector3.TransformCoordinate(pos, world);
					var worldNorm	= Vector3.TransformNormal(vert.Normal, world);


					double lon, lat;
					Gis.UtmToLatLon(easting + worldPos.X, northing - worldPos.Z, region, out lon, out lat);

					DVector3 norm = new DVector3(worldNorm.X, worldNorm.Z, worldNorm.Y);
					norm.Normalize();

					norm = DVector3.TransformNormal(norm, DMatrix.RotationYawPitchRoll(DMathUtil.DegreesToRadians(lon), DMathUtil.DegreesToRadians(lat), 0));
					norm.Normalize();

					norm.Y = -norm.Y;

					var point = new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(lon) + 0.0000068,
						Lat		= DMathUtil.DegreesToRadians(lat) + 0.0000113,
						Color	= vert.Color0,
						Tex0	= new Vector4(norm.ToVector3(), 0),
						Tex1	= new Vector4(0,0,0, worldPos.Y/1000.0f)
					};
					point.Color.Alpha = 0.5f;
					points.Add(point);
				}

				var inds = scene.Meshes[meshIndex].GetIndices();

				foreach (var ind in inds) {
					indeces.Add(vertexOffset + ind);
				}

			}

			return new PolyGisLayer(engine, points.ToArray(), indeces.ToArray(), false);
		}



		void SwapBuffers()
		{

		}


		static protected void CalculateVertices(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double leftLon, double rightLon, double topLat, double bottomLat, MapProjection projection)
		{
			int RowsCount		= density + 2;
			int ColumnsCount	= RowsCount;

			var ms		= projection;
			var verts	= new List<Gis.GeoPoint>();

			var leftTop		= ms.WorldToTilePos(leftLon, topLat, 0);
			var rightBottom = ms.WorldToTilePos(rightLon, bottomLat, 0);

			double left		= leftTop.X;
			double right	= rightBottom.X;
			double top		= leftTop.Y;
			double bottom	= rightBottom.Y;

			float	step	= 1.0f / (density + 1);
			double	dStep	= 1.0 / (double)(density + 1);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {
					double xx = left * (1.0 - dStep * col) + right * dStep * col;
					double yy = top * (1.0 - dStep * row) + bottom * dStep * row;

					var sc = ms.TileToWorldPos(xx, yy, 0);

					var lon = sc.X * Math.PI / 180.0;
					var lat = sc.Y * Math.PI / 180.0;

					verts.Add(new Gis.GeoPoint {
						Tex0	= new Vector4(step * col, step * row, 0, 0),
						Lon		= lon,
						Lat		= lat
					});
				}

			}


			var tindexes = new List<int>();

			for (int row = 0; row < RowsCount - 1; row++)
			{
				for (int col = 0; col < ColumnsCount - 1; col++)
				{
					tindexes.Add(col + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + row * ColumnsCount);

					tindexes.Add(col + 1 + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
				}
			}

			vertices = verts.ToArray();
			indeces = tindexes.ToArray();
		}
	}
}
