using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.BingMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.GoogleMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesGisLayer
	{
		public static List<BaseMapSource> MapSources = new List<BaseMapSource>();

		public enum MapSource : int
		{
			OpenStreetMap		= 0,
			GoogleMap			= 1,
			GoogleSatteliteMap	= 2,
			Yandex				= 3,
			YandexSatellite		= 4,
			PencilMap			= 5,
			SpaceStationMap		= 6,
			PirateMap			= 7,
		}

		public BaseMapSource CurrentMapSource { get; internal set; }


		protected void RegisterMapSources()
		{
			MapSources.Add(new OpenStreetMap(Game)		);
			MapSources.Add(new GoogleMap(Game)			);
			MapSources.Add(new GoogleSatelliteMap(Game)	);
			MapSources.Add(new YandexMap(Game)			);
			MapSources.Add(new YandexSatelliteMap(Game)	);
			MapSources.Add(new PencilMap(Game)			);
			MapSources.Add(new SpaceStationMap(Game)	);
			MapSources.Add(new PirateMap(Game)			);
			MapSources.Add(new BaseBingMapsSource(Game)	);
			MapSources.Add(new BingMapSatellite(Game)	);
		}


	    public void SetMapSource(MapSource map)
	    {
			var oldProj = CurrentMapSource.Projection;

			CurrentMapSource = MapSources[(int)map];

			if (!oldProj.Equals(CurrentMapSource.Projection)) {
				updateTiles = true;
			}
	    }

	}
}
