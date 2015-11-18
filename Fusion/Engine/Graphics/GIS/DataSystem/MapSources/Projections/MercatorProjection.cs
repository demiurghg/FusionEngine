using System;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections
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


		public override DVector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			DVector2 p = new DVector2();
			p.X = (lon + 180.0) / 360.0 * (1 << zoom);
			p.Y = (1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom);
			//p.Y = (1.0 - Math.Log((Math.Sin(lat * Math.PI / 180.0) + 1.0) / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom);
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
