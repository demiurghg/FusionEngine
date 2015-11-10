using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.GoogleMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.MapBox;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;

namespace Fusion.Engine.Graphics.GIS
{
    partial class TilesBatch
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
			MapSources.Add(new OpenStreetMap(GameEngine));
			MapSources.Add(new GoogleMap(GameEngine));
			MapSources.Add(new GoogleSatelliteMap(GameEngine));
			MapSources.Add(new YandexMap(GameEngine));
			MapSources.Add(new YandexSatelliteMap(GameEngine));
			MapSources.Add(new PencilMap(GameEngine));
			MapSources.Add(new SpaceStationMap(GameEngine));
			MapSources.Add(new PirateMap(GameEngine));
		}

	}
}
