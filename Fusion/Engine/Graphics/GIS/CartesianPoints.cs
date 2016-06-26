using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class CartesianPoints : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;

		[Flags]
		public enum PointFlags : int
		{
			DOTS_SCREENSPACE	= 1 << 0,
			TEST				= 1 << 1,
		}

		public int Flags;

		public class SelectedItem : Gis.SelectedItem
		{
			public int PointIndex;
		}


		 [StructLayout(LayoutKind.Explicit)]
		 public struct DotsData
		 {
			[FieldOffset(0)]	public Matrix	View;
			[FieldOffset(64)]	public Matrix	Proj;
			[FieldOffset(128)]	public Vector4	SizeMult;
			[FieldOffset(144)]	public Vector4	Dummy;
		 }
		DotsData dotsData;


		ConstantBuffer DotsBuffer;


		public float		SizeMultiplier;
		public int			PointsCount { get { return PointsCpu.Length; } }
		public int			PointsDrawOffset;
		public int			PointsCountToDraw;

		VertexBuffer currentBuffer;

		public Gis.CartPoint[]	PointsCpu	{ get; protected set; }
		public float			AlphaFactor { set; get; }


		public bool IsDynamic { get; protected set; }



		public CartesianPoints(Game engine, int maxPointsCount, bool isDynamic = false) : base(engine)
		{
			DotsBuffer	= new ConstantBuffer(engine.GraphicsDevice, typeof(DotsData));

			PointsCountToDraw	= maxPointsCount;
			PointsDrawOffset	= 0;

			SizeMultiplier	= 1;
			IsDynamic		= isDynamic;

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			currentBuffer	= new VertexBuffer(engine.GraphicsDevice, typeof(Gis.CartPoint), maxPointsCount, vbOptions);
			PointsCpu		= new Gis.CartPoint[maxPointsCount];

			Flags = (int)(PointFlags.DOTS_SCREENSPACE | PointFlags.TEST);

			shader	= Game.Content.Load<Ubershader>("globe.CartesianPoint.hlsl");
			factory = shader.CreateFactory( typeof(PointFlags), Primitive.PointList, new VertexInputElement[] {
				new VertexInputElement("TEXCOORD", 0, Drivers.Graphics.VertexFormat.UInt2, 0, 0),
 				new VertexInputElement("TEXCOORD", 1, Drivers.Graphics.VertexFormat.UInt2, 0, 8),
				new VertexInputElement("TEXCOORD", 2, Drivers.Graphics.VertexFormat.UInt2, 0, 16),
				new VertexInputElement("TEXCOORD", 3, Drivers.Graphics.VertexFormat.Vector4, 0, 24),
				new VertexInputElement("COLOR", 0, Drivers.Graphics.VertexFormat.Color, 0, 40),
			}, BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);

			AlphaFactor = 1.0f;
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public override void Update(GameTime gameTime)
		{
			dotsData.View				= Game.RenderSystem.Gis.Camera.ViewMatrixFloat;
			dotsData.Proj				= Game.RenderSystem.Gis.Camera.ProjMatrixFloat;
			dotsData.SizeMult			= new Vector4(SizeMultiplier, 0.0f, 0.0f, AlphaFactor);
			dotsData.Dummy				= new Vector4();

			DotsBuffer.SetData(dotsData);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			Update(gameTime);

			var dev = Game.GraphicsDevice;

			dev.PipelineState = factory[Flags];

			dev.VertexShaderConstants[0]	= constBuffer;
			dev.GeometryShaderConstants[0]	= constBuffer;

			dev.VertexShaderConstants[1]	= DotsBuffer;
			dev.GeometryShaderConstants[1]	= DotsBuffer;


			dev.SetupVertexInput(currentBuffer, null);
			dev.Draw(PointsCountToDraw, PointsDrawOffset);
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			DVector3[] rayHitPoints;
			var ret = new List<Gis.SelectedItem>();

			if (!GeoHelper.LineIntersection(nearPoint, farPoint, GeoHelper.EarthRadius, out rayHitPoints)) return ret;

			var rayLonLatRad			= GeoHelper.CartesianToSpherical(rayHitPoints[0]);
			//var OneGradusLengthKmInv	= 1.0 / (Math.Cos(rayLonLatRad.Y)*GeoHelper.EarthOneDegreeLengthOnEquatorMeters/1000.0);

			//for (int i = 0; i < PointsCountToDraw; i++) {
			//	int ind		= PointsDrawOffset + i;
			//	var point	= PointsCpu[ind];
			//
			//	var size		= point.Tex0.Z * 0.5;
			//	var pointLonLat = new DVector2(point.Lon, point.Lat);
			//
			//
			//	var dist = GeoHelper.DistanceBetweenTwoPoints(pointLonLat, rayLonLatRad);
			//
			//	if (dist <= size) {
			//		ret.Add(new PointsGisLayer.SelectedItem {
			//			Distance	= dist,
			//			PointIndex	= ind
			//		});
			//	}
			//}

			return ret;
		}


		public static void MergeAndSaveToCache(string filePath, List<CartesianPoints> list)
		{
			if (list.Count == 0) return;

			using (var stream = File.OpenWrite(filePath)) {
				var w = new BinaryWriter(stream);

				foreach (var ps in list) {
					if (ps == null || ps.PointsCount == 0) continue;

					foreach (var point in ps.PointsCpu) {
						w.Write(point.X);
						w.Write(point.Y);
						w.Write(point.Z);
						w.Write(point.Tex0.X);
						w.Write(point.Tex0.Y);
						w.Write(point.Tex0.Z);
						w.Write(point.Tex0.W);
						//w.Write(point.Color.R);
						//w.Write(point.Color.G);
						//w.Write(point.Color.B);
						//w.Write(point.Color.A);
					}
				}

			}
		}


		public static CartesianPoints LoadFromCache(string filePath)
		{
			if (!File.Exists(filePath)) return null;

			List<Gis.CartPoint> points = new List<Gis.CartPoint>();

			using (var stream = File.OpenRead(filePath)) {

				var count	= stream.Length/44;
				if (count <= 0) return null;

				var r		= new BinaryReader(stream);

				for (int i = 0; i < count; i++) {
					points.Add(new Gis.CartPoint {
						X = r.ReadDouble(),
						Y = r.ReadDouble(),
						Z = r.ReadDouble(),
						Tex0	= new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
						Color	=  new Color(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte())
					});
				}
			}

			CartesianPoints p = new CartesianPoints(Game.Instance, points.Count, false);

			for (int i = 0; i < points.Count; i++)
				p.PointsCpu[i] = points[i];

			//p.UpdatePointsBuffer();
			return p;
		}
	}
}
