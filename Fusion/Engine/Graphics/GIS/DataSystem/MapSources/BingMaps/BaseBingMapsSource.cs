using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.BingMaps
{
	class BaseBingMapsSource : BaseMapSource
	{
		public override MapProjection Projection {
			get { return MercatorProjection.Instance; }
		}

		public override string Name {
			get { return "BingMaps"; }
		}

		public override string ShortName {
			get { return "BM"; }
		}

		protected override string RefererUrl {
			get { return "http://www.virtualearth.net"; }
		}


		internal string TileXYToQuadKey(long tileX, long tileY, int levelOfDetail)
		{
			StringBuilder quadKey = new StringBuilder();
			for (int i = levelOfDetail; i > 0; i--) {
				char	digit	= '0';
				int		mask	= 1 << (i - 1);
				if ((tileX & mask) != 0) {
					digit++;
				}
				if ((tileY & mask) != 0) {
					digit++;
					digit++;
				}
				quadKey.Append(digit);
			}
			return quadKey.ToString();
		}


		public BaseBingMapsSource(Game game) : base(game)
		{
			
		}


		// http://ak.dynamic.t2.tiles.virtualearth.net/comp/ch/120030?mkt=ru-RU&it=G,BX,RL&shading=hill&n=z&og=117&c4w=1
		// http://ecn.t0.tiles.virtualearth.net/tiles/r120030?g=875&mkt=en-us&lbl=l1&stl=h&shading=hill&n=z

		static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}?g={2}&mkt={3}&lbl=l1&stl=h&shading=hill&n=z{4}";

		private string serverLetters = "01234567";

		char GetServerNum(int x, int y)
		{
			return serverLetters[ (x+y) % serverLetters.Length ];
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			var serverN =  GetServerNum(x, y);


			return "";
		}

	}
}
