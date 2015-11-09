using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.GIS.GlobeMath;

namespace Fusion.Engine.GIS.DataSystem.MapSources.Projections
{
	public class MercatorProjection : MapProjection
	{
		public static readonly MercatorProjection Instance = new MercatorProjection();

		static readonly float MinLatitude	= -85.05112878f;
		static readonly float MaxLatitude	= 85.05112878f;
		static readonly float MinLongitude	= -180;
		static readonly float MaxLongitude	= 180;

		//public override RectLatLng Bounds
		//{
		//	get { return RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude); }
		//}

		readonly Vector2 tileSize = new Vector2(256, 256);

		public Vector2 TileSize
		{
			get { return tileSize; }
		}

		public double Axis
		{
			get { return 6378137; }
		}

		public double Flattening
		{
			get { return (1.0/298.257223563); }
		}

		public Vector2 FromLatLngToPixel(float lat, float lng, int zoom = 0)
		{
			Vector2 ret = Vector2.Zero;

			lat = MathUtil.Clamp(lat, MinLatitude, MaxLatitude);
			lng = MathUtil.Clamp(lng, MinLongitude, MaxLongitude);

			float x = (lng + 180)/360;
			float sinLatitude = (float)System.Math.Sin(lat*Math.PI/180);
			float y = 0.5f - (float)Math.Log((1 + sinLatitude)/(1 - sinLatitude))/(4*(float)Math.PI);

			//Vector2 s = GetTileMatrixSizePixel(zoom);
			//float mapSizeX = s.X;
			//float mapSizeY = s.Y;

			//ret.X = MathUtil.Clamp(x * mapSizeX + 0.5f, 0, mapSizeX - 1);
			//ret.Y = MathUtil.Clamp(y * mapSizeY + 0.5f, 0, mapSizeY - 1);

			ret.X = x;
			ret.Y = y;

			return ret;
		}

		public Vector2 FromPixelToLatLng(float x, float y, int zoom = 0)
		{
			Vector2 ret = Vector2.Zero;

			//Vector2 s = GetTileMatrixSizePixel(zoom);
			//double mapSizeX = s.Width;
			//double mapSizeY = s.Height;
			//
			//double xx = (Clip(x, 0, mapSizeX - 1)/mapSizeX) - 0.5;
			//double yy = 0.5 - (Clip(y, 0, mapSizeY - 1)/mapSizeY);

			float xx = x - 0.5f;
			float yy = 0.5f - y;

			ret.Y = 90 - 360 * (float)Math.Atan((float)Math.Exp(-yy * 2 * (float)Math.PI)) / (float)Math.PI;
			ret.X = 360*xx;

			return ret;
		}


		public override DVector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			DVector2 p = new DVector2();
			p.X = (lon + 180.0) / 360.0 * (1 << zoom);
			p.Y = (1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom);

			return p;
		}

		public override DVector2 TileToWorldPos(double x, double y, int zoom)
		{
			double lon, lat;

			double n = Math.PI - ((2.0 * Math.PI * y) / (1 << zoom));

			lon = (x / (1 << zoom) * 360.0) - 180.0;
			lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));

			return new DVector2(lon, lat);
		}

	}
}
