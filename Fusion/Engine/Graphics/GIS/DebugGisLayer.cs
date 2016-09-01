using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class DebugGisLayer : Gis.GisLayer
	{
		List<Gis.CartPoint> boxes;
		List<Gis.CartPoint> lines;

		public class SelectedItem : Gis.SelectedItem { }

		Ubershader		shader;
		StateFactory	factory;

		VertexBuffer	buf;
		bool isDirty = false;

		[Flags]
		public enum DebugFlags : int
		{
			DRAW_LINES = 1 << 0,
		}

		public DebugGisLayer(Game game) : base(game)
		{
			shader	= game.Content.Load<Ubershader>("globe.Debug.hlsl");
			factory = shader.CreateFactory(typeof(DebugFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.CartPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.None);

			buf = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.CartPoint), 10000, VertexBufferOptions.Dynamic);

			boxes = new List<Gis.CartPoint>();
			lines = new List<Gis.CartPoint>();
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			Game.GraphicsDevice.PipelineState				= factory[(int)(DebugFlags.DRAW_LINES)];

			if (isDirty) {
				isDirty = false;

				buf.SetData(lines.ToArray(), 0, Math.Min(lines.Count, buf.Capacity));
			}

			if (lines.Any()) {
				Game.GraphicsDevice.SetupVertexInput(buf, null);
				Game.GraphicsDevice.Draw(lines.Count, 0);
			}
		}



		public void Clear()
		{
			lines.Clear();
		}


		public void DrawBoundingBox(BoundingBox box)
		{
			DrawLine(new DVector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z), new DVector3(box.Maximum.X, box.Minimum.Y, box.Minimum.Z), Color.Green);
			DrawLine(new DVector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z), new DVector3(box.Minimum.X, box.Maximum.Y, box.Minimum.Z), Color.Green);
			DrawLine(new DVector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z), new DVector3(box.Minimum.X, box.Minimum.Y, box.Maximum.Z), Color.Green);

			DrawLine(new DVector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z), new DVector3(box.Minimum.X, box.Maximum.Y, box.Maximum.Z), Color.Red);
			DrawLine(new DVector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z), new DVector3(box.Maximum.X, box.Minimum.Y, box.Maximum.Z), Color.Red);
			DrawLine(new DVector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z), new DVector3(box.Maximum.X, box.Maximum.Y, box.Minimum.Z), Color.Red);
		}


		public void DrawBoundingBox(BoundingBox box, DMatrix transform)
		{
			var corners			= box.GetCorners();
			var worldCorners	= corners.Select(x => DVector3.TransformCoordinate(new DVector3(x.X, x.Y, x.Z), transform)).ToArray();

			//foreach (var corner in worldCorners)
			//{
			//	DrawPoint(corner, 0.1f);
			//}
			

			DrawLine(worldCorners[0], worldCorners[1], Color.Green);
			DrawLine(worldCorners[0], worldCorners[4], Color.Green);
			DrawLine(worldCorners[0], worldCorners[3], Color.Green);

			DrawLine(worldCorners[7], worldCorners[6], Color.Red);
			DrawLine(worldCorners[7], worldCorners[3], Color.Red);
			DrawLine(worldCorners[7], worldCorners[4], Color.Red);

			DrawLine(worldCorners[5], worldCorners[1], Color.Yellow);
			DrawLine(worldCorners[5], worldCorners[4], Color.Yellow);
			DrawLine(worldCorners[5], worldCorners[6], Color.Yellow);

			DrawLine(worldCorners[2], worldCorners[1], Color.WhiteSmoke);
			DrawLine(worldCorners[2], worldCorners[3], Color.WhiteSmoke);
			DrawLine(worldCorners[2], worldCorners[6], Color.WhiteSmoke);
		}


		public void DrawLine(DVector3 pos0, DVector3 pos1, Color color)
		{
			lines.Add(new Gis.CartPoint {
				X = pos0.X,
				Y = pos0.Y,
				Z = pos0.Z,
				Tex0	= Vector4.Zero,
				Color	= color
			});
			lines.Add(new Gis.CartPoint {
				X = pos1.X,
				Y = pos1.Y,
				Z = pos1.Z,
				Tex0	= Vector4.Zero,
				Color	= color
			});

			isDirty = true;
		}


		public void DrawPoint(DVector3 pos, double size)
		{
			lines.Add(new Gis.CartPoint {
				X		= pos.X + size,
				Y		= pos.Y,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Red
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X - size,
				Y		= pos.Y,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Red
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y + size,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Green
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y - size,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Green
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y,
				Z		= pos.Z + size,
				Tex0	= Vector4.Zero,
				Color	= Color.Blue
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y,
				Z		= pos.Z - size,
				Tex0	= Vector4.Zero,
				Color	= Color.Blue
			});

			isDirty = true;
		}



		public void DrawCircle(double radius, Color color, DMatrix transform, int density = 40)
		{
			double		angleStep	= Math.PI*2/density;
			
			for (int i = 0; i < density; i++) {
				var point0X = radius * Math.Cos(angleStep * i);
				var point0Y = radius * Math.Sin(angleStep * i);

				var point1X = radius * Math.Cos(angleStep * (i+1));
				var point1Y = radius * Math.Sin(angleStep * (i+1));

				var p0 = DVector3.TransformCoordinate(new DVector3(point0X, point0Y, 0), transform);
				var p1 = DVector3.TransformCoordinate(new DVector3(point1X, point1Y, 0), transform);

				DrawLine(p0, p1, color);
			}

		}


		public void DrawSphere(double radius, Color color, DMatrix transform, int density = 40)
		{
			DrawCircle(radius, color, transform, density);
			DrawCircle(radius, color, DMatrix.RotationX(DMathUtil.PiOverTwo) * transform, density);
			DrawCircle(radius, color, DMatrix.RotationY(DMathUtil.PiOverTwo) * transform, density);
		}



		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			return null;
		}
	}
}
