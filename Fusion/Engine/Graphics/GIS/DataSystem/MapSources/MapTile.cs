using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources
{
	internal class MapTile
	{
		public Texture2D	Tile;

		public int			X;
		public int			Y;
		public int			Zoom;
		
		public string		Url;
		public string		Path;
		
		public float		Time;
		public int			LruIndex;

		public bool			IsLoaded = false;
	}
}
