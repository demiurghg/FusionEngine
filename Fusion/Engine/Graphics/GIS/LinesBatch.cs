using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class LinesGisLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;
		StateFactory	thinFactory;

		[Flags]
		public enum LineFlags : int
		{
			DRAW_LINES				= 1 << 0,
			DRAW_SEGMENTED_LINES	= 1 << 1,
			ARC_LINE				= 1 << 2,
			ADD_CAPS				= 1 << 3,
			FADING_LINE				= 1 << 4,
			THIN_LINE				= 1 << 5,
		}

		public int Flags;


		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }


		public override void Dispose()
		{
			if (firstBuffer != null)	firstBuffer.Dispose();
			if (secondBuffer != null)	secondBuffer.Dispose();

			if(shader != null) shader.Dispose();
			if(factory!= null) factory.Dispose();
		}


		public LinesGisLayer(GameEngine engine, int linesPointsCount, bool isDynamic = false) : base(engine)
		{
			shader	= GameEngine.Content.Load<Ubershader>("globe.Line.hlsl");
			factory		= new StateFactory(shader, typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);
			thinFactory = new StateFactory(shader, typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.Readonly);


			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer		= new VertexBuffer(engine.GraphicsDevice, typeof(Gis.GeoPoint), linesPointsCount, vbOptions);
			currentBuffer	= firstBuffer;

			PointsCpu = new Gis.GeoPoint[linesPointsCount];

			Flags = (int)(LineFlags.ARC_LINE);
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			var dev = GameEngine.GraphicsDevice;

			if (((LineFlags) Flags).HasFlag(LineFlags.THIN_LINE)) {
				dev.PipelineState = thinFactory[Flags];
			}
			else {
				dev.PipelineState = factory[Flags];
			}

			dev.GeometryShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderConstants[0]		= constBuffer;

			dev.PixelShaderResources[0] = Texture;
			dev.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap;


			dev.SetupVertexInput(currentBuffer, null);
			dev.Draw(currentBuffer.Capacity, 0);
		}


		public static LinesGisLayer GenerateGrid(GameEngine gameEngine, DVector2 leftTop, DVector2 rightBottom, int dimX, int dimY, Color color, MapProjection projection, bool keepQuad = false)
		{
			var lt = projection.WorldToTilePos(leftTop.X,		leftTop.Y, 0);
			var rb = projection.WorldToTilePos(rightBottom.X,	rightBottom.Y, 0);

			if (keepQuad) {
				rb.Y = lt.Y + (rb.X - lt.X);
			}

			double stepX = Math.Abs(rb.X - lt.X) / (dimX - 1);
			double stepY = Math.Abs(rb.Y - lt.Y) / (dimY - 1);


			List<Gis.GeoPoint> points = new List<Gis.GeoPoint>();
			
			// Too lazy
			for (int row = 1; row < dimY-1; row++) {
				for (int col = 0; col < dimX-1; col++) {
					var coords0 = projection.TileToWorldPos(lt.X + stepX * col,		lt.Y + stepY * row, 0);
					var coords1 = projection.TileToWorldPos(lt.X + stepX * (col+1), lt.Y + stepY * row, 0);

					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords0.X),
						Lat		= DMathUtil.DegreesToRadians(coords0.Y),
						Color	= color
					});
					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords1.X),
						Lat		= DMathUtil.DegreesToRadians(coords1.Y),
						Color	= color
					});
				} 
			}
			for (int col = 1; col < dimX-1; col++) {
				for (int row = 0; row < dimY-1; row++) {
					var coords0 = projection.TileToWorldPos(lt.X + stepX * col,	lt.Y + stepY * row, 0);
					var coords1 = projection.TileToWorldPos(lt.X + stepX * col, lt.Y + stepY * (row+1), 0);

					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords0.X),
						Lat		= DMathUtil.DegreesToRadians(coords0.Y),
						Color	= color
					});
					points.Add(new Gis.GeoPoint {
						Lon		= DMathUtil.DegreesToRadians(coords1.X),
						Lat		= DMathUtil.DegreesToRadians(coords1.Y),
						Color	= color
					});
				} 
			}

			var linesLayer = new LinesGisLayer(gameEngine, points.Count);
			Array.Copy(points.ToArray(), linesLayer.PointsCpu, points.Count);
			linesLayer.UpdatePointsBuffer();
			linesLayer.Flags = (int)(LineFlags.THIN_LINE);

			return linesLayer;
		}



		public static LinesGisLayer GenerateDistanceGrid(GameEngine gameEngine, DVector2 lonLatLeftBottomCorner, double step, int xStepsCount, int yStepsCount, Color color)
		{

			List<Gis.GeoPoint> points = new List<Gis.GeoPoint>();

			// Too lazy
			var yPoint = lonLatLeftBottomCorner;
			
			//for (int row = 0; row < yStepsCount; row++) {
			//
			//	yPoint = GeoHelper.RhumbDestinationPoint(yPoint, 0, step);
			//
			//	for (int col = 0; col < xStepsCount; col++)
			//	{
			//		var coords0 = GeoHelper.RhumbDestinationPoint(yPoint, 90, step * col);
			//		//var coords1 = GeoHelper.RhumbDestinationPoint(coords0, 90, step);
			//
			//		points.Add(new Gis.GeoPoint {
			//			Lon = DMathUtil.DegreesToRadians(coords0.X),
			//			Lat = DMathUtil.DegreesToRadians(coords0.Y),
			//			Color = color
			//		});
			//		//points.Add(new Gis.GeoPoint {
			//		//	Lon = DMathUtil.DegreesToRadians(coords1.X),
			//		//	Lat = DMathUtil.DegreesToRadians(coords1.Y),
			//		//	Color = color
			//		//});
			//	}
			//}

			for (int col = 0; col < xStepsCount; col++)
			{
				var xPoint = GeoHelper.RhumbDestinationPoint(lonLatLeftBottomCorner, 90, step * col);
				
				for (int row = 0; row < yStepsCount; row++)
				{
					var coords0 = GeoHelper.RhumbDestinationPoint(xPoint, 0, step * row);
					//var coords1 = GeoHelper.RhumbDestinationPoint(xPoint, 0, step * (row + 1));
			
					points.Add(new Gis.GeoPoint
					{
						Lon = DMathUtil.DegreesToRadians(coords0.X),
						Lat = DMathUtil.DegreesToRadians(coords0.Y),
						Color = color
					});
					//points.Add(new Gis.GeoPoint
					//{
					//	Lon = DMathUtil.DegreesToRadians(coords1.X),
					//	Lat = DMathUtil.DegreesToRadians(coords1.Y),
					//	Color = color
					//});
				}
			}


			var indeces = new List<int>();

			for (int col = 0; col < xStepsCount-1; col++) {
				for (int row = 0; row < yStepsCount; row++) {
					indeces.Add(row + (col+1) * yStepsCount);
					indeces.Add(row + col*yStepsCount);
				}
			}

			for (int row = 0; row < yStepsCount-1; row++)
			{
				for (int col = 0; col < xStepsCount; col++)
				{
					indeces.Add(col * yStepsCount + row);
					indeces.Add((col) * yStepsCount + row + 1);
				}
			}

			var newPoints = new List<Gis.GeoPoint>();
			foreach (var ind in indeces) {
				newPoints.Add(points[ind]);
			}

			var linesLayer = new LinesGisLayer(gameEngine, newPoints.Count);
			Array.Copy(newPoints.ToArray(), linesLayer.PointsCpu, newPoints.Count);
			linesLayer.UpdatePointsBuffer();
			linesLayer.Flags = (int)(LineFlags.THIN_LINE);

			return linesLayer;
		}

	}
}
