using System;
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
		Ubershader		shader;
		StateFactory	factory;

		[Flags]
		public enum LineFlags : int
		{
			DRAW_LINES				= 1 << 0,
			DRAW_SEGMENTED_LINES	= 1 << 1,
			ARC_LINE				= 1 << 2,
			ADD_CAPS				= 1 << 3,
			FADING_LINE				= 1 << 4,
		}

		public int Flags;


		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D Texture;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public GIS.GeoPoint[] PointsCpu { get; protected set; }


		public LinesGisBatch(GameEngine engine, int linesPointsCount, bool isDynamic = false) : base(engine)
		{
			shader = GameEngine.Content.Load<Ubershader>("globe.Line.hlsl");
			factory = new StateFactory(shader, typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<GIS.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);


			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer		= new VertexBuffer(engine.GraphicsDevice, typeof(GIS.GeoPoint), linesPointsCount, vbOptions);
			currentBuffer	= firstBuffer;

			PointsCpu = new GIS.GeoPoint[linesPointsCount];

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


			dev.PipelineState = factory[Flags];

			dev.GeometryShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderConstants[0]		= constBuffer;


			dev.SetupVertexInput(currentBuffer, null);
			dev.Draw(currentBuffer.Capacity, 0);
		}


		void SwapBuffers()
		{

		}
	}
}
