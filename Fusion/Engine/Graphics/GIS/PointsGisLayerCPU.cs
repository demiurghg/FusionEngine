using System;
using System.Collections.Generic;
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
	public class PointsGisLayerCPU : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;

		[StructLayout(LayoutKind.Explicit)]
	    public struct DataForDots
	    {
		    [FieldOffset(0)]	public float	LimitAlpha;
		    [FieldOffset(4)]	public float	FadingVelocity;
		    [FieldOffset(8)]	public float	DeltaTime;
		    [FieldOffset(12)]	public float	Parameter;
	    }
		DataForDots dotsData;

		[Flags]
		public enum PointFlags : int
		{
			DRAW_TEXTURED_POLY = 1 << 0,
			POINT_FADING  = 1 << 1,
		}

		public int Flags;

		public Gis.CartPoint[] PointsCpu { get; protected set; }
		VertexBuffer currentBuffer;
		private int[] indeces;
		private IndexBuffer indBuf;


		public Texture2D	TextureAtlas;
		public Vector2		ImageSizeInAtlas;
		public float		SizeMultiplier;
		public int	PointsCount { get { return PointsCpu.Length; } }
		public int	PointsDrawOffset;
		public int	PointsCountToDraw;

		ConstantBuffer DotsBuffer;

		public PointsGisLayerCPU(Game game, int maxPointsCount, bool isDynamic = true, float fadingVelocity = 0.01f, float limitAlpha = 0.5f) : base(game)
		{
			DotsBuffer	= new ConstantBuffer(game.GraphicsDevice, typeof(DataForDots));
			PointsCountToDraw = maxPointsCount;
			indeces = new int[maxPointsCount*6];
			PointsDrawOffset = 0;
			SizeMultiplier = 1;

			dotsData.DeltaTime = 0;
			dotsData.FadingVelocity	= fadingVelocity;
			dotsData.LimitAlpha = limitAlpha;

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			currentBuffer	= new VertexBuffer(Game.GraphicsDevice, typeof(Gis.CartPoint), maxPointsCount * 4, vbOptions);
			PointsCpu		= new Gis.CartPoint[maxPointsCount*4];

			indBuf = new IndexBuffer(Game.GraphicsDevice, indeces.Length);
			for (int i = 0; i < maxPointsCount; i += 1) {
				indeces[i*6 + 0] = i*4 + 0;
				indeces[i*6 + 1] = i*4 + 1;
				indeces[i*6 + 2] = i*4 + 2;

				indeces[i*6 + 3] = i*4 + 1;
				indeces[i*6 + 4] = i*4 + 3;
				indeces[i*6 + 5] = i*4 + 2;
			}
			indBuf.SetData(indeces);

			shader	= Game.Content.Load<Ubershader>("globe.Debug.hlsl");
			factory = shader.CreateFactory(typeof(PointFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.CartPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.None);

		}


		public void AddPoint(int index, DVector2 lonLat, int typeInd, Color color, float size = 0.01f)
		{
			var basis = GeoHelper.CalculateBasisOnSurface(lonLat, true);

			var trans = basis.TranslationVector;

			var len = trans.Length();

			var topLeft		= trans + basis.Forward*size - basis.Right*size;
			var topRight	= trans + basis.Forward*size + basis.Right*size;
			var botLeft		= trans - basis.Forward*size - basis.Right*size;
			var botRight	= trans - basis.Forward*size + basis.Right*size;

			PointsCpu[index * 4 + 0] = new Gis.CartPoint {
				X = topLeft.X,
				Y = topLeft.Y,
				Z = topLeft.Z,
				Color = color,
				Tex0 = new Vector4(1, 1, 0, 0)
			};
			PointsCpu[index * 4 + 1] = new Gis.CartPoint {
				X = topRight.X,
				Y = topRight.Y,
				Z = topRight.Z,
				Color = color,
				Tex0 = new Vector4(0, 1, 0, 0)
			};
			PointsCpu[index * 4 + 2] = new Gis.CartPoint {
				X = botLeft.X,
				Y = botLeft.Y,
				Z = botLeft.Z,
				Color = color,
				Tex0 = new Vector4(1, 0, 0, 0)
			};
			PointsCpu[index * 4 + 3] = new Gis.CartPoint {
				X = botRight.X,
				Y = botRight.Y,
				Z = botRight.Z,
				Color = color,
				Tex0 = new Vector4(0, 0, 0, 0)
			};
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			dotsData.DeltaTime	+= gameTime.ElapsedSec;
			
			DotsBuffer.SetData(dotsData);
			
			Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			Game.GraphicsDevice.VertexShaderConstants[1]	= DotsBuffer;
			Game.GraphicsDevice.PipelineState = factory[(int)(PointFlags.DRAW_TEXTURED_POLY | PointFlags.POINT_FADING)];

			Game.GraphicsDevice.PixelShaderResources[0] = TextureAtlas;
			Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

			if (PointsCpu.Any()) {
				Game.GraphicsDevice.SetupVertexInput(currentBuffer, indBuf);
				Game.GraphicsDevice.DrawIndexed(indeces.Length, 0, 0);
			}
		}

	}
}
