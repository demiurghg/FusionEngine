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
			factory = new StateFactory(shader, typeof(DebugFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.CartPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);

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


		public void DrawPoint(DVector3 pos, double size)
		{
			lines.Add(new Gis.CartPoint {
				X		= pos.X + size,
				Y		= pos.Y,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Red.ToColor4()
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X - size,
				Y		= pos.Y,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Red.ToColor4()
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y + size,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Green.ToColor4()
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y - size,
				Z		= pos.Z,
				Tex0	= Vector4.Zero,
				Color	= Color.Green.ToColor4()
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y,
				Z		= pos.Z + size,
				Tex0	= Vector4.Zero,
				Color	= Color.Blue.ToColor4()
			});
			lines.Add(new Gis.CartPoint {
				X		= pos.X,
				Y		= pos.Y,
				Z		= pos.Z - size,
				Tex0	= Vector4.Zero,
				Color	= Color.Blue.ToColor4()
			});

			isDirty = true;
		}

	}
}
