using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox
{
	public class Dark : BaseMapBoxMap
	{
		public override string Name
		{
			get { return "DarkMap"; }
		}

		public override string ShortName
		{
			get { return "Dark"; }
		}

		protected override string RefererUrl
		{
			get { return "https://www.mapbox.com/"; }
		}

		public Dark(Game game) : base(game)
		{
			TileSize	= 512;
			AcessToken	= "pk.eyJ1Ijoia2FwYzNkIiwiYSI6ImNpbGpodG82czAwMmlubmtxamdsOHF0a3AifQ.xCbMUsy_a_0A9cd4GvjXKQ";
			UrlFormat	= "http://api.mapbox.com/v4/mapbox.dark/{0}/{1}/{2}@2x.png32?access_token={3}";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, zoom, x, y, AcessToken);
		}
	}
}
