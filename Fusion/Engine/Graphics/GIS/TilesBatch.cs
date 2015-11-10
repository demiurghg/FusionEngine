using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;

namespace Fusion.Engine.Graphics.GIS
{
	public partial class TilesBatch : GIS.Batch
	{
		Ubershader		shader;
		StateFactory	factory;


		Texture2D frame;


		[Flags]
		public enum TileFlags : int
		{
			NONE = 0x0000,
			SHOW_FRAMES = 0x0001,
			DRAW_COLOR = 0x0002,
			DRAW_TEXTURED = 0x0004,
			DRAW_PALETTE = 0x0008,
			DRAW_POLY = 0x0010,
			DRAW_DOTS = 0x0020,
			DRAW_HEAT = 0x0040,
			DRAW_LINES = 0x0080,
			DRAW_ARCS = 0x0100,
			DRAW_SEGMENTED_LINES = 0x0200,
			DRAW_ATMOSPHERE = 0x0400,

			USE_GEOCOORDS = 0x1000,
			USE_CARTCOORDS = 0x2000,

			VERTEX_SHADER = 0x4000,
			DRAW_CHARTS = 0x8000,

			DOTS_SCREENSPACE = 0x00010000,
			DOTS_WORLDSPACE = 0x00020000,

			DRAW_VERTEX_LINES = 0x00040000,
			DRAW_VERTEX_DOTS = 0x00080000,
			DRAW_VERTEX_POLY = 0x00100000,
			GEOMETRY_NORMALS = 0x00200000,
			ROTATION_ANGLE = 0x00400000,
		}


		public TilesBatch(GameEngine engine) : base(engine)
		{
			RegisterMapSources();

			CurrentMapSource = MapSources[0];

			frame	= GameEngine.Content.Load<Texture2D>("redframe.tga");
			shader	= GameEngine.Content.Load<Ubershader>("globe.Tile.hlsl");
			factory = new StateFactory(shader, typeof(TileFlags), Primitive.TriangleList, VertexInputElement.FromStructure<GIS.GeoPoint>(), BlendState.Opaque, RasterizerState.CullCW, DepthStencilState.Default);
		}


		public override void Update(GameTime gameTime)
		{
			var oldProj = CurrentMapSource.Projection;

			CurrentMapSource.Update(gameTime);

			CurrentMapSource = MapSources[(int)0];


			if (!oldProj.Equals(CurrentMapSource.Projection)) {
				updateTiles = true;
			}


			DetermineTiles();
		}


		public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
		{
			var dev = GameEngine.GraphicsDevice;

			GameEngine.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			GameEngine.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
			GameEngine.GraphicsDevice.PixelShaderResources[1]	= frame;

			GameEngine.GraphicsDevice.PipelineState = factory[(int)(TileFlags.DRAW_POLY | TileFlags.DRAW_VERTEX_POLY | TileFlags.VERTEX_SHADER | TileFlags.DRAW_TEXTURED | TileFlags.SHOW_FRAMES)];


			foreach (var globeTile in tilesToRender) {
				var tex = CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z).Tile;
				
				dev.PixelShaderResources[0] = tex;

				dev.SetupVertexInput(globeTile.Value.VertexBuf, globeTile.Value.IndexBuf);
				dev.DrawIndexed(globeTile.Value.IndexBuf.Capacity, 0, 0);
			}
		}


		public override void Dispose()
		{
			factory.Dispose();
			shader.Dispose();

			foreach (var mapSource in MapSources) {
				mapSource.Dispose();
			}

			if (BaseMapSource.EmptyTile != null) {
				BaseMapSource.EmptyTile.Dispose();
			}

			foreach (var tile in tilesToRender) {
				tile.Value.Dispose();
			}
			foreach (var tile in tilesFree) {
				tile.Value.Dispose();
			}
		}
	}
}
