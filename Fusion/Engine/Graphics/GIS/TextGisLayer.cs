using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public class TextGisLayer : Gis.GisLayer
	{
		public GlobeCamera GlobeCamera { set; get; }
		//private bool		isDirty = false;

		public double MinZoom;
		public double MaxZoom;

		public SpriteLayer TextSpriteLayer { protected set; get; }

		public class GeoText
		{
			public DVector2 LonLat;
			public string	Text;
			public Color	Color;
		}
		
		public GeoText[]	GeoTextArray	{ protected set; get; }
		public DiscTexture	Font			{ set; get; }
		//public SpriteFont	spriteFont;
		public float		Scale			{ set; get; }



		public TextGisLayer(Game game, int capacity, GlobeCamera camera) : base(game)
		{
			GlobeCamera		= camera;
			TextSpriteLayer = new SpriteLayer(Game.RenderSystem, 2048);

			GeoTextArray	= new GeoText[capacity];
			Font			= Game.Content.Load<DiscTexture>("conchars");
			//spriteFont		= Game.Content.Load<SpriteFont>(@"Fonts\textFont");

			MinZoom = 6380;
			MaxZoom = 6500;

			Scale = 1.0f;
		}


		void UpdateText()
		{
			if (GlobeCamera == null) return;

			if (MaxZoom != MinZoom) {
				double amount = (GlobeCamera.CameraDistance - MinZoom)/(MaxZoom - MinZoom);
				Scale = (float) DMathUtil.Lerp(1.5, 0.0, amount);
				Scale = (float) DMathUtil.Clamp(Scale, 0.0, 1.5);
			}

			TextSpriteLayer.Clear();
			TextSpriteLayer.BlendMode = SpriteBlendMode.AlphaBlend;

			foreach (var text in GeoTextArray) {
				if(text == null || text.Text.Length == 0) continue;

				var cartPos		= GeoHelper.SphericalToCartesian(text.LonLat, GeoHelper.EarthRadius);
				var screenPos	= GlobeCamera.CartesianToScreen(cartPos);

				TextSpriteLayer.DrawDebugString(Font, (int)screenPos.X, (int)screenPos.Y, text.Text, text.Color, Scale);
				//spriteFont.DrawString(TextSpriteLayer, text.Text, screenPos.X, screenPos.Y, text.Color);
			}
		}


		public override void Update(GameTime gameTime)
		{
			//UpdateText();
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			UpdateText();
		}


		public override void Dispose()
		{
			TextSpriteLayer.Dispose();
		}
	}
}
