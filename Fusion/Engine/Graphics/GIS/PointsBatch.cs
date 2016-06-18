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
	public class PointsGisLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;

		[Flags]
		public enum PointFlags : int
		{
			DOTS_WORLDSPACE		= 1 << 0,
			DOTS_SCREENSPACE	= 1 << 1,
			ROTATION_ANGLE		= 1 << 2,
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
			[FieldOffset(128)]	public Vector4	AtlasSizeImgSize;
			[FieldOffset(144)]	public Vector4	SizeMult;
		 }
		DotsData dotsData;


		public struct ColorData
		{
			public Color Color;
		}
		public ColorData[] ColorDatas { get; protected set; }


		ConstantBuffer DotsBuffer;
		ConstantBuffer ColorBuffer;

		//public float	AtlasRow;
		//public float	AtlasCol;
		//public float	Size;
		//public float	Rotation;
		//public float	Height;


		public bool IsDoubleBuffer { get; protected set; }

		public Texture2D	TextureAtlas;
		public Vector2		ImageSizeInAtlas;
		public float		SizeMultiplier;
		public int			PointsCount { get { return PointsCpu.Length; } }
		public int			PointsDrawOffset;
		public int			PointsCountToDraw;

		VertexBuffer firstBuffer;
		VertexBuffer secondBuffer;
		VertexBuffer currentBuffer;

		public Gis.GeoPoint[] PointsCpu { get; protected set; }


		public bool IsDynamic { get; protected set; }



		public PointsGisLayer(Game engine, int maxPointsCount, bool isDynamic = false) : base(engine)
		{
			DotsBuffer	= new ConstantBuffer(engine.GraphicsDevice, typeof(DotsData));
			ColorBuffer = new ConstantBuffer(engine.GraphicsDevice, typeof(ColorData), 16);

			PointsCountToDraw	= maxPointsCount;
			PointsDrawOffset	= 0;

			SizeMultiplier	= 1;
			IsDynamic		= isDynamic;

			var vbOptions = isDynamic ? VertexBufferOptions.Dynamic : VertexBufferOptions.Default;

			firstBuffer		= new VertexBuffer(engine.GraphicsDevice, typeof(Gis.GeoPoint), maxPointsCount, vbOptions);
			currentBuffer	= firstBuffer;

			PointsCpu	= new Gis.GeoPoint[maxPointsCount];
			
			Flags		= (int) (PointFlags.DOTS_WORLDSPACE);

			shader	= Game.Content.Load<Ubershader>("globe.Point.hlsl");
			factory = shader.CreateFactory( typeof(PointFlags), Primitive.PointList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullCCW, DepthStencilState.None);

			ColorDatas = new ColorData[16];
			for (int i = 0; i < ColorDatas.Length; i++) {
				ColorDatas[i] = new ColorData {Color = Color.White};
			}

			ColorBuffer.SetData(ColorDatas);
		}


		public void UpdatePointsBuffer()
		{
			if (currentBuffer == null) return;

			currentBuffer.SetData(PointsCpu);
		}


		public void Update(GameTime gameTime, GlobeCamera camera = null)
		{
			if (TextureAtlas == null) return;

			var curCamera = camera ?? Game.RenderSystem.Gis.Camera;

			dotsData.View				= curCamera.ViewMatrixFloat;
			dotsData.Proj				= curCamera.ProjMatrixFloat;
			dotsData.AtlasSizeImgSize	= new Vector4(TextureAtlas.Width, TextureAtlas.Height, ImageSizeInAtlas.X, ImageSizeInAtlas.Y);
			dotsData.SizeMult			= new Vector4(SizeMultiplier);


			DotsBuffer.SetData(dotsData);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			if (TextureAtlas == null) return;

			Update(gameTime);

			var dev = Game.GraphicsDevice;

			//dev.ResetStates();

			dev.PipelineState = factory[Flags];

			dev.VertexShaderConstants[0]	= constBuffer;
			dev.GeometryShaderConstants[0]	= constBuffer;

			dev.VertexShaderConstants[1]	= DotsBuffer;
			dev.GeometryShaderConstants[1]	= DotsBuffer;

			dev.GeometryShaderConstants[2]	= ColorBuffer;


			dev.PixelShaderResources[0]	= TextureAtlas;
			dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;


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

			for (int i = 0; i < PointsCountToDraw; i++) {
				int ind		= PointsDrawOffset + i;
				var point	= PointsCpu[ind];

				var size		= point.Tex0.Z * 0.5;
				var pointLonLat = new DVector2(point.Lon, point.Lat);


				var dist = GeoHelper.DistanceBetweenTwoPoints(pointLonLat, rayLonLatRad);

				if (dist <= size) {
					ret.Add(new SelectedItem {
						Distance	= dist,
						PointIndex	= ind
					});
				}
			}

			return ret;
		}
	}
}
