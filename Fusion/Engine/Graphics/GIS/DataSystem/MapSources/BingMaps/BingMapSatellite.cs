using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.BingMaps
{
	class BingMapSatellite : BaseBingMapsSource
	{

		public override string Name {
			get { return "BingMapSatellite"; }
		}

		public override string ShortName {
			get { return "BMS"; }
		}

		public BingMapSatellite(Game game) : base(game)
		{
			UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}&n=z{4}";
		}

	}
}
