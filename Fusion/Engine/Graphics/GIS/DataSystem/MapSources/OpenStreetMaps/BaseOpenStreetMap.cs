﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;


namespace Fusion.Engine.Graphics.GIS.DataSystem.MapSources.OpenStreetMaps
{
	public abstract class BaseOpenStreetMap : BaseMapSource
	{
		protected BaseOpenStreetMap(Game game) : base(game)
		{
		}

		//protected List<> 
		public readonly string ServerLetters = "abc";

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }
	}


	public class OpenStreetMap : BaseOpenStreetMap
	{
		static readonly string UrlFormat = "http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png";


		public override string Name
		{
			get { return "OpenStreetMap"; }
		}

		public override string ShortName
		{
			get { return "OSM"; }
		}

		protected override string RefererUrl
		{
			get { return "http://www.openstreetmap.org/"; }
		}

		public OpenStreetMap(Game game) : base(game)
		{
			MaxZoom = 19;
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, ServerLetters[(x + y) % ServerLetters.Length], zoom, x, y);
		}

	}
}
