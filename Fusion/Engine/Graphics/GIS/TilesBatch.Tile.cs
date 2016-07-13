using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS
{
	partial class TilesGisLayer
	{
		int lowestLod	= 9;
		int minLod		= 3;

		int tileDensity		= 5;
		int CurrentLevel	= 8;

		bool updateTiles = false;

		class Node
		{
			public int X, Y, Z;

			public Node		Parent;
			public Node[]	Childs;
		}

		struct TraversalInfo
		{
			public Node CentralNode;
			public int Offset, Length;
		}


		class GlobeTile : IDisposable
		{
			public int X;
			public int Y;
			public int Z;


			public double left, top, right, bottom;

			public VertexBuffer VertexBuf;
			public IndexBuffer IndexBuf;


			public void Dispose()
			{
				VertexBuf.Dispose();
				IndexBuf.Dispose();
			}
		}

		Dictionary<string, GlobeTile> tilesToRender = new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesOld		= new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesFree		= new Dictionary<string, GlobeTile>();



		void DetermineTiles()
		{
			var ms = CurrentMapSource;

			//var d = Math.Log((camera.CameraDistance - camera.EarthRadius) * 1000.0, 2.0);
			double lod = lowestLod;
			
			if (camera.Viewport.Width != 0) {
				int closestZoom = lowestLod;
				double	closestRadius	= 100;

				for (int zoom = 3; zoom <= ms.MaxZoom; zoom++) {
					var dis = GetLevelScreenSpaceError(zoom, camera.CameraDistance - camera.EarthRadius);

					if (dis < closestRadius && dis >= 0.0f) {
						closestRadius	= dis;
						closestZoom		= zoom;
					}
				}

				lod = closestZoom;
			}


			var maxLod = ms.MaxZoom;

			lowestLod = (int)lod;

			if (lowestLod > maxLod.Value) lowestLod = maxLod.Value;
			CurrentLevel = lowestLod;

			if (CurrentLevel < 3) CurrentLevel = 3;


			// Get camera mercator position 
			var lonLat = camera.GetCameraLonLat();
			lonLat.X = DMathUtil.RadiansToDegrees(lonLat.X);
			lonLat.Y = DMathUtil.RadiansToDegrees(lonLat.Y);


			if (updateTiles) {
				foreach (var tile in tilesToRender) {
					tilesFree.Add(tile.Key, tile.Value);
				}
				updateTiles = false;
			} else {
				foreach (var tile in tilesToRender) {
					tilesOld.Add(tile.Key, tile.Value);
				}
			}

			tilesToRender.Clear();


			var info = new TraversalInfo[2];

			var centralNode = new Node { Z = CurrentLevel - 2 };

			var tileUpper = ms.Projection.WorldToTilePos(lonLat.X, lonLat.Y, centralNode.Z);
			centralNode.X = (int)tileUpper.X;
			centralNode.Y = (int)tileUpper.Y;
			//GetTileIndexByMerc(merc, centralNode.Z, out centralNode.X, out centralNode.Y);

			info[0].CentralNode = new Node { X = centralNode.X - 7, Y = centralNode.Y - 7, Z = centralNode.Z };
			info[0].Offset = 4;
			info[0].Length = 7;

			var offNode = new Node { X = info[0].CentralNode.X + info[0].Offset, Y = info[0].CentralNode.Y + info[0].Offset, Z = info[0].CentralNode.Z };
			GetChilds(ref offNode);

			info[1].CentralNode = offNode.Childs[0];
			info[1].Offset = 3;
			info[1].Length = 8;

			int tilesNum = 1 << info[0].CentralNode.Z;

			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{

					var nodeX = info[0].CentralNode.X + i;
					var nodeY = info[0].CentralNode.Y + j;

					nodeX = nodeX % tilesNum;
					if (nodeX < 0) nodeX = tilesNum + nodeX;
					if (nodeY < 0 || nodeY >= tilesNum) continue;

					var currNode = new Node { X = nodeX, Y = nodeY, Z = info[0].CentralNode.Z };

					QuadTreeTraversalDownTop(info, currNode, 0);
				}
			}


			foreach (var tile in tilesOld)
			{
				tilesFree.Add(tile.Key, tile.Value);
			}
			tilesOld.Clear();
		}

		
		private double GetLevelScreenSpaceError(int zoom, double distance)
		{
			double eps	= 256.0/(1 << zoom);
			double xx	= camera.Viewport.Height;
			double dd	= distance;
			double eta	= DMathUtil.DegreesToRadians(camera.Parameters.CameraFovDegrees);

			double p = (eps*xx)/(2*dd*Math.Tan(eta));

			var dis = 1.0 - p;
			return dis;
		}


		double GetOptimalDistanceForLevel(int zoom)
		{
			double eps = 256.0 / (1 << zoom);
			double xx = camera.Viewport.Height;
			double eta = DMathUtil.DegreesToRadians(camera.Parameters.CameraFovDegrees);

			double dd = (eps * xx) / (2 * Math.Tan(eta));

			return dd;
		}


		void QuadTreeTraversalDownTop(TraversalInfo[] info, Node node, int step)
		{
			int maxLevel = CurrentMapSource.MaxZoom.Value;

			if (node.Z > maxLevel) return;

			if (step >= info.Length) {
				AddTileToRenderList(node.X, node.Y, node.Z);
				return;
			}

			GetChilds(ref node);

			int offX = node.X - info[step].CentralNode.X;
			int offY = node.Y - info[step].CentralNode.Y;

			CurrentMapSource.GetTile(node.X, node.Y, node.Z);

			if (offX >= info[step].Offset && offX < info[step].Offset + info[step].Length &&
				offY >= info[step].Offset && offY < info[step].Offset + info[step].Length) {

				if (CheckTiles(node)) {
					foreach (var child in node.Childs) {
						QuadTreeTraversalDownTop(info, child, step + 1);
					}
				} else {
					AddTileToRenderList(node.X, node.Y, node.Z);
				}

			} else {
				AddTileToRenderList(node.X, node.Y, node.Z);
			}
		}


		bool CheckTiles(Node node)
		{
			if (node.Childs == null) return false;
			if (node.Childs[0].Z > CurrentMapSource.MaxZoom.Value) return false;

			bool check = true;
			foreach (var child in node.Childs) {
				check = check && CurrentMapSource.GetTile(child.X, child.Y, child.Z).IsLoaded;
			}
			return check;
		}


		void DetermineTiles(int startZoomLevel)
		{
			////////////////////////////////////////////////////
			if (updateTiles) {
				foreach (var tile in tilesToRender) {
					tilesFree.Add(tile.Key, tile.Value);
				}
				updateTiles = false;
			} else {
				foreach (var tile in tilesToRender) {
					tilesOld.Add(tile.Key, tile.Value);
				}
			}

			tilesToRender.Clear();
			/////////////////////////////////////////////////////


			//var cameraPos = DVector3.Transform(new DVector3(0, 0, 6700), DQuaternion.RotationYawPitchRoll(0.52932849788406378, -1.0458657020378879, 0));
			var cameraPos = camera.CameraPosition;
			var cameraNorm = DVector3.Normalize(cameraPos);

			//var debug		= Gis.Debug; 
			//debug.Clear();
			//debug.DrawPoint(cameraPos, 200);
			//Console.WriteLine("Camera pos: " + cameraPos);

			Stack<Node> nodes = new Stack<Node>();
			long numTiles = 1 << startZoomLevel;


			for(int i = 0; i < numTiles; i++)
				for (int j = 0; j < numTiles; j++)
					nodes.Push(new Node {
						X = i,
						Y = j,
						Z = startZoomLevel
					});

			//nodes.Push(new Node {
			//	X = 3,
			//	Y = 2,
			//	Z = startZoomLevel
			//});

			while (nodes.Any()) {
				var node = nodes.Pop();

				var nodePos = GetTileCenterPosition(node.X, node.Y, node.Z);

				//debug.DrawPoint(nodePos, 100);

				var nodeNorm = DVector3.Normalize(nodePos);

				var dist	= (nodePos - cameraPos).Length();

				//Console.WriteLine();
				//Console.WriteLine("Node: "		+ node.X + " " + node.Y + " " + node.Z);
				//Console.WriteLine("Dist: "		+ dist);
				//Console.WriteLine("Node pos: "	+ nodePos);
				//Console.WriteLine(GetLevelScreenSpaceError(node.Z, dist));

				if (GetLevelScreenSpaceError(node.Z, dist) < 0.0) // Break this tile to pieces
				{
					GetChilds(ref node);

					foreach (var child in node.Childs) {
						nodes.Push(child);
					}
				}
				else {
					if (DVector3.Dot(cameraNorm, nodeNorm) > -0.1)
						AddTileToRenderList(node.X, node.Y, node.Z);
				}
			}

			foreach (var node in nodes) {
				AddTileToRenderList(node.X, node.Y, node.Z);
			}

			/////////////////////////////////////////////////////
			foreach (var tile in tilesOld) {
				tilesFree.Add(tile.Key, tile.Value);
			}
			tilesOld.Clear();
		}



		DVector3 GetTileCenterPosition(int x, int y, int z)
		{
			long numTiles = 1 << z;

			double x0 = ((double)(x + 0) / (double)numTiles);
			double y0 = ((double)(y + 0) / (double)numTiles);
			double x1 = ((double)(x + 1) / (double)numTiles);
			double y1 = ((double)(y + 1) / (double)numTiles);

			var xHalf = (x0 + x1)/2.0;
			var yHalf = (y0 + y1)/2.0;

			var lonLat	= CurrentMapSource.Projection.TileToWorldPos(xHalf, yHalf);
			var ret		= GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(lonLat), GeoHelper.EarthRadius);
			return ret;
		}


		void AddTileToRenderList(int x, int y, int zoom)
		{
			string key = GenerateKey(x, y, zoom);

			if (tilesToRender.ContainsKey(key)) return;

			if (tilesOld.ContainsKey(key))
			{
				tilesToRender.Add(key, tilesOld[key]);
				tilesOld.Remove(key);
				return;
			}


			long numTiles = 1 << zoom;

			double x0 = ((double)(x + 0) / (double)numTiles);
			double y0 = ((double)(y + 0) / (double)numTiles);
			double x1 = ((double)(x + 1) / (double)numTiles);
			double y1 = ((double)(y + 1) / (double)numTiles);


			if (tilesFree.Any()) {
				GlobeTile tile;
				if (tilesFree.ContainsKey(key)) {
					tile = tilesFree[key];
					tilesFree.Remove(key);
				} else {
					var temp = tilesFree.First();
					tile = temp.Value;
					tilesFree.Remove(temp.Key);
				}


				tile.X = x;
				tile.Y = y;
				tile.Z = zoom;

				tile.left = x0;
				tile.right = x1;
				tile.top = y0;
				tile.bottom = y1;

				int[] indexes;
				Gis.GeoPoint[] vertices;

				CalculateVertices(out vertices, out indexes, tileDensity, x0, x1, y0, y1);

				tile.VertexBuf.SetData(vertices, 0, vertices.Length);
				tile.IndexBuf.SetData(indexes, 0, indexes.Length);

				tilesToRender.Add(key, tile);

			} else {

				var tile = new GlobeTile {
					X		= x,
					Y		= y,
					Z		= zoom,
					left	= x0,
					right	= x1,
					top		= y0,
					bottom	= y1
				};


				GenerateTileGrid(tileDensity, ref tile.VertexBuf, out tile.IndexBuf, x0, x1, y0, y1);

				tilesToRender.Add(key, tile);
			}
		}


		void GenerateTileGrid(int density, ref VertexBuffer vb, out IndexBuffer ib, double left, double right, double top, double bottom)
		{
			int[]			indexes;
			Gis.GeoPoint[]	vertices;

			DisposableBase.SafeDispose(ref vb);

			CalculateVertices(out vertices, out indexes, density, left, right, top, bottom);

			vb = new VertexBuffer(Game.GraphicsDevice, typeof(Gis.GeoPoint), vertices.Length);
			ib = new IndexBuffer(Game.GraphicsDevice, indexes.Length);
			ib.SetData(indexes);
			vb.SetData(vertices, 0, vertices.Length);
		}


		void CalculateVertices(out Gis.GeoPoint[] vertices, out int[] indeces, int density, double left, double right, double top, double bottom)
		{
			int RowsCount		= density + 2;
			int ColumnsCount	= RowsCount;

			//var el = Game.GetService<LayerService>().ElevationLayer;
			var ms = CurrentMapSource;

			var		verts	= new List<Gis.GeoPoint>();
			float	step	= 1.0f / (density + 1);
			double	dStep	= 1.0 / (double)(density + 1);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {

					double xx = left * (1.0 - dStep * col) + right * dStep * col;
					double yy = top * (1.0 - dStep * row) + bottom * dStep * row;

					double lon, lat;
					var sc = ms.Projection.TileToWorldPos(xx, yy, 0);

					//float elev = 0.0f;
					//if (zoom > 8) elev = el.GetElevation(sc.X, sc.Y) / 1000.0f;

					lon = sc.X * Math.PI / 180.0;
					lat = sc.Y * Math.PI / 180.0;


					verts.Add(new Gis.GeoPoint {
						Tex0	= new Vector4(step * col, step * row, 0, 0),
						Lon		= lon,
						Lat		= lat
					});
				}

			}


			var tindexes = new List<int>();

			for (int row = 0; row < RowsCount - 1; row++)
			{
				for (int col = 0; col < ColumnsCount - 1; col++)
				{
					tindexes.Add(col + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + row * ColumnsCount);

					tindexes.Add(col + 1 + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
				}
			}

			vertices = verts.ToArray();
			indeces = tindexes.ToArray();
		}



		void GetChilds(ref Node node)
		{
			long tilesCurrentLevel = 1 << node.Z;
			long tilesNextLevel = 1 << (node.Z + 1);

			int posX = (int)(((double)node.X / tilesCurrentLevel) * tilesNextLevel);
			int posY = (int)(((double)node.Y / tilesCurrentLevel) * tilesNextLevel);

			node.Childs = new[] {
					new Node{ X = posX,		Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX +1,	Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX,		Y = posY + 1,	Z = node.Z+1, Parent = node }, 
					new Node{ X = posX + 1,	Y = posY + 1,	Z = node.Z+1, Parent = node }
				};
		}


		string GenerateKey(int x, int y, int zoom)
		{
			return x + "_" + y + "_" + zoom;
		}
	}
}
