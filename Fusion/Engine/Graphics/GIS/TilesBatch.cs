using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	public partial class TilesGisLayer : Gis.GisLayer
	{
		Ubershader		shader;
		StateFactory	factory;
		
		Texture2D	frame;
		GlobeCamera camera;


		public class SelectedItem : Gis.SelectedItem
		{
			public long TileX;
			public long TileT;
		}


		[Flags]
		public enum TileFlags : int
		{
			SHOW_FRAMES		= 0x0001,
		}


		public TilesGisLayer(Game engine, GlobeCamera camera) : base(engine)
		{
			RegisterMapSources();

			CurrentMapSource = MapSources[2];

			this.camera = camera;

			frame	= Game.Content.Load<Texture2D>("redframe.tga");
			shader	= Game.Content.Load<Ubershader>("globe.Tile.hlsl");
			factory = shader.CreateFactory( typeof(TileFlags), Primitive.TriangleList, VertexInputElement.FromStructure<Gis.GeoPoint>(), BlendState.AlphaBlend, RasterizerState.CullCW, DepthStencilState.Default);
		}


		public override void Update(GameTime gameTime)
		{

			//Console.Clear();
			//Console.WriteLine("Current Zoom Level: " + CurrentLevel);
			//
			//for(int i = 0; i < 15; i++)
			//	Console.WriteLine("Zoom: " + i + "\tError: " + GetLevelScreenSpaceError(i, camera.CameraDistance - camera.EarthRadius));
			//
			//Console.WriteLine(camera.FinalCamPosition);

			CurrentMapSource.Update(gameTime);			

			DetermineTiles();

			//Console.WriteLine();
			//Console.WriteLine("Tiles to render: " + tilesToRender.Count);
			//Console.WriteLine("Free tiles:		" + tilesFree.Count);
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			var dev = Game.GraphicsDevice;

			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderSamplers[0]		= SamplerState.AnisotropicClamp;
			dev.PixelShaderResources[1]		= frame;

			dev.PipelineState = factory[(int)(TileFlags.SHOW_FRAMES)];


			foreach (var globeTile in tilesToRender) {
				var tex = CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z).Tile;
				
				dev.PixelShaderResources[0] = tex;

				dev.SetupVertexInput(globeTile.Value.VertexBuf, globeTile.Value.IndexBuf);
				dev.DrawIndexed(globeTile.Value.IndexBuf.Capacity, 0, 0);
			}
		}


		public override void Dispose()
		{
			foreach (var mapSource in MapSources) {
				mapSource.Dispose();
			}

			if (BaseMapSource.EmptyTile != null) {
				BaseMapSource.EmptyTile.Dispose();
				BaseMapSource.EmptyTile = null;
			}

			foreach (var tile in tilesToRender) {
				tile.Value.Dispose();
			}
			foreach (var tile in tilesFree) {
				tile.Value.Dispose();
			}
		}


		public override List<Gis.SelectedItem> Select(DVector3 nearPoint, DVector3 farPoint)
		{
			throw new NotImplementedException();
		}
	}
}
