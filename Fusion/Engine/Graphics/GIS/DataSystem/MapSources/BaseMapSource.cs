using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.Concurrent;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;

#pragma warning disable 0414

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources
{
	public abstract class BaseMapSource
	{
		public Game Game;
		/// <summary>
		/// minimum level of zoom
		/// </summary>
		public int		MinZoom;
		public float	TimeUntilRemove = 600;

		const int MaxDownloadTries = 3;

		/// <summary>
		/// maximum level of zoom
		/// </summary>
		public int		MaxZoom		= 18;
		public int		TileSize	= 256;
		public static	Texture2D	EmptyTile;

		List<string> ToRemove = new List<string>();
		
		public Dictionary<string, MapTile>	RamCache	= new Dictionary<string, MapTile>();

		Random r = new Random();

		string UserAgent;

		int		TimeoutMs		= 5000;
		string	requestAccept	= "*/*";

		public abstract MapProjection Projection { get; }

		bool isDisposed = false;


		protected BaseMapSource(Game game)
		{
			Game = game;

			if (EmptyTile == null) {
				EmptyTile = Game.Content.Load<Texture2D>(@"empty.png");
			}

			UserAgent = string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:{0}.0) Gecko/{2}{3:00}{4:00} Firefox/{0}.0.{1}", r.Next(3, 14), r.Next(1, 10), r.Next(DateTime.Today.Year - 4, DateTime.Today.Year), r.Next(12), r.Next(30));
		}

		public abstract string Name {
			get;
		}

		public abstract string ShortName {
			get;
		}

		protected abstract string RefererUrl { get; }


		public virtual void Update(GameTime gameTime)
		{
			foreach (var cachedTile in RamCache) {
				cachedTile.Value.Time += gameTime.ElapsedSec;

				if (cachedTile.Value.Time > TimeUntilRemove) {
					try {
						if (cachedTile.Value.IsLoaded) {
							//cachedTile.Value.Tile.Dispose();
							ToRemove.Add(cachedTile.Key);
						}
					} catch (Exception e) {
						Log.Warning(e.Message);
					}
				}
			}

			foreach (var e in ToRemove) {
				RamCache[e].Tile.Dispose();
				RamCache.Remove(e);
			}
			

			ToRemove.Clear();
		}


		public abstract string GenerateUrl(int x, int y, int zoom);

		//public MapTile GetTile(Vector2 latLon, int zoom);
		//public MapTile GetTile(float lat, float lon, int zoom);
		public MapTile GetTile(int x, int y, int zoom)
		{
			return CheckTileInMemory(x, y, zoom);
		}

		

		public byte[] DownloadMapTile(string url)
		{
			try {

				var client = new WebClient();
				return client.DownloadData(url);

				//var request = (HttpWebRequest) WebRequest.Create(url);
				//
				////WebClient wc = new WebClient();
				//request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
				//request.Timeout				= TimeoutMs;
				//request.UserAgent			= UserAgent;
				//request.ReadWriteTimeout	= TimeoutMs * 6;
				//request.Accept				= requestAccept;
				//request.Referer				= RefererUrl;
				//
				//HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				//
				//using (var s = new MemoryStream()) {
				//	var responseStream = response.GetResponseStream();
				//	if (responseStream != null) responseStream.CopyTo(s);
				//	return s.ToArray();
				//}

			} catch (Exception e) {
				Log.Warning(e.Message + "Url: " + url);
				return null;
			}
		}


		string GetKey(int m, int n, int level)
		{
			return string.Format(ShortName + "{0}_{1}_{2}", level, m, n);
		}


		MapTile CheckTileInMemory(int m, int n, int level)
		{
			string key = GetKey(m,n,level);
			string path = @"cache\" + Name + @"\" + key + ".jpg";

			if (!RamCache.ContainsKey(key)) {
				MapTile ct = new MapTile {
						Path		= path, 
						Url			= GenerateUrl(m, n, level), 
						LruIndex	= 0,
						Tile		= EmptyTile,
						X			= m,
						Y			= n,
						Zoom		= level
					};

				RamCache.Add(key, ct);

				Gis.ResourceWorker.Post(r => {
					var tile = r.Data as MapTile;
					
					if (!File.Exists(tile.Path)) {

						r.ProcessQueue.Post(t => {

							var data = DownloadMapTile(tile.Url);

							// TODO: responde to tile loading error
							if (data == null) {
								tile.LoadingTries++;
								return;
							}

							tile.Tile = new Texture2D(Game.Instance.GraphicsDevice, data);

							var fileName = tile.Path;
							r.DiskWRQueue.Post(q => {
								var file = new FileInfo(fileName);
								file.Directory.Create();

								using (var f = File.OpenWrite(fileName)) {
									var bytes = q.Data as byte[];
									f.Write(bytes, 0, bytes.Length);
								}
							}, data);

							tile.IsLoaded = true;
						}, r.Data);
					}
					else {
						r.DiskWRQueue.Post(q => {
							using (var stream = File.OpenRead(tile.Path)) {
								tile.Tile = new Texture2D(Game.Instance.GraphicsDevice, stream);
								tile.IsLoaded = true;
							}
						}, null);
					}

				}, ct);
			}

			RamCache[key].LruIndex	= level;
			RamCache[key].Time		= 0.0f;

			return RamCache[key];
		}



		public void Dispose()
		{
			isDisposed = true;

			foreach (var tile in RamCache) {
				tile.Value.Tile.Dispose();
				tile.Value.Tile = null;
			}
			RamCache.Clear();
		}

	}
}
