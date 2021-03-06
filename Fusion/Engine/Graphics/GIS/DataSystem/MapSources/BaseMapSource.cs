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

		/// <summary>
		/// maximum level of zoom
		/// </summary>
		public int?		MaxZoom		= 21;
		public int		TileSize	= 256;
		public static	Texture2D	EmptyTile;

		List<string> ToRemove = new List<string>();
		
		ConcurrentQueue<MapTile>			cacheQueue	= new ConcurrentQueue<MapTile>();
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

			killToken = new CancellationTokenSource();
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

		

		public bool DownloadTile(MapTile tile)
		{
			try {
				var request = (HttpWebRequest) WebRequest.Create(tile.Url);
				
				request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
				request.Timeout				= TimeoutMs;
				request.UserAgent			= UserAgent;
				request.ReadWriteTimeout	= TimeoutMs * 6;
				request.Accept				= requestAccept;
				request.Referer				= RefererUrl;
				
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				
				if (!Directory.Exists(@"cache\" + Name)) {
					Directory.CreateDirectory(@"cache\" + Name);
				}
				
				System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(response.GetResponseStream());
				bitmap.Save(tile.Path, System.Drawing.Imaging.ImageFormat.Jpeg);
				return true;
			} catch (Exception e) {
				Log.Warning(e.Message);
				return false;
			}
		}

		bool threadStopped = true;
		void TileStreamingThreadFunc(CancellationToken cancellationToken)
		{
			MapTile ct = null;
			threadStopped = false;

			while (!cancellationToken.IsCancellationRequested) {
				cacheQueue.TryDequeue(out ct);

				if (ct == null) {
					Thread.Sleep(100);
					continue;
				}

				try {
					if (!File.Exists(ct.Path)) {
						if (!DownloadTile(ct))
							throw new WebException(String.Format("Tile x:{0} y:{1} z:{2} not found", ct.X, ct.Y, ct.Zoom));
					}

					Texture2D tex = null;

					using ( var stream = File.OpenRead(ct.Path) ) {
						tex = new Texture2D( Game.GraphicsDevice, stream );
					}

					ct.Tile		= tex;
					ct.IsLoaded = true;

					tex = null;
				} catch (WebException e) {
					Log.Warning(e.Message);
				} catch (Exception e) {
					Console.WriteLine("Exception : {0}", e.Message);
					ct.LruIndex = 0;
					ct.Tile		= EmptyTile;
					cacheQueue.Enqueue(ct);
				} finally {
				}
			}

			threadStopped = true;
		}


		Task					tileStreamingTask;
		CancellationTokenSource killToken;


		MapTile CheckTileInMemory(int m, int n, int level)
		{
			string key	= string.Format(ShortName + "{0:00}{1:0000}{2:0000}", level, m, n);
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

				cacheQueue.Enqueue(ct);
			}

			if (tileStreamingTask == null && !isDisposed) {
				tileStreamingTask = Task.Run(() => TileStreamingThreadFunc(killToken.Token), killToken.Token);
			}

			RamCache[key].LruIndex	= level;
			RamCache[key].Time		= 0.0f;

			return RamCache[key];
		}



		public void Dispose()
		{
			if (killToken != null)			killToken.Cancel();
			if (tileStreamingTask != null)	tileStreamingTask.Wait();

			isDisposed = true;

			foreach (var tile in RamCache) {
				tile.Value.Tile.Dispose();
				tile.Value.Tile = null;
			}
			RamCache.Clear();
		}

	}
}
