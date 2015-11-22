using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS
{
	public class LinesGisLayer : Gis.GisLayer
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
			factory = new StateFactory(shader, typeof(LineFlags), Primitive.LineList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullNone, DepthStencilState.None);


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


			dev.PipelineState = factory[Flags];

			dev.GeometryShaderConstants[0]	= constBuffer;
			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderConstants[0]		= constBuffer;

			dev.PixelShaderResources[0] = Texture;
			dev.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap;


			dev.SetupVertexInput(currentBuffer, null);
			dev.Draw(currentBuffer.Capacity, 0);
		}


		void SwapBuffers()
		{

		}
	}
}
