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

namespace Fusion.Engine.Graphics.GIS
{
	public class PolyGisLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;

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
		}

		public int Flags;

		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;
		public Texture2D Palette;

		public Vector2	PatternSize;
		public float	ArrowsScale;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;
		IndexBuffer	 indexBuffer;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }


#region Heat map stuff
		public float[] Data;

		public float InterpFactor = 0;

		public Texture2D HeatTexture { get; protected set; }
		private RenderTarget2D Temp;
		private RenderTarget2D Final;
		private RenderTarget2D Prev;
		private RenderTarget2D FirstFinal;
		private RenderTarget2D SecondFinal;

		public int MapDimX { get; protected set; }
		public int MapDimY { get; protected set; }

		public double Left		{ get; protected set; }
		public double Right		{ get; protected set; }
		public double Top		{ get; protected set; }
		public double Bottom	{ get; protected set; }

		public int				GridDensity;
		public MapProjection	Projection;

		public float MaxHeatMapLevel;
		public float HeatMapTransparency;

		struct HeatMapConstData {
			public Vector4 Data;
		}
		HeatMapConstData	heatConstData;
		ConstantBuffer		cb;

		StateFactory blurFactory;

		protected static Texture2D[] HeatMapPalettes;
#endregion


		void Initialize(Gis.GeoPoint[] points, int[] indeces, bool isDynamic)
		{
			shader	= GameEngine.Content.Load<Ubershader>("globe.Poly.hlsl");
			factory = new StateFactory(shader, typeof(PolyFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);


			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer = new VertexBuffer(GameEngine.GraphicsDevice, typeof(Gis.GeoPoint), points.Length, vbOptions);
			firstBuffer.SetData(points);
			currentBuffer = firstBuffer;

			indexBuffer = new IndexBuffer(GameEngine.Instance.GraphicsDevice, indeces.Length);
			indexBuffer.SetData(indeces);

			PointsCpu = points;
		}

		public PolyGisLayer(GameEngine engine, Gis.GeoPoint[] points, int[] indeces, int mapDimX, int mapDimY, bool isDynamic = false) : base(engine)
		{
			Initialize(points, indeces, isDynamic);

			MapDimX = mapDimX;
			MapDimY = mapDimY;

			HeatTexture = new Texture2D(GameEngine.GraphicsDevice, MapDimX, MapDimY, ColorFormat.R32F, false);
			Temp		= new RenderTarget2D(GameEngine.GraphicsDevice, ColorFormat.R32F, MapDimX, MapDimY, true);
			FirstFinal	= new RenderTarget2D(GameEngine.GraphicsDevice, ColorFormat.R32F, MapDimX, MapDimY, true);
			SecondFinal = new RenderTarget2D(GameEngine.GraphicsDevice, ColorFormat.R32F, MapDimX, MapDimY, true);

			Final	= SecondFinal;
			Prev	= FirstFinal;

			Data = new float[MapDimX * MapDimY];

			HeatMapPalettes		= new Texture2D[1];
			HeatMapPalettes[0]	= GameEngine.Content.Load<Texture2D>("palette");

			cb				= new ConstantBuffer(GameEngine.GraphicsDevice, typeof(HeatMapConstData));
			heatConstData	= new HeatMapConstData();

			Flags = (int)(PolyFlags.PIXEL_SHADER | PolyFlags.VERTEX_SHADER | PolyFlags.DRAW_HEAT);

			blurFactory = new StateFactory(shader, typeof(PolyFlags), Primitive.TriangleList,
					null,
					BlendState.AlphaBlend,
					RasterizerState.CullNone,
					DepthStencilState.None);
		}


		public virtual void UpdateHeatMap()
		{
			HeatTexture.SetData(Data);

			var game = GameEngine;

			if (Final == FirstFinal) {
				Final = SecondFinal;
				Prev = FirstFinal;
			} else {
				Final = FirstFinal;
				Prev = SecondFinal;
			}

			game.GraphicsDevice.PipelineState = blurFactory[(int)(PolyFlags.COMPUTE_SHADER | PolyFlags.BLUR_VERTICAL)];

			game.GraphicsDevice.ComputeShaderResources[0] = HeatTexture;
			game.GraphicsDevice.SetCSRWTexture(0, Temp.Surface);

			game.GraphicsDevice.Dispatch(1, MapDimY, 1);

			game.GraphicsDevice.SetCSRWTexture(0, null);

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////

			game.GraphicsDevice.PipelineState = blurFactory[(int)(PolyFlags.COMPUTE_SHADER | PolyFlags.BLUR_HORIZONTAL)];

			game.GraphicsDevice.ComputeShaderResources[0] = Temp;
			game.GraphicsDevice.SetCSRWTexture(0, Final.Surface);

			game.GraphicsDevice.Dispatch(1, MapDimY, 1);

			game.GraphicsDevice.SetCSRWTexture(0, null);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{

			heatConstData.Data = new Vector4(MaxHeatMapLevel, HeatMapTransparency, MapDimX, InterpFactor);
			cb.SetData(heatConstData);

			GameEngine.GraphicsDevice.PipelineState = factory[
			(int)(
				PolyFlags.PIXEL_SHADER |
				PolyFlags.VERTEX_SHADER |
				PolyFlags.DRAW_HEAT)
			];

			//if(((PolyFlags)Flags).HasFlag(PolyFlags.DRAW_HEAT))

			GameEngine.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			GameEngine.GraphicsDevice.PixelShaderConstants[0]	= constBuffer;
			GameEngine.GraphicsDevice.PixelShaderConstants[1]	= cb;
			
			GameEngine.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			GameEngine.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;
			
			GameEngine.GraphicsDevice.PixelShaderResources[0] = HeatMapPalettes[0];
			GameEngine.GraphicsDevice.PixelShaderResources[1] = Final;
			GameEngine.GraphicsDevice.PixelShaderResources[2] = Prev;
			
			GameEngine.GraphicsDevice.SetupVertexInput(currentBuffer, indexBuffer);
			GameEngine.GraphicsDevice.DrawIndexed(indexBuffer.Capacity, 0, 0);

			//game.GraphicsDevice.ResetStates();
		}


		public void SetHeatMapCoordinates(double left, double right, double top, double bottom)
		{
			var hLen = right - left;
			bottom = top - hLen * 0.5;
		
		
			if (Left == left && Right == right && Top == top && Bottom == bottom) return;
		
			Left	= left;
			Right	= right;
			Top		= top;
			Bottom	= bottom;
		
			if (currentBuffer != null) {
				currentBuffer.Dispose();
			}
			if (indexBuffer != null) {
				indexBuffer.Dispose();
			}
		
			ClearData();

			int[]			indeces;
			Gis.GeoPoint[]	points;
			CalculateVertices(out points, out indeces, GridDensity, left, right, top, bottom, Projection);

			PointsCpu = points;
			UpdatePointsBuffer();

			if(indexBuffer != null)
				indexBuffer.SetData(indeces);

		}


		public void AddValue(double lon, double lat, float val)
		{
			var lonFactor = (lon - Left) / (Right - Left);
			var latFactor = (lat - Bottom) / (Top - Bottom);

			int x = (int)(lonFactor * MapDimX);
			int y = (int)(latFactor * MapDimY);

			if (x < 0 || x >= MapDimX) return;
			if (y < 0 || y >= MapDimY) return;

			var ind = x + y * MapDimX;

			if (ind < Data.Length && ind >= 0) {
				Data[ind] += val;
			}
		}


		public virtual void ClearData()
		{
			Array.Clear(Data, 0, Data.Length);
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public static PolyGisLayer GenerateRegularGrid(double left, double right, double top, double bottom, int density, int dimX, int dimY, MapProjection projection)
		{
			int[] indexes;
			Gis.GeoPoint[] vertices;

			CalculateVertices(out vertices, out indexes, density, left, right, top, bottom, projection);

			var vb = new VertexBuffer(GameEngine.Instance.GraphicsDevice, typeof(Gis.GeoPoint), vertices.Length);
			var ib = new IndexBuffer(GameEngine.Instance.GraphicsDevice, indexes.Length);
			ib.SetData(indexes);
			vb.SetData(vertices, 0, vertices.Length);

			return new PolyGisLayer(GameEngine.Instance, vertices, indexes, dimX, dimY, false) {
				Left		= left,
				Right		= right,
				Top			= top, 
				Bottom		= bottom,
				GridDensity = density,
				Projection	= projection
			};
		}

		public static PolyGisLayer CreateFromContour()
		{
			return null;
		}

		void SwapBuffers()
		{

		}


		static void CalculateVertices(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double leftLon, double rightLon, double topLat, double bottomLat, MapProjection projection)
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
