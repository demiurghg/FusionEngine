using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.GIS.GlobeMath;

namespace Fusion.Engine.GIS.DataSystem.MapSources.Projections
{
	public class MapProjection
	{
		public virtual DVector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			return new DVector2();
		}

		public virtual DVector2 TileToWorldPos(double x, double y, int zoom)
		{
			return new DVector2();
		}

	}
}
